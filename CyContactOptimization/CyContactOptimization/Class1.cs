﻿using Bag;
using BepInEx;
using BepInEx.Configuration;
using GUIPackage;
using HarmonyLib;
using JSONClass;
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
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.CyContactOptimization", "传音符联系人优化", "1.4")]
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
                Transform tTagImage = __instance.transform.Find("TagImage");
                Transform tBg = __instance.transform.Find("Bg");
                Transform tName = tBg.Find("Name");
                //调整官方标记的位置
                tTagImage.localPosition = new Vector3(-170f, -54.5f, 0f);

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
                    //int newnum = __instance.transform.childCount - 2;
                    RectBiaoQian.anchoredPosition = new Vector2(-47, 0);
                    //默认都不显示
                    goBiaoQian.SetActive(false);
                    //增加标签上的字
                    GameObject goBiaoQianText = UnityEngine.Object.Instantiate<GameObject>(tName.gameObject, goBiaoQian.transform);
                    goBiaoQianText.name = name + "Text";
                    goBiaoQianText.transform.localPosition = new Vector3(-83f, 12f, 0);
                    UnityEngine.UI.Text BiaoQianText = goBiaoQianText.GetComponent<UnityEngine.UI.Text>();
                    BiaoQianText.fontSize = 30;
                    BiaoQianText.text = $"{name[0]}{Environment.NewLine}{name[1]}";
                    BiaoQianText.alignment = TextAnchor.UpperLeft;
                    BiaoQianText.verticalOverflow = VerticalWrapMode.Overflow;

                    return goBiaoQian;
                }

                //增加删除标签
                GameObject goShanChu = MakeNewBiaoQian("删除");
                goShanChu.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickShanChu(npcId, tName.GetComponent<Text>().text); });


                //增加查看标签
                GameObject goChaKan = MakeNewBiaoQian("查看");
                goChaKan.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickChaKan(npcId); });
                //工具人是否能查看的开关
                //if (NPCEx.NPCIDToNew(npcId) >= 20000) goChaKan.SetActive(false);
                if (!__instance.isDeath && !__instance.IsFly)
                    goChaKan.SetActive(false);



                //收取
                GameObject goShouQu = MakeNewBiaoQian("收取");
                goShouQu.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickShouQu(npcId); goShouQu.SetActive(false); ArrangeLabelPositions(__instance.transform); });
                //不选中也显示收取按钮
                //goShouQu.SetActive(hasShouQuItem(npcId));


                //排序
                //由于新邮件人是先初始化cell再放红点，所以这里不合适排序
                //SortNpcCells(__instance.transform.parent);
                //标签排位置
                ArrangeLabelPositions(__instance.transform);
            }
            static void ArrangeLabelPositions(Transform parent)
            {
                if (parent.childCount <= 2) return;
                //后加的标签序号小且在下面
                for (int i = parent.childCount - 3; i >= 0; i--)
                {
                    Transform tLable = parent.GetChild(i);
                    int activenum = 0;
                    for (int j = parent.childCount - 2; j >= i; j--)
                    {
                        if (parent.GetChild(j).gameObject.activeSelf)
                            activenum++;
                    }
                    tLable.localPosition = new Vector3(-47f * activenum, -54.5f, 0);
                }
            }
            static UnityAction ClickChaKan(int npcId)
            {

                int id = NPCEx.NPCIDToNew(npcId);
                Instance.Logger.LogInfo("查看按钮被点击了" + (npcId < 20000 ? "原型" : "") + npcId + (id > npcId ? $"皮套人{id}" : ""));
                UINPCData npc = new UINPCData(id);
                if (id < 20000)
                {
                    npc.RefreshOldNpcData();
                    npc.IsFight = true;

                    UINPCData npc609 = new UINPCData(609);
                    npc609.RefreshData();
                    UINPCJiaoHu.Inst.NowJiaoHuNPC = npc609;

                    UINPCJiaoHu.Inst.NowJiaoHuEnemy = npc;
                    UINPCJiaoHu.Inst.InfoPanel.npc = npc;
                    CyUIMag.inst.Close();
                    UINPCJiaoHu.Inst.ShowNPCInfoPanel(UINPCJiaoHu.Inst.NowJiaoHuEnemy);
                    UINPCJiaoHu.Inst.InfoPanel.TabGroup.HideTab();
                }
                else
                {
                    npc.RefreshData();
                    npc.IsFight = false;
                    UINPCJiaoHu.Inst.NowJiaoHuNPC = npc;
                    CyUIMag.inst.Close();
                    UINPCJiaoHu.Inst.ShowNPCInfoPanel(npc);
                    UINPCJiaoHu.Inst.InfoPanel.TabGroup.UnHideTab();
                }



                //PanelMamager.CanOpenOrClose=true;


                return null;
            }

            static UnityAction ClickShanChu(int npcId, string name)
            {

                int id = NPCEx.NPCIDToNew(npcId);
                Instance.Logger.LogInfo("删除按钮被点击了" + npcId + "皮套人" + id);
                //由于可能会是死人，在jsonData.instance.AvatarRandomJsonData里没有信息

                USelectBox.Show($"确认要删除联系人{name}吗？ ", delegate
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
                if (tShanChu) tShanChu.gameObject.SetActive(__instance.isSelect);
                if (tChaKan) tChaKan.gameObject.SetActive(__instance.isSelect && !__instance.isDeath && !__instance.IsFly);
                if (tShouQu) tShouQu.gameObject.SetActive(__instance.isSelect && hasShouQuItem(__instance.npcId));

                ArrangeLabelPositions(__instance.transform);
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyFriendCell.ClickTag))]
            public static void ClickTag_Postfix(CyFriendCell __instance)
            {
                //有标记的置顶
                SortNpcCells(__instance.transform.parent);

            }

        }
        static void SortNpcCells(Transform tContent)
        {
            int count = tContent.childCount;
            Instance.Logger.LogInfo("排序cell共" + count);
            if (count == 0) return;
            int index = 0;
            for (int i = 0; i < count; i++)
            {
                CyFriendCell cell = tContent.GetChild(index).GetComponent<CyFriendCell>();
                if (cell.isTag && cell.redDian.activeSelf)
                {
                    cell.transform.SetAsLastSibling();
                    index--;
                }
                index++;
            }
            index = 0;
            for (int i = 0; i < count; i++)
            {
                CyFriendCell cell = tContent.GetChild(index).GetComponent<CyFriendCell>();
                if (!cell.isTag && cell.redDian.activeSelf)
                {
                    cell.transform.SetAsLastSibling();
                    index--;
                }
                index++;
            }
            index = 0;
            for (int i = 0; i < count; i++)
            {
                CyFriendCell cell = tContent.GetChild(index).GetComponent<CyFriendCell>();
                if (cell.isTag && !cell.redDian.activeSelf)
                {
                    cell.transform.SetAsLastSibling();
                    index--;
                }
                index++;
            }
            index = 0;
            for (int i = 0; i < count; i++)
            {
                CyFriendCell cell = tContent.GetChild(index).GetComponent<CyFriendCell>();
                if (!cell.isTag && !cell.redDian.activeSelf)
                {
                    cell.transform.SetAsLastSibling();
                    index--;
                }
                index++;
            }
        }
        [HarmonyPatch(typeof(CyNpcList))]
        class CyNpcList_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyNpcList.Init))]
            public static void Init_Postfix(CyNpcList __instance)
            {
                var t = __instance.transform.Find("List/Viewport/Content");
                SortNpcCells(t);
            }

            [HarmonyPrefix]
            [HarmonyPatch("InitNpcList")]
            public static void InitNpcList_Prefix(CyNpcList __instance)
            {
                __instance.friendList.Sort(new friendListComparer());
            }
        }
        //联系人比较器，返回负数为较小在前，正数为较大在后
        public class friendListComparer : Comparer<int>
        {
            public override int Compare(int x, int y)
            {


                //红点优先
                Dictionary<string, List<EmailData>> newEmailDict = Tools.instance.getPlayer().emailDateMag.newEmailDictionary;
                List<int> TagNpcList = Tools.instance.getPlayer().emailDateMag.TagNpcList;
                if (newEmailDict.ContainsKey(x.ToString()) && !newEmailDict.ContainsKey(x.ToString()))
                {
                    return -1;
                }
                else if (!newEmailDict.ContainsKey(x.ToString()) && newEmailDict.ContainsKey(x.ToString()))
                {
                    return 1;
                }
                //死亡和飞升的不管之前有没有标记，都先丢下去
                if (NpcJieSuanManager.inst.IsDeath(x) && !NpcJieSuanManager.inst.IsDeath(y))
                {
                    return 1;
                }
                else if (!NpcJieSuanManager.inst.IsDeath(x) && NpcJieSuanManager.inst.IsDeath(y))
                {
                    return -1;
                }

                if (NpcJieSuanManager.inst.IsFly(x) && !NpcJieSuanManager.inst.IsFly(y))
                {
                    return 1;
                }
                else if (!NpcJieSuanManager.inst.IsFly(x) && NpcJieSuanManager.inst.IsFly(y))
                {
                    return -1;
                }
                //标记优先
                if (TagNpcList.Contains(x) && !TagNpcList.Contains(y))
                {
                    return -1;
                }
                else if (!TagNpcList.Contains(x) && TagNpcList.Contains(y))
                {
                    return 1;
                }
                //好感高的优先
                UINPCData npcx = new UINPCData(x);
                if (NPCEx.NPCIDToNew(x) >= 20000)
                {
                    npcx.RefreshData();
                }
                else
                {
                    npcx.RefreshOldNpcData();
                }
                UINPCData npcy = new UINPCData(y);
                if (NPCEx.NPCIDToNew(y) >= 20000)
                {
                    npcy.RefreshData();
                }
                else
                {
                    npcy.RefreshOldNpcData();
                }
                return npcy.Favor - npcx.Favor;


                return default;

            }
        }

    }
}
