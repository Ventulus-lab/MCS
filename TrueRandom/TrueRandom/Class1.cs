using BepInEx;
using HarmonyLib;
using System;
using System.Security.Cryptography;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.TrueRandom", "TrueRandom", "1.0")]
    public class TrueRandom : BaseUnityPlugin
    {
        void Start()
        {

            Logger.LogInfo("真正的随机加载成功！");
            var harmony = new Harmony("Ventulus.MCS.TrueRandom");
            harmony.PatchAll();
        }
        public static TrueRandom Instance;
        void Awake()
        {
            Instance = this;
        }
        [HarmonyPatch(typeof(jsonData))]
        class jsonDataPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("QuikeGetRandom")]
            public static bool QuikeGetRandomPrefix(ref int __result)
            {
                __result = jsonData.GetRandom();
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetRandom")]
            public static bool GetRandomPrefix(ref int __result)
            {
                byte[] bytes = new byte[4];
                new RNGCryptoServiceProvider().GetBytes(bytes);
                Instance.Logger.LogInfo(BitConverter.ToString(bytes));
                int I =  BitConverter.ToInt32(bytes, 0);
                //整数位运算取绝对值
   
                __result = I ^ (I >> 31) - (I >> 31);
                Instance.Logger.LogInfo(__result);
                return false;
            }


        }
    }
}
