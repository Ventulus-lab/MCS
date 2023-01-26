using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using KBEngine;
using UnityEngine;
using UnityEngine.UI;
using static UltimateSurvival.ItemProperty;
using JSONClass;
using Steamworks;
using GUIPackage;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using script.Sleep;
using UnityEngine.Events;
using Fungus;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.BetterXiuxi", "BetterXiuxi", "1.0")]
    public class BetterXiuxi : BaseUnityPlugin
    {
        void Start()
        {
            //输出日志
            Logger.LogInfo("更好的休息加载成功！");
            var harmony = new Harmony("Ventulus.MCS.BetterXiuxi");
            harmony.PatchAll();
        }

        public static BetterXiuxi Instance;
        void Awake()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(SceneBtnMag))]
        class SceneBtnMagPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Init")]
            public static void Postfix(SceneBtnMag __instance) 
            {
                //Instance.Logger.LogInfo("更好的休息：修改场景按钮初始化");
                string sname = Tools.getScreenName();
                Instance.Logger.LogInfo("激活场景名" + sname);

                if (sname == "S101")
                {
                    Dictionary<string, FpBtn> btnDictionary = Traverse.Create(__instance).Field("btnDictionary").GetValue<Dictionary<string, FpBtn>>();
                    FpBtn btn = btnDictionary["xiuxi"];
                    btn.mouseUpEvent.RemoveAllListeners();
                    btn.mouseUpEvent.AddListener(new UnityAction(ISleepMag.Inst.Sleep));
                    btn.gameObject.SetActive(true);
                    Instance.Logger.LogInfo("修改洞府休息按钮" + sname);
                }
                if (sname == "AllMaps")
                {
                    Dictionary<string, FpBtn> btnDictionary = Traverse.Create(__instance).Field("btnDictionary").GetValue<Dictionary<string, FpBtn>>();
                    FpBtn btn = btnDictionary["xiuxi"];
                    btn.mouseUpEvent.RemoveAllListeners();
                    btn.mouseUpEvent.AddListener(new UnityAction(yewaiSleep));
                    btn.gameObject.SetActive(true);
                    Instance.Logger.LogInfo("修改野外休息按钮" + sname);
                }
                /*
                if (sname == "S11953" || sname == "S12353" || sname == "S12951") 
                {
                    Dictionary<string, FpBtn> btnDictionary = Traverse.Create(__instance).Field("btnDictionary").GetValue<Dictionary<string, FpBtn>>();
                    FpBtn btn = btnDictionary["xiuxi"];
                    btn.mouseUpEvent.RemoveAllListeners();
                    btn.mouseUpEvent.AddListener(new UnityAction(ISleepMag.Inst.Sleep));
                    btn.gameObject.SetActive(true);
                    Instance.Logger.LogInfo("修改倪府、林府、百里府客房休息按钮" + sname);
                }
                */
                
            }

            public static void yewaiSleep()
            {
                if (!NpcJieSuanManager.inst.isCanJieSuan)
                {
                    UIPopTip.Inst.Pop("正在结算中，请稍等", PopTipIconType.叹号);
                    return;
                }
                KBEngine.Avatar player = Tools.instance.getPlayer();
                int maxday = (int)player.money;
                if (maxday >30) 
                    maxday = 30;

                USelectNum.Show("野外露天休息每天花费1灵石，休息{num}天", 1, maxday, delegate (int num)
                {
                    //Resources.Load<GameObject>("talkPrefab/TalkPrefab/talk5000").Inst(null);
                    if (num >= 4)
                    {
                        player.AddHp(player.HP_Max);
                    }
                    else
                    {
                        player.AddHp(player.HP_Max * num / 4);
                    }
                    player.money -= (ulong)num;
                    player.AddTime(num, 0, 0);
                }, null);
            }
        }

        [HarmonyPatch(typeof(KeFangSelectTime))]
        class KeFangSelectTimePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Init")]
            public static bool Prefix(KeFangSelectTime __instance)
            {
                int price = __instance.price;
                Instance.Logger.LogInfo("租赁价格为" + price);
                if (price < 5)
                    price = 10;
                else if (price < 20)
                    price = 20;
                __instance.price= price;
                Instance.Logger.LogInfo("租赁价格修改为" + price);
                return true;
            }
        }
    }
}
