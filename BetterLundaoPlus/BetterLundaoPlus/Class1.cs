using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JSONClass;
using KBEngine;
using System;
using System.Collections.Generic;
using System.Linq;
//using KBEngine;
using UnityEngine;
using UnityEngine.UI;

namespace Ventulus
{

    [BepInPlugin("Ventulus.MCS.BetterLundaoPlus", "更好的论道Plus", "1.2")]
    public class BetterLundaoPlus : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
            LimitTopicsNum = Config.Bind<bool>("Ventulus", "限制玩家选题数量", true, "按照境界限制玩家选题数量，默认为true");
            LundaoSpendTime = Config.Bind<bool>("Ventulus", "每回合花费1天时间", true, "论道每回合花费1天时间，默认为true");
            InstWudaoExp = Config.Bind<int>("Ventulus", "获得悟道经验时立即获取的比例%", 20, new ConfigDescription("默认20%，其余转化为思绪时间，设为0即为原版全转为思绪", new AcceptableValueRange<int>(0, 100)));
        }
        void Start()
        {

            new Harmony("Ventulus.MCS.BetterLundaoPlus").PatchAll();

            Logger.LogInfo("加载成功！");
        }

        public static BetterLundaoPlus Instance;
        ConfigEntry<bool> LimitTopicsNum;
        ConfigEntry<bool> LundaoSpendTime;
        ConfigEntry<int> InstWudaoExp;


        [HarmonyPatch(typeof(LunDaoHuiHe))]
        class LunDaoHuiHe_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Init")]
            public static void Init_Postfix(LunDaoHuiHe __instance)
            {
                Instance.Logger.LogInfo("LunDaoHuiHeInit");
                //论道初始化修改总回合数
                int luntinum = LunDaoManager.inst.selectLunTiList.Count;
                __instance.totalHui = luntinum * 2 + 1;
                __instance.curHui = 1;
                __instance.shengYuHuiHe = __instance.totalHui - __instance.curHui;

                //Traverse.Create(__instance).Method("upDateHuiHeText").GetValue();

                Text curHuiText = Traverse.Create(__instance).Field("curHuiText").GetValue<Text>();
                curHuiText.horizontalOverflow = HorizontalWrapMode.Overflow;
                Traverse.Create(__instance).Field("curHuiText").SetValue(curHuiText);

                //论道初始化修改剩余回合显示
                Text shengYuHuiHeText = Traverse.Create(__instance).Field("shengYuHuiHeText").GetValue<Text>();
                string shengyu = (__instance.shengYuHuiHe + 1).ToCNNumber();
                shengYuHuiHeText.text = "剩余" + __instance.totalHui.ToCNNumber() + "之" + shengyu + "回合";
                shengYuHuiHeText.horizontalOverflow = HorizontalWrapMode.Overflow;
                Traverse.Create(__instance).Field("shengYuHuiHeText").SetValue(shengYuHuiHeText);
            }

            [HarmonyPostfix]
            [HarmonyPatch("ReduceHuiHe")]
            public static void ReduceHuiHe_Postfix(LunDaoHuiHe __instance)
            {
                Instance.Logger.LogInfo("LunDaoHuiHeReduceHuiHe");
                //论道每回合修改剩余回合显示
                Text shengYuHuiHeText = Traverse.Create(__instance).Field("shengYuHuiHeText").GetValue<Text>();
                string shengyu = (__instance.shengYuHuiHe + 1).ToCNNumber();
                shengYuHuiHeText.text = "剩余" + __instance.totalHui.ToCNNumber() + "之" + shengyu + "回合";
                shengYuHuiHeText.horizontalOverflow = HorizontalWrapMode.Overflow;
                Traverse.Create(__instance).Field("shengYuHuiHeText").SetValue(shengYuHuiHeText);

                //论道每回合消耗一天时间
                KBEngine.Avatar player = Tools.instance.getPlayer();
                if (Instance.LundaoSpendTime.Value)
                    player.AddTime(1, 0, 0);
                Instance.Logger.LogInfo(player.worldTimeMag.getNowTime());
            }
        }



        [HarmonyPatch(typeof(SelectLunTi))]
        class SelectLunTi_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("AddLunTiToList")]
            public static void AddLunTiToList_Postfix(SelectLunTi __instance)
            {
                Instance.Logger.LogInfo("SelectLunTiAddLunTiToList");
                //论道选题变动调用论道初始化
                LunDaoManager.inst.selectLunTiList = __instance.selectLunTiList;
                LunDaoManager.inst.playerController.lunDaoHuiHe.Init();
            }

            [HarmonyPostfix]
            [HarmonyPatch("RemoveLunTiByList")]
            public static void RemoveLunTiByList_Postfix(SelectLunTi __instance)
            {
                Instance.Logger.LogInfo("SelectLunTiRemoveLunTiByList");
                //论道选题变动调用论道初始化
                LunDaoManager.inst.selectLunTiList = __instance.selectLunTiList;
                LunDaoManager.inst.playerController.lunDaoHuiHe.Init();
            }
        }

        [HarmonyPatch(typeof(LunTiCell))]
        class LunTiCell_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("MouseUp")]
            public static bool MouseUp_Prefix(LunTiCell __instance)
            {
                Instance.Logger.LogInfo("LunTiCellMouseUp");
                if (Instance.LimitTopicsNum.Value == false)
                    return true;
                //按照境界限制玩家选题数量
                KBEngine.Avatar player = Tools.instance.getPlayer();
                int npcid = LunDaoManager.inst.npcId;
                JSONObject jsonobject = jsonData.instance.AvatarJsonData[npcid.ToString()];
                int npclv = jsonobject["Level"].I;
                int maxlv = player.level;
                if (npclv > maxlv) maxlv = npclv;
                string maxlevel = LevelUpDataJsonData.DataDict[maxlv].Name;
                int maxlunticount = (maxlv - 1) / 3 + 1;
                bool LunTiCellState = Traverse.Create(__instance).Field("state").GetValue<bool>();
                //Instance.Logger.LogInfo("格子状态"+ LunTiCellState + LunDaoManager.inst.selectLunTiList.Count);
                if (!LunTiCellState && LunDaoManager.inst.selectLunTiList.Count >= maxlunticount)
                {
                    UIPopTip.Inst.Pop(string.Format("两人最高{0}，可选{1}条论题", maxlevel, maxlunticount), PopTipIconType.叹号);
                    Instance.Logger.LogInfo(maxlevel + maxlunticount);
                    return false;
                }
                else
                    return true;
            }
        }
        struct WuDaoLv
        {
            public int id;
            public int lv;
        };
        [HarmonyPatch(typeof(LunDaoManager))]
        class LunDaoManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("GetSuiJiLuntTi")]
            public static void GetSuiJiLuntTi_Postfix(ref List<int> __result)
            {
                Instance.Logger.LogInfo("LunDaoManagerGetSuiJiLuntTi");
                //按照境界限制npc选题数量
                KBEngine.Avatar player = Tools.instance.getPlayer();
                int npcid = LunDaoManager.inst.npcId;
                JSONObject jsonobject = jsonData.instance.AvatarJsonData[npcid.ToString()];
                int npclv = jsonobject["Level"].I;
                int maxlv = player.level;
                if (npclv > maxlv) maxlv = npclv;
                string maxlevel = LevelUpDataJsonData.DataDict[maxlv].Name;
                int maxlunticount = (maxlv - 1) / 3 + 1;
                UIPopTip.Inst.Pop(string.Format("两人最高{0}，可选{1}条论题", maxlevel, maxlunticount), PopTipIconType.叹号);
                Instance.Logger.LogInfo(maxlevel + maxlunticount);

                //筛选npc论题
                JSONObject wuDaoJson = jsonobject["wuDaoJson"];
                //Instance.Logger.LogInfo(wuDaoJson);
                List<WuDaoLv> chooselist = new List<WuDaoLv>();
                for (int i = 1; i <= 10; i++)
                {
                    WuDaoLv onewudao = new WuDaoLv
                    {
                        id = wuDaoJson[i.ToString()]["id"].I,
                        lv = wuDaoJson[i.ToString()]["level"].I
                    };
                    chooselist.Add(onewudao);
                }
                //大道已成的和没成的分开
                List<WuDaoLv> orderlist = chooselist.OrderByDescending(x => x.lv).ToList();
                List<WuDaoLv> orderlist5 = new List<WuDaoLv>();
                List<WuDaoLv> orderlist4 = new List<WuDaoLv>();
                foreach (WuDaoLv item in orderlist)
                {
                    Instance.Logger.LogInfo((WuDaoType)item.id + item.lv.ToString());
                    if (item.lv >= 5)
                    {
                        orderlist5.Add(item);
                    }
                    else
                    {
                        orderlist4.Add(item);
                    }
                }
                //优先选未到悟道顶端的道中的较高的。
                List<int> resultlist = new List<int>();
                while (resultlist.Count < maxlunticount)
                {
                    if (orderlist4.Count > 0)
                    {
                        int random = LunDaoManager.inst.lunDaoCardMag.getRandom(0, orderlist4.Count - 1);
                        int mylv = orderlist4[0].lv;
                        if (orderlist4[random].lv == mylv)
                        {
                            resultlist.Add(orderlist4[random].id);
                            orderlist4.RemoveAt(random);
                        }
                    }
                    else
                    {
                        int random = LunDaoManager.inst.lunDaoCardMag.getRandom(0, orderlist5.Count - 1);
                        resultlist.Add(orderlist5[random].id);
                        orderlist5.RemoveAt(random);
                    }
                }

                __result = resultlist;
            }

            [HarmonyPrefix]
            [HarmonyPatch("InitLunDao")]
            public static bool InitLunDao_Prefix()
            {
                //用来强制论道为NPC随机选择
                //Tools.instance.IsSuiJiLunTi=true;
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GameOver")]
            public static bool GameOver_Prefix()
            {
                int paytime = LunDaoManager.inst.playerController.lunDaoHuiHe.totalHui;
                if (paytime >= LunDaoManager.inst.playerController.lunDaoHuiHe.curHui)
                {
                    //说明是在回合内完成论题结束游戏，当前回合还得再过一天
                    paytime = LunDaoManager.inst.playerController.lunDaoHuiHe.curHui;
                    KBEngine.Avatar player = Tools.instance.getPlayer();
                    if (Instance.LundaoSpendTime.Value)
                        player.AddTime(1, 0, 0);
                    Instance.Logger.LogInfo(player.worldTimeMag.getNowTime());
                }
                if (Instance.LundaoSpendTime.Value)
                    UIPopTip.Inst.Pop(string.Format("论道共花费{0}天", paytime), PopTipIconType.感悟);
                //Instance.Logger.LogInfo("论道花费天" + paytime);
                return true;
            }
        }

        [HarmonyPatch(typeof(WuDaoMag))]
        class WuDaoMag_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("AddLingGuang")]
            public static bool AddLingGuang_Prefix(int type, ref int studyTime, int quality)
            {
                //Instance.Logger.LogInfo("加灵光感悟" + name + type + studyTime + guoqiTime + desc + quality + isLunDao);
                Instance.Logger.LogInfo("WuDaoMagAddLingGuang");

                //获得几品灵感对应的经验倍率
                JSONObject wuDaoExBeiLuJson = jsonData.instance.WuDaoExBeiLuJson;
                int multi = wuDaoExBeiLuJson["1"]["lingguang" + quality.ToString()].I;

                //经验分配
                int exsum = studyTime * multi;
                int exins = (int)Math.Floor(exsum * (float)Instance.InstWudaoExp.Value / 100);
                //int exstu = exsum - exins;
                int stutime = (int)Math.Ceiling(studyTime * (1 - (float)Instance.InstWudaoExp.Value / 100));

                Tools.instance.getPlayer().wuDaoMag.addWuDaoEx(type, exins);
                studyTime = stutime;
                UIPopTip.Inst.Pop(string.Format("共感悟到{0}点{1}之道经验", exsum, (WuDaoType)type), PopTipIconType.感悟);
                UIPopTip.Inst.Pop(string.Format("其中{0}点悟道经验当场领悟 ", exins), PopTipIconType.感悟);

                Instance.Logger.LogInfo("共经验" + exsum + "直接" + exins + "其余时间" + stutime);
                if (studyTime <= 0)
                    return false;
                else
                {
                    UIPopTip.Inst.Pop(string.Format("其余经验转化为{0}日{1}品思绪 ", stutime, quality), PopTipIconType.感悟);
                    return true;
                }
            }
        }
    }
}
