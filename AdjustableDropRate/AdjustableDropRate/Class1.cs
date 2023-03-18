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
    [BepInPlugin("Ventulus.MCS.AdjustableDropRate", "可调战斗截杀掉落率", "1.0.0")]
    public class AdjustableDropRate : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;

            money0 = Config.Bind("Ventulus", "{0号掉落模式}灵石掉落%", 10, new ConfigDescription("对手灵石按百分比掉落", new AcceptableValueRange<int>(0, 100)));
            backpack0 = Config.Bind("Ventulus", "{0号掉落模式}背包掉落%", 20, new ConfigDescription("对手背包物品堆叠小于5个按概率掉落，大于等于五个按数量比例掉落", new AcceptableValueRange<int>(0, 100)));
            weapon0 = Config.Bind("Ventulus", "{0号掉落模式}装备掉落", 100, new ConfigDescription("大于0则对手装备必定全掉落，等于0则不掉落", new AcceptableValueRange<int>(0, 100)));

            money2 = Config.Bind("Ventulus", "{其他掉落模式}灵石掉落%", 0, new ConfigDescription("对手灵石按百分比掉落", new AcceptableValueRange<int>(0, 100)));
            backpack2 = Config.Bind("Ventulus", "{其他掉落模式}背包掉落%", 100, new ConfigDescription("对手背包物品堆叠小于5个按概率掉落，大于等于五个按数量比例掉落", new AcceptableValueRange<int>(0, 100)));
            weapon2 = Config.Bind("Ventulus", "{其他掉落模式}装备掉落", 0, new ConfigDescription("大于0则对手装备必定全掉落，等于0则不掉落", new AcceptableValueRange<int>(0, 100)));


        }
        void Start()
        {
            new Harmony("Ventulus.MCS.AdjustableDropRate").PatchAll();
            Logger.LogInfo("加载成功！");
        }

        public static AdjustableDropRate Instance;
        public static ConfigEntry<int> money0;
        public static ConfigEntry<int> backpack0;
        public static ConfigEntry<int> weapon0;
        public static ConfigEntry<int> money2;
        public static ConfigEntry<int> backpack2;
        public static ConfigEntry<int> weapon2;

        [HarmonyPatch(typeof(Fight.FightVictory))]
        class Fight_FightVictory_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Init")]
            public static bool Init_Prefix()
            {
                int monstarID = Tools.instance.MonstarID;
                JSONObject backpack;
                if (jsonData.instance.AvatarBackpackJsonData.HasField(monstarID.ToString()))
                {
                    backpack = jsonData.instance.AvatarBackpackJsonData[monstarID.ToString()];
                    Instance.Logger.LogInfo(backpack.ToString());
                }
                int dropType = jsonData.instance.AvatarJsonData[monstarID.ToString()]["dropType"].I;
                Instance.Logger.LogInfo("掉落模式" + dropType.ToString());

                JSONObject DropInfo = jsonData.instance.DropInfoJsonData.list.FirstOrDefault(x => dropType == (int)x["dropType"].n);
                if (DropInfo != null)
                {
                    if (dropType == 0)
                    {
                        DropInfo.SetField("moneydrop", money0.Value);
                        DropInfo.SetField("backpack", backpack0.Value);
                        DropInfo.SetField("wepen", weapon0.Value);

                    }
                    else
                    {
                        DropInfo.SetField("moneydrop", money2.Value);
                        DropInfo.SetField("backpack", backpack2.Value);
                        DropInfo.SetField("wepen", weapon2.Value);
                    }
                    Instance.Logger.LogInfo(DropInfo.ToString().ToCN());
                }

                return true;
            }
        }
    }
}
