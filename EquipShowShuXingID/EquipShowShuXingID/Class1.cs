using Bag;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.EquipShowShuXingID", "EquipShowShuXingID", "1.0")]
    public class EquipShowShuXingID : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("装备显示属性ID加载成功！");
            var harmony = new Harmony("Ventulus.MCS.EquipShowShuXingID");
            harmony.PatchAll();
        }

        public static EquipShowShuXingID Instance;

        void Awake()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(ToolTipsMag))]
        class TooltipItem_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(ToolTipsMag.Show), new Type[] { typeof(BaseItem) })]
            public static void Show_Postfix(ToolTipsMag __instance, BaseItem baseItem)
            {
                if(baseItem.ItemType == Bag.ItemType.法宝)
                {
                    List<int> listShuXingID;
                    //Instance.Logger.LogInfo(baseItem.Seid.ToString());
                    if (baseItem.Seid.HasField("shuXingIdList"))
                    {
                        listShuXingID = baseItem.Seid["shuXingIdList"].ToList();
                        __instance.Desc2.AddText(Environment.NewLine);
                        __instance.Desc2.AddText("属性ID：");
                        __instance.UpdateSize();
                        if (listShuXingID.Count > 0)
                        {
                            foreach(int ShuXingID in listShuXingID)
                            {
                                __instance.Desc2.AddText(Environment.NewLine);
                                __instance.Desc2.AddText(ShuXingID);
                                __instance.Desc2.AddText(GetEquipHeChengStr(ShuXingID));
                                __instance.UpdateSize();
                            }
                        }                      
                    }  
                }
            }
        }
        public static string GetEquipHeChengStr(int id, bool bShowInt = false)
        {
            JSONObject HeChengBiao = jsonData.instance.LianQiHeCheng;
            JSONObject ShuXingLeiBie = jsonData.instance.LianQiShuXinLeiBie;
            if (HeChengBiao.HasField(id.ToString()))
            {
                string strShuXing = ShuXingLeiBie[HeChengBiao[id.ToString()]["ShuXingType"].ToString()]["desc"].Str;
                string strEquipType = HeChengBiao[id.ToString()]["ZhuShi2"].str;
                string strXiaoGuo = HeChengBiao[id.ToString()]["ZhuShi3"].str;
                return $"{strShuXing}·{strEquipType}·{strXiaoGuo}".ToCN();
            }
            else
                return "未知";
        }
    }
}
