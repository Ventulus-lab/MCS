using Bag;
using BepInEx;
using HarmonyLib;
using KBEngine;
using script.NewLianDan;
using script.NewLianDan.LianDan;
using script.NewLianDan.Result;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.LianDanNaiJiu", "LianDanNaiJiu", "1.0")]
    public class LianDanNaiJiu : BaseUnityPlugin
    {
        void Start()
        {
            Logger.LogInfo("炼丹炉耐久刚好用完能正常出丹加载成功！");
            var harmony = new Harmony("Ventulus.MCS.LianDanNaiJiu");
            harmony.PatchAll();
        }
        public static LianDanNaiJiu Instance;
        void Awake()
        {
            Instance = this;
        }
        [HarmonyPatch(typeof(LianDanPanel))]
        class LianDanPanelPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("StartLianDan")]
            public static bool StartLianDanPrefix()
            {
                //Instance.Logger.LogInfo("啊啊啊啊啊开始算啦");
                DanLuSlot danLu = LianDanUIMag.Instance.LianDanPanel.DanLu;
                KBEngine.Avatar player = Tools.instance.getPlayer();
                int naijiu = danLu.Item.Seid["NaiJiu"].I;
                foreach (ITEM_INFO value in player.itemList.values)
                {
                    if (value.uuid == danLu.Item.Uid)
                    {
                        Instance.Logger.LogInfo("现在耐久是" + naijiu + "准备加一");
                        value.Seid.SetField("NaiJiu", naijiu + 1);
                        danLu.Item.Seid.SetField("NaiJiu", naijiu + 1);
                        Instance.Logger.LogInfo(value.ToString());
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LianDanResult))]
        class LianDanResultPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Success")]
            public static bool SuccessPrefix()
            {
                //Instance.Logger.LogInfo("啊啊啊啊啊算完啦");
                DanLuSlot danLu = LianDanUIMag.Instance.LianDanPanel.DanLu;
                KBEngine.Avatar player = Tools.instance.getPlayer();
                if (!danLu.IsNull())
                {
                    int naijiu = danLu.Item.Seid["NaiJiu"].I;
                    if (naijiu <= 1)
                    {
                        Instance.Logger.LogInfo("发现丹炉耐久是加的1准备炸啦");
                        player.removeItem(danLu.Item.Uid);
                        danLu.SetNull();
                        Tools.Say("{vpunch=10,0.5}在你刚刚将丹药取出来的时候，突然丹炉崩裂开来，幸好没有伤到人，好危险！", 0);
                    }
                    else
                    {
                        Instance.Logger.LogInfo("现在耐久是" + naijiu + "准备减一");
                        foreach (ITEM_INFO value in player.itemList.values)
                        {
                            if (value.uuid == danLu.Item.Uid)
                            {
                                value.Seid.SetField("NaiJiu", naijiu - 1);
                                danLu.Item.Seid.SetField("NaiJiu", naijiu - 1);
                                Instance.Logger.LogInfo(value.ToString());
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
