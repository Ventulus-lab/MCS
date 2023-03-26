﻿using GetWay;
using HarmonyLib;
using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using System;
using System.Collections.Generic;
using System.Linq;



//对话指令
namespace Ventulus.VNext.DialogEvent
{
    [DialogEvent("SendOldEmail")]
    public class SendOldEmail : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int contactNpcId = command.GetInt(0);
            int senderNpcId = command.GetInt(1);
            string message = command.GetStr(2);
            string sendTime = command.ParamList.Length > 3 ? command.GetStr(3) : string.Empty;

            VTools.SendOldEmail(contactNpcId, senderNpcId, message, sendTime);
            callback?.Invoke();
        }
    }

    [DialogEvent("SendNewEmail")]
    public class SendNewEmail : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int contactNpcId = command.GetInt(0);
            string message = command.GetStr(1);
            string sendTime = command.ParamList.Length > 2 ? command.GetStr(2) : string.Empty;
            int actionId = command.ParamList.Length > 3 ? command.GetInt(3) : 0;
            int itemId = command.ParamList.Length > 4 ? command.GetInt(4) : 0;
            int itemNum = command.ParamList.Length > 5 ? command.GetInt(5) : 0;
            int outTime = command.ParamList.Length > 6 ? command.GetInt(6) : 60;

            VTools.SendNewEmail(contactNpcId, message, sendTime, actionId, itemId, itemNum, outTime);
            callback?.Invoke();
        }
    }

    [DialogEvent("SendNTaskEmail")]
    public class SendNTaskEmail : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int contactNpcId = command.GetInt(0);
            int nTaskId = command.GetInt(1);
            string message = command.GetStr(2);
            string sendTime = command.ParamList.Length > 3 ? command.GetStr(3) : string.Empty;
            bool reset = command.ParamList.Length > 4 ? command.GetBool(4) : false;

            bool result = VTools.SendNTaskEmail(contactNpcId, nTaskId, message, sendTime, reset);
            env.tmpArgs.Remove("SendNTaskEmail");
            env.tmpArgs.Add("SendNTaskEmail", Convert.ToInt32(result));
            callback?.Invoke();
        }
    }

    [DialogEvent("AddShengWang")]
    public class AddShengWang : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int id = command.GetInt(0);
            int add = command.GetInt(1);

            PlayerEx.AddShengWang(id, add, true);
            callback?.Invoke();
        }
    }


    [DialogEvent("CreateOneNpc")]
    public class CreateOneNpc : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int length = command.ParamList.Length;
            int type = length > 0 && command.GetStr(0) != "" ? command.GetInt(0) : 0;
            int liuPai = length > 1 && command.GetStr(1) != "" ? command.GetInt(1) : 0;
            int level = length > 2 && command.GetStr(2) != "" ? command.GetInt(2) : 0;
            int sex = length > 3 && command.GetStr(3) != "" ? command.GetInt(3) : 0;
            int zhengXie = length > 4 && command.GetStr(4) != "" ? command.GetInt(4) : 0;

            int npcId = VTools.CreateNpc(type, liuPai, level, sex, zhengXie);
            env.roleID = npcId;
            env.roleName = VTools.GetNPCName(npcId);
            callback?.Invoke();
        }
    }

    [DialogEvent("SearchOneNpc")]
    public class SearchOneNpc : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int length = command.ParamList.Length;
            int type = length > 0 && command.GetStr(0) != "" ? command.GetInt(0) : 0;
            int liuPai = length > 1 && command.GetStr(1) != "" ? command.GetInt(1) : 0;
            int level = length > 2 && command.GetStr(2) != "" ? command.GetInt(2) : 0;
            int sex = length > 3 && command.GetStr(3) != "" ? command.GetInt(3) : 0;
            int zhengXie = length > 4 && command.GetStr(4) != "" ? command.GetInt(4) : 0;

            List<int> list = VTools.SearchNpc(type, liuPai, level, sex, zhengXie);
            if (list.Count > 0)
            {
                int j = VTools.GetRandom(0, list.Count);
                int npcId = list[j];
                env.roleID = npcId;
                env.roleName = VTools.GetNPCName(npcId);
            }
            else
            {
                int npcId = 0;
                env.roleID = npcId;
                env.roleName = VTools.GetNPCName(npcId);
            }
            callback?.Invoke();
        }
    }

    [DialogEvent("NpcDoAction")]
    public class NpcDoAction : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int npcId = command.GetInt(0);
            int actionId = command.GetInt(1);

            bool result = VTools.NpcDoAction(npcId, actionId);
            env.tmpArgs.Remove("NpcDoAction");
            env.tmpArgs.Add("NpcDoAction", Convert.ToInt32(result));
            callback?.Invoke();
        }
    }

}

//触发器
namespace Ventulus.VNext.DialogTrigger
{
    [HarmonyPatch]
    public class NearNpc
    {

        private static bool bRefreshed = false;
        private static List<int> lastNpcList = new List<int>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UINPCJiaoHu), nameof(UINPCJiaoHu.RefreshNowMapNPC))]
        public static void RefreshNowMapNPC_Postfix()
        {
            //VTools.LogInfo("UINPCJiaoHu.RefreshNowMapNPC");
            bRefreshed = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UINPCLeftList), nameof(UINPCLeftList.RefreshNPC))]
        public static void RefreshNPC_Postfix()
        {
            //VTools.LogInfo("UINPCLeftList.RefreshNPC");

            if (!bRefreshed)
                return;
            bRefreshed = false;

            List<int> newNpcList = new List<int>();

            newNpcList.AddRange(UINPCJiaoHu.Inst.TNPCIDList);
            newNpcList.AddRange(UINPCJiaoHu.Inst.NPCIDList);
            newNpcList.AddRange(UINPCJiaoHu.Inst.SeaNPCIDList);
            //VTools.LogMessage("lastNearNpcCount" + lastNpcList.Count);
            //VTools.LogMessage("newNearNpcCount" + newNpcList.Count);

            if (newNpcList.Count == 0)
                return;
            //空列表直接结束不存，屏蔽反复进出空地方刷概率

            newNpcList.Sort();
            if (newNpcList.SequenceEqual(lastNpcList))
                return;
            //和上次有人的列表不同，才允许进
            lastNpcList = newNpcList;

            //这里仅能做到初步筛选
            DialogEnvironment env = new DialogEnvironment();
            env.customData.Add("NearNpcList", newNpcList);

            DialogAnalysis.TryTrigger(new string[]
            {
                "附近的人",
                "OnNearNpc"
            }, env, true);
        }

    }

    [HarmonyPatch]
    public class AllMapMove
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapComponent), nameof(MapComponent.NewMovaAvatar))]
        public static bool NewMovaAvatar_Prefix()
        {
            //VTools.LogMessage("MapComponent.NewMovaAvatar_Prefix");
            //屏蔽多次点击实际小人在移动中
            if (AllMapManage.instance.isPlayMove)
                return true;

            DialogEnvironment env = new DialogEnvironment();

            if (DialogAnalysis.TryTrigger(new string[]
            {
                "大地图移动前",
                "BeforeAllMapMove"
            }, env, false))
            {
                MapGetWay.Inst.IsStop = true;
                return false;
            }
            else
                return true;


        }

    }

    [HarmonyPatch]
    public class FubenMove
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapInstComport), nameof(MapInstComport.AvatarMoveToThis))]
        public static bool AvatarMoveToThis_Prefix()
        {

            // VTools.LogMessage("MapInstComport.AvatarMoveToThis_Prefix");

            DialogEnvironment env = new DialogEnvironment();

            if (DialogAnalysis.TryTrigger(new string[]
            {
                "副本移动前",
                "BeforeFubenMove"
            }, env, false))
            {
                return false;
            }
            else
                return true;
        }

    }

    //通过VTools中相关函数来调用
    public class JieSuanComplete
    {
        public static void CallTrigger()
        {
            VTools.LogMessage("调用触发函数");
            DialogEnvironment env = new DialogEnvironment();

            DialogAnalysis.TryTrigger(new string[]
            {
                "结算完成",
                "OnJieSuanComplete"
            }, env, true);

        }
    }

}

//环境脚本
namespace Ventulus.VNext.DialogEnvQuery
{
    [DialogEnvQuery("GetNPCName")]
    public class GetNPCName : IDialogEnvQuery
    {
        public object Execute(DialogEnvQueryContext context)
        {
            int npcId = context.GetArg(0, -1);
            return VTools.GetNPCName(npcId);
        }
    }

    [DialogEnvQuery("NearNpcContains")]
    public class NearNpcContains : IDialogEnvQuery
    {
        public object Execute(DialogEnvQueryContext context)
        {
            if (!context.Env.customData.ContainsKey("NearNpcList"))
                return false;

            List<int> npcIds = new List<int>();
            object ob = context.GetArg(0);
            //判断第一个参数类型是整数还是数组
            if (ob.GetType() == typeof(int))
            {
                npcIds.Add(NPCEx.NPCIDToNew((int)ob));
            }
            else if (ob.GetType() == typeof(object[]))
            {
                foreach (object o in (object[])ob)
                {
                    npcIds.Add(NPCEx.NPCIDToNew((int)o));
                }
            }
            else
            {
                VTools.LogError("传入参数数据类型错误");
                return false;
            }

            List<int> oldIds = npcIds.Select(x => NPCEx.NPCIDToOld(x)).ToList();
            List<int> NearNpcList = context.Env.customData["NearNpcList"] as List<int>;

            //要把找到的npcId存下来
            //if (NearNpcList.Exists(x => npcIds.Contains(x)) || NearNpcList.Exists(x => oldIds.Contains(x)))
            int findNpc = NearNpcList.FirstOrDefault(x => npcIds.Contains(x));
            if (findNpc == 0)
                findNpc = NearNpcList.FirstOrDefault(x => oldIds.Contains(x));
            if (findNpc > 0)
            {

                //实际上在切换场景黑屏的时候会发送很多次，只会最后成功一次，你不知道那次会成功
                //概率触发
                int randomNum = context.Args.Length > 1 ? context.GetArg(1, 100) : 100;
                int roll = VTools.GetRandom(0, 100);
                //VTools.LogInfo($"Roll {roll}/{randomNum}");
                if (roll < randomNum)
                {
                    int npcId = NPCEx.NPCIDToNew(findNpc);
                    context.Env.roleID = npcId;
                    context.Env.roleName = VTools.GetNPCName(findNpc);
                    context.Env.roleBindID = NPCEx.NPCIDToOld(findNpc);
                    context.Env.mapScene = SceneEx.NowSceneName;
                    return true;
                }
            }
            return false;
        }

    }

    [DialogEnvQuery("RandomProbability")]
    public class RandomProbability : IDialogEnvQuery
    {
        public object Execute(DialogEnvQueryContext context)
        {
            int randomNum = context.GetArg<int>(0, 100);
            int roll = VTools.GetRandom(0, 100);
            //VTools.LogInfo($"Roll {roll}/{randomNum}");
            return roll < randomNum;
        }
    }

    [DialogEnvQuery("GetCurFubenIndex")]
    public class GetCurFubenIndex : IDialogEnvQuery
    {
        public object Execute(DialogEnvQueryContext context)
        {
            return PlayerEx.Player.fubenContorl[Tools.getScreenName()].NowIndex;
        }
    }

    [DialogEnvQuery("GetPlaceName")]
    public class GetPlaceName : IDialogEnvQuery
    {
        public object Execute(DialogEnvQueryContext context)
        {
            return VTools.GetPlaceName();
        }
    }

    [DialogEnvQuery("GetShengWang")]
    public class GetShengWang : IDialogEnvQuery
    {
        public object Execute(DialogEnvQueryContext context)
        {
            int id = context.GetArg(0, 0);
            return PlayerEx.GetShengWang(id);
        }
    }
}
