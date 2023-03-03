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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UIPopupList;


namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.PopulationDynamicsAdjustment", "修仙人口动态调整", "1.0.0")]
    public class PopulationDynamicsAdjustment : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
            TargetPopulation = Config.Bind<PopulationEnum>("Ventulus", "调控目标人数", PopulationEnum.Less, new ConfigDescription("分为五档"));
        }
        void Start()
        {
            Logger.LogInfo("加载成功");

            var harmony = new Harmony("Ventulus.MCS.PopulationDynamicsAdjustment");
            harmony.PatchAll();

            MessageMag.Instance.Register(MessageName.MSG_Npc_JieSuan_COMPLETE, new Action<MessageData>(this.AfterJieSuanStatistics));
        }

        public static PopulationDynamicsAdjustment Instance;
        private static DateTime LastJieSuanTime;
        public static ConfigEntry<PopulationEnum> TargetPopulation;
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
        private const int N = 4;

        private static int TotalPopulation;
        private static WeightDictionary NPCBigLevelStatistics;
        private static WeightDictionary NPCTypeStatistics;

        private static WeightDictionary NPCBigLevelTarget;
        private static WeightDictionary NPCTypeTarget;
        private static WeightDictionary NPCBigLevelAdjustment;
        private static WeightDictionary NPCTypeAdjustment;
        private static int PopulationAdjustment;

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
            //每次结算统计
            StatisticsPopulation();

            //每年六月
            //return;
            StartCoroutine(AdjustPopulation());

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
            DateTime tempdate = LastJieSuanTime;
            DateTime NowJieSuanTime = DateTime.Parse(NpcJieSuanManager.inst.JieSuanTime);
            while (NowJieSuanTime > RecentJune(tempdate, 6))
            {

                Logger.LogMessage("经过六月份");
                Logger.LogMessage(RecentJune(tempdate).ToString());
                ///
                yield return null;
                //调查人口
                StatisticsPopulation();
                PopulationAdjustment = ((int)TargetPopulation.Value - TotalPopulation) / N + 1;
                if (PopulationAdjustment <= 0)
                {
                    PopulationAdjustment = 0;
                    Logger.LogMessage("无需调整");
                    continue;
                }

                ///
                yield return null;
                //计算比例
                NPCBigLevelTarget = new WeightDictionary(TargetBigLevelWeight, NPCBigLevel);
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
                for (int num = 1; num <= PopulationAdjustment; num++)
                {
                    int ChooseBigLevel = NPCBigLevelAdjustment.RollByWeight(out double roll);
                    int ChooseType = NPCTypeAdjustment.RollByWeight(out double roll2);
                    if (ChooseBigLevel <= 0 || ChooseType <= 0) continue;
                    Logger.LogInfo($"{ChooseBigLevel}{NPCBigLevel[ChooseBigLevel]}+{ChooseType}{NPCType[ChooseType]}");


                    ///
                    //yield return null;
                }
                tempdate = tempdate.AddYears(1);
            }
            Logger.LogMessage("人口调整完成");
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

                int biglevel = LevelToBigLevel(avatar["Level"].I);
                NPCBigLevelStatistics.AddWeight(biglevel);

                int npctype = avatar["Type"].I;
                NPCTypeStatistics.AddWeight(npctype);
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
            {21,10},
            {22,10},
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
            {1,120},
            {2,60},
            {3,20},
            {4,5},
            {5,1},
        };
    }

    public class WeightDictionary
    {
        public Dictionary<int, double> WeightDict
        {
            get;
            set;
        }
        public Dictionary<int, string> NameDict
        {
            get;
            set;
        }
        private SortedDictionary<int, double> _sortDict;
        private Dictionary<int, double> _percentDict;

        public WeightDictionary()
        {
            WeightDict = new Dictionary<int, double>();
            NameDict = new Dictionary<int, string>();
        }
        public WeightDictionary(Dictionary<int, double> weightdict)
        {
            WeightDict = new Dictionary<int, double>(weightdict);
            NameDict = new Dictionary<int, string>();
        }
        public WeightDictionary(Dictionary<int, string> namedict)
        {
            WeightDict = new Dictionary<int, double>();
            NameDict = new Dictionary<int, string>(namedict);
        }

        public WeightDictionary(Dictionary<int, double> weightdict, Dictionary<int, string> namedict)
        {
            WeightDict = new Dictionary<int, double>(weightdict);
            NameDict = new Dictionary<int, string>(namedict);
        }
        public WeightDictionary(WeightDictionary weightdictionary)
        {
            WeightDict = new Dictionary<int, double>(weightdictionary.WeightDict);
            NameDict = new Dictionary<int, string>(weightdictionary.NameDict);
        }
        public void AddWeight(int key)
        {
            AddWeight(key, 1);
        }
        public void AddWeight(int key, float num)
        {
            if (WeightDict.ContainsKey(key))
                WeightDict[key] += num;
            else
                WeightDict.Add(key, num);
        }
        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();
            SB.Append(Environment.NewLine);
            _sortDict = new SortedDictionary<int, double>(WeightDict);
            foreach (var item in _sortDict)
            {
                SB.Append("[");
                SB.Append(item.Key);
                if (NameDict.ContainsKey(item.Key))
                    SB.Append(NameDict[item.Key]);
                SB.Append(",");
                SB.Append(item.Value);
                SB.Append("]");
                SB.Append(Environment.NewLine);
            }
            return SB.ToString();
        }
        public void Normalization()
        {
            WeightDict = Normalization(WeightDict);
        }
        public Dictionary<int, double> Normalization(Dictionary<int, double> weightdict)
        {
            _sortDict = new SortedDictionary<int, double>();
            double sum = weightdict.Values.Where(x => x >= 0).Sum();
            foreach (var item in weightdict)
            {
                _sortDict.Add(item.Key, item.Value > 0 ? item.Value / sum : 0);
            }
            return new Dictionary<int, double>(_sortDict);
        }
        public Dictionary<int, double> PositiveSubtraction(Dictionary<int, double> percentdict2)
        {
            this.Normalization();
            _percentDict = Normalization(percentdict2);
            _sortDict = new SortedDictionary<int, double>();
            foreach (var item in WeightDict)
            {
                if (_percentDict.ContainsKey(item.Key))
                {
                    double m = item.Value - _percentDict[item.Key];
                    _sortDict.Add(item.Key, m > 0 ? m : 0);
                }
                else
                {
                    _sortDict.Add(item.Key, item.Value);
                }
            }
            if (_sortDict.Values.Where(x => x >= 0).Sum() <= 0)
            {
                return new Dictionary<int, double>(WeightDict);
            }
            return new Dictionary<int, double>(_sortDict);
        }
        private System.Random rnd = new System.Random();
        public static long GetRandomLong()
        {
            byte[] array = new byte[8];
            new RNGCryptoServiceProvider().GetBytes(array);
            return BitConverter.ToInt64(array, 0);
        }
        public static double GetRandomDoubleRoll(double max)
        {
            if (max <= 0) return 0;
            double result;
            do
            {
                result = Math.Abs((double)GetRandomLong() / long.MaxValue);
            } while (result >= max);
            return result;
        }
        public double GetRandomDoubleRoll2(double max)
        {
            if (max <= 0) return 0;
            double result;
            do
            {
                result = rnd.NextDouble();
            } while (result >= max);
            return result;
        }
        public int RollByWeight(out double roll)
        {
            double sum = WeightDict.Values.Where(x => x >= 0).Sum();
            roll = GetRandomDoubleRoll2(sum);
            if (sum <= 0) return 0;

            double countsum = 0;
            foreach (var item in WeightDict)
            {
                if (item.Value <= 0) continue;
                countsum += item.Value;
                if (countsum > roll)
                {
                    return item.Key;
                }
            }
            return 0;
        }
    }
}
