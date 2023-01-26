using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KBEngine;
using UnityEngine;
using UnityEngine.UI;
using static UltimateSurvival.ItemProperty;
using JSONClass;
using Steamworks;
using GUIPackage;
using System.Text.RegularExpressions;
using Bag;
using script.NewLianDan.LianDan;
using script.NewLianDan;
using System.Reflection.Emit;

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
        class UIBiGuanLingWuPanelPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("SetLingWu")]
            public static void SetLingWuPostfix(UIBiGuanLingWuPanel __instance)
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
        class UIBiGuanTuPoPanelPanelPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("SetTuPo")]
            public static void SetTuPoPostfix(UIBiGuanTuPoPanel __instance)
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
        class GanWuSelectPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("updateData")]
            public static void updateDataPostfix(GanWuSelect __instance)
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


        [HarmonyPatch(typeof(LianDanSelect), "UpdateUI")]
        class LianDanSelectUpdateUIPatch
        {
            public static void Postfix(LianDanSelect __instance)
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
                    liandanDays = list2[maxpingzhi - 1] * count;
                }
                /*
                string ymdstr = LianDanUIMag.Instance.LianDanPanel.GetCostTime(__instance.CurNum);
            string[] separatingStrings = { "年", "月", "日" };
            string[] ymd = ymdstr.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
            Instance.Logger.LogInfo(ymdstr);
            int d, m=0, y=0;
            d = Convert.ToInt32(ymd[ymd.Length - 1]);
            if (ymd.Length > 1) m = Convert.ToInt32(ymd[ymd.Length - 2]);
            if (ymd.Length > 2) y = Convert.ToInt32(ymd[ymd.Length - 3]);


            Avatar player = Tools.instance.getPlayer();
            DateTime yuqi = player.worldTimeMag.getNowTime();
            yuqi = yuqi.AddDays(d);
            yuqi = yuqi.AddMonths(m);
            yuqi = yuqi.AddYears(y);
                */
                Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime();
                yuqi = yuqi.AddDays(liandanDays);
                __instance.Content.AddText(" 预计日期");
                __instance.Content.AddText(yuqi.ToLongDateString());
            }
        }

        [HarmonyPatch(typeof(PutMaterialPageManager))]
        class PutMaterialPageManagerPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("lianQiBtnOnclick")]
            public static bool lianQiBtnOnclickPrefix()
            {
                Instance.Logger.LogInfo("预计日期修改炼器");
                int costTime = LianQiTotalManager.inst.lianQiResultManager.getCostTime();

                Avatar player = Tools.instance.getPlayer();
                DateTime yuqi = player.worldTimeMag.getNowTime().AddMonths(costTime);

                UIPopTip.Inst.Pop("预计日期" + yuqi.ToLongDateString(),PopTipIconType.叹号);
                return true;
            }
        }
    }
}
