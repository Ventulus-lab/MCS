using BepInEx;
using HarmonyLib;
using JSONClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Ventulus
{
    [BepInDependency("skyswordkill.plugin.Next", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("Ventulus.MCS.VTools", "微风的工具库", "1.6.1")]
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
            MessageMag.Instance.Register(MessageName.MSG_Npc_JieSuan_COMPLETE, new Action<MessageData>(JieSuanComplete));
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
        public void JieSuanComplete(MessageData data = null)
        {
            VTools.LogMessage("结算完成了");
            StartCoroutine(AfterJieSuan());
        }
        IEnumerator AfterJieSuan()
        {
            //VTools.LogMessage("开始协程");
            yield return new WaitForSeconds(2f);
            VNext.DialogTrigger.JieSuanComplete.CallTrigger();
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
                player.nomelTaskMag.randomTask(nTaskId, reset);
                if (player.NomelTaskJson.HasField(nTaskId.ToString()))
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
                return FactoryManager.inst.npcFactory.AfterCreateNpc(list[j], isImportant: false, ZhiDingindex: 0, isNewPlayer: false, importantJson: null);
            }
            return 0;
        }
        ///   <summary>  
        ///   按条件生成npc，不强制要求的参数写成0，结果为id
        ///   </summary>  
        public static int CreateNpc(int type = 0, int liuPai = 0, int level = 0, int sex = 0, int zhengXie = 0)
        {
            List<JSONObject> list = jsonData.instance.NPCLeiXingDate.list;
            if (type > 0)
                list = list.Where(x => x["Type"].I == type).ToList();
            if (liuPai > 0)
                list = list.Where(x => x["LiuPai"].I == liuPai).ToList();
            if (level > 0)
                list = list.Where(x => x["Level"].I == level).ToList();
            if (list.Count > 0)
            {
                int j = VTools.GetRandom(0, list.Count);
                int result = FactoryManager.inst.npcFactory.AfterCreateNpc(list[j], isImportant: false, ZhiDingindex: 0, isNewPlayer: false, importantJson: null, setSex: sex);
                if (zhengXie == 1 || zhengXie == 2)
                {
                    int xingGe = FactoryManager.inst.npcFactory.getRandomXingGe(zhengXie);
                    jsonData.instance.AvatarJsonData[result.ToString()].SetField("XingGe", xingGe);
                }
                LogMessage("CreateNpc id" + result.ToString());
                return result;
            }
            LogMessage("CreateNpc Fail");
            return 0;
        }

        ///   <summary>  
        ///   按条件筛选npc，不强制要求的参数写成0，结果为id列表
        ///   </summary>  
        public static List<int> SearchNpc(int type = 0, int liuPai = 0, int level = 0, int sex = 0, int zhengXie = 0)
        {
            //LogMessage("SearchNpc tiaojian " + type + liuPai + level + sex + zhengXie);
            List<JSONObject> list = jsonData.instance.AvatarJsonData.list.Where(x => x["id"].I >= 20000 && !x.HasField("IsFly")).ToList();
            if (type > 0)
                list = list.Where(x => x["Type"].I == type).ToList();
            if (liuPai > 0)
                list = list.Where(x => x["LiuPai"].I == liuPai).ToList();
            if (level > 0)
                list = list.Where(x => x["Level"].I == level).ToList();
            if (sex > 0)
                list = list.Where(x => x["SexType"].I == sex).ToList();
            if (zhengXie == 1)
                list = list.Where(x => x["XingGe"].I < 10).ToList();
            if (zhengXie == 2)
                list = list.Where(x => x["XingGe"].I > 10).ToList();
            LogMessage("SearchNpc num" + list.Count);
            return list.Select(x => x["id"].I).ToList();
        }

        ///   <summary>  
        ///   npc执行行动，会改变行动id，地点，获得行动报酬
        ///   </summary>  
        public static bool NpcDoAction(int npcId, int actionId)
        {
            npcId = NPCEx.NPCIDToNew(npcId);
            if (npcId < 20000) return false;
            if (NpcJieSuanManager.inst.ActionDictionary.ContainsKey(actionId))
            {
                NpcMapRemoveNpc(npcId);
                jsonData.instance.AvatarJsonData[npcId.ToString()].SetField("ActionId", actionId);
                NpcJieSuanManager.inst.ActionDictionary[actionId](npcId);
                NpcJieSuanManager.inst.isUpDateNpcList = true;
                return true;
            }
            return false;
        }
        ///   <summary>  
        ///   移除地图上特定npc
        ///   </summary> 
        public static bool NpcMapRemoveNpc(int npcId)
        {
            //原版也有一个NPCMap.RemoveNpcByList，但原版的只移除一次就返回了，而且表中有特殊隐藏npc就不处理（特殊key）
            npcId = NPCEx.NPCIDToNew(npcId);
            bool removed = false;
            //地点在副本
            foreach (Dictionary<int, List<int>> fubendict in NpcJieSuanManager.inst.npcMap.fuBenNPCDictionary.Values)
            {
                foreach (List<int> posdict in fubendict.Values)
                {
                    if (posdict.Contains(npcId))
                    {
                        posdict.Remove(npcId);
                        removed = true;
                    }
                }
            }
            //地点在大地图
            foreach (List<int> ludiandict in NpcJieSuanManager.inst.npcMap.bigMapNPCDictionary.Values)
            {
                if (ludiandict.Contains(npcId))
                {
                    ludiandict.Remove(npcId);
                    removed = true;
                }

            }
            //地点在三级场景
            foreach (List<int> threedict in NpcJieSuanManager.inst.npcMap.threeSenceNPCDictionary.Values)
            {
                if (threedict.Contains(npcId))
                {
                    threedict.Remove(npcId);
                    removed = true;
                }

            }
            //刷新玩家当前所在场景的npc
            NpcJieSuanManager.inst.isUpDateNpcList = true;
            return removed;
        }
        ///   <summary>  
        ///   传送npc至指定场景
        ///   </summary>  
        public static bool NpcWarp(int npcId, string scene, int index = 0)
        {
            npcId = NPCEx.NPCIDToNew(npcId);
            if (NPCEx.IsDeath(npcId))
                return false;
            LogMessage("NpcWarp " + npcId + scene);
            if (scene.StartsWith("AllMaps"))
            {
                //这里也只清了大地图中的npc
                NpcMapRemoveNpc(npcId);
                NPCEx.WarpToMap(npcId, index);
                return true;
            }
            else if (scene.StartsWith("F"))
            {
                // NPCEx.WarpToPlayerNowFuBen仅为传送到玩家当前所在副本
                NpcMapRemoveNpc(npcId);
                if (index == 0 && player.NowFuBen == scene) index = player.fubenContorl[scene].NowIndex;
                if (index == 0) index = 1;
                Dictionary<string, Dictionary<int, List<int>>> fuBenDict = NpcJieSuanManager.inst.npcMap.fuBenNPCDictionary;
                if (!fuBenDict.ContainsKey(scene))
                {
                    fuBenDict.Add(scene, new Dictionary<int, List<int>>());
                }
                if (!fuBenDict[scene].ContainsKey(index))
                {
                    fuBenDict[scene].Add(index, new List<int>());
                }
                if (!fuBenDict[scene][index].Contains(npcId))
                {
                    fuBenDict[scene][index].Add(npcId);
                }
                NpcJieSuanManager.inst.isUpDateNpcList = true;
                return true;
            }
            else if (scene.StartsWith("S"))
            {
                //原版只移除三级场景中的，不行
                //NPCEx.WarpToScene(npcId, scene);
                NpcMapRemoveNpc(npcId);
                Dictionary<string, List<int>> threeDict = NpcJieSuanManager.inst.npcMap.threeSenceNPCDictionary;
                if (!threeDict.ContainsKey(scene))
                {
                    threeDict.Add(scene, new List<int>());
                }
                if (!threeDict[scene].Contains(npcId))
                {
                    threeDict[scene].Add(npcId);
                }
                if (scene == "S101")
                {
                    jsonData.instance.AvatarJsonData[npcId.ToString()].SetField("DongFuId", index);
                }
                NpcJieSuanManager.inst.isUpDateNpcList = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        ///   <summary>  
        ///   传送玩家至指定场景
        ///   </summary>  
        public static bool PlayerWarp(string scene, int index = 0)
        {
            LogMessage("NowMapIndex " + player.NowMapIndex);
            if (scene.StartsWith("AllMaps"))
            {
                if (index <= 0) index = 101;
                player.NowMapIndex = index;
                Tools.instance.loadMapScenes(scene);
                return true;
            }
            else if (scene.StartsWith("F"))
            {
                //此方法从大地图进副本出来后还在原地
                //player.NowMapIndex = AutoMapIndex(scene);
                if (index <= 0) index = 1;
                SceneEx.LoadFuBen(scene, index);
                return true;
            }
            else if (scene.StartsWith("Sea"))
            {
                player.NowMapIndex = 29;
                if (index <= 0) index = SeaZuoBiao[scene];
                SceneEx.LoadFuBen(scene, index);
                return true;
            }
            else if (scene.StartsWith("S"))
            {
                //注意S101洞府要手动设置进第几层，推荐手动设置出来大地图的路点位置。
                if (index > 0)
                    player.NowMapIndex = index;
                if (index == -1)
                    player.NowMapIndex = AutoMapIndex(scene);
                LogMessage("set NowMapIndex " + player.NowMapIndex);
                Tools.instance.loadMapScenes(scene);
                return true;
            }
            else
            {
                return false;
            }
        }
        //场景id和大地图上路点的规律并没有严格遵守，所以还是建议手动指定的好
        private static int AutoMapIndex(string name)
        {
            int type = SceneNameJsonData.DataDict[name].MoneyType;
            if (type == 2 || type == 3)
                return 29;
            if (name == "S101")
            {
                if (DongFuManager.NowDongFuID == 1)
                    return 98;
                if (DongFuManager.NowDongFuID == 2)
                {
                    switch (player.menPai)
                    {
                        case 1: return 12;
                        case 3: return 14;
                        case 4: return 16;
                        case 5: return 15;
                        case 6: return 11;
                        default: return 101;
                    }
                }
            }
            if (name.Length <= 3)
                return Convert.ToInt32(name.Remove(0, 1));
            else if (name.StartsWith("S10"))
                return 101;
            else if (name.StartsWith("S1"))
            {
                if (name.Length == 4)
                    return Convert.ToInt32(name.Substring(2, 1));
                else
                    return Convert.ToInt32(name.Substring(2, 2));
            }
            else
                return Convert.ToInt32(name.Substring(1, 2));

        }
        //关于无尽之海中格子的序号：
        //概念上分为小海域，大海域（为什么不是中？），和无尽之海
        //小海域是7*7格的方块。
        //大海域就是我们地图上看到的XX海，所谓Sea2~Sea18，每个都包含若干个完整小海域，形状不同，有配置表@大海域拥有的小海域记录了包含关系
        //无尽之海总共是宽19个小海域，高10个小海域，也就是总宽133格，高70格
        //NodeIndex从1开始，到133*70=9310，是每个格子的编号，按从左到右从上到下顺序排列，转化为第几行第几格NodePos（xy从0开始）
        //同样，小海域的编号，也是在无尽之海中从左往右从上到下计算，可用函数 int GetSmallSeaIDByNodeIndex(int nodeIndex)计算
        //有了小海域id，可通过查询SeaEx.BigSeaHasSmallSeaIDDict(jsonData.instance.EndlessSeaHaiYuData)得知属于哪个我们熟悉的大海域
        //每个格子在小海域中的位置，可以由Vector2Int GetSmallSeaPosByNodePos(Vector2Int nodePos)计算
        //格子在小海域中的序号inseaid（1~49），可转为总无尽之海的Index = EndlessSeaMag.GetRealIndex(int seaID, int index)，也可再转回来SeaNodeMag.GetInSeaID(int AllMapIndex, int wide)
        //结合一下，就可以循环遍历知道每个大海域包含哪些格子序号了。
        private static Dictionary<string, int> SeaZuoBiao = new Dictionary<string, int>()
        {
            {"Sea2",200},//北宁海
            {"Sea3",189},//西宁海
            {"Sea4",1929},//南宁海
            {"Sea5",3217},//千流海域
            {"Sea6",6976},//南崖海域
            {"Sea7",5142},//碎星海域
            {"Sea8",1425},//蓬莎海域
            {"Sea9",1842},//浪方海域
            {"Sea10",5895},//吞云海
            {"Sea11",952},//雷鸣海
            {"Sea12",6144},//图南海
            {"Sea13",6591},//阴冥海
            {"Sea14",7554},//幽冥海
            {"Sea15",7336},//玄冥海
            {"Sea16",3829},//东海
            {"Sea17",5831},//化龙海
            {"Sea18",4172},//无尽海渊
        };
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
