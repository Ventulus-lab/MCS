using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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
            Logger.LogInfo("加载成功！");

            var harmony = new Harmony("Ventulus.MCS.PopulationDynamicsAdjustment");
            harmony.PatchAll();

            MessageMag.Instance.Register(MessageName.MSG_Npc_JieSuan_COMPLETE, new Action<MessageData>(this.AfterJieSuanStatistics));
        }

        public static PopulationDynamicsAdjustment Instance;
        private DateTime LastJieSuanTime;
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

        private static int TotalPopulation;
        private WeightDictionary NPCBigLevelStatistics;
        private WeightDictionary NPCTypeStatistics;

        private WeightDictionary NPCBigLevelTarget;
        private WeightDictionary NPCTypeTarget;
        private WeightDictionary NPCBigLevelAdjustment;

        [HarmonyPatch(typeof(NpcJieSuanManager))]
        class NpcJieSuanManager_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(NpcJieSuanManager.NpcJieSuan))]
            public static bool NpcJieSuan_Prefix()
            {
                Instance.LastJieSuanTime = DateTime.Parse(NpcJieSuanManager.inst.JieSuanTime);
                Instance.Logger.LogInfo("开始结算了！");
                Instance.Logger.LogInfo(NpcJieSuanManager.inst.JieSuanTime);
                return true;
            }
        }

        public void AfterJieSuanStatistics(MessageData data = null)
        {

            Logger.LogInfo("结算完成！");
            Logger.LogInfo(NpcJieSuanManager.inst.JieSuanTime);
            //每次结算统计
            StatisticsPopulation();

            //每年六月
            //return;
            StartCoroutine(AdjustPopulation());

        }

        public static DateTime RecentJune(DateTime lasttime, int month = 6)
        {
            var temptime = new DateTime(lasttime.Year, month, lasttime.Day);
            if (temptime < lasttime)
                return temptime.AddYears(1);
            else
                return temptime;
        }

        IEnumerator AdjustPopulation()
        {
            DateTime tempdate = Instance.LastJieSuanTime;
            DateTime NowJieSuanTime = DateTime.Parse(NpcJieSuanManager.inst.JieSuanTime);
            while (NowJieSuanTime > RecentJune(tempdate, 6))
            {

                Logger.LogMessage("经过六月份！");
                Logger.LogMessage(RecentJune(tempdate).ToString());
                ///
                yield return null;
                StatisticsPopulation();
                ///
                yield return null;
                NPCBigLevelTarget = new WeightDictionary(TargetBigLevelWeight, NPCBigLevel);
                NPCTypeTarget = new WeightDictionary(TargetTypeWeight, NPCType);
                Dictionary<int, float> subdict = NPCBigLevelTarget.PositiveSubtraction(NPCBigLevelStatistics.WeightDict);
                NPCBigLevelAdjustment = new WeightDictionary(subdict, NPCBigLevel);
                ///
                yield return null;
                tempdate = tempdate.AddYears(1);
            }

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
                int biglevel = (avatar["Level"].I - 1) / 3 + 1;
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
        private static Dictionary<int, float> TargetTypeWeight = new Dictionary<int, float>()
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
        private static Dictionary<int, float> TargetBigLevelWeight = new Dictionary<int, float>()
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
        public Dictionary<int, float> WeightDict
        {
            get;
            set;
        }
        public Dictionary<int, string> NameDict
        {
            get;
            set;
        }
        private SortedDictionary<int, float> _sortDict;
        private Dictionary<int, float> _percentDict;



        public WeightDictionary()
        {
            WeightDict = new Dictionary<int, float>();
            NameDict = new Dictionary<int, string>();
        }
        public WeightDictionary(Dictionary<int, float> weightdict)
        {
            WeightDict = new Dictionary<int, float>(weightdict);
            NameDict = new Dictionary<int, string>();
        }
        public WeightDictionary(Dictionary<int, string> namedict)
        {
            WeightDict = new Dictionary<int, float>();
            NameDict = new Dictionary<int, string>(namedict);
        }

        public WeightDictionary(Dictionary<int, float> weightdict, Dictionary<int, string> namedict)
        {
            WeightDict = new Dictionary<int, float>(weightdict);
            NameDict = new Dictionary<int, string>(namedict);
        }
        public WeightDictionary(WeightDictionary weightdictionary)
        {
            WeightDict = new Dictionary<int, float>(weightdictionary.WeightDict);
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
            _sortDict = new SortedDictionary<int, float>(WeightDict);
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
        public Dictionary<int, float> Normalization(Dictionary<int, float> weightdict)
        {
            _sortDict = new SortedDictionary<int, float>();
            float _sum = weightdict.Values.Where(x => x > 0).Sum();
            foreach (var item in weightdict)
            {
                _sortDict.Add(item.Key, item.Value > 0 ? item.Value / _sum : 0);
            }
            return new Dictionary<int, float>(_sortDict);
        }
        public Dictionary<int, float> PositiveSubtraction(Dictionary<int, float> percentdict2)
        {
            this.Normalization();
            _percentDict = Normalization(percentdict2);
            _sortDict = new SortedDictionary<int, float>();
            foreach (var item in WeightDict)
            {
                if (_percentDict.ContainsKey(item.Key))
                {
                    float m = item.Value - _percentDict[item.Key];
                    _sortDict.Add(item.Key, m > 0 ? m : 0);
                }
                else
                {
                    _sortDict.Add(item.Key, item.Value);
                }
            }
            return new Dictionary<int, float>(_sortDict);
        }
    }
}
