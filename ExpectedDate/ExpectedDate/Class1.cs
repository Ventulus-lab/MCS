using Bag;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JSONClass;
using script.ExchangeMeeting.Logic.Interface;
using script.ExchangeMeeting.UI.Interface;
//using KBEngine;
using script.ExchangeMeeting.UI.UI;
using script.NewLianDan;
using script.NewLianDan.LianDan;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DebuggingEssentials.RuntimeInspector;
using static UINPCQingJiaoSkillData;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.ExpectedDate", "预计日期", "2.0.0")]
    public class ExpectedDate : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
            ShowExchangeExpectedDate = Config.Bind<bool>("Ventulus", "显示天机阁交易会预计日期", true, new ConfigDescription("玩家发布交易会需求时显示预计多久能换到"));
        }
        void Start()
        {
            new Harmony("Ventulus.MCS.ExpectedDate").PatchAll();
            Logger.LogInfo("加载成功");
        }

        public static ExpectedDate Instance;
        public static ConfigEntry<bool> ShowExchangeExpectedDate;
        private static Vector3 V3;

        [HarmonyPatch(typeof(UIBiGuanLingWuPanel))]
        class UIBiGuanLingWuPanel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("SetLingWu")]
            public static void SetLingWu_Postfix(UIBiGuanLingWuPanel __instance)
            {
                Instance.Logger.LogInfo("预计日期修改领悟");
                UIIconShow tmpIcon = Traverse.Create(__instance).Field("tmpIcon").GetValue<UIIconShow>();
                int itemID = tmpIcon.tmpItem.itemID;
                int lingwuday = Tools.CalcLingWuTime(itemID);

                KBEngine.Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddDays(lingwuday);

                __instance.LingWuXiaoHaoText.text += $" 预计日期{yuqi.Year}年{yuqi.Month}月{yuqi.Day}日";
            }
        }

        [HarmonyPatch(typeof(UIBiGuanTuPoPanel))]
        class UIBiGuanTuPoPanelPanel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("SetTuPo")]
            public static void SetTuPo_Postfix(UIBiGuanTuPoPanel __instance)
            {
                Instance.Logger.LogInfo("预计日期修改突破");
                UIIconShow tmpIcon = Traverse.Create(__instance).Field("tmpIcon").GetValue<UIIconShow>();
                int skill_ID = tmpIcon.tmpSkill.skill_ID;
                int tupomon = Tools.CalcTuPoTime(skill_ID);

                KBEngine.Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddMonths(tupomon);

                __instance.TuPoXiaoHaoText.text += $" 预计日期{yuqi.Year}年{yuqi.Month}月{yuqi.Day}日";
            }
        }

        [HarmonyPatch(typeof(GanWuSelect))]
        class GanWuSelect_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("updateData")]
            public static void updateData_Postfix(GanWuSelect __instance)
            {
                Instance.Logger.LogInfo("预计日期修改感悟");
                int ganwuday = Traverse.Create(__instance).Field("curDay").GetValue<int>();

                KBEngine.Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddDays(ganwuday);

                Text curExpText = Traverse.Create(__instance).Field("curExpText").GetValue<Text>();
                curExpText.AddText($" 预计日期{yuqi.Year}年{yuqi.Month}月{yuqi.Day}日");
                Traverse.Create(__instance).Field("curExpText").SetValue(curExpText);
            }
        }


        [HarmonyPatch(typeof(LianDanSelect))]
        class LianDanSelect_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("UpdateUI")]
            public static void UpdateUI_Postfix(LianDanSelect __instance)
            {
                Instance.Logger.LogInfo("预计日期修改炼丹");

                LianDanPanel liandanpanel = LianDanUIMag.Instance.LianDanPanel;
                int count = __instance.CurNum;
                List<LianDanResultManager.DanyaoItem> danYaoItemList = new List<LianDanResultManager.DanyaoItem>();
                List<int> indexToLeixin = new List<int>
                {
                    1,
                    2,
                    2,
                    3,
                    3
                };
                Dictionary<int, int> fuyaoList = new Dictionary<int, int>();
                Dictionary<int, int> zhuyaoList = new Dictionary<int, int>();
                liandanpanel.GetYaoLeiList(indexToLeixin, danYaoItemList, fuyaoList, zhuyaoList, false);

                List<JSONObject> DanFans = new List<JSONObject>();
                liandanpanel.GetDanfangList(DanFans, indexToLeixin, danYaoItemList, fuyaoList, zhuyaoList);

                int liandanDays = 0;
                if (DanFans.Count > 0)
                {
                    liandanpanel.GetDanFang(out int maxNum, out int maxpingzhi, out JSONObject danFangItemID, DanFans, indexToLeixin, danYaoItemList, fuyaoList, zhuyaoList);

                    List<int> list2 = new List<int>
                    {
                        3,
                        4,
                        5,
                        6,
                        7,
                        8
                    };
                    liandanDays = (maxpingzhi <= 0) ? (3 * count) : list2[maxpingzhi - 1] * count;
                }
                else
                    liandanDays = 3 * count;
                KBEngine.Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime();
                yuqi = yuqi.AddDays(liandanDays);
                __instance.Content.AddText($" 预计日期{yuqi.Year}年{yuqi.Month}月{yuqi.Day}日");
            }
        }

        [HarmonyPatch(typeof(PutMaterialPageManager))]
        class PutMaterialPageManager_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("lianQiBtnOnclick")]
            public static bool lianQiBtnOnclick_Prefix()
            {
                Instance.Logger.LogInfo("预计日期修改炼器");
                int costTime = LianQiTotalManager.inst.lianQiResultManager.getCostTime();

                KBEngine.Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddMonths(costTime);

                UIPopTip.Inst.Pop($"预计日期 {yuqi.Year}年{yuqi.Month}月{yuqi.Day}日", PopTipIconType.叹号);
                return true;
            }
        }
        [HarmonyPatch(typeof(script.ExchangeMeeting.UI.UI.PublishingDataUI))]
        class script_ExchangeMeeting_UI_UI_PublishingDataUI_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Init")]
            public static void Init_Postfix(PublishingDataUI __instance)
            {
                if (!ShowExchangeExpectedDate.Value)
                    return;
                Transform tchoucheng = __instance.GetTransform().Find("抽成");
                PlayerExchangeData data = Traverse.Create(__instance).Field("data").GetValue<PlayerExchangeData>();
                if (tchoucheng != null && data != null)
                {
                    //UI修改
                    tchoucheng.Find("名称").localPosition = new Vector3(0, 0, 0);
                    V3 = tchoucheng.localPosition;
                    V3.y = -20;
                    tchoucheng.localPosition = V3;
                    Transform tYuQi = UnityEngine.Object.Instantiate<GameObject>(tchoucheng.gameObject, tchoucheng.parent).transform;
                    tYuQi.name = "预计日期";
                    V3 = tYuQi.localPosition;
                    V3.y = 35;
                    tYuQi.localPosition = V3;
                    tYuQi.Find("名称").GetComponent<Text>().text = "预计日期";
                    tYuQi.Find("Value").localPosition += new Vector3 (20, 0, 0);
                    tYuQi.Find("Value/1").gameObject.SetActive(false);

                    //数据计算
                    if (data.NeedUpdate)
                    {
                        int shengyumonth = data.NeedTime - data.HasCostTime;
                        KBEngine.Avatar player = Tools.instance.getPlayer();
                        DateTime yuqi = player.worldTimeMag.getNowTime().AddMonths(shengyumonth);
                        SetYuQi(tYuQi, $"预计日期 {yuqi.Year}年{yuqi.Month}月", $"{data.HasCostTime}/{data.NeedTime}个月");
                    }
                    else
                    {
                        SetYuQi(tYuQi, $"预计日期", $"无人能获取");
                    }


                }
            }

        }
        static void SetYuQi(Transform tYuQi, string name, string value)
        {
            tYuQi.Find("名称").GetComponent<Text>().text = name;
            tYuQi.Find("Value").GetComponent<Text>().text = value;
        }
        [HarmonyPatch(typeof(script.ExchangeMeeting.UI.UI.PublishDataUI))]
        class script_ExchangeMeeting_UI_UI_PublishDataUI_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("UpdateUI")]
            public static void UpdateUI_Postfix(PublishDataUI __instance)
            {
                if (!ShowExchangeExpectedDate.Value)
                    return;
                Transform tchoucheng = __instance.GetTransform().Find("抽成");
                Transform tYuQi = __instance.GetTransform().Find("预计日期");
                if (tchoucheng != null && tYuQi == null)
                {
                    //UI修改
                    tchoucheng.Find("名称").localPosition = new Vector3(0, 0, 0);
                    V3 = tchoucheng.localPosition;
                    V3.y = -20;
                    tchoucheng.localPosition = V3;
                    tYuQi = UnityEngine.Object.Instantiate<GameObject>(tchoucheng.gameObject, tchoucheng.parent).transform;
                    tYuQi.name = "预计日期";
                    V3 = tYuQi.localPosition;
                    V3.y = 35;
                    tYuQi.localPosition = V3;
                    tYuQi.Find("名称").GetComponent<Text>().text = "预计日期";
                    tYuQi.Find("Value").localPosition += new Vector3(20, 0, 0);
                    tYuQi.Find("Value/1").gameObject.SetActive(false);

                }
                //数据计算
                if (__instance.NeedItem == null || __instance.GiveItems == null)
                {
                    SetYuQi(tYuQi, $"预计日期", $"UI错误");
                }
                else
                {
                    switch (CheckCanPublishGetNeedSay(__instance.NeedItem, __instance.GiveItems))
                    {
                        case 0:
                            SetYuQi(tYuQi, $"预计日期", $"请放入物品");
                            break;
                        case 1:
                            SetYuQi(tYuQi, $"预计日期", $"出价不够");
                            break;
                        case 2:
                            SetYuQi(tYuQi, $"预计日期", $"没钱支付抽成");
                            break;
                        case 3:
                            SetYuQi(tYuQi, $"预计日期", $"不能交换相同物品");
                            break;
                        case 11:
                            SetYuQi(tYuQi, $"预计日期", $"无人能获取");
                            break;
                        case 12:
                            int NeedTime = CalculateNeedTime(__instance.NeedItem, __instance.GiveItems);
                            KBEngine.Avatar player = Tools.instance.getPlayer();
                            DateTime yuqi = player.worldTimeMag.getNowTime().AddMonths(NeedTime);
                            SetYuQi(tYuQi, $"预计日期 约{yuqi.Year}年{yuqi.Month}月", $"约{NeedTime}个月");
                            break;
                        default:
                            SetYuQi(tYuQi, $"预计日期", $"计算错误");
                            break;
                    }
                }
            }


            [HarmonyPostfix]
            [HarmonyPatch("Clear")]
            public static void Clear_Postfix(PublishDataUI __instance)
            {
                Transform tchoucheng = __instance.GetTransform().Find("抽成");
                Transform tYuQi = __instance.GetTransform().Find("预计日期");
                if (tchoucheng != null && tYuQi != null)
                {
                    SetYuQi(tYuQi, $"预计日期", $"请放入物品");
                }
            }
        }
        public static int CheckCanPublishGetNeedSay(BaseSlot NeedItem, List<BaseSlot> GiveItems)
        {
            //CheckCanPublish
            int give = 0;
            int need = 0;
            Dictionary<int, _ItemJsonData> dataDict = _ItemJsonData.DataDict;
            foreach (BaseSlot giveItem in GiveItems)
            {
                if (!giveItem.IsNull() && dataDict.ContainsKey(giveItem.Item.Id))
                {
                    int num3 = giveItem.Item.GetPrice() * giveItem.Item.Count;
                    if (dataDict[giveItem.Item.Id].ItemFlag.Contains(52))
                    {
                        num3 = num3 * 13 / 10;
                    }
                    if (dataDict[giveItem.Item.Id].ItemFlag.Contains(53))
                    {
                        num3 = num3 * 13 / 10;
                    }
                    give += num3;
                }
            }
            if (!NeedItem.IsNull() && dataDict.ContainsKey(NeedItem.Item.Id))
            {
                int num4 = NeedItem.Item.GetPrice();
                if (dataDict[NeedItem.Item.Id].ItemFlag.Contains(52))
                {
                    num4 = num4 * 13 / 10;
                }
                if (dataDict[NeedItem.Item.Id].ItemFlag.Contains(53))
                {
                    num4 = num4 * 13 / 10;
                }
                need = num4;
            }

            //GetNeedSay
            if (NeedItem.IsNull())
            {
                return 0;
            }
            int id = NeedItem.Item.Id;
            if (_ItemJsonData.DataDict[id].ItemFlag.Contains(53))
            {
                return 11;
            }
            int DrawMoney = NeedItem.Item.BasePrice * 2 / 100;
            if (DrawMoney == 0 || DrawMoney > (int)PlayerEx.Player.money)
            {
                return 2;
            }
            if (need <= 0 || give <= 0)
            {
                return 0;
            }
            foreach (BaseSlot giveItem in GiveItems)
            {
                if (!giveItem.IsNull() && NeedItem.Item.Id == giveItem.Item.Id)
                {
                    return 3;
                }
            }
            if (need * 95 / 100 > give)
            {
                return 1;
            }
            return 12;
        }
        public static int CalculateNeedTime(BaseSlot NeedItem, List<BaseSlot> GiveItems)
        {
            int NeedValue = 0;
            int GiveValue = 0;

            if (NeedItem.Item != null)
                NeedValue += NeedItem.Item.BasePrice * NeedItem.Item.Count;

            foreach (BaseSlot giveItem in GiveItems)
            {
                if (giveItem.Item == null)
                    continue;
                int num3 = giveItem.Item.BasePrice * giveItem.Item.Count;
                if (_ItemJsonData.DataDict[giveItem.Item.Id].ItemFlag.Contains(52))
                {
                    num3 = num3 * 130 / 100;
                }
                if (_ItemJsonData.DataDict[giveItem.Item.Id].ItemFlag.Contains(53))
                {
                    num3 = num3 * 130 / 100;
                }
                GiveValue += num3;
            }
            int ValueDifference = NeedValue * 2 - GiveValue;
            //int num5 = IExchangeData.random.Next(900, 1101);
            int num5 = 1000;
            int NeedTime = ValueDifference / num5;
            if (ValueDifference % num5 != 0)
            {
                NeedTime++;
            }
            if (NeedTime <= 0)
            {
                NeedTime = 1;
            }
            return NeedTime;
        }
    }
}
