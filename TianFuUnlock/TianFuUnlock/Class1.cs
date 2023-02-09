using BepInEx;
using HarmonyLib;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.TianFuUnlock", "TianFuUnlock", "1.0")]
    public class TianFuUnlock : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("天赋解锁加载成功！");
            var harmony = new Harmony("Ventulus.MCS.TianFuUnlock");
            harmony.PatchAll();
        }
        public static TianFuUnlock Instance;
        void Awake()
        {
            Instance = this;
        }
        [HarmonyPatch(typeof(MainUITianFuCell))]
        class MainUITianFuCellPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Init")]
            public static bool InitPrefix(ref JSONObject json)
            {
                //Instance.Logger.LogInfo("SetNPCInfoPrefix");
                json["jiesuo"] = new JSONObject(0);
                json["UnlockKey"] = new JSONObject(string.Empty);
                json["UnlockDesc"] = new JSONObject(string.Empty);
                return true;
            }
        }
    }
}
