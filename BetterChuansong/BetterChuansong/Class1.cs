using BepInEx;
using BepInEx.Configuration;
using Fungus;
using HarmonyLib;
//using KBEngine;
using System;
using System.Collections.Generic;
using System.Linq;
//using KBEngine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.BetterChuansong", "BetterChuansong", "1.0")]
    public class BetterChuansong : BaseUnityPlugin
    {
        internal struct IdShiliScene
        {
            internal int Id;
            internal string Shili;
            internal string Scene;
            internal string SceneName;
            internal IdShiliScene(int id, string shili, string scene, string scenename)
            {
                Id = id;
                Shili = shili;
                Scene = scene;
                SceneName = scenename;
            }
        }

        List<IdShiliScene> DataList = new List<IdShiliScene>() {
            new IdShiliScene(0, "宁州", "AllMaps", "大地图"),
            new IdShiliScene(1, "竹山宗", "S1", "竹山宗"),
            new IdShiliScene(2, "宁州", "", ""),
            new IdShiliScene(3, "金虹剑派", "S3", "金虹剑派"),
            new IdShiliScene(4, "星河剑派", "S4", "星河剑派"),
            new IdShiliScene(5, "离火门", "S2", "离火门"),
            new IdShiliScene(6, "化尘教", "S5", "化尘教"),

            new IdShiliScene(-1, "", "S6", "东石谷"),
            new IdShiliScene(-1, "御剑门", "S9", "九嶷山"),
            new IdShiliScene(-1, "玄古门", "S10", "巫山"),
            new IdShiliScene(-1, "", "S17", "岫络谷"),
            new IdShiliScene(-1, "", "S25", "天魔眼"),
            new IdShiliScene(-1, "", "S35", "阴魂岛"),//阴冥海
            new IdShiliScene(-1, "万魂殿", "S71", "无名孤岛"),//玄冥海
            //幽冥海
            new IdShiliScene(-1, "星宫", "S1237", "星宫"),
            new IdShiliScene(-1, "玄道宗", "S28", "玄道宗"),

            new IdShiliScene(7, "武陵城", "S19", "武陵城"),
            new IdShiliScene(8, "天星城", "S23", "天星城"),
            new IdShiliScene(9, "广陵城", "S29", "广陵城"),
            new IdShiliScene(10, "风雨楼", "S22", "云汐城"),
            new IdShiliScene(11, "天机阁", "S21", "天机阁"),
            new IdShiliScene(12, "逸风城", "S20", "逸风城"),
            new IdShiliScene(13, "天魔道", "", ""),
            new IdShiliScene(14, "古神教", "", ""),
            new IdShiliScene(-1, "血剑宫", "", ""),
            new IdShiliScene(15, "灵药堂", "S19", "武陵城"),
            new IdShiliScene(16, "中草阁", "S22", "云汐城"),
            new IdShiliScene(17, "万宝阁", "S29", "广陵城"),
            new IdShiliScene(18, "神兵阁", "S20", "逸风城"),

            new IdShiliScene(19, "无尽之海", "S30", "南崖城"),
            new IdShiliScene(20, "蓬莎岛", "S31", "蓬莎岛"),
            new IdShiliScene(21, "碎星岛", "S32", "碎星岛"),
            new IdShiliScene(22, "千流岛", "S33", "千流岛"),
            new IdShiliScene(23, "龙族", "S34", "龙宫"),
            new IdShiliScene(24, "白帝楼", "S19", "武陵城"),

            new IdShiliScene(86, "黑煞教", "S29", "广陵城"),
            new IdShiliScene(87, "沂山派", "S7", "沂山"),
            new IdShiliScene(88, "禾山道", "S8", "禾山"),
            new IdShiliScene(799, "千竹教", "S26", "青石灵脉"),

            new IdShiliScene(-1, "", "S101", "洞府"),
            new IdShiliScene(-1, "", "S101", "门派洞府"),
            new IdShiliScene(-1, "", "S101", "莲池洞府"),
        };
        List<string> NewNingzhouWarp = new List<string>() {
            "武陵城",
            "天星城",
            "广陵城",
            "云汐城",
            "逸风城",

            "竹山宗",
            "金虹剑派",
            "星河剑派",
            "离火门",
            "化尘教",

            "东石谷",
            "青石灵脉",
            "天机阁",

            "九嶷山",
            "巫山",
            "沂山",
            "禾山",

            "洞府",
            "门派洞府",
            "莲池洞府"
        };
        List<string> NewSeaWarp = new List<string>() {
            "南崖城",
            "蓬莎岛",
            "碎星岛",
            "千流岛",
            "龙宫",
            //"阴魂岛",
            //"无名孤岛"
        };

        List<string> CanWarpSceneNameList = new List<string>();

        List<string> SiShan = new List<string>() {
            "九嶷山",
            "巫山",
            "沂山",
            "禾山",
        };
        List<string> SanDi = new List<string>() {
            "东石谷",
            "青石灵脉",
            "天机阁",
        };

        public bool IsSceneInWarpList(string ListName)
        {
            string sname = Tools.getScreenName();
            bool InList = false;
            switch (ListName)
            {
                case "Ningzhou":
                    InList = Instance.NewNingzhouWarp.Exists(x => Instance.DataList.Find(y => y.SceneName == x).Scene == sname);
                    break;
                case "Sea":
                    InList = Instance.NewSeaWarp.Exists(x => Instance.DataList.Find(y => y.SceneName == x).Scene == sname);
                    break;
                case "SiShan":
                    InList = Instance.SiShan.Exists(x => Instance.DataList.Find(y => y.SceneName == x).Scene == sname);
                    break;
                case "SanDi":
                    InList = Instance.SanDi.Exists(x => Instance.DataList.Find(y => y.SceneName == x).Scene == sname);
                    break;
                case "Can":
                    UpdateCanWarpSceneNameList();
                    InList = Instance.CanWarpSceneNameList.Exists(x => Instance.DataList.Find(y => y.SceneName == x).Scene == sname);
                    break;
            }
            return InList;
        }

        ConfigEntry<bool> AlwaysShowWarp;
        //ConfigEntry<bool> WarpSpendTime;
        ConfigEntry<bool> NingzhouWarpCostIncrease;
        ConfigEntry<int> NingzhouWarpTime;
        ConfigEntry<int> SeaWarpCost;
        ConfigEntry<int> SeaWarpTime;
        ConfigEntry<int> InterstateWarpCost;
        ConfigEntry<int> InterstateWarpTime;
        ConfigEntry<bool> FlightAddDays;
        //ConfigEntry<bool> UnitKilometer;
        ConfigEntry<bool> Enter5School;
        void Start()
        {
            //输出日志
            Logger.LogInfo("更好的传送加载成功！");
            var harmony = new Harmony("Ventulus.MCS.BetterChuansong");
            harmony.PatchAll();
            AlwaysShowWarp = Config.Bind<bool>("config", "AlwaysShowWarp", true, "总是显示传送阵，若为true，则无论是否可用都显示传送阵按钮；若为false，则只有在此传送阵可用时才显示按钮");
            //WarpSpendTime = Config.Bind<bool>("config", "WarpSpendTime", true, "传送花费时间，若为true，则使用传送阵会花费时间；若为false，则改为瞬间传送");
            NingzhouWarpCostIncrease = Config.Bind<bool>("config", "NingzhouWarpCostIncrease", true, "宁州传送花费按境界增加，若为true，则炼气10筑基100金丹300元婴600；若为false，则传送只花费10灵石");
            NingzhouWarpTime = Config.Bind<int>("config", "NingzhouWarpTime", 1, "宁州传送时间，默认1天");
            SeaWarpCost = Config.Bind<int>("config", "SeaWarpCost", 3000, "海域传送花费，默认3000灵石");
            SeaWarpTime = Config.Bind<int>("config", "SeaWarpTime", 30, "海域传送时间，默认30天");
            InterstateWarpCost = Config.Bind<int>("config", "InterstateWarpCost", 6000, "跨州传送花费，默认6000灵石");
            InterstateWarpTime = Config.Bind<int>("config", "InterstateWarpTime", 60, "跨州传送时间，默认60天");
            FlightAddDays = Config.Bind<bool>("config", "FlightAddDays", true, "在宁州用遁术飞行增加花费天数，默认开启");
            //UnitKilometer = Config.Bind<bool>("config", "UnitKilometer", false, "在显示飞行距离时增加单位公里，默认关闭");
            Enter5School = Config.Bind<bool>("config", "Enter5School", false, "可传送进入五大门派，默认关闭");
        }

        public static BetterChuansong Instance;
        void Awake()
        {
            Instance = this;
        }

        void UpdateCanWarpSceneNameList()
        {
            CanWarpSceneNameList = new List<string>();
            //加入门派必定可传送  
            int menPai = PlayerEx.Player.menPai;
            if (menPai > 0 && PlayerEx.GetShengWang(menPai) > 0)
            {
                Instance.CanWarpSceneNameList.Add(Instance.DataList.Find(y => y.Id == menPai).SceneName);
                Instance.Logger.LogInfo("玩家门派" + PlayerEx.Player.menPai + "当前场景" + Tools.getScreenName());
            }

            //三势力声望略有薄名100
            void ShiliShengWang100(string shili)
            {
                int ShengWang = PlayerEx.GetShengWang(Instance.DataList.Find(y => y.Shili == shili).Id);
                if (ShengWang >= 100)
                {
                    //Instance.Logger.LogInfo(shili + "声望" + ShengWang);
                    Instance.CanWarpSceneNameList.Add(Instance.DataList.Find(y => y.Shili == shili).SceneName);
                }
            }
            ShiliShengWang100("龙族");
            ShiliShengWang100("白帝楼");
            ShiliShengWang100("风雨楼");

            //洞府
            if (DongFuManager.PlayerHasDongFu(1))
            {
                int level = PlayerEx.Player.DongFuData["DongFu1"]["LingYanLevel"].I;
                if (level >= 2)
                {
                    Instance.CanWarpSceneNameList.Add("洞府");
                    Instance.CanWarpSceneNameList.Add(DongFuManager.GetDongFuName(1));
                }

            }

            if (DongFuManager.PlayerHasDongFu(2))
            {
                int level = PlayerEx.Player.DongFuData["DongFu2"]["LingYanLevel"].I;
                if (level >= 2)
                {
                    Instance.CanWarpSceneNameList.Add("门派洞府");
                    Instance.CanWarpSceneNameList.Add(DongFuManager.GetDongFuName(2));
                }

            }

            if (DongFuManager.PlayerHasDongFu(3))
            {
                int level = PlayerEx.Player.DongFuData["DongFu3"]["LingYanLevel"].I;
                if (level >= 2)
                {
                    Instance.CanWarpSceneNameList.Add("莲池洞府");
                    Instance.CanWarpSceneNameList.Add(DongFuManager.GetDongFuName(3));
                }

            }

            int NingZhouShengWangLevel = PlayerEx.GetNingZhouShengWangLevel();
            //声名远扬500进入五大门派
            if (NingZhouShengWangLevel >= 6 && Enter5School.Value)
            {
                Instance.CanWarpSceneNameList.Add("竹山宗");
                Instance.CanWarpSceneNameList.Add("金虹剑派");
                Instance.CanWarpSceneNameList.Add("星河剑派");
                Instance.CanWarpSceneNameList.Add("离火门");
                Instance.CanWarpSceneNameList.Add("化尘教");
            }
            //声望高于声名狼藉-50，可使用五主城传送阵
            if (NingZhouShengWangLevel >= 3)
            {
                Instance.CanWarpSceneNameList.Add("武陵城");
                Instance.CanWarpSceneNameList.Add("天星城");
                Instance.CanWarpSceneNameList.Add("广陵城");
                Instance.CanWarpSceneNameList.Add("云汐城");
                Instance.CanWarpSceneNameList.Add("逸风城");
            }
            //声望低于等于声名狼藉-50，可使用四山传送阵
            if (NingZhouShengWangLevel < 3)
            {
                Instance.CanWarpSceneNameList.AddRange(Instance.SiShan);
            }
            int PlayerLevelType = PlayerEx.Player.getLevelType();
            //按境界开启三地
            if (PlayerLevelType >= 1)
                Instance.CanWarpSceneNameList.Add("东石谷");
            if (PlayerLevelType >= 2)
                Instance.CanWarpSceneNameList.Add("青石灵脉");
            if (PlayerLevelType >= 3)
                Instance.CanWarpSceneNameList.Add("天机阁");


            //海域声望大于等于声名远扬500，或者高于等于元婴期，可使用四岛传送阵
            int SeaShengWangLevel = PlayerEx.GetSeaShengWangLevel();

            if (SeaShengWangLevel >= 6 || PlayerLevelType >= 4)
            {
                Instance.CanWarpSceneNameList.Add("南崖城");
                Instance.CanWarpSceneNameList.Add("蓬莎岛");
                Instance.CanWarpSceneNameList.Add("碎星岛");
                Instance.CanWarpSceneNameList.Add("千流岛");
            }


            //去重
            List<string> Dislist = Instance.CanWarpSceneNameList.Distinct().ToList();
            Instance.CanWarpSceneNameList = Dislist;
        }


        [HarmonyPatch(typeof(SceneBtnMag))]
        class SceneBtnMagPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Init")]
            public static void InitPostfix(SceneBtnMag __instance)
            {
                //Instance.Logger.LogInfo("更好的传送：修改场景按钮初始化");
                string sname = Tools.getScreenName();
                Instance.Logger.LogInfo("激活场景" + sname);


                if (Instance.IsSceneInWarpList("Ningzhou"))
                {
                    Dictionary<string, FpBtn> btnDictionary = Traverse.Create(__instance).Field("btnDictionary").GetValue<Dictionary<string, FpBtn>>();
                    FpBtn btn = btnDictionary["chuansong"];
                    btn.mouseUpEvent.RemoveAllListeners();
                    btn.mouseUpEvent.AddListener(delegate ()
                    {
                        UIMapPanel.Inst.NingZhou.FungusNowScene = sname;
                        UIMapPanel.Inst.OpenMap(MapArea.NingZhou, UIMapState.Warp);
                    });
                    if (Instance.AlwaysShowWarp.Value || Instance.IsSceneInWarpList("Can"))
                    {
                        btn.gameObject.SetActive(true);
                        Instance.Logger.LogInfo("增加传送按钮" + sname);
                    }
                    else
                    {
                        btn.gameObject.SetActive(false);
                    }

                }
                else if (Instance.IsSceneInWarpList("Sea"))
                {
                    Dictionary<string, FpBtn> btnDictionary = Traverse.Create(__instance).Field("btnDictionary").GetValue<Dictionary<string, FpBtn>>();
                    FpBtn btn = btnDictionary["chuansong"];
                    btn.mouseUpEvent.RemoveAllListeners();
                    btn.mouseUpEvent.AddListener(delegate ()
                    {
                        UIMapPanel.Inst.OpenMap(MapArea.Sea, UIMapState.Warp);
                    });
                    if (Instance.AlwaysShowWarp.Value || Instance.IsSceneInWarpList("Can"))
                    {
                        btn.gameObject.SetActive(true);
                        Instance.Logger.LogInfo("增加传送按钮" + sname);
                    }
                    else
                    {
                        btn.gameObject.SetActive(false);
                    }
                }
            }
        }

        struct Dongfu2Pos
        {
            internal int Id;
            internal string HouShan;
            internal Vector3 Pos;
            internal int MapIndex;
            internal Dongfu2Pos(int id, string houshan, Vector3 pos = new Vector3(), int mapindext = 98)
            {
                Id = id;
                HouShan = houshan;
                Pos = pos;
                MapIndex = mapindext;
            }
        }

        List<Dongfu2Pos> Dongfu2Data = new List<Dongfu2Pos>()
        {
            new Dongfu2Pos(1,"方壶山",new Vector3(820,130,0),12),
            new Dongfu2Pos(3,"连石山",new Vector3(1360,650,0),14),
            new Dongfu2Pos(4,"风雷谷",new Vector3(235,590,0),16),
            new Dongfu2Pos(5,"沃焦山",new Vector3(1180,650,0),15),
            new Dongfu2Pos(6,"正阳山",new Vector3(210,420,0),11),
            new Dongfu2Pos(86,"洞府",new Vector3(260,180,0),29),
        };



        [HarmonyPatch(typeof(UIMapPanel))]
        class UIMapPanelPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OpenMap")]
            public static bool OpenMapPrefix(UIMapPanel __instance)
            {
                Instance.Logger.LogInfo("UIMapPanelOpenMapPre");

                KBEngine.Avatar Player = PlayerEx.Player;
                if (Player == null)
                { Instance.Logger.LogInfo("玩家未加载"); }
                else
                {
                    Dongfu2Pos? menPaiHouShan = Instance.Dongfu2Data.Find(y => y.Id == Player.menPai);

                    if (Player.menPai > 0 && menPaiHouShan != null)
                    {
                        GameObject oDongfu2 = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/NingZhou/NingZhouNodes/洞府2");

                        if (oDongfu2 == null)
                        {
                            List<UIMapNingZhouNode> nodes = Traverse.Create(__instance.NingZhou).Field("nodes").GetValue<List<UIMapNingZhouNode>>();
                            UIMapNingZhouNode Houshan = nodes.Find(x => x.gameObject.name == menPaiHouShan?.HouShan);
                            if (Houshan == null)
                                Houshan = nodes.Find(x => x.gameObject.name == "洞府");
                            UIMapNingZhouNode dongfu2 = GameObject.Instantiate(Houshan, Houshan.transform.parent);
                            dongfu2.name = "洞府2";
                            dongfu2.Init();

                            dongfu2.transform.localScale = new Vector3(0.8F, 0.8F, 0.8F);

                            nodes.Add(dongfu2);
                            Traverse.Create(__instance.NingZhou).Field("nodes").SetValue(nodes);
                        }
                        oDongfu2 = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/NingZhou/NingZhouNodes/洞府2");
                        oDongfu2.transform.localPosition = Instance.Dongfu2Data.Find(y => y.Id == Player.menPai).Pos;
                        //没改图像，所以如果先后加载不同门派的人，会发现门派小图没变
                        if (DongFuManager.PlayerHasDongFu(2))
                        {
                            oDongfu2.GetComponent<UIMapNingZhouNode>().SetNodeName(DongFuManager.GetDongFuName(2));
                            oDongfu2.SetActive(true);
                        }
                        else
                        {
                            oDongfu2.SetActive(false);
                        }
                    }


                    GameObject oDongfu3 = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/NingZhou/NingZhouNodes/洞府3");
                    if (oDongfu3 == null)
                    {
                        List<UIMapNingZhouNode> nodes = Traverse.Create(__instance.NingZhou).Field("nodes").GetValue<List<UIMapNingZhouNode>>();
                        UIMapNingZhouNode Dongshi = nodes.Find(x => x.gameObject.name == "东石谷");
                        UIMapNingZhouNode dongfu3 = GameObject.Instantiate(Dongshi, Dongshi.transform.parent);
                        dongfu3.name = "洞府3";
                        dongfu3.Init();

                        dongfu3.transform.localScale = new Vector3(0.8F, 0.8F, 0.8F);
                        dongfu3.transform.localPosition = new Vector3(1065, 235, 0);
                        nodes.Add(dongfu3);
                        Traverse.Create(__instance.NingZhou).Field("nodes").SetValue(nodes);
                    }
                    oDongfu3 = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/NingZhou/NingZhouNodes/洞府3");
                    if (DongFuManager.PlayerHasDongFu(3))
                    {
                        oDongfu3.GetComponent<UIMapNingZhouNode>().SetNodeName(DongFuManager.GetDongFuName(3));
                        oDongfu3.SetActive(true);
                    }
                    else
                    {
                        oDongfu3.SetActive(false);
                    }

                }



                string sname = Tools.getScreenName();
                Instance.UpdateCanWarpSceneNameList();
                if (__instance.NowState == UIMapState.Warp)
                {
                    if (Instance.IsSceneInWarpList("Can"))
                    {
                        return true;
                    }
                    else if (sname == "S34")
                    {
                        Tools.Say("周围的龙族守卫表示我得在龙族略有薄名，才能使用这个传送阵。", 1);
                        return false;
                    }
                    else if (sname == "S101")
                    {
                        Tools.Say("需要将灵眼升级到中品，才能使用这个传送阵。", 1);
                        return false;
                    }
                    else if (Instance.IsSceneInWarpList("SanDi"))
                    {
                        Tools.Say("只有突破更高的境界，才能使用这个传送阵。", 1);
                        return false;
                    }
                    else if (Instance.IsSceneInWarpList("Sea"))
                    {
                        Tools.Say("周围的海岛守卫表示我得在海域声名远扬，或者达到元婴境界，才能使用这个传送阵。", 1);
                        return false;
                    }
                    else if (Instance.IsSceneInWarpList("SiShan"))
                    {
                        Tools.Say("此处传送阵似为邪派人士建立，只有在宁州声名狼藉，才能使用这个传送阵。", 1);
                        return false;
                    }
                    else if (Instance.IsSceneInWarpList("Ningzhou"))
                    {
                        Tools.Say("只有提高我的声望，才能使用这个传送阵。", 1);
                        return false;
                    }
                }
                return true;

            }

            [HarmonyPostfix]
            [HarmonyPatch("OpenMap")]
            public static void OpenMapPostfix(UIMapPanel __instance)
            {
                Instance.Logger.LogInfo("UIMapPanelOpenMapPost");
                //所有打开地图都修改为宁州海域两标签都能看
                bool show = true;
                Traverse.Create(__instance).Method("SetTabShow", new object[] { show }).GetValue(new object[] { show });
                /*
                int menPai = PlayerEx.Player.menPai;
                Instance.Logger.LogInfo("玩家门派" + menPai);
                JSONObject ShiLiNameData = jsonData.instance.CyShiLiNameData;
                Instance.Logger.LogInfo("势力名称" + Regex.Unescape(ShiLiNameData.ToString()));
                JSONObject MenPaiHaoGanDu = PlayerEx.Player.MenPaiHaoGanDu;
                Instance.Logger.LogInfo("势力好感度" + MenPaiHaoGanDu.ToString());
                */
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnNingZhouTabClick")]
            public static bool OnNingZhouTabClickPrefix(UIMapPanel __instance)
            {
                Instance.Logger.LogInfo("UIMapPanelOnNingZhouTabClick");
                __instance.NowArea = MapArea.NingZhou;
                bool show = true;
                Traverse.Create(__instance).Method("SetTabShow", new object[] { show }).GetValue(new object[] { show });
                __instance.NingZhou.RefreshDongFuNode();
                if (SceneEx.NowSceneName.StartsWith("Sea"))
                {
                    __instance.NowState = UIMapState.Normal;
                }
                Traverse.Create(__instance).Method("ShowPanel").GetValue();
                Instance.Logger.LogInfo(__instance.NowState.ToString());
                return false;
            }
            //仅在海上原版打开传送图，切换查看宁州要改状态为普通图，其他情况在宁州打开传送图、打开普通图、或在海岛打开传送图、打开普通图，都是状态保持不变。
            [HarmonyPrefix]
            [HarmonyPatch("OnSeaTabClick")]
            public static bool OnSeaTabClickPrefix(UIMapPanel __instance)
            {
                Instance.Logger.LogInfo("UIMapPanelOnSeaTabClick");
                __instance.NowArea = MapArea.Sea;
                bool show = true;
                Traverse.Create(__instance).Method("SetTabShow", new object[] { show }).GetValue(new object[] { show });
                if (SceneEx.NowSceneName.StartsWith("Sea"))
                {
                    __instance.NowState = UIMapState.Warp;
                }
                Traverse.Create(__instance).Method("ShowPanel").GetValue();
                Instance.Logger.LogInfo(__instance.NowState.ToString());
                return false;
            }
        }

        public static UnityAction PayAndWarp(int cost, int costtime, string warpscene)
        {
            KBEngine.Avatar player = PlayerEx.Player;
            if (player.money >= (uint)cost)
            {
                player.AddMoney(-cost);
                AvatarTransfer.Do(int.Parse(warpscene.Replace("S", "")));
                if (warpscene == "S101")
                {
                    //若传送到洞府，则大地图上小人位置要特殊设置，普通情况下是和场景S对应的
                    switch (DongFuManager.NowDongFuID)
                    {
                        case 1:
                            Tools.instance.getPlayer().NowMapIndex = 98;
                            break;
                        case 2:
                            Tools.instance.getPlayer().NowMapIndex = Instance.Dongfu2Data.Find(x => x.Id == player.menPai).MapIndex;
                            break;
                        case 3:
                            Tools.instance.getPlayer().NowMapIndex = 101;
                            break;
                    }
                };
                UIMapPanel.Inst.HidePanel();
                if (costtime > 0)
                {
                    PlayerEx.Player.AddTime(costtime, 0, 0);
                    UIAnimProgressBar.Show(new ProgressBarShowData("正在传送...", delegate ()
                    {
                        Tools.instance.loadMapScenes(warpscene, true);
                    }, ProgressBarType.Normal));
                }
                else
                {
                    Tools.instance.loadMapScenes(warpscene, true);
                }
            }
            else
            {
                //USelectBox.Show("灵石不足!在宁州使用传送阵，炼气需要10灵石，筑基需要100灵石，金丹需要300灵石,，在海岛使用传送阵需要3000灵石，跨州传送需要6000灵石", null, null);
                UIPopTip.Inst.Pop("灵石不足!", PopTipIconType.叹号);
            }
            return null;
        }

        public static UnityAction ChangeDongFu(string obname)
        {
            //要传洞府前确认进哪个
            if (obname == "洞府")
            {
                DongFuManager.NowDongFuID = 1;
            }
            else if (obname == "洞府2")
            {
                DongFuManager.NowDongFuID = 2;
            }
            else if (obname == "洞府3")
            {
                DongFuManager.NowDongFuID = 3;
            }
            return null;
        }
        public static bool DaDaoYiCheng()
        {
            JSONObject wuDaoJson = PlayerEx.Player.WuDaoJson;
            //Instance.Logger.LogInfo(wuDaoJson.ToString());
            bool dadaoyicheng = false;
            for (int dao = 1; dao <= 9; dao++)
            {
                int ex = wuDaoJson[dao.ToString()]["ex"].I;
                if (ex >= 150000)
                    dadaoyicheng = true;
            }
            return dadaoyicheng;
        }

        public static void KuaZhouWarp(int PlayerLevelType, string NodeName, string WarpScene, string obname = "")
        {
            int costtime = Instance.InterstateWarpTime.Value;
            int cost = Instance.InterstateWarpCost.Value;
            string CostText = string.Format("是否花费{0}灵石和{1}天传送至{2}？", cost.ToString(), costtime.ToString(), NodeName);

            if (PlayerLevelType >= 5 || PlayerEx.GetSeaShengWangLevel() >= 7 || PlayerEx.GetNingZhouShengWangLevel() >= 7)
            {
                USelectBox.Show(CostText, delegate ()
                {
                    ChangeDongFu(obname);
                    PayAndWarp(cost, costtime, WarpScene);
                    //如果从宁州跨州到海域，需将宁州面板的位置修改，避免不能返回
                    if (Instance.NewSeaWarp.Contains(NodeName))
                    {
                        Instance.Logger.LogInfo("从宁州跨州到海域");
                        PlayerEx.Player.NowMapIndex = 29;
                        UIMapPanel.Inst.NingZhou.FungusNowScene = WarpScene;
                        PlayerEx.Player.NowFuBen = Instance.SeaSceneToSea[WarpScene];
                        PlayerEx.Player.fubenContorl[WarpScene].NowIndex = Instance.SeaSceneToIndex[WarpScene];
                    }
                },
                null);
            }
            else
            {
                USelectBox.Show("要承受住跨州传送的空间乱流，需要化神境界，或者宁州或海域声望达到誉满天下，由大势力出手协助跨州传送", null, null);
            }
        }


        [HarmonyPatch(typeof(UIMapNingZhou))]
        class UIMapNingZhouPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Show")]
            public static void ShowPrefix(UIMapNingZhou __instance)
            {
                Instance.Logger.LogInfo("UIMapNingZhouShow");
                Instance.UpdateCanWarpSceneNameList();


                List<UIMapNingZhouNode> nodes = Traverse.Create(__instance).Field("nodes").GetValue<List<UIMapNingZhouNode>>();
                List<UIMapNingZhouNode> warpNodes = Traverse.Create(__instance).Field("warpNodes").GetValue<List<UIMapNingZhouNode>>();
                warpNodes = new List<UIMapNingZhouNode>();

                foreach (UIMapNingZhouNode uimapNingZhouNode in nodes)
                {
                    uimapNingZhouNode.WarpSceneName = string.Empty;
                    if (Instance.CanWarpSceneNameList.Exists(x => x == uimapNingZhouNode.NodeName))
                    {
                        if (uimapNingZhouNode.gameObject.name.Contains("洞府"))
                        {
                            uimapNingZhouNode.WarpSceneName = "S101";
                        }
                        else
                        {
                            uimapNingZhouNode.WarpSceneName = Instance.DataList.Find(y => y.SceneName == uimapNingZhouNode.NodeName).Scene;
                        }

                        warpNodes.Add(uimapNingZhouNode);
                    }

                    //Instance.Logger.LogInfo(uimapNingZhouNode.NodeName + uimapNingZhouNode.WarpSceneName);
                }
                Traverse.Create(__instance).Field("nodes").SetValue(nodes);
                Traverse.Create(__instance).Field("warpNodes").SetValue(warpNodes);

            }

            [HarmonyPostfix]
            [HarmonyPatch("Show")]
            public static void ShowPostfix(UIMapNingZhou __instance)
            {
                //因原版官方show只根据场景名判断当前场景和对应节点一致时不能交互，没考虑到多个洞府传送，因此这里通过objectname来区分非当前洞府可交互。
                List<UIMapNingZhouNode> nodes = Traverse.Create(__instance).Field("nodes").GetValue<List<UIMapNingZhouNode>>();
                foreach (UIMapNingZhouNode uimapNingZhouNode in nodes)
                {
                    if (uimapNingZhouNode.gameObject.name.Contains("洞府"))
                    {
                        string sname = Tools.getScreenName();
                        string tryoname = "洞府" + (DongFuManager.NowDongFuID == 1 ? "" : DongFuManager.NowDongFuID.ToString());
                        if (sname == "S101" && uimapNingZhouNode.gameObject.name == tryoname)
                        {
                            uimapNingZhouNode.SetCanJiaoHu(false);
                        }
                        else
                            uimapNingZhouNode.SetCanJiaoHu(true);
                        if (UIMapPanel.Inst.NowState != UIMapState.Warp)
                            uimapNingZhouNode.SetCanJiaoHu(false);
                    }
                }
                Traverse.Create(__instance).Field("nodes").SetValue(nodes);
            }


            [HarmonyPostfix]
            [HarmonyPatch("RefreshDongFuNode")]
            public static void RefreshDongFuNodePost()
            {
                //对2号3号洞府设置是否显示、名称
                GameObject oDongfu2 = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/NingZhou/NingZhouNodes/洞府2");
                if (oDongfu2)
                {
                    if (PlayerEx.Player != null && DongFuManager.PlayerHasDongFu(2))
                    {
                        oDongfu2.GetComponent<UIMapNingZhouNode>().SetNodeName(DongFuManager.GetDongFuName(2));
                        oDongfu2.SetActive(true);
                    }
                    else
                        oDongfu2.SetActive(false);
                }

                GameObject oDongfu3 = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/NingZhou/NingZhouNodes/洞府3");
                if (oDongfu3)
                {
                    if (PlayerEx.Player != null && DongFuManager.PlayerHasDongFu(3))
                    {
                        oDongfu3.GetComponent<UIMapNingZhouNode>().SetNodeName(DongFuManager.GetDongFuName(3));
                        oDongfu3.SetActive(true);
                    }
                    else
                        oDongfu3.SetActive(false);
                }


            }


            [HarmonyPostfix]
            [HarmonyPatch("Init")]
            public static void InitPostfix()
            {
                Instance.Logger.LogInfo("UIMapNingZhouInit");
                GameObject oJinhongjianpai = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/NingZhou/NingZhouNodes/金虹剑派");
                if (oJinhongjianpai)
                {
                    if (oJinhongjianpai.GetComponent<UIMapNingZhouNode>().NodeName != "金虹剑派")
                    {
                        Instance.Logger.LogInfo("Find 金虹剑派 尝试修复节点名称问题");
                        oJinhongjianpai.GetComponent<UIMapNingZhouNode>().NodeName = "金虹剑派";
                    }
                }


            }



            [HarmonyPrefix]
            [HarmonyPatch("OnNodeClick")]
            public static bool OnNodeClickPrefix(UIMapNingZhouNode node)
            {

                Instance.Logger.LogInfo("UIMapNingZhouOnNodeClick");
                string sname = Tools.getScreenName();
                if (UIMapPanel.Inst.NowState == UIMapState.Warp)
                {

                    int PlayerLevelType = PlayerEx.Player.getLevelType();
                    int cost = 10;
                    if (Instance.NingzhouWarpCostIncrease.Value == false || PlayerLevelType == 1)
                        cost = 10;
                    else if (PlayerLevelType == 2)
                        cost = 100;
                    else if (PlayerLevelType == 3)
                        cost = 300;
                    else if (PlayerLevelType >= 4)
                        cost = 600;
                    int costtime = Instance.NingzhouWarpTime.Value;
                    string CostText = string.Format("是否花费{0}灵石和{1}天传送至{2}？", cost.ToString(), costtime.ToString(), node.NodeName);



                    if (SceneEx.NowSceneName.StartsWith("Sea"))
                    {
                        UIPopTip.Inst.Pop("无法从海面上直接旅行至宁州内地！", PopTipIconType.叹号);
                        return false;
                    }
                    if (Instance.IsSceneInWarpList("Sea"))
                    {
                        //从海域跨州传送到宁州
                        KuaZhouWarp(PlayerLevelType, node.NodeName, node.WarpSceneName, node.gameObject.name);
                    }
                    else
                    {
                        //普通在宁州传送
                        USelectBox.Show(CostText, delegate () { ChangeDongFu(node.gameObject.name); PayAndWarp(cost, costtime, node.WarpSceneName); }, null);
                    }

                }
                return false;
            }
        }

        Dictionary<string, int> SeaSceneToIndex = new Dictionary<string, int>() {
            { "S30",6976 },
            { "S31",1452 },
            { "S32",5142 },
            { "S33",3217 },
            { "S29",200 },
            { "S34",5831 }
        };

        Dictionary<string, string> SeaSceneToSea = new Dictionary<string, string>() {
            { "S30","Sea6" },
            { "S31","Sea8" },
            { "S32","Sea7" },
            { "S33","Sea5" },
            { "S29","Sea2" },
            { "S34","Sea17" }
        };

        [HarmonyPatch(typeof(UIMapSea))]
        class UIMapSeaPatch
        {
            //正常情况下warp模式打开海图必定在海上，此Show方法内会以现场景名索引副本json中的Index，以在循环中让当前海域不可交互，且计算到达其他海岛花费的时间，
            //如果是陆地上打开warp海图就会报错没此对象，因此要编造这个json对象
            [HarmonyPrefix]
            [HarmonyPatch("Show")]
            public static void ShowPrefix(UIMapSea __instance)
            {
                Instance.Logger.LogInfo("UIMapSeaShow");
                Instance.UpdateCanWarpSceneNameList();
                string sname = Tools.getScreenName();
                List<UIMapSeaNode> nodes = Traverse.Create(__instance).Field("nodes").GetValue<List<UIMapSeaNode>>();

                if (!PlayerEx.Player.FuBen.HasField(sname))
                {
                    PlayerEx.Player.FuBen.AddField(sname, new JSONObject(JSONObject.Type.OBJECT));
                    PlayerEx.Player.FuBen[sname].AddField("NowIndex", new int());
                }

                if (Instance.IsSceneInWarpList("Sea"))
                {
                    PlayerEx.Player.fubenContorl[Tools.getScreenName()].NowIndex = Instance.SeaSceneToIndex[sname];
                }


                //龙族声望大于一百则显示海图龙宫
                /*
                foreach (UIMapSeaNode uimapSeaNode in nodes)
                {
                    if (uimapSeaNode.NodeName == "龙宫")
                    {
                        uimapSeaNode.AlwaysShow = PlayerEx.GetShengWang(23) >= 100;
                        uimapSeaNode.RefreshUI();
                    }
                }
                */
            }

            [HarmonyPostfix]
            [HarmonyPatch("Init")]
            public static void InitPostfix(UIMapSea __instance)
            {
                Instance.Logger.LogInfo("UIMapSeaInit");
                List<UIMapSeaNode> nodes = Traverse.Create(__instance).Field("nodes").GetValue<List<UIMapSeaNode>>();


                GameObject oNanyacheng = GameObject.Find("NewUICanvas(Clone)/UIMap(Clone)/Scale/Root/Sea/SeaNodes/南崖城");
                if (oNanyacheng)
                {
                    if (oNanyacheng.GetComponent<UIMapSeaNode>().NodeName != "南崖城")
                    {
                        Instance.Logger.LogInfo("Find 南崖城 尝试修复节点名称问题");
                        oNanyacheng.GetComponent<UIMapSeaNode>().NodeName = "南崖城";
                    }

                }

            }

            [HarmonyPrefix]
            [HarmonyPatch("OnNodeClick")]
            public static bool OnNodeClickPrefix(UIMapSeaNode node)
            {
                Instance.Logger.LogInfo("UIMapSeaOnNodeClick");
                string sname = Tools.getScreenName();
                if (UIMapPanel.Inst.NowState == UIMapState.Warp && SceneEx.NowSceneName.StartsWith("Sea"))
                {
                    //原版海域移动
                    return true;
                }
                if (UIMapPanel.Inst.NowState == UIMapState.Normal)
                {
                    UIPopTip.Inst.Pop("普通地图不能传送！", PopTipIconType.叹号);
                }
                else if (UIMapPanel.Inst.NowState == UIMapState.Warp)
                {
                    //只有在地图传送模式下，则拦截原方法改用新传送         
                    int PlayerLevelType = PlayerEx.Player.getLevelType();
                    int cost = Instance.SeaWarpCost.Value;
                    int costtime = Instance.SeaWarpTime.Value;
                    string CostText = string.Format("是否花费{0}灵石和{1}天传送至{2}？", cost.ToString(), costtime.ToString(), node.NodeName);

                    if (SceneEx.NowSceneName.StartsWith("Sea"))
                    {
                        //海上出发只能用原版快速
                        return true;
                    }
                    if (node.NodeName == "龙宫" && PlayerEx.GetShengWang(Instance.DataList.Find(y => y.Shili == "龙族").Id) < 100)
                    {
                        USelectBox.Show("传送至龙宫需要龙族声望100", null, null);
                        UIPopTip.Inst.Pop("声望不足!", PopTipIconType.叹号);
                    }
                    else if (Instance.IsSceneInWarpList("Ningzhou"))
                    {
                        //从宁州跨州传送到海域
                        KuaZhouWarp(PlayerLevelType, node.NodeName, node.WarpSceneName);

                    }
                    else if (Instance.IsSceneInWarpList("Can"))
                    {
                        Instance.Logger.LogInfo("普通在海域传送");

                        USelectBox.Show(CostText, delegate () { PayAndWarp(cost, costtime, node.WarpSceneName); PlayerEx.Player.NowFuBen = Instance.SeaSceneToSea[node.WarpSceneName]; PlayerEx.Player.fubenContorl[node.WarpSceneName].NowIndex = Instance.SeaSceneToIndex[node.WarpSceneName]; }, null);
                    }
                    else
                    {
                        USelectBox.Show("传送至海域海岛需要元婴境界，或者海域声望声名远扬。", null, null);
                        UIPopTip.Inst.Pop("声望不足!", PopTipIconType.叹号);
                    }
                    return false;
                }
                //否则按原方法进行移动
                return true;
            }
        }

        //声望界面小修
        [HarmonyPatch(typeof(UIShengWangManager))]
        class UIShengWangManagerPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("SetShiLiInfo")]
            public static void SetShiLiInfofix(int id)
            {
                Instance.Logger.LogInfo("UIShengWangManagerSetShiLiInfo");
                int MenPaiId = PlayerEx.Player.menPai;
                string MenPaiName = Instance.DataList.Find(x => x.Id == MenPaiId).Shili;
                Transform tShengWang = UIShengWangManager.Inst.transform;
                Text ShiLiText = UIShengWangManager.Inst.PanelShiLiText;

                ShiLiText.horizontalOverflow = HorizontalWrapMode.Overflow;
                if (MenPaiId > 0)
                {
                    //修改宗门Label显示宗门名称
                    Transform tZongMenLabel = tShengWang.Find("BG/ShiLiToggles/UISWShiLiToggle_ZongMen/Label");
                    Text Lable = tZongMenLabel.GetComponent<Text>();
                    Lable.text = MenPaiName;
                    if (Lable.text.Length >= 4)
                    {
                        Lable.lineSpacing = 0.7F;
                    }
                    else
                    {
                        Lable.lineSpacing = 1F;
                    }

                    //选择宗门Label的时候修改信息
                    if (id == 1)
                    {
                        ShiLiText.text = MenPaiName;
                        UITeQuanItem component = UnityEngine.Object.Instantiate<GameObject>(UIShengWangManager.Inst.TeQuanItemPrefab, UIShengWangManager.Inst.ContentRT).GetComponent<UITeQuanItem>();
                        //修改这条的索引位置
                        Transform TeQuanMisc = component.transform.parent.Find("TeQuanMisc");
                        component.transform.SetSiblingIndex(TeQuanMisc.GetSiblingIndex() + 1);
                        if (PlayerEx.GetMenPaiShengWang() > 0)
                        {
                            component.SetText("允许使用宗门传送阵");
                        }
                        else
                        {
                            component.SetLockText("允许使用宗门传送阵");
                        }
                    }
                }

                //添加三势力使用传送阵特权
                if (id > 1)
                {
                    string ShiliWarpScene = Instance.DataList.Find(x => x.Id == id).SceneName;
                    UITeQuanItem component = UnityEngine.Object.Instantiate<GameObject>(UIShengWangManager.Inst.TeQuanItemPrefab, UIShengWangManager.Inst.ContentRT).GetComponent<UITeQuanItem>();
                    //修改这条的索引位置
                    Transform TeQuanMisc = component.transform.parent.Find("TeQuanMisc");
                    component.transform.SetSiblingIndex(TeQuanMisc.GetSiblingIndex() + 1);
                    if (PlayerEx.GetShengWang(id) >= 100)
                    {
                        component.SetText(string.Format("允许使用{0}传送阵", ShiliWarpScene));
                    }
                    else
                    {
                        component.SetLockText(string.Format("允许使用{0}传送阵", ShiliWarpScene));
                    }
                }
                //修改声望等级灯笼的tooltip
                UIShengWangManager.Inst.NingZhouShengWangList[1].TeQuanDesc = "宁州各大主城将对你发起悬赏。你将无法使用主城传送阵，但能使用荒山传送阵。";
                UIShengWangManager.Inst.NingZhouShengWangList[5].TeQuanDesc = "允许前往宁州各大门派拜山。允许使用各大门派传送阵。";
                UIShengWangManager.Inst.NingZhouShengWangList[6].TeQuanDesc = "可以请教宁州各大势力的不传绝学。可在宁州和海域之间跨州传送。";
                UIShengWangManager.Inst.SeaShengWangList[5].TeQuanDesc = "允许使用各大岛屿传送阵。";
                UIShengWangManager.Inst.SeaShengWangList[6].TeQuanDesc = "可以请教无尽之海各大势力的不传绝学。可在宁州和海域之间跨州传送。";

            }

            [HarmonyPostfix]
            [HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix()
            {
                Instance.Logger.LogInfo("UIShengWangManagerOnEnable");

                Transform tShengWang = UIShengWangManager.Inst.transform;

                JSONObject MenPaiHaoGanDu = PlayerEx.Player.MenPaiHaoGanDu;
                Instance.Logger.LogInfo("势力好感度" + MenPaiHaoGanDu.ToString());
                List<string> Shilis = MenPaiHaoGanDu.keys;

                string ShiLiHaoGanDu = "各势力声望：";
                for (int i = 0; i < MenPaiHaoGanDu.Count; i++)
                {
                    ShiLiHaoGanDu += Environment.NewLine;
                    ShiLiHaoGanDu += Instance.DataList.Find(x => x.Id == Convert.ToInt32(Shilis[i])).Shili.PadRight(6);
                    ShiLiHaoGanDu += MenPaiHaoGanDu[Shilis[i]].I.ToString().PadLeft(5);
                }

                Transform tNZLeft = tShengWang.Find("BG/NingZhouShengWang/Left");
                if (tNZLeft.GetComponent<PointerItem>() == null)
                {
                    PointerItem PI = tNZLeft.gameObject.AddComponent<PointerItem>();
                }
                tNZLeft.GetComponent<PointerItem>().TeQuanDesc = ShiLiHaoGanDu;

                Transform tSeaLeft = tShengWang.Find("BG/SeaShengWang/Left");
                if (tSeaLeft.GetComponent<PointerItem>() == null)
                {
                    PointerItem PI = tSeaLeft.gameObject.AddComponent<PointerItem>();
                }
                tSeaLeft.GetComponent<PointerItem>().TeQuanDesc = ShiLiHaoGanDu;
            }
        }

        public class PointerItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
        {
            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!string.IsNullOrWhiteSpace(TeQuanDesc))
                {
                    UToolTip.Show(TeQuanDesc, 200f);
                    UToolTip.BindObj = base.gameObject;
                }
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                UToolTip.Close();
            }

            public string TeQuanDesc;
        }

        //大地图移动
        [HarmonyPatch(typeof(MapComponent))]
        class MapComponentPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("AvatarMoveToThis")]
            public static bool AvatarMoveToThisPrefix(MapComponent __instance)
            {
                Instance.Logger.LogInfo("MapComponentAvatarMoveToThis");
                //起点为玩家小人所在位置
                MapPlayerController playerController = AllMapManage.instance.MapPlayerController;
                UnityEngine.GameObject StartNode = playerController.gameObject;
                //if (StartNode) { Instance.Logger.LogInfo("Find StartNode"); }

                //终点一种情况是点到城镇，则里面会有"PlayerPosition"，另一种就是点到节点Index的对象
                Transform transform = __instance.transform.Find("PlayerPosition");
                UnityEngine.GameObject EndNode = (transform != null) ? transform.gameObject : AllMapManage.instance.mapIndex[__instance.NodeIndex].gameObject;
                //if (EndNode) { Instance.Logger.LogInfo("Find EndNode"); }

                if (Instance.FlightAddDays.Value && playerController.ShowType == MapPlayerShowType.遁术)
                {
                    string dunshuname = "遁术飞行";
                    foreach (KBEngine.SkillItem skillItem in PlayerEx.Player.equipStaticSkillList)
                    {
                        if (skillItem.itemId == 804)
                            dunshuname = "御剑飞行";
                        else if (skillItem.itemId == 805)
                            dunshuname = "鹤点足飞行";
                    }
                    float dis = Vector2.Distance(StartNode.transform.position, EndNode.transform.position);
                    int dunsu = Tools.instance.getPlayer().dunSu;
                    //去掉飞行相当于一格的原版时间
                    int totaldays = (int)Math.Round(dis * 10 / dunsu) - 1;
                    if (totaldays < 0) { totaldays = 0; }
                    PlayerEx.Player.AddTime(totaldays, 0, 0);
                    Instance.Logger.LogInfo("这次飞行距离为" + dis + "玩家遁速" + dunsu + "共花费天数" + (totaldays + 1).ToString());
                    //初始和大傻的第二格到东石谷，“此处西北百余里便是东石谷”，按直线距离算，每地图长度单位等于50里路，按步行绕路总长算，每地图长度单位为20里路。
                    UIPopTip.Inst.Pop(string.Format("{0}{1}里，花费{2}天时间", dunshuname, ((int)Math.Ceiling(dis * 50)).ToCNNumber(), (totaldays + 1).ToCNNumber()), PopTipIconType.任务完成);
  
                }

                return true;
            }
        }
        /*海上移动
        [HarmonyPatch(typeof(MapSeaCompent))]
        class MapSeaCompentPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("AvatarMoveToThis")]
            public static void AvatarMoveToThisPostfix()
            {
                Instance.Logger.LogInfo("MapSeaCompentAvatarMoveToThis");
            }
        }
        */
        /*副本移动
        [HarmonyPatch(typeof(MapInstComport))]
        class MapInstComportPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("AvatarMoveToThis")]
            public static void AvatarMoveToThisPostfix()
            {
                Instance.Logger.LogInfo("MapInstComportAvatarMoveToThis");
            }
        }
        */
        /*基础层
        [HarmonyPatch(typeof(BaseMapCompont))]
        class BaseMapCompontPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("AvatarMoveToThis")]
            public static void AvatarMoveToThisPostfix()
            {
                Instance.Logger.LogInfo("BaseMapCompontAvatarMoveToThis");
            }
        }
        */
        /*
        public class MyMouse : MonoBehaviour
        { 
            void Update()
            {
                if(Input.GetMouseButtonUp(2))
                {
                    Vector3 MousePosScreen = Input.mousePosition;
                    Instance.Logger.LogInfo("鼠标中键点击屏幕坐标为" + MousePosScreen.ToString());
                    MousePosScreen.z = Camera.main.transform.position.z;
                    Vector3 MousePosWorld = Camera.main.ScreenToWorldPoint(MousePosScreen);
                    Instance.Logger.LogInfo("鼠标中键点击世界坐标为" + MousePosWorld.ToString());
                }
            }
        }
        */
    }
}
