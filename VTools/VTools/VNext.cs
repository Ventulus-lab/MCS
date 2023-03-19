using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using System;


//对话指令
namespace Ventulus.VNext.DialogEvent
{
    [DialogEvent("SendOldEmail")]
    public class SendOldEmail : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int ContactNPCid = command.GetInt(0);
            int SenderNPCid = command.GetInt(1);
            string Message = command.GetStr(2);
            string SendTime = command.ParamList.Length > 3 ? command.GetStr(3) : string.Empty;

            VTools.SendOldEmail(ContactNPCid, SenderNPCid, Message, SendTime);
            callback?.Invoke();
        }
    }

    [DialogEvent("SendNewEmail")]
    public class SendNewEmail : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int ContactNPCid = command.GetInt(0);
            string Message = command.GetStr(1);
            string SendTime = command.ParamList.Length > 2 ? command.GetStr(2) : string.Empty;
            int actionId = command.ParamList.Length > 3 ? command.GetInt(3) : 0;
            int itemId = command.ParamList.Length > 4 ? command.GetInt(4) : 0;
            int itemNum = command.ParamList.Length > 5 ? command.GetInt(5) : 0;
            int outtime = command.ParamList.Length > 6 ? command.GetInt(6) : 60;

            VTools.SendNewEmail(ContactNPCid, Message, SendTime, actionId, itemId, itemNum, outtime);
            callback?.Invoke();
        }
    }

    [DialogEvent("SendNTaskEmail")]
    public class SendNTaskEmail : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            int ContactNPCid = command.GetInt(0);
            int NTaskid = command.GetInt(1);
            string Message = command.GetStr(2);
            string SendTime = command.ParamList.Length > 3 ? command.GetStr(3) : string.Empty;
            bool Reset = command.ParamList.Length > 4 ? command.GetBool(4) : false;

            VTools.SendNTaskEmail(ContactNPCid, NTaskid, Message, SendTime, Reset);
            callback?.Invoke();
        }
    }
}

//触发器(扳机)
namespace Ventulus.VNext.DialogTrigger
{

}

//环境指令(运行时脚本)
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
}
