﻿using Bag;
using BepInEx;
using BepInEx.Configuration;
using GUIPackage;
using HarmonyLib;
using JSONClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.CyContactOptimization", "传音符联系人优化", "1.0")]
    public class CyContactOptimization : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("传音符联系人优化加载成功！");
            var harmony = new Harmony("Ventulus.MCS.CyContactOptimization");
            harmony.PatchAll();

        }
        public static CyContactOptimization Instance;
        void Awake()
        {
            Instance = this;
        }
        [HarmonyPatch(typeof(CyFriendCell))]
        class CyFriendCell_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyFriendCell.Init))]
            public static void Init_Postfix(CyFriendCell __instance, int npcId)
            {
                //有标记的置顶
                if (__instance.isTag)
                {
                    __instance.transform.SetAsFirstSibling();
                }

                Transform tTagImage = __instance.transform.Find("TagImage");
                Transform tBg = __instance.transform.Find("Bg");
                Transform tName = tBg.Find("Name");
                (tTagImage as RectTransform).anchoredPosition = new Vector2(-170f, 0f);

                GameObject MakeNewBiaoQian(string name)
                {
                    //增加标签对象
                    GameObject goBiaoQian = UnityEngine.Object.Instantiate<GameObject>(tTagImage.gameObject, tTagImage.parent);
                    goBiaoQian.name = name;
                    goBiaoQian.GetComponent<UnityEngine.UI.Image>().sprite = CyUIMag.inst.npcList.npcCellSpriteList[3];
                    goBiaoQian.transform.SetAsFirstSibling();
                    RectTransform RectBiaoQian = goBiaoQian.transform as RectTransform;
                    RectBiaoQian.offsetMax = new Vector2(179f, 54.5f);
                    RectBiaoQian.offsetMin = new Vector2(-179f, -54.5f);
                    int newnum = __instance.transform.childCount - 2;
                    RectBiaoQian.anchoredPosition = new Vector2(-47 - (47 * newnum), 0);
                    goBiaoQian.SetActive(false);
                    //增加标签上的字
                    GameObject goBiaoQianText = UnityEngine.Object.Instantiate<GameObject>(tName.gameObject, goBiaoQian.transform);
                    goBiaoQianText.name = name + "Text";
                    (goBiaoQianText.transform as RectTransform).anchoredPosition = new Vector2(-83f, 12f);
                    UnityEngine.UI.Text BiaoQianText = goBiaoQianText.GetComponent<UnityEngine.UI.Text>();
                    BiaoQianText.fontSize = 30;
                    BiaoQianText.text = $"{name[0]}{Environment.NewLine}{name[1]}";
                    BiaoQianText.alignment = TextAnchor.UpperLeft;
                    BiaoQianText.verticalOverflow = VerticalWrapMode.Overflow;
                    return goBiaoQian;
                }

                //增加删除标签
                GameObject goShanChu = MakeNewBiaoQian("删除");
                goShanChu.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickShanChu(npcId); });


                if (NPCEx.NPCIDToNew(npcId) >= 20000)
                {
                    //增加查看标签
                    GameObject goChaKan = MakeNewBiaoQian("查看");
                    goChaKan.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickChaKan(npcId); });
                }

                //收取
                if (hasShouQuItem(npcId))
                {
                    GameObject goShouQu = MakeNewBiaoQian("收取");
                    goShouQu.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickShouQu(npcId); goShouQu.SetActive(false); });
                }
            }
            static UnityAction ClickChaKan(int npcId)
            {

                int id = NPCEx.NPCIDToNew(npcId);
                Instance.Logger.LogInfo("查看按钮被点击了" + npcId + "皮套人" + id);
                if (id < 20000) return null;
                UINPCData npc = new UINPCData(id);
                npc.RefreshData();

                CyUIMag.inst.Close();
                UINPCJiaoHu.Inst.NowJiaoHuNPC = npc;
                UINPCJiaoHu.Inst.ShowNPCInfoPanel(npc);

                return null;
            }

            static UnityAction ClickShanChu(int npcId)
            {

                int id = NPCEx.NPCIDToNew(npcId);
                Instance.Logger.LogInfo("删除按钮被点击了" + npcId + "皮套人" + id);

                UINPCData npc = new UINPCData(id);
                npc.RefreshData();
                USelectBox.Show($"确认要删除联系人{npc.Name}吗？ ", delegate
                {
                    CyUIMag.inst.npcList.friendList.Remove(npcId);
                    CyUIMag.inst.npcList.Init();
                }, null);
                return null;
            }

            static UnityAction ClickShouQu(int npcId)
            {

                int id = NPCEx.NPCIDToNew(npcId);
                Instance.Logger.LogInfo("收取按钮被点击了" + npcId + "皮套人" + id);


                Dictionary<string, List<EmailData>> newEmailDictionary = PlayerEx.Player.emailDateMag.newEmailDictionary;
                Dictionary<string, List<EmailData>> hasReadEmailDictionary = PlayerEx.Player.emailDateMag.hasReadEmailDictionary;

                if (newEmailDictionary.Count > 0 && newEmailDictionary.ContainsKey(npcId.ToString()))
                {
                    List<EmailData> emails = newEmailDictionary[npcId.ToString()];
                    foreach (EmailData emailData in emails)
                    {
                        //actionId=1是赠送给玩家，2是索取
                        if (emailData.actionId == 1)
                        {
                            if (emailData.item[1] > 0)
                            {
                                Tools.instance.getPlayer().addItem(emailData.item[0], emailData.item[1], Tools.CreateItemSeid(emailData.item[0]), true);
                                Instance.Logger.LogInfo($"收取了物品{emailData.item[0]}共{emailData.item[1]}个");
                                emailData.item[1] = -1;
                            }
                        }
                        else if (emailData.actionId == 2)
                        {
                        }
                    }
                }
                if (hasReadEmailDictionary.Count > 0 && hasReadEmailDictionary.ContainsKey(npcId.ToString()))
                {
                    List<EmailData> emails = hasReadEmailDictionary[npcId.ToString()];
                    foreach (EmailData emailData in emails)
                    {
                        //actionId=1是赠送给玩家，2是索取
                        if (emailData.actionId == 1)
                        {
                            if (emailData.item[1] > 0)
                            {
                                Tools.instance.getPlayer().addItem(emailData.item[0], emailData.item[1], Tools.CreateItemSeid(emailData.item[0]), true);
                                Instance.Logger.LogInfo($"收取了物品{emailData.item[0]}共{emailData.item[1]}个");
                                emailData.item[1] = -1;
                            }
                        }
                        else if (emailData.actionId == 2)
                        {
                        }
                    }
                }
                CyUIMag.inst.cyEmail.cySendBtn.Hide();
                CyUIMag.inst.cyEmail.Init(npcId);
                return null;
            }
            static bool hasShouQuItem(int npcId)
            {
                bool has = false;
                Dictionary<string, List<EmailData>> newEmailDictionary = PlayerEx.Player.emailDateMag.newEmailDictionary;
                Dictionary<string, List<EmailData>> hasReadEmailDictionary = PlayerEx.Player.emailDateMag.hasReadEmailDictionary;

                if (newEmailDictionary.Count > 0 && newEmailDictionary.ContainsKey(npcId.ToString()))
                {
                    List<EmailData> emails = newEmailDictionary[npcId.ToString()];
                    if (emails.FirstOrDefault(emailData => emailData.actionId == 1 && emailData.item[1] > 0) != null)
                        has = true;
                }
                if (hasReadEmailDictionary.Count > 0 && hasReadEmailDictionary.ContainsKey(npcId.ToString()))
                {
                    List<EmailData> emails = hasReadEmailDictionary[npcId.ToString()];
                    if (emails.FirstOrDefault(emailData => emailData.actionId == 1 && emailData.item[1] > 0) != null)
                        has = true;
                }
                return has;
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyFriendCell.updateState))]
            public static void updateState_Postfix(CyFriendCell __instance)
            {

                Transform tChaKan = __instance.transform.Find("查看");
                Transform tShanChu = __instance.transform.Find("删除");
                Transform tShouQu = __instance.transform.Find("收取");
                if (__instance.isSelect)
                {
                    if (tChaKan && NPCEx.NPCIDToNew(__instance.npcId) >= 20000) tChaKan.gameObject.SetActive(true);
                    if (tShanChu) tShanChu.gameObject.SetActive(true);
                    if (tShouQu && hasShouQuItem(__instance.npcId)) tShouQu.gameObject.SetActive(true);
                }
                else
                {
                    if (tChaKan) tChaKan.gameObject.SetActive(false);
                    if (tShanChu) tShanChu.gameObject.SetActive(false);
                    if (tShouQu) tShouQu.gameObject.SetActive(false);
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyFriendCell.ClickTag))]
            public static void ClickTag_Postfix(CyFriendCell __instance)
            {
                //有标记的置顶
                if (__instance.isTag)
                {
                    __instance.transform.SetAsFirstSibling();
                }
            }
        }
    }
}