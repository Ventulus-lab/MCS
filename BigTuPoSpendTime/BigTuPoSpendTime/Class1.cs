using BepInEx;
using BepInEx.Configuration;
using Fungus;
using HarmonyLib;
using KBEngine;
using PaiMai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.BigTuPoSpendTime", "大境界突破花费时间", "1.1")]
    public class BigTuPoSpendTime : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("大境界突破花费时间加载成功！");
            ZhuJiHuiHeDays = Config.Bind<int>("config", "筑基每回合天数", 1, "默认1天");
            JieDanHuiHeDays = Config.Bind<int>("config", "结丹每回合天数", 3, "默认3天");
            JieYingHuiHeDays = Config.Bind<int>("config", "结婴每回合天数", 5, "默认5天");
            HuaShenHuiHeDays = Config.Bind<int>("config", "化神每回合天数", 7, "默认7天");
            DuJieHuiHeDays = Config.Bind<int>("config", "渡劫每回合天数", 9, "默认9天");
            var harmony = new Harmony("Ventulus.MCS.BigTuPoSpendTime");
            harmony.PatchAll();
        }

        public static BigTuPoSpendTime Instance;
        public static ConfigEntry<int> ZhuJiHuiHeDays;
        public static ConfigEntry<int> JieDanHuiHeDays;
        public static ConfigEntry<int> JieYingHuiHeDays;
        public static ConfigEntry<int> HuaShenHuiHeDays;
        public static ConfigEntry<int> DuJieHuiHeDays;
        void Awake()
        {
            Instance = this;
        }


        [HarmonyPatch(typeof(RoundManager))]
        class RoundManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(RoundManager.endRound))]
            public static void endRound_Postfix(Entity _avater)
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改每回合");
                //Instance.Logger.LogInfo(Tools.instance.monstarMag.FightType);
                Avatar avatar = (Avatar)_avater;
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.ZhuJi && avatar.isPlayer())
                {
                    avatar.AddTime(ZhuJiHuiHeDays.Value, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.JieDan && avatar.isPlayer())
                {
                    avatar.AddTime(JieDanHuiHeDays.Value, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.JieYing && avatar.isPlayer())
                {
                    avatar.AddTime(JieYingHuiHeDays.Value, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.HuaShen && avatar.isPlayer())
                {
                    avatar.AddTime(HuaShenHuiHeDays.Value, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.DuJie && avatar.isPlayer())
                {
                    avatar.AddTime(DuJieHuiHeDays.Value, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
            }
        }



        [HarmonyPatch(typeof(JieDanManager))]
        class JieDanManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(JieDanManager.JieSuan))]
            public static void JieSuan_Postfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改金丹提前完成");
                Avatar player = Tools.instance.getPlayer();
                //buff4010表示剩余回合，若结算时还有表示结丹提前完成
                if (player.buffmag.HasBuff(4010))
                {
                    player.AddTime(JieDanHuiHeDays.Value, 0, 0);
                    Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
                }
            }
        }

        [HarmonyPatch(typeof(JieYin))]
        class JieYin_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(JieYin.JieYinSuccess))]
            public static void JieYinSuccess_Postfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改结婴成功");
                Avatar player = Tools.instance.getPlayer();

                player.AddTime(JieYingHuiHeDays.Value, 0, 0);
                Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(JieYin.JieYinFail))]
            public static void JieYinFail_Postfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改结婴失败");
                Avatar player = Tools.instance.getPlayer();

                player.AddTime(JieYingHuiHeDays.Value, 0, 0);
                Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(JieYin.JieYingYiZhiFail))]
            public static void JieYingYiZhiFail_Postfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改结婴意志失败");
                Avatar player = Tools.instance.getPlayer();

                player.AddTime(JieYingHuiHeDays.Value, 0, 0);
                Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
            }
        }
    }
}
