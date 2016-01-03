// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。
// 文 件 名：UserEquip.cs
// 创建标识：2012/11/1 23:39:34 Created by 胡奇龙
// 功能说明：
// 注意事项：
// 
// 更新记录：
// -------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 城池项信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserCityItem : EntityItem
    {
        /// <summary>
        /// 系统城池id【对应SysCityCfg表】
        /// </summary>
        [ProtoMember(1)]
        public int CityId { get; set; }

        /// <summary>
        /// 全服的大地图城池id
        /// </summary>
        [ProtoMember(2)]
        public int ServerMapCityItemId { get; set; }

        /// <summary>
        /// 匹配的到全服城池的时间/匹配后被占的时间【用于是否可以重新匹配对手】
        /// </summary>
        [ProtoMember(3)]
        public DateTime MatchOrBeOccupiedTime { get; set; }

        /// <summary>
        /// 占领时间或者最后一次收获的时间【用于收获时计算具体的收获量】
        /// </summary>
        //[ProtoMember(4)]
        //public DateTime OccupiedOrGainTime { get; set; }

        /// <summary>
        /// 大于零说明 世界城池有变化
        /// </summary>
        [ProtoMember(5)]
        public int Status { get; set; }

        /// <summary>
        /// 占领类型
        /// </summary>
        [ProtoMember(6)]
        public OwnerType OwnerType { get; set; }

        /// <summary>
        /// 寻访的武将ID
        /// </summary>
        //[ProtoMember(7)]
        //public int SearchHeroId { get; set; }

        /// <summary>
        /// 寻访的截止时间
        /// </summary>
        //[ProtoMember(8)]
        //public DateTime SearchEndTime { get; set; }

        /// <summary>
        /// 到访的列表
        /// </summary>
        [ProtoMember(9), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<VisitorItem> VisitorItems { get; set; }

        /// <summary>
        /// 寻访的总次数
        /// </summary>
        [ProtoMember(10)]
        public int SearchNum { get; set; }

        /// <summary>
        /// 重置寻访次数
        /// </summary>
        [ProtoMember(11), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue BuySearchNum { get; set; }

        /// <summary>
        /// 今日已使用寻访次数
        /// </summary>
        [ProtoMember(12), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue UseSearchNum { get; set; }

        /// <summary>
        /// 剩余寻访次数
        /// </summary>
        public int LaveSearchNum
        {
            get
            {
                return (int)(3 - UseSearchNum.Value);
            }
        }

        /// <summary>
        /// 重置内政次数
        /// </summary>
        [ProtoMember(13), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue BuyInternalAffairsNum { get; set; }

        /// <summary>
        /// 已经进行的内政次数
        /// </summary>
        [ProtoMember(14), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue UseInternalAffairsNum { get; set; }

        /// <summary>
        /// 剩余内政次数
        /// </summary>
        public int LaveInternalAffairsNum
        {
            get
            {
                return (int)(3 - UseInternalAffairsNum.Value);
            }
        }

        /// <summary>
        /// 产量
        /// </summary>
        public int Product
        {
            get
            {
                //暂时这么直接取配置 TODO:后续可能和太守有联系
                return SysCityCfg.Product;
            }
        }

        /// <summary>
        /// 城池配置信息
        /// </summary>
        public SysCityCfg SysCityCfg
        {
            get
            {
                var sysCityCfg = SysCityCfg.Find(CityId);
                if (sysCityCfg == null)
                    throw new ApplicationException(string.Format("SysCityCfg:Id:{0} NOT FIND", CityId));
                return sysCityCfg;
            }
        }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity
        {
            get
            {
                //暂时这么直接取配置 TODO:后续可能和太守有联系
                return SysCityCfg.Capacity;
            }
        }

        /// <summary>
        /// 资源类型
        /// </summary>
        public MoneyType MoneyType
        {
            get
            {
                return SysCityCfg.MoneyType;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void NewObjectInit()
        {
            VisitorItems = new List<VisitorItem>();
        }

        //public SearchItem Search(int isReq = 0)
        //{
        //    SearchVisitor(SearchHeroId);

        //    var userCity = DataStorage.Current.Load<UserCity>(Pid, true);
        //    var item = userCity.AddSearch(VisitorItems, CityId, SearchHeroId, SearchEndTime, isReq);

        //    SearchHeroId = 0;
        //    SearchEndTime = DateTime.Now;

        //    return item;
        //}

        public override void LoadInit()
        {
            VisitorItems = VisitorItems ?? new List<VisitorItem>();

            //if (SearchHeroId > 0 && SearchEndTime.ToTs() <= 0)
            //{
            //    Search();
            //}

            //删除离开的武将
            for (var j = VisitorItems.Count - 1; j >= 0; j--)
            {
                var item = VisitorItems[j];
                if (item.LeaveTimes.ToTs() <= 0)
                {
                    VisitorItems.Remove(item);
                }
            }
        }

        /// <summary>
        /// 寻访到访武将/妃子
        /// </summary>
        /// <param name="searchHeroId"></param>
        public void SearchVisitor(int searchHeroId)
        {
            VisitorItems = new List<VisitorItem>();

            var intel = 0;
            var sysHeroCfg = SysHeroCfg.Items.Find(o => o.Id == searchHeroId);
            if (sysHeroCfg != null)
            {
                intel = sysHeroCfg.Intel;
            }

            var prob = intel * 1.0 / (intel + 500);

            var visitorItems = Utility.GetServerMapCityActivityByCityId(CityId);

            var sysVisitorCfgs = SysVisitorCfg.Items.Where(o => o.CityId == CityId).ToList();
            foreach (var sysVisitorCfg in sysVisitorCfgs)
            {
                //过滤 如果到访者已经出现在了全局的活动中 这里就寻访不到了
                if (visitorItems != null && !visitorItems.Exists(o => o.VisitorId == sysVisitorCfg.VisitorId))
                {
                    var isHit = Util.IsHit(prob + sysVisitorCfg.Probability * 1.0 / 100);
                    //第一次寻访洛阳 都可以寻访到
                    if (SearchNum == 0) isHit = true;
                    if (isHit)
                    {
                        VisitorItems.Add(new VisitorItem()
                        {
                            VisitorId = sysVisitorCfg.VisitorId,
                            VisitTimes = 1,
                            LeaveTimes = DateTime.Now.AddHours(24),
                        });
                    }
                }
            }

            SearchNum++;

            if (VisitorItems.Any())
            {
                //设置城池有变化
                Status = 1;
            }

            UseSearchNum += 1;

            //添加寻访次数
            Utility.AddDailyTaskGoalData(Pid, DailyType.SearchVisitor);
        }

        /// <summary>
        /// 刷新组！
        /// </summary>
        public void Refresh()
        {
            var serverMapCityId = Utility.GetServerMapCityId(SysCityCfg.TeamId > 0 ? SysCityCfg.TeamId : CityId);
            var serverMapCity = Storage.Load<ServerMapCity>(serverMapCityId, true);
            var serverMapCityItem = serverMapCity.GetServerMapCityItemId(Pid, CityId, false, ServerMapCityItemId);
            ServerMapCityItemId = serverMapCityItem.Id;
            if (serverMapCityItem.OwnerType == OwnerType.User)
            {
                if (serverMapCityItem.OwnerId == Pid)
                {
                    OwnerType = OwnerType.Own;
                }
                else
                    OwnerType = OwnerType.User;
            }
            else
            {
                OwnerType = OwnerType.Npc;
            }
            MatchOrBeOccupiedTime = DateTime.Now;
            serverMapCityItem.OccupiedOrGainTime = DateTime.Now;
            //OccupiedOrGainTime = DateTime.Now;
            Status = 0;
        }
    }
    /// <summary>
    /// 领地项信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserDomainItem : EntityItem
    {
        /// <summary>
        /// 系统城池id【对应SysCityCfg表】
        /// </summary>
        [ProtoMember(1)]
        public int CityId { get; set; }

        /// <summary>
        /// 占领类型
        /// </summary>
        [ProtoMember(2)]
        public OwnerType OwnerType { get; set; }

        /// <summary>
        /// 占领者【玩家/NPC】ID
        /// </summary>
        [ProtoMember(3)]
        public int OwnerId { get; set; }

        /// <summary>
        /// 匹配的到全服城池的时间/匹配后被占的时间【用于是否可以重新匹配对手】
        /// </summary>
        [ProtoMember(4)]
        public DateTime MatchOrBeOccupiedTime { get; set; }

        /// <summary>
        /// 占领时间或者最后一次收获的时间【用于收获时计算具体的收获量】
        /// </summary>
        //[ProtoMember(5)]
        //public DateTime OccupiedOrGainTime { get; set; }

        /// <summary>
        /// 上一次侦查的时间
        /// </summary>
        [ProtoMember(6)]
        public DateTime InvestigateTime { get; set; }

        /// <summary>
        /// 可获得的资源列表
        /// </summary>
        [ProtoMember(7), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> CanRobResList { get; set; }

        /// <summary>
        /// 大于零说明 自己领地有变化
        /// </summary>
        [ProtoMember(8)]
        public int Status { get; set; }

        /// <summary>
        /// 产量
        /// </summary>
        public int Product
        {
            get
            {
                //暂时这么直接取配置 TODO:后续可能和太守有联系
                return SysCityCfg.Product;
            }
        }

        /// <summary>
        /// 资源类型
        /// </summary>
        public MoneyType MoneyType
        {
            get
            {
                return SysCityCfg.MoneyType;
            }
        }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity
        {
            get
            {
                //暂时这么直接取配置 TODO:后续可能和太守有联系
                return SysCityCfg.Capacity;
            }
        }

        /// <summary>
        /// 城池等级
        /// </summary>
        public int CityLevel
        {
            get
            {
                return SysCityCfg.Level;
            }
        }

        /// <summary>
        /// 领地配置信息
        /// </summary>
        public SysCityCfg SysCityCfg
        {
            get
            {
                var sysCityCfg = SysCityCfg.Find(CityId);
                if (sysCityCfg == null)
                    throw new ApplicationException(string.Format("SysCityCfg:Id:{0} NOT FIND", CityId));
                return sysCityCfg;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void NewObjectInit()
        {
            CanRobResList = new List<int>();
            InvestigateTime = Util.UnixEpochDateTime;
            //OccupiedOrGainTime = Util.UnixEpochDateTime;
        }

        public override void LoadInit()
        {
            CanRobResList = CanRobResList ?? new List<int>();
        }
    }

    /// <summary>
    /// 用户城池
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserCity : KVEntity
    {
        /// <summary>
        /// 城池列表
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserCityItem> CityItems { get; set; }

        /// <summary>
        /// 领地列表
        /// </summary>
        [ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserDomainItem> DomainItems { get; set; }

        /// <summary>
        /// 已经探索过的区域id列表
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> OpenedAreaIdList { get; set; }

        /// <summary>
        /// 上次刷新领地数据的时间-用于领地更换NPC或者玩家
        /// </summary>
        [ProtoMember(4)]
        public DateTime LastReqTime { get; set; }

        /// <summary>
        /// 上次收获的时间
        /// </summary>
        [ProtoMember(5)]
        public DateTime LastGainTime { get; set; }

        /// <summary>
        /// 还未收获但已经生产出来的量【系统匹配对手或者pvp主动被占领】
        /// </summary>
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ResourceItem> OutPut { get; set; }

        /// <summary>
        /// 当前的数字（用于重新匹配！！！）
        /// </summary>
        [ProtoMember(7)]
        public int CurNum { get; set; }

        /// <summary>
        /// 事件列表
        /// </summary>
        [ProtoMember(8), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<EventItem> EventItems { get; set; }

        /// <summary>
        /// 寻访的队列个数
        /// </summary>
        //[ProtoMember(9)]
        //public int SearchQueue { get; set; }

        /// <summary>
        /// 寻访列表
        /// </summary>
        [ProtoMember(10), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<SearchItem> SearchItems { get; set; }

        /// <summary>
        /// 商店列表
        /// </summary>
        [ProtoMember(11), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ShopItem> ShopItems { get; set; }

        /// <summary>
        /// 当天商店元宝刷新数量列表
        /// </summary>
        [ProtoMember(12), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<DayZeorValue> TodayShopRefreshNumList { get; set; }

        /// <summary>
        /// 商店元宝刷新总数量列表
        /// </summary>
        [ProtoMember(13), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> ShopRefreshNumList { get; set; }

        /// <summary>
        /// 今日征收领地资源次数
        /// </summary>
        [ProtoMember(14), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue GainDomainResNum { get; set; }

        /// <summary>
        /// 今日强征收领地资源次数
        /// </summary>
        [ProtoMember(15), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue StrGainDomainResNum { get; set; }

        /// <summary>
        /// 当前剧情任务ID
        /// </summary>
        [ProtoMember(16)]
        public int StoryTaskId { get; set; }

        /// <summary>
        /// 最后一次请求大地图数据的时间
        /// </summary>
        [ProtoMember(17)]
        public DateTime LastReqDataTime { get; set; }

        /// <summary>
        /// 是否已领取过爵位奖励
        /// </summary>
        [ProtoMember(19), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue HasGetTitleReward { get; set; }

        /// <summary>
        /// 大地图有显示我在其领地里的玩家列表
        /// </summary>
        [ProtoMember(20)]
        public List<int> ShowMineOfOtherUserIdList { get; set; }

        /// <summary>
        /// 暗黑军团活动NpcId
        /// </summary>
        [ProtoMember(21)]
        public int DiabloNpcId { get; set; }

        /// <summary>
        /// 暗黑军团活动上次开始的时间
        /// </summary>
        [ProtoMember(22)]
        public DateTime DiabloBeginTime { get; set; }

        public DateTime DiabloEndTime
        {
            get
            {
                var sysActivityCfg = SysActivityCfg.Items.FirstOrDefault(o => o.Type == ActivityType.Diablo && o.IsClose == 0);
                if (sysActivityCfg != null)
                {
                    return sysActivityCfg.GetEndTime();
                }
                return DateTime.Now;
            }
        }

        public override void NewObjectInit()
        {
            ShowMineOfOtherUserIdList = new List<int>();
            CityItems = new List<UserCityItem>();
            DomainItems = new List<UserDomainItem>();
            OpenedAreaIdList = new List<int>();
            EventItems = new List<EventItem>();
            LastReqTime = DateTime.Now;
            LastGainTime = DateTime.Now.AddDays(-1);
            SearchItems = new List<SearchItem>();
            OutPut = new List<ResourceItem>();
            TodayShopRefreshNumList = new List<DayZeorValue>();
            ShopRefreshNumList = new List<int>();
            ShopItems = new List<ShopItem>();

            while (TodayShopRefreshNumList.Count < (int)ShopType.Max + 1) TodayShopRefreshNumList.Add(new DayZeorValue());
            while (ShopRefreshNumList.Count < (int)ShopType.Max + 1) ShopRefreshNumList.Add(0);

            CurNum = ConfigHelper.ClearBigMapNum;

            //刷新固定下来的商店
            CheckStayShop();
            InitVipNum();
            InitStoryTaskId();

            CheckAndInitDiablo();
        }

        public override void LoadInit()
        {
            ShowMineOfOtherUserIdList = ShowMineOfOtherUserIdList ?? new List<int>();
            CityItems = CityItems ?? new List<UserCityItem>();
            DomainItems = DomainItems ?? new List<UserDomainItem>();
            OpenedAreaIdList = OpenedAreaIdList ?? new List<int>();
            EventItems = EventItems ?? new List<EventItem>();
            OutPut = OutPut ?? new List<ResourceItem>();
            SearchItems = SearchItems ?? new List<SearchItem>();
            TodayShopRefreshNumList = TodayShopRefreshNumList ?? new List<DayZeorValue>();
            ShopRefreshNumList = ShopRefreshNumList ?? new List<int>();
            ShopItems = ShopItems ?? new List<ShopItem>();

            while (TodayShopRefreshNumList.Count < (int)ShopType.Max + 1) TodayShopRefreshNumList.Add(new DayZeorValue());
            while (ShopRefreshNumList.Count < (int)ShopType.Max + 1) ShopRefreshNumList.Add(0);

            if (LastGainTime < DateTime.Now.AddYears(-10))
                LastGainTime = DateTime.Now.AddDays(-1);

            var clearBigMapNum = ConfigHelper.ClearBigMapNum;
            if (clearBigMapNum > CurNum)
            {
                CityItems.Clear();
                DomainItems.Clear();
                OpenedAreaIdList.Clear();
                SearchItems.Clear();
                EventItems.Clear();

                CurNum = clearBigMapNum;

                Utility.CheckAndSetBigMapNum();
            }

            if (DomainItems.Count > 0 && OpenedAreaIdList.Count == 0)
            {
                var sysAreaCfg = SysAreaCfg.Items.FirstOrDefault(o => o.CityIdList.Contains(DomainItems[0].CityId));
                if (sysAreaCfg != null)
                    OpenedAreaIdList.Add(sysAreaCfg.Id);
            }

            if (DomainItems.Count > 0 && DomainItems.Count(o => o.OwnerType == OwnerType.Own) > 0)
            {
                var hour = DateTime.Now.Subtract(LastReqTime).TotalHours;
                if (hour > 0)
                {
                    var matchIntervalHour = ConfigHelper.BigMapCfgData.MatchIntervalHour;
                    var times = (int)(hour * 1.0 / matchIntervalHour);
                    if (times > 0)
                    {
                        MatchNpcOrUser(times);

                        LastReqTime = LastReqTime.AddHours(times * matchIntervalHour);
                    }
                }
            }
            foreach (var userCityItem in CityItems)
            {
                userCityItem.LoadInit();
            }

            //删除过期事件、寻访记录
            var eventSaveDay = ConfigHelper.BigMapCfgData.EventSaveDay;
            EventItems.RemoveAll(o => o.CreateTime.AddDays(eventSaveDay) < DateTime.Now);
            SearchItems.RemoveAll(o => o.CreateTime.AddDays(eventSaveDay) < DateTime.Now);

            //if (SearchQueue == 0) InitVipNum();

            //刷新固定下来的商店
            CheckStayShop();
            InitStoryTaskId();

            CheckAndInitDiablo();
        }

        /// <summary>
        /// 检查并初始化暗黑军团信息
        /// </summary>
        public void CheckAndInitDiablo()
        {
            var sysActivityCfg = SysActivityCfg.Items.FirstOrDefault(o => o.Type == ActivityType.Diablo && o.IsClose == 0);
            if (sysActivityCfg != null)
            {
                var hasStart = sysActivityCfg.HasActivityStart(DiabloBeginTime);
                if (hasStart)
                {
                    DiabloBeginTime = DateTime.Now;

                    DiabloNpcId = SysDiabloCfg.Items.Min(o => o.Id);
                }
            }
        }

        /// <summary>
        /// 通知其他玩家列表
        /// </summary>
        public void NoticeOtherUserList()
        {
            if (ShowMineOfOtherUserIdList.Count > 0)
            {
                var userCityList = Storage.LoadList<UserCity>(ShowMineOfOtherUserIdList.ToArray(), true);
                foreach (var userCity in userCityList)
                {
                    var userDomainItem =
                        userCity.DomainItems.FirstOrDefault(o => o.OwnerId == Id && o.OwnerType == OwnerType.User);
                    if (userDomainItem != null)
                    {
                        userDomainItem.Status = 1;
                    }
                }
            }
        }

        /// <summary>
        /// 初始化剧情任务ID
        /// </summary>
        public void InitStoryTaskId()
        {
            if (StoryTaskId == 0)
            {
                var sysTaskCfgList = SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.Story).ToList();
                if (sysTaskCfgList.Any())
                {
                    StoryTaskId = sysTaskCfgList.Min(o => o.Id);
                }
            }
            else if (StoryTaskId == -1)
            {
                StoryTaskId = 10000000 + 501027;
            }
            else if (StoryTaskId > 10000000)
            {
                var maxTaskId = SysTaskCfg.Items.Where(o => o.TaskSort == TaskSort.Story).Max(o => o.Id);
                var oldTaskId = StoryTaskId - 10000000;
                if (maxTaskId > oldTaskId)
                {
                    StoryTaskId = oldTaskId + 1;
                }
            }
        }

        /// <summary>
        /// 刷新领地内的玩家
        /// </summary>
        /// <param name="cityId"></param>
        public void RefreshDomainUser(int cityId)
        {
            MatchNpcOrUser(1, cityId);
        }

        /// <summary>
        /// 初始化每天VIP免费次数
        /// </summary>
        public void InitVipNum(int vipLevel = 0)
        {
            //SearchQueue = Utility.GetVipNum(Id, VipNumType.SearchVisitor, vipLevel);
            //if (SearchQueue == 0) SearchQueue = 1;

            //修改神秘/西域商人所在的城池
            var shopCfgData = ConfigHelper.ShopCfgData;
            var index = 0;
            foreach (var i in shopCfgData.StayCityNeedVipList)
            {
                if (vipLevel >= i)
                {
                    var shop = ShopItems.FirstOrDefault(o => o.ShopType == (ShopType)(index + 2));
                    if (shop != null)
                    {
                        shop.CityId = shopCfgData.BussinessStayCityList[index + 1];
                        shop.LeaveTime = DateTime.Now.AddYears(30);
                    }
                }
                index++;
            }
        }

        /// <summary>
        /// 判断是否有资源可以收获
        /// </summary>
        /// <returns></returns>
        public bool CheckHaveResGain()
        {
            OutPut = OutPut ?? new List<ResourceItem>();
            if (OutPut.Sum(o => o.Num) > 0) return true;
            if (CityItems.Count(o => o.OwnerType == OwnerType.Own) > 0) return true;
            if (DomainItems.Count(o => o.OwnerType == OwnerType.Own) > 0) return true;
            return false;
        }

        /// <summary>
        /// 获得下次可以征收的时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetRealGainTime()
        {
            var gainIntervalHour = ConfigHelper.BigMapCfgData.GainIntervalHour;
            var gainTime = LastGainTime.AddHours(gainIntervalHour);
            if (CheckHaveResGain()) return gainTime;
            else return DateTime.Now.AddDays(1);
        }

        /// <summary>
        /// 初始默认开启的城池和领地
        /// </summary>
        public void CheckAndAddCityDomain()
        {
            var initDomainIdList = ConfigHelper.BigMapCfgData.InitDomainIdList;
            foreach (var i in initDomainIdList)
            {
                if (!DomainItems.Exists(o => o.CityId == i))
                    OpenDomain(i);
            }

            var initCityIdList = ConfigHelper.BigMapCfgData.InitCityIdList;
            foreach (var i in initCityIdList)
            {
                if (!CityItems.Exists(o => o.CityId == i))
                    OpenCity(i);
            }
        }

        /// <summary>
        /// 检查是否有新消息
        /// </summary>
        /// <returns></returns>
        public int HasNewMsg()
        {
            if (CityItems.Count == 0 && DomainItems.Count == 0) return 0;
            var gainIntervalHour = ConfigHelper.BigMapCfgData.GainIntervalHour;
            if (LastGainTime.AddHours(gainIntervalHour) < DateTime.Now)
            {
                if (CheckHaveResGain()) return 1;
            }
            if (EventItems.Exists(o => o.IsRead == 0 || o.IsReq == 0)) return 1;
            return 0;
        }

        /// <summary>
        /// 改变大地图新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeNewMsg(UserRole userRole = null)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var openCfgItem = ConfigHelper.OpenCfgsData.FirstOrDefault(o => o.Type == OpenModuleType.BigMap);
            if (openCfgItem == null || userRole.Level >= openCfgItem.Level)
                userRole.SetHasNewMsg((int)NewMsgType.BigMap, HasNewMsg() > 0 ? 1 : 0);
        }

        /// <summary>
        /// 添加地图事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isWin">攻击方是否胜利/领地系统匹配的时间index</param>
        /// <param name="userId"></param>
        /// <param name="cityId"></param>
        /// <param name="nickName"></param>
        /// <param name="cityName"></param>
        /// <param name="battleId"></param>
        /// <param name="robResList"></param>
        public void AddEvent(EventType type, int isWin = 0, int userId = 0, int cityId = 0, string nickName = "", string cityName = "", int battleId = 0,
            List<ItemPair> robResList = null)
        {
            var item = KVEntity.CreateNew<EventItem>();
            item.CityId = cityId;
            item.IsWin = isWin;
            item.BattleId = battleId;
            if (userId > 0 && string.IsNullOrEmpty(nickName))
            {
                var userRole = Storage.Load<UserRole>(userId);
                nickName = userRole.NickName;
            }
            item.Type = type;
            if (type == EventType.Domain)
            {
                //var repute = ConfigHelper.BigMapCfgData.BeRobedRepute;
                if (isWin >= 20000000)
                {
                    //匹配的是NPC
                    isWin -= 20000000;
                    var sysBigMapCfg = SysBigMapCfg.Find(userId);
                    if (sysBigMapCfg != null)
                        nickName = sysBigMapCfg.NickName;
                    item.Msg = LangResource.GetLangResource(ResourceId.R_DomainBeMatchNpc, nickName);//, repute
                }
                else
                {
                    //匹配的是玩家
                    isWin -= 10000000;
                    var userRole = Storage.Load<UserRole>(userId);
                    nickName = userRole.NickName;
                    item.Msg = LangResource.GetLangResource(ResourceId.R_DomainBeMatchUser, userId, nickName);//, repute
                }
                item.IsWin = 1;
                //具体系统匹配的时间，有可能是一起匹配的但是逻辑是一个个匹配时间上得分开
                item.CreateTime = LastReqTime.AddHours(isWin);
            }
            else if (type == EventType.City)
            {
                if (isWin == 0)
                {
                    item.Msg =
                        LangResource.GetLangResource(ResourceId.R_BeCityUserFail, userId, nickName, cityName);

                    //城池防守成功
                    item.Money = Util.GetRandom(1, 6);
                }
                else
                {
                    robResList = robResList ?? new List<ItemPair>();
                    if (robResList.Count == 0) robResList.Add(new ItemPair((int)SpecialToolId.Coin, 0));
                    var resName = Utility.GetSpecialToolIdName((SpecialToolId)robResList[0].ItemId);
                    var resNum = robResList[0].Num;

                    item.Msg =
                        LangResource.GetLangResource(ResourceId.R_BeCityUserWin, userId, nickName, cityName, resNum, resName);
                }
            }
            else if (type == EventType.Main)
            {
                if (isWin == 0)
                {
                    item.Msg = LangResource.GetLangResource(ResourceId.R_BeDomainUserFail, userId, nickName);

                    //宛城防守成功
                    item.Money = Util.GetRandom(1, 6);
                }
                else
                {
                    var robedResList = new List<int>();
                    while (robedResList.Count < 4)
                    {
                        robedResList.Add(0);
                    }
                    if (robResList != null)
                    {
                        foreach (var itemPair in robResList)
                        {
                            if (itemPair.ItemId == (int)SpecialToolId.Coin) robedResList[0] = itemPair.Num;
                            else if (itemPair.ItemId == (int)SpecialToolId.Wood) robedResList[1] = itemPair.Num;
                            else if (itemPair.ItemId == (int)SpecialToolId.Stone) robedResList[2] = itemPair.Num;
                            else if (itemPair.ItemId == (int)SpecialToolId.Iron) robedResList[3] = itemPair.Num;
                        }
                    }
                    if (robResList != null && robResList.Count > 0)
                        item.ItemPairList = robResList.Where(o => o.Num > 0).ToList();

                    item.Msg = LangResource.GetLangResource(ResourceId.R_BeDomainUserWin, userId, nickName);//,
                    //robedResList[0], robedResList[1], robedResList[2], robedResList[3]
                }
            }

            EventItems.Add(item);

            //删除多于事件项
            var eventSaveNum = ConfigHelper.BigMapCfgData.EventSaveNum;
            if (EventItems.Count >= eventSaveNum)
                EventItems.RemoveAt(0);

            ChangeNewMsg();
        }

        ////// <summary>
        ///// 添加寻访记录
        ///// </summary>
        ///// <param name="visitorItems"></param>
        ///// <param name="cityId"></param>
        ///// <param name="heroId"></param>
        ///// <param name="createTime"></param>
        ///// <param name="isReq"></param>
        //public SearchItem AddSearch(List<VisitorItem> visitorItems, int cityId, int heroId, DateTime createTime, int isReq = 0)
        //{
        //    var item = KVEntity.CreateNew<SearchItem>();
        //    var sysCityCfg = SysCityCfg.Find(o => o.Id == cityId);
        //    var cityName = sysCityCfg.Name;
        //    if (visitorItems.Any())
        //    {
        //        item.IsWin = 1;
        //        var visitorMsg = Utility.GetVisitorMsg(visitorItems);
        //        item.Msg = LangResource.GetLangResource(ResourceId.R_SearchSuccess, cityName, visitorMsg);
        //    }
        //    else
        //    {
        //        //寻访失败
        //        item.Msg = LangResource.GetLangResource(ResourceId.R_SearchFail, cityName);
        //    }
        //    item.SearchCityId = cityId;
        //    item.SearchHeroId = heroId;
        //    item.CreateTime = createTime;
        //    item.IsReq = isReq;
        //    if (item.IsReq == 1) item.CreateTime = DateTime.Now;
        //    SearchItems.Add(item);

        //    var eventSaveNum = ConfigHelper.BigMapCfgData.EventSaveNum;
        //    if (SearchItems.Count >= eventSaveNum)
        //        SearchItems.RemoveAt(0);

        //    ChangeNewMsg();

        //    return item;
        //}

        /// <summary>
        /// 获得寻访记录描述
        /// </summary>
        public string GetSearchMsg(List<VisitorItem> visitorItems, string cityName)
        {
            if (visitorItems.Any())
            {
                //寻访成功
                var visitorMsg = Utility.GetVisitorMsg(visitorItems);
                return LangResource.GetLangResource(ResourceId.R_SearchSuccess, cityName, visitorMsg);
            }
            else
            {
                //寻访失败
                return LangResource.GetLangResource(ResourceId.R_SearchFail, cityName);
            }
        }

        /// <summary>
        /// 增加产出量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="num"></param>
        public void AddOutPut(MoneyType type, int num)
        {
            OutPut = OutPut ?? new List<ResourceItem>();
            var item = OutPut.FirstOrDefault(o => o.Type == type);
            if (item == null)
            {
                OutPut.Add(new ResourceItem() { Type = type, Num = num });
            }
            else
            {
                item.Num += num;
            }
        }

        /// <summary>
        /// 随机匹配Npc或者玩家 到领地城
        /// </summary>
        /// <param name="times"></param>
        /// <param name="cityId">只需要重新匹配的领地</param>
        public void MatchNpcOrUser(int times, int cityId = 0)
        {
            var domainItems = DomainItems.Where(o => o.OwnerType == OwnerType.Own).ToList();
            if (domainItems.Count == 0 && cityId == 0) return;//没有领地停止匹配
            //if (domainItems.Count < (int)(DomainItems.Count * 0.5) && cityId == 0) return;//剩余领地不足50%停止匹配
            var totalDomainNum = DomainItems.Count;
            var ownDomainNum = DomainItems.Count(o => o.OwnerType == OwnerType.Own);

            //可以刷到敌人（玩家、npc）的概率
            var showPrec = ownDomainNum*1.0/totalDomainNum;

            //var precDomainNum = (int)(totalDomainNum * 0.2);
            //var bigMapMatchMaxUserNum = ConfigHelper.BigMapMatchMaxUserNum;
            //var bigMapMatchMaxNpcNum = ConfigHelper.BigMapMatchMaxNpcNum;

            //var maxUserNum = precDomainNum > bigMapMatchMaxUserNum ? bigMapMatchMaxUserNum : precDomainNum;
            //var maxNpcNum = precDomainNum > bigMapMatchMaxNpcNum ? bigMapMatchMaxNpcNum : precDomainNum;

            //领地里的玩家
            var userIdList =
            DomainItems.Where(o => o.OwnerId > 0 && o.OwnerType == OwnerType.User)
                .Select(o => o.OwnerId)
                .ToList();

            var npcIdList =
            DomainItems.Where(o => o.OwnerId > 0 && o.OwnerType == OwnerType.Npc)
                .Select(o => o.OwnerId)
                .ToList();

            //NPC和玩家占领的领地 大于20% 或者 8个时 不匹配了 最大值了
            //if (userIdList.Count >= maxUserNum && npcIdList.Count >= maxNpcNum && cityId == 0) return;

            var index = 1;
            if (cityId > 0) times = 1;
            while (times > 0 && Util.IsHit(showPrec))
            {
                var userDomainItem = domainItems.FirstOrDefault(o => o.CityId == cityId);
                var isUser = false;
                if (cityId > 0)
                {
                    userDomainItem = DomainItems.FirstOrDefault(o => o.CityId == cityId);
                    if (userDomainItem == null)
                    {
                        throw new ApplicationException(string.Format("DomainItems:CityId:{0}Not Find", cityId));
                    }
                    isUser = true;
                    userIdList.Add(userDomainItem.OwnerId);//原来在领地里的玩家排除 排除
                }
                else
                {
                    if (domainItems.Count == 0) break;
                    var r = Util.GetRandom(0, domainItems.Count);
                    userDomainItem = domainItems[r];
                    
                    var domainNpcNum = npcIdList.Count;
                    var domainUserNum = userIdList.Count;
                    if (domainNpcNum >= domainUserNum) isUser = true;
                    else isUser = false;

                    //isUser = Util.IsHit(ConfigHelper.BigMapCfgData.MatchUserPrec * 1.0 / 100);
                    //if ((int)(userIdList.Count * 0.5) > npcIdList.Count) isUser = false;//如果玩家数量的一半比NPC多，则比配NPC
                    //if ((int)(npcIdList.Count * 0.5) > userIdList.Count) isUser = true;////如果NPC数量的一半比玩家多，则比配玩家
                    //if (userIdList.Count >= maxUserNum) isUser = false;//如果玩家匹配数量已达到极限了，匹配NPC
                    //if (npcIdList.Count >= maxNpcNum) isUser = true;//如果NPC匹配数量已达极限了，匹配玩家
                }
                var addNum = 10000000;
                if (isUser)
                {
                    var ownerId = Utility.GetOwnerUserId(Id, userIdList);
                    if (ownerId > 0)
                    {
                        userDomainItem.OwnerType = OwnerType.User;
                        userDomainItem.OwnerId = ownerId;

                        //把被匹配的玩家id添加到显示在了的玩家列表里面
                        var ouserCity = Storage.Load<UserCity>(ownerId, true);
                        ouserCity.ShowMineOfOtherUserIdList.Add(Id);

                        userIdList.Add(ownerId);//新匹配的玩家要添加进去 排除
                    }
                    else
                    {
                        //匹配Npc 
                        //exceptIdList =
                        //DomainItems.Where(o => o.OwnerId > 0 && o.OwnerType == OwnerType.Npc)
                        //    .Select(o => o.OwnerId)
                        //    .ToList();, exceptIdList
                        var userRole = DataStorage.Current.Load<UserRole>(Id);
                        ownerId = Utility.GetOwnerNpcId(userRole.Level);
                        if (ownerId > 0)
                        {
                            userDomainItem.OwnerType = OwnerType.Npc;
                            userDomainItem.OwnerId = ownerId;

                            npcIdList.Add(ownerId);//新匹配的NPC要添加进去 计算数量
                        }
                        else
                        {
                            return;
                        }
                        addNum += 10000000;
                    }
                }
                else
                {
                    //匹配Npc 
                    //var exceptIdList =
                    //DomainItems.Where(o => o.OwnerId > 0 && o.OwnerType == OwnerType.Npc)
                    //    .Select(o => o.OwnerId)
                    //    .ToList();, exceptIdList
                    //var ownerId = Utility.GetOwnerNpcId(userDomainItem.CityLevel);
                    var userRole = DataStorage.Current.Load<UserRole>(Id);
                    var ownerId = Utility.GetOwnerNpcId(userRole.Level);
                    if (ownerId > 0)
                    {
                        userDomainItem.OwnerType = OwnerType.Npc;
                        userDomainItem.OwnerId = ownerId;
                    }
                    else
                    {
                        return;
                    }
                    addNum += 10000000;
                }

                //var repute = ConfigHelper.BigMapCfgData.BeRobedRepute;
                if (userDomainItem.OwnerType != OwnerType.Own)
                {
                    //收获资源
                    GainDomainItems(userDomainItem.Id);
                    userDomainItem.MatchOrBeOccupiedTime = DateTime.Now;

                    //被玩家或者NPC占领扣除声望——声望不扣除作为累积
                    //Utility.ConsumeResource(null, ItemType.Repute, (int)OpcodeType.UserCity_MatchNpcOrUser
                    //, repute, Id);
                }
                //添加事件
                AddEvent(EventType.Domain, index + addNum, userDomainItem.OwnerId, userDomainItem.CityId);
                times--;
                index++;
                //自己领地有数据变更
                userDomainItem.Status = 1;

                ownDomainNum = DomainItems.Count(o => o.OwnerType == OwnerType.Own);
                //可以刷到敌人（玩家、npc）的概率
                showPrec = ownDomainNum * 1.0 / totalDomainNum;
            }
        }

        /// <summary>
        /// 收获领地资源
        /// </summary>
        /// <param name="domainItemId">默认0代表收获全部领地</param>
        public int GainDomainItems(int domainItemId = 0)
        {
            var outPut = 0;
            var domainItemIdList = new List<int>();
            if (domainItemId > 0)
            {
                domainItemIdList.Add(domainItemId);
            }
            else
            {
                domainItemIdList = DomainItems.Where(o => o.OwnerType == OwnerType.Own).Select(o => o.Id).ToList();
            }

            foreach (var i in domainItemIdList)
            {
                var userDomainItem = DomainItems.FirstOrDefault(o => o.Id == i);
                if (userDomainItem == null)
                    throw new ApplicationException(string.Format("UserDomainItem:Id:{0} NOT FIND", i));

                //var totalSecond = DateTime.Now.Subtract(userDomainItem.OccupiedOrGainTime).TotalSeconds;
                //var outPut = (int)(totalSecond * 1.0 * userDomainItem.Product / 3600);
                //outPut = outPut > userDomainItem.Capacity ? userDomainItem.Capacity : outPut;
                //userDomainItem.OccupiedOrGainTime = DateTime.Now;

                outPut += userDomainItem.Product;
                //添加至已产出但未收获到玩家身上字段
                //AddOutPut(userDomainItem.MoneyType, userDomainItem.Product);
            }
            return outPut;
        }

        /// <summary>
        /// 收获城池资源
        /// </summary>
        /// <param name="cityItemId">默认0代表收获全部城池</param>
        /// <param name="ifAddToOutPut">1代表得收获，0仅仅计算而已</param>
        public int GainCityItems(int cityItemId = 0, int ifAddToOutPut = 1)
        {
            var cityItemIdList = new List<int>();
            var serverMapCityItemIdList =
                CityItems.Where(o => o.OwnerType == OwnerType.Own).Select(o => o.ServerMapCityItemId).ToList();
            if (cityItemId > 0)
            {
                cityItemIdList.Add(cityItemId);
                var userCityItem = CityItems.FirstOrDefault(o => o.Id == cityItemId);
                if (userCityItem == null)
                    throw new ApplicationException(string.Format("CityItems:Id:{0}", cityItemId));
                serverMapCityItemIdList.Add(userCityItem.ServerMapCityItemId);
            }
            else
            {
                cityItemIdList = CityItems.Where(o => o.OwnerType == OwnerType.Own).Select(o => o.Id).ToList();
            }
            var outPut = 0;
            var serverMapCityItemList =
                DataStorage.Current.LoadList<ServerMapCityItem>(serverMapCityItemIdList.ToArray(), true);
            foreach (var i in cityItemIdList)
            {
                var userCityItem = CityItems.FirstOrDefault(o => o.Id == i);
                if (userCityItem == null)
                    throw new ApplicationException(string.Format("UserCityItem:Id:{0} NOT FIND", i));

                var serverMapCityItem = serverMapCityItemList.Find(o => o.Id == userCityItem.ServerMapCityItemId);
                if (serverMapCityItem == null)
                    throw new ApplicationException(string.Format("serverMapCityItem:Id:{0} NOT FIND",
                        userCityItem.ServerMapCityItemId));

                var totalSecond = DateTime.Now.Subtract(serverMapCityItem.OccupiedOrGainTime).TotalSeconds;
                outPut = (int)(totalSecond * 1.0 * userCityItem.Product / 3600);
                outPut = outPut > userCityItem.Capacity ? userCityItem.Capacity : outPut;
                //userCityItem.OccupiedOrGainTime = DateTime.Now;
                serverMapCityItem.OccupiedOrGainTime = DateTime.Now;

                //添加至已产出但未收获到玩家身上字段
                if (ifAddToOutPut == 1) AddOutPut(userCityItem.MoneyType, outPut);
            }
            return outPut;
        }

        /// <summary>
        /// 添加城池
        /// </summary>
        /// <param name="cityId">城池Id</param>
        /// <returns></returns>
        public UserCityItem OpenCity(int cityId)
        {
            CityItems = CityItems ?? new List<UserCityItem>();
            if (CityItems.Exists(o => o.CityId == cityId))
                throw new ApplicationException(string.Format("City Already Exist,CityId:{0}", cityId));

            var sysCityCfg = SysCityCfg.Find(cityId);
            if (sysCityCfg == null) throw new ApplicationException(string.Format("SysCityCfg:Id:{0} NOT FIND", cityId));

            if (sysCityCfg.Type != CityType.City)
                throw new ApplicationException(string.Format("Not CityType,CityId:{0}", cityId));

            bool isCreateNew = false;
            var teamFirstCityCfg = SysCityCfg.Items.Where(o => o.TeamId == sysCityCfg.TeamId).OrderBy(o => o.Id).FirstOrDefault();
            if (teamFirstCityCfg != null && teamFirstCityCfg.Id == sysCityCfg.Id)
            {
                isCreateNew = true;
            }

            var userCityItem = CreateNew<UserCityItem>();
            userCityItem.Pid = Id;
            userCityItem.CityId = cityId;
            userCityItem.MatchOrBeOccupiedTime = DateTime.Now;

            var serverMapCityId = Utility.GetServerMapCityId(sysCityCfg.TeamId > 0 ? sysCityCfg.TeamId : cityId);
            var serverMapCity = Storage.Load<ServerMapCity>(serverMapCityId, true);
            var serverMapCityItem = serverMapCity.GetServerMapCityItemId(Id, cityId, isCreateNew);
            userCityItem.ServerMapCityItemId = serverMapCityItem.Id;
            if (serverMapCityItem.OwnerType == OwnerType.User)
            {
                if (serverMapCityItem.OwnerId == Id)
                {
                    userCityItem.OwnerType = OwnerType.Own;

                    Utility.AddMainLineTaskGoalData(Id, MainLineType.OccupiedCity, cityId);
                }
                else
                    userCityItem.OwnerType = OwnerType.User;
            }
            else
            {
                userCityItem.OwnerType = OwnerType.Npc;
            }

            CityItems.Add(userCityItem);

            return userCityItem;
        }

        /// <summary>
        /// 添加领地
        /// </summary>
        /// <param name="cityId">领地Id</param>
        /// <returns></returns>
        public UserDomainItem OpenDomain(int cityId)
        {
            DomainItems = DomainItems ?? new List<UserDomainItem>();
            if (DomainItems.Exists(o => o.CityId == cityId))
                throw new ApplicationException(string.Format("Domain Already Exist,CityId:{0}", cityId));

            var sysCityCfg = SysCityCfg.Find(cityId);
            if (sysCityCfg == null) throw new ApplicationException(string.Format("SysCityCfg:Id:{0} NOT FIND", cityId));

            if (sysCityCfg.Type != CityType.Domain)
                throw new ApplicationException(string.Format("Not DomainType,CityId:{0}", cityId));

            var userDomainItem = CreateNew<UserDomainItem>();
            userDomainItem.Pid = Id;
            userDomainItem.CityId = cityId;

            if (sysCityCfg.IsNpc == 0)
            {
                //刚开区即匹配对手给玩家
                var exceptIdList =
                    DomainItems.Where(o => o.OwnerId > 0 && o.OwnerType == OwnerType.User)
                        .Select(o => o.OwnerId)
                        .ToList();
                userDomainItem.OwnerId = Utility.GetOwnerUserId(Id, exceptIdList);
                if (userDomainItem.OwnerId > 0)
                {
                    userDomainItem.OwnerType = OwnerType.User;
                    userDomainItem.MatchOrBeOccupiedTime = DateTime.Now;

                    //把被匹配的玩家id添加到显示在了的玩家列表里面
                    var ouserCity = Storage.Load<UserCity>(userDomainItem.OwnerId, true);
                    ouserCity.ShowMineOfOtherUserIdList.Add(Id);
                }
            }
            if (userDomainItem.OwnerId == 0)
            {
                //匹配不到对手或者领地最开始即为NPC占领
                userDomainItem.OwnerType = OwnerType.Npc;
                userDomainItem.OwnerId = sysCityCfg.NpcId;
            }

            DomainItems.Add(userDomainItem);

            return userDomainItem;
        }

        /// <summary>
        /// 检查固定下来的商人物品是否需要刷新
        /// </summary>
        public void CheckStayShop()
        {
            var shopCfgData = ConfigHelper.ShopCfgData;
            var refreshTimeList = new List<DateTime>();
            foreach (var i in shopCfgData.LuoYangRefreshTimeList)
            {
                refreshTimeList.Add(DateTime.Today.AddHours(i));
            }
            var index1 = 0;
            foreach (var dateTime in refreshTimeList)
            {
                if (dateTime > DateTime.Now) break;
                index1++;
            }
            var sList = new List<ShopType>() { ShopType.LuoYang, ShopType.Mysterious, ShopType.Western, ShopType.Pk };
            var userRole = DataStorage.Current.Load<UserRole>(Id);
            var vipLevel = userRole.RealVipLevel;
            var needVipLevel = 0;
            foreach (var shopType in sList)
            {
                var shop = ShopItems.FirstOrDefault(o => o.ShopType == shopType);
                needVipLevel = shopType == ShopType.LuoYang ? 0 : shopCfgData.StayCityNeedVipList[(int)shopType - 2];
                if (shop == null)
                {
                    if (shopType == ShopType.LuoYang)
                    {
                        //还不存在该商店 生成!
                        RefreshShop(shopType);
                        //return;
                    }
                    else //if (shopType == ShopType.Mysterious || shopType == ShopType.Western)
                    {
                        if (vipLevel >= needVipLevel)
                        {
                            RefreshShop(shopType, -1);
                        }
                        //return;
                    }
                }
                else
                {
                    var index2 = 0;
                    if (shop.LeaveTime.ToTs() <= shopCfgData.BussinessStayTime)
                    {
                        //没有固定下来
                        index2 = -1;
                        if (vipLevel >= needVipLevel)
                        {
                            //需要固定下来了
                            shop.LeaveTime = DateTime.Now.AddYears(30);
                            shop.CityId = shopCfgData.BussinessStayCityList[(int)shopType - 1];
                        }
                    }
                    else
                    {
                        foreach (var dateTime in refreshTimeList)
                        {
                            if (dateTime > shop.LastRefreshTime) break;
                            index2++;
                        }
                    }

                    if (index1 > index2 && index2 != -1)
                    {
                        RefreshShop(shopType);
                        //if (shopType == ShopType.LuoYang)
                        //{
                        //    //到了刷新点 刷新
                        //    RefreshLuoYangShop();
                        //}
                        //else if (shopType == ShopType.Mysterious || shopType == ShopType.Western)
                        //{
                        //    RefreshMysteriousOrWesternShop(shopType);
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// 刷新/初始化洛阳商人物品列表
        /// </summary>
        //public void RefreshLuoYangShop(int useMoney = 0)
        //{
        //    var shopCfgData = ConfigHelper.ShopCfgData;
        //    var shop = ShopItems.FirstOrDefault(o => o.ShopType == ShopType.LuoYang);
        //    if (shop == null)
        //    {
        //        //不存在该商店 生成一个
        //        shop = new ShopItem();
        //        shop.ShopType = ShopType.LuoYang;
        //        shop.CityId = shopCfgData.BussinessStayCityList[(int)ShopType.LuoYang - 1];
        //        shop.LeaveTime = DateTime.Now.AddYears(30);

        //        ShopItems.Add(shop);
        //    }

        //    //刷新
        //    if (useMoney == 0)
        //        shop.LastRefreshTime = DateTime.Now;
        //    else
        //    {
        //        var index = (int)ShopType.LuoYang - 1;
        //        TodayShopRefreshNumList[index] += 1;
        //        ShopRefreshNumList[index]++;
        //    }
        //    shop.GoodsItems = GetShopGoodItem(ShopType.LuoYang, shopCfgData.TotalGoodsNum);
        //}

        /// <summary>
        /// 随机获取该类型的物品列表
        /// </summary>
        /// <param name="shopType"></param>
        /// <returns></returns>
        public List<ShopGoodTtem> GetShopGoodItem(ShopType shopType, int num)
        {
            var userRole = Storage.Load<UserRole>(Id);
            var list = new List<ShopGoodTtem>();

            var sysShopCfgList =
                SysShopCfg.Items.Where(
                    o => o.ShopType == shopType && o.ExtractWeights > 0 && o.NeedLevel <= userRole.Level).ToList();
            var wList = sysShopCfgList.Select(o => o.ExtractWeights).ToList();

            var findCount = num * 3;
            while (num > 0 && findCount > 0)
            {
                findCount--;
                var shopGoodsItem = new ShopGoodTtem();

                int index = Utility.GetIndexFromWeightsList(wList);
                if (index < sysShopCfgList.Count)
                {
                    var sysShopCfg = sysShopCfgList[index];
                    if (list.Exists(o => o.Id == sysShopCfg.Id))
                    {
                        continue;
                    }

                    shopGoodsItem.Id = sysShopCfg.Id;
                    //shopGoodsItem.ItemType = sysShopCfg.ItemType;
                    //shopGoodsItem.ItemId = sysShopCfg.GoodsId;
                    shopGoodsItem.SNum = Util.GetRandom(1, sysShopCfg.Num + 1);
                    list.Add(shopGoodsItem);
                    num--;
                }
            }

            //var sysShopPrecCfgItems = SysShopPrecCfg.Items.Where(o => o.Type == shopType).ToList();

            //while (num > 0)
            //{
            //    var shopGoodsItem = new ShopGoodTtem();
            //    shopGoodsItem.Id = (int)shopType * 100 + num;

            //    int index = Utility.GetIndexFromWeightsList(sysShopPrecCfgItems.Select(o => o.TeamPre).ToList());
            //    var sysShopPrecCfg = sysShopPrecCfgItems[index];

            //    var randPrecList = new List<int>()
            //                        {
            //                            sysShopPrecCfg.White,
            //                            sysShopPrecCfg.Green,
            //                            sysShopPrecCfg.Blue,
            //                            sysShopPrecCfg.Purple,
            //                            sysShopPrecCfg.Orange
            //                        };
            //    var quality = Utility.GetIndexFromWeightsList(randPrecList);

            //    var eType = sysShopPrecCfg.ItemType;
            //    shopGoodsItem.ItemType = eType;

            //    if (eType == ExtractItemType.Tool || eType == ExtractItemType.HeroChip ||
            //        eType == ExtractItemType.ConcubineChip || eType == ExtractItemType.EquipChip || eType == ExtractItemType.PetChip)
            //    {
            //        var itemList = SysToolCfg.Items;
            //        if (eType == ExtractItemType.Tool)
            //            itemList = SysToolCfg.Items.Where(
            //                o =>
            //                    o.ExtractWeights > 0 && o.GetToolType() < ToolType.HeroChip &&
            //                    o.Quality == (ItemQuality)quality).ToList();
            //        else if (eType == ExtractItemType.HeroChip)
            //        {
            //            itemList = SysToolCfg.Items.Where(
            //                o =>
            //                    o.ExtractWeights > 0 && o.GetToolType() == ToolType.HeroChip &&
            //                    o.Quality == (ItemQuality)quality).ToList();
            //        }
            //        else if (eType == ExtractItemType.ConcubineChip)
            //        {
            //            itemList = SysToolCfg.Items.Where(
            //                o =>
            //                    o.ExtractWeights > 0 && o.GetToolType() == ToolType.ConcubineChip &&
            //                    o.Quality == (ItemQuality)quality).ToList();
            //        }
            //        else if (eType == ExtractItemType.EquipChip)
            //        {
            //            itemList = SysToolCfg.Items.Where(
            //                o =>
            //                    o.ExtractWeights > 0 && o.GetToolType() == ToolType.EquipChip &&
            //                    o.Quality == (ItemQuality)quality).ToList();
            //        }
            //        if (itemList.Count == 0)
            //        {
            //            throw new ApplicationException(string.Format("itemList is null,eType:{0},quality:{1}", (int)eType, quality));
            //        }
            //        var rItemIndex = Utility.GetIndexFromWeightsList(itemList.Select(o => o.ExtractWeights).ToList());
            //        if (rItemIndex < itemList.Count)
            //        {
            //            var rItem = itemList[rItemIndex];

            //            shopGoodsItem.ItemId = rItem.Id;
            //        }
            //    }
            //    else if (eType == ExtractItemType.Equip)
            //    {
            //        var equipList = SysEquipCfg.Items.Where(o => o.Quality == (ItemQuality)quality).ToList();
            //        if (equipList.Count == 0)
            //        {
            //            throw new ApplicationException(string.Format("SysEquipCfg is null,quality:{0}", quality));
            //        }
            //        var rEquipIndex = Util.GetRandom(0, equipList.Count);
            //        if (rEquipIndex < equipList.Count)
            //        {
            //            var rEquip = equipList[rEquipIndex];

            //            shopGoodsItem.ItemId = rEquip.Id;
            //        }
            //    }
            //    else
            //    {
            //        throw new ApplicationException(string.Format("shop not have is null,eType:{0}", (int)eType));
            //    }

            //    shopGoodsItem.Num = Util.GetRandom(1, sysShopPrecCfg.Num + 1);
            //    list.Add(shopGoodsItem);
            //    num--;
            //}

            return list;
        }

        /// <summary>
        /// 概率出现神秘/西域商人
        /// </summary>
        /// <returns></returns>
        public int ShowMysteriousOrWesternShop(int cityId, out ShopType shopType)
        {
            shopType = ShopType.None;
            var shopCfgData = ConfigHelper.ShopCfgData;

            var userRole = Storage.Load<UserRole>(Id);
            var vipLevel = userRole.RealVipLevel;

            var index = 0;
            foreach (var i in shopCfgData.BussinessStayCityList)
            {
                if (index == 0)
                {
                    //跳过洛阳商人
                    index++;
                    shopType = ShopType.Mysterious;
                    continue;
                }
                var stayCityneedVipLevel = shopCfgData.StayCityNeedVipList[(int)shopType - 2];
                if (vipLevel >= stayCityneedVipLevel)
                {
                    //如果神秘商人已经固定下来了，那出来的就是西域商人了，否则出来的肯定是神秘商人
                    shopType = ShopType.Western;
                    continue;
                }
                var sType = shopType;

                //存在该类型的商人 在城池中
                var shop = ShopItems.FirstOrDefault(o => o.ShopType == sType);
                if (shop != null && shop.LeaveTime.ToTs() > 0)
                {
                    //已经存在这个类型的商人则直接跳过
                    break;
                }

                var userCityItem = CityItems.FirstOrDefault(o => o.CityId == i);
                if (userCityItem == null)
                {
                    //还未开启固定城池 还没资格出现该商人
                    break;
                }

                var probability = shopCfgData.MysteriousProbability;
                if (shopType == ShopType.Western) probability = shopCfgData.WesternProbability;
                if (Util.IsHit(probability * 1.0 / 100))
                {
                    //命中
                    if (cityId == 0)
                    {
                        //如果打领地的话 就随机一个已开启的城池放商人
                        var r = Util.GetRandom(1, CityItems.Count);
                        cityId = CityItems[r].CityId;
                    }

                    //RefreshMysteriousOrWesternShop(shopType, cityId);
                    RefreshShop(shopType, cityId);
                    //已经出来一个商人则直接退出
                    return cityId;
                }

                shopType = ShopType.Western;
            }

            shopType = ShopType.None;
            cityId = 0;
            return cityId;
        }

        /// <summary>
        /// 刷新/初始化神秘/西域商人物品列表
        /// </summary>
        /// <param name="shopType"></param>
        /// <param name="cityId">-1 表示固定城池</param>
        /// <param name="useMoney"></param>
        public void RefreshShop(ShopType shopType, int cityId = 0, int useMoney = 0)
        {
            var shopCfgData = ConfigHelper.ShopCfgData;
            var shop = ShopItems.FirstOrDefault(o => o.ShopType == shopType);
            if (shop == null)
            {
                //不存在该商店 生成一个
                shop = new ShopItem();
                shop.ShopType = shopType;

                ShopItems.Add(shop);
            }

            if (shopType == ShopType.LuoYang) cityId = -1;

            if (cityId == -1)
            {
                shop.LeaveTime = DateTime.Now.AddYears(30);
                shop.CityId = shopCfgData.BussinessStayCityList[(int)shopType - 1];
                if (shop.CityId == 0)
                {
                    //非大地图商店
                    shop.LeaveTime = DateTime.Now.AddYears(30);
                }
            }
            if (cityId > 0) shop.CityId = cityId;
            //刷新
            if (useMoney == 0)
            {
                shop.LastRefreshTime = DateTime.Now;
                if (shop.LeaveTime < DateTime.Now.AddYears(1))
                    shop.LeaveTime = DateTime.Now.AddSeconds(shopCfgData.BussinessStayTime);
            }
            else
            {
                var index = (int)shopType - 1;
                TodayShopRefreshNumList[index] += 1;
                ShopRefreshNumList[index]++;
            }
            shop.GoodsItems = GetShopGoodItem(shopType, shopCfgData.TotalGoodsNum);
        }
    }

    /// <summary>
    /// 占领类型
    /// </summary>
    public enum OwnerType
    {
        /// <summary>
        /// 自己
        /// </summary>
        Own = 0,
        /// <summary>
        /// Npc
        /// </summary>
        Npc = 1,
        /// <summary>
        /// 玩家/其他玩家
        /// </summary>
        User = 2,
    }

    /// <summary>
    /// 资源项
    /// </summary>
    [ProtoContract]
    public class ResourceItem
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public MoneyType Type { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public int Num { get; set; }
    }

    /// <summary>
    /// 时间的类型
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// 城池失守/防守成功
        /// </summary>
        City = 0,
        /// <summary>
        /// 主城被攻打
        /// </summary>
        Main = 1,
        /// <summary>
        /// 领地失守【被匹配为NPC/玩家】
        /// </summary>
        Domain = 2,
    }

    /// <summary>
    /// 大地图事件项
    /// </summary>
    [ProtoContract]
    public class EventItem : KVEntity
    {
        public EventItem()
        {
            CreateTime = DateTime.Now;
            IsCanPalyback = 1;
            ItemPairList = new List<ItemPair>();
            SysConcubineIdList = new List<int>();
        }
        /// <summary>
        /// 事件类型
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public EventType Type { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public string Msg { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        [ProtoMember(3)]
        [Tag(3)]
        public int IsRead { get; set; }
        /// <summary>
        /// 是否胜利【指的是攻击方！】
        /// </summary>
        [ProtoMember(4)]
        [Tag(4)]
        public int IsWin { get; set; }
        /// <summary>
        /// 事件发生时间
        /// </summary>
        [ProtoMember(5)]
        [Tag(5)]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 关联的战斗ID
        /// </summary>
        [ProtoMember(6)]
        [Tag(6)]
        public int BattleId { get; set; }
        /// <summary>
        /// 是否已请求下发过
        /// </summary>
        [ProtoMember(7)]
        public int IsReq { get; set; }
        /// <summary>
        /// 战报是否可以回放
        /// </summary>
        [Tag(7)]
        public int IsCanPalyback { get; set; }
        /// <summary>
        /// 资源损失列表
        /// </summary>
        [ProtoMember(8)]
        [Tag(8)]
        public List<ItemPair> ItemPairList { get; set; }
        /// <summary>
        /// 被调戏的系统妃子ID列表
        /// </summary>
        [ProtoMember(9)]
        [Tag(9)]
        public List<int> SysConcubineIdList { get; set; }
        /// <summary>
        /// 战斗除自己外的用户昵称
        /// </summary>
        [Tag(10)]
        public string TargetName { get; set; }
        /// <summary>
        /// 是否已经复仇【0还未复仇 1已复仇】
        /// </summary>
        [Tag(11)]
        public int HasRevenge { get; set; }
        /// <summary>
        /// 事件关联的系统城池ID【为零表示是宛城】
        /// </summary>
        [ProtoMember(10)]
        [Tag(12)]
        public int CityId { get; set; }
        /// <summary>
        /// 防守成功获得的随机元宝（1-5）大于零则显示即可
        /// </summary>
        [ProtoMember(11)]
        [Tag(13)]
        public int Money { get; set; }

        /// <summary>
        /// 添加损失的资源
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="num"></param>
        public void AddItemPair(int itemId, int num)
        {
            ItemPairList = ItemPairList ?? new List<ItemPair>();
            var itemPair = ItemPairList.FirstOrDefault(o => o.ItemId == itemId);
            if (itemPair == null)
            {
                itemPair = new ItemPair();
                itemPair.ItemId = itemId;
                itemPair.Num = num;
                ItemPairList.Add(itemPair);
            }
            else
            {
                itemPair.Num += num;
            }
        }

        /// <summary>
        /// 添加被调戏的妃子
        /// </summary>
        /// <param name="sysConcubineId"></param>
        public void AddSysConcubineId(int sysConcubineId)
        {
            SysConcubineIdList = SysConcubineIdList ?? new List<int>();
            SysConcubineIdList.Add(sysConcubineId);
        }
    }

    /// <summary>
    /// 大地图寻访事件项
    /// </summary>
    [ProtoContract]
    public class SearchItem : KVEntity
    {
        public SearchItem()
        {
            CreateTime = DateTime.Now;
        }
        /// <summary>
        /// 消息
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public string Msg { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        [ProtoMember(3)]
        [Tag(3)]
        public int IsRead { get; set; }
        /// <summary>
        /// 寻访的城池ID
        /// </summary>
        [ProtoMember(4)]
        [Tag(4)]
        public int SearchCityId { get; set; }
        /// <summary>
        /// 事件发生时间
        /// </summary>
        [ProtoMember(5)]
        [Tag(5)]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 寻访的武将ID
        /// </summary>
        [ProtoMember(6)]
        [Tag(6)]
        public int SearchHeroId { get; set; }
        /// <summary>
        /// 是否已请求下发过
        /// </summary>
        [ProtoMember(7)]
        public int IsReq { get; set; }
        /// <summary>
        /// 寻访是否成功
        /// </summary>
        [ProtoMember(8)]
        [Tag(8)]
        public int IsWin { get; set; }
    }

    /// <summary>
    /// 到访的武将/妃子信息
    /// </summary>
    [ProtoContract]
    public class VisitorItem
    {
        /// <summary>
        /// 到访的武将/妃子ID
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public int VisitorId { get; set; }
        /// <summary>
        /// 剩余拜访的次数
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public int VisitTimes { get; set; }
        /// <summary>
        /// 离开的时间
        /// </summary>
        [ProtoMember(3)]
        [Tag(3)]
        public DateTime LeaveTimes { get; set; }
        /// <summary>
        /// 是否可以拜访(1:可以拜访，0：已拜访)
        /// </summary>
        [Tag(4)]
        public int CanVisitor { get; set; }
    }

    /// <summary>
    /// 全局活动的到访的武将/妃子信息
    /// </summary>
    [ProtoContract]
    public class ActivityVisitorTtem
    {
        /// <summary>
        /// 到访的武将/妃子ID
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public int VisitorId { get; set; }
        /// <summary>
        /// 剩余拜访的次数
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public int VisitTimes { get; set; }
        /// <summary>
        /// 离开的时间
        /// </summary>
        [ProtoMember(3)]
        [Tag(3)]
        public DateTime LeaveTimes { get; set; }
        /// <summary>
        /// 已拜访过的ID列表
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> VisitedIdList { get; set; }
    }

    /// <summary>
    /// 商店项
    /// </summary>
    [ProtoContract]
    public class ShopItem
    {
        public ShopItem()
        {
            GoodsItems = new List<ShopGoodTtem>();
        }
        /// <summary>
        /// 商店类型
        /// </summary>
        [ProtoMember(1)]
        public ShopType ShopType { get; set; }
        /// <summary>
        /// 上次刷新时间
        /// </summary>
        [ProtoMember(2)]
        public DateTime LastRefreshTime { get; set; }
        /// <summary>
        /// 商店里面的物品列表
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ShopGoodTtem> GoodsItems { get; set; }
        /// <summary>
        /// 所在的城池ID
        /// </summary>
        [ProtoMember(4)]
        public int CityId { get; set; }
        /// <summary>
        /// 离开时间
        /// </summary>
        [ProtoMember(5)]
        public DateTime LeaveTime { get; set; }

        /// <summary>
        /// 获得商人下次刷新的描述：今日9点
        /// </summary>
        /// <returns></returns>
        public string GetRefreshRefreshDescription()
        {
            var shopCfgData = ConfigHelper.ShopCfgData;
            if (LeaveTime.ToTs() <= shopCfgData.BussinessStayTime) return "";

            var refreshTimeList = new List<DateTime>();
            foreach (var i in shopCfgData.LuoYangRefreshTimeList)
            {
                refreshTimeList.Add(DateTime.Today.AddHours(i));
            }
            var index = 0;
            foreach (var dateTime in refreshTimeList)
            {
                if (dateTime > LastRefreshTime) break;
                index++;
            }
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                    return LangResource.GetLangResource(ResourceId.R_0000_Today,
                        shopCfgData.LuoYangRefreshTimeList[index]);
            }
            return LangResource.GetLangResource(ResourceId.R_0000_Tomorrow,
                shopCfgData.LuoYangRefreshTimeList[0]);
        }
    }

    /// <summary>
    /// 商店里面的物品列表项
    /// </summary>
    [ProtoContract]
    public class ShopGoodTtem
    {
        /// <summary>
        /// id（唯一标识符）
        /// </summary>
        [ProtoMember(1)]
        public int Id { get; set; }
        /// <summary>
        /// 是否已经购买过
        /// </summary>
        [ProtoMember(5)]
        public int Buyed { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [ProtoMember(7)]
        public int SNum { get; set; }
    }


    public class ResShopGoodTtem
    {
        /// <summary>
        /// id（唯一标识符）
        /// </summary>
        [Tag(1)]
        public int Id { get; set; }
        /// <summary>
        /// 物品类型
        /// </summary>
        [Tag(2)]
        public ExtractItemType ItemType { get; set; }
        /// <summary>
        /// 物品的id
        /// </summary>
        [Tag(3)]
        public int ItemId { get; set; }
        /// <summary>
        /// 物品数量
        /// </summary>
        [Tag(4)]
        public int Num { get; set; }
        /// <summary>
        /// 购买所需的货币类型
        /// </summary>
        [Tag(5)]
        public MoneyType MoneyType { get; set; }
        /// <summary>
        /// 总共价格
        /// </summary>
        [Tag(6)]
        public int TotalPrice { get; set; }
        /// <summary>
        /// 是否已经购买过
        /// </summary>
        [Tag(7)]
        public int Buyed { get; set; }
    }
}
