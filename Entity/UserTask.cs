// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。
// 文 件 名：UserChip.cs
// 创建标识：2012/11/1 23:39:34 Created by 胡奇龙
// 功能说明：
// 注意事项：
// 
// 更新记录：
// -------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Core;
using MobileGame.tianzi;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Repository;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 用户任务表
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserTask : KVEntity
    {
        /// <summary>
        /// 当前主线任务ID列表,值随意排列
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> MainLineIdList { get; set; }

        /// <summary>
        /// 当前每日任务ID列表
        /// </summary>
        [ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> DailyIdList { get; set; }

        /// <summary>
        /// 当前每日任务ID列表
        /// </summary>
        [ProtoMember(3)]
        public int CurNum { get; set; }

        /// <summary>
        /// 主线任务是否有领取的任务
        /// </summary>
        //[ProtoMember(4)]
        //public int IsNew { get; set; }

        /// <summary>
        /// 每天重置为零——判断每日任务ID列表是否需要重新计算
        /// </summary>
        [ProtoMember(5), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue DailyTaskNum { get; set; }

        /// <summary>
        /// 每日任务完成度列表
        /// </summary>
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> DailyValueList { get; set; }

        /// <summary>
        /// 主线任务完成度列表,值按照类型排列
        /// </summary>
        [ProtoMember(7), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> MainLineValueList { get; set; }

        /// <summary>
        /// 占领城池ID列表
        /// </summary>
        [ProtoMember(8), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> OccupiedCityIdList { get; set; }

        /// <summary>
        /// 已经领取的登录奖励ID列表
        /// </summary>
        [ProtoMember(9), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> LoginGainedIdList { get; set; }

        /// <summary>
        /// 已经领取的充值奖励ID列表
        /// </summary>
        [ProtoMember(10), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> RechargeGainedIdList { get; set; }

        /// <summary>
        /// 上次签到的时间
        /// </summary>
        [ProtoMember(11)]
        public DateTime LastSignTime { get; set; }

        /// <summary>
        /// 本月签到次数
        /// </summary>
        [ProtoMember(13)]
        public int SignNum { get; set; }

        /// <summary>
        /// 当天充值数
        /// </summary>
        [ProtoMember(14), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue TodayRecharge { get; set; }

        /// <summary>
        /// 当天充值数
        /// </summary>
        [ProtoMember(15), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<DayZeorValue> TodayRechargeGainedIdList { get; set; }

        public override void NewObjectInit()
        {
            MainLineIdList = new List<int>();
            DailyIdList = new List<int>();
            DailyValueList = new List<int>();
            OccupiedCityIdList = new List<int>();
            MainLineValueList = new List<int>();
            LoginGainedIdList = new List<int>();
            RechargeGainedIdList = new List<int>();
            TodayRechargeGainedIdList = new List<DayZeorValue>();
            var reLoadTaskNum = ConfigHelper.ReLoadTaskNum;
            CurNum = reLoadTaskNum;
            ReLoadMainLineTaskData();
            ReLoadDailyTaskData();

            InitTaskValue();
            InitSignList();
        }

        /// <summary>
        /// 初始化签到信息
        /// </summary>
        public void InitSignList()
        {
            if (LastSignTime < DateTime.Now.AddYears(-10))
                LastSignTime = DateTime.Now.AddYears(-5);

            if (LastSignTime.AddYears(4) < DateTime.Now)
            {
                //第一次初始化
                var serverData = DataStorage.Current.Load<ServerData>((int)ServerDataIdType.SignEdition, true);
                if (serverData.IntData[0] == 0)
                {
                    serverData.IntData[0] = 1;
                    serverData.IntData[1] = (int)DateTime.Now.ToUnixTime();
                }
            }
            else
            {
                if (LastSignTime.Month != DateTime.Now.Month)
                {
                    SignNum = 0;
                    //到下个月了且上个月有签到过 -> 重置
                    var serverData = DataStorage.Current.Load<ServerData>((int)ServerDataIdType.SignEdition, true);
                    if (serverData.IntData[0] == 0)
                    {
                        //第一次初始化
                        serverData.IntData[0] = 1;
                        serverData.IntData[1] = (int)DateTime.Now.ToUnixTime();
                    }
                    else
                    {
                        long lasttime = (long)serverData.IntData[1];
                        DateTime lastt = lasttime.FromUnixTime();
                        if (lastt.Month != DateTime.Now.Month)
                        {
                            //下个月第一次改动
                            serverData.IntData[0] = serverData.IntData[0] == 1 ? 2 : 1;
                            serverData.IntData[1] = (int)DateTime.Now.ToUnixTime();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取签到奖励版本信息
        /// </summary>
        /// <returns></returns>
        public int GetSignEdition()
        {
            var serverData = DataStorage.Current.Load<ServerData>((int)ServerDataIdType.SignEdition, true);
            if (serverData.IntData[0] == 0)
            {
                serverData.IntData[0] = 1;
                serverData.IntData[1] = (int)DateTime.Now.ToUnixTime();
            }

            return serverData.IntData[0];
        }

        public override void LoadInit()
        {
            MainLineIdList = MainLineIdList ?? new List<int>();
            DailyIdList = DailyIdList ?? new List<int>();
            DailyValueList = DailyValueList ?? new List<int>();
            MainLineValueList = MainLineValueList ?? new List<int>();
            OccupiedCityIdList = OccupiedCityIdList ?? new List<int>();
            LoginGainedIdList = LoginGainedIdList ?? new List<int>();
            RechargeGainedIdList = RechargeGainedIdList ?? new List<int>();
            TodayRechargeGainedIdList = TodayRechargeGainedIdList ?? new List<DayZeorValue>();

            var reLoadTaskNum = ConfigHelper.ReLoadTaskNum;
            if (CurNum < reLoadTaskNum)
            {
                ReLoadMainLineTaskData();
                ReLoadDailyTaskData();
                CurNum = reLoadTaskNum;

                InitTaskValue();
            }
            if (DailyTaskNum.Value <= 0)
            {
                ReLoadDailyTaskData();
            }
            CheckMainTask();
            InitSignList();

            //检测有错误的主线任务id列表
            CheckMainLineError();
        }

        /// <summary>
        /// 检查并修复主线任务数据
        /// </summary>
        public void CheckMainLineError()
        {
            var mainLineCount =
                SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.MainLine).Distinct(o => o.TaskType).Count();
            if (MainLineIdList.Count > mainLineCount)
            {
                //有问题，出现多个了。
                var logItem = new GameLogItem();

                logItem.F1 = MainLineIdList.Count;
                logItem.S1 = JsonConvert.SerializeObject(MainLineIdList);

                var tempMianLineIdList = new List<int>();
                foreach (var i in MainLineIdList)
                {
                    tempMianLineIdList.Add(i);
                }
                //var needRemoveIdList = new List<int>();
                //foreach (var i in MainLineIdList)

                logItem.S2 = "";
                var totalMoney = 0;
                var totalSp = 0;
                for (var i = MainLineIdList.Count - 1; i >= 0; i--)
                {
                    var id = MainLineIdList[i];

                    logItem.S2 += id.ToString() + ",";

                    tempMianLineIdList.Remove(id);
                    var taskType = (id - (int)(TaskSort.MainLine) * 100000) / 1000;
                    var sameTypeId =
                        tempMianLineIdList.FirstOrDefault(o => (o - (int)(TaskSort.MainLine) * 100000) / 1000 == taskType);

                    logItem.S2 += sameTypeId.ToString() + ",";

                    if (sameTypeId != null && sameTypeId != 0)
                    {
                        //获取小的为错误任务id
                        var errorId = id > sameTypeId ? sameTypeId : id;

                        logItem.S2 += errorId.ToString() + ",";

                        //needRemoveIdList.Add(errorId);
                        var errorSysTaskCfgList =
                            SysTaskCfg.Items.Where(
                                o => o.TaskSort == TaskSort.MainLine && o.TaskType == taskType && o.Id < errorId)
                                .ToList();

                        totalMoney += errorSysTaskCfgList.Sum(o => o.Money);
                        totalSp += errorSysTaskCfgList.Sum(o => o.Sp);

                        logItem.S2 += totalMoney.ToString() + ",";
                        logItem.S2 += totalSp.ToString() + ";";

                        MainLineIdList.Remove(errorId);
                    }
                }
                logItem.S3 = JsonConvert.SerializeObject(MainLineIdList);

                //扣除多领取的体力和元宝
                var userRole = DataStorage.Current.Load<UserRole>(Id, true);
                int subMoney, subGiveMoney;
                totalMoney = userRole.TotalMoney > totalMoney ? totalMoney : userRole.TotalMoney;
                totalSp = userRole.Sp > totalSp ? totalSp : userRole.Sp;

                logItem.F2 = totalMoney;
                logItem.F3 = totalSp;
                if (totalMoney > 0)
                {
                    RoleManager.Consume(userRole, (int)SpecialLogType.MainLineErrorLog, totalMoney, 1,
                        out subMoney, out subGiveMoney);
                }
                if (totalSp > 0)
                {
                    userRole.ConsumeResource(ItemType.Sp, (int)SpecialLogType.MainLineErrorLog, totalSp);
                }

                //记录日志
                GameLogManager.CommonLog((int)SpecialLogType.MainLineErrorLog, Id, 0, logItem);
            }
        }

        /// <summary>
        /// 改变登录奖励新消息
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="loginDays"></param>
        public void ChangeLoginNewMsg(UserRole userRole = null, int loginDays = 0)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var sysTaskCfgList =
                SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.Login && o.GoalValue <= loginDays)
                    .ToList();
            foreach (var sysTaskCfg in sysTaskCfgList)
            {
                if (!LoginGainedIdList.Contains(sysTaskCfg.Id))
                {
                    userRole.SetHasNewMsg((int)NewMsgType.Login, 1);
                    return;
                }
            }
            userRole.SetHasNewMsg((int)NewMsgType.Login, 0);
        }

        /// <summary>
        /// 改变充值奖励新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeRechargeNewMsg(UserRole userRole = null)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var sysTaskCfgList =
                SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.Recharge && o.GoalValue <= userRole.TotalCharge)
                    .ToList();
            foreach (var sysTaskCfg in sysTaskCfgList)
            {
                if (!RechargeGainedIdList.Contains(sysTaskCfg.Id))
                {
                    userRole.SetHasNewMsg((int)NewMsgType.Recharge, 1);
                    return;
                }
            }
            userRole.SetHasNewMsg((int)NewMsgType.Recharge, 0);
        }

        /// <summary>
        /// 改变今日充值奖励新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeTodayRechargeNewMsg(UserRole userRole = null)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var sysTaskCfgList =
                SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.TodayRecharge && o.GoalValue <= (int)TodayRecharge.Value)
                    .ToList();
            foreach (var sysTaskCfg in sysTaskCfgList)
            {
                if (!TodayRechargeGainedIdList.Exists(o => (int)o.Value == sysTaskCfg.Id))
                {
                    userRole.SetHasNewMsg((int)NewMsgType.TodayRecharge, 1);
                    return;
                }
            }
            userRole.SetHasNewMsg((int)NewMsgType.TodayRecharge, 0);
        }

        /// <summary>
        /// 检查主线任务 完成了最后一个任务后 是否有接下来的任务
        /// </summary>
        public void CheckMainTask()
        {
            for (var i = MainLineValueList.Count - 1; i >= 0; i--)
            {
                if (i == -1)
                {
                    var id = MainLineIdList[i] + 1;
                    var sysTaskCfg = SysTaskCfg.Items.FirstOrDefault(o => o.Id == id);
                    if (sysTaskCfg != null)
                    {
                        MainLineIdList[i] = id;

                        if (sysTaskCfg.TaskType == (int)MainLineType.PassLevels)
                        {
                            var userLevel = DataStorage.Current.Load<UserLevels>(Id);
                            MainLineValueList[i] = userLevel.OpenedMaxLevelId - 1;
                        }
                        else if (sysTaskCfg.TaskType == (int)MainLineType.UserLevel)
                        {
                            var userRole = DataStorage.Current.Load<UserRole>(Id);
                            MainLineValueList[i] = userRole.Level;
                        }
                        else if (sysTaskCfg.TaskType == (int)MainLineType.RecruitHeroNum)
                        {
                            var userHero = DataStorage.Current.Load<UserHero>(Id);
                            MainLineValueList[i] = userHero.Items.Count;
                        }
                        else if (sysTaskCfg.TaskType == (int)MainLineType.RecruitConcubineNum)
                        {
                            var userConcubine = DataStorage.Current.Load<UserConcubine>(Id);
                            MainLineValueList[i] = userConcubine.Items.Count;
                        }
                        else if (sysTaskCfg.TaskType == (int)MainLineType.ReputeReach)
                        {
                            var userRole = DataStorage.Current.Load<UserRole>(Id);
                            MainLineValueList[i] = userRole.Repute;
                        }
                        else if (sysTaskCfg.TaskType == (int)MainLineType.CharmReach)
                        {
                            var userRole = DataStorage.Current.Load<UserRole>(Id);
                            MainLineValueList[i] = userRole.Charm;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 改变每日任务新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeDailyNewMsg(UserRole userRole = null)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var openCfgItem = ConfigHelper.OpenCfgsData.FirstOrDefault(o => o.Type == OpenModuleType.DailyTask);
            if (openCfgItem == null || userRole.Level >= openCfgItem.Level)
                userRole.SetHasNewMsg((int)NewMsgType.DailyTask, HasDailyNewMsg() > 0 ? 1 : 0);
        }

        /// <summary>
        /// 改变主线任务新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeMainLineNewMsg(UserRole userRole = null)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            userRole.SetHasNewMsg((int)NewMsgType.MainLineTask, HasMainLineNewMsg() > 0 ? 1 : 0);
        }

        /// <summary>
        /// 定时任务是否可以领取奖励了
        /// </summary>
        /// <returns></returns>
        public int HasNewTimeMsg()
        {
            var id = GetTaskId(TaskSort.Daily, (int)DailyType.ReceiveSp);
            var sysTaskCfg = SysTaskCfg.Find(id);
            if (sysTaskCfg != null)
            {
                var raList = new List<int>();
                var item = TaskCommon.GetDailyTaskItem(sysTaskCfg, this, raList);
                if (raList.Count == 2)
                {
                    DailyIdList.Remove(raList[0]);
                    DailyIdList.Add(raList[1]);
                }
                var index = sysTaskCfg.TaskType - 1;
                var value = DailyValueList[index];
                if (item != null && item.CanGetReward == 1 && value != -1) return 1;
            }
            return 0;
        }

        /// <summary>
        /// 每日任务是否可以领取奖励了
        /// </summary>
        /// <returns></returns>
        public int HasDailyNewMsg()
        {
            var hasNewMsg = HasNewTimeMsg();
            if (hasNewMsg > 0) return 1;

            var index = 1;
            while (index <= ((int)DailyType.Max - 1))
            {
                var id = GetTaskId(TaskSort.Daily, index);
                var sysTaskCfg = SysTaskCfg.Find(id);
                if (sysTaskCfg != null)
                {
                    var raList = new List<int>();
                    var item = TaskCommon.GetDailyTaskItem(sysTaskCfg, this, raList);
                    if (raList.Count == 2)
                    {
                        DailyIdList.Remove(raList[0]);
                        DailyIdList.Add(raList[1]);
                    }
                    var index2 = sysTaskCfg.TaskType - 1;
                    var value = DailyValueList[index2];
                    if (item != null && item.CanGetReward == 1 && value != -1) return 1;
                }
                index++;
            }
            return 0;
        }

        /// <summary>
        /// 主线任务是否可以领取奖励了
        /// </summary>
        /// <returns></returns>
        public int HasMainLineNewMsg()
        {
            var index = 1;
            while (index <= (int)MainLineType.Max)
            {
                var id = GetTaskId(TaskSort.MainLine, index);
                var sysTaskCfg = SysTaskCfg.Find(id);
                if (sysTaskCfg != null)
                {
                    var item = TaskCommon.GetMianLineTaskItem(sysTaskCfg, this);
                    var index2 = sysTaskCfg.TaskType - 1;
                    var value = MainLineValueList[index2];
                    if (item != null && item.CanGetReward == 1 && value != -1) return 1;
                }
                index++;
            }
            return 0;
        }

        /// <summary>
        /// 获取当前进行的任务ID
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetTaskId(TaskSort sort, int type)
        {
            if (sort == TaskSort.MainLine)
            {
                foreach (var i in MainLineIdList)
                {
                    if ((int)((i - 100000) / 1000) == type) return i;
                }
            }
            else
            {
                foreach (var i in DailyIdList)
                {
                    if ((int)((i - 200000) / 1000) == type) return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 初始化任务达成Value
        /// </summary>
        public void InitTaskValue()
        {
            DailyValueList = new List<int>();
            while (DailyValueList.Count < (int)DailyType.Max)
            {
                DailyValueList.Add(0);
            }
            MainLineValueList = new List<int>();
            OccupiedCityIdList = new List<int>();
            while (MainLineValueList.Count < (int)MainLineType.Max)
            {
                var mainLineType = (MainLineType)(MainLineValueList.Count + 1);

                var userRole = DataStorage.Current.Load<UserRole>(Id);
                switch (mainLineType)
                {
                    case MainLineType.PassLevels:
                        {
                            var userLevels = DataStorage.Current.Load<UserLevels>(Id);
                            MainLineValueList.Add(userLevels.OpenedMaxLevelId);
                            break;
                        }
                    case MainLineType.UserLevel:
                        {
                            MainLineValueList.Add(userRole.Level);
                            break;
                        }
                    case MainLineType.OccupiedCity:
                        {
                            var userCity = DataStorage.Current.Load<UserCity>(Id);
                            foreach (var cityItem in userCity.CityItems.Where(o => o.OwnerType == OwnerType.Own).ToList())
                            {
                                if (!OccupiedCityIdList.Contains(cityItem.CityId))
                                    OccupiedCityIdList.Add(cityItem.CityId);
                            }
                            MainLineValueList.Add(0);
                            break;
                        }
                    case MainLineType.RecruitHeroNum:
                        {
                            var userHero = DataStorage.Current.Load<UserHero>(Id);
                            MainLineValueList.Add(userHero.Items.Count);
                            break;
                        }
                    case MainLineType.RecruitConcubineNum:
                        {
                            var userConcubine = DataStorage.Current.Load<UserConcubine>(Id);
                            MainLineValueList.Add(userConcubine.Items.Count);
                            break;
                        }
                    case MainLineType.ReputeReach:
                        {
                            MainLineValueList.Add(userRole.MaxRepute);
                            break;
                        }
                    case MainLineType.CharmReach:
                        {
                            MainLineValueList.Add(userRole.Charm);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 重新加载主线任务
        /// </summary>
        public void ReLoadMainLineTaskData()
        {
            var mainLineTaskCfgList =
                SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.MainLine)
                    .OrderBy(o => o.Id)
                    .Distinct(p => p.TaskType)
                    .ToList();

            var isFirst = MainLineIdList.Count == 0;
            foreach (var sysTaskCfg in mainLineTaskCfgList)
            {
                if (isFirst)
                {
                    MainLineIdList.Add(sysTaskCfg.Id);
                }
                else
                {
                    var otherTaskList =
                        SysTaskCfg.Items.Where(o => o.TaskSort == sysTaskCfg.TaskSort && o.TaskType == sysTaskCfg.TaskType)
                            .ToList();
                    //foreach (var taskCfg in otherTaskList)
                    //{
                    //    if (MainLineIdList.Contains(taskCfg.Id))
                    //    {
                    //        break;
                    //    }
                    //}
                    //MainLineIdList.Remove(sysTaskCfg.Id);
                    //MainLineIdList.Add(sysTaskCfg.Id);
                    var hasTask = false;
                    foreach (var taskCfg in otherTaskList)
                    {
                        if (MainLineIdList.Contains(taskCfg.Id))
                        {
                            hasTask = true; break;
                        }
                    }
                    if (!hasTask)
                    {
                        MainLineIdList.Remove(sysTaskCfg.Id);
                        MainLineIdList.Add(sysTaskCfg.Id);
                    }
                }
            }
        }

        /// <summary>
        /// 重新加载每日任务
        /// </summary>
        public void ReLoadDailyTaskData()
        {
            DailyIdList = new List<int>();
            DailyValueList = new List<int>();
            DailyTaskNum += 1;
            while (DailyValueList.Count < (int)DailyType.Max)
            {
                DailyValueList.Add(0);
            }

            ChangeDailyNewMsg();

            //每日任务
            var dailyTaskCfgList =
                SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.Daily)
                    .OrderBy(o => o.Id)
                    .Distinct(p => p.TaskType)
                    .ToList();

            var isFirst = DailyIdList.Count == 0;
            foreach (var sysTaskCfg in dailyTaskCfgList)
            {
                if (isFirst)
                {
                    DailyIdList.Add(sysTaskCfg.Id);
                }
                else
                {
                    var otherTaskList =
                        SysTaskCfg.Items.Where(o => o.TaskSort == sysTaskCfg.TaskSort && o.TaskType == sysTaskCfg.TaskType)
                            .ToList();
                    foreach (var taskCfg in otherTaskList)
                    {
                        if (DailyIdList.Contains(taskCfg.Id)) break;
                    }
                    DailyIdList.Remove(sysTaskCfg.Id);
                    DailyIdList.Add(sysTaskCfg.Id);
                }
            }
        }
    }

    public class CommonEqualityComparer<T, V> : IEqualityComparer<T>
    {
        private Func<T, V> keySelector;

        public CommonEqualityComparer(Func<T, V> keySelector)
        {
            this.keySelector = keySelector;
        }

        public bool Equals(T x, T y)
        {
            return EqualityComparer<V>.Default.Equals(keySelector(x), keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return EqualityComparer<V>.Default.GetHashCode(keySelector(obj));
        }
    }

    public static class DistinctExtensions
    {
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector)
        {
            return source.Distinct(new CommonEqualityComparer<T, V>(keySelector));
        }
    }
}
