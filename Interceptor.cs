using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.Dapper;
using MobileGame.Core.Tasks;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;
using MobileGame.tianzi.Repository;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi
{
    #region 玩家界面数据
    /// <summary>
    /// 玩家界面数据
    /// </summary>
    public class PlayerData
    {
        /// <summary>
        /// 等级
        /// </summary>
        [Tag(1)]
        public int Level { get; set; }
        /// <summary>
        /// 经验值
        /// </summary>
        [Tag(2)]
        public int Exp { get; set; }
        /// <summary>
        /// Money
        /// </summary>
        [Tag(3)]
        public int Money { get; set; }
        /// <summary>
        /// 铜钱
        /// </summary>
        [Tag(4)]
        public int Coin { get; set; }
        /// <summary>
        /// VIP等级
        /// </summary>
        [Tag(5)]
        public int VipLevel { get; set; }
        /// <summary>
        /// 魅力——竞技场兑换妃子
        /// </summary>
        [Tag(6)]
        public int Charm { get; set; }
        /// <summary>
        /// 声望——爵位系统 团队战获得
        /// </summary>
        [Tag(7)]
        public int Repute { get; set; }
        /// <summary>
        /// 荣誉值，竞技场兑换物品使用
        /// </summary>
        [Tag(8)]
        public int Honor { get; set; }
        /// <summary>
        /// 当前体力
        /// </summary>
        [Tag(9)]
        public int Sp { get; set; }
        /// <summary>
        /// 木材
        /// </summary>
        [Tag(10)]
        public int Wood { get; set; }
        /// <summary>
        /// 石头
        /// </summary>
        [Tag(11)]
        public int Stone { get; set; }
        /// <summary>
        /// 铁矿
        /// </summary>
        [Tag(12)]
        public int Iron { get; set; }
        /// <summary>
        /// 最大铜钱
        /// </summary>
        [Tag(13)]
        public int MaxCoin { get; set; }
        /// <summary>
        /// 最大木材
        /// </summary>
        [Tag(14)]
        public int MaxWood { get; set; }
        /// <summary>
        /// 最大石头
        /// </summary>
        [Tag(15)]
        public int MaxStone { get; set; }
        /// <summary>
        /// 最大铁矿
        /// </summary>
        [Tag(16)]
        public int MaxIron { get; set; }
        /// <summary>
        /// 是否有新消息的列表
        ///  No.1代表竞技场、No.2代表邮件、No.3代表大地图、No.4代表抽奖、No.5代表每日任务、No.6代表主线任务
        /// No.7代表后宫、No.8代表充值奖励、No.9代表登录奖励
        /// </summary>
        [Tag(17)]
        public List<int> HasNewMsgList { get; set; }
        /// <summary>
        /// 当前体力购买次数
        /// </summary>
        [Tag(18)]
        public int CurSpBuyNum { get; set; }
        /// <summary>
        /// 新手引导的步骤数列表[1,2,3]代表1大步，2小步，3小小步，以此类推
        /// </summary>
        [Tag(19)]
        public List<int> GuideStepList { get; set; }

        /// <summary>
        /// 下次可以分享的时间
        /// </summary>
        [Tag(20)]
        public DateTime NextCanShareTime { get; set; }

        /// <summary>
        /// 爵位
        /// </summary>
        [Tag(21)]
        public TitleType TitleType { get; set; }

        /// <summary>
        /// 封号名称
        /// </summary>
        [Tag(22)]
        public string TitleName { get; set; }

        /// <summary>
        /// 下次增加体力的时间
        /// </summary>
        [Tag(23)]
        public DateTime NextAddSpTime { get; set; }
    }
    #endregion

    #region VIP等级计算
    /// <summary>
    /// VIP等级计算
    /// </summary>
    public class VipCompute : IVipCompute
    {
        public int Compute(int userId, int totalCharge, int chargeType, int chargeNum)
        {
            UserRole userRole;
            UserTask userTask;
            DataStorage.Current.Load(out userRole, out userTask, userId, true);

            //充值奖励新消息
            userTask.ChangeRechargeNewMsg(userRole);

            for (int i = SysVipCfg.Items.Count - 1; i >= 0; i--)
            {
                if (totalCharge >= SysVipCfg.Items[i].Recharge)
                {
                    userRole.VipExpireTime = DateTime.Now.AddYears(100);
                    // 保证vip充值后不下降
                    var newVipLevel = SysVipCfg.Items[i].VipLevel;
                    //if (SysVipCfg.Items[i].VipLevel >= userRole.VipLevel)
                    //{
                        //return i;
                    //}
//                    else
                    if(newVipLevel < userRole.VipLevel)
                    {
                        newVipLevel = SysVipCfg.Items.FindIndex(o => o.VipLevel == userRole.VipLevel);
                        //return Utility.VipChangeNum(userRole.VipLevel, levelIndex, userId, chargeNum);
                        //return levelIndex;
                    }
                    return Utility.VipChangeNum(userRole.VipLevel, newVipLevel, userId, chargeNum);
                }
            }
            return 0;
        }
    }
    #endregion

    #region 登录奖励更新消息
    /// <summary>
    /// 登录奖励更新消息
    /// </summary>
    public class LoginCompute : ILoginCompute
    {
        public int Compute(int userId, int loginDays)
        {
            UserRole userRole;
            UserTask userTask;
            DataStorage.Current.Load(out userRole, out userTask, userId, true);

            //充值奖励新消息
            userTask.ChangeLoginNewMsg(userRole, loginDays);
            return 0;
        }
    }
    #endregion

    #region 检查是否有新消息——暂时无用
    /// <summary>
    /// 检查是否有新消息
    /// </summary>
    public class NewMsg : INewMsg
    {
        public void Check(int userId)
        {
            //UserPk userPk;
            //GMMail gmMail;
            //UserMail userMail;
            //UserRole userRole;
            //UserCity userCity;
            //UserExtract userExtract;
            //DataStorage.Current.Load(out gmMail, out userPk, out userMail, out userRole, out userCity, out userExtract, userId, true);

            //userRole.HasNewMsgList[(int)NewMsgType.Pk] = userPk.HasNewMsg;
            //userRole.HasNewMsgList[(int)NewMsgType.Mail] = gmMail.UnReadNum > 0 ? 1 : 0;//系统邮件
            //userRole.HasNewMsgList[(int)NewMsgType.Mail] = userMail.UnReadNum > 0 ? 1 : 0;//用户邮件
            //userRole.HasNewMsgList[(int)NewMsgType.BigMap] = userCity.HasNewMsg();//大地图新消息
            //userRole.HasNewMsgList[(int)NewMsgType.Extract] = userExtract.CanExtract();//抽奖信息
        }
    }
    #endregion

    #region 每个请求接口需要经过的函数
    public class Interceptor : IFlowInterceptor
    {
        static Interceptor()
        {
            GameApplication.Instance.ApplicationStarted += new EventHandler(Instance_ApplicationStarted);
        }

        static void Instance_ApplicationStarted(object sender, EventArgs e)
        {
            //检查log表是否存在
            Console.WriteLine("Check Log Table.");
            CheckLog();

            using (var conn = Util.ObtainConn(ParamHelper.Get<string>("GameAccountDb")))
            {
            }
        }

        /// <summary>
        /// 检查LOG表是否存在
        /// </summary>
        static void CheckLog()
        {
            var dateStr = DateTime.Now.ToString("yyyyMM");

            //var sql = @"select count(*) from sysobjects where id = object_id('GameLog{Date}')";
            var sql = @"show tables like 'gamelog{Date}';";
            sql = sql.Replace("{Date}", dateStr);
            using (var conn = Util.ObtainConn(ParamHelper.GameLog))
            {
                try
                {
                    Console.WriteLine("sql:{0}", sql);
                    var name = conn.Query<string>(sql).First();
                    Console.WriteLine("name:{0}", name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ex:{0}", ex.Message);
                    Console.WriteLine("Not Find Log Table,Add Log Table {0}", dateStr);
                    Utility.AddLogTable(dateStr);
                }
                //if (conn.Query<int>(sql).First() == 0)
                //{
                //    //不存在表
                //    Console.WriteLine("Not Find Log Table,Add Log Table {0}", dateStr);
                //    Utility.AddLogTable(dateStr);
                //}
            }
        }
        public void BeforeProcess(GameContext context)
        {

        }

        public void AfterProcess(GameContext context)
        {
            if (context.Request.OpCode == 1007)
            {
                if (context.Response.StatusCode == 10000)
                {
                }
            }
        }


        public void PreInit(GameContext context)
        {

        }

        public void PreWrite(GameContext context)
        {
            if (context.Session.ClearNotify())
            {
                var userRole = DataStorage.Current.Load<UserRole>(context.Session.UserId);
                //SysUserUpgradeCfg cfg = SysUserUpgradeCfg.Find(userRole.Level);
                context.Response.PlayerData = new PlayerData
                {
                    Level = userRole.Level,
                    Exp = userRole.Exp,
                    Money = userRole.TotalMoney,
                    Coin = userRole.Coin,
                    VipLevel = userRole.VipLevel,
                    Charm = userRole.Charm,
                    Repute = userRole.Repute,
                    Honor = userRole.Honor,
                    Sp = userRole.Sp,
                    Wood = userRole.Wood,
                    Stone = userRole.Stone,
                    Iron = userRole.Iron,
                    MaxCoin = userRole.MaxCoin,
                    MaxWood = userRole.MaxWood,
                    MaxStone = userRole.MaxStone,
                    MaxIron = userRole.MaxIron,
                    HasNewMsgList = userRole.HasNewMsgList,
                    CurSpBuyNum = (int)userRole.BuySpNum.Value,
                    GuideStepList = userRole.GuideStepList,
                    NextCanShareTime = userRole.NextCanShareTime,
                    TitleType = (TitleType)userRole.TitleLevel,
                    TitleName = string.IsNullOrEmpty(userRole.TitleName) ? "布衣" : userRole.TitleName,
                    NextAddSpTime = userRole.GetNextAddSpTime,
                };
            }
        }
    }
    #endregion

    #region 竞技场每日排名奖励定时器
    /// <summary>
    /// 竞技场玩家排名项
    /// </summary>
    public class PkUserRankItem
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }
    }
    /// <summary>
    /// 竞技场下发奖励定时器
    /// 注：此类定时器，是服务器已开启自动定时任务开始，服务器停止运行所有的定时任务结束
    /// 如果停止服务器的这段时间正好应该是定时器运行的时间那么再次开启服务器后也是不会执行的！
    /// </summary>
    public class PkDayRewardTask : IntervalTask
    {
        public PkDayRewardTask()
        {
            //1秒后执行,轮询的间隔时间设置为最大
            base.DueTime = 10;
            base.Period = Int32.MaxValue;
        }

        protected override void DoWork(object state)
        {
            //5分钟后初始化 竞技场任务【每日奖励】
            HashDict<string> param = new HashDict<string>();
            param["IsInit"] = "1";
            Schedule.AddTask(typeof(StartOrChangePkDayRewardTask), DateTime.Now.AddSeconds(100), param);
        }
    }

    /// <summary>
    /// 开始或者改变每日竞技场奖励任务
    /// </summary>
    public class StartOrChangePkDayRewardTask : ScheduleTaskBase
    {
        static readonly object _object = new object();
        //进入任务执行函数时，task已经从redis删除，所以需要获得任务真正的执行时间必须传参
        public override void DoWork(HashDict<string> param)
        {
            lock (_object)
            {
                var curTaskId = 0;

                var dayRewardDateTimeData = ConfigHelper.DayRewardDateTimeData;
                if (param != null) Logger.InfoFormat("StartOrChangePkDayRewardTask param:{0}", JsonConvert.SerializeObject(param));
                var serverData = Storage.Load<ServerData>((int)ServerDataIdType.Pk, true);
                Logger.InfoFormat("StartOrChangePkDayRewardTask Start serverData:{0}", JsonConvert.SerializeObject(serverData));
                var taskIndex = (int)ServerDataPkIntType.PkTimerTaskId;
                var clearPkTaskIndex = (int)ServerDataPkIntType.ClearPkTaskNum;
                var nextTime = GetNextTime();

                var lastExecTime = DateTime.Now;
                //var isInit = 0;
                //var nowExecTime = lastExecTime;
                if (param != null && param.ContainsKey("ExecTime"))
                {
                    lastExecTime = Convert.ToDateTime(param["ExecTime"]);
                    curTaskId = Convert.ToInt32(param["TaskId"]);
                    //nowExecTime = lastExecTime;
                    //if (param.ContainsKey("isInit")) isInit = Convert.ToInt32(param["isInit"]);
                }
                ScheduleTaskInfo curTask = null;
                //if (curTaskId > 0)
                //{
                //    curTask = Schedule.GetTask(curTaskId);
                    //if (curTask == null && isInit == 0)
                    //{
                    //    Logger.InfoFormat("StartOrChangePkDayRewardTask TaskNotExist:{0}!!!", curTaskId);
                    //    return;
                    //}
                //}

                if (param != null && param.ContainsKey("IsInit") && param["IsInit"] == "1")
                {
                    //刚开始服务器初始化竞技场任务
                    var oldClearPkTaskNum = serverData.IntData[clearPkTaskIndex];
                    var clearPkTaskNum = ParamHelper.Get<int>("ClearPkTaskNum", 0);
                    var oldTaskId = serverData.IntData[taskIndex];
                    Logger.InfoFormat("StartOrChangePkDayRewardTask Init,oldTaskId:{0}!!!", oldTaskId);
                    //已经有任务了
                    if (oldTaskId > 0)
                    {
                        var task = Schedule.GetTask(oldTaskId);
                        if (oldClearPkTaskNum < clearPkTaskNum)
                        {
                            //取消上一次的任务，下面重新生成一个任务
                            serverData.IntData[clearPkTaskIndex] = clearPkTaskNum;
                            if (task != null)
                            {
                                Schedule.CancelTask(oldTaskId);
                                Logger.InfoFormat("StartOrChangePkDayRewardTask Init,Cancel Task:{0}!!!", oldTaskId);
                            }
                            else
                            {
                                Logger.InfoFormat("StartOrChangePkDayRewardTask Init,NotFindCancel Task:{0}!!!", oldTaskId);
                            }
                        }
                        else if (task == null)
                        {
                            //没找到之前的任务，则记录下，下面重新生成一个任务
                            Logger.InfoFormat("StartOrChangePkDayRewardTask Init,Not Find Task:{0}!!!", oldTaskId);
                        }
                        else
                        {
                            //等待任务执行，返回。
                            Logger.InfoFormat("StartOrChangePkDayRewardTask Init,Wait Task:{0}!!!", oldTaskId);


                            //设置最后一次执行方法的时间
                            ConfigHelper.SetDayRewardDateTimeData(new ConfigHelper.DayRewardDateTime()
                            {
                                LastDateTime = DateTime.Now,
                                TaskId = curTaskId
                            });
                            Logger.InfoFormat("StartOrChangePkDayRewardTask DayRewardDateTimeData lastExecTime:{0},TaskId:{1}",
                                dayRewardDateTimeData.LastDateTime.ToString(), dayRewardDateTimeData.TaskId);
                            return;
                        }
                    }

                    //初始化任务/取消任务后初始化
                    if (!nextTime.Equals(DateTime.Now))
                    {
                        //还未到时间
                        var taskId = Schedule.AddTask(typeof(StartOrChangePkDayRewardTask), nextTime, null);
                        serverData.IntData[taskIndex] = taskId;
                        Logger.InfoFormat("StartOrChangePkDayRewardTask Init and Add Task,Time:{0},TaskId:{1}",
                            nextTime.ToString(), taskId);

                        //设置最后一次执行方法的时间
                        ConfigHelper.SetDayRewardDateTimeData(new ConfigHelper.DayRewardDateTime()
                        {
                            LastDateTime = DateTime.Now,
                            TaskId = curTaskId
                        });
                        Logger.InfoFormat("StartOrChangePkDayRewardTask DayRewardDateTimeData lastExecTime:{0}TaskId:{1}",
                    dayRewardDateTimeData.LastDateTime.ToString(), dayRewardDateTimeData.TaskId);
                        Logger.InfoFormat("StartOrChangePkDayRewardTask End serverData:{0}", JsonConvert.SerializeObject(serverData));
                        return;
                    }
                    //等于当前时间 则直接执行下面的奖励下发
                }

                //判断今天是否已经下发了。
                if (serverData.TodayHasIssuedPkReward.Value > 0)
                {
                    Logger.InfoFormat(
                        "StartOrChangePkDayRewardTask Start and Already Exec Task,TodayHasIssuedPkReward:{0}",
                        (int) serverData.TodayHasIssuedPkReward.Value);
                    return;
                }

                //真正任务调用
                Logger.InfoFormat("StartOrChangePkDayRewardTask Real Start!!!");

                var hour = ParamHelper.Get<int>("PkDayRewardDueTime", 21);
                Logger.InfoFormat("StartOrChangePkDayRewardTask DayRewardDateTimeData lastExecTime:{0} TaskId:{1}",
                    dayRewardDateTimeData.LastDateTime.ToString(), dayRewardDateTimeData.TaskId);
                Logger.InfoFormat("StartOrChangePkDayRewardTask lastExecTime:{0}", lastExecTime.ToString());

                //缓存中对比上次执行的时间和任务id，如果都一样则表示执行过了
                //if (dayRewardDateTimeData.LastDateTime.Equals(lastExecTime) &&
                //    dayRewardDateTimeData.TaskId.Equals(curTaskId))
                //{
                //    //有执行过任务了
                //    var taskId = 0;
                //    if (param != null && param.ContainsKey("TaskId"))
                //    {
                //        taskId = Convert.ToInt32(param["TaskId"]);
                //    }
                //    Logger.InfoFormat("StartOrChangePkDayRewardTask Start and Already Exec Task,TaskId:{0}", taskId);

                //    return;
                //}

                //设置最后一次执行方法的时间
                ConfigHelper.SetDayRewardDateTimeData(new ConfigHelper.DayRewardDateTime()
                {
                    LastDateTime = lastExecTime,
                    TaskId = curTaskId,
                });
                Logger.InfoFormat("StartOrChangePkDayRewardTask Set DayRewardDateTimeData lastExecTime:{0},TaskId:{1}",
                    lastExecTime.ToString(), curTaskId);

                //获取前1000000玩家排名 下发排名奖励——Range两个重载方法，传double传int
                //key字段不能用long类型不能反序列化的时候会报错
                var maxRank = SysPkCfg.Items.Max(o => o.EndRank);
                KeyValuePair<Utility.PkRankKey, double>[] rankList =
                    Storage.SortedSets.Range<Utility.PkRankKey>(Utility.PkSetKey, 1.0, maxRank);
                rankList = rankList.Where(o => o.Key.Type == (int)BattleTargetType.User).ToArray();

                Logger.InfoFormat("StartOrChangePkDayRewardTask rankList:{0}", JsonConvert.SerializeObject(rankList.Take(3)));

                var logItem = new GameLogItem();
                logItem.S1 = lastExecTime.ToString();
                logItem.S2 = "";
                logItem.S3 = "";
                logItem.S4 = "";
                logItem.S5 = JsonConvert.SerializeObject(rankList);
                var rindex = 0;
                foreach (var keyValuePair in rankList)
                {
                    var rank = (int)keyValuePair.Value;
                    var userId = keyValuePair.Key.UserId;

                    if (rindex < 3)
                    {
                        Logger.InfoFormat("StartOrChangePkDayRewardTask Down Reward UserId:{0},Rank:{1}", userId, rank);
                        rindex++;
                    }

                    var sysPkCfg = SysPkCfg.Items.FirstOrDefault(o => o.StarRank <= rank && o.EndRank >= rank);
                    if (sysPkCfg != null)
                    {
                        var attach = new List<ItemPair>();

                        attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Money, Num = sysPkCfg.Money });
                        attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Coin, Num = sysPkCfg.Coin });
                        //attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Charm, Num = sysPkCfg.Charm });
                        attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Honor, Num = sysPkCfg.Honor });
                        attach.Add(new ItemPair() { ItemId = sysPkCfg.ToolId, Num = sysPkCfg.ToolNum });
                        var msg = LangResource.GetLangResource(ResourceId.R_DayRankMsg,
                                                       hour, rank);
                        Utility.SendGmMailToTarget(userId, "竞技场每日排名奖励", msg, attach, lastExecTime.ToString());

                        logItem.S2 += rank + ",";
                    }
                    else
                    {
                        logItem.S2 += ",";
                    }
                    logItem.S3 += userId + ",";
                    logItem.S4 += rank + ",";
                }
                GameLogManager.CommonLog((int)SpecialLogType.PkDayRewardLog, 0, 0, logItem);

                var period = ParamHelper.Get<int>("PkDayRewardPeriodTime", 24);
                var now = DateTime.Now;
                lastExecTime = lastExecTime.AddHours(period);
                while (lastExecTime < now)
                {
                    Logger.InfoFormat("Delayed PkDayRewardTask!!!{0}", lastExecTime.ToString());
                    //服务器关闭的这段时间 按照当前排行下发奖励
                    foreach (var keyValuePair in rankList)
                    {
                        var rank = (int)keyValuePair.Value;
                        var userId = keyValuePair.Key.UserId;
                        var sysPkCfg = SysPkCfg.Items.FirstOrDefault(o => o.StarRank <= rank && o.EndRank >= rank);
                        if (sysPkCfg != null)
                        {
                            var attach = new List<ItemPair>();

                            attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Money, Num = sysPkCfg.Money });
                            attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Coin, Num = sysPkCfg.Coin });
                            //attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Charm, Num = sysPkCfg.Charm });
                            attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Honor, Num = sysPkCfg.Honor });
                            attach.Add(new ItemPair() { ItemId = sysPkCfg.ToolId, Num = sysPkCfg.ToolNum });
                            var msg = LangResource.GetLangResource(ResourceId.R_DayRankMsg,
                                                           hour, rank);
                            Utility.SendGmMailToTarget(userId, "竞技场每日排名奖励", msg, attach, lastExecTime.ToString());
                        }
                    }
                    logItem.S1 = lastExecTime.ToString();
                    GameLogManager.CommonLog((int)SpecialLogType.PkDayRewardLog, 0, 0, logItem);
                    lastExecTime = lastExecTime.AddHours(period);
                }

                //生成下一次定时任务
                nextTime = lastExecTime;//GetNextTime();
                var taskId2 = Schedule.AddTask(typeof(StartOrChangePkDayRewardTask), nextTime, null);
                serverData.IntData[taskIndex] = taskId2;
                serverData.TodayHasIssuedPkReward += 1;
                //serverData.LongData[(int) ServerDataPkLongType.LastExecTimeNum] = Util.ToUnixTime(nextTime);
                Logger.InfoFormat("StartOrChangePkDayRewardTask End and Add Task,Time:{0},TaskId:{1}",
                    nextTime.ToString(), taskId2);
                Logger.InfoFormat("StartOrChangePkDayRewardTask End serverData:{0}", JsonConvert.SerializeObject(serverData));

                curTask = Schedule.GetTask(curTaskId);
                if (curTask != null)
                {
                    //删除当前已经执行的任务
                    Schedule.CancelTask(curTaskId);
                    Logger.InfoFormat("StartOrChangePkDayRewardTask CancelTask,TaskId:{0}", curTaskId);
                }
                else
                {
                    Logger.InfoFormat("StartOrChangePkDayRewardTask End TaskNotExist,TaskId:{0}", curTaskId);
                }
            }
        }

        /// <summary>
        /// 获取下一次任务的时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetNextTime()
        {
            //初始化任务/取消任务后初始化
            var period = ParamHelper.Get<int>("PkDayRewardPeriodTime", 24);
            var difHour = 24 - ParamHelper.Get<int>("PkDayRewardDueTime", 21);
            var nextTime = DateTime.Now.AddDays(1).Date.AddHours(-difHour);
            var difMillis = (int)(nextTime.Subtract(DateTime.Now).TotalMilliseconds);
            if (difMillis < 0) nextTime = nextTime.AddHours(period);

            return nextTime;
        }
    }
    #endregion
}
