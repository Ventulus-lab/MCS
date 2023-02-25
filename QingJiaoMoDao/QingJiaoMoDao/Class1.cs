using Bag;
using BepInEx;
using BepInEx.Configuration;
using Fungus;
using GUIPackage;
using HarmonyLib;
using JSONClass;
using KBEngine;
using script.NpcAction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using YSGame;
//using static Fungus.StartFight;
using static GUIPackage.item;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.QingJiaoMoDao", "请教魔道功法神通", "1.0")]
    public class QingJiaoMoDao : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("请教魔道功法神通加载成功！");
            var harmony = new Harmony("Ventulus.MCS.QingJiaoMoDao");
            harmony.PatchAll();

        }
        public static QingJiaoMoDao Instance;

        private _ItemJsonData QingJiaoBook;
        void Awake()
        {
            Instance = this;
            QingJiaoBook = null;
        }
        private static Dictionary<int, int> FavorDict = new Dictionary<int, int>()
        {
            { 1, 5 },
            { 2, 6 },
            { 3, 8 }
        };
        [HarmonyPatch(typeof(UINPCQingJiao))]
        class UINPCQingJiao_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("GongFaSlotAction")]
            public static void GongFaSlotAction_Postfix(bool isShiFu, int qingJiaoType, int pinJie, JSONObject skill)
            {
                UINPCData npc = UINPCJiaoHu.Inst.NowJiaoHuNPC;
                Instance.Logger.LogInfo("请教功法");
                Instance.Logger.LogInfo(skill.ToString());
                //确保是请教魔道结束的
                if (qingJiaoType != 6 || isShiFu)
                    return;
                UINPCJiaoHu.Inst.IsQingJiaoShiBaiSW = false;
                if (npc.FavorLevel < FavorDict[pinJie])
                {
                    Tools.Say("桀桀桀，我们的感情还没这么好吧？", npc.ID);
                    return;
                }
                int qingFenCost = NPCEx.GetQingFenCost(skill, isGongFa: true);
                if (npc.QingFen < qingFenCost)
                {
                    Tools.Say("桀桀桀桀，我像是这么慷慨的人吗？", npc.ID);
                    //UINPCJiaoHu.Inst.IsQingJiaoShiBaiQF = true;
                    return;
                }

                if (npc.IsNingZhouNPC && PlayerEx.GetNingZhouShengWangLevel() > 1)
                {
                    Tools.Say("桀桀桀桀桀，魔道功法可不会教给宁州的好人。", npc.ID);
                    return;
                }
                if (!npc.IsNingZhouNPC && PlayerEx.GetSeaShengWangLevel() > 1)
                {
                    Tools.Say("桀桀桀桀桀，魔道功法可不会教给无尽之海的好人。", npc.ID);
                    return;
                }

                UINPCJiaoHu.Inst.HideNPCQingJiaoPanel();
                UINPCJiaoHu.Inst.HideJiaoHuPop();

                //记录请教的物品
                int SkillID = skill["Skill_ID"].I;
                Instance.QingJiaoBook = _ItemJsonData.DataList.FirstOrDefault(data => data.type == 4 && (int)float.Parse(data.desc) == SkillID);

                if (Instance.QingJiaoBook == null)
                {
                    Tools.Say("{punch=10,1}桀桀桀桀桀桀，这个功法我也不太懂，还是算了吧。", npc.ID);
                    return;
                }
                Tools.Say("{punch=10,1}桀桀桀桀桀桀，既然同是魔道中人，想必你已经做好准备了吧？", npc.ID);
                NPCEx.AddQingFen(npc.ID, -qingFenCost);
                Instance.Logger.LogInfo(Instance.QingJiaoBook.id + Instance.QingJiaoBook.name + Instance.QingJiaoBook.desc);

                //开启战斗
                int MonstarID = NPCEx.NPCIDToNew(npc.ID);
                Tools.instance.CanFpRun = 0;
                Tools.instance.monstarMag.FightType = StartFight.FightEnumType.DiFangTaoLi;
                Tools.instance.startFight(MonstarID);



            }

            [HarmonyPostfix]
            [HarmonyPatch("ShenTongSlotAction")]
            public static void ShenTongSlotAction_Postfix(bool isShiFu, int qingJiaoType, int pinJie, JSONObject skill)
            {
                UINPCData npc = UINPCJiaoHu.Inst.NowJiaoHuNPC;
                Instance.Logger.LogInfo("请教神通");
                Instance.Logger.LogInfo(skill.ToString());
                //确保是请教魔道结束的
                if (qingJiaoType != 6 || isShiFu)
                    return;
                UINPCJiaoHu.Inst.IsQingJiaoShiBaiSW = false;
                if (npc.FavorLevel < FavorDict[pinJie])
                {
                    Tools.Say("桀桀桀，我们的感情还没这么好吧？", npc.ID);
                    return;
                }
                int qingFenCost = NPCEx.GetQingFenCost(skill, isGongFa: false);
                if (npc.QingFen < qingFenCost)
                {
                    Tools.Say("桀桀桀桀，我像是这么慷慨的人吗？", npc.ID);
                    //UINPCJiaoHu.Inst.IsQingJiaoShiBaiQF = true;
                    return;
                }

                if (npc.IsNingZhouNPC && PlayerEx.GetNingZhouShengWangLevel() > 1)
                {
                    Tools.Say("桀桀桀桀桀，魔道神通可不会教给宁州的好人。", npc.ID);
                    return;
                }
                if (!npc.IsNingZhouNPC && PlayerEx.GetSeaShengWangLevel() > 1)
                {
                    Tools.Say("桀桀桀桀桀，魔道神通可不会教给无尽之海的好人。", npc.ID);
                    return;
                }
                UINPCJiaoHu.Inst.HideNPCQingJiaoPanel();
                UINPCJiaoHu.Inst.HideJiaoHuPop();

                //记录请教的物品
                int SkillID = skill["Skill_ID"].I;
                Instance.QingJiaoBook = _ItemJsonData.DataList.FirstOrDefault(data => data.type == 3 && (int)float.Parse(data.desc) == SkillID);

                if (Instance.QingJiaoBook == null)
                {
                    Tools.Say("{punch=10,1}桀桀桀桀桀桀，这个神通我也不太懂，还是算了吧。", npc.ID);
                    return;
                }
                Tools.Say("{punch=10,1}桀桀桀桀桀桀，既然同是魔道中人，想必你已经做好准备了吧？", npc.ID);
                NPCEx.AddQingFen(npc.ID, -qingFenCost);
                Instance.Logger.LogInfo(Instance.QingJiaoBook.id + Instance.QingJiaoBook.name + Instance.QingJiaoBook.desc);

                //开启战斗
                int MonstarID = NPCEx.NPCIDToNew(npc.ID);
                Tools.instance.CanFpRun = 0;
                Tools.instance.monstarMag.FightType = StartFight.FightEnumType.DiFangTaoLi;
                Tools.instance.startFight(MonstarID);
            }
        }
        [HarmonyPatch(typeof(Fight.FightResultMag))]
        class Fight_FightResultMag_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Fight.FightResultMag.VictoryClick))]
            public static void VictoryClick_Postfix()
            {
                if (Instance.QingJiaoBook != null)
                {
                    Tools.Say("{punch=10,2}桀桀桀桀桀桀桀~" + $"看来魔道后继有人啊！既然如此，便教你《{Instance.QingJiaoBook.name}》", Tools.instance.MonstarID);
                    PlayerEx.Player.addItem(Instance.QingJiaoBook.id, 1, null, ShowText: true);
                    Instance.Logger.LogInfo(Instance.QingJiaoBook.id + Instance.QingJiaoBook.name + Instance.QingJiaoBook.desc);
                    Instance.QingJiaoBook = null;
                }
            }
        }
    }
}
