using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        private static List<string> favorStrList = new List<string>();
        private static List<int> favorQuJianList = new List<int>();
        public static ConfigEntry<bool> MaskByCondition;
        public static ConfigEntry<bool> ShowStringInt;
        void Awake()
        {
            Instance = this;
        }

        void Init(MessageData data)
        {
            MaskByCondition = Config.Bind<bool>("config", "MaskByCondition", true, "按条件遮挡部分信息，默认开启");
            ShowStringInt = Config.Bind<bool>("config", "ShowStringInt", false, "显示代表字符串的数值，默认关闭");
            foreach (JSONObject jsonobject in jsonData.instance.NpcHaoGanDuData.list)
            {
                favorQuJianList.Add(jsonobject["QuJian"].list[0].I);
                favorStrList.Add(jsonobject["HaoGanDu"].Str);
            }

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

            //备份图片
            foreach (var item in tShuXing.GetComponentsInChildren<Image>())
            {
                IconImage.Add(UnityEngine.Object.Instantiate<GameObject>(item.gameObject));
            }
            Instance.Logger.LogInfo("共获取图片对象" + IconImage.Count);
            //标题图、标题图、年龄、气血、情分、修为、状态、寿元、资质、悟性、遁速、神识

            //存一个词条范例
            Transform tCun = tShuXing.Find("NianLing");
            Instance.CiTiao = UnityEngine.Object.Instantiate<GameObject>(tCun.gameObject);
            //存一个标题字体
            tCun = tShuXing.Find("Title").GetComponentInChildren<Text>().transform;
            Instance.BiaoTi = UnityEngine.Object.Instantiate<GameObject>(tCun.gameObject);
            tCun = null;


            //开始协程
            StartCoroutine(BuildCiTiao());
        }
        IEnumerator BuildCiTiao()
        {
            UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
            Transform tShuXing = NPCInfoPanel.transform.Find("ShuXing");
            //【删除所有子对象】
            tShuXing.DestoryAllChild();

            //【协程返回控制权】
            yield return null;

            //【新建词条子对象】
            Transform tZhongZu = MakeNewCiTiao("ZhongZu", tShuXing);
            tZhongZu.Find("Title").GetComponent<Text>().text = "种族:";

            Transform tNianLing = MakeNewCiTiao("NianLing", tShuXing);
            tNianLing.Find("Title").GetComponent<Text>().text = "年龄:";

            Transform tQingFen = MakeNewCiTiao("QingFen", tShuXing);
            tQingFen.Find("Title").GetComponent<Text>().text = "好感:";

            Transform tQiXue = MakeNewCiTiao("QiXue", tShuXing);
            tQiXue.Find("Title").GetComponent<Text>().text = "气血:";

            Transform tZiZhi = MakeNewCiTiao("ZiZhi", tShuXing);
            tZiZhi.Find("Title").GetComponent<Text>().text = "资质:";

            Transform tWuXing = MakeNewCiTiao("WuXing", tShuXing);
            tWuXing.Find("Title").GetComponent<Text>().text = "悟性:";

            Transform tDunSu = MakeNewCiTiao("DunSu", tShuXing);
            tDunSu.Find("Title").GetComponent<Text>().text = "遁速:";

            Transform tShenShi = MakeNewCiTiao("ShenShi", tShuXing);
            tShenShi.Find("Title").GetComponent<Text>().text = "神识:";

            //8左8右
            Transform tAction = MakeNewCiTiao("Action", tShuXing);
            tAction.Find("Title").GetComponent<Text>().text = "行动:";

            Transform tType = MakeNewCiTiao("Type", tShuXing);
            tType.Find("Title").GetComponent<Text>().text = "类型:";

            Transform tLiuPai = MakeNewCiTiao("LiuPai", tShuXing);
            tLiuPai.Find("Title").GetComponent<Text>().text = "流派:";

            Transform tXingGe = MakeNewCiTiao("XingGe", tShuXing);
            tXingGe.Find("Title").GetComponent<Text>().text = "性格:";

            Transform tTag = MakeNewCiTiao("Tag", tShuXing);
            tTag.Find("Title").GetComponent<Text>().text = "标签:";

            Transform tWuDao = MakeNewCiTiao("WuDao", tShuXing);
            tWuDao.Find("Title").GetComponent<Text>().text = "悟道:";

            Transform tXiuWei = MakeNewCiTiao("XiuWei", tShuXing);
            tXiuWei.Find("Title").GetComponent<Text>().text = "修为:";

            Transform tZhuangTai = MakeNewCiTiao("ZhuangTai", tShuXing);
            tZhuangTai.Find("Title").GetComponent<Text>().text = "状态:";

            //不显示
            //Transform tShouYuan = MakeNewCiTiao("ShouYuan", tShuXing);
            //tShouYuan.Find("Title").GetComponent<Text>().text = "寿元:";
            //tShouYuan.gameObject.SetActive(false);

            for (int i = 0; i < tShuXing.childCount; i++)
            {
                Vector3 v3;
                if (i < 8)
                    v3 = new Vector3(-130f, 157.5f - 45f * i, 0);
                else
                    v3 = new Vector3(130f, 157.5f - 45f * (i - 8), 0);
                tShuXing.GetChild(i).localPosition = v3;
            }

            Transform tID = UnityEngine.Object.Instantiate<GameObject>(Instance.BiaoTi, tShuXing).transform;
            tID.name = "ID";
            tID.localPosition = new Vector3(-230, 700, 0);
        }
        public static Transform MakeNewCiTiao(string name, Transform tShuXing)
        {
            Transform transform = tShuXing.Find(name);
            if (transform == null)
            {
                transform = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                transform.name = name;
            }
            return transform;
        }

        public List<GameObject> IconImage = new List<GameObject>();
        public GameObject CiTiao = new GameObject();
        public GameObject BiaoTi = new GameObject();
        private static Dictionary<int, string> NPCAction = new Dictionary<int, string>()
        {
            {1,"休息"},
            {2,"闭关"},
            {3,"采药"},
            {4,"采矿"},
            {5,"炼丹"},
            {6,"炼器"},
            {7,"修炼神通"},
            {8,"挑选秘籍"},
            {9,"挑选法宝"},
            {10,"论道"},
            {11,"买丹药"},
            {30,"猎杀宁州妖兽"},
            {31,"做主城任务"},
            {32,"暂无"},
            {33,"游历"},
            {34,"打劫"},
            {35,"做门派任务"},
            {36,"收集突破材料"},
            {37,"寿元将尽寻找丹药"},
            {41,"猎杀海上妖兽"},
            {42,"海上游历"},
            {43,"碎星岛进货"},
            {44,"准备出海"},
            {45,"炼制阵旗"},
            {46,"参加大比"},
            {50,"闭关突破"},
            {51,"跑商(东石)"},
            {52,"跑商(天星)"},
            {53,"跑商(海上)"},
            {54,"参加拍卖会(东石)"},
            {55,"参加拍卖会(天机阁)"},
            {56,"参加拍卖会(海上)"},
            {57,"参加拍卖会(南崖城)"},
            {99,"准备飞升"},
            {100,"神游太虚"},
            {101,"担任大师兄"},
            {102,"担任长老"},
            {103,"担任掌门"},
            {104,"招待拜山"},
            {105,"招待拜山"},
            {111,"天机阁跑商"},
            {112,"天机阁进货"},
            {113,"受邀至洞府"},
            {114,"道侣至洞府"},
            {115,"管理碎星商会事宜"},
            {116,"星宫宫主闭关"},
            {117,"飞升观礼"},
            {121,"采集1品灵核"},
            {122,"采集2品灵核"},
            {123,"采集3品灵核"},
            {124,"采集4品灵核"},
            {125,"采集5品灵核"},
            {126,"采集6品灵核"},
            {131,"执行杀手任务"},
            {201,"倪少开局关禁闭"},
            {202,"倪少金丹回倪府"},
            {203,"倪少港口等玩家"},
            {210,"大师兄广场罚站"},
            {211,"林二在林府修炼"},
            {212,"林大被家族逼婚"},
            {221,"百里奇府内修炼"},
            {222,"百里奇去蓬莎岛"},
            {231,"麻老九禾山罚站"},
            {232,"杜老二沂山罚站"},
            {233,"碎星商会罚站"},
            {234,"青石灵脉假挖矿"},
            {235,"感悟五行剑诀"},
        };
        private static Dictionary<int, string> NPCType = new Dictionary<int, string>()
        {
            {1,"竹山"},
            {2,"金虹"},
            {3,"星河"},
            {4,"离火"},
            {5,"化尘"},
            {6,"倪家"},
            {7,"林家"},
            {8,"百里"},
            {9,"公孙"},
            {10,"散修"},
            {11,"白帝楼"},
            {12,"天机阁"},
            {13,"沂山派"},
            {14,"禾山道"},
            {15,"蓬莎岛"},
            {16,"碎星岛"},
            {17,"千流岛"},
            {18,"古神教"},
            {19,"天魔道"},
            {20,"血剑宫"},
            {21,"风雨楼"},
            {22,"杀手"},
            {23,"星宫"},
            {24,"废弃"},
            {25,"万魂殿"},
        };
        private static Dictionary<int, string> NPCLiuPai = new Dictionary<int, string>()
        {
            {1,"竹山五行"},
            {2,"竹山缠绕"},
            {3,"竹山宗中毒流"},
            {4,"温杰"},
            {11,"金虹五行"},
            {12,"金虹神剑"},
            {13,"金虹蓄势"},
            {14,"徐凡"},
            {15,"弃牌隐虚破"},
            {21,"星河五行"},
            {22,"星河冰霜"},
            {23,"星河剑修"},
            {31,"离火五行"},
            {32,"离火灼烧"},
            {33,"离火化焰"},
            {34,"焦飞"},
            {35,"火剑"},
            {41,"化尘五行"},
            {42,"化尘引力"},
            {43,"化尘屯牌"},
            {44,"化尘尘沙阵"},
            {61,"倪家修士"},
            {62,"倪旭欣"},
            {71,"林家修士"},
            {72,"林沐心"},
            {81,"百里修士"},
            {82,"百里奇"},
            {91,"公孙-纯阵"},
            {92,"公孙季"},
            {101,"散修五行"},
            {102,"散修崩岩"},
            {103,"散修暗劲"},
            {104,"散修乱刃"},
            {105,"散修火毒"},
            {106,"散修烈火掌"},
            {107,"散修暗劲2（待替换）"},
            {108,"散修剑罡"},
            {111,"白帝楼阴阳"},
            {112,"白帝楼剑阵"},
            {121,"天机剑罡"},
            {122,"天机五行"},
            {123,"天机火毒"},
            {131,"沂山派"},
            {141,"禾山道"},
            {151,"蓬莎岛烈焰"},
            {152,"蓬莎岛流血"},
            {153,"蓬莎岛滞气"},
            {154,"蓬莎岛自愈"},
            {155,"蓬莎岛剑罡"},
            {161,"碎星岛爪"},
            {162,"碎星岛神"},
            {163,"碎星岛针"},
            {164,"唐连峰"},
            {171,"千流岛"},
            {181,"古神教"},
            {191,"天魔道"},
            {201,"血剑宫"},
            {211,"风雨楼杀手-绝命流（废弃连击）"},
            {212,"风雨楼杀手-吸血"},
            {213,"风雨楼杀手-绝命流"},
            {214,"风雨楼杀手-血剑"},
            {221,"普通杀手-剑修"},
            {222,"普通杀手-五行"},
            {223,"普通杀手-崩岩"},
            {224,"普通杀手-火毒"},
            {231,"星宫修士"},
            {232,"王登"},
            {241,"弃牌隐虚破（废弃）"},
            {251,"万魂殿"},
        };
        private static Dictionary<int, string> NPCXingGe = new Dictionary<int, string>()
        {
            {1,"善良"},
            {2,"稳重"},
            {3,"洒脱"},
            {4,"活泼"},
            {5,"傲慢"},
            {6,"温柔"},
            {7,"孤僻"},
            {8,"暴躁"},
            {11,"阴险"},
            {12,"稳重"},
            {13,"洒脱"},
            {14,"傲慢"},
            {15,"贪婪"},
            {16,"唯我"},
            {17,"孤僻"},
            {18,"暴躁"},
        };
        private static Dictionary<int, string> NPCTag = new Dictionary<int, string>()
        {
            {1,"门派勤奋修炼型"},
            {2,"门派快乐炼丹型"},
            {3,"门派酷爱炼器型"},
            {4,"门派搬砖打工型"},
            {5,"门派战狂型"},
            {6,"门派游山玩水型"},
            {7,"门派心思歹毒型"},
            {21,"散修勤奋修炼型"},
            {22,"散修快乐炼丹型"},
            {23,"散修酷爱炼器型"},
            {24,"散修搬砖打工型"},
            {25,"散修战狂型"},
            {26,"散修游山玩水型"},
            {27,"散修心思歹毒型"},
            {31,"蓬莎岛杀妖型正"},
            {32,"蓬莎岛杀妖型邪"},
            {33,"碎星岛跑商型正"},
            {34,"碎星岛跑商型邪"},
            {51,"邪修和魔修"},
        };
        private static Dictionary<int, string> NPCWuDao = new Dictionary<int, string>()
        {
            {1,"通用散修1"},
            {2,"基础剑气"},
            {3,"基础神识"},
            {4,"基础阵修"},
            {5,"基础丹修"},
            {6,"遁速流"},
            {7,"暗劲流"},
            {8,"通用散修2"},
            {9,"通用散修3"},
            {11,"五行金"},
            {12,"蓄势流"},
            {13,"神剑流"},
            {21,"五行木"},
            {22,"中毒流"},
            {23,"缠绕流"},
            {31,"五行水"},
            {32,"五行水2"},
            {33,"水剑"},
            {34,"冰冻"},
            {41,"五行火"},
            {42,"灼烧"},
            {43,"化焰"},
            {44,"火剑"},
            {51,"五行土"},
            {52,"引力"},
            {54,"屯牌"},
            {53,"尘沙阵"},
            {61,"散修五行"},
            {62,"散修火毒"},
            {63,"体修自愈"},
            {64,"徐凡"},
            {65,"唐连峰"},
            {66,"神殇流"},
            {71,"剑阵流"},
            {81,"乱刃"},
            {82,"滞气流"},
            {83,"禾山流"},
            {84,"木剑流"},
            {85,"百里体修"},
            {86,"崩岩"},
            {87,"弃牌"},
            {88,"万魂殿"},
            {89,"非也"},
            {91,"金龙"},
            {92,"木龙"},
            {93,"水龙"},
            {94,"火龙"},
            {95,"土龙"},
            {96,"灵龙"},
            {97,"暴龙"},
            {99,"神树"},
            {100,"九幽大圣"},
            {101,"浪方大圣"},
            {102,"吞云大圣"},
            {103,"恶饕老祖"},
            {104,"金虹剑仙"},
        };
        private static Dictionary<int, string> NPCStatus = new Dictionary<int, string>()
        {
            {1,"正常"},
            {2,"陷入瓶颈"},
            {4,"身负重伤"},
            {5,"神识受损"},
            {6,"寿元将尽"},
            {10,"天人感应"},
            {11,"灵光闪现"},
            {12,"灵思枯竭"},
            {20,"普通受邀"},
            {21,"道侣受邀"},
        };

        [HarmonyPatch(typeof(UINPCInfoPanel))]
        class UINPCInfoPanelPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("SetNPCInfo")]
            public static bool SetNPCInfoPrefix(UINPCInfoPanel __instance)
            {
                Instance.Logger.LogInfo("SetNPCInfoPrefix");
                Instance.Logger.LogInfo(__instance.npc.json.ToString());
                UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
                Transform tShuXing = NPCInfoPanel.transform.Find("ShuXing");
                UINPCData npc = __instance.npc;

                //称号

                Transform tChengHao = NPCInfoPanel.transform.Find("NPCShow/ChengHao");
                tChengHao.Find("Text").GetComponent<Text>().text = npc.Title;

                //ID
                tShuXing.Find("ID").GetComponent<Text>().text = npc.ID.ToString() + (npc.IsZhongYaoNPC ? string.Format("({0})", npc.ZhongYaoNPCID) : "");

                //种族+性别
                string ZhongZu = string.Empty;
                int AvatarType = npc.json.GetField("AvatarType").I;
                int SexType = npc.json.GetField("SexType").I;
                if (AvatarType == 1)
                    ZhongZu = "人族";
                else if (AvatarType == 2)
                    ZhongZu = "妖族";
                else if (AvatarType == 3)
                    ZhongZu = "魔族";
                else if (AvatarType == 4)
                    ZhongZu = "鬼族";
                if (SexType == 1)
                    ZhongZu = ZhongZu + "男";
                else if (SexType == 2)
                    ZhongZu = ZhongZu + "女";
                else if (SexType == 3)
                    ZhongZu = ZhongZu + "变态";
                if (ZhongZu == string.Empty)
                    ZhongZu = "未知";
                tShuXing.Find("ZhongZu/Text").GetComponent<Text>().text = ZhongZu;

                //年龄+寿元
                tShuXing.Find("NianLing/Text").GetComponent<Text>().text = npc.Age.ToString() + "/" + npc.ShouYuan.ToString();

                //好感级别
                int FavorLevel = 1;
                while (FavorLevel < favorQuJianList.Count && npc.Favor >= favorQuJianList[FavorLevel])
                {
                    FavorLevel++;
                }
                tShuXing.Find("QingFen/Text").GetComponent<Text>().text = favorStrList[FavorLevel - 1] + "(" + npc.Favor.ToString() + ")";

                //气血
                tShuXing.Find("QiXue/Text").GetComponent<Text>().text = npc.HP.ToString();

                //资质
                tShuXing.Find("ZiZhi/Text").GetComponent<Text>().text = npc.ZiZhi.ToString();

                //悟性
                tShuXing.Find("WuXing/Text").GetComponent<Text>().text = npc.WuXing.ToString();

                //遁速
                tShuXing.Find("DunSu/Text").GetComponent<Text>().text = npc.DunSu.ToString();

                //神识
                tShuXing.Find("ShenShi/Text").GetComponent<Text>().text = npc.ShenShi.ToString();

                //行动
                string actionstr = NPCAction.ContainsKey(npc.ActionID) ? NPCAction[npc.ActionID] : "未知";
                string placestr = string.Empty;
                foreach (var dian in from int dian in NpcJieSuanManager.inst.npcMap.bigMapNPCDictionary.Keys
                                     where NpcJieSuanManager.inst.npcMap.bigMapNPCDictionary[dian].Contains(npc.ID)
                                     select dian)
                {
                    placestr = "在大地图上";
                    foreach (string ludian in jsonData.instance.AllMapLuDainType.keys)
                    {
                        if (ludian == dian.ToString())
                        {
                            placestr = "在" + jsonData.instance.AllMapLuDainType[ludian]["LuDianName"].str.ToCN();
                            break;
                        }
                    }
                }

                foreach (var scene in from string scene in NpcJieSuanManager.inst.npcMap.threeSenceNPCDictionary.Keys
                                      where NpcJieSuanManager.inst.npcMap.threeSenceNPCDictionary[scene].Contains(npc.ID)
                                      select scene)
                {
                    placestr = "在" + jsonData.instance.SceneNameJsonData[scene]["MapName"].str.ToCN();
                    break;
                }

                foreach (var (fuben, pos) in from string fuben in NpcJieSuanManager.inst.npcMap.fuBenNPCDictionary.Keys
                                             let fubendict = NpcJieSuanManager.inst.npcMap.fuBenNPCDictionary[fuben]
                                             from int pos in fubendict.Keys
                                             where fubendict[pos].Contains(npc.ID)
                                             select (fuben, pos))
                {
                    placestr = "在" + jsonData.instance.SceneNameJsonData[fuben]["MapName"].str.ToCN() + "的第" + pos.ToString() + "位置";
                    break;
                }

                tShuXing.Find("Action/Text").GetComponent<Text>().text = (placestr == string.Empty ? "" : placestr) + actionstr + (ShowStringInt.Value ? npc.ActionID.ToString() : "");

                //类型
                tShuXing.Find("Type/Text").GetComponent<Text>().text = (NPCType.ContainsKey(npc.NPCType) ? NPCType[npc.NPCType] : "未知") + (ShowStringInt.Value ? npc.NPCType.ToString() : "");

                //流派
                tShuXing.Find("LiuPai/Text").GetComponent<Text>().text = (NPCLiuPai.ContainsKey(npc.LiuPai) ? NPCLiuPai[npc.LiuPai] : "未知") + (ShowStringInt.Value ? npc.LiuPai.ToString() : "");

                //性格
                tShuXing.Find("XingGe/Text").GetComponent<Text>().text = (NPCXingGe.ContainsKey(npc.XingGe) ? NPCXingGe[npc.XingGe] : "未知") + (ShowStringInt.Value ? npc.XingGe.ToString() : "") + (npc.XingGe < 10 ? "(正)" : "(邪)");

                //标签
                tShuXing.Find("Tag/Text").GetComponent<Text>().text = (NPCTag.ContainsKey(npc.Tag) ? NPCTag[npc.Tag] : "未知") + (ShowStringInt.Value ? npc.Tag.ToString() : "");

                //悟道类型
                int wudao = 0;
                if (npc.json.HasField("wudaoType"))
                    wudao = npc.json.GetField("wudaoType").I;
                tShuXing.Find("WuDao/Text").GetComponent<Text>().text = (NPCWuDao.ContainsKey(wudao) ? NPCWuDao[wudao] : "未知") + (ShowStringInt.Value ? wudao.ToString() : "");

                //修为
                int maxexp = jsonData.instance.LevelUpDataJsonData[npc.Level.ToString()]["MaxExp"].I;
                int percent = npc.Exp * 100 / maxexp;
                tShuXing.Find("XiuWei/Text").GetComponent<Text>().text = npc.LevelStr + "(" + percent + "%)";

                //状态
                string zhuangtaistr = (NPCStatus.ContainsKey(npc.ZhuangTai) ? NPCStatus[npc.ZhuangTai] : "未知");
                int time = 0;
                if (npc.json.HasField("Status"))
                    time = npc.json["Status"]["StatusTime"].I;
                if (time <= 1200 && time > 0)
                    zhuangtaistr += "(" + time + "个月)";
                if (npc.ZhuangTai == 2)
                {
                    if (NpcJieSuanManager.inst.npcTuPo.IsCanSmallTuPo(npc.ID))
                        zhuangtaistr += "(小境界突破)";
                    else if (NpcJieSuanManager.inst.npcTuPo.IsCanBigTuPo(npc.ID))
                        zhuangtaistr += "(大境界突破率" + NpcJieSuanManager.inst.npcTuPo.GetNpcBigTuPoLv(npc.ID) + ")";
                }
                tShuXing.Find("ZhuangTai/Text").GetComponent<Text>().text = zhuangtaistr;

                return false;
            }
            [HarmonyPostfix]
            [HarmonyPatch("SetNPCInfo")]
            public static void SetNPCInfoPostfix()
            {

            }
        }
    }
}
