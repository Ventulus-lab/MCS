﻿using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ventulus
{
    [BepInDependency("skyswordkill.plugin.Next", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("Ventulus.MCS.VTools", "微风的工具库", "1.4.0")]
    public class VTools : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            new Harmony("Ventulus.MCS.VTools").PatchAll();
            MessageMag.Instance.Register(MessageName.MSG_GameInitFinish, new Action<MessageData>(InitMailSystem));
            Logger.LogInfo("加载成功");
        }
        public static VTools Instance;
        private static System.Random random = new System.Random();
        public static void LogError(object data)
        {
            Instance.Logger.LogError(data);
        }
        public static void LogWarning(object data)
        {
            Instance.Logger.LogWarning(data);
        }
        public static void LogMessage(object data)
        {
            Instance.Logger.LogMessage(data);
        }
        public static void LogInfo(object data)
        {
            Instance.Logger.LogInfo(data);
        }
        public static void LogDebug(object data)
        {
            Instance.Logger.LogDebug(data);
        }

        public void InitMailSystem(MessageData data = null)
        {
            int CyDuiBaiId = 100000;
            if (!jsonData.instance.CyNpcDuiBaiData.HasField(CyDuiBaiId.ToString()))
            {
                JSONObject emailJson = new JSONObject(newCyDuiBai);
                jsonData.instance.CyNpcDuiBaiData.SetField(CyDuiBaiId.ToString(), emailJson);
            }
        }

        protected static KBEngine.Avatar player => Tools.instance.getPlayer();

        ///   <summary>  
        ///   获取DateTime格式的当前时间
        ///   </summary>
        public static DateTime NowTime => player.worldTimeMag.getNowTime();

        ///   <summary>  
        ///   获取字符串格式的当前时间
        ///   </summary>   
        public static string nowTime => player.worldTimeMag.nowTime;

        ///   <summary>  
        ///   随机数
        ///   </summary>
        public static int GetRandom()
        {
            return random.Next();
        }

        ///   <summary>  
        ///   返回大于等于min且小于max的随机数
        ///   </summary>
        public static int GetRandom(int min, int max)
        {
            return random.Next(min, max);
        }

        ///   <summary>  
        ///   根据npcId获取姓名
        ///   </summary>
        public static string GetNPCName(int npcId)
        {
            //一般情情况在AvatarRandomJsonData有，死了在npcDeathJson里记newId，实在不行再AvatarJsonData里找oldId
            npcId = NPCEx.NPCIDToNew(npcId);
            int oldId = NPCEx.NPCIDToOld(npcId);
            if (npcId == 0)
                return "旁白";
            else if (npcId == 1)
                return Tools.GetPlayerName();
            else if (jsonData.instance.AvatarRandomJsonData.HasField(npcId.ToString()))
                return jsonData.instance.AvatarRandomJsonData[npcId.ToString()]["Name"].str.ToCN();
            else if (NpcJieSuanManager.inst.npcDeath.npcDeathJson.HasField(npcId.ToString()))
                return NpcJieSuanManager.inst.npcDeath.npcDeathJson[npcId.ToString()]["deathName"].str.ToCN();
            else if (jsonData.instance.AvatarJsonData.HasField(oldId.ToString()))
            {
                JSONObject jsonobject = jsonData.instance.AvatarJsonData[oldId.ToString()];
                return jsonobject["FirstName"].str.ToCN() + jsonobject["Name"].str.ToCN();
            }
            else
                return "未知";
        }

        ///   <summary>  
        ///   获取用于显示的npcId字符串  
        ///   </summary>
        public static string MakeNPCIdStr(int npcId)
        {
            npcId = NPCEx.NPCIDToNew(npcId);
            int oldId = NPCEx.NPCIDToOld(npcId);
            string str = npcId >= 20000 ? npcId.ToString() : string.Empty;
            if (oldId < 20000)
                str += $"({oldId})";
            return str;
        }

        ///   <summary>  
        ///   获取当前场景名称
        ///   </summary>
        public static string GetPlaceName()
        {
            string screenName = Tools.getScreenName();
            if (RandomFuBen.IsInRandomFuBen)
            {
                return (string)player.RandomFuBenList[RandomFuBen.NowRanDomFuBenID.ToString()]["Name"];
            }
            else if (screenName == "S101")
            {
                return DongFuManager.GetDongFuName(DongFuManager.NowDongFuID);
            }
            else
            {
                if (!jsonData.instance.SceneNameJsonData.HasField(screenName))
                {
                    return "未知";
                }
                return jsonData.instance.SceneNameJsonData[screenName]["MapName"].Str;
                //其实本来是EventName场景名称，奈何觅长生比较奇葩，MapName地图名称信息更多，比如四大岛的客栈坊市码头，而五宗门广场编号不同但是没做区分。
            }
        }
        //发送任意信息OldEmail，联系人和发信人可不相同，每有一个发信人需占用一个传音符Id，不可携带物品（要携带物品则需要再单独占用传音符Id）。不支持任务
        private static readonly string oldCyFu = @"{""id"":100000,""AvatarID"":1,""info"":""{DiDian}"",""Type"":3,""DelayTime"":[]}";
        public static void SendOldEmail(int contactNpcId, int senderNpcId, string message, string sendTime = "")
        {
            contactNpcId = NPCEx.NPCIDToNew(contactNpcId);
            senderNpcId = NPCEx.NPCIDToNew(senderNpcId);
            int CyFuId = 100000 + senderNpcId;
            if (!player.NewChuanYingList.HasField(CyFuId.ToString()))
            {
                JSONObject emailJson = new JSONObject(oldCyFu);
                emailJson.SetField("id", CyFuId.ToString());
                emailJson.SetField("AvatarID", senderNpcId.ToString());
                emailJson.SetField("sendTime", nowTime);
                emailJson.SetField("CanCaoZuo", false);
                emailJson.SetField("AvatarName", GetNPCName(senderNpcId));//关键就在于显示的名字不同
                //LogMessage(emailJson.ToString());
                player.NewChuanYingList.SetField(CyFuId.ToString(), emailJson);
            }

            //加入新传音符
            player.AddFriend(contactNpcId);
            if (NPCEx.NPCIDToNew(contactNpcId) < 20000 && !jsonData.instance.AvatarJsonData[contactNpcId.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[contactNpcId.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(sendTime))
            {
                sendTime = nowTime;
            }
            EmailData emailData = new EmailData(contactNpcId, isOld: true, CyFuId, sendTime)
            {
                sceneName = message
            };
            player.emailDateMag.AddNewEmail(contactNpcId.ToString(), emailData);
        }

        //发送任意信息NewEmail，发信人只能是联系人（因为发信人名字限制死了只能为邮件npcId），可携带物品及数量，只占用对白表100000号.actionId=1是发送物品给玩家，2是向玩家请求物品，这种情况下outTime为npc等待月份如果超时npc回答不同，且好感度固定只加1（加情分按物品价值）
        private static readonly string newCyDuiBai = @"{""id"":100000,""Type"":100000,""XingGe"":1,""dir1"":""{DiDian}"",""dir2"":""{DiDian}"",""dir3"":""{DiDian}""}";
        public static void SendNewEmail(int contactNpcId, string message, string sendTime = "", int actionId = 0, int itemId = 0, int itemNum = 0, int outTime = 60)
        {
            contactNpcId = NPCEx.NPCIDToNew(contactNpcId);
            int CyDuiBaiId = 100000;
            if (!jsonData.instance.CyNpcDuiBaiData.HasField(CyDuiBaiId.ToString()))
            {
                JSONObject emailJson = new JSONObject(newCyDuiBai);
                //LogMessage(emailJson.ToString());
                jsonData.instance.CyNpcDuiBaiData.SetField(CyDuiBaiId.ToString(), emailJson);
            }
            //加入新传音符
            player.AddFriend(contactNpcId);
            if (NPCEx.NPCIDToNew(contactNpcId) < 20000 && !jsonData.instance.AvatarJsonData[contactNpcId.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[contactNpcId.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(sendTime))
            {
                sendTime = nowTime;
            }
            EmailData emailData = new EmailData(contactNpcId, isOut: false, isComplete: false, new List<int> { CyDuiBaiId, 1 }, actionId, new List<int> { itemId, itemNum }, outTime, 1, sendTime)
            {
                sceneName = message
            };
            player.emailDateMag.AddNewEmail(contactNpcId.ToString(), emailData);
        }

        //请作者查询task表中的（委托任务）“任务大类”和“详细任务”。nTaskId为任务大类的id，会安其中的“详细任务随机范围”在“详细任务”表中按“Type”符合的子任务进行随机选取，同时也要符合玩家的境界区间。
        //发送委托任务邮件，可重复发送不同内容，每个nTaskId占一个传音符id，发信人名字固定显示"委托任务"，可选择联系人。发送前会执行对NTask随机生成子类详细任务，bool reset是强制重随任务,默认为false则上次随机后还未出cd就不重随。随任务注意子任务有境界限制，可能玩家境界正好都不符合而生成失败。
        public static bool SendNTaskEmail(int contactNpcId, int nTaskId, string message, string sendTime = "", bool reset = false)
        {
            contactNpcId = NPCEx.NPCIDToNew(contactNpcId);
            int CyFuId = 200000 + nTaskId;
            if (!player.NewChuanYingList.HasField(CyFuId.ToString()))
            {
                JSONObject emailJson = new JSONObject(oldCyFu);
                emailJson.SetField("id", CyFuId.ToString());
                emailJson.SetField("AvatarID", contactNpcId.ToString());
                emailJson.SetField("sendTime", nowTime);
                emailJson.SetField("CanCaoZuo", true);
                emailJson.SetField("WeiTuo", nTaskId);
                emailJson.SetField("AvatarName", "委托任务");
                //LogMessage(emailJson.ToString());

                //先要造一个NTask
                Tools.instance.getPlayer().nomelTaskMag.randomTask(nTaskId, reset);
                if (PlayerEx.Player.NomelTaskJson.HasField(nTaskId.ToString()))
                {
                    //LogInfo(PlayerEx.Player.NomelTaskJson[nTaskId.ToString()].ToString().ToCN());
                }
                else
                {
                    LogInfo(nTaskId.ToString() + "任务生成失败");
                    return false;
                }

                player.NewChuanYingList.SetField(CyFuId.ToString(), emailJson);
            }

            //加入新传音符
            player.AddFriend(contactNpcId);
            if (NPCEx.NPCIDToNew(contactNpcId) < 20000 && !jsonData.instance.AvatarJsonData[contactNpcId.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[contactNpcId.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(sendTime))
            {
                sendTime = nowTime;
            }
            EmailData emailData = new EmailData(contactNpcId, isOld: true, CyFuId, sendTime)
            {
                sceneName = message
            };
            player.emailDateMag.AddNewEmail(contactNpcId.ToString(), emailData);
            return true;
        }

        ///   <summary>  
        ///   移除传音符联系人好友
        ///   </summary>
        public static void RemoveFriend(int npcId)
        {
            npcId = NPCEx.NPCIDToNew(npcId);
            int oldId = NPCEx.NPCIDToOld(npcId);
            if (player.emailDateMag.IsFriend(npcId))
            {
                player.emailDateMag.cyNpcList.Remove(npcId);
            }
            if (player.emailDateMag.cyNpcList.Contains(oldId))
            {
                player.emailDateMag.cyNpcList.Remove(oldId);
            }
        }

        ///   <summary>  
        ///   获取接下来最近的某一个月，默认找最近的6月
        ///   </summary>
        public static DateTime RecentMonth(DateTime lastTime, int month = 6)
        {
            DateTime tempTime = new DateTime(lastTime.Year, month, lastTime.Day);
            if (tempTime < lastTime)
                return tempTime.AddYears(1);
            else
                return tempTime;
        }

        ///   <summary>  
        ///   按类型和境界生成npc，可选排除特定流派
        ///   </summary>  
        public static int CreateNpcByTypeAndLevel(int type, int level, int banLiuPai = 0)
        {
            List<JSONObject> list = jsonData.instance.NPCLeiXingDate.list.Where(x => x["Type"].I == type && x["Level"].I == level && x["LiuPai"].I != banLiuPai).ToList();
            if (list.Count > 0)
            {
                int j = VTools.GetRandom(0, list.Count);
                return FactoryManager.inst.npcFactory.AfterCreateNpc(list[j], isImportant: false, ZhiDingindex: 0, isNewPlayer: false);
            }
            return 0;
        }
    }
    ///   <summary>  
    ///   带权重的字典  
    ///   </summary>
    public class WeightDictionary
    {
        public Dictionary<int, double> WeightDict
        {
            get;
            set;
        }
        public Dictionary<int, string> NameDict
        {
            get;
            set;
        }
        private SortedDictionary<int, double> _sortDict;
        private Dictionary<int, double> _percentDict;

        public WeightDictionary()
        {
            WeightDict = new Dictionary<int, double>();
            NameDict = new Dictionary<int, string>();
        }
        public WeightDictionary(Dictionary<int, double> weightdict)
        {
            WeightDict = new Dictionary<int, double>(weightdict);
            NameDict = new Dictionary<int, string>();
        }
        public WeightDictionary(Dictionary<int, string> namedict)
        {
            WeightDict = new Dictionary<int, double>();
            NameDict = new Dictionary<int, string>(namedict);
        }

        public WeightDictionary(Dictionary<int, double> weightdict, Dictionary<int, string> namedict)
        {
            WeightDict = new Dictionary<int, double>(weightdict);
            NameDict = new Dictionary<int, string>(namedict);
        }
        public WeightDictionary(WeightDictionary weightdictionary)
        {
            WeightDict = new Dictionary<int, double>(weightdictionary.WeightDict);
            NameDict = new Dictionary<int, string>(weightdictionary.NameDict);
        }
        public void AddWeight(int key)
        {
            AddWeight(key, 1);
        }
        public void AddWeight(int key, double num)
        {
            if (WeightDict.ContainsKey(key))
                WeightDict[key] += num;
            else
                WeightDict.Add(key, num);
        }
        public override string ToString()
        {
            StringBuilder SB = new StringBuilder();
            SB.Append(Environment.NewLine);
            _sortDict = new SortedDictionary<int, double>(WeightDict);
            foreach (var item in _sortDict)
            {
                SB.Append("[");
                SB.Append(item.Key);
                if (NameDict.ContainsKey(item.Key))
                    SB.Append(NameDict[item.Key]);
                SB.Append(",");
                SB.Append(item.Value);
                SB.Append("]");
                SB.Append(Environment.NewLine);
            }
            return SB.ToString();
        }
        public void Normalization()
        {
            WeightDict = Normalization(WeightDict);
        }
        public Dictionary<int, double> Normalization(Dictionary<int, double> weightdict)
        {
            _sortDict = new SortedDictionary<int, double>();
            double sum = weightdict.Values.Where(x => x >= 0).Sum();
            foreach (var item in weightdict)
            {
                _sortDict.Add(item.Key, item.Value > 0 ? item.Value / sum : 0);
            }
            return new Dictionary<int, double>(_sortDict);
        }
        public Dictionary<int, double> PositiveSubtraction(Dictionary<int, double> percentdict2)
        {
            Normalization();
            _percentDict = Normalization(percentdict2);
            _sortDict = new SortedDictionary<int, double>();
            foreach (var item in WeightDict)
            {
                if (_percentDict.ContainsKey(item.Key))
                {
                    double m = item.Value - _percentDict[item.Key];
                    _sortDict.Add(item.Key, m > 0 ? m : 0);
                }
                else
                {
                    _sortDict.Add(item.Key, item.Value);
                }
            }
            if (_sortDict.Values.Where(x => x >= 0).Sum() <= 0)
            {
                return new Dictionary<int, double>(WeightDict);
            }
            return new Dictionary<int, double>(_sortDict);
        }
        public static System.Random random = new System.Random((int)GetRandomLong());
        public static long GetRandomLong()
        {
            byte[] array = new byte[8];
            new RNGCryptoServiceProvider().GetBytes(array);
            return BitConverter.ToInt64(array, 0);
        }
        public static double GetRandomDoubleRoll(double max)
        {
            if (max <= 0) return 0;
            double result;
            do
            {
                result = Math.Abs((double)GetRandomLong() / long.MaxValue);
            } while (result >= max);
            return result;
        }
        public double GetRandomDoubleRoll2(double max)
        {
            if (max <= 0) return 0;
            double result;
            do
            {
                result = random.NextDouble();
            } while (result >= max);
            return result;
        }
        public int RollByWeight(out double roll)
        {
            double sum = WeightDict.Values.Where(x => x >= 0).Sum();
            roll = GetRandomDoubleRoll2(sum);
            if (sum <= 0) return 0;

            double countsum = 0;
            foreach (var item in WeightDict)
            {
                if (item.Value <= 0) continue;
                countsum += item.Value;
                if (countsum > roll)
                {
                    return item.Key;
                }
            }
            return 0;
        }
    }
}
