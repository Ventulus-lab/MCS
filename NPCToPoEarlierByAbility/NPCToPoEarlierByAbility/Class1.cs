using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NPCToPoEarlierByAbility
{
    [BepInPlugin("Ventulus.MCS.NPCToPoEarlierByAbility", "NPCToPoEarlierByAbility", "1.0")]
    public class NPCToPoEarlierByAbility : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("重要NPC凭本事提前突破加载成功！");
            var harmony = new Harmony("Ventulus.MCS.NPCToPoEarlierByAbility");
            harmony.PatchAll();

        }

        public static NPCToPoEarlierByAbility Instance;
        void Awake()
        {
            Instance = this;
        }
    }
}
