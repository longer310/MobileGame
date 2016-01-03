using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Repository;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 建筑实例
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserBuildingItem : KVEntity
    {
        /// <summary>
        /// 系统建筑ID
        /// </summary>
        [ProtoMember(1)]
        public int BuildingId { get; set; }

        /// <summary>
        /// 建筑等级
        /// </summary>
        [ProtoMember(2)]
        public int Level { get; set; }

        /// <summary>
        /// 存储量
        /// </summary>
        [ProtoMember(3)]
        public int Storage { get; set; }

        /// <summary>
        /// 妃子id列表
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> ConcubineIdList { get; set; }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity { get { return SysBuildingCfg.Capacity; } }

        /// <summary>
        /// 生产的资源类型
        /// </summary>
        public MoneyType MoneyType { get { return SysBuildingCfg.MoneyType; } }

        /// <summary>
        /// 用户id
        /// </summary>
        [ProtoMember(5)]
        public int Pid { get; set; }

        /// <summary>
        /// 建筑配置文件
        /// </summary>
        public SysBuildingCfg SysBuildingCfg
        {
            get
            {
                var sysBuildingCfg =
                    SysBuildingCfg.Items.FirstOrDefault(o => o.BuildingId == BuildingId && o.Level == Level);
                if (sysBuildingCfg == null) throw new ApplicationException(string.Format("建筑[BuildingId={0},Level={1}]不存在", BuildingId, Level));
                return sysBuildingCfg;
            }
        }

        public override void NewObjectInit()
        {
            ConcubineIdList = new List<int>();
        }

        public override void LoadInit()
        {
            ConcubineIdList = ConcubineIdList ?? new List<int>();
        }

        
        /// <summary>
        /// 获取建筑抢劫的数量
        /// </summary>
        /// <param name="robBuildingPrec"></param>
        /// <param name="opCode"></param>
        /// <param name="subtract">是否直接扣除被抢方资源然后添加给抢劫方</param>
        /// <returns></returns>
        public int GetBuildingRobNum(int robBuildingPrec, int opCode, int subtract = 0)
        {
            //抢建筑里头的资源
            var canRobRes = (int)(Storage * 1.0 * robBuildingPrec / 100);
            if (canRobRes < 0) canRobRes = 0;
            if (canRobRes == 0 && Storage > 0) canRobRes = 1;

            if (subtract == 1 && canRobRes > 0)
            {
                //扣除玩家身上的资源，并且不立即分配建筑存储量
                var index = (int)MoneyType - 2;
                Utility.ConsumeResource(null, (ItemType)(index + 2), opCode, canRobRes, Pid, 0);
            }
            return canRobRes;
        }

        /// <summary>
        /// 获取建筑中的妃子抢劫的数量
        /// </summary>
        /// <returns></returns>
        public int GetConcubinesRobNum(int robConcubinePrec, UserConcubine userConcubine = null, int subtract = 0)
        {
            //var bigMapCfgData = ConfigHelper.BigMapCfgData;
            //var robConcubinePrec = bigMapCfgData.RobConcubinePrec;
            ConcubineIdList = ConcubineIdList ?? new List<int>();
            var canRobRes = 0;
            //抢妃子身上未收获的资源
            if (ConcubineIdList.Count > 0)
            {
                if (userConcubine == null)
                {
                    userConcubine = DataStorage.Current.Load<UserConcubine>(Pid);
                }
                var concubineList =
                    userConcubine.Items.Where(o => ConcubineIdList.Contains(o.Id)).ToList();
                foreach (var userConcubineItem in concubineList)
                {
                    //先把产量计算出来
                    userConcubineItem.GainToOutput((int)OpcodeType.InvestigateRequest);
                    canRobRes = (int)(userConcubineItem.OutPut * 1.0 * robConcubinePrec / 100);
                    if (canRobRes > userConcubineItem.OutPut) canRobRes = userConcubineItem.OutPut;
                    if (canRobRes < 0) canRobRes = 0;
                    if (canRobRes == 0 && userConcubineItem.OutPut > 0) canRobRes = 1;

                    if (canRobRes > 0 && subtract == 1)
                    {
                        //扣除妃子身上的资源
                        userConcubineItem.OutPut -= canRobRes;
                    }
                }
            }
            return canRobRes;
        }
    }


    /// <summary>
    /// 用户建筑
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserBuilding : KVEntity
    {
        /// <summary>
        /// 建筑实例——对内
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserBuildingItem> Items { get; set; }

        /// <summary>
        /// 升级CD时间
        /// </summary>
        [ProtoMember(2)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 重新匹配的数字
        /// </summary>
        [ProtoMember(3)]
        public int CurNum { get; set; }

        /// <summary>
        /// 升级的建筑id
        /// </summary>
        [ProtoMember(4)]
        public int UpgradeBuildingId { get; set; }

        /// <summary>
        /// 检测宫殿是否存在配置等
        /// </summary>
        [ProtoMember(5)]
        public int CheckNum { get; set; }

        public override void NewObjectInit()
        {
            Items = new List<UserBuildingItem>();
            EndTime = DateTime.Now;

            CurNum = ConfigHelper.ClearBuildingNum;
            CheckNum = ConfigHelper.CheckBuildingNum;

            //初始凰宫
            InitBuilding();
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<UserBuildingItem>();
            if (EndTime < new DateTime(1970, 1, 1))
                EndTime = DateTime.Now;

            var clearBuildingNum = ConfigHelper.ClearBuildingNum;
            var clearUserIdList = ConfigHelper.ClearUserIdList;
            if (clearBuildingNum > CurNum && (clearUserIdList.Contains("," + Id + ",") || clearUserIdList == "*"))
            {
                Items.Clear();
                EndTime = DateTime.Now;

                var userConcubine = Storage.Load<UserConcubine>(Id, true);
                foreach (var userConcubineItem in userConcubine.Items)
                {
                    userConcubineItem.Status = ConcubineStatus.Idle;
                    userConcubineItem.OutPut = 0;
                }

                RefreshCapacity(MoneyType.Coin);
                RefreshCapacity(MoneyType.Wood);
                RefreshCapacity(MoneyType.Stone);
                RefreshCapacity(MoneyType.Iron);

                CurNum = clearBuildingNum;
            }

            var checkBuildingNum = ConfigHelper.CheckBuildingNum;
            if (CheckNum < checkBuildingNum && Items.Count > 1)
            {
                var userConcubine = DataStorage.Current.Load<UserConcubine>(Id, true);
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    var userBuildingItem = Items[i];
                    var sysBuildingCfg =
                        SysBuildingCfg.Items.FirstOrDefault(o => o.BuildingId == userBuildingItem.BuildingId);
                    if (sysBuildingCfg == null)
                    {
                        userBuildingItem.ConcubineIdList = userBuildingItem.ConcubineIdList ?? new List<int>();
                        foreach (var concubineId in userBuildingItem.ConcubineIdList)
                        {
                            var userConcubineItem = userConcubine.Items.FirstOrDefault(o => o.ConcubineId == concubineId);
                            if (userConcubineItem != null)
                            {
                                userConcubineItem.Status = ConcubineStatus.Idle;//设置妃子空闲
                                userConcubineItem.GainToUser(0);//收获资源
                            }
                        }

                        Items.Remove(userBuildingItem);
                    }
                }

                CheckNum = checkBuildingNum;
            }

            //初始凰宫
            InitBuilding();

            foreach (var userBuildingItem in Items)
            {
                if (userBuildingItem.Pid == 0) userBuildingItem.Pid = Id;
                userBuildingItem.LoadInit();
            }
        }

        public void InitBuilding()
        {
            //modify by hql at 2015.10.10 4个宫殿都在一进后宫就建好了，不需要在建造。
            //其实应该判断是1级开启的就直接建造好即可
            var initBuildIdList = ConfigHelper.InitBuildIdList;
            foreach (var initBuildId in initBuildIdList)
            {
                var item = Items.FirstOrDefault(o => o.BuildingId == initBuildId);
                if (item == null)
                {
                    AddBuildingToUser(initBuildId, 0);
                }
            }
        }

        /// <summary>
        /// 重新分配资源
        /// </summary>
        /// <param name="moneyType"></param>
        /// <param name="value"></param>
        public void ReassignResource(MoneyType moneyType, int value)
        {
            //把资源分配到该资源类型的建筑中
            var buildingList = Items.Where(o => o.MoneyType == moneyType).ToList();
            foreach (var userBuildingItem in buildingList)
            {
                //全部建筑清空
                userBuildingItem.Storage = 0;
            }
            var buildingCount = buildingList.Count;
            var maxTimes = buildingCount;
            while (buildingCount > 0 && value > 0)
            {
                if (buildingCount == 1)
                {
                    //一个该类型建筑 一一对应。
                    buildingList[0].Storage = value;
                    if (buildingList[0].Storage > buildingList[0].Capacity)
                        buildingList[0].Storage = buildingList[0].Capacity;
                }
                else
                {
                    //两个及以上该类型建筑
                    double dAverage = value * 1.0 / buildingCount;
                    int iAverage = (int)dAverage;
                    int remainder = value - (int)dAverage * buildingCount;
                    if (iAverage == 0 && remainder > 0)
                    {
                        //分配数字低于建筑数 这是到最后分配才会出现的数字
                        buildingList = buildingList.OrderBy(o => o.Storage).Take(remainder).ToList();
                        iAverage = 1;
                        foreach (var userBuildingItem in buildingList)
                        {
                            userBuildingItem.Storage += iAverage;
                            value -= iAverage;
                        }
                    }
                    else if (iAverage > 0)
                    {
                        foreach (var userBuildingItem in buildingList)
                        {
                            if (userBuildingItem.Capacity >= (iAverage + userBuildingItem.Storage))
                            {
                                userBuildingItem.Storage += iAverage;
                                value -= iAverage;
                            }
                            else
                            {
                                //容量不够装载平均数
                                userBuildingItem.Storage = userBuildingItem.Capacity;
                                value -= userBuildingItem.Capacity;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                //最多循环的次数
                maxTimes--;
                if (maxTimes == 0) break;

                //轮询执行，直到数据都分配出去或者建筑都满
                buildingList = buildingList.Where(o => o.Capacity > o.Storage).ToList();
                buildingCount = buildingList.Count;
            }
        }

        /// <summary>
        /// 获取影响到的宫殿列表
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="moneyType"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public List<BuildBuilResponse.ResSimpleBuildingItem> GetAffectedBuildingList(UserRole userRole,
            MoneyType moneyType, int buildingId = 0)
        {
            var result = new List<BuildBuilResponse.ResSimpleBuildingItem>();
            var resValue = Utility.GetResourceValue(userRole, moneyType);
            ReassignResource(moneyType, resValue);

            var affectedBuildingItems =
                Items.Where(o => o.MoneyType == moneyType && o.BuildingId != buildingId)
                    .ToList();
            foreach (var affectedBuildingItem in affectedBuildingItems)
            {
                result.Add(new BuildBuilResponse.ResSimpleBuildingItem()

                {
                    BuildingId = affectedBuildingItem.BuildingId,
                    Storage = affectedBuildingItem.Storage,
                });
            }
            return result;
        }

        /// <summary>
        /// 刷新存储上限【建筑、升级】
        /// </summary>
        /// <param name="moneyType"></param>
        public void RefreshCapacity(MoneyType moneyType)
        {
            var value = 0;
            var userRole = Storage.Load<UserRole>(Id, true);
            var buildingList = Items.Where(o => o.MoneyType == moneyType);
            foreach (var userBuildingItem in buildingList)
            {
                var sysBuildingCfg =
                    SysBuildingCfg.Items.FirstOrDefault(
                        o => o.BuildingId == userBuildingItem.BuildingId && o.Level == userBuildingItem.Level);
                if (sysBuildingCfg != null)
                {
                    value += sysBuildingCfg.Capacity;
                }
            }

            switch (moneyType)
            {
                case MoneyType.Coin:
                    value += ConfigHelper.BuildingCfgData.InitCoinCapacity;
                    userRole.MaxCoin = value; break;
                case MoneyType.Wood:
                    value += ConfigHelper.BuildingCfgData.InitWoodCapacity;
                    userRole.MaxWood = value; break;
                case MoneyType.Stone:
                    value += ConfigHelper.BuildingCfgData.InitStoneCapacity;
                    userRole.MaxStone = value; break;
                case MoneyType.Iron:
                    value += ConfigHelper.BuildingCfgData.InitIronCapacity;
                    userRole.MaxIron = value; break;
                default: break;
            }
        }

        /// <summary>
        /// 添加建筑
        /// </summary>
        /// <param name="buildingId">建筑Id</param>
        /// <param name="opCode">接口id</param>
        /// <returns></returns>
        public UserBuildingItem AddBuildingToUser(int buildingId, int opCode)
        {
            var sysBuildingCfg = SysBuildingCfg.Items.FirstOrDefault(o => o.BuildingId == buildingId && o.Level == 1);
            if (sysBuildingCfg == null) throw new ApplicationException(string.Format("SysBuildingCfg:Id:{0} NOT FIND", buildingId));
            //添加新建筑
            var userBuildingItem = KVEntity.CreateNew<UserBuildingItem>();
            userBuildingItem.BuildingId = buildingId;
            userBuildingItem.Level = 1;
            userBuildingItem.Pid = Id;
            Items.Add(userBuildingItem);

            GameLogManager.ItemLog(Id, buildingId, 1, opCode, (int)ItemType.Building, 0, 0);

            RefreshCapacity(userBuildingItem.MoneyType);
            return userBuildingItem;
        }
    }
}
