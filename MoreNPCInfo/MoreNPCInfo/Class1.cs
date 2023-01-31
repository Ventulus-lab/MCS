﻿using BepInEx;
using BepInEx.Configuration;
using Fungus;
using HarmonyLib;
//using KBEngine;
using System;
using System.Collections.Generic;
using System.Linq;
//using KBEngine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UltimateSurvival.ItemProperty;


namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.MoreNPCInfo", "MoreNPCInfo", "1.0")]
    public class MoreNPCInfo : BaseUnityPlugin
    {
        void Start()
        {
            //输出日志
            Logger.LogInfo("更多NPC信息加载成功！");
            var harmony = new Harmony("Ventulus.MCS.MoreNPCInfo");
            harmony.PatchAll();

        }

        public static MoreNPCInfo Instance;
        void Awake()
        {
            Instance = this;
        }

        //声望界面小修
        [HarmonyPatch(typeof(UINPCJiaoHu))]
        class UINPCJiaoHuPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ShowNPCInfoPanel")]
            public static void ShowNPCInfoPanelPostfix(UINPCData npc)
            {
                Instance.Logger.LogInfo("ShowNPCInfoPanel");
                UINPCData NPCData = UINPCJiaoHu.Inst.NowJiaoHuNPC;
                Instance.Logger.LogInfo(NPCData.json.ToString());
                UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
            }
        }
    }
}
