using Bag;
using BepInEx;
using BepInEx.Configuration;
using GUIPackage;
using HarmonyLib;
using JSONClass;
//using KBEngine;
using script.NpcAction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YSGame;

namespace Ventulus
{
    [BepInDependency("Ventulus.MCS.VTools", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Ventulus.MCS.CyContactOptimization", "传音符联系人优化", "2.0.0")]
    public class CyContactOptimization : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            new Harmony("Ventulus.MCS.CyContactOptimization").PatchAll();
            Logger.LogInfo("加载成功");
        }
        public static CyContactOptimization Instance;
        private int CyOpenInfoPanel;
        private int CyOpenZengLi;
        private float ScrollPosition = 1;

        private static Dictionary<int, string> NPCdeathType = new Dictionary<int, string>()
        {
            {1,"寿元已尽"},
            {2,"被玩家打死"},
            {3,"游历时意外身亡"},
            {4,"被妖兽反杀"},
            {5,"被其它修士截杀"},
            {6,"做宗门任务死了"},
            {7,"做主城任务死了"},
            {8,"炼丹被炸死"},
            {9,"炼器被炸死"},
            {10,"不明原因死亡"},
            {11,"截杀时被反杀"},
            {12,"飞升失败"},
        };

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
                goShanChu.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickShanChu(npcId); });


                //增加查看标签
                GameObject goChaKan = MakeNewBiaoQian("查看");
                goChaKan.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickChaKan(npcId); });


                //收取
                GameObject goShouQu = MakeNewBiaoQian("收取");
                goShouQu.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickShouQu(npcId); goShouQu.SetActive(false); ArrangeLabelPositions(__instance.transform); });
                //不选中也显示收取按钮
                //goShouQu.SetActive(hasShouQuItem(npcId));

                //增加死因标签
                GameObject goSiYin = MakeNewBiaoQian("死因");
                goSiYin.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickSiYin(npcId); CyUIMag.inst.cyEmail.Init(npcId); __instance.Click(); });

                //增加飞剑标签
                GameObject goFeiJian = MakeNewBiaoQian("飞剑");
                goFeiJian.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickFeiJian(npcId); });


                //由于新邮件人是先初始化cell再放红点，所以这里不合适排序

                //标签排位置
                ArrangeLabelPositions(__instance.transform);

                //死亡类型
                if (NpcJieSuanManager.inst.IsDeath(npcId))
                {
                    JSONObject npcDeath = NpcJieSuanManager.inst.npcDeath.npcDeathJson[npcId.ToString()];
                    Instance.Logger.LogInfo(npcDeath.ToString());
                }

                //红点放大
                bool BigRedDian = false;
                Dictionary<string, List<EmailData>> newEmailDictionary = PlayerEx.Player.emailDateMag.newEmailDictionary;
                if (newEmailDictionary.Count > 0 && newEmailDictionary.ContainsKey(npcId.ToString()))
                {
                    List<EmailData> emails = newEmailDictionary[npcId.ToString()];
                    foreach (EmailData emailData in emails)
                    {
                        if (emailData.isOld && npcId != 912 || emailData.actionId == 2)
                        {
                            BigRedDian = true;
                            break;
                        }
                    }
                }
                __instance.redDian.transform.localScale = BigRedDian ? new Vector3(2, 2, 2) : new Vector3(1, 1, 1);

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
                if (NpcJieSuanManager.inst.IsDeath(npcId) || NpcJieSuanManager.inst.IsFly(npcId))
                    return null;

                int id = NPCEx.NPCIDToNew(npcId);
                Instance.Logger.LogInfo("查看按钮被点击了" + VTools.MakeNPCIdStr(npcId));
                UINPCData npc = new UINPCData(id);
                if (id < 20000)
                {
                    npc.RefreshOldNpcData();
                    npc.IsFight = true;

                    UINPCJiaoHu.Inst.NowJiaoHuEnemy = npc;
                    UINPCJiaoHu.Inst.InfoPanel.npc = npc;
                    Instance.ScrollPosition = CyUIMag.inst.npcList.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition;
                    CyUIMag.inst.Close();
                    UINPCJiaoHu.Inst.ShowNPCInfoPanel(UINPCJiaoHu.Inst.NowJiaoHuEnemy);
                    UINPCJiaoHu.Inst.InfoPanel.TabGroup.HideTab();
                }
                else
                {
                    npc.RefreshData();
                    npc.IsFight = false;
                    UINPCJiaoHu.Inst.NowJiaoHuNPC = npc;
                    Instance.ScrollPosition = CyUIMag.inst.npcList.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition;
                    CyUIMag.inst.Close();
                    UINPCJiaoHu.Inst.ShowNPCInfoPanel(npc);
                    UINPCJiaoHu.Inst.InfoPanel.TabGroup.UnHideTab();
                }

                Instance.CyOpenInfoPanel = npcId;

                Instance.Logger.LogInfo(Instance.CyOpenInfoPanel);
                return null;
            }
            static UnityAction ClickFeiJian(int npcId)
            {
                if (NpcJieSuanManager.inst.IsDeath(npcId) || NpcJieSuanManager.inst.IsFly(npcId))
                    return null;
                Instance.Logger.LogInfo("飞剑按钮被点击了" + VTools.MakeNPCIdStr(npcId));
                int id = NPCEx.NPCIDToNew(npcId);
                UINPCData npc = new UINPCData(id);
                if (id < 20000)
                {
                    Instance.Logger.LogMessage("工具人NPC无法飞剑传书");
                    return null;
                }
                else
                {
                    npc.RefreshData();
                    npc.IsFight = false;
                    UINPCJiaoHu.Inst.NowJiaoHuNPC = npc;
                    Instance.ScrollPosition = CyUIMag.inst.npcList.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition;
                    CyUIMag.inst.Close();
                    UINPCJiaoHu.Inst.ShowNPCZengLi();
                }

                Instance.CyOpenZengLi = npcId;
                Instance.Logger.LogInfo(Instance.CyOpenZengLi);
                return null;
            }

            static UnityAction ClickShanChu(int npcId)
            {
                Instance.Logger.LogInfo("删除按钮被点击了" + VTools.MakeNPCIdStr(npcId));
                string name = VTools.GetNPCName(npcId);

                USelectBox.Show($"确认要删除联系人{name}吗？ ", delegate
                {
                    CyUIMag.inst.npcList.friendList.Remove(npcId);
                    CyUIMag.inst.npcList.Init();
                }, null);
                return null;
            }
            static UnityAction ClickSiYin(int npcId)
            {

                Instance.Logger.LogInfo("死因按钮被点击了" + VTools.MakeNPCIdStr(npcId));
                JSONObject npcDeath = NpcJieSuanManager.inst.npcDeath.npcDeathJson[npcId.ToString()];
                string DeathDesc = "唉…斯人已逝，幽思长存。且待我推算一番……";
                DeathDesc += $"{Environment.NewLine}死者姓名：{VTools.GetNPCName(npcId)}";
                if (npcDeath.HasField("deathTime"))
                {
                    //死亡记录时间为下次月结时间，月结时间正常为下个月1号
                    DateTime deathTime = DateTime.Parse(npcDeath.GetField("deathTime").str).AddMonths(-1);
                    DeathDesc += $"{Environment.NewLine}死亡时间：{deathTime.Year}年{deathTime.Month}月";
                }
                if (npcDeath.HasField("deathType"))
                {
                    int deathType = npcDeath.GetInt("deathType");

                    DeathDesc += $"{Environment.NewLine}死因：{NPCdeathType[deathType]?.Replace("玩家", Tools.GetPlayerName()) ?? "未知"}";
                }
                if (npcDeath.HasField("killNpcId"))
                {
                    int killNpcId = npcDeath.GetInt("killNpcId");
                    string killNpcName = VTools.GetNPCName(killNpcId);
                    DeathDesc += $"{Environment.NewLine}凶手：{killNpcName}";
                }

                VTools.SendOldEmail(npcId, 2, DeathDesc);
                return null;
            }

            static UnityAction ClickShouQu(int npcId)
            {
                Instance.Logger.LogInfo("收取按钮被点击了" + VTools.MakeNPCIdStr(npcId));

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
                //如果是原型工具人寄的信，则信字典里只有EmailData(npcId, isOld: true, oldId, sendTime)，信具体内容另在他处
                //天机阁交易人固定id912，信的oldid是2082912 + 交换序号PlayerEx.Player.ExchangeMeetingID，
                if (npcId == 912 && Tools.instance.getPlayer().NewChuanYingList.Count > 0)
                {
                    foreach (JSONObject mail in Tools.instance.getPlayer().NewChuanYingList.list)
                    {
                        if (mail.HasField("AvatarID") && mail["AvatarID"].I == 912 && mail.HasField("ItemID") && mail["ItemID"].I > 0 && mail["ItemHasGet"].b == false)
                        {
                            Tools.instance.getPlayer().addItem(mail["ItemID"].I, 1, null, ShowText: true);
                            mail.SetField("ItemHasGet", val: true);
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
                if (npcId == 912 && Tools.instance.getPlayer().NewChuanYingList.Count > 0)
                {
                    foreach (JSONObject mail in Tools.instance.getPlayer().NewChuanYingList.list)
                    {
                        if (mail.HasField("AvatarID") && mail["AvatarID"].I == 912 && mail.HasField("ItemID") && mail["ItemID"].I > 0 && mail["ItemHasGet"].b == false)
                        {
                            has = true;
                            break;
                        }
                    }
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
                Transform tSiYin = __instance.transform.Find("死因");
                Transform tFeiJian = __instance.transform.Find("飞剑");
                if (tShanChu) tShanChu.gameObject.SetActive(__instance.isSelect);
                if (tChaKan) tChaKan.gameObject.SetActive(__instance.isSelect && !__instance.isDeath && !__instance.IsFly);
                if (tShouQu) tShouQu.gameObject.SetActive(__instance.isSelect && hasShouQuItem(__instance.npcId));
                if (tSiYin) tSiYin.gameObject.SetActive(__instance.isSelect && __instance.isDeath);
                if (tFeiJian) tFeiJian.gameObject.SetActive(__instance.isSelect && !__instance.isDeath && !__instance.IsFly && __instance.npcId >= 20000);
                //工具人是否能查看的开关
                //if (tChaKan) tChaKan.gameObject.SetActive(__instance.isSelect && !__instance.isDeath && !__instance.IsFly && NPCEx.NPCIDToNew(__instance.npcId) >= 20000);

                //标签位置整理
                ArrangeLabelPositions(__instance.transform);
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyFriendCell.ClickTag))]
            public static void ClickTag_Postfix(CyFriendCell __instance)
            {
                //重选排序
                List<CyFriendCell> friendCells = CyUIMag.inst.npcList.friendCells;
                friendCells.Sort(new CyFriendCellComparer());
                for (int i = 0; i < friendCells.Count; i++)
                {
                    friendCells[i].transform.SetSiblingIndex(i);
                }

                //移动滚动条
                if (__instance.isTag)
                    CellScrollRect(1);

            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyFriendCell.Click))]
            public static void Click_Postfix(CyFriendCell __instance)
            {
                Instance.Logger.LogMessage("Click" + __instance.npcId);

            }

        }
        static void CellScrollRect(float position = 2)
        {
            CyNpcList NpcList = CyUIMag.inst.npcList;

            if (position > 1 && NpcList != null && NpcList.curSelectFriend != null)
            {
                int count = 0;
                int num = NpcList.friendCells.Count;
                foreach (CyFriendCell cell in NpcList.friendCells)
                {
                    if (cell.npcId == NpcList.curSelectFriend.npcId)
                        break;
                    count++;
                }
                position = 1 - (float)count / num;
            }


            Instance.Logger.LogInfo(position.ToString());
            ScrollRect ScrollRect = NpcList.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
            ScrollRect.verticalNormalizedPosition = position;

        }

        [HarmonyPatch(typeof(CyNpcList))]
        class CyNpcList_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyNpcList.Init))]
            public static void Init_Postfix(CyNpcList __instance)
            {

                List<CyFriendCell> friendCells = __instance.friendCells;
                friendCells.Sort(new CyFriendCellComparer());
                for (int i = 0; i < friendCells.Count; i++)
                {
                    friendCells[i].transform.SetSiblingIndex(i);
                }

                //尝试选中刚才的npc
                if (Instance.CyOpenInfoPanel > 0 || Instance.CyOpenZengLi > 0)
                {
                    foreach (CyFriendCell cell in __instance.friendCells)
                    {
                        if (cell.npcId == Instance.CyOpenInfoPanel || cell.npcId == Instance.CyOpenZengLi)
                        {
                            Instance.Logger.LogInfo("找到要选中的npccell" + Instance.CyOpenInfoPanel + Instance.CyOpenZengLi);
                            //CyUIMag.inst.cyEmail.Init(cell.npcId);
                            Instance.StartCoroutine(Instance.SelectCell(cell));
                            break;
                        }
                    }
                }
                //无论是否重选人，都恢复
                Instance.CyOpenInfoPanel = -1;
                Instance.CyOpenZengLi = -1;
            }

        }
        //延后一帧再选中，并滚动
        IEnumerator SelectCell(CyFriendCell cell)
        {
            yield return 1;
            cell.Click();
            yield return null;
            //移动滚动条
            CellScrollRect(Instance.ScrollPosition);
        }

        //联系人比较器，返回负数为较小在前，正数为较大在后

        public class CyFriendCellComparer : Comparer<CyFriendCell>
        {
            public override int Compare(CyFriendCell A, CyFriendCell B)
            {
                //红点优先
                if (A.redDian.activeSelf && !B.redDian.activeSelf)
                {
                    return -1;
                }
                else if (!A.redDian.activeSelf && B.redDian.activeSelf)
                {
                    return 1;
                }
                if (A.isTag && !B.isTag)
                {
                    return -1;
                }
                //死亡和飞升的不管之前有没有标记，都先丢下去
                if (A.isDeath && !B.isDeath)
                {
                    return 1;
                }
                else if (!A.isDeath && B.isDeath)
                {
                    return -1;
                }

                if (A.IsFly && !B.IsFly)
                {
                    return 1;
                }
                else if (!A.IsFly && B.IsFly)
                {
                    return -1;
                }

                //标记优先
                if (A.isTag && !B.isTag)
                {
                    return -1;
                }
                else if (!A.isTag && B.isTag)
                {
                    return 1;
                }

                //同时死亡飞升的得排除
                if (A.isDeath || B.isDeath || A.IsFly || B.IsFly)
                    return 0;
                else
                {
                    UINPCData npcx = new UINPCData(A.npcId);
                    if (NPCEx.NPCIDToNew(A.npcId) >= 20000)
                        npcx.RefreshData();
                    else
                        npcx.RefreshOldNpcData();

                    UINPCData npcy = new UINPCData(B.npcId);
                    if (NPCEx.NPCIDToNew(B.npcId) >= 20000)
                        npcy.RefreshData();
                    else
                        npcy.RefreshOldNpcData();

                    if (npcy.Favor != npcx.Favor)
                        return npcy.Favor - npcx.Favor;
                }
                //最后按id排序避免乱跑
                return A.npcId - B.npcId;
            }

        }
        [HarmonyPatch(typeof(UINPCJiaoHu))]
        class UINPCJiaoHu_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(UINPCJiaoHu.HideNPCInfoPanel))]
            public static void HideNPCInfoPanel_Postfix()
            {
                Instance.Logger.LogInfo("关闭NPC信息面板");
                Instance.Logger.LogInfo(Instance.CyOpenInfoPanel);
                if (Instance.CyOpenInfoPanel > 0)
                {
                    //重新打开传音面板，比较绿皮
                    Instance.Logger.LogInfo("重新打开传音符");
                    Instance.Logger.LogInfo(Instance.CyOpenInfoPanel);
                    PanelMamager.inst.OpenPanel(PanelMamager.PanelType.传音符, 1);

                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(UINPCJiaoHu.HideNPCZengLi))]
            public static void HideNPCZengLi_Postfix()
            {
                Instance.Logger.LogInfo("关闭NPC赠礼");
                Instance.Logger.LogInfo(Instance.CyOpenZengLi);
                if (Instance.CyOpenZengLi > 0)
                {
                    //重新打开传音面板，比较绿皮
                    Instance.Logger.LogInfo("重新打开传音符");
                    Instance.Logger.LogInfo(Instance.CyOpenZengLi);
                    PanelMamager.inst.OpenPanel(PanelMamager.PanelType.传音符, 1);

                }
            }
        }
        [HarmonyPatch(typeof(UINPCZengLi))]
        class UINPCZengLi_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(UINPCZengLi.OnOKBtnClick))]
            public static bool OnOKBtnClick_Prefix()
            {
                if (Instance.CyOpenZengLi > 0)
                {
                    //拦截原版赠礼
                    Instance.Logger.LogInfo("拦截赠礼确定");
                    Instance.Logger.LogInfo(Instance.CyOpenZengLi);
                    UIIconShow ZengLiSlot = UINPCJiaoHu.Inst.ZengLi.ZengLiSlot;
                    string npcsay = "（只见天边的一道剑光须臾间就到了眼前，其上还有一个包裹，此刻破空声才堪堪传来）";
                    if (ZengLiSlot.NowType != 0)
                    {
                        int itemId = ZengLiSlot.tmpItem.itemID;
                        int itemNum = ZengLiSlot.Count;
                        //计算运费
                        int addCount = 10000;
                        if (jsonData.instance.ItemJsonData.HasField(itemId.ToString()))
                            addCount = jsonData.instance.ItemJsonData[itemId.ToString()]["price"].I * itemNum;
                        int Shipping = 2000 + addCount / 20;
                        Instance.Logger.LogInfo("运费" + Shipping.ToString());
                        //扣运费
                        KBEngine.Avatar player = Tools.instance.getPlayer();
                        if ((int)player.money < Shipping)
                        {
                            UIPopTip.Inst.Pop("没有足够灵石支付飞剑灵力", PopTipIconType.包裹);
                            return false;
                        }
                        else
                        {
                            player.AddMoney(-Shipping);
                            UIPopTip.Inst.Pop($"{Shipping.ToCNNumber()}灵石化作了飞剑的灵力", PopTipIconType.包裹);
                        }    
                        Instance.Logger.LogInfo("发送npc请求邮件");
                        VTools.SendNewEmail(Instance.CyOpenZengLi, npcsay, SendTime: VTools.nowTime, actionId: 2, itemId: itemId, itemNum: itemNum, outtime: 1);
                        UINPCJiaoHu.Inst.ZengLi.RefreshUI();
                        UINPCJiaoHu.Inst.HideNPCZengLi();
                        //新邮件，导致联系人排序置顶
                        Instance.ScrollPosition = 1f;
                    }
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(UINPCZengLi.OnReturnBtnClick))]
            public static bool OnReturnBtnClick_Prefix()
            {
                if (Instance.CyOpenZengLi > 0)
                {
                    Instance.Logger.LogInfo("拦截赠礼返回");
                    UINPCJiaoHu.Inst.HideNPCZengLi();
                    return false;
                }
                return true;
            }
        }

    }
}
