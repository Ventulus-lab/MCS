using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JSONClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.MoreNPCInfo", "更多NPC信息", "1.6")]
    public class MoreNPCInfo : BaseUnityPlugin
    {
        void Start()
        {
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
        private static Vector3 v3;
        void Awake()
        {
            Instance = this;
        }

        void Init(MessageData data = null)
        {
            //MaskByCondition = Config.Bind<bool>("config", "MaskByCondition", true, "按条件遮挡部分信息，默认开启");
            ShowStringInt = Config.Bind<bool>("config", "ShowStringInt", false, "显示代表字符串的数值，默认关闭");
            foreach (JSONObject jsonobject in jsonData.instance.NpcHaoGanDuData.list)
            {
                favorQuJianList.Add(jsonobject["QuJian"].list[0].I);
                favorStrList.Add(jsonobject["HaoGanDu"].Str);
            }

            UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
            Transform tShuXing = NPCInfoPanel.transform.Find("ShuXing");



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


            //开始协程
            StartCoroutine(BuildCiTiao());
        }

        IEnumerator BuildCiTiao()
        {
            UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;

            //调整【NPC形象】
            //称号
            Transform tNPCShow = NPCInfoPanel.transform.Find("NPCShow");
            Transform tName = tNPCShow.Find("Name");
            Transform tChengHao = tNPCShow.Find("ChengHao");
            if (tChengHao == null)
            {
                tChengHao = UnityEngine.Object.Instantiate<GameObject>(tName.gameObject, tNPCShow).transform;
                tChengHao.gameObject.name = "ChengHao";
                //原姓名下移
                tName.localPosition = new Vector3(0, -287.4f, 0);
            }
            //左上角id显示
            Transform tID = UnityEngine.Object.Instantiate<GameObject>(Instance.BiaoTi, tNPCShow).transform;
            tID.name = "ID";
            tID.localPosition = new Vector3(-280, 320, 0);

            //【协程返回控制权】
            yield return null;

            //调整【普通查看信息面板】
            Transform tShuXing = NPCInfoPanel.transform.Find("ShuXing");
            //【删除所有子对象】
            //tShuXing.DestoryAllChild();
            for (int i = 0; i < tShuXing.childCount; i++)
            {
                tShuXing.GetChild(i).gameObject.SetActive(false);
            }

            //【协程返回控制权】
            yield return null;

            //int IndexPos = 0;

            //【新建词条子对象】
            Transform tAction = MakeNewCiTiao("行动", tShuXing, 1);


            Transform tZhuangTai = MakeNewCiTiao("状态", tShuXing, 2);


            Transform tNianLing = MakeNewCiTiao("年龄", tShuXing, 3);


            Transform tQiXue = MakeNewCiTiao("气血", tShuXing, 4);


            Transform tZiZhi = MakeNewCiTiao("资质", tShuXing, 5);


            Transform tWuXing = MakeNewCiTiao("悟性", tShuXing, 6);


            Transform tDunSu = MakeNewCiTiao("遁速", tShuXing, 7);


            Transform tShenShi = MakeNewCiTiao("神识", tShuXing, 8);



            //8左8右

            Transform tQingFen = MakeNewCiTiao("好感", tShuXing, 11);


            Transform tGuanXi = MakeNewCiTiao("关系", tShuXing, 12);


            Transform tXingGe = MakeNewCiTiao("性格", tShuXing, 13);


            Transform tXiuWei = MakeNewCiTiao("修为", tShuXing, 14);


            Transform tType = MakeNewCiTiao("类型", tShuXing, 15);


            Transform tTag = MakeNewCiTiao("标签", tShuXing, 16);

            //【协程返回控制权】
            yield return null;
            //调整【装备功法面板】
            Transform tZhuangBeiGongFaPanel = NPCInfoPanel.transform.Find("Panels/ZhuangBeiGongFaPanel");

            //增加流派信息条
            Transform tLiuPai3 = UnityEngine.Object.Instantiate<GameObject>(Instance.BiaoTi, tZhuangBeiGongFaPanel).transform;
            tLiuPai3.name = "LiuPai";
            tLiuPai3.localPosition = new Vector3(-240, 70, 0);
            tLiuPai3.GetComponent<Text>().text = "流派：";

            //装备上移
            Transform tZhuangBei3Solt = tZhuangBeiGongFaPanel.Find("ZhuangBei3Solt");
            v3 = tZhuangBei3Solt.localPosition;
            v3.y = 253;
            tZhuangBei3Solt.localPosition = v3;
            Transform tZhuangBei4Solt = tZhuangBeiGongFaPanel.Find("ZhuangBei4Solt");
            v3 = tZhuangBei4Solt.localPosition;
            v3.y = 253;
            tZhuangBei4Solt.localPosition = v3;

            //增加装备偏好信息条
            Transform tPianHao = UnityEngine.Object.Instantiate<GameObject>(Instance.BiaoTi, tZhuangBeiGongFaPanel).transform;
            tPianHao.name = "PianHao";
            tPianHao.localPosition = new Vector3(300, 155, 0);
            tPianHao.GetComponent<Text>().text = "装备偏好";
            tPianHao.gameObject.AddComponent<PointerItem>();

            //【协程返回控制权】
            yield return null;

            //调整【战斗探查信息面板】
            Transform tFightShuXing = NPCInfoPanel.transform.Find("FightShuXing");


            //增加种族
            Transform tZhongZu2 = MakeNewCiTiao("种族", tFightShuXing, 1);
            tZhongZu2.localPosition = new Vector3(30, 67.5f, 0);

            Transform tQiXue2 = tFightShuXing.Find("QiXue");
            tQiXue2.localPosition = new Vector3(30, 22.5f, 0);

            Transform tDunSu2 = tFightShuXing.Find("DunSu");
            tDunSu2.localPosition = new Vector3(-15, -22.5f, 0);

            Transform tShenShi2 = tFightShuXing.Find("ShenShi");
            tShenShi2.localPosition = new Vector3(-15, -67.5f, 0);

            Transform tXiuWei2 = tFightShuXing.Find("XiuWei");
            tXiuWei2.localPosition = new Vector3(30, -112.5f, 0);

            //增加灵根简易
            Transform tZLingGen2 = MakeNewCiTiao("灵根", tFightShuXing, 6);
            tZLingGen2.localPosition = new Vector3(30, -157.5f, 0);

            //获取五行灵根UI
            //Instance.Logger.LogInfo("尝试获取五行灵根UI");
            //Transform tShuXingPanel = SingletonMono<TabUIMag>.Instance.transform.Find("TabSelect/Panel/属性");
            //if (tShuXingPanel != null) Instance.Logger.LogInfo("找到属性面板");
            //GameObject goLingGen = tShuXingPanel.Find().gameObject;
            //Instance.LingGen = UnityEngine.Object.Instantiate<GameObject>(goLingGen);


        }
        public static Transform MakeNewCiTiao(string name, Transform tShuXing, int pos)
        {
            Transform transform = tShuXing.Find(name);
            if (transform == null)
            {
                transform = UnityEngine.Object.Instantiate<GameObject>(Instance.CiTiao, tShuXing).transform;
                transform.name = name;
                transform.Find("Title").GetComponent<Text>().text = name + ":";
                IndexPosition(transform, pos);
            }
            return transform;
        }
        public static void IndexPosition(Transform tCiTiao, int index)
        {
            if (index < 9)
                v3 = new Vector3(-130f, 202.5f - 45f * index, 0);
            else
                v3 = new Vector3(130f, 202.5f - 45f * (index - 8), 0);
            tCiTiao.localPosition = v3;
        }
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

        public List<GameObject> IconImage = new List<GameObject>();
        public GameObject CiTiao = new GameObject();
        public GameObject BiaoTi = new GameObject();
        public GameObject LingGen = new GameObject();
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
            {111,"为天机阁跑商"},
            {112,"为天机阁进货"},
            {113,"受邀拜访洞府"},
            {114,"道侣拜访洞府"},
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
        class UINPCInfoPanel_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(UINPCInfoPanel.SetNPCInfo))]
            public static bool SetNPCInfo_Prefix(UINPCInfoPanel __instance)
            {
                Instance.Logger.LogInfo("SetNPCInfoPrefix");
                UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
                UINPCData npc = __instance.npc;

                //手动激活普通的查看属性
                NPCInfoPanel.ShuXing.SetActive(true);
                NPCInfoPanel.FightShuXing.SetActive(false);

                Transform tShuXing = NPCInfoPanel.transform.Find("ShuXing");
                //Instance.Logger.LogInfo(npc.json.ToString().ToCN());
                Transform tNPCShow = NPCInfoPanel.transform.Find("NPCShow");

                //称号
                tNPCShow.Find("ChengHao/Text").GetComponent<Text>().text = npc.Title;

                //ID
                tNPCShow.Find("ID").GetComponent<Text>().text = MakeNPCIDStr(npc.ID);



                //年龄+寿元
                tShuXing.Find("年龄/Text").GetComponent<Text>().text = npc.Age.ToString() + "/" + npc.ShouYuan.ToString();

                //好感级别
                int FavorLevel = 1;
                while (FavorLevel < favorQuJianList.Count && npc.Favor >= favorQuJianList[FavorLevel])
                {
                    FavorLevel++;
                }
                tShuXing.Find("好感/Text").GetComponent<Text>().text = favorStrList[FavorLevel - 1] + $"({npc.Favor})";

                //气血
                tShuXing.Find("气血/Text").GetComponent<Text>().text = npc.HP.ToString();

                //资质
                tShuXing.Find("资质/Text").GetComponent<Text>().text = npc.ZiZhi.ToString();

                //悟性
                tShuXing.Find("悟性/Text").GetComponent<Text>().text = npc.WuXing.ToString();

                //遁速
                tShuXing.Find("遁速/Text").GetComponent<Text>().text = npc.DunSu.ToString();

                //神识
                tShuXing.Find("神识/Text").GetComponent<Text>().text = npc.ShenShi.ToString();

                //行动
                tShuXing.Find("行动/Text").GetComponent<Text>().text = MakeNPCAtionStr(npc) + (ShowStringInt.Value ? npc.ActionID.ToString() : "");

                //类型
                tShuXing.Find("类型/Text").GetComponent<Text>().text = (NPCType.ContainsKey(npc.NPCType) ? NPCType[npc.NPCType] : "未知") + (ShowStringInt.Value ? npc.NPCType.ToString() : "");


                //性格
                tShuXing.Find("性格/Text").GetComponent<Text>().text = (NPCXingGe.ContainsKey(npc.XingGe) ? NPCXingGe[npc.XingGe] : "未知") + (ShowStringInt.Value ? npc.XingGe.ToString() : "") + (npc.XingGe < 10 ? "(正)" : "(邪)");

                //标签
                tShuXing.Find("标签/Text").GetComponent<Text>().text = (NPCTag.ContainsKey(npc.Tag) ? NPCTag[npc.Tag] : "未知") + (ShowStringInt.Value ? npc.Tag.ToString() : "");

                //关系   
                tShuXing.Find("关系/Text").GetComponent<Text>().text = MakeNPCGuanXiStr(npc);

                //修为
                //"MaxExp":194400000最大值刚刚好用int装下，但要是*100就不够了
                long maxexp = jsonData.instance.LevelUpDataJsonData[npc.Level.ToString()]["MaxExp"].i;
                long percent = npc.json.GetField("exp").i * 100 / maxexp;
                tShuXing.Find("修为/Text").GetComponent<Text>().text = $"{npc.LevelStr}({percent}%)";

                //状态
                tShuXing.Find("状态/Text").GetComponent<Text>().text = MakeNPCZhuangTaiStr(npc);


                return false;
            }


            [HarmonyPrefix]
            [HarmonyPatch(nameof(UINPCInfoPanel.SetFightInfo))]
            public static bool SetFightInfo_Prefix(UINPCInfoPanel __instance)
            {
                Instance.Logger.LogInfo("SetFightInfoPrefix");
                UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
                Transform tFightShuXing = NPCInfoPanel.transform.Find("FightShuXing");
                //手动激活战斗的查看属性
                //NPCInfoPanel.ShuXing.SetActive(false);
                //NPCInfoPanel.FightShuXing.SetActive(true);

                UINPCData npc = __instance.npc;
                Instance.Logger.LogInfo(npc.json.ToString().ToCN());
                Transform tNPCShow = NPCInfoPanel.transform.Find("NPCShow");

                //称号
                tNPCShow.Find("ChengHao/Text").GetComponent<Text>().text = npc.Title;

                //ID
                tNPCShow.Find("ID").GetComponent<Text>().text = MakeNPCIDStr(npc.ID);


                //种族+性别
                tFightShuXing.Find("种族/Text").GetComponent<Text>().text = MakeZhongZuSexStr(npc);

                //灵根
                tFightShuXing.Find("灵根/Text").GetComponent<Text>().text = MakeLingGenStr(npc);

                return true;
            }
        }
        private static string MakeLingGenStr(UINPCData npc)
        {
            string strLingGen = string.Empty;
            List<string> linggenname = new List<string>() { "金", "木", "水", "火", "土" };
            List<int> linggenvalue = npc.json.GetField("LingGen").ToList();
            for (int i = 0; i < 5; i++)
            {
                strLingGen += $"{linggenname[i]}{linggenvalue[i]} ";
            }
            return strLingGen;
        }
        private static string MakeNPCGuanXiStr(UINPCData npc)
        {
            string strGuanxi = "无";
            if (npc.IsKnowPlayer || npc.IsGuDingNPC) strGuanxi = "普通";
            if (PlayerEx.IsDaoLv(npc.ID))
                strGuanxi += "、道侣";
            if (PlayerEx.IsTheather(npc.ID))
                strGuanxi += "、师父";
            if (PlayerEx.IsTuDi(npc.ID))
                strGuanxi += "、徒弟";
            if (PlayerEx.IsBrother(npc.ID))
                strGuanxi += "、结义";
            if (PlayerEx.Player.menPai > 0 && npc.json.HasField("MenPai") && PlayerEx.Player.menPai == npc.json["MenPai"].I)
                strGuanxi += "、同门";
            strGuanxi = strGuanxi.Replace("无、", string.Empty).Replace("普通、", string.Empty);
            return strGuanxi;
        }
        private static string MakeZhongZuSexStr(UINPCData npc)
        {
            int AvatarType = npc.json.GetField("AvatarType").I;
            int SexType = npc.json.GetField("SexType").I;
            string ZhongZu;
            if (AvatarType == 1 && SexType == 1)
                ZhongZu = "男人";
            else if (AvatarType == 1 && SexType == 2)
                ZhongZu = "女人";
            else if (AvatarType == 2 && SexType == 1)
                ZhongZu = "公妖";
            else if (AvatarType == 2 && SexType == 2)
                ZhongZu = "母妖";
            else if (AvatarType == 3 && SexType == 1)
                ZhongZu = "牡魔";
            else if (AvatarType == 3 && SexType == 2)
                ZhongZu = "牝魔";
            else if (AvatarType == 4 && SexType == 1)
                ZhongZu = "雄鬼";
            else if (AvatarType == 4 && SexType == 2)
                ZhongZu = "雌鬼";
            else
                ZhongZu = "未知";
            return ZhongZu;
        }
        private static string MakeNPCZhuangTaiStr(UINPCData npc)
        {
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
                else
                {
                    int tupolv = NpcJieSuanManager.inst.npcTuPo.GetNpcBigTuPoLv(npc.ID);
                    if (NpcJieSuanManager.inst.npcTuPo.IsCanBigTuPo(npc.ID))
                        zhuangtaistr = "准备突破";
                    zhuangtaistr += $"(突破率{tupolv}%)";
                }
            }
            return zhuangtaistr;
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
        private static string MakeNPCAtionStr(UINPCData npc)
        {
            string actionstr = NPCAction.ContainsKey(npc.ActionID) ? NPCAction[npc.ActionID] : "未知";
            string placestr = string.Empty;

            //地点在大地图
            Dictionary<int, List<int>> bigDict = NpcJieSuanManager.inst.npcMap.bigMapNPCDictionary;
            int dian = bigDict.Keys.FirstOrDefault(key => bigDict[key].Contains(npc.ID));
            if (dian > 0)
            {
                placestr = "在大地图上";
                var ludian = jsonData.instance.AllMapLuDainType.keys.FirstOrDefault(key => key == dian.ToString());
                if (!string.IsNullOrWhiteSpace(ludian))
                    placestr = "在" + jsonData.instance.AllMapLuDainType[ludian]["LuDianName"].str.ToCN();
            }
            //地点在三级场景
            Dictionary<string, List<int>> threeDict = NpcJieSuanManager.inst.npcMap.threeSenceNPCDictionary;
            string scene = threeDict.Keys.FirstOrDefault(key => threeDict[key].Contains(npc.ID));
            if (!string.IsNullOrEmpty(scene))
                placestr = "在" + jsonData.instance.SceneNameJsonData[scene]["MapName"].str.ToCN();

            //地点在副本
            //foreach (var (fuben, pos) in from string fuben in NpcJieSuanManager.inst.npcMap.fuBenNPCDictionary.Keys
            //                             let fubendict = NpcJieSuanManager.inst.npcMap.fuBenNPCDictionary[fuben]
            //                             from int pos in fubendict.Keys
            //                             where fubendict[pos].Contains(npc.ID)
            //                             select (fuben, pos))
            //{
            //    placestr = "在" + jsonData.instance.SceneNameJsonData[fuben]["MapName"].str.ToCN() + "的第" + pos.ToString() + "位置";
            //    break;
            //}
            Dictionary<string, Dictionary<int, List<int>>> fuBenDict = NpcJieSuanManager.inst.npcMap.fuBenNPCDictionary;
            IEnumerable<(string fuben, int pos)> query = fuBenDict.Keys.SelectMany(fuben => fuBenDict[fuben].Where(fubendict => fubendict.Value.Contains(npc.ID)).Select(fubendict => (fuben, fubendict.Key)));

            if (query.Count() > 0)
                placestr = $"在{jsonData.instance.SceneNameJsonData[query.First().fuben]["MapName"].str.ToCN()}的第{query.First().pos}位置";

            return placestr + actionstr;
        }

        [HarmonyPatch(typeof(UINPCZhuangBeiGongFaPanel))]
        class UINPCZhuangBeiGongFaPanel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(UINPCZhuangBeiGongFaPanel.OnPanelShow))]
            public static void OnPanelShow_Postfix(UINPCZhuangBeiGongFaPanel __instance)
            {
                UINPCInfoPanel NPCInfoPanel = UINPCJiaoHu.Inst.InfoPanel;
                Transform tZhuangBeiGongFaPanel = NPCInfoPanel.transform.Find("Panels/ZhuangBeiGongFaPanel");
                UINPCData npc = Traverse.Create(__instance).Field("npc").GetValue<UINPCData>();
                //Instance.Logger.LogInfo(npc.json.ToString().ToCN());
                if (npc.json.HasField("LiuPai"))
                {
                    //流派
                    tZhuangBeiGongFaPanel.Find("LiuPai").gameObject.SetActive(true);
                    tZhuangBeiGongFaPanel.Find("LiuPai").GetComponent<Text>().text = "流派：" + (NPCLiuPai.ContainsKey(npc.LiuPai) ? NPCLiuPai[npc.LiuPai] : "未知") + (ShowStringInt.Value ? npc.LiuPai.ToString() : "");
                }
                else
                {
                    //遇到怪物没有人的信息
                    tZhuangBeiGongFaPanel.Find("LiuPai").gameObject.SetActive(false);
                }

                if (npc.json.HasField("equipWeaponPianHao"))
                {
                    //偏好
                    tZhuangBeiGongFaPanel.Find("PianHao").gameObject.SetActive(true);
                    tZhuangBeiGongFaPanel.Find("PianHao").GetComponent<PointerItem>().Desc = MakePianHaoStr(npc);
                }
                else
                {
                    //遇到怪物没有人的信息
                    tZhuangBeiGongFaPanel.Find("PianHao").gameObject.SetActive(false);
                }

            }
        }

        private static string MakePianHaoStr(UINPCData npc)
        {
            if (!npc.json.HasField("equipWeaponPianHao"))
                return string.Empty;
            List<int> listWeaponPianHao = npc.json["equipWeaponPianHao"].ToList();
            List<int> listClothingPianHao = npc.json["equipClothingPianHao"].ToList();
            List<int> listRingPianHao = npc.json["equipRingPianHao"].ToList();
            string strPianHao = string.Empty;
            if (listWeaponPianHao.Count > 0)
            {
                strPianHao += "武器偏好：" + Environment.NewLine;
                foreach (int i in listWeaponPianHao)
                {
                    strPianHao += i.ToString() + GetEquipHeChengStr(i);
                    strPianHao += Environment.NewLine;
                }
            }
            if (listClothingPianHao.Count > 0)
            {
                strPianHao += Environment.NewLine + "防具偏好：" + Environment.NewLine;
                foreach (int i in listClothingPianHao)
                {
                    strPianHao += i.ToString() + GetEquipHeChengStr(i);
                    strPianHao += Environment.NewLine;
                }
            }
            if (listRingPianHao.Count > 0)
            {
                strPianHao += Environment.NewLine + "饰品偏好：" + Environment.NewLine;
                foreach (int i in listRingPianHao)
                {
                    strPianHao += i.ToString() + GetEquipHeChengStr(i);
                    strPianHao += Environment.NewLine;
                }
            }

            return strPianHao;

        }
        public static string GetEquipHeChengStr(int id)
        {
            //根据装备属性ID生成中文词条
            JSONObject HeChengBiao = jsonData.instance.LianQiHeCheng;
            JSONObject ShuXingLeiBie = jsonData.instance.LianQiShuXinLeiBie;
            if (HeChengBiao.HasField(id.ToString()))
            {
                string strShuXing = ShuXingLeiBie[HeChengBiao[id.ToString()]["ShuXingType"].ToString()]["desc"].Str;
                string strEquipType = HeChengBiao[id.ToString()]["ZhuShi2"].str;
                string strXiaoGuo = HeChengBiao[id.ToString()]["ZhuShi3"].str;
                return $"{strShuXing}·{strEquipType}·{strXiaoGuo}".ToCN();
            }
            else
                return "未知";
        }

        [HarmonyPatch(typeof(UINPCWuDaoPanel))]
        class UINPCWuDaoPanel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(UINPCWuDaoPanel.OnPanelShow))]
            public static void OnPanelShow_Postfix(UINPCWuDaoPanel __instance)
            {
                //调整【悟道面板】
                //因为每次内容都会清空，只能即时增加一条对象。改名悟道类型
                Transform tWuDaoLeiXing = UnityEngine.Object.Instantiate<GameObject>(__instance.SVItemPrefab, __instance.ContentRT).transform;
                tWuDaoLeiXing.name = "WuDaoLeiXing";
                tWuDaoLeiXing.SetAsFirstSibling();

                UINPCWuDaoSVItem WuDaoSVItem = tWuDaoLeiXing.GetComponent<UINPCWuDaoSVItem>();
                //Instance.Logger.LogInfo(WuDaoSVItem.WuDaoTypeSprites.Count);
                WuDaoSVItem.TypeImage.gameObject.SetActive(false);
                //tWuDaoLeiXing.Find("Head").gameObject.SetActive(false);
                UINPCData npc = Traverse.Create(__instance).Field("npc").GetValue<UINPCData>();
                int wudao = 0;
                if (npc.json.HasField("wudaoType"))
                    wudao = npc.json.GetField("wudaoType").I;
                string strWuDaoLeiXing = (NPCWuDao.ContainsKey(wudao) ? NPCWuDao[wudao] : "未知") + (ShowStringInt.Value ? wudao.ToString() : "");
                WuDaoSVItem.LevelText.text = "悟道类型";
                WuDaoSVItem.SkillText.text = "#s34#cb47a39" + strWuDaoLeiXing;

                GameObject goDao = UnityEngine.Object.Instantiate<GameObject>(WuDaoSVItem.LevelText.gameObject, tWuDaoLeiXing.Find("Title/Head"));
                MakeDaoChar(goDao);
            }
        }
        private static void MakeDaoChar(GameObject goDao)
        {
            goDao.name = "Dao";
            goDao.transform.localPosition = Vector3.zero;
            Text Dao = goDao.GetComponent<Text>();
            Dao.text = "道";
            Dao.color = new Color(132f / 255f, 94f / 255f, 33f / 225f, 1f);
            Dao.fontStyle = FontStyle.Bold;
        }

        [HarmonyPatch(typeof(UINPCWuDaoSVItem))]
        class UINPCWuDaoSVItem_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(UINPCWuDaoSVItem.SetWuDao))]
            public static void SetWuDao_Postfix(UINPCWuDaoSVItem __instance, UINPCWuDaoData data)
            {
                //显示npc每一种悟道的经验
                int nextexp = jsonData.instance.WuDaoJinJieJson[data.Level]["Max"].I;
                string strExp = "#s34#cb47a39经验 #n" + data.Exp.ToString() + (data.Level < 5 ? $"/{nextexp}" : "") + Environment.NewLine;
                __instance.SkillText.text += strExp;

            }
        }

        [HarmonyPatch(typeof(UINPCEventPanel))]
        class UINPCEventPanel_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(UINPCEventPanel.OnPanelShow))]
            public static void OnPanelShow_Postfix(UINPCEventPanel __instance)
            {
                //调整【重要事件面板】
                //因为每次内容都会清空，只能即时增加一条对象。

                //耐药性结果字符串字典
                Dictionary<int, string> DanYaoSeidToNaiYaoStr = new Dictionary<int, string>();

                UINPCData npc = Traverse.Create(__instance).Field("npc").GetValue<UINPCData>();

                if (npc.json.HasField("useItem") && !npc.json["useItem"].IsNull)
                {
                    //遍历每一个已使用过的药
                    JSONObject jsuseItem = npc.json.GetField("useItem");
                    foreach (string item in jsuseItem.keys)
                    {
                        _ItemJsonData ItemJson = _ItemJsonData.DataDict[int.Parse(item)];

                        //获取最大耐药性
                        int maxNaiYao = ItemJson.CanUse;
                        if (npc.json["wuDaoSkillList"].ToList().Contains(2131))
                        {
                            maxNaiYao *= 2;
                        }
                        if (maxNaiYao > 0)
                        {
                            //将药按效果分类，词条加入字典
                            int seid = item == "5523" ? ItemJson.seid[1] : ItemJson.seid[0];//避劫丹有两个效果用第二个，其它丹药用第一个
                            string danyaonaiyao = $"{ItemJson.name}({jsuseItem[item].I}/{maxNaiYao})  ";
                            if (DanYaoSeidToNaiYaoStr.ContainsKey(seid))
                            {
                                DanYaoSeidToNaiYaoStr[seid] += danyaonaiyao;
                            }
                            else
                            {
                                DanYaoSeidToNaiYaoStr.Add(seid, danyaonaiyao);
                            }
                        }
                    }

                    //按耐药性结果字符串字典增加事件对象
                    Transform tNaiYao;
                    if (DanYaoSeidToNaiYaoStr.Count > 0)
                        foreach (KeyValuePair<int, string> strNaiYao in DanYaoSeidToNaiYaoStr)
                        {
                            tNaiYao = UnityEngine.Object.Instantiate<GameObject>(__instance.SVItemPrefab, __instance.ContentRT).transform;
                            tNaiYao.name = "NaiYao";
                            tNaiYao.SetAsFirstSibling();
                            int seidkey = DanYaoSeidToCN.ContainsKey(strNaiYao.Key) ? strNaiYao.Key : 0;
                            tNaiYao.GetComponent<UINPCEventSVItem>().SetEvent(DanYaoSeidToCN[seidkey], strNaiYao.Value);
                        }
                }


            }
        }
        private static Dictionary<int, string> DanYaoSeidToCN = new Dictionary<int, string>()
        {
            {0,"其他药"},
            {4,"修为药"},
            {5,"神识药"},
            {6,"气血药"},
            {7,"寿元药"},
            {9,"资质药"},
            {10,"悟性药"},
            {11,"遁速药"},
            {25,"悟道经验药"},
            {26,"悟道点药"},
            {37,"避劫丹"},
        };
    }
}
