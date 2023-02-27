using Bag;
using BepInEx;
using BepInEx.Configuration;
using DG.Tweening;
using GUIPackage;
using HarmonyLib;
using JSONClass;
using script.NewLianDan;
using script.NewLianDan.LianDan;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WXB;
using YSGame.EquipRandom;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.LianDanShowYaoZhi", "炼丹显示药质", "1.0")]
    public class LianDanShowYaoZhi : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("炼丹丹方丹药草药显示药性药力加载成功！");
            ShowCaoYaoYaoZhi = Config.Bind<bool>("config", "显示草药药质", true, "草药显示药性和药力");
            ShowDanYaoYaoZhi = Config.Bind<bool>("config", "显示丹药药质", true, "丹药和丹方显示炼制需要的药性和药力");
            ShowLianDanYaoZhi = Config.Bind<bool>("config", "显示炼丹药质", true, "炼丹界面即时显示放入草药的总和药性和药力");
            var harmony = new Harmony("Ventulus.MCS.LianDanShowYaoZhi");
            harmony.PatchAll();
        }

        public static LianDanShowYaoZhi Instance;
        public static ConfigEntry<bool> ShowCaoYaoYaoZhi;
        public static ConfigEntry<bool> ShowDanYaoYaoZhi;
        public static ConfigEntry<bool> ShowLianDanYaoZhi;
        public static List<int> YaoZhi = new List<int>() { 1, 3, 9, 36, 180, 1080 };
        public static List<int> indexToLeixin = new List<int> { 1, 2, 2, 3, 3 };

        void Awake()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(ToolTipsMag))]
        class ToolTipsMag_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(ToolTipsMag.Show), new Type[] { typeof(BaseItem) })]
            public static void Show_Postfix(ToolTipsMag __instance, BaseItem baseItem)
            {
                if (ShowCaoYaoYaoZhi.Value && baseItem.ItemType == Bag.ItemType.草药)
                {
                    CaoYaoItem caoYaoItem = (CaoYaoItem)baseItem;
                    if (caoYaoItem.ZhuYao != 0)
                    {
                        LianDanItemLeiXin YaoZhiZhongLei = LianDanItemLeiXin.DataDict[caoYaoItem.ZhuYao];
                        __instance.Desc2.AddText(Environment.NewLine);
                        __instance.Desc2.AddText("主药：");
                        __instance.Desc2.AddText(YaoZhiZhongLei.name);
                        __instance.Desc2.AddText("×" + YaoZhi[caoYaoItem.GetBaseQuality() - 1]);
                        if (caoYaoItem.Count > 1)
                        {
                            __instance.Desc2.AddText("×" + caoYaoItem.Count);
                            __instance.Desc2.AddText("=" + YaoZhi[caoYaoItem.GetBaseQuality() - 1] * caoYaoItem.Count);
                        }
                        __instance.UpdateSize();
                    }
                    if (caoYaoItem.FuYao != 0)
                    {
                        LianDanItemLeiXin YaoZhiZhongLei = LianDanItemLeiXin.DataDict[caoYaoItem.FuYao];
                        __instance.Desc2.AddText(Environment.NewLine);
                        __instance.Desc2.AddText("辅药：");
                        __instance.Desc2.AddText(YaoZhiZhongLei.name);
                        __instance.Desc2.AddText("×" + YaoZhi[caoYaoItem.GetBaseQuality() - 1]);
                        if (caoYaoItem.Count > 1)
                        {
                            __instance.Desc2.AddText("×" + caoYaoItem.Count);
                            __instance.Desc2.AddText("=" + YaoZhi[caoYaoItem.GetBaseQuality() - 1] * caoYaoItem.Count);
                        }
                        __instance.UpdateSize();
                    }
                    if (caoYaoItem.YaoYin != 0)
                    {
                        LianDanItemLeiXin YaoZhiZhongLei = LianDanItemLeiXin.DataDict[caoYaoItem.YaoYin];
                        __instance.Desc2.AddText(Environment.NewLine);
                        __instance.Desc2.AddText("药引：");
                        __instance.Desc2.AddText(YaoZhiZhongLei.name);
                        __instance.Desc2.AddText("×" + YaoZhi[caoYaoItem.GetBaseQuality() - 1]);
                        if (caoYaoItem.Count > 1)
                        {
                            __instance.Desc2.AddText("×" + caoYaoItem.Count);
                            __instance.Desc2.AddText("=" + YaoZhi[caoYaoItem.GetBaseQuality() - 1] * caoYaoItem.Count);
                        }
                        __instance.UpdateSize();
                    }

                }

                if (ShowDanYaoYaoZhi.Value && baseItem.ItemType == Bag.ItemType.丹药)
                {
                    Instance.Logger.LogInfo("丹药显示 " + baseItem.GetName() + baseItem.Id);

                    ShowYaoZhi(ref __instance, baseItem);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(ToolTipsMag.Show), new Type[] { typeof(BaseItem), typeof(Vector2) })]
            public static void Show_Vector2_Postfix(ToolTipsMag __instance, BaseItem baseItem)
            {
                if (ShowDanYaoYaoZhi.Value && baseItem.ItemType == Bag.ItemType.丹药)
                {
                    Instance.Logger.LogInfo("丹方显示丹药" + baseItem.GetName() + baseItem.Id);

                    ShowYaoZhi(ref __instance, baseItem);
                }
            }

            static void ShowYaoZhi(ref ToolTipsMag __instance, BaseItem baseItem)
            {
                JSONObject DanFang = jsonData.instance.LianDanDanFangBiao.list.FirstOrDefault(item => item["ItemID"].I == baseItem.Id);
                if (DanFang == null) return;
                Dictionary<int, int> YaoYinDict = new Dictionary<int, int>();
                Dictionary<int, int> ZhuYaoDict = new Dictionary<int, int>();
                Dictionary<int, int> FuYaoDict = new Dictionary<int, int>();
                for (int i = 1; i <= 5; i++)
                {
                    int CaoYaoId = DanFang["value" + i].I;
                    if (CaoYaoId > 0)
                    {
                        JSONObject CaoYaoJson = jsonData.instance.ItemJsonData[CaoYaoId.ToString()];
                        int YaoZhiType = CaoYaoJson["yaoZhi" + indexToLeixin[i - 1]].I;
                        int CaoYaoQuality = CaoYaoJson["quality"].I;
                        if (indexToLeixin[i - 1] == 1)
                            Tools.dictionaryAddNum(YaoYinDict, YaoZhiType, DanFang["num" + i].I * YaoZhi[CaoYaoQuality - 1]);
                        if (indexToLeixin[i - 1] == 2)
                            Tools.dictionaryAddNum(ZhuYaoDict, YaoZhiType, DanFang["num" + i].I * YaoZhi[CaoYaoQuality - 1]);
                        if (indexToLeixin[i - 1] == 3)
                            Tools.dictionaryAddNum(FuYaoDict, YaoZhiType, DanFang["num" + i].I * YaoZhi[CaoYaoQuality - 1]);
                    }
                }
                if (ZhuYaoDict.Count > 0)
                {
                    foreach (KeyValuePair<int, int> yaozhi in ZhuYaoDict)
                    {
                        __instance.Desc2.AddText(Environment.NewLine);
                        __instance.Desc2.AddText("主药：");
                        __instance.Desc2.AddText(LianDanItemLeiXin.DataDict[yaozhi.Key].name);
                        __instance.Desc2.AddText("×" + yaozhi.Value);
                        __instance.UpdateSize();
                    }
                }
                if (FuYaoDict.Count > 0)
                {
                    foreach (KeyValuePair<int, int> yaozhi in FuYaoDict)
                    {
                        __instance.Desc2.AddText(Environment.NewLine);
                        __instance.Desc2.AddText("辅药：");
                        __instance.Desc2.AddText(LianDanItemLeiXin.DataDict[yaozhi.Key].name);
                        __instance.Desc2.AddText("×" + yaozhi.Value);
                        __instance.UpdateSize();
                    }
                }
                if (YaoYinDict.Count > 0)
                {
                    foreach (KeyValuePair<int, int> yaozhi in YaoYinDict)
                    {
                        __instance.Desc2.AddText(Environment.NewLine);
                        __instance.Desc2.AddText("药引：");
                        __instance.Desc2.AddText("配平");
                        __instance.Desc2.AddText("×" + yaozhi.Value);
                        __instance.UpdateSize();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LianDanUIMag))]
        class script_NewLianDan_LianDanUIMag_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Init")]
            public static void Init_Postfix()
            {
                Transform tLianDanPanel = LianDanUIMag.Instance.transform.Find("炼丹界面");
                if (tLianDanPanel != null)
                {
                    tLianDanPanel.Find("DanLu").localPosition = new Vector3(0, 0, 0);
                    tLianDanPanel.Find("药引").localPosition = new Vector3(-270, 180, 0);
                    tLianDanPanel.Find("辅药1").localPosition = new Vector3(270, 180, 0);

                    tLianDanPanel.Find("主药1").localPosition = new Vector3(-320, -25, 0);
                    tLianDanPanel.Find("辅药2").localPosition = new Vector3(320, -25, 0);

                    tLianDanPanel.Find("主药2").localPosition = new Vector3(-270, -230, 0);
                    tLianDanPanel.Find("丹炉").localPosition = new Vector3(270, -230, 0);
                    tLianDanPanel.Find("开始炼丹").localPosition = new Vector3(0, -350, 0);

                    Transform tYaoZhi = UnityEngine.Object.Instantiate<GameObject>(tLianDanPanel.Find("DanLu/NaiJiuDu/Value").gameObject, tLianDanPanel).transform;
                    tYaoZhi.name = "药质";
                    tYaoZhi.localPosition = new Vector3(0, 380, 0);
                    Text YaoZhiText = tYaoZhi.GetComponent<Text>();
                    YaoZhiText.fontSize = 34;
                    YaoZhiText.text = "请放入草药";
                    tYaoZhi.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(LianDanPanel))]
        class script_NewLianDan_LianDan_LianDanPanel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(LianDanPanel.CheckCanMade))]
            public static void CheckCanMade_Postfix(LianDanPanel __instance)
            {
                Transform tLianDanPanel = LianDanUIMag.Instance.transform.Find("炼丹界面");
                if (tLianDanPanel != null && tLianDanPanel.gameObject.activeSelf)
                {
                    Text YaoZhiText = tLianDanPanel.Find("药质").GetComponent<Text>();
                    if (!ShowLianDanYaoZhi.Value)
                    {
                        YaoZhiText.gameObject.SetActive(false);
                        return;
                    }
                    else
                    {
                        YaoZhiText.gameObject.SetActive(true);
                    }
                    List<LianDanResultManager.DanyaoItem> DanYaoItemList = new List<LianDanResultManager.DanyaoItem>();
                    Dictionary<int, int> fuyaoList = new Dictionary<int, int>();
                    Dictionary<int, int> zhuyaoList = new Dictionary<int, int>();
                    __instance.GetYaoLeiList(indexToLeixin, DanYaoItemList, fuyaoList, zhuyaoList, unlockYaoXing: false);

                    Instance.Logger.LogInfo("已获取炼丹物品个数" + DanYaoItemList.Count.ToString());
                    Instance.Logger.LogInfo("其中主药药质" + zhuyaoList.Count.ToString());
                    Instance.Logger.LogInfo("辅药药质" + fuyaoList.Count.ToString());
                    if (DanYaoItemList.Where(x => x.ItemID < 0).Count() == 5)
                    {
                        YaoZhiText.text = "请放入草药";
                        return;
                    }
                    else
                        YaoZhiText.text = string.Empty;
                    if (zhuyaoList.Count > 0)
                    {
                        foreach (KeyValuePair<int, int> item in zhuyaoList)
                        {
                            YaoZhiText.text += LianDanItemLeiXin.DataDict[item.Key].name;
                            YaoZhiText.text += "×" + item.Value.ToString();
                            YaoZhiText.text += Environment.NewLine;
                        }
                    }
                    if (fuyaoList.Count > 0)
                    {
                        foreach (KeyValuePair<int, int> item in fuyaoList)
                        {
                            YaoZhiText.text += LianDanItemLeiXin.DataDict[item.Key].name;
                            YaoZhiText.text += "×" + item.Value.ToString();
                            YaoZhiText.text += Environment.NewLine;
                        }
                    }
                    int HanRe = 0;
                    int Quality0 = 0;
                    for (int i = 2; i <= 5; i++)
                    {
                        if (DanYaoItemList[i - 1].ItemID > 0)
                        {
                            int yaoZhi1 = jsonData.instance.ItemJsonData[DanYaoItemList[i - 1].ItemID.ToString()]["yaoZhi1"].I;
                            if (yaoZhi1 == 1)
                                HanRe--;
                            else if (yaoZhi1 == 2)
                                HanRe++;
                        }
                    }
                    if (HanRe > 0)
                        HanRe = 1;
                    else if (HanRe < 0)
                        HanRe = -1;
                    if (DanYaoItemList[0].ItemNum > 0)
                    {
                        int yaoZhi0 = jsonData.instance.ItemJsonData[DanYaoItemList[0].ItemID.ToString()]["yaoZhi1"].I;
                        Quality0 = jsonData.instance.ItemJsonData[DanYaoItemList[0].ItemID.ToString()]["quality"].I;
                        if (yaoZhi0 == 1)
                            HanRe--;
                        else if (yaoZhi0 == 2)
                            HanRe++;
                    }
                    if (HanRe > 0)
                        YaoZhiText.text += "性热";
                    else if (HanRe < 0)
                        YaoZhiText.text += "性寒";
                    else
                        YaoZhiText.text += "配平";
                    if (DanYaoItemList[0].ItemNum > 0)
                        YaoZhiText.text += "×" + DanYaoItemList[0].ItemNum * YaoZhi[Quality0 - 1];
                    YaoZhiText.text += Environment.NewLine;
                    if (YaoZhiText.text == string.Empty)
                        YaoZhiText.text = "请放入草药";
                }
            }
        }
    }
}
