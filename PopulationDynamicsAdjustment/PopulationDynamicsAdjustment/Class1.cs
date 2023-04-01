using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


namespace Ventulus
{
    [BepInDependency("Ventulus.MCS.VTools", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Ventulus.MCS.PopulationDynamicsAdjustment", "修仙人口动态调整", "1.1.3")]
    public class PopulationDynamicsAdjustment : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
            TargetPopulation = Config.Bind<PopulationEnum>("Ventulus", "调控目标人数", PopulationEnum.Less, new ConfigDescription("分为五档"));
            AllowSpecialLiuPai = Config.Bind<bool>("Ventulus", "允许随机NPC使用特定流派", true, new ConfigDescription("若为false，则随机生成NPC会避开“倪旭欣”等专用流派"));
            Statisticsbroadcast = Config.Bind<bool>("Ventulus", "播报每年人口统计结果", true, new ConfigDescription("通过传音符特定人物播报"));
        }
        void Start()
        {
            new Harmony("Ventulus.MCS.PopulationDynamicsAdjustment").PatchAll();
            MessageMag.Instance.Register(MessageName.MSG_Npc_JieSuan_COMPLETE, new Action<MessageData>(AfterJieSuanStatistics));

            Logger.LogInfo("加载成功");
        }

        public static PopulationDynamicsAdjustment Instance;
        private static DateTime LastJieSuanTime;
        public static ConfigEntry<PopulationEnum> TargetPopulation;
        public static ConfigEntry<bool> AllowSpecialLiuPai;
        public static ConfigEntry<bool> Statisticsbroadcast;
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
        public void AddCyNPC(DateTime sendTime)
        {
            KBEngine.Avatar player = PlayerEx.Player;
            if (Statisticsbroadcast.Value && !player.emailDateMag.cyNpcList.Contains(CyNPCId))
            {

                Logger.LogInfo("传音主持人");
                player.AddFriend(CyNPCId);
                string message = "咳咳…信号有点不好。我在这把剑里，牵引灵机，能些许感受到此方天地中修士的数量。这些信息或许会对你修行有所帮助。";
                //加入新传音符
                VTools.SendOldEmail(CyNPCId, CyNPCId, message, sendTime.ToString());

            }
            else if (!Statisticsbroadcast.Value && player.emailDateMag.cyNpcList.Contains(CyNPCId))
            {
                Logger.LogInfo("移除传音");
                VTools.RemoveFriend(CyNPCId);
            }
        }


        IEnumerator AdjustPopulation()
        {
            //UIPopTip.Inst.Pop("开始人口普查", PopTipIconType.任务进度);
            //等待一秒
            yield return new WaitForSeconds(1f);

            AddCyNPC(LastJieSuanTime);
            DateTime nowJieSuanTime = DateTime.Parse(NpcJieSuanManager.inst.JieSuanTime);
            DateTime cycleJuneDate = VTools.RecentMonth(LastJieSuanTime, 6);

            while (nowJieSuanTime > cycleJuneDate)
            {
                Logger.LogMessage("经过六月份");
                Logger.LogMessage(cycleJuneDate.ToString());
                yield return null;
                //调查人口
                StatisticsPopulation();
                string broadcast = $"此方天地共有修士{TotalPopulation}人。{Environment.NewLine}按修为境界分：{NPCBigLevelStatistics}{Environment.NewLine}按类型分：{NPCTypeStatistics}";
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
                    if (cycleJuneDate >= new DateTime(300, 1, 1))
                    {
                        NPCBigLevelTarget = new WeightDictionary(TargetBigLevelWeight300, NPCBigLevel);
                    }
                    else if (cycleJuneDate >= new DateTime(120, 1, 1))
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
                    int createNum = 0;
                    for (int num = 1; num <= PopulationAdjustment; num++)
                    {
                        int chooseBigLevel = NPCBigLevelAdjustment.RollByWeight(out _);
                        int chooseType = NPCTypeAdjustment.RollByWeight(out _);
                        if (chooseBigLevel <= 0 || chooseType <= 0) continue;
                        int banLiuPai = !AllowSpecialLiuPai.Value && TypeBanLiuPai.ContainsKey(chooseType) ? TypeBanLiuPai[chooseType] : 0;
                        //正式调用造人
                        int id = VTools.CreateNpcByTypeAndLevel(chooseType, BigLevelToLevel(chooseBigLevel), banLiuPai);

                        if (id >= 0)
                        {
                            createNum++;
                            Logger.LogInfo($"{chooseBigLevel}{NPCBigLevel[chooseBigLevel]}+{chooseType}{NPCType[chooseType]} =ID:{id}");
                        }
                        else
                        {
                            Logger.LogInfo($"{chooseBigLevel}{NPCBigLevel[chooseBigLevel]}+{chooseType}{NPCType[chooseType]} =Fail");
                        }
                        ///
                        //yield return null;
                    }
                    Logger.LogMessage("实际造人" + createNum);
                    if (createNum > 0)
                    {
                        broadcast += $"另外，还有刚开始修炼及入世修行的修士{createNum}人。";
                    }
                }
                ///
                yield return null;
                //播报人口统计
                if (Statisticsbroadcast.Value)
                {
                    Logger.LogInfo("传音符播报");

                    //加入新传音符
                    VTools.SendOldEmail(CyNPCId, CyNPCId, broadcast, cycleJuneDate.ToString());
                }
                cycleJuneDate = cycleJuneDate.AddYears(1);
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
                int npcId = avatar.GetInt("id");
                if (npcId < 20000) continue;
                if (NpcJieSuanManager.inst.IsDeath(npcId) || NpcJieSuanManager.inst.IsFly(npcId)) continue;

                TotalPopulation++;
                if (avatar.HasField("Level"))
                {
                    int biglevel = LevelToBigLevel(avatar["Level"].I);
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

        //大小境界转换
        public static int LevelToBigLevel(int level)
        {
            return (level - 1) / 3 + 1;
        }
        public static int BigLevelToLevel(int biglevel)
        {
            return (biglevel - 1) * 3 + 1;
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
