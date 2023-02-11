using BepInEx;
using HarmonyLib;
using KBEngine;
using script.NewLianDan;
using script.NewLianDan.LianDan;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.ExpectedDate", "ExpectedDate", "1.0")]
    public class ExpectedDate : BaseUnityPlugin
    {
        void Start()
        {
            //输出日志
            Logger.LogInfo("预计日期加载成功！");
            var harmony = new Harmony("Ventulus.MCS.ExpectedDate");
            harmony.PatchAll();
        }

        public static ExpectedDate Instance;
        void Awake()
        {
            Instance = this;
        }

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

                Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddDays(lingwuday);

                __instance.LingWuXiaoHaoText.text = __instance.LingWuXiaoHaoText.text + " 预计日期" + yuqi.ToLongDateString();
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

                Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddMonths(tupomon);

                __instance.TuPoXiaoHaoText.text = __instance.TuPoXiaoHaoText.text + " 预计日期" + yuqi.ToLongDateString();
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

                Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddDays(ganwuday);

                Text curExpText = Traverse.Create(__instance).Field("curExpText").GetValue<Text>();
                curExpText.AddText(" 预计日期");
                curExpText.AddText(yuqi.ToLongDateString());
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
                Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime();
                yuqi = yuqi.AddDays(liandanDays);
                __instance.Content.AddText(" 预计日期");
                __instance.Content.AddText(yuqi.ToLongDateString());
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

                Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddMonths(costTime);

                UIPopTip.Inst.Pop("预计日期" + yuqi.ToLongDateString(), PopTipIconType.叹号);
                return true;
            }
        }
    }
}
