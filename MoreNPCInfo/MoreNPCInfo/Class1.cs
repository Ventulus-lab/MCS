using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
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
            foreach (var item in tShuXing.GetComponentsInChildren<Image>())
            {
                IconImage.Add(UnityEngine.Object.Instantiate<GameObject>(item.gameObject));
            }
            Instance.Logger.LogInfo("共获取图片对象" + IconImage.Count);
            //标题图、标题图、年龄、气血、情分、修为、状态、寿元、资质、悟性、遁速、神识
            //存一个范例
            Transform tNianLing = tShuXing.Find("NianLing");
            Instance.CiTiao = UnityEngine.Object.Instantiate<GameObject>(tNianLing.gameObject);
            
            if (Instance.CiTiao == null ) Instance.Logger.LogInfo("备份失败");
            //【删除所有子对象】
            //tShuXing.DestoryAllChild();
        }

        private  List<GameObject> IconImage = new List<GameObject>();
        private  GameObject CiTiao = new GameObject();

        private static Dictionary<int, string> NPCAction = new Dictionary<int, string>()
        {
            {1,"消失"},
            {2,"闭关"},
            {3,"采药"},
            {4,"采矿"},
            {5,"炼丹"},
            {6,"炼器"},
            {7,"修炼神通"},
            {8,"挑选秘籍"},
            {9,"挑选法宝"},
            {10,"论道"},
            {11,"买药"},
            {30,"宁州猎杀妖兽"},
            {31,"做主城任务"},
            {32,"暂无"},
            {33,"游历"},
            {34,"打劫"},
            {35,"做门派任务"},
            {36,"收集资源"},
            {37,"寿元将尽"},
            {41,"海上猎杀妖兽"},
            {42,"海上游历"},
            {43,"碎星岛进货"},
            {44,"准备出海"},
            {45,"炼制阵旗"},
            {46,"参加大比"},
            {50,"突破"},
            {51,"跑商"},
            {52,"跑商"},
            {53,"跑商"},
            {54,"参加拍卖会"},
            {55,"参加拍卖会"},
            {56,"参加拍卖会"},
            {57,"参加拍卖会"},
            {99,"准备飞升"},
            {100,"神游太虚"},
            {101,"大师兄"},
            {102,"长老"},
            {103,"掌门"},
            {104,"拜山"},
            {105,"拜山"},
            {111,"天机阁跑商"},
            {112,"天机阁进货"},
            {113,"受邀"},
            {114,"道侣"},
            {115,"管理商会事宜"},
            {116,"星宫两位宫主用闭关"},
            {117,"飞升观礼"},
            {121,"采集灵核"},
            {122,"采集灵核"},
            {123,"采集灵核"},
            {124,"采集灵核"},
            {125,"采集灵核"},
            {126,"采集灵核"},
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
                if (tShuXingTitle) tShuXingTitle.gameObject.SetActive(false);

                //年龄
                Transform tNianLing = tShuXing.Find("NianLing");
                (tNianLing as RectTransform).anchoredPosition3D = new Vector3(-120, 22.5f, 0);
                tNianLing.Find("Text").GetComponent<Text>().text = npc.Age.ToString() + "/" + npc.ShouYuan.ToString();

                //存一个范例

                Instance.CiTiao = UnityEngine.Object.Instantiate<GameObject>(tNianLing.gameObject);
                if (Instance.CiTiao == null) Instance.Logger.LogInfo("备份失败");
                Instance.Logger.LogInfo("年龄往后");
                //Id
                Transform tId = tShuXing.Find("Id");
                if (tId == null)
                {
                    Instance.Logger.LogInfo("准备新建词条ID");
                    if (Instance.CiTiao == new GameObject()) Instance.Logger.LogInfo("根本没备份等于新go");
                    if (Instance.CiTiao == null) Instance.Logger.LogInfo("根本没备份等于空");
                    if (tShuXing == null) Instance.Logger.LogInfo("属性对象都没有了");
                    if (Instance.CiTiao.transform == null) Instance.Logger.LogInfo("属性对象的transform都没有了");
                    tId = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tId.name = "Id";
                    (tId as RectTransform).anchoredPosition3D = new Vector3(-120, 157.5f, 0);
                    Instance.Logger.LogInfo("新建词条ID完成");
                }
                tId.Find("Title").GetComponent<Text>().text = "ID:";
                tId.Find("Text").GetComponent<Text>().text = npc.ID.ToString() + (npc.IsZhongYaoNPC ? string.Format("({0})", npc.ZhongYaoNPCID) : "");


                //行动
                Transform tAction = tShuXing.Find("Action");
                if (tAction == null)
                {
                    tAction = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tAction.name = "Action";
                    (tAction as RectTransform).anchoredPosition3D = new Vector3(130, 157.5f, 0);
                }
                tAction.Find("Title").GetComponent<Text>().text = "行动:";
                tAction.Find("Text").GetComponent<Text>().text = npc.ActionID.ToString();

                //类型
                Transform tType = tShuXing.Find("Type");
                if (tType == null)
                {
                    
                    tType = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tType.name = "Type";
                    (tType as RectTransform).anchoredPosition3D = new Vector3(-120, 112.5f, 0);
                }
                tType.Find("Title").GetComponent<Text>().text = "类型:";
                tType.Find("Text").GetComponent<Text>().text = (NPCType.ContainsKey(npc.NPCType) ? NPCType[npc.NPCType] : "未知") + (ShowStringInt.Value ? npc.NPCType.ToString() : "");


                //流派
                Transform tLiuPai = tShuXing.Find("LiuPai");
                if (tLiuPai == null)
                {
                    tLiuPai = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tLiuPai.name = "LiuPai";
                    (tLiuPai as RectTransform).anchoredPosition3D = new Vector3(130, 112.5f, 0);
                }
                tLiuPai.Find("Title").GetComponent<Text>().text = "流派:";
                tLiuPai.Find("Text").GetComponent<Text>().text = (NPCLiuPai.ContainsKey(npc.LiuPai) ? NPCLiuPai[npc.LiuPai] : "未知") + (ShowStringInt.Value ? npc.LiuPai.ToString() : "");

                //性格
                Transform tXingGe = tShuXing.Find("XingGe");
                if (tXingGe == null)
                {
                    tXingGe = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tXingGe.name = "XingGe";
                    (tXingGe as RectTransform).anchoredPosition3D = new Vector3(-120, 67.5f, 0);
                }
                tXingGe.Find("Title").GetComponent<Text>().text = "性格:";
                tXingGe.Find("Text").GetComponent<Text>().text = (NPCXingGe.ContainsKey(npc.XingGe) ? NPCXingGe[npc.XingGe] : "未知") + (ShowStringInt.Value ? npc.XingGe.ToString() : "") + (npc.XingGe < 10 ? "(正)" : "(邪)");


                //标签
                Transform tTag = tShuXing.Find("Tag");
                if (tTag == null)
                {
                    tTag = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tTag.name = "Tag";
                    (tTag as RectTransform).anchoredPosition3D = new Vector3(130, 67.5f, 0);
                }
                tTag.Find("Title").GetComponent<Text>().text = "标签:";
                tTag.Find("Text").GetComponent<Text>().text = (NPCTag.ContainsKey(npc.Tag) ? NPCTag[npc.Tag] : "未知") + (ShowStringInt.Value ? npc.Tag.ToString() : "");

                Instance.Logger.LogInfo("标签往后");

                //寿元
                Transform tShouYuan = tShuXing.Find("ShouYuan");
                if (tShouYuan) tShouYuan.gameObject.SetActive(false);

                //悟道类型
                Transform tWuDao = tShuXing.Find("WuDao");
                if (tWuDao == null)
                {
                    tWuDao = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tWuDao.name = "WuDao";
                    (tWuDao as RectTransform).anchoredPosition3D = new Vector3(130, 22.5f, 0);
                }
                tWuDao.Find("Title").GetComponent<Text>().text = "悟道:";
                int wudao = 0;
                if (npc.json.HasField("wudaoType"))
                    wudao = npc.json.GetField("wudaoType").I;
                tWuDao.Find("Text").GetComponent<Text>().text = (NPCWuDao.ContainsKey(wudao) ? NPCWuDao[wudao] : "未知") + (ShowStringInt.Value ? wudao.ToString() : "");

                //气血
                Transform tQiXue = tShuXing.Find("QiXue");
                if (tQiXue == null)
                {
                    tQiXue = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tQiXue.name = "QiXue";
                    (tQiXue as RectTransform).anchoredPosition3D = new Vector3(-120, -22.5f, 0);
                }


                //好感
                Transform tFavor = tShuXing.Find("QingFen");
                if (tFavor == null)
                {
                    tFavor = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tFavor.name = "QingFen";
                    (tFavor as RectTransform).anchoredPosition3D = new Vector3(130, -22.5f, 0);
                }
                int FavorLevel = 1;
                while (FavorLevel < favorQuJianList.Count && npc.Favor >= favorQuJianList[FavorLevel])
                {
                    FavorLevel++;
                }
                tFavor.Find("Text").GetComponent<Text>().text = favorStrList[FavorLevel - 1] + npc.Favor.ToString();


                //悟性
                Transform tWuXing = tShuXing.Find("WuXing");
                if (tWuXing == null)
                {
                    tWuXing = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tWuXing.name = "WuXing";
                    (tWuXing as RectTransform).anchoredPosition3D = new Vector3(-120, -67.5f, 0);
                }

                //资质
                Transform tZiZhi = tShuXing.Find("ZiZhi");
                if (tZiZhi == null)
                {
                    tZiZhi = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tZiZhi.name = "ZiZhi";
                    (tZiZhi as RectTransform).anchoredPosition3D = new Vector3(130, -67.5f, 0);
                }


                //遁速
                Transform tDunSu = tShuXing.Find("DunSu");
                if (tDunSu == null)
                {
                    tDunSu = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tDunSu.name = "DunSu";
                    (tDunSu as RectTransform).anchoredPosition3D = new Vector3(-120, -112.5f, 0);
                }


                //修为
                Transform tXiuWei = tShuXing.Find("XiuWei");
                if (tXiuWei == null)
                {
                    tXiuWei = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tXiuWei.name = "XiuWei";
                    (tXiuWei as RectTransform).anchoredPosition3D = new Vector3(130, -112.5f, 0);
                }


                //神识
                Transform tShenShi = tShuXing.Find("ShenShi");
                if (tShenShi == null)
                {
                    tShenShi = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tShenShi.name = "ShenShi";
                    (tShenShi as RectTransform).anchoredPosition3D = new Vector3(-120, -157.5f, 0);
                }


                //状态
                Transform tZhuangTai = tShuXing.Find("ZhuangTai");
                if (tZhuangTai == null)
                {
                    tZhuangTai = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                    tZhuangTai.name = "ZhuangTai";
                    (tZhuangTai as RectTransform).anchoredPosition3D = new Vector3(130, -157.5f, 0);
                }



            }
        }

    }
}
