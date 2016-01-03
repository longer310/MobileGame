using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.Core.ObjectMapper.MappingConfiguration.MappingOperations;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;

namespace MobileGame.tianzi.Repository
{
    #region 9003 妃子入住列表
    public class InBuildingConcubineListResponse
    {
        public InBuildingConcubineListResponse()
        {
            Items = new List<ResConcubineItem>();
        }
        /// <summary>
        /// 妃子列表
        /// </summary>
        [Tag(1)]
        public List<ResConcubineItem> Items { get; set; }
        public class ResConcubineItem
        {
            /// <summary>
            /// 妃子ID
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 妃子ID(系统)
            /// </summary>
            [Tag(2)]
            public int ConcubineId { get; set; }
            /// <summary>
            /// 生产速度（每小时）
            /// </summary>
            [Tag(3)]
            public int Product { get; set; }
            /// <summary>
            /// 特色增加生产速度（每小时）
            /// </summary>
            [Tag(4)]
            public int AddProduct { get; set; }
            /// <summary>
            /// 妃子封号等级
            /// </summary>
            [Tag(5)]
            public int TitleLevel { get; set; }
        }
    }
    /// <summary>
    /// 妃子入住列表
    /// </summary>
    [GameCode(OpCode = 9003, ResponseType = typeof(InBuildingConcubineListResponse))]
    public class InBuildingConcubineListRequest : GameHandler
    {
        /// <summary>
        /// 宫殿ID
        /// </summary>
        public int BuildingId { get; set; }
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            Storage.Load(out userBuilding, out userConcubine, CurrentUserId, true);
            var response = new InBuildingConcubineListResponse();

            var userBuildingItem = userBuilding.Items.FirstOrDefault(o => o.BuildingId == BuildingId);
            if (userBuildingItem == null)
            {
                //不存在宫殿
                SetError(ResourceId.R_0000_IdNotExist, "UserBuilding", BuildingId);
                return;
            }

            //妃子必须入住在同类型宫殿！modify by hql at 2015.9.20
            var userConcubineList =
                userConcubine.Items.Where(
                    o => o.MoneyType == userBuildingItem.MoneyType && o.Status == ConcubineStatus.Idle);
            var moneyType = userBuildingItem.MoneyType;
            //var userConcubineList = userConcubine.Items.Where(o => o.Status == ConcubineStatus.Idle);
            foreach (var userConcubineItem in userConcubineList)
            {
                response.Items.Add(new InBuildingConcubineListResponse.ResConcubineItem()
                {
                    Id = userConcubineItem.Id,
                    ConcubineId = userConcubineItem.ConcubineId,
                    Product = userConcubineItem.Product,
                    AddProduct = userConcubineItem.GetAddProduct(moneyType),
                    TitleLevel = userConcubineItem.TitleLevel,
                });
            }

            response.Items = response.Items.OrderByDescending(o => (o.Product + o.AddProduct)).ToList();

            ResultObj = response;
        }
    }
    #endregion

    #region 9008 翻牌
    public class GainFavorResponse
    {
        /// <summary>
        /// 翻牌随机到的ID对话及奖励
        /// </summary>
        [Tag(1)]
        public int Id { get; set; }
    }
    /// <summary>
    /// 翻牌
    /// </summary>
    [GameCode(OpCode = 9008, ResponseType = typeof(GainFavorResponse))]
    public class FlopRequest : GameHandler
    {
        /// <summary>
        /// 妃子ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            UserConcubine userConcubine;
            UserRole userRole;
            Storage.Load(out userConcubine, out userRole, CurrentUserId, true);

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }
            //var p = userConcubineItem.RealProduct;

            if (userConcubine.LaveFlopNum <= 0)
            {
                //翻牌次数不足
                SetError(ResourceId.R_9008_FlopNumNotEnough);
                return;
            }

            //var sysConcubineFavorCfg = SysConcubineFavorCfg.Items.FirstOrDefault(o => o.Id == userConcubineItem.Level + 1);
            //if (sysConcubineFavorCfg == null)
            //{
            //    //SetError(ResourceId.R_0000_IdNotExist, "SysConcubineFavorCfg:Id", userConcubineItem.Level + 1);
            //    SetError(ResourceId.R_9008_ConcubineLevelFull);
            //    return;
            //}

            //var maxFavor = sysConcubineFavorCfg.Favor;
            //if (userConcubineItem.Favor == maxFavor && userRole.Level <= userConcubineItem.Level)
            //{
            //    SetError(ResourceId.R_9008_ConcubineLevelCanNotHigherUser);
            //    return;
            //}

            var count = SysFlopCfg.Items.Count;
            if (count <= 0)
            {
                SetError(ResourceId.R_0000_OldDataError);
                return;
            }
            var r = Util.GetRandom(0, count);
            var sysFlopCfg = SysFlopCfg.Items[r];
            //减少翻牌次数 增加好感度 魅力
            var gainFavor = sysFlopCfg.Favor;//ConfigHelper.BuildingCfgData.FlopFavor;
            var gainCharm = sysFlopCfg.Charm;
            userConcubine.Flop();

            Utility.AddResource(userRole, ItemType.Charm, Request.OpCode, gainCharm);
            userConcubineItem.Favor += gainFavor;
            //Utility.AddResource(userRole, ItemType.Favor, Request.OpCode, gainFavor);
            //userConcubineItem.AddFavor(gainFavor, Request.OpCode);

            //返回获得的好感度
            var response = new GainFavorResponse();
            response.Id = sysFlopCfg.Id;
            ResultObj = response;
        }
    }
    #endregion

    #region 9009 晋封
    public class JinFengResponse
    {
        /// <summary>
        /// 妃子晋封CD时间【调用1401清除】
        /// </summary>
        //[Tag(1)]
        //public DateTime EndTime { get; set; }
        /// <summary>
        /// 晋封后的产量
        /// </summary>
        [Tag(1)]
        public int Product { get; set; }
    }
    /// <summary>
    /// 晋封
    /// </summary>
    [GameCode(OpCode = 9009, ResponseType = typeof(JinFengResponse))]//, ResponseType = typeof(JinFengResponse)
    public class JinFengRequest : GameHandler
    {
        /// <summary>
        /// 妃子ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            UserConcubine userConcubine;
            UserRole userRole;
            UserChip userChip;
            Storage.Load(out userConcubine, out userRole, out userChip, CurrentUserId, true);

            string errorMsg;
            if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
            {
                SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                return;
            }
            //if (userConcubine.EndTime.ToTs() > 0)
            //{
            //    //晋封CD中
            //    SetError(ResourceId.R_9009_JinFengCding);
            //    return;
            //}

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }

            var sysConcubineTitleCfg =
                SysConcubineTitleCfg.Items.FirstOrDefault(o => o.Id == userConcubineItem.TitleLevel);
            if (sysConcubineTitleCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysConcubineTitleCfg:Id", userConcubineItem.TitleLevel);
                return;
            }
            if (sysConcubineTitleCfg.NeedFavor == 0)
            {
                //封号已满
                SetError(ResourceId.R_9009_ConcubineTitleIsFull);
                return;
            }
            if (sysConcubineTitleCfg.NeedLevel > userRole.Level)
            {
                SetError(ResourceId.R_0000_UserLowLevel, sysConcubineTitleCfg.NeedLevel);
                return;
            }
            //if (sysConcubineTitleCfg.NeedFavor > userConcubineItem.Favor)
            if (sysConcubineTitleCfg.NeedFavor > userConcubineItem.TotalFavor)
            {
                SetError(ResourceId.R_0000_FavorNotEnough);
                return;
            }

            //判断资源是否足够
            //if (!Utility.JudgeResourceEnough(userRole, sysConcubineTitleCfg.NeedCoin, sysConcubineTitleCfg.NeedWood,
            //    sysConcubineTitleCfg.NeedStone, sysConcubineTitleCfg.NeedIron, out errorMsg))
            //{
            //    SetError(errorMsg);
            //    return;
            //}
            //var opCode = Request.OpCode;
            //Utility.ConsumeResource(userRole, ItemType.Coin, opCode, sysConcubineTitleCfg.NeedWood);
            //Utility.ConsumeResource(userRole, ItemType.Wood, opCode, sysConcubineTitleCfg.NeedWood);
            //Utility.ConsumeResource(userRole, ItemType.Stone, opCode, sysConcubineTitleCfg.NeedStone);
            //Utility.ConsumeResource(userRole, ItemType.Iron, opCode, sysConcubineTitleCfg.NeedIron);

            var needFavor = sysConcubineTitleCfg.NeedFavor;
            var userChipItem =
                userChip.ChipItems.FirstOrDefault(o => o.ItemId == userConcubineItem.ConcubineId + (int)ToolType.ConcubineChip);
            if (userChipItem != null)
            {
                var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == userChipItem.ItemId);
                if (sysToolCfg != null)
                {
                    //扣除碎片当好感度 modify by hql at 2015.10.10
                    var realUseNum = (int)(needFavor * 1.0 / sysToolCfg.Param3);
                    realUseNum = realUseNum >= userChipItem.Num ? userChipItem.Num : realUseNum;
                    userChip.SubChip(userChipItem.ItemId, realUseNum, ToolType.ConcubineChip);
                    needFavor -= realUseNum * sysToolCfg.Param3;
                }
            }

            userConcubineItem.Favor -= needFavor;
            userConcubineItem.TitleLevel++;
            userConcubineItem.RefreshProperties(Request.OpCode);
            userConcubine.EndTime = DateTime.Now.AddMinutes(sysConcubineTitleCfg.NeedTime);
            userConcubine.JinFengConcubineId = Id;

            var response = new JinFengResponse() { Product = userConcubineItem.RealProduct };
            ResultObj = response;
        }
    }
    #endregion

    #region 9010 给妃子使用增加好感度的道具
    /// <summary>
    /// 给妃子使用增加好感度的道具
    /// </summary>
    [GameCode(OpCode = 9010)]
    public class UseToolAddFavorRequest : GameHandler
    {
        /// <summary>
        /// 物品id
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// 使用的个数-长按传相应的数量，点击则传1
        /// </summary>
        public int UseNum { get; set; }

        /// <summary>
        /// 妃子Id（非系统ID）
        /// </summary>
        public int Id { get; set; }

        public override void Process(GameContext context)
        {
            UserConcubine userConcubine;
            UserRole userRole;
            UserTool userTool;
            Storage.Load(out userConcubine, out userRole, out userTool, CurrentUserId, true);

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }
            var sysConcubineTitleCfg = SysConcubineTitleCfg.Items.FirstOrDefault(o => o.Id == userConcubineItem.TitleLevel);
            if (sysConcubineTitleCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysConcubineTitleCfg:Id", userConcubineItem.TitleLevel);
                return;
            }
            //if (sysConcubineTitleCfg.NeedFavor <= userConcubineItem.Favor)
            if (sysConcubineTitleCfg.NeedFavor <= userConcubineItem.TotalFavor)
            {
                //好感度已满
                SetError(ResourceId.R_9010_FavorIsFull);
                return;
            }

            var sysToolCfg = SysToolCfg.Find(ItemId);
            if (sysToolCfg == null)
            {
                //物品不存在
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", ItemId);
                return;
            }
            var toolItem = userTool.Items.FirstOrDefault(o => o.ItemId == ItemId);
            if (toolItem == null)
            {
                //道具不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserTool", ItemId);
                return;
            }
            if (sysToolCfg.GetToolType() != ToolType.FavorTool)
            {
                //不是增加好感度道具
                SetError(ResourceId.R_9010_IsNotFavorTool);
                return;
            }
            if (toolItem.Num < UseNum)
            {
                //道具个数不足
                SetError(ResourceId.R_0000_ToolNotEnough);
                return;
            }

            //增加好感度
            var gainFavor = sysToolCfg.Param1 * UseNum;
            userConcubineItem.Favor += gainFavor;
            //if (userConcubineItem.Favor > sysConcubineTitleCfg.NeedFavor)
            //{
            //    userConcubineItem.Favor = sysConcubineTitleCfg.NeedFavor;
            //}
            userTool.RemoveTool(toolItem.ItemId, UseNum, Request.OpCode);
            //Utility.AddResource(userRole, ItemType.Favor, Request.OpCode, gainFavor);
        }
    }
    #endregion

    #region 9011 获取后宫界面信息
    /// <summary>
    /// 建筑列表信息
    /// </summary>
    public class GetHouGongInfoResponse
    {
        public GetHouGongInfoResponse()
        {
            BuildingItems = new List<ResBuildingItem>();
            ConcubineItems = new List<ResConcubineItem>();
            AccItems = new List<AccItem>();
        }
        /// <summary>
        /// 宫殿列表【可以建筑的、升级的、入住的根据本地配置进行提示】
        /// </summary>
        [Tag(1)]
        public List<ResBuildingItem> BuildingItems { get; set; }

        /// <summary>
        /// 妃子列表【宫殿中妃子数据从这里获得】
        /// </summary>
        [Tag(2)]
        public List<ResConcubineItem> ConcubineItems { get; set; }

        /// <summary>
        /// CD列表
        /// </summary>
        [Tag(3)]
        public List<AccItem> AccItems { get; set; }

        /// <summary>
        /// 翻牌次数
        /// </summary>
        [Tag(4)]
        public int FlopNum { get; set; }

        /// <summary>
        /// 今日已重置的翻牌次数
        /// </summary>
        [Tag(5)]
        public int BuyFlopNum { get; set; }

        /// <summary>
        /// 妃子训练位置配置
        /// </summary>
        [Tag(6)]
        public List<ConfigHelper.TrainConcubineItem> TrainConcubineItems { get; set; }

        public class ResBuildingItem
        {
            public ResBuildingItem()
            {
                ConcubineIdItems = new List<int>();
            }
            /// <summary>
            /// 系统建筑ID
            /// </summary>
            [Tag(1)]
            public int BuildingId { get; set; }
            /// <summary>
            /// 建筑等级
            /// </summary>
            [Tag(2)]
            public int Level { get; set; }
            /// <summary>
            /// 存储量
            /// </summary>
            [Tag(3)]
            public int Storage { get; set; }
            /// <summary>
            /// 入住妃子Id列表
            /// </summary>
            [Tag(4)]
            public List<int> ConcubineIdItems { get; set; }

            /// <summary>
            /// 加载数据
            /// </summary>
            /// <param name="userBuildingItem"></param>
            public void LoadBuildingData(UserBuildingItem userBuildingItem)
            {
                BuildingId = userBuildingItem.BuildingId;
                Level = userBuildingItem.Level;
                Storage = userBuildingItem.Storage;
                ConcubineIdItems = userBuildingItem.ConcubineIdList;
            }
        }
        public class ResConcubineItem
        {
            /// <summary>
            /// 妃子ID
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 妃子ID（系统）
            /// </summary>
            [Tag(2)]
            public int ConcubineId { get; set; }
            /// <summary>
            /// 妃子封号等级
            /// </summary>
            [Tag(3)]
            public int TitleLevel { get; set; }
            /// <summary>
            /// 基础产量（周期内）
            /// </summary>
            [Tag(4)]
            public int Product { get; set; }
            /// <summary>
            /// 特长增加的产量（周期内）
            /// </summary>
            [Tag(5)]
            public int AddProduct { get; set; }
            /// <summary>
            /// 就是上次收获的时间节点
            /// </summary>
            [Tag(6)]
            public DateTime LastGainTime { get; set; }
            /// <summary>
            /// 好感度
            /// </summary>
            [Tag(7)]
            public int Favor { get; set; }
            /// <summary>
            /// 妃子状态（0：空闲，1：生产）
            /// </summary>
            [Tag(8)]
            public ConcubineStatus Status { get; set; }
            /// <summary>
            /// 升级训练所在的位置（从1开始）
            /// </summary>
            [Tag(9)]
            public int TrainLocation { get; set; }
            /// <summary>
            /// 升级截止时间
            /// </summary>
            [Tag(10)]
            public DateTime UpgradeEndTime { get; set; }
            /// <summary>
            /// 妃子等级
            /// </summary>
            [Tag(11)]
            public int Level { get; set; }
            /// <summary>
            /// 妃子训练状态（0：未训练，1：等待训练，2：训练中（以训练截止时间判断训练是否结束））
            /// </summary>
            [Tag(12)]
            public ConcubineTrainStatus TrainStatus { get; set; }
        }
        public class AccItem
        {
            /// <summary>
            /// CD类型
            /// </summary>
            [Tag(1)]
            public AccEndTimeType Type { get; set; }
            /// <summary>
            /// 建筑升级CD时间【调用1401清除】
            /// </summary>
            [Tag(2)]
            public DateTime EndTime { get; set; }
            /// <summary>
            /// 每元宝清除的秒数【计算所需元宝】
            /// </summary>
            [Tag(3)]
            public int EveryMoneyRemoveSecond { get; set; }
            /// <summary>
            /// 升级/晋封的建筑/妃子ID
            /// </summary>
            [Tag(4)]
            public int UpgradeId { get; set; }
        }
    }
    /// <summary>
    /// 获取后宫界面信息（取代之前的9000和9007接口）
    /// </summary>
    [GameCode(OpCode = 9011, ResponseType = typeof(GetHouGongInfoResponse))]
    public class GetHouGongInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            Storage.Load(out userBuilding, out userConcubine, CurrentUserId, true);

            var response = new GetHouGongInfoResponse();
            foreach (var userBuildingItem in userBuilding.Items)
            {
                var item = new GetHouGongInfoResponse.ResBuildingItem();
                item.LoadBuildingData(userBuildingItem);

                response.BuildingItems.Add(item);
            }

            foreach (var userConcubineItem in userConcubine.Items
                .OrderByDescending(o => o.SysConcubineCfg.Quality)
                .ThenByDescending(o => o.TitleLevel)
                //.ThenByDescending(o => o.Favor)
                .ThenByDescending(o => o.TotalFavor))
            {
                var item = new GetHouGongInfoResponse.ResConcubineItem();
                var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserConcubineItem, GetHouGongInfoResponse.ResConcubineItem>();
                mapper.Map(userConcubineItem, item);
                item.Product = userConcubineItem.RealProduct;//userConcubineItem.Product + userConcubineItem.SysConcubineCfg.Growth * 5;
                item.Favor = userConcubineItem.TotalFavor;

                response.ConcubineItems.Add(item);
            }

            //建筑升级CD
            response.AccItems.Add(new GetHouGongInfoResponse.AccItem()
            {
                Type = AccEndTimeType.BuildingUpgrade,
                EndTime = userBuilding.EndTime > DateTime.Now ? userBuilding.EndTime : DateTime.Now.AddMinutes(-10),
                EveryMoneyRemoveSecond = ConfigHelper.ClearCdCfgData.BuildingUpgrade,
                UpgradeId = userBuilding.UpgradeBuildingId
            });
            //妃子晋封CD
            //response.AccItems.Add(new GetHouGongInfoResponse.AccItem()
            //{
            //    Type = AccEndTimeType.ConcubineJinFeng,
            //    EndTime = userConcubine.EndTime > DateTime.Now ? userConcubine.EndTime : DateTime.Now.AddMinutes(-10),
            //    EveryMoneyRemoveSecond = ConfigHelper.ClearCdCfgData.ConcubineJinFeng,
            //    UpgradeId = userConcubine.JinFengConcubineId
            //});
            response.FlopNum = userConcubine.LaveFlopNum;
            response.BuyFlopNum = (int)userConcubine.BuyFlopNum.Value;

            response.TrainConcubineItems = ConfigHelper.TrainConcubineCfgData.Items;

            ResultObj = response;
        }
    }
    #endregion

    #region 9012 建造宫殿
    public class BuildBuilResponse
    {
        public BuildBuilResponse()
        {
            NewBuildingItem = new GetHouGongInfoResponse.ResBuildingItem();
            AffectedBuildingItems = new List<ResSimpleBuildingItem>();
        }
        /// <summary>
        /// 新建建筑信息
        /// </summary>
        [Tag(1)]
        public GetHouGongInfoResponse.ResBuildingItem NewBuildingItem { get; set; }

        /// <summary>
        /// 影响到的同类型的建筑列表（会影响存储量）
        /// </summary>
        [Tag(2)]
        public List<ResSimpleBuildingItem> AffectedBuildingItems { get; set; }
        public class ResSimpleBuildingItem
        {
            /// <summary>
            /// 系统建筑id
            /// </summary>
            [Tag(1)]
            public int BuildingId { get; set; }
            /// <summary>
            /// 存储量
            /// </summary>
            [Tag(2)]
            public int Storage { get; set; }
        }
    }
    /// <summary>
    /// 建造宫殿（取代之前的9001接口）
    /// </summary>
    [GameCode(OpCode = 9012, ResponseType = typeof(BuildBuilResponse))]
    public class BuildBuilRequest : GameHandler
    {
        /// <summary>
        /// 宫殿ID
        /// </summary>
        public int BuildingId { get; set; }
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserRole userRole;
            Storage.Load(out userBuilding, out userRole, CurrentUserId, true);

            if (userBuilding.Items.Exists(o => o.BuildingId == BuildingId))
            {
                SetError(ResourceId.R_9001_BuildIsExist);
                return;
            }

            var sysBuildingCfg = SysBuildingCfg.Items.FirstOrDefault(o => o.BuildingId == BuildingId && o.Level == 1);
            if (sysBuildingCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBuildingCfg:", BuildingId);
                return;
            }

            if (sysBuildingCfg.NeedLevel > userRole.Level)
            {
                SetError(ResourceId.R_0000_UserLowLevel, sysBuildingCfg.NeedLevel);
                return;
            }
            var userBuildingItem = userBuilding.AddBuildingToUser(BuildingId, Request.OpCode);
            var response = new BuildBuilResponse();
            response.NewBuildingItem.LoadBuildingData(userBuildingItem);

            response.AffectedBuildingItems = userBuilding.GetAffectedBuildingList(userRole, userBuildingItem.MoneyType,
                userBuildingItem.BuildingId);

            ResultObj = response;
        }
    }
    #endregion

    #region 9013 升级宫殿
    public class UpgradeBuiResponse
    {
        public UpgradeBuiResponse()
        {
            AffectedBuildingItems = new List<BuildBuilResponse.ResSimpleBuildingItem>();
        }
        /// <summary>
        /// 宫殿升级CD截止时间【调用1401清除】
        /// </summary>
        [Tag(1)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 影响到的同类型的建筑列表（会影响存储量）
        /// </summary>
        [Tag(2)]
        public List<BuildBuilResponse.ResSimpleBuildingItem> AffectedBuildingItems { get; set; }
    }
    /// <summary>
    /// 升级宫殿(取代之前的9002)
    /// </summary>
    [GameCode(OpCode = 9013, ResponseType = typeof(UpgradeBuiResponse))]
    public class UpgradeBuilRequest : GameHandler
    {
        /// <summary>
        /// 宫殿ID
        /// </summary>
        public int BuildingId { get; set; }
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserRole userRole;
            Storage.Load(out userBuilding, out userRole, CurrentUserId, true);

            string errorMsg;
            if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
            {
                SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                return;
            }
            if (userBuilding.EndTime.ToTs() > 0)
            {
                SetError(ResourceId.R_9001_BuildingUpgradeCding);
                return;
            }

            var userBuildingItem = userBuilding.Items.FirstOrDefault(o => o.BuildingId == BuildingId);
            if (userBuildingItem == null)
            {
                //不存在宫殿
                SetError(ResourceId.R_0000_IdNotExist, "UserBuilding", BuildingId);
                return;
            }

            var sysBuildingCfg =
                SysBuildingCfg.Items.FirstOrDefault(o => o.BuildingId == BuildingId && o.Level == userBuildingItem.Level + 1);
            if (sysBuildingCfg == null)
            {
                //已经是最高等级
                SetError(ResourceId.R_0000_IdNotExist, "SysBuildingCfg:Level", BuildingId + userBuildingItem.Level + 1);
                return;
            }

            if (sysBuildingCfg.NeedLevel > userRole.Level)
            {
                //等级不足
                SetError(ResourceId.R_0000_UserLowLevel, sysBuildingCfg.NeedLevel);
                return;
            }

            //判断资源是否足够
            if (!Utility.JudgeResourceEnough(userRole, sysBuildingCfg.NeedCoin, sysBuildingCfg.NeedWood
                , sysBuildingCfg.NeedStone, sysBuildingCfg.NeedIron, out errorMsg))
            {
                SetError(errorMsg);
                return;
            }
            var opCode = Request.OpCode;
            Utility.ConsumeResource(userRole, ItemType.Coin, opCode, sysBuildingCfg.NeedCoin);
            Utility.ConsumeResource(userRole, ItemType.Wood, opCode, sysBuildingCfg.NeedWood);
            Utility.ConsumeResource(userRole, ItemType.Stone, opCode, sysBuildingCfg.NeedStone);
            Utility.ConsumeResource(userRole, ItemType.Iron, opCode, sysBuildingCfg.NeedIron);
            userBuildingItem.Level++;

            //刷新上限
            userBuilding.RefreshCapacity(userBuildingItem.MoneyType);

            userBuilding.EndTime = DateTime.Now.AddMinutes(sysBuildingCfg.NeedTime);

            var response = new UpgradeBuiResponse();
            response.EndTime = userBuilding.EndTime;
            response.AffectedBuildingItems = userBuilding.GetAffectedBuildingList(userRole, userBuildingItem.MoneyType);

            ResultObj = response;
        }
    }
    #endregion

    #region 9014 妃子入住宫殿
    public class InBuilResponse
    {
        /// <summary>
        /// 妃子ID
        /// </summary>
        [Tag(1)]
        public int Id { get; set; }
        /// <summary>
        /// 可收获的时间
        /// </summary>
        //[Tag(2)]
        //public DateTime CanGainTime { get; set; }
        /// <summary>
        /// 妃子状态（0：空闲，1：生产）
        /// </summary>
        [Tag(3)]
        public ConcubineStatus Status { get; set; }
    }
    /// <summary>
    /// 妃子入住宫殿（取代之前的9004接口）
    /// </summary>
    [GameCode(OpCode = 9014, ResponseType = typeof(InBuilResponse))]
    public class InBuilRequest : GameHandler
    {
        /// <summary>
        /// 宫殿ID
        /// </summary>
        public int BuildingId { get; set; }
        /// <summary>
        /// 妃子ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            Storage.Load(out userBuilding, out userConcubine, CurrentUserId, true);

            var userBuildingItem = userBuilding.Items.FirstOrDefault(o => o.BuildingId == BuildingId);
            if (userBuildingItem == null)
            {
                //不存在宫殿
                SetError(ResourceId.R_0000_IdNotExist, "UserBuilding", BuildingId);
                return;
            }

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }

            if (userConcubineItem.Status != ConcubineStatus.Idle)
            {
                //妃子已经入住宫殿
                SetError(ResourceId.R_9004_ConcubineAlreadyInBuilding);
                return;
            }

            var sysBuildingCfg =
                SysBuildingCfg.Items.FirstOrDefault(o => o.BuildingId == BuildingId && o.Level == userBuildingItem.Level);
            if (sysBuildingCfg == null)
            {
                //不存在宫殿配置信息
                SetError(ResourceId.R_0000_IdNotExist, "SysBuildingCfg:Level", BuildingId + userBuildingItem.Level + 1);
                return;
            }
            var concubineNum = sysBuildingCfg.ConcubineNum;
            if (userBuildingItem.ConcubineIdList.Count >= concubineNum)
            {
                //宫殿已满
                SetError(ResourceId.R_9004_BuildingIsFull);
                return;
            }
            userBuildingItem.ConcubineIdList.Add(Id);
            userConcubineItem.Status = ConcubineStatus.Produce;
            userConcubineItem.LastGainTime = DateTime.Now;
            //userConcubineItem.RealGainTime = DateTime.Now;
            userConcubineItem.BuildingMoneyType = userBuildingItem.MoneyType;

            var response = new InBuilResponse()
            {
                Id = userConcubineItem.Id,
                //CanGainTime = userConcubineItem.CanGainTime,
                Status = userConcubineItem.Status,
            };
            ResultObj = response;
        }
    }
    #endregion

    #region 9015 妃子休息（出宫殿）
    /// <summary>
    /// 妃子休息（出宫殿并且收获资源，如果达到资源存储上限客户端得提示“是否放弃多余资源”）【取代之前的9005】
    /// </summary>
    [GameCode(OpCode = 9015, ResponseType = typeof(GainResResponse))]
    public class OutBuilRequest : GameHandler
    {
        /// <summary>
        /// 宫殿ID
        /// </summary>
        public int BuildingId { get; set; }
        /// <summary>
        /// 妃子ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            UserRole userRole;
            Storage.Load(out userBuilding, out userConcubine, out userRole, CurrentUserId, true);

            var userBuildingItem = userBuilding.Items.FirstOrDefault(o => o.BuildingId == BuildingId);
            if (userBuildingItem == null)
            {
                //不存在宫殿
                SetError(ResourceId.R_0000_IdNotExist, "UserBuilding", BuildingId);
                return;
            }

            var moneyType = userBuildingItem.SysBuildingCfg.MoneyType;
            if (Utility.CheckResourceIsFull(userRole, moneyType))
            {
                var moneyName = Utility.GetMonenyTypeName(moneyType);
                //资源达到上限了
                SetError(ResourceId.R_0000_ResourceIsFull, moneyName);
                return;
            }

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }

            if (userConcubineItem.Status == ConcubineStatus.Idle)
            {
                //妃子正在休息
                SetError(ResourceId.R_9005_ConcubineAlreadyIdle);
                return;
            }

            if (!userBuildingItem.ConcubineIdList.Contains(Id))
            {
                //妃子不在该宫殿
                SetError(ResourceId.R_9005_ConcubineNotInBuilding);
                return;
            }


            //休息时被动触发收获资源，如果超出最大值客户端提示即可，这里直接去掉！！
            //没有时间限制了，所以直接全部收获,1
            var gainResource = userConcubineItem.GainToUser(Request.OpCode);
            var outPut = userConcubineItem.OutPut;

            if (outPut == 0)
            {
                //休息成功！
                userBuildingItem.ConcubineIdList.Remove(Id);
                userConcubineItem.Status = ConcubineStatus.Idle;
                userConcubineItem.BuildingMoneyType = MoneyType.None;

                //userConcubine.ChangeNewMsg(userRole);//后宫信息
            }

            //返回获得的资源
            var response = new GainResResponse(gainResource, userBuildingItem.MoneyType, userConcubineItem, userBuilding);
            ResultObj = response;
        }
    }
    #endregion

    #region 9016 收获资源
    public class GainResResponse
    {
        /// <summary>
        /// 获得的资源
        /// </summary>
        [Tag(1)]
        public int Resource { get; set; }
        /// <summary>
        /// 妃子身上的资源（如果大于零，说明玩家达到资源最大上限，生产出来的资源只能继续存在妃子身上。）
        /// 对于9015接口来说，大于零即意味着妃子休息失败！只是把一部分生产出来的资源收获到玩家身上而已。
        /// 对于9016接口来说，没有影响，只是收获了部分生产出来的资源而已。
        /// </summary>
        [Tag(2)]
        public int OutPut { get; set; }

        /// <summary>
        /// 影响到的建筑列表（收获后会影响建筑的当前存储量）
        /// </summary>
        [Tag(3)]
        public List<BuildBuilResponse.ResSimpleBuildingItem> AffectedBuildingItems { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="gainResource"></param>
        /// <param name="userConcubineItem"></param>
        /// <param name="userBuilding"></param>
        public GainResResponse(int gainResource, MoneyType moneyType, UserConcubineItem userConcubineItem, UserBuilding userBuilding)
        {
            Resource = gainResource;
            OutPut = userConcubineItem.OutPut;

            AffectedBuildingItems = new List<BuildBuilResponse.ResSimpleBuildingItem>();
            var buildingItems = userBuilding.Items.Where(o => o.MoneyType == moneyType).ToList();
            foreach (var userBuildingItem in buildingItems)
            {
                AffectedBuildingItems.Add(new BuildBuilResponse.ResSimpleBuildingItem()
                {
                    BuildingId = userBuildingItem.BuildingId,
                    Storage = userBuildingItem.Storage,
                });
            }
        }
    }
    /// <summary>
    /// 收获资源（取代之前的9006）
    /// </summary>
    [GameCode(OpCode = 9016, ResponseType = typeof(GainResResponse))]
    public class GainResRequest : GameHandler
    {
        /// <summary>
        /// 宫殿ID
        /// </summary>
        public int BuildingId { get; set; }
        /// <summary>
        /// 妃子ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            UserRole userRole;
            Storage.Load(out userBuilding, out userConcubine, out userRole, CurrentUserId, true);

            var userBuildingItem = userBuilding.Items.FirstOrDefault(o => o.BuildingId == BuildingId);
            if (userBuildingItem == null)
            {
                //不存在宫殿
                SetError(ResourceId.R_0000_IdNotExist, "UserBuilding", BuildingId);
                return;
            }

            var moneyType = userBuildingItem.SysBuildingCfg.MoneyType;
            if (Utility.CheckResourceIsFull(userRole, moneyType))
            {
                var moneyName = Utility.GetMonenyTypeName(moneyType);
                //资源达到上限了
                SetError(ResourceId.R_0000_ResourceIsFull, moneyName);
                return;
            }

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);

            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }
            //if (userConcubineItem.CanGainTime.ToTs() > 0)
            //{
            //    //妃子还不能收获
            //    SetError(ResourceId.R_9005_ConcubineCanNotGain);
            //    return;
            //}

            if (userConcubineItem.Status == ConcubineStatus.Idle)
            {
                //妃子正在休息
                SetError(ResourceId.R_9005_ConcubineAlreadyIdle);
                return;
            }

            if (!userBuildingItem.ConcubineIdList.Contains(Id))
            {
                //妃子不在该宫殿
                SetError(ResourceId.R_9005_ConcubineNotInBuilding);
                return;
            }

            //主动收获资源
            var gainResource = userConcubineItem.GainToUser(Request.OpCode);

            //userConcubine.ChangeNewMsg(userRole);//后宫信息

            //返回获得的资源
            var response = new GainResResponse(gainResource, userBuildingItem.MoneyType, userConcubineItem, userBuilding);
            ResultObj = response;
        }
    }
    #endregion

    #region 9017 获取敌方后宫信息
    public class GetOtherHouGongInfoResponse
    {
        public GetOtherHouGongInfoResponse()
        {
            Items = new List<ResBuildingItem>();
            UserInfo = new ResUserInfo();
            //CanRobResList = new List<ItemPair>();
        }
        /// <summary>
        /// 建筑列表
        /// </summary>
        [Tag(1)]
        public List<ResBuildingItem> Items { get; set; }

        /// <summary>
        /// 被抢用户信息
        /// </summary>
        [Tag(2)]
        public ResUserInfo UserInfo { get; set; }

        /// <summary>
        /// 总共可抢的资源列表
        /// </summary>
        //[Tag(3)]
        //public List<ItemPair> CanRobResList { get; set; }

        public class ResBuildingItem
        {
            public ResBuildingItem()
            {
                ConcubineItems = new List<ResConcubineItem>();
            }
            /// <summary>
            /// 系统建筑ID
            /// </summary>
            [Tag(1)]
            public int BuildingId { get; set; }
            /// <summary>
            /// 建筑等级
            /// </summary>
            [Tag(2)]
            public int Level { get; set; }
            /// <summary>
            /// 入住系统妃子Id列表
            /// </summary>
            [Tag(3)]
            public List<ResConcubineItem> ConcubineItems { get; set; }

            /// <summary>
            /// 加载数据
            /// </summary>
            /// <param name="userBuildingItem"></param>
            /// <param name="cityLevel"></param>
            public void LoadBuildingData(UserConcubine userConcubine, UserBuildingItem userBuildingItem, int cityLevel, ConfigHelper.BigMapCfg bigMapCfgData, out int CanRobNum)
            {
                //modify by hql at 2015.10.24
                //被抢资源其实在打赢就已经抢完了！！这里无需返回值了。。
                //var robBuildingPrec = bigMapCfgData.RobBuildPrec;
                BuildingId = userBuildingItem.BuildingId;
                Level = userBuildingItem.Level;
                CanRobNum = 0;//userBuildingItem.GetBuildingRobNum(robBuildingPrec);
                //if (CanRobNum == 0) CanRobNum = cityLevel * bigMapCfgData.MinRobResModulus;
                CanRobNum += 0;//userBuildingItem.GetConcubinesRobNum(userConcubine);

                foreach (var i in userBuildingItem.ConcubineIdList)
                {
                    var concubineItem = userConcubine.Items.FirstOrDefault(o => o.Id == i);
                    if (concubineItem != null)
                        ConcubineItems.Add(new ResConcubineItem() { Id = i, SysConcubineId = concubineItem.ConcubineId });
                }
            }
        }
        public class ResConcubineItem
        {
            /// <summary>
            /// ID
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 系统妃子ID
            /// </summary>
            [Tag(2)]
            public int SysConcubineId { get; set; }
        }
        public class ResUserInfo
        {
            /// <summary>
            /// 头像ID
            /// </summary>
            [Tag(1)]
            public int HeadId { get; set; }
            /// <summary>
            /// 玩家等级
            /// </summary>
            [Tag(2)]
            public int Level { get; set; }
            /// <summary>
            /// vip等级
            /// </summary>
            [Tag(3)]
            public int VipLevel { get; set; }
            /// <summary>
            /// 昵称
            /// </summary>
            [Tag(4)]
            public string NickName { get; set; }
        }
    }
    /// <summary>
    /// 获取敌方后宫信息
    /// </summary>
    [GameCode(OpCode = 9017, ResponseType = typeof(GetOtherHouGongInfoResponse))]
    public class GetOtherHouGongInfoRequest : GameHandler
    {
        /// <summary>
        /// 战斗ID
        /// </summary>
        public int BattleId { get; set; }
        public override void Process(GameContext context)
        {
            var allBattle = Storage.Load<AllBattle>(BattleId);
            //if (allBattle.CreateTime.AddMinutes(30) < DateTime.Now)
            //{
            //    //此战斗的抢劫已过期
            //    SetError(ResourceId.R_9017_RobExpired);
            //    return;
            //}
            if (allBattle.DefenderId == CurrentUserId)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            //var sysCityId = allBattle.Param1;
            //var sysCityCfg = SysCityCfg.Items.FirstOrDefault(o => o.Id == sysCityId);
            //if (sysCityCfg == null)
            //{
            //    SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", sysCityId);
            //    return;
            //}

            UserConcubine defenderUserConcubine;
            UserBuilding defenderUserBuilding;
            UserRole defenderUserRole;
            Storage.Load(out defenderUserConcubine, out defenderUserBuilding, out defenderUserRole, allBattle.DefenderId);

            //var userRole = Storage.Load<UserRole>(allBattle.DefenderId, true);
            if (defenderUserRole.BeAttackEndTime.ToTs() <= 0)
            {
                //此战斗的抢劫已过期
                SetError(ResourceId.R_9017_RobExpired);
                return;
            }

            var response = new GetOtherHouGongInfoResponse();

            var bigMapCfgData = ConfigHelper.BigMapCfgData;
            //var canRobResList = new List<ItemPair>();
            foreach (var userBuildingItem in defenderUserBuilding.Items)
            {
                int canRobNum;
                var item = new GetOtherHouGongInfoResponse.ResBuildingItem();
                item.LoadBuildingData(defenderUserConcubine, userBuildingItem, defenderUserRole.Level, bigMapCfgData, out canRobNum);

                //int itemId = (int)Utility.GetSpecialToolId(userBuildingItem.MoneyType);
                //var canRobItem = canRobResList.FirstOrDefault(o => o.ItemId == itemId);
                //if (canRobItem == null)
                //{
                //    canRobItem = new ItemPair() { ItemId = itemId, Num = canRobNum };
                //    canRobResList.Add(canRobItem);
                //}
                //else
                //{
                //    canRobItem.Num += canRobNum;
                //}

                response.Items.Add(item);
            }

            //response.CanRobResList = canRobResList.OrderBy(o => o.ItemId).ToList();
            var userInfo = new GetOtherHouGongInfoResponse.ResUserInfo();
            userInfo.HeadId = defenderUserRole.HeadId;
            userInfo.Level = defenderUserRole.Level;
            userInfo.VipLevel = defenderUserRole.VipLevel;
            userInfo.NickName = defenderUserRole.NickName;
            response.UserInfo = userInfo;

            //改变用户的被抢劫截止时间
            defenderUserRole.BeAttackEndTime = DateTime.Now.AddSeconds(ConfigHelper.RobExpireTime);
            defenderUserRole.BeAttackType = BeAttackType.Rob;

            //添加采花次数——一次战斗就计算一次采花。
            Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.Deflower);

            ResultObj = response;
        }
    }
    #endregion

    #region 9018 点击敌方后宫宫殿抢劫资源——作废！改为一打完直接给
    public class RobResResponse
    {
        /// <summary>
        /// 真实获得的资源类型【12000002：铜钱，12000005：木材，12000006：石头，12000007：铁矿】
        /// </summary>
        [Tag(1)]
        public int ItemId { get; set; }
        /// <summary>
        /// 真实获得的资源数量
        /// </summary>
        [Tag(2)]
        public int Num { get; set; }
    }
    /// <summary>
    /// 点击敌方后宫宫殿抢劫资源——作废！改为一打完直接给
    /// </summary>
    [GameCode(OpCode = 9018, ResponseType = typeof(RobResResponse))]
    public class RobResRequest : GameHandler
    {
        /// <summary>
        /// 战斗ID
        /// </summary>
        public int BattleId { get; set; }
        /// <summary>
        /// 系统建筑ID
        /// </summary>
        public int BuildingId { get; set; }
        public override void Process(GameContext context)
        {
            ResultObj = new RobResResponse();
            return;
            //var allBattle = Storage.Load<AllBattle>(BattleId, true);
            ////if (allBattle.CreateTime.AddMinutes(30) < DateTime.Now)
            ////{
            ////    //此战斗的抢劫已过期
            ////    SetError(ResourceId.R_9017_RobExpired);
            ////    return;
            ////}
            //if (allBattle.DefenderId == CurrentUserId)
            //{
            //    SetError(ResourceId.R_0000_IllegalParam);
            //    return;
            //}

            //if (allBattle.ListIntParam1.Contains(BuildingId))
            //{
            //    //该建筑已被抢劫过
            //    SetError(ResourceId.R_9019_BuildIsRobed);
            //    return;
            //}

            //UserConcubine defenderUserConcubine;
            //UserBuilding defenderUserBuilding;
            //UserRole defenderUserRole;
            //UserCity defenderUserCity;
            //Storage.Load(out defenderUserConcubine, out defenderUserBuilding, out defenderUserRole, out defenderUserCity,
            //    allBattle.DefenderId);

            //var userBuildingItem = defenderUserBuilding.Items.FirstOrDefault(o => o.BuildingId == BuildingId);
            //if (userBuildingItem == null)
            //{
            //    //该建筑不存在
            //    SetError(ResourceId.R_9018_BuildIsNotExist);
            //    return;
            //}

            //if (defenderUserRole.BeAttackEndTime.ToTs() <= 0)
            //{
            //    //此战斗的抢劫已过期
            //    SetError(ResourceId.R_9017_RobExpired);
            //    return;
            //}

            //var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            //var moneyType = userBuildingItem.MoneyType;
            ////if (Utility.CheckResourceIsFull(userRole, moneyType))
            ////{
            ////    var moneyName = Utility.GetMonenyTypeName(moneyType);
            ////    //资源达到上限了
            ////    SetError(ResourceId.R_0000_ResourceIsFull, moneyName);
            ////    return;
            ////}
            //var bigMapCfgData = ConfigHelper.BigMapCfgData;
            //var robedNum = userBuildingItem.GetBuildingRobNum(bigMapCfgData.RobBuildPrec);

            ////扣除玩家身上的资源，并且不立即分配建筑存储量
            //var index = (int)userBuildingItem.MoneyType - 2;
            //Utility.ConsumeResource(defenderUserRole, (ItemType)(index + 2), Request.OpCode, robedNum);

            //robedNum += userBuildingItem.GetConcubinesRobNum(defenderUserConcubine, 1);
            //allBattle.ListIntParam1.Add(BuildingId);

            ////系统根据 城池等级赠送的资源
            ////var cityId = allBattle.Param1;
            ////var sysCityCfg = SysCityCfg.Find(cityId);
            ////if (sysCityCfg == null)
            ////{
            ////    SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", cityId);
            ////    return;
            ////}
            ////robedNum += sysCityCfg.Level * bigMapCfgData.MinRobResModulus;

            ////抢的东西添加到用户身上
            //robedNum = Utility.AddResource(userRole, (ItemType)(index + 2), Request.OpCode, robedNum);

            ////添加事件损失的资源
            //var itemId = (int)Utility.GetSpecialToolId(userBuildingItem.MoneyType);
            //var eventItem = defenderUserCity.EventItems.Find(o => o.BattleId == BattleId);
            //if (eventItem != null)
            //{
            //    eventItem.AddItemPair(itemId, robedNum);
            //}

            //var response = new RobResResponse() { ItemId = itemId, Num = robedNum };
            //ResultObj = response;
        }
    }
    #endregion

    #region 9019 采花
    public class FlowersResponse
    {
        public FlowersResponse()
        {
            IsSuccess = 0;
            ItemPairList = new List<ItemPair>();
        }
        /// <summary>
        /// 采花是否成功[0:失败，1：成功]
        /// </summary>
        [Tag(1)]
        public int IsSuccess { get; set; }
        /// <summary>
        /// 道具(13000000:鲜花、12000003:魅力等)列表
        /// </summary>
        [Tag(2)]
        public List<ItemPair> ItemPairList { get; set; }
    }
    /// <summary>
    /// 采花
    /// </summary>
    [GameCode(OpCode = 9019, ResponseType = typeof(FlowersResponse))]
    public class FlowersRequest : GameHandler
    {
        /// <summary>
        /// 战斗ID
        /// </summary>
        public int BattleId { get; set; }
        /// <summary>
        /// 妃子ID【不是系统ID】
        /// </summary>
        public int ConcubineId { get; set; }
        public override void Process(GameContext context)
        {
            var allBattle = Storage.Load<AllBattle>(BattleId);
            //if (allBattle.CreateTime.AddMinutes(30) < DateTime.Now)
            //{
            //    //此战斗的抢劫已过期
            //    SetError(ResourceId.R_9017_RobExpired);
            //    return;
            //}
            if (allBattle.DefenderId == CurrentUserId)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            var sysVipCfg = SysVipCfg.Items.FirstOrDefault(o => o.VipLevel == userRole.RealVipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg:VipLevel", userRole.RealVipLevel);
                return;
            }
            if (allBattle.ListIntParam2.Count >= sysVipCfg.FlowersNum)
            {
                //采花次数已用完
                // SetError(ResourceId.R_9019_FlowersNumIsOver);
                // return;
            }

            UserRole defenderUserRole;
            UserConcubine defenderUserConcubine;
            UserCity defenderUserCity;
            Storage.Load(out defenderUserRole, out defenderUserConcubine, out defenderUserCity, allBattle.DefenderId, true);

            if (defenderUserRole.BeAttackEndTime.ToTs() <= 0)
            {
                //此战斗的抢劫已过期
                SetError(ResourceId.R_9017_RobExpired);
                return;
            }

            var response = new FlowersResponse();
            var userConcubineItem = defenderUserConcubine.Items.FirstOrDefault(o => o.Id == ConcubineId);
            if (userConcubineItem == null)
            {
                //找不到妃子
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", ConcubineId);
                return;
            }

            var initSuccessPrec = 30;//初始的成功率
            var eachCharm = 100;//每多100魅力多1点的成功率
            var eachAddSuccessPrec = 1;
            var difCharm = userRole.Charm - defenderUserRole.Charm;
            var charmAddSuccessPrec = difCharm > 0 ? (int)(difCharm * 1.0 / eachCharm) * eachAddSuccessPrec : 0;

            var successPrec = initSuccessPrec + charmAddSuccessPrec;
            var result = Util.IsHit(successPrec * 1.0 / 100);
            if (result)
            {
                response.IsSuccess = 1;
                //采花成功
                //1、减少妃子好感度
                var reduceFavor = 10;
                userConcubineItem.Favor = userConcubineItem.Favor > reduceFavor
                    ? userConcubineItem.Favor - reduceFavor
                    : 0;

                //2、赠送道具给抢劫者
                var toolId = 13000000;
                var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == toolId);
                if (sysToolCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", toolId);
                    return;
                }
                sysToolCfg.AddToUser(CurrentUserId, Request.OpCode);
                response.ItemPairList.Add(new ItemPair() { ItemId = toolId, Num = 1 });

                //3、获得魅力
                var addCharm = 10;
                Utility.AddResource(userRole, ItemType.Charm, Request.OpCode, addCharm);
                response.ItemPairList.Add(new ItemPair() { ItemId = (int)SpecialToolId.Charm, Num = addCharm });

                //4、几率获得妃子碎片
                var probabilityList = new List<int>() { 40, 35, 30, 25, 20 };//白色、绿色、蓝色、紫色、黄色
                var sysConcubineCfg = SysConcubineCfg.Items.Find(o => o.Id == userConcubineItem.ConcubineId);
                if (sysConcubineCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysConcubineCfg:Id", userConcubineItem.ConcubineId);
                    return;
                }
                var probability = 0;
                if ((int)sysConcubineCfg.Quality > (probabilityList.Count - 1))
                    probability = probabilityList[probabilityList.Count - 1];
                else probability = probabilityList[(int)sysConcubineCfg.Quality];
                result = Util.IsHit(probability * 1.0 / 100);
                if (result)
                {
                    //获得了妃子碎片
                    //妃子碎片ID
                    var concubineChipId = sysConcubineCfg.Id + (int)ToolType.ConcubineChip;
                    sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == concubineChipId);
                    if (sysToolCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", concubineChipId);
                        return;
                    }
                    sysToolCfg.AddToUser(CurrentUserId, Request.OpCode);
                    response.ItemPairList.Add(new ItemPair() { ItemId = concubineChipId, Num = 1 });
                }
            }
            //添加事件被调戏的妃子
            var eventItem = defenderUserCity.EventItems.Find(o => o.BattleId == BattleId);
            if (eventItem != null)
            {
                eventItem.AddSysConcubineId(userConcubineItem.ConcubineId);
            }

            allBattle.ListIntParam2.Add(ConcubineId);

            ResultObj = response;
        }
    }
    #endregion

    #region 9020 提前抢劫完
    /// <summary>
    /// 9020 提前抢劫完
    /// </summary>
    [GameCode(OpCode = 9020)]
    public class FinishRobRequest : GameHandler
    {
        /// <summary>
        /// 战斗ID
        /// </summary>
        public int BattleId { get; set; }
        public override void Process(GameContext context)
        {
            var allBattle = Storage.Load<AllBattle>(BattleId);
            if (allBattle.AttackerId != CurrentUserId)
            {
                //该战斗不属于该用户
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }
            if (allBattle.CreateTime.AddMinutes(15) < DateTime.Now)
            {
                //战斗抢劫已结束
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            var userRole = Storage.Load<UserRole>(allBattle.DefenderId, true);

            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, new GameLogItem()
            {
                F1 = BattleId,
                S1 = userRole.BeAttackEndTime.ToString(),
            });
            userRole.BeAttackEndTime = DateTime.Now;
            userRole.BeAttackType = BeAttackType.None;
        }
    }
    #endregion

    #region 9021 重置翻牌次数
    /// <summary>
    /// 9021 重置翻牌次数 
    /// </summary>
    [GameCode(OpCode = 9021)]
    public class ResetFlopNumRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserConcubine userConcubine;
            Storage.Load(out userRole, out userConcubine, CurrentUserId, true);

            var vipLevel = userRole.RealVipLevel;
            var sysVipCfg = SysVipCfg.Find(o => o.VipLevel == vipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg", vipLevel);
                return;
            }
            var curBuyNum = (int)userConcubine.BuyFlopNum.Value;
            if (curBuyNum >= sysVipCfg.FlopBuyNum)
            {
                //购买次数已用完
                SetError(ResourceId.R_0000_NoBuyNum);
                return;
            }
            var sysBuyNumCfg = SysBuyNumCfg.Find(curBuyNum + 1);
            if (sysBuyNumCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBuyNumCfg", curBuyNum + 1);
                return;
            }
            var needMoney = sysBuyNumCfg.BuyFlopNumMoney;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.BuyFlopNum);

            //重置次数
            userConcubine.UseFlopNum -= userConcubine.UseFlopNum.Value;
            userConcubine.BuyFlopNum += 1;
        }
    }
    #endregion

    #region 9022 一键收获资源
    public class GainAllResResponse
    {
        public GainAllResResponse()
        {
            GainConcubineItems = new List<ResConcubineGainItem>();
            AffectedBuildingItems = new List<BuildBuilResponse.ResSimpleBuildingItem>();
        }
        /// <summary>
        /// 收获的妃子列表
        /// </summary>
        [Tag(1)]
        public List<ResConcubineGainItem> GainConcubineItems { get; set; }
        /// <summary>
        /// 影响到的建筑列表（收获后会影响建筑的当前存储量）
        /// </summary>
        [Tag(2)]
        public List<BuildBuilResponse.ResSimpleBuildingItem> AffectedBuildingItems { get; set; }

        public class ResConcubineGainItem
        {
            /// <summary>
            /// 获得的资源
            /// </summary>
            [Tag(1)]
            public int Resource { get; set; }
            /// <summary>
            /// 妃子身上的资源（如果大于零，说明玩家达到资源最大上限，生产出来的资源只能继续存在妃子身上。）
            /// 对于9005接口来说，大于零即意味着妃子休息失败！只是把一部分生产出来的资源收获到玩家身上而已。
            /// 对于9006接口来说，没有影响，只是收获了部分生产出来的资源而已。
            /// </summary>
            [Tag(2)]
            public int OutPut { get; set; }
            /// <summary>
            /// 资源类型
            /// </summary>
            [Tag(3)]
            public MoneyType MoneyType { get; set; }
            /// <summary>
            /// 妃子id
            /// </summary>
            [Tag(4)]
            public int ConcubineId { get; set; }
        }

        /// <summary>
        /// 增加妃子收获
        /// </summary>
        /// <param name="gainResource"></param>
        /// <param name="userConcubineItem"></param>
        /// <param name="userBuilding"></param>
        public void AddConcubineGain(int gainResource, UserConcubineItem userConcubineItem, UserBuilding userBuilding)
        {
            GainConcubineItems.Add(new ResConcubineGainItem()
            {
                Resource = gainResource,
                OutPut = userConcubineItem.OutPut,
                MoneyType = userConcubineItem.MoneyType,
                ConcubineId = userConcubineItem.ConcubineId,
            });

            var buildingItems = userBuilding.Items.Where(o => o.MoneyType == userConcubineItem.MoneyType).ToList();
            foreach (var userBuildingItem in buildingItems)
            {
                if (!AffectedBuildingItems.Exists(o => o.BuildingId == userBuildingItem.BuildingId))
                {
                    AffectedBuildingItems.Add(new BuildBuilResponse.ResSimpleBuildingItem()
                    {
                        BuildingId = userBuildingItem.BuildingId,
                        Storage = userBuildingItem.Storage,
                    });
                }
            }
        }
    }
    /// <summary>
    /// 一键收获资源
    /// </summary>
    [GameCode(OpCode = 9022, ResponseType = typeof(GainAllResResponse))]
    public class GainAllResRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            UserRole userRole;
            Storage.Load(out userBuilding, out userConcubine, out userRole, CurrentUserId, true);

            //返回获得的资源
            var response = new GainAllResResponse();

            var produceUserConcubineList =
                userConcubine.Items.Where(o => o.Status == ConcubineStatus.Produce)
                    .ToList();// && o.CanGainTime.ToTs() <= 0
            foreach (var userConcubineItem in produceUserConcubineList)
            {
                //偷懒 直接以妃子特长作为生产的币种 当前版本可行，如果改为了不限制特长入住宫殿则需要改哦！！
                var moneyType = userConcubineItem.SysConcubineCfg.MoneyType;

                //资源达到上限后则直接收获
                if (Utility.CheckResourceIsFull(userRole, moneyType)) continue;

                //主动收获资源
                var gainResource = userConcubineItem.GainToUser(Request.OpCode);
                if (gainResource > 0)
                    response.AddConcubineGain(gainResource, userConcubineItem, userBuilding);
            }

            userConcubine.ChangeNewMsg(userRole);//后宫信息
            ResultObj = response;

        }
    }
    #endregion

    #region 9023 升级训练妃子
    /// <summary>
    /// 9023 升级训练妃子
    /// </summary>
    [GameCode(OpCode = 9023)]
    public class UpgradeConcubineRequest : GameHandler
    {
        /// <summary>
        /// 妃子id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 训练位置
        /// </summary>
        //public int Location { get; set; }
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserConcubine userConcubine;
            Storage.Load(out userRole, out userConcubine, CurrentUserId, true);

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }

            var location = userConcubineItem.TrainLocation;
            if (location <= 0)
            {
                //妃子不在训练位置上
                SetError(ResourceId.R_9023_ConcubineNotAtTrainLocation);
                return;
            }

            //TODO 判定等级是否超过

            var sysConcubineCfg = userConcubineItem.SysConcubineCfg;
            var sysConcubineUpgradeCfg = SysConcubineUpgradeCfg.GetCfg(userConcubineItem.MoneyType,
                sysConcubineCfg.Quality, userConcubineItem.Level + 1);
            if (sysConcubineUpgradeCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist,
                    string.Format("SysConcubineUpgradeCfg:MoneyType:{0},Quality:{1}",
                        (int)userConcubineItem.MoneyType, (int)(sysConcubineCfg.Quality)), userConcubineItem.Level + 1);
                return;
            }
            if (sysConcubineUpgradeCfg.NeedLevel > userRole.Level)
            {
                SetError(ResourceId.R_0000_UserLowLevel, sysConcubineUpgradeCfg.NeedLevel);
                return;
            }

            string errorMsg;
            //判断资源是否足够
            if (!Utility.JudgeResourceEnough(userRole, sysConcubineUpgradeCfg.NeedCoin, sysConcubineUpgradeCfg.NeedWood,
                sysConcubineUpgradeCfg.NeedStone, sysConcubineUpgradeCfg.NeedIron, out errorMsg))
            {
                SetError(errorMsg);
                return;
            }
            var opCode = Request.OpCode;
            Utility.ConsumeResource(userRole, ItemType.Coin, opCode, sysConcubineUpgradeCfg.NeedCoin);
            Utility.ConsumeResource(userRole, ItemType.Wood, opCode, sysConcubineUpgradeCfg.NeedWood);
            Utility.ConsumeResource(userRole, ItemType.Stone, opCode, sysConcubineUpgradeCfg.NeedStone);
            Utility.ConsumeResource(userRole, ItemType.Iron, opCode, sysConcubineUpgradeCfg.NeedIron);

            userConcubineItem.UpgradeEndTime = DateTime.Now.AddMinutes(sysConcubineUpgradeCfg.NeedTime);
            userConcubineItem.TrainStatus = ConcubineTrainStatus.Train;
            //userConcubineItem.TrainLocation = Location;
        }
    }
    #endregion

    #region 9024 加速妃子训练
    /// <summary>
    /// 9024 加速妃子训练
    /// </summary>
    [GameCode(OpCode = 9024)]
    public class AccUpgradeConcubineRequest : GameHandler
    {
        /// <summary>
        /// 妃子id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 训练位置
        /// </summary>
        //public int Location { get; set; }
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserConcubine userConcubine;
            Storage.Load(out userRole, out userConcubine, CurrentUserId, true);

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }
            var location = userConcubineItem.TrainLocation;
            if (location <= 0)
            {
                //妃子不在训练位置上
                SetError(ResourceId.R_9023_ConcubineNotAtTrainLocation);
                return;
            }
            if (userConcubineItem.UpgradeEndTime.ToTs() <= 0)
            {
                //已训练完无需加速
                SetError(ResourceId.R_9024_TrainOver);
                return;
            }

            var needMoney = (int)(userConcubineItem.UpgradeEndTime.ToTs() / 60.0);
            if (needMoney == 0) needMoney = 1;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.AccUpgradeConcubine);

            //加速直接加！
            //userConcubineItem.Level++;
            userConcubineItem.UpgradeEndTime = DateTime.Now;
            //userConcubineItem.TrainLocation = 0;
        }
    }
    #endregion

    #region 9025 确认训练完（非加速操作）
    /// <summary>
    /// 9025 确认训练完（非加速操作）
    /// </summary>
    [GameCode(OpCode = 9025)]
    public class ConfirmUpgradeConcubineRequest : GameHandler
    {
        /// <summary>
        /// 妃子id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 训练位置
        /// </summary>
        //public int Location { get; set; }
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserConcubine userConcubine;
            Storage.Load(out userRole, out userConcubine, CurrentUserId, true);

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }
            if (userConcubineItem.UpgradeEndTime.ToTs() > 0)
            {
                //训练未完成
                SetError(ResourceId.R_9025_TrainNotOver);
                return;
            }

            userConcubineItem.UpgradeEndTime = DateTime.Now;
            //userConcubineItem.TrainLocation = 0;
            userConcubineItem.TrainStatus = ConcubineTrainStatus.WaitTrain;
            userConcubineItem.Level++;
        }
    }
    #endregion

    #region 9026 加速妃子生产并收获资源
    /// <summary>
    /// 加速妃子生产并收获资源
    /// </summary>
    [GameCode(OpCode = 9026, ResponseType = typeof(GainResResponse))]
    public class AccAndGainResRequest : GameHandler
    {
        /// <summary>
        /// 宫殿ID
        /// </summary>
        public int BuildingId { get; set; }
        /// <summary>
        /// 妃子ID
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            ResultObj = null;
            return;
            //UserBuilding userBuilding;
            //UserConcubine userConcubine;
            //UserRole userRole;
            //Storage.Load(out userBuilding, out userConcubine, out userRole, CurrentUserId, true);

            //var userBuildingItem = userBuilding.Items.FirstOrDefault(o => o.BuildingId == BuildingId);
            //if (userBuildingItem == null)
            //{
            //    //不存在宫殿
            //    SetError(ResourceId.R_0000_IdNotExist, "UserBuilding", BuildingId);
            //    return;
            //}

            //var moneyType = userBuildingItem.SysBuildingCfg.MoneyType;
            //if (Utility.CheckResourceIsFull(userRole, moneyType))
            //{
            //    var moneyName = Utility.GetMonenyTypeName(moneyType);
            //    //资源达到上限了
            //    SetError(ResourceId.R_0000_ResourceIsFull, moneyName);
            //    return;
            //}

            //var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);

            //if (userConcubineItem == null)
            //{
            //    //妃子不存在
            //    SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
            //    return;
            //}
            //if (userConcubineItem.CanGainTime.ToTs() <= 0)
            //{
            //    //妃子无需加速收获
            //    SetError(ResourceId.R_9026_ConcubineCanNotAccGain);
            //    return;
            //}
            //var needMoney = (int)(userConcubineItem.CanGainTime.ToTs() / 60.0);
            //if (needMoney == 0) needMoney = 1;
            //if (userRole.TotalMoney < needMoney)
            //{
            //    //元宝不足
            //    SetError(ResourceId.R_0000_MoneyNotEnough);
            //    return;
            //}

            //if (userConcubineItem.Status == ConcubineStatus.Idle)
            //{
            //    //妃子正在休息
            //    SetError(ResourceId.R_9005_ConcubineAlreadyIdle);
            //    return;
            //}

            //if (!userBuildingItem.ConcubineIdList.Contains(Id))
            //{
            //    //妃子不在该宫殿
            //    SetError(ResourceId.R_9005_ConcubineNotInBuilding);
            //    return;
            //}

            ////消费
            //Utility.Concume(userRole, needMoney, SpecialStoreId.AccConcubineProduct);
            ////收获加速的时间产量
            //var accMinutes = userConcubineItem.CanGainTime.Subtract(DateTime.Now).TotalMinutes;
            //userConcubineItem.GainToOutput(Request.OpCode, accMinutes);
            ////收获已经过时间产量
            //var gainResource = userConcubineItem.GainToUser(Request.OpCode);

            ////userConcubine.ChangeNewMsg(userRole);//后宫信息

            ////返回获得的资源
            //var response = new GainResResponse(gainResource, userBuildingItem.MoneyType, userConcubineItem, userBuilding);
            //ResultObj = response;
        }
    }
    #endregion

    #region 9027 选择/更换妃子入住到训练位置
    /// <summary>
    /// 9027 选择/更换妃子入住到训练位置
    /// </summary>
    [GameCode(OpCode = 9027)]
    public class InConcubineTrainLocationRequest : GameHandler
    {
        /// <summary>
        /// 妃子id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 训练位置
        /// </summary>
        public int Location { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (Location <= 0) return false;
            return true;
        }
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserConcubine userConcubine;
            Storage.Load(out userRole, out userConcubine, CurrentUserId, true);

            var userConcubineItem = userConcubine.Items.Find(o => o.Id == Id);
            if (userConcubineItem == null)
            {
                //妃子不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", Id);
                return;
            }

            var userTrainConcubineItem = userConcubine.Items.Find(o => o.TrainLocation == Location);
            if (userTrainConcubineItem != null)
            {
                if (userTrainConcubineItem.UpgradeEndTime.ToTs() > 0)
                {
                    //训练位已经有妃子训练
                    SetError(ResourceId.R_9023_TrainLocationIsFull);
                    return;
                }
            }

            var trainConcubineCfg = ConfigHelper.TrainConcubineCfgData;
            var trainConcubineItem = trainConcubineCfg.Items.FirstOrDefault(o => o.Location == Location);
            if (trainConcubineItem == null)
            {
                //不存在训练位
                SetError(ResourceId.R_9023_TrainLocationNotExist);
                return;
            }
            if (trainConcubineItem.NeedLevel > userRole.Level || trainConcubineItem.NeedVip > userRole.RealVipLevel)
            {
                //训练位还未开启
                SetError(ResourceId.R_9023_TrainLocationNotOpen);
                return;
            }

            userConcubineItem.TrainLocation = Location;
            userConcubineItem.TrainStatus = ConcubineTrainStatus.WaitTrain;
            if (userTrainConcubineItem != null)
            {
                userTrainConcubineItem.TrainLocation = 0;
                userTrainConcubineItem.TrainStatus = ConcubineTrainStatus.Idle;
            }
        }
    }
    #endregion

    // 采花相关

    #region  9028 获得采花列表


    public class DeflowerUserListResponse
    {
        /// <summary>
        /// 总共采花次数
        /// </summary>
        [Tag(1)]
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前采花次数
        /// </summary>
        [Tag(2)]
        public int CurCount { get; set; }

        /// <summary>
        /// 重置次数
        /// </summary>
        [Tag(3)]
        public int BuyCount { get; set; }


        public class ListItem
        {
            /// <summary>
            /// 用户ID
            /// </summary>
            [Tag(1)]
            public int UserId { get; set; }

            /// <summary>
            /// 昵称
            /// </summary>
            [Tag(2)]
            public string NickName { get; set; }

            /// <summary>
            /// 妃子ID
            /// </summary>
            [Tag(3)]
            public int ConcubineId { get; set; }

            [Tag(4)]
            public int TitleLevel { get; set; }

            [Tag(5)]
            public int Level { get; set; }
        }

        /// <summary>
        /// 列表
        /// </summary>
        [Tag(4)]
        public List<ListItem> Items { get; set; }

        /// <summary>
        /// 下一次采花时间
        /// </summary>
        [Tag(5)]
        public DateTime NextTime { set; get; }
    }

    /// <summary>
    /// 获得采花列表
    /// </summary>
    [GameCode(OpCode = 9028, ResponseType = typeof(DeflowerUserListResponse))]
    public class DeflowerUserListRequest : GameHandler
    {

        /// <summary>
        /// 可忽略,为1的时候 为换一批
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int NeedFlesh { get; set; }

        public override void Process(GameContext context)
        {

            var result = new DeflowerUserListResponse();
            var userdeflower = DataStorage.Current.Load<UserDeflower>(CurrentUserId, true);

            bool value = false;
            if (NeedFlesh >= 1) value = true;

            var deflowlist = userdeflower.GetDeflowerList(value);
            var userlist = DataStorage.Current.LoadList<UserRole>(deflowlist.ToArray());
            var conlist = DataStorage.Current.LoadList<UserConcubine>(deflowlist.ToArray());

            result.Items = new List<DeflowerUserListResponse.ListItem>();
            foreach (var userConcubine in conlist)
            {
                var item = new DeflowerUserListResponse.ListItem();
                item.UserId = userConcubine.Id;
                item.NickName = userlist.Find(o => o.Id == userConcubine.Id).NickName;
                //var concubine = userConcubine.Items.OrderByDescending(o => o.Favor).ToList().FirstOrDefault();
                //modify by hql at 2015.10.10 好感度需要加上碎片换算过去的好感度
                var concubine = userConcubine.Items.OrderByDescending(o => o.TotalFavor).ToList().FirstOrDefault();
                if (concubine != null && concubine.SysConcubineCfg != null)
                {
                    item.ConcubineId = concubine.SysConcubineCfg.Id;
                    item.TitleLevel = concubine.TitleLevel;
                    item.Level = concubine.Level;
                    result.Items.Add(item);
                }

            }

            result.CurCount = (int)userdeflower.UseNum.Value;
            result.TotalCount = ConfigHelper.DeflowCfgData.Count;
            result.BuyCount = (int)userdeflower.BuyNum.Value;
            result.NextTime = userdeflower.NextDeflowerTime;
            ResultObj = result;
        }
    }

    #endregion

    #region 9029 进入采花

    /// <summary>
    /// 获得宫殿信息
    /// </summary>
    [GameCode(OpCode = 9029, ResponseType = typeof(GetOtherHouGongInfoResponse))]
    public class GetOtherHouGongInfoFromDeflowerRequest : GameHandler
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
        public override void Process(GameContext context)
        {




            var userdeflower = DataStorage.Current.Load<UserDeflower>(CurrentUserId, true);
            var deflowlist = userdeflower.GetDeflowerList();
            if (!deflowlist.Contains(UserId))
            {
                SetError(ResourceId.R_0000_IdNotExist, UserId);
                return;
            }
            if (ConfigHelper.DeflowCfgData.Count - (int)userdeflower.UseNum.Value <= 0)
            {
                SetError(ResourceId.R_9019_FlowersNumIsOver);
                return;
            }
            if (DateTime.Now < userdeflower.NextDeflowerTime)
            {
                SetError(ResourceId.R_7000_HaveCoolTimeCannotBuy);
                return;
            }

            userdeflower.UseNum += 1;
            userdeflower.NextDeflowerTime = DateTime.Now.AddSeconds(ConfigHelper.DeflowCfgData.CoolTime);
            userdeflower.TempDeflowerUser = UserId;
            userdeflower.TempConcubineList.Clear();

            //TODO 返回列表

            UserConcubine defenderUserConcubine;
            UserBuilding defenderUserBuilding;
            UserRole defenderUserRole;
            Storage.Load(out defenderUserConcubine, out defenderUserBuilding, out defenderUserRole, UserId);

            var response = new GetOtherHouGongInfoResponse();
            foreach (var userBuildingItem in defenderUserBuilding.Items)
            {
                var item = new GetOtherHouGongInfoResponse.ResBuildingItem();
                item.BuildingId = userBuildingItem.BuildingId;
                item.Level = userBuildingItem.Level;
                foreach (var i in userBuildingItem.ConcubineIdList)
                {
                    var concubineItem = defenderUserConcubine.Items.FirstOrDefault(o => o.Id == i);
                    if (concubineItem != null)
                        item.ConcubineItems.Add(new GetOtherHouGongInfoResponse.ResConcubineItem() { Id = i, SysConcubineId = concubineItem.ConcubineId });
                }

                response.Items.Add(item);
            }

            //添加采花次数——一次战斗就计算一次采花。
            Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.Deflower);

            var userInfo = new GetOtherHouGongInfoResponse.ResUserInfo();
            userInfo.HeadId = defenderUserRole.HeadId;
            userInfo.Level = defenderUserRole.Level;
            userInfo.VipLevel = defenderUserRole.VipLevel;
            userInfo.NickName = defenderUserRole.NickName;
            response.UserInfo = userInfo;
            ResultObj = response;
        }
    }



    #endregion

    #region 9030 返回采花结果

    /// <summary>
    /// 返回采花结果
    /// </summary>
    [GameCode(OpCode = 9030, ResponseType = typeof(FlowersResponse))]
    public class DeFlowersRequest : GameHandler
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 妃子ID
        /// </summary>
        public int ConcubineId { get; set; }

        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserDeflower userdeflower;
            DataStorage.Current.Load(out userRole, out userdeflower, CurrentUserId, true);

            if (UserId != userdeflower.TempDeflowerUser)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserRole:Id", UserId);
                return;
            }
            UserRole defenderUserRole;
            UserConcubine defenderUserConcubine;
            UserDeflower defenderUserDeflower;
            Storage.Load(out defenderUserRole, out defenderUserConcubine, out defenderUserDeflower, UserId, true);

            var response = new FlowersResponse();
            var userConcubineItem = defenderUserConcubine.Items.FirstOrDefault(o => o.Id == ConcubineId);
            if (userConcubineItem == null)
            {
                //找不到妃子
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubine:Id", ConcubineId);
                return;
            }

            var initSuccessPrec = 30;//初始的成功率
            var eachCharm = 100;//每多100魅力多1点的成功率
            var eachAddSuccessPrec = 1;
            var difCharm = userRole.Charm - defenderUserRole.Charm;
            var charmAddSuccessPrec = difCharm > 0 ? (int)(difCharm * 1.0 / eachCharm) * eachAddSuccessPrec : 0;

            var successPrec = initSuccessPrec + charmAddSuccessPrec;
            var result = Util.IsHit(successPrec * 1.0 / 100);
            if (result)
            {
                response.IsSuccess = 1;
                //采花成功
                //1、减少妃子好感度
                var reduceFavor = 10;
                userConcubineItem.Favor = userConcubineItem.Favor > reduceFavor
                    ? userConcubineItem.Favor - reduceFavor
                    : 0;

                //2、赠送道具给抢劫者
                var toolId = 13000000;
                var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == toolId);
                if (sysToolCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", toolId);
                    return;
                }
                sysToolCfg.AddToUser(CurrentUserId, Request.OpCode);
                response.ItemPairList.Add(new ItemPair() { ItemId = toolId, Num = 1 });

                //3、获得魅力
                var addCharm = 10;
                Utility.AddResource(userRole, ItemType.Charm, Request.OpCode, addCharm);
                response.ItemPairList.Add(new ItemPair() { ItemId = (int)SpecialToolId.Charm, Num = addCharm });
                userdeflower.TempValueget += addCharm;


                //4、几率获得妃子碎片
                var probabilityList = new List<int>() { 40, 35, 30, 25, 20 };//白色、绿色、蓝色、紫色、黄色
                var sysConcubineCfg = SysConcubineCfg.Items.Find(o => o.Id == userConcubineItem.ConcubineId);
                if (sysConcubineCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysConcubineCfg:Id", userConcubineItem.ConcubineId);
                    return;
                }
                var probability = 0;
                if ((int)sysConcubineCfg.Quality > (probabilityList.Count - 1))
                    probability = probabilityList[probabilityList.Count - 1];
                else probability = probabilityList[(int)sysConcubineCfg.Quality];
                result = Util.IsHit(probability * 1.0 / 100);
                if (result)
                {
                    //获得了妃子碎片
                    //妃子碎片ID
                    var concubineChipId = sysConcubineCfg.Id + (int)ToolType.ConcubineChip;
                    sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == concubineChipId);
                    if (sysToolCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", concubineChipId);
                        return;
                    }
                    sysToolCfg.AddToUser(CurrentUserId, Request.OpCode);
                    response.ItemPairList.Add(new ItemPair() { ItemId = concubineChipId, Num = 1 });
                }

                userdeflower.TempConcubineList.Add(userConcubineItem.ConcubineId);

            }
            else
            {
                //失败了下发记录
                var endresuqes = new EndDeflowerReques();
                endresuqes.Process(context);
            }
            ResultObj = response;

        }
    }


    #endregion

    #region 9031 主动终止采花

    /// <summary>
    /// 终止采花没有失败的情况下退出界面时候调用,失败的时候自动就会调用
    /// </summary>
    [GameCode(OpCode = 9031)]
    public class EndDeflowerReques : GameHandler
    {
        public override void Process(GameContext context)
        {
            var userdeflower = DataStorage.Current.Load<UserDeflower>(CurrentUserId, true);
            if (userdeflower.TempDeflowerUser != 0)
            {
                var deuserflower = DataStorage.Current.Load<UserDeflower>(userdeflower.TempDeflowerUser, true);

                if (userdeflower.TempConcubineList.Count > 0)
                {
                    var item = new DeflowerRecord();
                    item.Type = DeflowerType.DeflowerOther;
                    item.UserId = userdeflower.TempDeflowerUser;
                    foreach (var id in userdeflower.TempConcubineList)
                    {
                        item.ConcubineIds.Add(id);
                    }
                    item.Valueget = userdeflower.TempValueget;
                    userdeflower.DeflowerList.Insert(0, item);


                    var sitem = new DeflowerRecord();
                    sitem.Type = DeflowerType.OtherDeflower;
                    sitem.UserId = CurrentUserId;
                    foreach (var id in userdeflower.TempConcubineList)
                    {
                        sitem.ConcubineIds.Add(id);
                    }

                    deuserflower.DeflowerList.Insert(0, sitem);
                }

                userdeflower.TempDeflowerUser = 0;
                userdeflower.TempValueget = 0;
                userdeflower.TempConcubineList.Clear();

                userdeflower.GetDeflowerList(true);
            }



        }
    }


    #endregion

    #region 9032 获取采花记录

    public class DeflowerRecordResponse
    {

        public class DeflowerRecordItem
        {
            [Tag(1)]
            public DeflowerType Type { get; set; }

            [Tag(2)]
            public int UserId { get; set; }

            [Tag(3)]
            public string NickName { get; set; }

            [Tag(4)]
            public List<int> Concubines { get; set; }

            [Tag(5)]
            public DateTime Time { get; set; }

            /// <summary>
            /// 获得的魅力
            /// </summary>
            [Tag(6)]
            public int Valueget { get; set; }

            public DeflowerRecordItem()
            {
                Concubines = new List<int>();
            }
        }

        [Tag(1)]
        public List<DeflowerRecordItem> Items { get; set; }

        public DeflowerRecordResponse()
        {
            Items = new List<DeflowerRecordItem>();
        }

    }

    /// <summary>
    /// 采花记录
    /// </summary>
    [GameCode(OpCode = 9032, ResponseType = typeof(DeflowerRecordResponse))]
    public class DeflowerRecordList : GameHandler
    {
        public override void Process(GameContext context)
        {
            var userdeflower = DataStorage.Current.Load<UserDeflower>(CurrentUserId, true);
            var ids = userdeflower.DeflowerList.Select(o => o.UserId).ToList();
            var res = new DeflowerRecordResponse();
            if (ids.Count > 0)
            {
                var userRoles = DataStorage.Current.LoadList<UserRole>(ids.ToArray(), true);
                foreach (var item in userdeflower.DeflowerList)
                {
                    var sitem = new DeflowerRecordResponse.DeflowerRecordItem();
                    sitem.Type = item.Type;
                    sitem.UserId = item.UserId;
                    sitem.NickName = userRoles.Find(o => o.Id == item.UserId).NickName;
                    sitem.Concubines = item.ConcubineIds;
                    sitem.Time = item.Time;
                    sitem.Valueget = item.Valueget;
                    res.Items.Add(sitem);
                }

            }
            ResultObj = res;
        }
    }
    #endregion

    #region 9033 清除CD

    /// <summary>
    /// 清除采花CD
    /// </summary>
    [GameCode(OpCode = 9033)]
    public class DeflowerCDRequeset : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserDeflower userDeflower;
            DataStorage.Current.Load(out userRole, out userDeflower, CurrentUserId, true);


            var needMoney = (int)(userDeflower.NextDeflowerTime.ToTs() / 60.0);
            if (needMoney == 0) needMoney = 1;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.AccUpgradeConcubine);
            userDeflower.NextDeflowerTime = DateTime.Now;

        }
    }


    #endregion

    #region 9034 重置采花次数
    /// <summary>
    /// 重置采花次数
    /// </summary>
    [GameCode(OpCode = 9034)]
    public class ResetDeflowerCountRequeset : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserDeflower userDeflower;
            DataStorage.Current.Load(out userRole, out userDeflower, CurrentUserId, true);

            //TODO 扣钱
            var vipcfg = SysVipCfg.Items.Find(o => o.VipLevel == userRole.VipLevel);
            if (userDeflower.BuyNum.Value >= vipcfg.FlowersNum)
            {
                SetError(ResourceId.R_0000_NoBuyNum);
                return;
            }
            var num = vipcfg.FlowersNum;
            var needmoney = SysBuyNumCfg.Items.Find(o => o.Id == num).DeflowerMoney;
            if (userRole.TotalMoney < needmoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needmoney, SpecialStoreId.BuyDeflowerNum);//需要写入log 重置采花次数 modify by hql at 2015.10.18

            userDeflower.BuyNum += 1;
            userDeflower.UseNum -= userDeflower.UseNum.Value;
        }
    }
    #endregion


    #region 9035 获取单个妃子信息
    /// <summary>
    /// 9035 获取单个妃子信息
    /// </summary>
    [GameCode(OpCode = 9035, ResponseType = typeof(GetHouGongInfoResponse.ResConcubineItem))]
    public class GetConcubineInfoRequest : GameHandler
    {
        /// <summary>
        /// 妃子id
        /// </summary>
        public int Id { get; set; }
        public override void Process(GameContext context)
        {
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            Storage.Load(out userBuilding, out userConcubine, CurrentUserId, true);

            var response = new GetHouGongInfoResponse.ResConcubineItem();
            var userConcubineItem = userConcubine.Items.FirstOrDefault(O => O.ConcubineId == Id);
            if (userConcubineItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserConcubineItem:Id", Id);
                return;
            }

            var mapper =
               ObjectMapperManager.DefaultInstance.GetMapper<UserConcubineItem, GetHouGongInfoResponse.ResConcubineItem>();
            mapper.Map(userConcubineItem, response);
            response.Product = userConcubineItem.RealProduct;
            response.Favor = userConcubineItem.TotalFavor;

            ResultObj = response;
        }
    }
    #endregion
}
