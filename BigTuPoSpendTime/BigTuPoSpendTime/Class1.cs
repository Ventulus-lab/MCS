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
using Fungus;
using PaiMai;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.BigTuPoSpendTime", "BigTuPoSpendTime", "1.0")]
    public class BigTuPoSpendTime : BaseUnityPlugin
    {
        void Start()
        {
            //输出日志
            Logger.LogInfo("大境界突破花费时间加载成功！");
            var harmony = new Harmony("Ventulus.MCS.BigTuPoSpendTime");
            harmony.PatchAll();
        }

        public static BigTuPoSpendTime Instance;
        void Awake()
        {
            Instance = this;
        }

        
        [HarmonyPatch(typeof(RoundManager))]
        class RoundManagerPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("endRound")]
            public static void Postfix(Entity _avater)
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改每回合");
                //Instance.Logger.LogInfo(Tools.instance.monstarMag.FightType);
                Avatar avatar = (Avatar)_avater;
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.ZhuJi && avatar.isPlayer())
                {
                    avatar.AddTime(1, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.JieDan && avatar.isPlayer())
                {
                    avatar.AddTime(3, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.JieYing && avatar.isPlayer())
                {
                    avatar.AddTime(5, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }     
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.HuaShen && avatar.isPlayer())
                {
                    avatar.AddTime(7, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
                if (Tools.instance.monstarMag.FightType == StartFight.FightEnumType.DuJie && avatar.isPlayer())
                {
                    avatar.AddTime(9, 0, 0);
                    Instance.Logger.LogInfo(avatar.worldTimeMag.nowTime);
                }
            }
        }
            


        [HarmonyPatch(typeof(JieDanManager))]
        class JieDanManagerPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("JieSuan")]
            public static void Postfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改金丹提前完成");
                Avatar player = Tools.instance.getPlayer();
                //buff4010表示剩余回合，若结算时还有表示结丹提前完成
                if (player.buffmag.HasBuff(4010))
                {
                    player.AddTime(3, 0, 0);
                    Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
                }
            }
        }

        [HarmonyPatch(typeof(JieYin))]
        class JieYinPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("JieYinSuccess")]
            public static void JieYinSuccessPostfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改结婴成功");
                Avatar player = Tools.instance.getPlayer();

                player.AddTime(5, 0, 0);
                Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
            }

            [HarmonyPostfix]
            [HarmonyPatch("JieYinFail")]
            public static void JieYinFailPostfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改结婴失败");
                Avatar player = Tools.instance.getPlayer();

                player.AddTime(5, 0, 0);
                Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
            }

            [HarmonyPostfix]
            [HarmonyPatch("JieYingYiZhiFail")]
            public static void JieYingYiZhiFailPostfix()
            {
                Instance.Logger.LogInfo("大境界突破花费时间修改结婴意志失败");
                Avatar player = Tools.instance.getPlayer();

                player.AddTime(5, 0, 0);
                Instance.Logger.LogInfo(player.worldTimeMag.nowTime);
            }
        }
    }
}
