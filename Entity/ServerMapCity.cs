// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。
// 文 件 名：UserBag.cs
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
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 全局城池信息项
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class ServerMapCityItem : KVEntity
    {
        /// <summary>
        /// 占领城池的玩家ID
        /// </summary>
        [ProtoMember(1)]
        public int OwnerId { get; set; }
        /// <summary>
        /// 系统CityId
        /// </summary>
        [ProtoMember(2)]
        public int CityId { get; set; }
        /// <summary>
        /// 城池等级【会根据战斗次数不断递增】(废除)
        /// 意思改为城池玩家或者NPC的等级，字段名就不改了。。
        /// </summary>
        [ProtoMember(3)]
        public int CityLevel { get; set; }
        /// <summary>
        /// 太守【先不做，扩展为占领者指派武将守城使用】
        /// </summary>
        [ProtoMember(4)]
        public int Guard { get; set; }
        /// <summary>
        /// 防守阵容
        /// </summary>
        [ProtoMember(5), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<DefendItem> DefendItems { get; set; }
        /// <summary>
        /// 被匹配的玩家数量
        /// </summary>
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> MatchIdList { get; set; }

        /// <summary>
        /// 占领类型
        /// </summary>
        [ProtoMember(7)]
        public OwnerType OwnerType { get; set; }

        /// <summary>
        /// 城防【大于50 每5点提升2%的城防 小于50 每5点降低3%的城防】
        /// </summary>
        [ProtoMember(8)]
        public int Defense { get; set; }

        /// <summary>
        /// 军备【大于50 每5点提升2%的带兵 小于50 每5点降低3%的带兵】
        /// </summary>
        [ProtoMember(9)]
        public int Army { get; set; }

        /// <summary>
        /// 士气 5点提升10点怒气
        /// </summary>
        [ProtoMember(10), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayDescendingValue Morale { get; set; }

        /// <summary>
        /// 已经进行的内政次数
        /// </summary>
        //[ProtoMember(11), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        //public DayZeorValue UseNum { get; set; }

        /// <summary>
        /// 占领时间或者最后一次收获的时间【用于收获时计算具体的收获量】
        /// </summary>
        [ProtoMember(12)]
        public DateTime OccupiedOrGainTime { get; set; }

        public override void NewObjectInit()
        {
            MatchIdList = new List<int>();
            DefendItems = new List<DefendItem>();
            OwnerType = OwnerType.Npc;
            Defense = Army = 50;
            Morale = new DayDescendingValue(ConfigHelper.BigMapDefenseCfgData.DescendingZhiAn, 50, 0, 100);

            OccupiedOrGainTime = DateTime.Now;
        }

        public override void LoadInit()
        {

            MatchIdList = MatchIdList ?? new List<int>();
            DefendItems = DefendItems ?? new List<DefendItem>();

            if (OccupiedOrGainTime.AddYears(1) < DateTime.Now)
                OccupiedOrGainTime = DateTime.Now;

            if(OwnerType == OwnerType.User && !MatchIdList.Contains(OwnerId))
                MatchIdList.Add(OwnerId);

            //if (OwnerId == 0 && CityId == 0 && DefendItems.Count == 0)
            //{
            //    InitDefendFromNpc();
            //}

            //初始化ID，之前没这个字段
            if (DefendItems.Exists(o => o.Id == 0))
            {
                var index = 1;
                foreach (var defendItem in DefendItems)
                {
                    defendItem.Id = index;
                    index++;
                }
            }

            if (OwnerType == OwnerType.Npc)
            {
                DefendItems = new List<DefendItem>();
                //var isOldData = false;
                //foreach (var defendItem in DefendItems)
                //{
                //    var cfg =
                //        SysBigMapHeroCfg.Items.FirstOrDefault(
                //            o =>
                //                o.NpcId == OwnerId && o.HeroId == defendItem.HeroId &&
                //                o.Location == defendItem.Location);
                //    if (cfg == null || cfg.Id != defendItem.Id)
                //    {
                //        isOldData = true;
                //        break;
                //    }
                //}
                //if (isOldData)
                //{
                //    DefendItems = new List<DefendItem>();
                //    var turnList = new List<int>() { 1, 2, 3 };
                //    //var npcHeroList = SysNpcFormationCfg.Items.Where(o => o.NpcId == npcId).ToList();
                //    var npcHeroList = SysBigMapHeroCfg.Items.Where(o => o.NpcId == OwnerId).ToList();
                //    foreach (var turn in turnList)
                //    {
                //        if (turn <= 0) break;

                //        var tempList = npcHeroList.Where(o => o.Turn == turn).ToList();
                //        if (tempList.Count == 0) break;
                //        //NPC防守阵型
                //        foreach (var sysBigMapNpcHeroCfg in tempList)
                //        {
                //            sysBigMapNpcHeroCfg.Init();
                //            var item = new DefendItem();
                //            item.Turn = turn;
                //            item.Location = sysBigMapNpcHeroCfg.Location;
                //            item.HeroId = sysBigMapNpcHeroCfg.HeroId;
                //            item.Level = sysBigMapNpcHeroCfg.Level;
                //            item.Id = sysBigMapNpcHeroCfg.Id;

                //            DefendItems.Add(item);
                //        }
                //    }
                //}
            }

            if (Defense == 0) Defense = 50;
            if (Army == 0) Army = 50;
            var bigMapDefenseCfgData = ConfigHelper.BigMapDefenseCfgData;
            var descending = Morale.Descending;
            if (descending.Equals(0.0) || !descending.Equals(bigMapDefenseCfgData.DescendingZhiAn))
            {
                var value = Morale.Value;
                Morale = new DayDescendingValue(bigMapDefenseCfgData.DescendingZhiAn, value, 0, 100);
            }
        }
    }

    /// <summary>
    /// 防守阵容项
    /// </summary>
    [ProtoContract]
    public class DefendItem
    {
        /// <summary>
        /// 第几阵容
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public int Turn { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public LocationNumber Location { get; set; }
        /// <summary>
        /// 英雄ID
        /// </summary>
        [ProtoMember(3)]
        [Tag(3)]
        public int HeroId { get; set; }
        /// <summary>
        /// 英雄等级
        /// </summary>
        [ProtoMember(4)]
        [Tag(4)]
        public int Level { get; set; }
        /// <summary>
        /// 武将列表中的唯一标识符
        /// </summary>
        [ProtoMember(5)]
        [Tag(5)]
        public int Id { get; set; }

        /// <summary>
        /// 获取系统NPC武将
        /// </summary>
        /// <returns></returns>
        public SysBigMapHeroCfg GetSysBigMapNpcHeroCfg()
        {
            var cfg = SysBigMapHeroCfg.Items.FirstOrDefault(o => o.Id == Id);
            if (cfg == null)
                throw new ApplicationException(string.Format("GetSysLevelNpcHeroCfg找不到数据:Id[{0}]", Id));
            return cfg;
        }

        ///// <summary>
        ///// 获取系统NPC武将
        ///// </summary>
        ///// <returns></returns>
        //public SysNpcHeroCfg GetNextSysNpcHeroCfg()
        //{
        //    var cfg = SysNpcHeroCfg.Items.FirstOrDefault(o => o.HeroId == HeroId && o.Level == Level + 1);
        //    return cfg;
        //}

        ///// <summary>
        ///// 获取武将下一等级增加的战斗力
        ///// </summary>
        ///// <returns></returns>
        //public int GetNextLevelAddCombat()
        //{
        //    var nextSysNpcHeroCfg = GetNextSysNpcHeroCfg();
        //    if (nextSysNpcHeroCfg == null) return 0;
        //    var curSysNpcHeroCfg = GetSysNpcHeroCfg();

        //    return nextSysNpcHeroCfg.Combat - curSysNpcHeroCfg.Combat;
        //}
    }

    /// <summary>
    /// 大地图的战力
    /// </summary>
    public class DefendItem2 : DefendItem
    {
        /// <summary>
        /// 战力
        /// </summary>
        [Tag(10)]
        public int Combat { get; set; }
        /// <summary>
        /// 星级
        /// </summary>
        [Tag(11)]
        public int Star { get; set; }
    }


    /// <summary>
    /// 城池全局数据【城池CityId生成的全局数据】
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class ServerMapCity : KVEntity
    {
        /// <summary>
        /// 城池实例【不能直接被访问，这里写成public是因为private属性没法同步到数据库。】
        /// </summary>
        //[ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        //public List<ServerMapCityItem> Items { get; set; }

        /// <summary>
        /// 当前数字
        /// </summary>
        [ProtoMember(2)]
        public int CurNum { get; set; }

        /// <summary>
        /// 城池实例ID列表【不能直接被访问，这里写成public是因为private属性没法同步到数据库。】
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> ItemIdList { get; set; }

        /// <summary>
        /// 获取全局城池信息项ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cityId"></param>
        /// <param name="isCreatNew"></param>
        /// <param name="serverMapCityItemId">排除原来的组id</param>
        /// <returns></returns>
        public ServerMapCityItem GetServerMapCityItemId(int userId, int cityId, bool isCreatNew = false, int serverMapCityItemId = 0)
        {
            ItemIdList = ItemIdList ?? new List<int>();
            //Items = Items ?? new List<ServerMapCityItem>();
            var items = DataStorage.Current.LoadList<ServerMapCityItem>(ItemIdList.ToArray(), true);
            var item = items.FirstOrDefault(o => o.Id == serverMapCityItemId);
            var curUserId = item == null ? 0 : item.OwnerId;
            if (isCreatNew || items.Count == 0)
            {
                return CreateNewServerCity(userId, cityId);
            }
            else if (isCreatNew || items.Count(o => o.MatchIdList.Count < ConfigHelper.BigMapCfgData.CityMaxMatchCount) <= 0)
            {
                return CreateNewServerCity(userId, cityId);
            }
            else
            {
                var userRole = DataStorage.Current.Load<UserRole>(userId);
                var level = userRole.Level;

                //大地图匹配城池：查找该组的城池，所占领玩家等级5级以内的，如果找不到则10级以内，以此类推。
                var index = 0;
                var miniLevel = (level - index * 5) < 0 ? 0 : level - index * 5;
                var maxLevel = level + index * 5;

                var tempList =
                    items.Where(
                        o =>
                            o.MatchIdList.Count < ConfigHelper.BigMapCfgData.CityMaxMatchCount &&
                            !o.MatchIdList.Contains(userId) &&
                            !o.MatchIdList.Contains(curUserId) &&
                            o.Id != serverMapCityItemId &&
                            o.CityLevel > miniLevel &&
                            o.CityLevel < maxLevel).ToList();
                while (index < 4 && tempList.Count == 0)
                {
                    index++;
                    miniLevel = (level - index * 5) < 0 ? 0 : level - index * 5;
                    maxLevel = level + index * 5;

                    tempList =
                    items.Where(
                        o =>
                            o.MatchIdList.Count < ConfigHelper.BigMapCfgData.CityMaxMatchCount &&
                            !o.MatchIdList.Contains(userId) &&
                            !o.MatchIdList.Contains(curUserId) &&
                            o.Id != serverMapCityItemId &&
                            o.CityLevel > miniLevel &&
                            o.CityLevel < maxLevel).ToList();
                }

                if (serverMapCityItemId > 0)
                {
                    if (item != null)
                    {
                        item.MatchIdList.Remove(userId);
                    }
                }
                if (tempList.Count > 0)
                {
                    var r = Util.GetRandom(0, tempList.Count);
                    var temp = tempList[r];
                    temp.MatchIdList.Remove(userId);
                    temp.MatchIdList.Add(userId);
                    return temp;
                }
                else
                {
                    return CreateNewServerCity(userId, cityId);
                }
            }
        }

        /// <summary>
        /// 生成新的全服城池项
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public ServerMapCityItem CreateNewServerCity(int userId, int cityId)
        {
            //var cityId = Utility.GetCityId(Id);
            var sysCityCfg = SysCityCfg.Find(cityId);
            if (sysCityCfg == null) throw new ApplicationException(string.Format("SysCityCfg:Id:{0} NOT FIND", cityId));

            var serverMapCityItemId = Util.GetSequence(typeof(ServerMapCityItem));
            var serverMapCityItem = Storage.Load<ServerMapCityItem>(serverMapCityItemId, true);
            serverMapCityItem.CityId = cityId;
            serverMapCityItem.CityLevel = sysCityCfg.Level;
            serverMapCityItem.OwnerType = OwnerType.Npc;
            var npcId = sysCityCfg.NpcId;
            if (npcId == 0)
            {
                var sysCityCfgList = SysCityCfg.Items.Where(o => o.TeamId == sysCityCfg.TeamId);
                foreach (var cityCfg in sysCityCfgList)
                {
                    if (cityCfg.NpcId > 0)
                    {
                        npcId = cityCfg.NpcId;
                        break;
                    }
                }
            }
            serverMapCityItem.OwnerId = npcId;
            //if (sysCityCfg.IsNpc == 1)
            //{
            //    serverMapCityItem.OwnerType = OwnerType.Npc;
            //    serverMapCityItem.OwnerId = sysCityCfg.NpcId;
            //}
            //else
            //{
            //    serverMapCityItem.OwnerType = OwnerType.User;
            //    serverMapCityItem.OwnerId = userId;//最新生成的全服城池占领者为第一个玩家
            //}
            serverMapCityItem.MatchIdList.Add(userId);

            serverMapCityItem.DefendItems = new List<DefendItem>();
            var turnList = new List<int>() { 1, 2, 3 };
            //var npcHeroList = SysNpcFormationCfg.Items.Where(o => o.NpcId == npcId).ToList();
            var npcHeroList = SysBigMapHeroCfg.Items.Where(o => o.NpcId == npcId).ToList();
            foreach (var turn in turnList)
            {
                if (turn <= 0) break;

                var tempList = npcHeroList.Where(o => o.Turn == turn).ToList();
                if (tempList.Count == 0) break;
                //NPC防守阵型
                foreach (var sysBigMapHeroCfg in tempList)
                {
                    sysBigMapHeroCfg.Init();
                    var item = new DefendItem();
                    item.Turn = turn;
                    item.Location = sysBigMapHeroCfg.Location;
                    item.HeroId = sysBigMapHeroCfg.HeroId;
                    item.Level = sysBigMapHeroCfg.Level;
                    item.Id = sysBigMapHeroCfg.Id;

                    serverMapCityItem.DefendItems.Add(item);
                }
            }
            //新建添加到列表
            //Items.Add(serverMapCityItem);
            ItemIdList.Add(serverMapCityItemId);

            return serverMapCityItem;
        }

        public override void NewObjectInit()
        {
            //Items = new List<ServerMapCityItem>();

            ItemIdList = new List<int>();
            CurNum = ConfigHelper.ClearBigMapNum;
        }

        public override void LoadInit()
        {
            ItemIdList = ItemIdList ?? new List<int>();
            //Items = Items ?? new List<ServerMapCityItem>();

            if (ConfigHelper.ClearBigMapNum > CurNum)
            {
                //Items.Clear();
                ItemIdList.Clear();

                CurNum = ConfigHelper.ClearBigMapNum;
            }

        }
    }

    /// <summary>
    /// 内政类型
    /// </summary>
    public enum InternalAffairsType
    {
        /// <summary>
        /// 城防【守城部队护甲和法抗】
        /// </summary>
        Defense = 1,
        /// <summary>
        /// 军备【守城部队带兵数】
        /// </summary>
        Army = 2,
        /// <summary>
        /// 士气【守城部队带兵数】
        /// </summary>
        Morale = 3,
    }
}
