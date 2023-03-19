using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ventulus
{
    [BepInDependency("skyswordkill.plugin.Next", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("Ventulus.MCS.VTools", "微风的工具库", "1.2.0")]
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
        private System.Random random = new System.Random();
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

        protected static KBEngine.Avatar player => Tools.instance.getPlayer();

        //快速找到当前时间
        public static DateTime NowTime => player.worldTimeMag.getNowTime();
        public static string nowTime => player.worldTimeMag.nowTime;

        //随机数
        public static int GetRandom()
        {
            return Instance.random.Next();
        }
        public static int GetRandom(int min, int max)
        {
            return Instance.random.Next(min, max);
        }

        //根据NPCid获取姓名
        public static string GetNPCName(int NPCid)
        {
            //一般情情况在AvatarRandomJsonData有，死了在npcDeathJson里记newid，实在不行再AvatarJsonData里找Oldid
            NPCid = NPCEx.NPCIDToNew(NPCid);
            int Oldid = NPCEx.NPCIDToOld(NPCid);
            if (NPCid == 0)
                return "旁白";
            else if (NPCid == 1)
                return Tools.GetPlayerName();
            else if (jsonData.instance.AvatarRandomJsonData.HasField(NPCid.ToString()))
                return jsonData.instance.AvatarRandomJsonData[NPCid.ToString()]["Name"].str.ToCN();
            else if (NpcJieSuanManager.inst.npcDeath.npcDeathJson.HasField(NPCid.ToString()))
                return NpcJieSuanManager.inst.npcDeath.npcDeathJson[NPCid.ToString()]["deathName"].str.ToCN();
            else if (jsonData.instance.AvatarJsonData.HasField(Oldid.ToString()))
            {
                JSONObject jsonobject = jsonData.instance.AvatarJsonData[Oldid.ToString()];
                return jsonobject["FirstName"].str.ToCN() + jsonobject["Name"].str.ToCN();
            }
            else
                return "未知";
        }
        //获取显示的npcid字符串
        public static string MakeNPCIdStr(int NPCid)
        {
            NPCid = NPCEx.NPCIDToNew(NPCid);
            int Oldid = NPCEx.NPCIDToOld(NPCid);
            string str = NPCid >= 20000 ? NPCid.ToString() : string.Empty;
            if (Oldid < 20000)
                str += $"({Oldid})";
            return str;
        }

        //发送任意信息OldEmail，联系人和发信人可不相同，每有一个发信人需占用一个传音符Id，不可携带物品（要携带物品则需要再单独占用传音符Id）。不支持任务
        private static readonly string CyFuOld = @"{""id"":100000,""AvatarID"":1,""info"":""{DiDian}"",""Type"":3,""DelayTime"":[]}";
        public static void SendOldEmail(int ContactNPCid, int SenderNPCid, string Message, string SendTime = "")
        {
            ContactNPCid = NPCEx.NPCIDToNew(ContactNPCid);
            SenderNPCid = NPCEx.NPCIDToNew(SenderNPCid);
            int CyFuId = 100000 + SenderNPCid;
            if (!player.NewChuanYingList.HasField(CyFuId.ToString()))
            {
                JSONObject emailjson = new JSONObject(CyFuOld);
                emailjson.SetField("id", CyFuId.ToString());
                emailjson.SetField("AvatarID", SenderNPCid.ToString());
                emailjson.SetField("sendTime", nowTime);
                emailjson.SetField("CanCaoZuo", false);
                emailjson.SetField("AvatarName", GetNPCName(SenderNPCid));//关键就在于显示的名字不同
                //LogMessage(emailjson.ToString());
                player.NewChuanYingList.SetField(CyFuId.ToString(), emailjson);
            }

            //加入新传音符
            player.AddFriend(ContactNPCid);
            if (NPCEx.NPCIDToNew(ContactNPCid) < 20000 && !jsonData.instance.AvatarJsonData[ContactNPCid.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[ContactNPCid.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(SendTime))
            {
                SendTime = nowTime;
            }
            EmailData emailData = new EmailData(ContactNPCid, isOld: true, CyFuId, SendTime)
            {
                sceneName = Message
            };
            player.emailDateMag.AddNewEmail(ContactNPCid.ToString(), emailData);
        }

        //发送任意信息NewEmail，发信人只能是联系人（因为发信人名字限制死了只能为邮件npcid），可携带物品及数量，只占用对白表100000号.actionId=1是发送物品给玩家，2是向玩家请求物品，这种情况下outtime为npc等待月份如果超时npc回答不同，且好感度固定只加1（加情分按物品价值）
        private static readonly string CyDuiBaiNew = @"{""id"":100000,""Type"":100000,""XingGe"":1,""dir1"":""{DiDian}"",""dir2"":""{DiDian}"",""dir3"":""{DiDian}""}";
        public static void SendNewEmail(int ContactNPCid, string Message, string SendTime = "", int actionId = 0, int itemId = 0, int itemNum = 0, int outtime = 60)
        {
            ContactNPCid = NPCEx.NPCIDToNew(ContactNPCid);
            int CyDuiBaiId = 100000;
            if (!jsonData.instance.CyNpcDuiBaiData.HasField(CyDuiBaiId.ToString()))
            {
                JSONObject emailjson = new JSONObject(CyDuiBaiNew);
                //LogMessage(emailjson.ToString());
                jsonData.instance.CyNpcDuiBaiData.SetField(CyDuiBaiId.ToString(), emailjson);
            }
            //加入新传音符
            player.AddFriend(ContactNPCid);
            if (NPCEx.NPCIDToNew(ContactNPCid) < 20000 && !jsonData.instance.AvatarJsonData[ContactNPCid.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[ContactNPCid.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(SendTime))
            {
                SendTime = nowTime;
            }
            EmailData emailData = new EmailData(ContactNPCid, isOut: false, isComplete: false, new List<int> { CyDuiBaiId, 1 }, actionId, new List<int> { itemId, itemNum }, outtime, 1, SendTime)
            {
                sceneName = Message
            };
            player.emailDateMag.AddNewEmail(ContactNPCid.ToString(), emailData);
        }

        //请作者查询task表中的（委托任务）“任务大类”和“详细任务”。NTaskid为任务大类的id，会安其中的“详细任务随机范围”在“详细任务”表中按“Type”符合的子任务进行随机选取，同时也要符合玩家的境界区间。
        //发送委托任务邮件，可重复发送不同内容，每个NTaskid占一个传音符id，发信人名字固定显示"委托任务"，可选择联系人。发送前会执行对NTask随机生成子类详细任务，bool Reset是强制重随任务,默认为false则上次随机后还未出cd就不重随。随任务注意子任务有境界限制，可能玩家境界正好都不符合而生成失败。
        public static bool SendNTaskEmail(int ContactNPCid, int NTaskid, string Message, string SendTime = "", bool Reset = false)
        {
            ContactNPCid = NPCEx.NPCIDToNew(ContactNPCid);
            int CyFuId = 200000 + NTaskid;
            if (!player.NewChuanYingList.HasField(CyFuId.ToString()))
            {
                JSONObject emailjson = new JSONObject(CyFuOld);
                emailjson.SetField("id", CyFuId.ToString());
                emailjson.SetField("AvatarID", ContactNPCid.ToString());
                emailjson.SetField("sendTime", nowTime);
                emailjson.SetField("CanCaoZuo", true);
                emailjson.SetField("WeiTuo", NTaskid);
                emailjson.SetField("AvatarName", "委托任务");
                //LogMessage(emailjson.ToString());

                //先要造一个NTask
                Tools.instance.getPlayer().nomelTaskMag.randomTask(NTaskid, Reset);
                if (PlayerEx.Player.NomelTaskJson.HasField(NTaskid.ToString()))
                {
                    //Instance.Logger.LogInfo(PlayerEx.Player.NomelTaskJson[NTaskid.ToString()].ToString().ToCN());
                }
                else
                {
                    Instance.Logger.LogInfo(NTaskid.ToString() + "任务生成失败");
                    return false;
                }

                player.NewChuanYingList.SetField(CyFuId.ToString(), emailjson);
            }

            //加入新传音符
            player.AddFriend(ContactNPCid);
            if (NPCEx.NPCIDToNew(ContactNPCid) < 20000 && !jsonData.instance.AvatarJsonData[ContactNPCid.ToString()].HasField("ActionId"))
            {
                jsonData.instance.AvatarJsonData[ContactNPCid.ToString()].SetField("ActionId", 1);
            }
            if (string.IsNullOrEmpty(SendTime))
            {
                SendTime = nowTime;
            }
            EmailData emailData = new EmailData(ContactNPCid, isOld: true, CyFuId, SendTime)
            {
                sceneName = Message
            };
            player.emailDateMag.AddNewEmail(ContactNPCid.ToString(), emailData);
            return true;
        }
        //移除传音符联系人
        public static void RemoveFriend(int NPCid)
        {
            int id = NPCEx.NPCIDToNew(NPCid);
            if (player.emailDateMag.IsFriend(id))
            {
                player.emailDateMag.cyNpcList.Remove(id);
            }
        }
        //大小境界转换
        public static int LevelToBigLevel(int level)
        {
            return (level - 1) / 3 + 1;
        }
        public static int BigLevelToLevel(int biglevel)
        {
            return (biglevel - 1) * 3 + 1;
        }
        //最近的某一个月，默认找6月
        public static DateTime RecentMonth(DateTime lasttime, int month = 6)
        {
            DateTime temptime = new DateTime(lasttime.Year, month, lasttime.Day);
            if (temptime < lasttime)
                return temptime.AddYears(1);
            else
                return temptime;
        }
        //按类型和境界生成npc
        public static int CreateNpcByTypeAndLevel(int type, int level, int banliupai = 0)
        {
            List<JSONObject> list = jsonData.instance.NPCLeiXingDate.list.Where(x => x["Type"].I == type && x["Level"].I == level && x["LiuPai"].I != banliupai).ToList();
            if (list.Count > 0)
            {
                int j = VTools.GetRandom(0, list.Count);
                return FactoryManager.inst.npcFactory.AfterCreateNpc(list[j], isImportant: false, ZhiDingindex: 0, isNewPlayer: false);
            }
            return 0;
        }
    }
    //权重字典类移动至工具
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
