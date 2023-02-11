using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.NPCToPoEarlierByAbility", "NPCToPoEarlierByAbility", "1.0")]
    public class NPCToPoEarlierByAbility : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("固定NPC又想凭本事又想保送加载成功！");
            var harmony = new Harmony("Ventulus.MCS.NPCToPoEarlierByAbility");
            harmony.PatchAll();
            //EnableNPCToPoByAbility = Config.Bind<bool>("config", "EnableNPCToPoByAbility", true, "本模组允许固定NPC凭本事突破的主要功能，默认true，则固定NPC可以修为圆满后和普通NPC一样自行尝试突破；若想和游戏原版一样则选false在特定日期前禁止其突破");
            //EnableNPCAddSpeed = Config.Bind<bool>("config", "EnableNPCAddSpeed", false, "为配合本模组允许固定NPC凭本事突破的主要功能，默认false关闭固定NPC修炼额外加速，使之和普通NPC一样修炼；若想和游戏原版一样则选true有额外加速");
            //EnableNPCBaoSong = Config.Bind<bool>("config", "EnableNPCBaoSong", true, "固定NPC保送功能，到特定时间必定突破成功，默认选true按游戏原版开启；若为false则关闭保送，有可能固定NPC会半路夭折");
        }

        public static NPCToPoEarlierByAbility Instance;
        //public static ConfigEntry<bool> EnableNPCToPoByAbility;
        //public static ConfigEntry<bool> EnableNPCAddSpeed;
        //public static ConfigEntry<bool> EnableNPCBaoSong;
        void Awake()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(NPCTuPo))]
        public static class NPCTuPo_Patch
        {
            [HarmonyPatch(nameof(NPCTuPo.GetNpcBigTuPoLv))]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> GetNpcBigTuPoLv_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                Instance.Logger.LogInfo("NPCTuPo.GetNpcBigTuPoLv共计IL指令数量" + codes.Count);

                if (true)
                {
                    //允许固定NPC凭本事突破
                    for (var i = 0; i < codes.Count; i++)
                    {
                        //Instance.Logger.LogInfo(codes[i].ToString());
                        if (codes[i].opcode == OpCodes.Ret && codes[i - 1].opcode == OpCodes.Ldc_I4_M1)
                        {
                            Instance.Logger.LogInfo("找到返回-1指令开始删除");
                            codes[i].opcode = OpCodes.Nop;
                            codes[i - 1].opcode = OpCodes.Nop;
                        }
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(NpcJieSuanManager))]
        public static class NpcJieSuanManager_Patch
        {
            [HarmonyPatch(nameof(NpcJieSuanManager.GuDingAddExp))]
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> GuDingAddExp_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                Instance.Logger.LogInfo("NpcJieSuanManager.GuDingAddExp共计IL指令数量" + codes.Count);

                if (true)
                {
                    //关闭固定NPC修炼额外加速
                    for (var i = 0; i < codes.Count; i++)
                    {
                        //Instance.Logger.LogInfo(codes[i].ToString());
                        if (codes[i].opcode == OpCodes.Ldloc_2 && codes[i + 2].opcode == OpCodes.Bne_Un)
                        {
                            Instance.Logger.LogInfo("找到判断境界指令开始跳过");
                            codes[i + 1].opcode = OpCodes.Ldc_I4_0;
                        }
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}
