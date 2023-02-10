using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UltimateSurvival;
using UnityEngine;
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.NPCToPoEarlierByAbility", "NPCToPoEarlierByAbility", "1.0")]
    public class NPCToPoEarlierByAbility : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("保送NPC凭本事提前突破加载成功！");
            var harmony = new Harmony("Ventulus.MCS.NPCToPoEarlierByAbility");
            harmony.PatchAll();

        }

        public static NPCToPoEarlierByAbility Instance;
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
                Instance.Logger.LogInfo("NPCTuPo.GetNpcBigTuPoLv共计IL指令数量" +  codes.Count);
                for (var i = 0; i < codes.Count; i++)
                {
                    //Instance.Logger.LogInfo(codes[i].ToString());
                    if (codes[i].opcode == OpCodes.Ret && codes[i-1].opcode == OpCodes.Ldc_I4_M1)
                    {
                        Instance.Logger.LogInfo("找到返回-1指令开始删除");
                        codes[i].opcode = OpCodes.Nop;
                        codes[i - 1].opcode = OpCodes.Nop;
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}
