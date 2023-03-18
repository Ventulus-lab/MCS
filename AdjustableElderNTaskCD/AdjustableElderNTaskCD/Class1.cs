using BepInEx;
using BepInEx.Configuration;
using GUIPackage;
using HarmonyLib;
using JSONClass;
//using KBEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.AdjustableElderNTaskCD", "可调门派长老任务间隔", "1.0.0")]
    public class AdjustableElderNTaskCD : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;

            Elder6NTaskCD = Config.Bind("Ventulus", "炼丹长老任务间隔", 600, new ConfigDescription("当次炼丹长老任务到下一次任务的间隔月数，已有任务时不会重复发放", new AcceptableValueRange<int>(1, 1200)));
            Elder7NTaskCD = Config.Bind("Ventulus", "炼器长老任务间隔", 600, new ConfigDescription("当次炼器长老任务到下一次任务的间隔月数，已有任务时不会重复发放", new AcceptableValueRange<int>(1, 1200)));
            Elder8NTaskCD = Config.Bind("Ventulus", "外务长老任务间隔", 300, new ConfigDescription("当次外务长老任务到下一次任务的间隔月数，已有任务时不会重复发放", new AcceptableValueRange<int>(1, 1200)));
            Elder9NTaskCD = Config.Bind("Ventulus", "授业长老任务间隔", 600, new ConfigDescription("当次授业长老任务到下一次任务的间隔月数，已有任务时不会重复发放", new AcceptableValueRange<int>(1, 1200)));

        }
        void Start()
        {
            new Harmony("Ventulus.MCS.AdjustableElderNTaskCD").PatchAll();
            Logger.LogInfo("加载成功！");
        }

        public static AdjustableElderNTaskCD Instance;
        public static ConfigEntry<int> Elder6NTaskCD;
        public static ConfigEntry<int> Elder7NTaskCD;
        public static ConfigEntry<int> Elder8NTaskCD;
        public static ConfigEntry<int> Elder9NTaskCD;

        [HarmonyPatch(typeof(script.MenPaiTask.MenPaiTaskMag))]
        class script_MenPaiTask_MenPaiTaskMag_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(script.MenPaiTask.MenPaiTaskMag.SendTask))]
            public static bool SendTask_Prefix()
            {
                Instance.Logger.LogInfo("修改CD");
                MenPaiFengLuBiao.DataDict[6].CD = Elder6NTaskCD.Value;
                MenPaiFengLuBiao.DataDict[7].CD = Elder7NTaskCD.Value;
                MenPaiFengLuBiao.DataDict[8].CD = Elder8NTaskCD.Value;
                MenPaiFengLuBiao.DataDict[9].CD = Elder9NTaskCD.Value;

                return true;
            }
        }
    }
}
