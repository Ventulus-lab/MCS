using Bag;
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
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YSGame;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.CyContactOptimization", "传音符联系人优化", "1.7.0")]
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
        private static readonly string Cy100000 = @"{""id"":100000,""AvatarID"":2,""info"":""{DiDian}"",""Type"":3,""DelayTime"":[],""TaskID"":0,""TaskIndex"":[],""WeiTuo"":0,""ItemID"":0,""valueID"":[],""value"":[],""SPvalueID"":0,""StarTime"":"""",""EndTime"":"""",""Level"":[],""HaoGanDu"":0,""EventValue"":[],""fuhao"":"""",""IsOnly"":1,""IsAdd"":0,""IsDelete"":0,""NPCLevel"":[],""IsAlive"":0}";
        //魏老播报，占用传音符id100000
        private static int CyFuId = 100000;
        private static int CyNPCId = 2;
        public void AddCy100000(MessageData data = null)
        {

            Logger.LogInfo("增加100000号传音符");
            JSONObject js100000 = new JSONObject(Cy100000);
            Logger.LogInfo(js100000.ToString());
            jsonData.instance.ChuanYingFuBiao.SetField(CyFuId.ToString(), js100000);
        }
        public void AddCyWuZuo()
        {
            KBEngine.Avatar player = Tools.instance.getPlayer();
            //ChuanYingManager.ReadData竟然是Private，还是手动给他加吧
            if (!jsonData.instance.ChuanYingFuBiao.HasField(CyFuId.ToString()))
            {
                AddCy100000();
            }
            if (!player.NewChuanYingList.HasField(CyFuId.ToString()))
            {
                JSONObject emailjson = jsonData.instance.ChuanYingFuBiao[CyFuId.ToString()];
                emailjson.SetField("sendTime", player.worldTimeMag.nowTime);
                emailjson.SetField("CanCaoZuo", false);
                emailjson.SetField("AvatarName", jsonData.instance.AvatarJsonData[CyNPCId.ToString()]["Name"].Str);
                Logger.LogMessage(emailjson.ToString());
                player.NewChuanYingList.SetField(CyFuId.ToString(), emailjson);
            }

        }
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
        public class PointerItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
        {
            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!string.IsNullOrWhiteSpace(Desc))
                {
                    UToolTip.Show(Desc, 330f);
                }
            }
            public void OnPointerExit(PointerEventData eventData)
            {
                UToolTip.Close();
            }
            public string Desc;
        }
        public static string GetNPCName(int npcId)
        {
            if (jsonData.instance.AvatarRandomJsonData.HasField(npcId.ToString()))
                return jsonData.instance.AvatarRandomJsonData[npcId.ToString()]["Name"].str.ToCN();
            else if (NpcJieSuanManager.inst.npcDeath.npcDeathJson.HasField(npcId.ToString()))
                return NpcJieSuanManager.inst.npcDeath.npcDeathJson[npcId.ToString()]["deathName"].str.ToCN();
            else if (jsonData.instance.AvatarJsonData.HasField(npcId.ToString()))
                return jsonData.instance.AvatarJsonData[npcId.ToString()]["Name"].str.ToCN();
            else
                return "未知";
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


                //收取
                GameObject goShouQu = MakeNewBiaoQian("收取");
                goShouQu.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickShouQu(npcId); goShouQu.SetActive(false); ArrangeLabelPositions(__instance.transform); });
                //不选中也显示收取按钮
                //goShouQu.SetActive(hasShouQuItem(npcId));

                //增加死因标签
                GameObject goSiYin = MakeNewBiaoQian("死因");
                goSiYin.GetComponent<BtnCell>().mouseUp.AddListener(delegate { ClickSiYin(npcId); CyUIMag.inst.cyEmail.Restart(); CyUIMag.inst.cyEmail.Init(npcId); __instance.Click(); });

                //排序
                //由于新邮件人是先初始化cell再放红点，所以这里不合适排序
                //SortNpcCells(__instance.transform.parent);
                //标签排位置
                ArrangeLabelPositions(__instance.transform);

                //死亡类型
                if (NpcJieSuanManager.inst.IsDeath(npcId))
                {
                    JSONObject npcDeath = NpcJieSuanManager.inst.npcDeath.npcDeathJson[npcId.ToString()];
                    Instance.Logger.LogInfo(npcDeath.ToString());
 

                    //PI.Desc = DeathDesc;
                    Instance.AddCyWuZuo();
                }
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
                Instance.Logger.LogInfo("查看按钮被点击了" + MakeNPCIDStr(npcId));
                UINPCData npc = new UINPCData(id);
                if (id < 20000)
                {
                    npc.RefreshOldNpcData();
                    npc.IsFight = true;

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

                Instance.CyOpenInfoPanel = npcId;


                return null;
            }

            static UnityAction ClickShanChu(int npcId, string name)
            {
                Instance.Logger.LogInfo("删除按钮被点击了" + MakeNPCIDStr(npcId));
                //由于可能会是死人，在jsonData.instance.AvatarRandomJsonData里没有信息，所以得把name传进来
                name = GetNPCName(npcId);


                USelectBox.Show($"确认要删除联系人{name}吗？ ", delegate
                {
                    CyUIMag.inst.npcList.friendList.Remove(npcId);
                    CyUIMag.inst.npcList.Init();
                }, null);
                return null;
            }
            static UnityAction ClickSiYin(int npcId)
            {
                Instance.Logger.LogInfo("死因按钮被点击了" + MakeNPCIDStr(npcId));
                JSONObject npcDeath = NpcJieSuanManager.inst.npcDeath.npcDeathJson[npcId.ToString()];
                string DeathDesc = "唉…斯人已逝，幽思长存。且待我推算一番……";
                DeathDesc += $"{Environment.NewLine}死者姓名：{GetNPCName(npcId)}";
                if (npcDeath.HasField("deathTime"))
                {
                    DateTime deathTime = DateTime.Parse(npcDeath.GetField("deathTime").str);
                    DeathDesc += $"{Environment.NewLine}死亡记录时间：{deathTime.Year}年{deathTime.Month}月{deathTime.Day}日";
                }
                if (npcDeath.HasField("deathType"))
                {
                    int deathType = npcDeath.GetInt("deathType");
                    DeathDesc += $"{Environment.NewLine}死因：{NPCdeathType[npcDeath.GetInt("deathType")].Replace("玩家", Tools.GetPlayerName()) ?? "未知"}";
                }
                if (npcDeath.HasField("killNpcId"))
                {
                    int killNpcId = npcDeath.GetInt("killNpcId");
                    string killNpcName;
                    if (killNpcId > 0)
                        killNpcName = GetNPCName(killNpcId);
                    else
                        killNpcName = Tools.GetPlayerName();
                    DeathDesc += $"{Environment.NewLine}凶手：{killNpcName}";
                }
                KBEngine.Avatar player = Tools.instance.getPlayer();
                //加入新传音符
                EmailData emailData = new EmailData(npcId, isOld: true, CyFuId, player.worldTimeMag.nowTime)
                {
                    sceneName = DeathDesc
                };
                player.emailDateMag.AddNewEmail(npcId.ToString(), emailData);
                return null;
            }

            static UnityAction ClickShouQu(int npcId)
            {
                Instance.Logger.LogInfo("收取按钮被点击了" + MakeNPCIDStr(npcId));

                Dictionary<string, List<EmailData>> newEmailDictionary = PlayerEx.Player.emailDateMag.newEmailDictionary;
                Dictionary<string, List<EmailData>> hasReadEmailDictionary = PlayerEx.Player.emailDateMag.hasReadEmailDictionary;
                List<int> oldIdlist = new List<int>();

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
            private static string MakeNPCIDStr(int id)
            {
                id = NPCEx.NPCIDToNew(id);
                int npcId = NPCEx.NPCIDToOld(id);
                string str = id >= 20000 ? id.ToString() : string.Empty;
                if (npcId < 20000)
                    str += $"({npcId})";
                return str;
            }

            [HarmonyPostfix]
            [HarmonyPatch(nameof(CyFriendCell.updateState))]
            public static void updateState_Postfix(CyFriendCell __instance)
            {

                Transform tChaKan = __instance.transform.Find("查看");
                Transform tShanChu = __instance.transform.Find("删除");
                Transform tShouQu = __instance.transform.Find("收取");
                Transform tSiYin = __instance.transform.Find("死因");
                if (tShanChu) tShanChu.gameObject.SetActive(__instance.isSelect);
                if (tChaKan) tChaKan.gameObject.SetActive(__instance.isSelect && !__instance.isDeath && !__instance.IsFly);
                if (tShouQu) tShouQu.gameObject.SetActive(__instance.isSelect && hasShouQuItem(__instance.npcId));
                if (tSiYin) tSiYin.gameObject.SetActive(__instance.isSelect && __instance.isDeath);
                //工具人是否能查看的开关
                //if (tChaKan) tChaKan.gameObject.SetActive(__instance.isSelect && !__instance.isDeath && !__instance.IsFly && NPCEx.NPCIDToNew(__instance.npcId) >= 20000);


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
                Instance.Logger.LogInfo(Instance.CyOpenInfoPanel);
                //尝试选中刚才的npc
                if (Instance.CyOpenInfoPanel > 0)
                {
                    CyFriendCell CyFriendCell = __instance.friendCells.FirstOrDefault(x => x.npcId == Instance.CyOpenInfoPanel);
                    if (CyFriendCell != null)
                    {
                        Instance.Logger.LogInfo("找到要选中的npccell" + Instance.CyOpenInfoPanel);
                        CyFriendCell.Click();
                    }
                }

                Instance.CyOpenInfoPanel = -1;
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
                //好感高的优先，死亡飞升的得排除
                if (NpcJieSuanManager.inst.IsDeath(x) || NpcJieSuanManager.inst.IsFly(x) || NpcJieSuanManager.inst.IsDeath(y) || NpcJieSuanManager.inst.IsFly(y))
                    return 0;
                else
                {
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
                }

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
        }
    }
}
