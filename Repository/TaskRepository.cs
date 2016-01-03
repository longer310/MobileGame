using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;
using Newtonsoft.Json;

namespace MobileGame.tianzi.Repository
{
    #region 任务公用方法
    public class TaskCommon
    {
        /// <summary>
        /// 获取主线任务下发的项
        /// </summary>
        /// <param name="sysTaskCfg"></param>
        /// <param name="userTask"></param>
        /// <returns></returns>
        public static GetTaskListResponse.TaskItem GetMianLineTaskItem(SysTaskCfg sysTaskCfg, UserTask userTask)
        {
            var item = new GetTaskListResponse.TaskItem();
            item.Id = sysTaskCfg.Id;
            var index = (int)sysTaskCfg.MainLineType - 1;
            item.CurValue = userTask.MainLineValueList[index];
            item.CanGetReward = sysTaskCfg.GoalValue <= item.CurValue ? 1 : 0;
            if (sysTaskCfg.MainLineType == MainLineType.OccupiedCity)
            {
                item.CanGetReward = userTask.OccupiedCityIdList.Contains(sysTaskCfg.GoalValue) ? 1 : 0;
            }

            return item;
        }

        /// <summary>
        /// 获取每日任务下发的项
        /// </summary>
        /// <param name="sysTaskCfg"></param>
        /// <param name="userTask"></param>
        /// <param name="raList"></param>
        /// <returns></returns>
        public static GetTaskListResponse.TaskItem GetDailyTaskItem(SysTaskCfg sysTaskCfg, UserTask userTask, List<int> raList)
        {
            var item = new GetTaskListResponse.TaskItem();
            item.Id = sysTaskCfg.Id;
            if (sysTaskCfg.DailyType != DailyType.ReceiveSp)
            {
                var index = (int)sysTaskCfg.DailyType - 1;
                item.CurValue = userTask.DailyValueList[index];
                item.CanGetReward = sysTaskCfg.GoalValue <= item.CurValue ? 1 : 0;
            }
            else
            {
                var now = DateTime.Now;
                var startTime = DateTime.Today.AddHours(sysTaskCfg.GoalValue);
                var endTime = startTime.AddHours(2);
                if (endTime < now)
                {
                    //该任务最后领取的时间点已经过去了
                    var i = sysTaskCfg.Id;

                    //试着查找下一个任务！
                    sysTaskCfg = SysTaskCfg.Find(i + 1);
                    if (sysTaskCfg == null)
                    {
                        return null;
                    }
                    //得删除当前任务
                    raList.Add(i);
                    raList.Add(i + 1);

                    startTime = DateTime.Today.AddHours(sysTaskCfg.GoalValue);
                    endTime = startTime.AddHours(2);
                }

                if (startTime > now)
                {
                    var lastSysTaskCfg = SysTaskCfg.Find(sysTaskCfg.Id - 1);
                    if (lastSysTaskCfg != null)
                    {
                        var lastEndTime = DateTime.Today.AddHours(lastSysTaskCfg.GoalValue + 2);
                        //如果还未过上一个领取体力截止的时间的话，不需要显示下一个领取体力的任务
                        if (lastEndTime > now)
                            return null;
                    }

                    //时间未到 不需要展示出来
                    item.CanGetReward = -1;
                    item.CurValue = (int)startTime.Subtract(now).TotalSeconds;
                }
                else if (endTime > now)
                {
                    //时间刚好可以领取
                    item.CanGetReward = 1;
                }
                else
                {
                    return null;
                }
            }
            return item;
        }

        /// <summary>
        /// 获取任务奖励
        /// </summary>
        /// <param name="sysTaskCfg"></param>
        /// <param name="userRole"></param>
        /// <param name="opCode"></param>
        public static void GainTaskReward(SysTaskCfg sysTaskCfg, UserRole userRole, int opCode)
        {
            var multiple = 1;
            if (sysTaskCfg.TaskSort == TaskSort.Sign && sysTaskCfg.GoalValue > 0 && userRole.RealVipLevel >= sysTaskCfg.GoalValue)
                multiple = 2;

            //元宝
            if (sysTaskCfg.Money > 0)
                RoleManager.AddGiveMoney(userRole, sysTaskCfg.Money * multiple);
            //用户经验
            if (sysTaskCfg.UserExp > 0)
                Utility.AddUserExp(userRole, sysTaskCfg.UserExp * multiple, opCode);
            //铜钱
            if (sysTaskCfg.Coin > 0)
                Utility.AddResource(userRole, ItemType.Coin, opCode, sysTaskCfg.Coin * multiple);
            //木材
            if (sysTaskCfg.Wood > 0)
                Utility.AddResource(userRole, ItemType.Wood, opCode, sysTaskCfg.Wood * multiple);
            //石头
            if (sysTaskCfg.Stone > 0)
                Utility.AddResource(userRole, ItemType.Stone, opCode, sysTaskCfg.Stone * multiple);
            //铁矿
            if (sysTaskCfg.Iron > 0)
                Utility.AddResource(userRole, ItemType.Iron, opCode, sysTaskCfg.Iron * multiple);
            //体力
            if (sysTaskCfg.Sp > 0)
                Utility.AddResource(userRole, ItemType.Sp, opCode, sysTaskCfg.Sp * multiple);

            //添加道具1
            AddReward(userRole, sysTaskCfg.ToolId, sysTaskCfg.ToolNum * multiple, opCode);
            AddReward(userRole, sysTaskCfg.ToolId2, sysTaskCfg.ToolNum2 * multiple, opCode);
        }

        /// <summary>
        /// 获得id代表的类型
        /// </summary>
        /// <param name="id"></param>
        public static ExtractItemType GetExtractItemType(int id)
        {
            if (id > 10000000)
            {
                return ExtractItemType.Tool;
            }
            else if ((id / 10000) == 900)
            {
                return ExtractItemType.Hero;
            }
            else if ((id / 10000) == 500)
            {
                if (id < 5001000)
                    return ExtractItemType.Pet;
                else
                {
                    return ExtractItemType.Concubine;
                }
            }
            else if ((id / 10000) == 700)
            {
                return ExtractItemType.Equip;
            }
            return ExtractItemType.Tool;
        }

        /// <summary>
        /// 添加奖励 道具、装备、武将、妃子、骑宠
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="id"></param>
        /// <param name="num"></param>
        /// <param name="opCode"></param>
        public static void AddReward(UserRole userRole, int id, int num, int opCode)
        {
            //添加道具2
            if (num > 0)
            {
                var itemType = GetExtractItemType(id);
                if (itemType == ExtractItemType.Tool)
                {
                    var sysToolCfg = SysToolCfg.Items.Find(o => o.Id == id);
                    if (sysToolCfg != null)
                    {
                        //道具
                        sysToolCfg.AddToUser(userRole.Id, opCode, num);
                    }
                }
                else if (itemType == ExtractItemType.Hero)
                {
                    //武将
                    var sysHeroCfg = SysHeroCfg.Find(o => o.Id == id);
                    if (sysHeroCfg != null)
                    {
                        var userHero = DataStorage.Current.Load<UserHero>(userRole.Id, true);
                        var userHeroItem = userHero.AddHeroToUser(id, opCode);
                        if (userHeroItem == null)
                        {
                            //已有武将则添加碎片
                            var toolId = id + (int)ToolType.HeroChip;
                            var sysToolCfg = SysToolCfg.Find(o => o.Id == toolId);
                            if (sysToolCfg != null)
                            {
                                var chipNum = sysToolCfg.Param3;//ConfigHelper.ExtractCfgData.ExchangeHeroChips[(int)sysHeroCfg.Quality];
                                sysToolCfg.AddToUser(userRole.Id, opCode, chipNum);
                            }
                        }
                    }
                }
                else if (itemType == ExtractItemType.Equip)
                {
                    //装备
                    var sysEquipCfg = SysEquipCfg.Find(o => o.Id == id);
                    if (sysEquipCfg != null)
                    {
                        var userEquip = DataStorage.Current.Load<UserEquip>(userRole.Id, true);
                        userEquip.AddEquipToUser(id, opCode, false);
                    }
                }
                else if (itemType == ExtractItemType.Pet)
                {
                    //骑宠
                    var sysPetCfg = SysPetCfg.Find(o => o.Id == id);
                    if (sysPetCfg != null)
                    {
                        var userPet = DataStorage.Current.Load<UserPet>(userRole.Id, true);
                        userPet.AddPetToUser(id, opCode, false);
                    }
                }
                else if (itemType == ExtractItemType.Concubine)
                {
                    //妃子
                    var sysConcubineCfg = SysConcubineCfg.Find(o => o.Id == id);
                    if (sysConcubineCfg != null)
                    {
                        var userConcubine = DataStorage.Current.Load<UserConcubine>(userRole.Id, true);
                        userConcubine.AddConcubineToUser(id, opCode);
                    }
                }
            }
        }
    }
    #endregion

    #region 12000 获取任务详细列表
    public class GetTaskListResponse
    {
        public GetTaskListResponse()
        {
            Items = new List<TaskItem>();
        }
        /// <summary>
        /// 任务列表
        /// </summary>
        [Tag(1)]
        public List<TaskItem> Items { get; set; }

        /// <summary>
        /// 任务项
        /// </summary>
        public class TaskItem
        {
            /// <summary>
            /// 任务ID
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 任务当前目标值(还有多少秒才到领取时间)
            /// </summary>
            [Tag(2)]
            public int CurValue { get; set; }
            /// <summary>
            /// 是否可以领取当前任务的奖励[0：不可领取，1：可领取，-1:时间未到]
            /// </summary>
            [Tag(3)]
            public int CanGetReward { get; set; }
        }
    }
    /// <summary>
    /// 获取任务详细列表
    /// </summary>
    [GameCode(OpCode = 12000, ResponseType = typeof(GetTaskListResponse))]
    public class GetTaskListRequest : GameHandler
    {
        /// <summary>
        /// 任务分类
        /// </summary>
        public TaskSort TaskSort { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (TaskSort != TaskSort.MainLine && TaskSort != TaskSort.Daily) return false;
            return true;
        }
        public override void Process(GameContext context)
        {
            var userTask = Storage.Load<UserTask>(CurrentUserId, true);

            while (userTask.DailyValueList.Count < (int)DailyType.Max)
            {
                userTask.DailyValueList.Add(0);
            }

            var response = new GetTaskListResponse();

            if (TaskSort == TaskSort.MainLine)
            {
                //foreach (var i in userTask.MainLineIdList)
                for (int i = userTask.MainLineIdList.Count - 1; i >= 0; i--)
                {
                    var id = userTask.MainLineIdList[i];
                    var sysTaskCfg = SysTaskCfg.Find(id);
                    if (sysTaskCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", id);
                        return;
                    }
                    int index = sysTaskCfg.TaskType - 1;
                    if (userTask.MainLineValueList[index] == -1)
                    {
                        sysTaskCfg = SysTaskCfg.Find(id + 1);
                        if (sysTaskCfg != null)
                        {
                            userTask.MainLineIdList[i] = sysTaskCfg.Id;
                            userTask.MainLineValueList[index] = sysTaskCfg.GoalValue;
                            var item1 = TaskCommon.GetMianLineTaskItem(sysTaskCfg, userTask);
                            response.Items.Add(item1);
                        }
                        continue;
                    }
                    var item = TaskCommon.GetMianLineTaskItem(sysTaskCfg, userTask);

                    response.Items.Add(item);
                }
            }
            else
            {
                var raList = new List<int>();
                foreach (var i in userTask.DailyIdList)
                {
                    var sysTaskCfg = SysTaskCfg.Find(i);
                    if (sysTaskCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", i);
                        return;
                    }
                    int index = sysTaskCfg.TaskType - 1;
                    if (userTask.DailyValueList[index] == -1)
                    {
                        continue;
                    }
                    var item = TaskCommon.GetDailyTaskItem(sysTaskCfg, userTask, raList);

                    if (item != null)
                    {
                        response.Items.Add(item);
                    }
                }
                if (raList.Count > 0)
                {
                    userTask.DailyIdList.Remove(raList[0]);
                    userTask.DailyIdList.Add(raList[1]);
                }
            }

            if (TaskSort == TaskSort.MainLine)
            {
                userTask.ChangeMainLineNewMsg();
            }
            else
            {
                userTask.ChangeDailyNewMsg();
            }

            response.Items = response.Items.OrderByDescending(o => o.CanGetReward).ThenByDescending(o => o.Id).ToList();
            ResultObj = response;
        }
    }
    #endregion

    #region 12001 领取任务奖励
    /// <summary>
    /// 领取任务奖励[返回值ID>0说明有下一个任务，-1则直接把当前领取的任务从列表删除]
    /// </summary>
    [GameCode(OpCode = 12001, ResponseType = typeof(GetTaskListResponse.TaskItem))]
    public class GetTaskRewardRequest : GameHandler
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            var sysTaskCfg = SysTaskCfg.Find(Id);
            if (sysTaskCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", Id);
                return;
            }
            UserTask userTask;
            UserRole userRole;
            Storage.Load(out userTask, out userRole, CurrentUserId, true);
            var taskSort = sysTaskCfg.TaskSort;
            int index = sysTaskCfg.TaskType - 1;
            if (taskSort == TaskSort.MainLine)
            {
                if (!userTask.MainLineIdList.Contains(Id))
                {
                    SetError(ResourceId.R_12001_RewardGainedOrNotReach);
                    return;
                }

                int curValue = userTask.MainLineValueList[index];
                if (curValue == -1)
                {
                    //已领取过任务奖励
                    SetError(ResourceId.R_12001_RewardGaine);
                    return;
                }
                if (sysTaskCfg.MainLineType != MainLineType.OccupiedCity && curValue < sysTaskCfg.GoalValue)
                {
                    //未完成任务
                    SetError(ResourceId.R_12001_NotReach);
                    return;
                }
                if (sysTaskCfg.MainLineType == MainLineType.OccupiedCity && !userTask.OccupiedCityIdList.Contains(sysTaskCfg.GoalValue))
                {
                    //未完成任务
                    SetError(ResourceId.R_12001_NotReach);
                    return;
                }
            }
            else
            {
                if (!userTask.DailyIdList.Contains(Id))
                {
                    SetError(ResourceId.R_12001_RewardGainedOrNotReach);
                    return;
                }

                int curValue = userTask.DailyValueList[index];
                if (curValue == -1)
                {
                    //已领取过任务奖励
                    SetError(ResourceId.R_12001_RewardGaine);
                    return;
                }
                if (sysTaskCfg.DailyType != DailyType.ReceiveSp && curValue < sysTaskCfg.GoalValue)
                {
                    //未完成任务
                    SetError(ResourceId.R_12001_NotReach);
                    return;
                }
                if (sysTaskCfg.DailyType == DailyType.ReceiveSp)
                {
                    var now = DateTime.Now;
                    var startTime = DateTime.Today.AddHours(sysTaskCfg.GoalValue);
                    var endTime = startTime.AddHours(2);
                    if (now < startTime || now > endTime)
                    {
                        //领取时间未到
                        SetError(ResourceId.R_12001_TimeNotReach);
                        return;
                    }
                }
            }

            //领取奖励
            TaskCommon.GainTaskReward(sysTaskCfg, userRole, Request.OpCode);

            Id += 1;
            var response = new GetTaskListResponse.TaskItem();
            sysTaskCfg = SysTaskCfg.Find(Id);
            if (sysTaskCfg == null)
            {
                //没有下一个任务了
                response.Id = -1;
                if (taskSort == TaskSort.MainLine)
                {
                    userTask.MainLineValueList[index] = -1;
                }
                else
                {
                    userTask.DailyValueList[index] = -1;
                }
            }
            else
            {
                response.Id = Id;
                if (taskSort == TaskSort.MainLine)
                {
                    userTask.MainLineIdList.Remove(Id - 1);
                    userTask.MainLineIdList.Add(Id);
                    response = TaskCommon.GetMianLineTaskItem(sysTaskCfg, userTask);
                }
                else
                {
                    userTask.DailyIdList.Remove(Id - 1);
                    userTask.DailyIdList.Add(Id);

                    var raList = new List<int>();
                    response = TaskCommon.GetDailyTaskItem(sysTaskCfg, userTask, raList);
                    if (raList.Count > 0)
                    {
                        userTask.DailyIdList.Remove(raList[0]);
                        userTask.DailyIdList.Add(raList[1]);
                    }

                    if (response == null)
                    {
                        response = new GetTaskListResponse.TaskItem();
                        response.Id = -1;
                    }
                }
            }

            //保险起见，获取列表的时候 也刷新一次是否有新消息
            if (taskSort == TaskSort.MainLine)
            {
                userTask.ChangeMainLineNewMsg(userRole);
            }
            else
            {
                userTask.ChangeDailyNewMsg(userRole);
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 12002 获得登录奖励信息
    public class GetLogoinRewardInfoResponse
    {
        public GetLogoinRewardInfoResponse()
        {
            GainedIdList = new List<int>();
        }
        /// <summary>
        /// 登录的天数，与任务表中的GoalValue对比 判断是否可以领取
        /// </summary>
        [Tag(1)]
        public int LoginDays { get; set; }
        /// <summary>
        /// 已经领取登录奖励的Id【SysTaskCfg】列表
        /// </summary>
        [Tag(2)]
        public List<int> GainedIdList { get; set; }
    }
    /// <summary>
    /// 获得登录奖励信息
    /// </summary>
    [GameCode(OpCode = 12002, ResponseType = typeof(GetLogoinRewardInfoResponse))]
    public class GetLogoinRewardInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserTask userTask;
            Storage.Load(out userRole, out userTask, CurrentUserId);

            var response = new GetLogoinRewardInfoResponse();
            response.LoginDays = userRole.LoginDays;
            response.GainedIdList = userTask.LoginGainedIdList;

            ResultObj = response;
        }
    }
    #endregion

    #region 12003 领取登录奖励
    /// <summary>
    /// 领取登录奖励
    /// </summary>
    [GameCode(OpCode = 12003)]
    public class GainLoginRewardInfoRequest : GameHandler
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            var sysTaskCfg = SysTaskCfg.Find(Id);
            if (sysTaskCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", Id);
                return;
            }
            var taskSort = sysTaskCfg.TaskSort;
            if (taskSort != TaskSort.Login)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            UserTask userTask;
            UserRole userRole;
            Storage.Load(out userTask, out userRole, CurrentUserId, true);

            if (sysTaskCfg.GoalValue > userRole.LoginDays)
            {
                //未完成任务
                SetError(ResourceId.R_12001_NotReach);
                return;
            }

            if (userTask.LoginGainedIdList.Contains(Id))
            {
                SetError(ResourceId.R_12001_RewardGainedOrNotReach);
                return;
            }

            //领取奖励
            TaskCommon.GainTaskReward(sysTaskCfg, userRole, Request.OpCode);
            userTask.LoginGainedIdList.Add(Id);
            //更新是否有新消息
            userTask.ChangeLoginNewMsg(userRole, userRole.LoginDays);
        }
    }
    #endregion

    #region 12004 领取充值奖励
    /// <summary>
    /// 领取充值奖励
    /// </summary>
    [GameCode(OpCode = 12004)]
    public class GainRechargeRewardInfoRequest : GameHandler
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            var sysTaskCfg = SysTaskCfg.Find(Id);
            if (sysTaskCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", Id);
                return;
            }
            var taskSort = sysTaskCfg.TaskSort;
            if (taskSort != TaskSort.Recharge)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            UserTask userTask;
            UserRole userRole;
            Storage.Load(out userTask, out userRole, CurrentUserId, true);

            if (sysTaskCfg.GoalValue > userRole.TotalCharge)
            {
                //未完成任务
                SetError(ResourceId.R_12001_NotReach);
                return;
            }

            if (userTask.RechargeGainedIdList.Contains(Id))
            {
                SetError(ResourceId.R_12001_RewardGainedOrNotReach);
                return;
            }

            //领取奖励
            TaskCommon.GainTaskReward(sysTaskCfg, userRole, Request.OpCode);
            userTask.RechargeGainedIdList.Add(Id);
            //更新是否有新消息
            userTask.ChangeRechargeNewMsg(userRole);
        }
    }
    #endregion

    #region 12005 获得充值奖励信息
    public class GetRechargeRewardInfoResponse
    {
        public GetRechargeRewardInfoResponse()
        {
            GainedIdList = new List<int>();
        }
        /// <summary>
        /// 总充值
        /// </summary>
        [Tag(1)]
        public int TotalRecharge { get; set; }
        /// <summary>
        /// 已经领取充值奖励的Id【SysTaskCfg】列表
        /// </summary>
        [Tag(2)]
        public List<int> GainedIdList { get; set; }
    }
    /// <summary>
    /// 获得充值奖励信息
    /// </summary>
    [GameCode(OpCode = 12005, ResponseType = typeof(GetRechargeRewardInfoResponse))]
    public class GetRechargeRewardInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserTask userTask;
            Storage.Load(out userRole, out userTask, CurrentUserId);

            var response = new GetRechargeRewardInfoResponse();
            response.TotalRecharge = userRole.TotalCharge;
            response.GainedIdList = userTask.RechargeGainedIdList;

            ResultObj = response;
        }
    }
    #endregion

    #region 12006 领取剧情任务奖励
    public class GetStoryRewardResponse
    {
        /// <summary>
        /// 下一个剧情任务ID
        /// </summary>
        [Tag(1)]
        public int StoryTaskId { get; set; }
    }
    /// <summary>
    /// 领取剧情任务奖励
    /// </summary>
    [GameCode(OpCode = 12006, ResponseType = typeof(GetStoryRewardResponse))]
    public class GetStoryRewardRequest : GameHandler
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 触发剧情任务的城池ID
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            var sysTaskCfg = SysTaskCfg.Find(Id);
            if (sysTaskCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", Id);
                return;
            }
            var taskSort = sysTaskCfg.TaskSort;
            if (taskSort != TaskSort.Story)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);

            var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserCity:CityItems:CityId", CityId);
                return;
            }

            if (Id != userCity.StoryTaskId)
            {
                //任务奖励已领取
                SetError(ResourceId.R_12001_RewardGaine);
                return;
            }
            //var visitor = userCityItem.VisitorItems.FirstOrDefault(o => o.VisitorId == sysTaskCfg.GoalValue);
            //if (visitor == null)
            //{
            //    //未完成任务
            //    SetError(ResourceId.R_12001_NotReach);
            //    return;
            //}

            //领取奖励
            TaskCommon.GainTaskReward(sysTaskCfg, userRole, Request.OpCode);

            sysTaskCfg = SysTaskCfg.Find(Id + 1);
            if (sysTaskCfg == null)
            {
                userCity.StoryTaskId = 10000000 + Id;
            }
            else
            {
                userCity.StoryTaskId = Id + 1;
            }

            var response = new GetStoryRewardResponse() { StoryTaskId = userCity.StoryTaskId > 10000000 ? -1 : userCity.StoryTaskId };
            ResultObj = response;
        }
    }
    #endregion

    #region 12007 获取月签到界面信息
    /// <summary>
    /// 获取月签到界面信息
    /// </summary>
    public class GetSignResponse
    {
        /// <summary>
        /// 当前签到月份
        /// </summary>
        [Tag(1)]
        public int Month { get; set; }

        /// <summary>
        /// 签到次数
        /// </summary>
        [Tag(2)]
        public int SignNum { get; set; }

        /// <summary>
        /// 当前使用的奖励版本
        /// </summary>
        [Tag(3)]
        public int Edition { get; set; }

        /// <summary>
        /// 今日是否签到(1表示已签到，0表示未签到)
        /// </summary>
        [Tag(4)]
        public int TodayHasSign { get; set; }
    }
    /// <summary>
    /// 获取月签到界面信息
    /// </summary>
    [GameCode(OpCode = 12007, ResponseType = typeof(GetSignResponse))]
    public class GetSignRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetSignResponse();

            UserTask userTask = Storage.Load<UserTask>(CurrentUserId, true);

            response.Month = DateTime.Now.Month;
            response.SignNum = userTask.SignNum;
            response.Edition = userTask.GetSignEdition();
            response.TodayHasSign = DateTime.Now.Date == userTask.LastSignTime.Date ? 1 : 0;

            ResultObj = response;
        }
    }
    #endregion

    #region 12008 签到并领取奖励
    /// <summary>
    /// 签到并领取奖励
    /// </summary>
    [GameCode(OpCode = 12008)]
    public class SignAndGainRewardRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserTask userTask;
            UserRole userRole;
            Storage.Load(out userTask, out userRole, CurrentUserId, true);

            if (userTask.LastSignTime.Date == DateTime.Now.Date)
            {
                //今日已签到
                SetError(ResourceId.R_12008_TodaySigned);
                return;
            }

            var id = ((int)TaskSort.Sign) * 100000 + userTask.GetSignEdition() * 1000 + userTask.SignNum + 1;
            var sysTaskCfg = SysTaskCfg.Find(id);
            if (sysTaskCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", id);
                return;
            }

            //领取奖励
            TaskCommon.GainTaskReward(sysTaskCfg, userRole, Request.OpCode);

            userTask.LastSignTime = DateTime.Now;
            userTask.SignNum++;
        }
    }
    #endregion

    #region 12009 获得今日充值奖励信息
    public class GetTodayRechargeRewardInfoResponse
    {
        public GetTodayRechargeRewardInfoResponse()
        {
            GainedIdList = new List<int>();
        }
        /// <summary>
        /// 今日总充值
        /// </summary>
        [Tag(1)]
        public int TotalRecharge { get; set; }
        /// <summary>
        /// 已经领取今日充值奖励的Id【SysTaskCfg】列表
        /// </summary>
        [Tag(2)]
        public List<int> GainedIdList { get; set; }
    }
    /// <summary>
    /// 获得今日充值奖励信息
    /// </summary>
    [GameCode(OpCode = 12009, ResponseType = typeof(GetTodayRechargeRewardInfoResponse))]
    public class GetTodayRechargeRewardInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var userTask = Storage.Load<UserTask>(CurrentUserId, true);

            userTask.TodayRechargeGainedIdList.RemoveAll(o => (int)o.Value == 0);

            var response = new GetTodayRechargeRewardInfoResponse();
            response.TotalRecharge = (int)userTask.TodayRecharge.Value;
            foreach (var dayZeorValue in userTask.TodayRechargeGainedIdList)
            {
                var i = dayZeorValue.Value;
            }
            response.GainedIdList = userTask.TodayRechargeGainedIdList.Select(o => (int)o.Value).ToList();

            ResultObj = response;
        }
    }
    #endregion

    #region 12010 领取今日充值奖励
    /// <summary>
    /// 领取今日充值奖励
    /// </summary>
    [GameCode(OpCode = 12010)]
    public class GainTodayRechargeRewardInfoRequest : GameHandler
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            var sysTaskCfg = SysTaskCfg.Find(Id);
            if (sysTaskCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysTaskCfg:Id", Id);
                return;
            }
            var taskSort = sysTaskCfg.TaskSort;
            if (taskSort != TaskSort.TodayRecharge)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            UserTask userTask;
            UserRole userRole;
            Storage.Load(out userTask, out userRole, CurrentUserId, true);

            if (sysTaskCfg.GoalValue > (int)userTask.TodayRecharge.Value)
            {
                //未完成任务
                SetError(ResourceId.R_12001_NotReach);
                return;
            }

            if (userTask.TodayRechargeGainedIdList.Exists(o=>(int)o.Value == Id))
            {
                SetError(ResourceId.R_12001_RewardGainedOrNotReach);
                return;
            }

            //领取奖励
            TaskCommon.GainTaskReward(sysTaskCfg, userRole, Request.OpCode);
            userTask.TodayRechargeGainedIdList.Add(new DayZeorValue(Id));
            //更新是否有新消息
            userTask.ChangeTodayRechargeNewMsg(userRole);
        }
    }
    #endregion
}
