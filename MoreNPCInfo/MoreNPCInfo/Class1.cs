using BepInEx;
using BepInEx.Configuration;
using Fungus;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UINPCQingJiaoSkillData;
using static UltimateSurvival.ItemProperty;


namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.MoreNPCInfo", "MoreNPCInfo", "1.0")]
    public class MoreNPCInfo : BaseUnityPlugin
    {
        void Start()
        {
            //输出日志
            Logger.LogInfo("更多NPC信息加载成功！");
            var harmony = new Harmony("Ventulus.MCS.MoreNPCInfo");
            harmony.PatchAll();

            MessageMag.Instance.Register(MessageName.MSG_GameInitFinish, new Action<MessageData>(this.Init));

           
        }

        public static MoreNPCInfo Instance;
        void Awake()
        {
            Instance = this;
        }

        void Init(MessageData data)
        {
            foreach (JSONObject jsonobject in jsonData.instance.NpcHaoGanDuData.list)
            {
                favorQuJianList.Add(jsonobject["QuJian"].list[0].I);
                favorStrList.Add(jsonobject["HaoGanDu"].Str);
            }
        }
        // Token: 0x0400112F RID: 4399
        private static List<string> favorStrList = new List<string>();

        // Token: 0x04001130 RID: 4400
        private static List<int> favorQuJianList = new List<int>();

        [HarmonyPatch(typeof(UINPCJiaoHu))]
        class UINPCJiaoHuPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ShowNPCInfoPanel")]
            public static void ShowNPCInfoPanelPostfix(UINPCData npc)
            {
                Instance.Logger.LogInfo("ShowNPCInfoPanel");
                if (npc == null) 
                { 
                    npc = UINPCJiaoHu.Inst.NowJiaoHuNPC;
                }
                //UINPCData NPCData = UINPCJiaoHu.Inst.NowJiaoHuNPC;
                Instance.Logger.LogInfo(npc.json.ToString());
                UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
                Transform tShuXing = NPCInfoPanel.transform.Find("ShuXing");

                //称号
                Transform tChengHao = NPCInfoPanel.transform.Find("NPCShow/ChengHao");
                Transform tName = NPCInfoPanel.transform.Find("NPCShow/Name");
  
                if (tChengHao == null)
                {
                    tChengHao = UnityEngine.Object.Instantiate<GameObject>(tName.gameObject, tName.parent).transform;
                    tChengHao.gameObject.name = "ChengHao";
                    //原姓名下移
                    tName.localPosition = new Vector3(0, -287.4f, 0);
                }
                tChengHao.Find("Text").GetComponent<Text>().text = npc.Title;
                //NPCInfoPanel.NPCName.text = npc.Title + " " + npc.Name;

                //腾地方
                Transform tShuXingTitle = tShuXing.Find("Title");
                tShuXingTitle.gameObject.SetActive(false);

                //年龄
                Transform tNianLing = tShuXing.Find("NianLing");
                tNianLing.localPosition = new Vector3(-120, 100, 0);
                tNianLing.Find("Text").GetComponent<Text>().text = npc.Age.ToString() + "/" + npc.ShouYuan.ToString();

                //Id
                Transform tId = tShuXing.Find("Id");
                if (tId == null)
                {
                    tId = UnityEngine.Object.Instantiate<GameObject>(tNianLing.gameObject, tNianLing.parent).transform;
                    tId.name = "Id";
                    tId.localPosition = new Vector3(-120, 150, 0);
                }
                tId.Find("Icon").gameObject.SetActive(false);
                tId.Find("Title").GetComponent<Text>().text = "Id:" +  npc.ID.ToString();
                tId.Find("Text").gameObject.SetActive(false);

                //行动
                Transform tAction = tShuXing.Find("Action");
                if (tAction == null)
                {
                    tAction = UnityEngine.Object.Instantiate<GameObject>(tNianLing.gameObject, tNianLing.parent).transform;
                    tAction.name = "Action";
                    tAction.localPosition = new Vector3(130, 150, 0);
                }
                tAction.Find("Icon").gameObject.SetActive(false);
                tAction.Find("Title").GetComponent<Text>().text = "行动:" + npc.ActionID.ToString();
                tAction.Find("Text").gameObject.SetActive(false);


                //好感
                Transform tFavor = tShuXing.Find("QingFen");

                int FavorLevel = 1;
                while (FavorLevel < favorQuJianList.Count && npc.Favor >= favorQuJianList[FavorLevel])
                {
                    FavorLevel++;
                }

                tFavor.Find("Text").GetComponent<Text>().text = npc.Favor.ToString() + favorStrList[FavorLevel-1];
            }
        }

    }
}
