using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.TrueRandom", "真正的随机", "2.0.0")]
    public class TrueRandom : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
            StrongRandom = Config.Bind<RandomMethod>("Ventulus", "选择采用的随机方法", RandomMethod.Pseudo, new ConfigDescription("伪随机方法足够使用且较原版能提高些许性能，强随机方法相较会轻微降低性能。"));
        }
        void Start()
        {
            Logger.LogInfo("加载成功");
            var harmony = new Harmony("Ventulus.MCS.TrueRandom");
            harmony.PatchAll();
        }
        public static TrueRandom Instance;
        public static ConfigEntry<RandomMethod> StrongRandom;
        private System.Random random = new System.Random();
        private RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();
        public enum RandomMethod
        {
            [Description("原版随机")]
            Original,
            [Description("伪随机")]
            Pseudo,
            [Description("强随机")]
            Strong,
        }
        public static int GetStrongRandom()
        {
            byte[] array = new byte[4];
            Instance.RNG.GetBytes(array);
            return Math.Abs(BitConverter.ToInt32(array, 0));
        }

        [HarmonyPatch(typeof(jsonData))]
        class jsonData_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(jsonData.QuikeGetRandom))]
            public static bool QuikeGetRandom_Prefix(ref int __result)
            {
                if (StrongRandom.Value == RandomMethod.Pseudo)
                {
                    __result = Instance.random.Next();
                    return false;
                }
                else if (StrongRandom.Value == RandomMethod.Strong)
                {
                    __result = GetStrongRandom();
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(jsonData.getRandom))]
            public static bool getRandom_Prefix(ref int __result)
            {
                if (StrongRandom.Value == RandomMethod.Pseudo)
                {
                    __result = Instance.random.Next();
                    return false;
                }
                else if (StrongRandom.Value == RandomMethod.Strong)
                {
                    __result = GetStrongRandom();
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(jsonData.GetRandom))]
            public static bool GetRandom_Prefix(ref int __result)
            {
                if (StrongRandom.Value == RandomMethod.Pseudo)
                {
                    __result = Instance.random.Next();
                    return false;
                }
                else if (StrongRandom.Value == RandomMethod.Strong)
                {
                    __result = GetStrongRandom();
                    return false;
                }
                return true;
            }

        }
    }
}
