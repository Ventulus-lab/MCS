using Bag;
using BepInEx;
using BepInEx.Configuration;
using Fungus;
using GUIPackage;
using HarmonyLib;
using JSONClass;
using script.NpcAction;
using SkySwordKill.Next.DialogEvent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YSGame;

namespace Ventulus
{
    [BepInPlugin("Ventulus.MCS.VTools", "微风的工具库", "1.0.1")]
    public class VTools : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            new Harmony("Ventulus.MCS.VTools").PatchAll();
            Logger.LogInfo("加载成功");
        }
        public static VTools Instance;
        protected static void LogError(object data)
        {
            Instance.Logger.LogError(data);
        }
        protected static void LogWarning(object data)
        {
            Instance.Logger.LogWarning(data);
        }
        protected static void LogMessage(object data)
        {
            Instance.Logger.LogMessage(data);
        }
        protected static void LogInfo(object data)
        {
            Instance.Logger.LogInfo(data);
        }
        protected static void LogDebug(object data)
        {
            Instance.Logger.LogDebug(data);
        }
        public static KBEngine.Avatar player => Tools.instance.getPlayer();

        public static DateTime NowTime => player.worldTimeMag.getNowTime();
        public static string nowTime => player.worldTimeMag.nowTime;

        //根据npcId获取姓名
        public static string GetNPCName(int npcId)
        {
            //一般情情况在AvatarRandomJsonData有，死了在npcDeathJson里记newid，实在不行再AvatarJsonData里找oldid
            int id = NPCEx.NPCIDToNew(npcId);
            if (id == 0)
                return "旁白";
            else if (id == 1)
                return Tools.GetPlayerName();
            else if (jsonData.instance.AvatarRandomJsonData.HasField(id.ToString()))
                return jsonData.instance.AvatarRandomJsonData[id.ToString()]["Name"].str.ToCN();
            else if (NpcJieSuanManager.inst.npcDeath.npcDeathJson.HasField(id.ToString()))
                return NpcJieSuanManager.inst.npcDeath.npcDeathJson[id.ToString()]["deathName"].str.ToCN();
            else if (jsonData.instance.AvatarJsonData.HasField(id.ToString()))
            {
                JSONObject jsonobject = jsonData.instance.AvatarJsonData[NPCEx.NPCIDToOld(npcId).ToString()];
                return jsonobject["FirstName"].str.ToCN() + jsonobject["Name"].str.ToCN();
            }
            else
                return "未知";
        }
        //获取显示的npcid字符串
        public static string MakeNPCIdStr(int id)
        {
            id = NPCEx.NPCIDToNew(id);
            int npcId = NPCEx.NPCIDToOld(id);
            string str = id >= 20000 ? id.ToString() : string.Empty;
            if (npcId < 20000)
                str += $"({npcId})";
            return str;
        }

        //发送任意信息OldEmail，联系人和发信人可不相同，每有一个发信人需占用一个传音符Id，不可携带物品（要携带物品则需要再单独占用传音符Id）
        private static readonly string CyFuOld = @"{""id"":100000,""AvatarID"":2,""info"":""{DiDian}"",""Type"":3,""DelayTime"":[],""TaskID"":0,""TaskIndex"":[],""WeiTuo"":0,""ItemID"":0,""valueID"":[],""value"":[],""SPvalueID"":0,""StarTime"":"""",""EndTime"":"""",""Level"":[],""HaoGanDu"":0,""EventValue"":[],""fuhao"":"""",""IsOnly"":1,""IsAdd"":0,""IsDelete"":0,""NPCLevel"":[],""IsAlive"":0}";
        public static void SendOldEmail(int ContactNPCId, int SenderNPCId, string Message, string SendTime = "")
        {
            int CyFuId = 100000 + SenderNPCId;
            if (!player.NewChuanYingList.HasField(CyFuId.ToString()))
            {
                JSONObject emailjson = new JSONObject(CyFuOld);
                emailjson.SetField("id", CyFuId.ToString());
                emailjson.SetField("AvatarID", SenderNPCId.ToString());
                emailjson.SetField("sendTime", nowTime);
                emailjson.SetField("CanCaoZuo", false);
                emailjson.SetField("AvatarName", GetNPCName(SenderNPCId));//关键就在于显示的名字不同
                LogMessage(emailjson.ToString());
                player.NewChuanYingList.SetField(CyFuId.ToString(), emailjson);
            }

            //加入新传音符
            player.AddFriend(ContactNPCId);
            if (NPCEx.NPCIDToNew(ContactNPCId) < 20000 && !jsonData.instance.AvatarJsonData[ContactNPCId.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[ContactNPCId.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(SendTime))
            {
                SendTime = nowTime;
            }
            EmailData emailData = new EmailData(ContactNPCId, isOld: true, CyFuId, SendTime)
            {
                sceneName = Message
            };
            player.emailDateMag.AddNewEmail(ContactNPCId.ToString(), emailData);
        }

        //发送任意信息NewEmail，发信人只能是联系人（因为发信人名字限制死了只能为邮件npcid），可携带物品及数量，只占用对白表100000号.actionId=1是发送物品给玩家，2是向玩家请求物品，这种情况下outtime为npc等待月份如果超时npc回答不同，且好感度固定只加1（加情分按物品价值）
        private static readonly string CyDuiBaiNew = @"{""id"":100000,""Type"":100000,""XingGe"":1,""dir1"":""{DiDian}"",""dir2"":""{DiDian}"",""dir3"":""{DiDian}""}";
        public static void SendNewEmail(int NPCId, string Message, string SendTime = "", int actionId = 0, int itemId = 0, int itemNum = 0, int outtime = 60)
        {
            int CyDuiBaiId = 100000;
            if (!jsonData.instance.CyNpcDuiBaiData.HasField(CyDuiBaiId.ToString()))
            {
                JSONObject emailjson = new JSONObject(CyDuiBaiNew);
                LogMessage(emailjson.ToString());
                jsonData.instance.CyNpcDuiBaiData.SetField(CyDuiBaiId.ToString(), emailjson);
            }
            //加入新传音符
            player.AddFriend(NPCId);
            if (NPCEx.NPCIDToNew(NPCId) < 20000 && !jsonData.instance.AvatarJsonData[NPCId.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[NPCId.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(SendTime))
            {
                SendTime = nowTime;
            }
            EmailData emailData = new EmailData(NPCId, isOut: false, isComplete: false, new List<int> { CyDuiBaiId, 1 }, actionId, new List<int> { itemId, itemNum }, outtime, 1, SendTime)
            {
                sceneName = Message
            };
            player.emailDateMag.AddNewEmail(NPCId.ToString(), emailData);
        }

        //移除传音符联系人
        public static void RemoveFriend(int npcId)
        {
            int id = NPCEx.NPCIDToNew(npcId);
            if (player.emailDateMag.IsFriend(id))
            {
                player.emailDateMag.cyNpcList.Remove(id);
            }
        }

    }
}
