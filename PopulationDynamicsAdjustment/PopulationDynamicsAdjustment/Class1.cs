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
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.PopulationDynamicsAdjustment", "修仙人口动态调整", "1.0.0")]
    public class PopulationDynamicsAdjustment : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("修仙人口动态调整加载成功！");

            var harmony = new Harmony("Ventulus.MCS.PopulationDynamicsAdjustment");
            harmony.PatchAll();

            MessageMag.Instance.Register(MessageName.MSG_Npc_JieSuan_COMPLETE, new Action<MessageData>(this.AfterJieSuanStatistics));
        }

        public static PopulationDynamicsAdjustment Instance;
        private DateTime LastJieSuanTime;

        void Awake()
        {
            Instance = this;
        }

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
            Instance.Logger.LogInfo("结算完成！");
            Instance.Logger.LogInfo(NpcJieSuanManager.inst.JieSuanTime);
            //每次结算统计
            //StartCoroutine(StatisticsPopulation());

            //return;
            StartCoroutine(AdjustPopulation());

        }

        public static DateTime RecentJune(DateTime lasttime)
        {
            var temptime = new DateTime(lasttime.Year, 6, lasttime.Day);
            if (temptime < lasttime)
                return temptime.AddYears(1);
            else
                return temptime;
        }

        IEnumerator AdjustPopulation()
        {
            DateTime tempdate = Instance.LastJieSuanTime;
            DateTime NowJieSuanTime = DateTime.Parse(NpcJieSuanManager.inst.JieSuanTime);
            while (NowJieSuanTime > RecentJune(tempdate))
            {
                yield return null;
                Instance.Logger.LogMessage("经过六月份！");
                Instance.Logger.LogMessage(RecentJune(tempdate).ToString());
                ///
                //子协程后再进行父协程
                yield return StartCoroutine(StatisticsPopulation());
                ///
                tempdate = tempdate.AddYears(1);
            }

        }
        IEnumerator StatisticsPopulation()
        {
            yield return null;
            Instance.Logger.LogInfo("开始统计");
            TotalPopulation = 0;
            NPCBigLevelStatistics = new Dictionary<int, int>();
            NPCTypeStatistics = new Dictionary<int, int>();
            foreach (JSONObject avatar in jsonData.instance.AvatarJsonData.list)
            {
                int id = avatar.GetInt("id");
                if (id < 20000) continue;
                TotalPopulation++;
                int biglevel = (avatar["Level"].I - 1) / 3 + 1;
                AddDict(NPCBigLevelStatistics, biglevel);

                int npctype = avatar["Type"].I;
                AddDict(NPCTypeStatistics, npctype);
            }

            yield return null;
            Instance.Logger.LogInfo("统计总人口");
            Instance.Logger.LogInfo(TotalPopulation);

            Instance.Logger.LogInfo("按境界");
            foreach (var item in NPCBigLevelStatistics)
            {
                Instance.Logger.LogInfo(item.ToString());
            }

            Instance.Logger.LogInfo("按类型");
            foreach (var item in NPCTypeStatistics)
            {
                Instance.Logger.LogInfo(item.ToString());
            }
        }

        private static void AddDict(Dictionary<int, int> dict, int key)
        {
            AddDict(dict, key, 1);
        }
        private static void AddDict(Dictionary<int, int> dict, int key, int num)
        {
            if (dict.ContainsKey(key))
                dict[key] += num;
            else
                dict.Add(key, num);
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
        private static Dictionary<int, int> NPCBigLevelStatistics = new Dictionary<int, int>();
        private static Dictionary<int, int> NPCTypeStatistics = new Dictionary<int, int>();
        private static int TotalPopulation = 0;
    }

    class WeightDictionary
    {
        public Dictionary<int, int> WeightDict
        {
            get { return WeightDict; }
            set { WeightDict = new Dictionary<int, int>(value); }
        }
        public Dictionary<int, string> NameDict
        {
            get { return NameDict; }
            set { NameDict = new Dictionary<int, string>(value); }
        }
        private Dictionary<int, int> _tempDict;

        WeightDictionary()
            : this(new Dictionary<int, int>())
        { }
        WeightDictionary(Dictionary<int, int> weightdict)
            : this(weightdict, new Dictionary<int, string>())
        { }

        WeightDictionary(Dictionary<int, int> weightdict, Dictionary<int, string> namedict)
        {
            WeightDict = new Dictionary<int, int>(weightdict);
            NameDict = new Dictionary<int, string>(namedict);
        }
        WeightDictionary(WeightDictionary weightdictionary)
        {
            WeightDict = new Dictionary<int, int>(weightdictionary.WeightDict);
            NameDict = new Dictionary<int, string>(weightdictionary.NameDict);
        }
    }
}
