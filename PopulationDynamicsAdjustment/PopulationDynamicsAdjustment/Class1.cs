using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fungus;
using GUIPackage;
using HarmonyLib;
using JSONClass;
using KBEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaiMai;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Ventulus
{
    [BepInDependency("Ventulus.MCS.VTools", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Ventulus.MCS.PopulationDynamicsAdjustment", "修仙人口动态调整", "1.1.0")]
    public class PopulationDynamicsAdjustment : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
            TargetPopulation = Config.Bind<PopulationEnum>("Ventulus", "调控目标人数", PopulationEnum.Less, new ConfigDescription("分为五档"));
            AllowSpecialLiuPai = Config.Bind<bool>("Ventulus", "允许随机NPC使用特定流派", true, new ConfigDescription("若为false，则随机生成NPC会避开“倪旭欣”等专用流派"));
            StatisticsBroadcast = Config.Bind<bool>("Ventulus", "播报每年人口统计结果", true, new ConfigDescription("通过传音符特定人物播报"));
        }
        void Start()
        {
            new Harmony("Ventulus.MCS.PopulationDynamicsAdjustment").PatchAll();
            MessageMag.Instance.Register(MessageName.MSG_Npc_JieSuan_COMPLETE, new Action<MessageData>(this.AfterJieSuanStatistics));

            Logger.LogInfo("加载成功");
        }

        public static PopulationDynamicsAdjustment Instance;
        private static DateTime LastJieSuanTime;
        public static ConfigEntry<PopulationEnum> TargetPopulation;
        public static ConfigEntry<bool> AllowSpecialLiuPai;
        public static ConfigEntry<bool> StatisticsBroadcast;
        public enum PopulationEnum
        {
            [Description("很少(600)")]
            Little = 600,
            [Description("较少(900)")]
            Less = 900,
            [Description("中等(1200)")]
            Moderate = 1200,
            [Description("较多(1500)")]
            More = 1500,
            [Description("很多(1800)")]
            Much = 1800,

        }
        //每年补充人数比例，数字越高补充越慢
        private const int N = 5;

        private static int TotalPopulation;
        private static WeightDictionary NPCBigLevelStatistics;
        private static WeightDictionary NPCTypeStatistics;

        private static WeightDictionary NPCBigLevelTarget;
        private static WeightDictionary NPCTypeTarget;
        private static WeightDictionary NPCBigLevelAdjustment;
        private static WeightDictionary NPCTypeAdjustment;
        private static int PopulationAdjustment;

        //魏老播报
        private static int CyNPCId = 2;

        [HarmonyPatch(typeof(NpcJieSuanManager))]
        class NpcJieSuanManager_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(NpcJieSuanManager.NpcJieSuan))]
            public static bool NpcJieSuan_Prefix()
            {
                LastJieSuanTime = DateTime.Parse(NpcJieSuanManager.inst.JieSuanTime);
                Instance.Logger.LogInfo("开始结算了");
                Instance.Logger.LogInfo(NpcJieSuanManager.inst.JieSuanTime);

                return true;
            }
        }

        public void AfterJieSuanStatistics(MessageData data = null)
        {

            Logger.LogInfo("结算完成");
            Logger.LogInfo(NpcJieSuanManager.inst.JieSuanTime);

            //每次结算
            StartCoroutine(AdjustPopulation());
        }
        public void AddCyNPC(DateTime sendtime)
        {
            KBEngine.Avatar Player = Tools.instance.getPlayer();

            if (StatisticsBroadcast.Value && !Player.emailDateMag.cyNpcList.Contains(CyNPCId))
            {

                Logger.LogInfo("传音主持人");
                Player.emailDateMag.cyNpcList.Add(CyNPCId);
                string Message = "咳咳…信号有点不好。我在这把剑里，牵引灵机，能些许感受到此方天地中修士的数量。这些信息或许会对你修行有所帮助。";
                //加入新传音符
                VTools.SendOldEmail(CyNPCId, CyNPCId, Message, sendtime.ToString());

            }
            else if (!StatisticsBroadcast.Value && Player.emailDateMag.cyNpcList.Contains(CyNPCId))
            {
                Logger.LogInfo("移除传音");
                Player.emailDateMag.cyNpcList.Remove(CyNPCId);
            }
        }
        public static DateTime RecentJune(DateTime lasttime, int month = 6)
        {
            DateTime temptime = new DateTime(lasttime.Year, month, lasttime.Day);
            if (temptime < lasttime)
                return temptime.AddYears(1);
            else
                return temptime;
        }

        IEnumerator AdjustPopulation()
        {
            //补充进入结算状态，防止快速存档影响
            //UIPopTip.Inst.Pop("开始人口普查", PopTipIconType.任务进度);
            //NpcJieSuanManager.inst.isCanJieSuan = false;
            //等待一秒
            yield return new WaitForSeconds(1f);
            
            AddCyNPC(LastJieSuanTime);
            DateTime tempdate = LastJieSuanTime;
            DateTime NowJieSuanTime = DateTime.Parse(NpcJieSuanManager.inst.JieSuanTime);
            while (NowJieSuanTime > RecentJune(tempdate, 6))
            {
                DateTime cycledateTime = RecentJune(tempdate);
                Logger.LogMessage("经过六月份");
                Logger.LogMessage(cycledateTime.ToString());

                //调查人口
                StatisticsPopulation();
                string Broadcast = $"此方天地共有修士{TotalPopulation}人。{Environment.NewLine}按修为境界分：{NPCBigLevelStatistics}{Environment.NewLine}按类型分：{NPCTypeStatistics}";
                PopulationAdjustment = Math.Min(((int)TargetPopulation.Value - TotalPopulation + N - 1) / N, (int)TargetPopulation.Value / 10);
                if (PopulationAdjustment <= 0)
                {
                    PopulationAdjustment = 0;
                    Logger.LogMessage("无需调整");
                }
                else
                {

                    ///
                    yield return null;
                    //计算比例
                    if (cycledateTime >= new DateTime(300, 1, 1))
                    {
                        NPCBigLevelTarget = new WeightDictionary(TargetBigLevelWeight300, NPCBigLevel);
                    }
                    else if (cycledateTime >= new DateTime(120, 1, 1))
                    {
                        NPCBigLevelTarget = new WeightDictionary(TargetBigLevelWeight120, NPCBigLevel);
                    }
                    else
                    {
                        NPCBigLevelTarget = new WeightDictionary(TargetBigLevelWeight, NPCBigLevel);
                    }

                    NPCTypeTarget = new WeightDictionary(TargetTypeWeight, NPCType);

                    Dictionary<int, double> subdict = NPCBigLevelTarget.PositiveSubtraction(NPCBigLevelStatistics.WeightDict);
                    NPCBigLevelAdjustment = new WeightDictionary(subdict, NPCBigLevel);

                    subdict = NPCTypeTarget.PositiveSubtraction(NPCTypeStatistics.WeightDict);
                    NPCTypeAdjustment = new WeightDictionary(subdict, NPCType);


                    Logger.LogInfo("调整境界比例");
                    Logger.LogInfo(NPCBigLevelAdjustment.ToString());

                    Logger.LogInfo("调整类型比例");
                    Logger.LogInfo(NPCTypeAdjustment.ToString());

                    Logger.LogInfo("本年调整人口");
                    Logger.LogInfo(PopulationAdjustment);
                    ///
                    yield return null;
                    //开始循环造人
                    int createnum = 0;
                    for (int num = 1; num <= PopulationAdjustment; num++)
                    {
                        int ChooseBigLevel = NPCBigLevelAdjustment.RollByWeight(out _);
                        int ChooseType = NPCTypeAdjustment.RollByWeight(out _);
                        //Logger.LogInfo($"{ChooseBigLevel}{NPCBigLevel[ChooseBigLevel]}+{ChooseType}{NPCType[ChooseType]}");
                        if (ChooseBigLevel <= 0 || ChooseType <= 0) continue;
                        int banliupai = !AllowSpecialLiuPai.Value && TypeBanLiuPai.ContainsKey(ChooseType) ? TypeBanLiuPai[ChooseType] : 0;
                        int id = VTools.CreateNpcByTypeAndLevel(ChooseType, VTools.BigLevelToLevel(ChooseBigLevel), banliupai);

                        if (id >= 0)
                        {
                            createnum++;
                            Logger.LogInfo($"{ChooseBigLevel}{NPCBigLevel[ChooseBigLevel]}+{ChooseType}{NPCType[ChooseType]} =ID:{id}");
                        }
                        else
                        {
                            Logger.LogInfo($"{ChooseBigLevel}{NPCBigLevel[ChooseBigLevel]}+{ChooseType}{NPCType[ChooseType]} =Fail");
                        }
                        ///
                        yield return null;
                    }
                    Logger.LogMessage("实际造人" + createnum);
                    if (createnum > 0)
                    {
                        Broadcast += $"另外，还有刚开始修炼及入世修行的修士{createnum}人。";
                    }
                }
                ///
                yield return null;
                //播报人口统计
                if (StatisticsBroadcast.Value)
                {
                    Logger.LogInfo("传音符播报");

                    //加入新传音符
                    VTools.SendOldEmail(CyNPCId, CyNPCId, Broadcast, cycledateTime.ToString());
                }
                tempdate = tempdate.AddYears(1);
            }

            //退出结算状态
            //NpcJieSuanManager.inst.isCanJieSuan = true;
            //UIPopTip.Inst.Pop("完成人口普查", PopTipIconType.任务完成);

        }
        public void StatisticsPopulation()
        {

            Logger.LogInfo("开始统计");
            TotalPopulation = 0;
            NPCBigLevelStatistics = new WeightDictionary(NPCBigLevel);
            NPCTypeStatistics = new WeightDictionary(NPCType);

            foreach (JSONObject avatar in jsonData.instance.AvatarJsonData.list)
            {
                int id = avatar.GetInt("id");
                if (id < 20000) continue;

                TotalPopulation++;
                if (avatar.HasField("Level"))
                {
                    int biglevel = VTools.LevelToBigLevel(avatar["Level"].I);
                    NPCBigLevelStatistics.AddWeight(biglevel);
                }

                if (avatar.HasField("Type"))
                {
                    int npctype = avatar["Type"].I;
                    NPCTypeStatistics.AddWeight(npctype);
                }

            }

            Logger.LogInfo("统计总人口");
            Logger.LogInfo(TotalPopulation);

            Logger.LogInfo("按境界");
            Logger.LogInfo(NPCBigLevelStatistics.ToString());

            Logger.LogInfo("按类型");
            Logger.LogInfo(NPCTypeStatistics.ToString());

            Logger.LogInfo("目标人数");
            Logger.LogInfo((int)TargetPopulation.Value);

        }
        //官方bug，暂时帮忙修复下
        [HarmonyPatch(typeof(KillSystem.Killer.Killer_Factory))]
        class KillSystem_Killer_Killer_Factory_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetRandomDaiHao")]
            public static bool GetRandomDaiHao_Prefix(ref string __result)
            {
                int randomInt = Tools.instance.GetRandomInt(0, KillerDaiHaoData.DataDict.Count - 1);
                //DataList的序号是0-24，DataDict的键是1-25
                Instance.Logger.LogInfo("修复官方杀手bug");
                __result = KillerDaiHaoData.DataList[randomInt].Name;
                return false;
            }
        }

        

        private static Dictionary<int, string> NPCType = new Dictionary<int, string>()
        {
            {1,"竹山"},
            {2,"金虹"},
            {3,"星河"},
            {4,"离火"},
            {5,"化尘"},
            {6,"倪家"},
            {7,"林家"},
            {8,"百里"},
            {9,"公孙"},
            {10,"散修"},
            {11,"白帝楼"},
            {12,"天机阁"},
            {13,"沂山派"},
            {14,"禾山道"},
            {15,"蓬莎岛"},
            {16,"碎星岛"},
            {17,"千流岛"},
            {18,"古神教"},
            {19,"天魔道"},
            {20,"血剑宫"},
            {21,"风雨楼"},
            {22,"杀手"},
            {23,"星宫"},
            {24,"废弃"},
            {25,"万魂殿"},
        };
        private static Dictionary<int, double> TargetTypeWeight = new Dictionary<int, double>()
        {
            {1,80},
            {2,80},
            {3,80},
            {4,80},
            {5,80},
            {6,10},
            {7,10},
            {8,10},
            {9,10},
            {10,160},
            {11,20},
            {12,20},
            {13,20},
            {14,20},
            {15,120},
            {16,120},
            {17,20},
            {18,10},
            {19,10},
            {20,10},
            {21,5},
            {22,5},
            {23,20},
            {24,0},
            {25,10},
        };
        private static Dictionary<int, string> NPCBigLevel = new Dictionary<int, string>()
        {
            {1,"练气"},
            {2,"筑基"},
            {3,"金丹"},
            {4,"元婴"},
            {5,"化神"},
        };
        private static Dictionary<int, double> TargetBigLevelWeight = new Dictionary<int, double>()
        {
            {1,240},
            {2,90},
            {3,24},
            {4,5},
            {5,1},
        };
        private static Dictionary<int, double> TargetBigLevelWeight120 = new Dictionary<int, double>()
        {
            {1,180},
            {2,75},
            {3,22},
            {4,5},
            {5,1},
        };
        private static Dictionary<int, double> TargetBigLevelWeight300 = new Dictionary<int, double>()
        {
            {1,120},
            {2,60},
            {3,20},
            {4,5},
            {5,1},
        };
        private static Dictionary<int, int> TypeBanLiuPai = new Dictionary<int, int>()
        {
            //npc类型对应多种流派往往有一个重要NPC特有的流派
            {1,4},
            {2,14},
            {4,34},
            {6,62},
            {7,72},
            {8,82},
            {9,92},
            {16,164},
            {21,211},
            {23,232},
            {24,241},
        };
    }

}
