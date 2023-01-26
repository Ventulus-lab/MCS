using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JSONClass;
using System;
using System.Collections.Generic;
//using KBEngine;
using System.IO;
using System.Reflection;
//using KBEngine;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.CustomShuangXiuPlus", "CustomShuangXiuPlus", "2.0")]
    public class CustomShuangXiuPlus : BaseUnityPlugin
    {
        void Start()
        {

            Instance.Logger.LogInfo("自定义双修增强版加载成功！");
            var harmony = new Harmony("Ventulus.MCS.CustomShuangXiuPlus");
            harmony.PatchAll();

            MessageMag.Instance.Register(MessageName.MSG_GameInitFinish, new Action<MessageData>(this.Init));

        }

        public static CustomShuangXiuPlus Instance;
        public static ConfigEntry<float> SpaceTime;
        public static string dllPath = string.Empty;
        public Random random = new Random();
        public static ConfigEntry<bool> ShuangxiuSpendDays;
        void Awake()
        {
            Instance = this;
        }

        void Init(MessageData data)
        {
            dllPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            Instance.Logger.LogInfo("路径" + dllPath);

            //绑定ppt模式的间隔时间
            SpaceTime = Config.Bind<float>("config", "SpaceTime", 1f, "双修播放图片间隔时间(秒)");
            ShuangxiuSpendDays = Config.Bind<bool>("config", "ShuangxiuSpendDays", true, "双修消耗时间");

            //目录下创建文件夹
            DirectoryInfo directoryInfo = new DirectoryInfo(dllPath + "\\Custom\\ShuangXiu\\随机");
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
                Instance.Logger.LogInfo("创建" + directoryInfo.ToString());
            }
            foreach (ShuangXiuMiShu shuangXiuMiShu in ShuangXiuMiShu.DataList)
            {
                directoryInfo = new DirectoryInfo(dllPath + "\\Custom\\ShuangXiu\\" + shuangXiuMiShu.name);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                    Instance.Logger.LogInfo("创建" + directoryInfo.ToString());
                }
            }


        }


        [HarmonyPatch(typeof(VideoImage))]
        class VideoImagePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Play")]
            public static void PlayPostfix(VideoImage __instance)
            {
                if (__instance.GroupName == "ShuangXiu")
                {
                    __instance.SpriteSpaceTime = CustomShuangXiuPlus.SpaceTime.Value;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("TargetDirPath", MethodType.Getter)]
            public static void TargetDirPathPostfix(VideoImage __instance, ref string __result)
            {
                if (__instance.GroupName == "ShuangXiu")
                {
                    __result = dllPath + "\\Custom\\ShuangXiu\\" + __instance.TargetFileName;
                    Instance.Logger.LogInfo("修改双修动画目录" + __result);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("TargetVideoFilePath", MethodType.Getter)]
            public static void TargetVideoFilePathPostfix(VideoImage __instance, ref string __result)
            {
                string suijiPath = dllPath + "\\Custom\\ShuangXiu\\随机";
                if (__instance.GroupName == "ShuangXiu" && Directory.Exists(suijiPath))
                {
                    List<string> pngFiles = new List<string>(Directory.EnumerateFiles(__instance.TargetDirPath, "*.png"));
                    List<string> jpgFiles = new List<string>(Directory.EnumerateFiles(__instance.TargetDirPath, "*.jpg"));
                    //双修秘术文件不存在，到随机文件夹寻找
                    if (!File.Exists(__result) && pngFiles.Count == 0 && jpgFiles.Count == 0)
                    {
                        List<string> suijiFiles = new List<string>(Directory.EnumerateFiles(suijiPath, "*.mp4"));
                        Instance.Logger.LogInfo(__result + "双修秘术动画文件不存在，到随机文件夹找到随机视频" + suijiFiles.Count);
                        if (suijiFiles.Count > 0)
                        {
                            int at = Instance.random.Next(0, suijiFiles.Count);
                            __result = suijiFiles[at];
                            Instance.Logger.LogInfo("选用随机视频" + __result);
                        }

                    }

                }
            }
        }

        [HarmonyPatch(typeof(PlayerEx))]
        class PlayerExPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("DoShuangXiu")]
            public static void DoShuangXiuPostfix(UINPCData npc)
            {
                if (ShuangxiuSpendDays.Value)
                {
                    KBEngine.Avatar player = Tools.instance.getPlayer();
                    int minlv = (player.level > npc.Level) ? npc.Level : player.level;
                    int paytime = (minlv - 1) / 3 + 1;
                    PlayerEx.Player.AddTime(paytime, 0, 0);
                    UIPopTip.Inst.Pop(string.Format("双修共持续了{0}天{0}夜", paytime.ToCNNumber()), PopTipIconType.感悟);
                }
            }
        }

    }
}
