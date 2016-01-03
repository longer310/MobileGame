using System.Collections.Generic;
using System.Linq;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;

namespace MobileGame.tianzi.Repository
{
    #region 获取用户背包道具物品 4000
    public class GetUserToolResponse
    {
        public GetUserToolResponse()
        {
            Items = new List<UserToolItem>();
        }
        /// <summary>
        /// 道具列表
        /// </summary>
        [Tag(1)]
        public List<UserToolItem> Items { get; set; }

        /// <summary>
        /// 道具容量
        /// </summary>
        [Tag(2)]
        public int Capacity { get; set; }
    }
    /// <summary>
    /// 获取用户背包道具物品
    /// </summary>
    [GameCode(OpCode = 4000, ResponseType = typeof(GetUserToolResponse))]
    public class GetUserToolRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetUserToolResponse();

            var userTool = Storage.Load<UserTool>(CurrentUserId);
            response.Items = userTool.Items.Where(o => o.Num > 0).OrderByDescending(o => o.Quality).ToList();
            response.Capacity = userTool.Capacity;

            ResultObj = response;
        }
    }
    #endregion

    #region 获取用户背包装备物品 4001
    public class GetUserEquipResponse
    {
        public GetUserEquipResponse()
        {
            Items = new List<ReqUserEquipItem>();
        }
        /// <summary>
        /// 装备列表
        /// </summary>
        [Tag(1)]
        public List<ReqUserEquipItem> Items { get; set; }

        /// <summary>
        /// 装备容量
        /// </summary>
        [Tag(2)]
        public int Capacity { get; set; }

        /// <summary>
        /// 进阶石数量
        /// </summary>
        //[Tag(3)]
        //public int AdvancedStone { get; set; }

        public class ReqUserEquipItem
        {
            public ReqUserEquipItem()
            {
            }
            /// <summary>
            /// Id（用于使用、出售传参）
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 物品Id（用于获取装备的详细信息）
            /// </summary>
            [Tag(2)]
            public int ItemId { get; set; }
            /// <summary>
            /// 等级
            /// </summary>
            [Tag(3)]
            public int Level { get; set; }
            /// <summary>
            /// 系统武将id 
            /// </summary>
            [Tag(4)]
            public int HeroId { get; set; }
            /// <summary>
            /// 星级
            /// </summary>
            [Tag(5)]
            public int Star { get; set; }
            /// <summary>
            /// 星级经验 
            /// </summary>
            [Tag(6)]
            public int Exp { get; set; }
        }
    }
    /// <summary>
    /// 获取用户背包装备物品
    /// </summary>
    [GameCode(OpCode = 4001, ResponseType = typeof(GetUserEquipResponse))]
    public class GetUserEquipRequest : GameHandler
    {
        /// <summary>
        /// 类型【可不传，1：铁匠铺】
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int Type { get; set; }
        /// <summary>
        /// 装备类型【可不传，点击武将的装备格子的时候传】
        /// </summary>
        [ParamCheck(Ignore = true)]
        public EquipType EquipType { get; set; }
        public override void Process(GameContext context)
        {
            var response = new GetUserEquipResponse();

            var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);

            var equipList = userEquip.Items.Where(o => o.HeroId == 0).OrderBy(o => o.Quality).ToList();
            if (Type == 1)
            {
                var needQuality = ConfigHelper.EquipCfgData.NeedQuality;
                equipList =
                userEquip.Items.Where(o => o.Quality > needQuality) //未佩戴并且品质大于等于3
                    .OrderByDescending(o => o.Quality)      //品质高
                    //.ThenByDescending(o => o.Rank)          //进阶级高
                    .ThenByDescending(o => o.Level)         //强化等级高
                    .ToList();
            }
            else if (EquipType != EquipType.None)
            {
                equipList = equipList.Where(o => o.EquipType == EquipType)
                    .OrderByDescending(o => o.Quality)                //品质高
                    .ThenByDescending(o => o.Level)         //强化等级高
                    .ThenByDescending(o => o.NeedLevel)       //需要武将等级高
                    .ToList();
            }
            else
            {
                //背包中不下发已装备的装备
                equipList = equipList.Where(o => o.HeroId == 0).ToList();
            }

            //所有装备一次性下发
            foreach (var userEquipItem in equipList)
            {
                var item = new GetUserEquipResponse.ReqUserEquipItem();
                var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserEquipItem, GetUserEquipResponse.ReqUserEquipItem>();
                mapper.Map(userEquipItem, item);

                response.Items.Add(item);
            }
            response.Capacity = userEquip.Capacity;

            ResultObj = response;
        }
    }
    #endregion

    #region 获取用户背包装备&武将&妃子碎片物品 4002
    public class GetUserChipResponse
    {
        public GetUserChipResponse()
        {
            Items = new List<ChipItem>();
        }
        /// <summary>
        /// 碎片列表
        /// </summary>
        [Tag(1)]
        public List<ChipItem> Items { get; set; }
    }
    /// <summary>
    /// 获取用户背包装备&武将&妃子碎片物品
    /// </summary>
    [GameCode(OpCode = 4002, ResponseType = typeof(GetUserChipResponse))]
    public class GetUserEquipChipRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetUserChipResponse();

            var userChip = Storage.Load<UserChip>(CurrentUserId);
            response.Items = userChip.ChipItems.Where(o => o.Num > 0).ToList();

            ResultObj = response;
        }
    }
    #endregion

    #region 出售背包物品 4003
    public class BuyBagItemsResponse
    {
        public int TotalPrice { get; set; }
    }
    /// <summary>
    /// 出售背包物品
    /// </summary>
    [GameCode(OpCode = 4003, ResponseType = typeof(BuyBagItemsResponse))]
    public class BuyBagItemsRequest : GameHandler
    {
        public BagItemType Type { get; set; }
        /// <summary>
        /// 背包物品Id列表
        /// </summary>
        public string BagItemIdArray { get; set; }
        /// <summary>
        /// 背包物品Id列表
        /// </summary>
        public List<int> BagItemIdList;
        /// <summary>
        /// 背包物品数量列表
        /// </summary>
        public string BagItemNumIdArray { get; set; }
        /// <summary>
        /// 背包物品数量列表
        /// </summary>
        public List<int> BagItemNumIdList;

        public override bool InitParams(GameContext context)
        {
            if (Type == BagItemType.All) return false;
            if (BagItemIdList.Count == 0) return false;
            if (BagItemNumIdList.Count == 0) return false;
            if (BagItemNumIdList.Count(o => o <= 0) > 0) return false;
            if (BagItemNumIdList.Count != BagItemIdList.Count) return false;
            return true;
        }

        public override void Process(GameContext context)
        {
            var response = new BuyBagItemsResponse();
            var index = 0;

            var logItem = new GameLogItem();
            logItem.S1 = "";
            logItem.S2 = "";

            var opCode = Request.OpCode;

            if (Type == BagItemType.Tool)
            {
                var userTool = Storage.Load<UserTool>(CurrentUserId, true);
                foreach (var i in BagItemIdList)
                {
                    var num = BagItemNumIdList[index];
                    var toolItem = userTool.Items.FirstOrDefault(o => o.Id == i);
                    if (toolItem == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "UserToolItem", i);
                        return;
                    }
                    var sysToolCfg = SysToolCfg.Find(toolItem.ItemId);
                    if (sysToolCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", toolItem.ItemId);
                        return;
                    }
                    if (!userTool.RemoveTool(toolItem.ItemId, num, opCode))
                    {
                        SetError(ResourceId.R_0000_IllegalParam);
                        return;
                    }
                    response.TotalPrice += sysToolCfg.SellPrice * num;
                    index++;

                    logItem.S1 += toolItem.ItemId + ",";
                    logItem.S2 += num + ",";
                }
            }
            else if (Type == BagItemType.Equip)
            {
                var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);
                foreach (var i in BagItemIdList)
                {
                    var equipItem = userEquip.Items.FirstOrDefault(o => o.Id == i);
                    if (equipItem != null)
                    {
                        if (equipItem.HeroId > 0)
                        {
                            SetError(ResourceId.R_4003_CanNotSellEquipedEquip);
                            return;
                        }
                        userEquip.RemoveEquip(equipItem, Request.OpCode);

                        response.TotalPrice += equipItem.SysEquipCfg.SellPrice * equipItem.Level;

                        logItem.S1 += equipItem.ItemId + ",";
                        logItem.S2 += 1 + ",";
                    }
                }
            }
            else if (Type == BagItemType.Chip)
            {
                var userChip = Storage.Load<UserChip>(CurrentUserId, true);
                foreach (var i in BagItemIdList)
                {
                    var chipItem = userChip.ChipItems.FirstOrDefault(o => o.ItemId == i);
                    var num = BagItemNumIdList[index];
                    if (chipItem != null)
                    {
                        var sysToolCfg = SysToolCfg.Find(chipItem.ItemId);
                        if (sysToolCfg == null)
                        {
                            SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", chipItem.ItemId);
                            return;
                        }
                        if (!userChip.SubChip(chipItem.ItemId, num, ToolType.None))
                        {
                            SetError(ResourceId.R_0000_IllegalParam);
                            return;
                        }

                        response.TotalPrice += sysToolCfg.SellPrice * num;
                        index++;

                        logItem.S1 += chipItem.ItemId + ",";
                        logItem.S2 += num + ",";
                    }
                }
            }
            else if (Type == BagItemType.Pet)
            {
                var userPet = Storage.Load<UserPet>(CurrentUserId, true);
                foreach (var i in BagItemIdList)
                {
                    var petItem = userPet.Items.FirstOrDefault(o => o.Id == i);
                    if (petItem != null)
                    {
                        if (petItem.HeroId > 0)
                        {
                            //不能出售已佩戴的骑宠
                            SetError(ResourceId.R_4003_CanNotSellEquipedPet);
                            return;
                        }
                        userPet.RemovePet(petItem, opCode);

                        response.TotalPrice += petItem.SysPetCfg.SellPrice * petItem.Level;

                        logItem.S1 += petItem.PetId + ",";
                        logItem.S2 += 1 + ",";
                    }
                }
            }

            logItem.F1 = (int)Type;
            logItem.F2 = response.TotalPrice;
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            logItem.F3 = userRole.Coin;

            //添加铜钱
            Utility.AddResource(userRole, ItemType.Coin, opCode, response.TotalPrice);

            logItem.F4 = userRole.Coin;
            GameLogManager.CommonLog(opCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion

    #region 碎片合成装备/骑宠/妃子/武将 4004
    public class MixtureResponse
    {
        /// <summary>
        /// Id（用于使用、出售传参）
        /// </summary>
        [Tag(1)]
        public int Id { get; set; }
        /// <summary>
        /// 物品Id（用于获取装备/骑宠/妃子/武将的详细信息）
        /// </summary>
        [Tag(2)]
        public int ItemId { get; set; }
    }
    /// <summary>
    /// 碎片合成装备/骑宠/妃子/武将
    /// </summary>
    [GameCode(OpCode = 4004, ResponseType = typeof(MixtureResponse))]
    public class MixtureRequest : GameHandler
    {
        /// <summary>
        /// 背包碎片id
        /// </summary>
        public int ChipId { get; set; }

        public override void Process(GameContext context)
        {
            var response = new MixtureResponse();

            UserChip userChip;
            UserRole userRole;
            Storage.Load(out userChip, out userRole, CurrentUserId, true);

            var chipItem = userChip.ChipItems.FirstOrDefault(o => o.ItemId == ChipId);
            if (chipItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "userChip.EquipChipItems", ChipId);
                return;
            }
            //检查物品是否存在
            var sysToolCfg = SysToolCfg.Find(chipItem.ItemId);//Item.Find(chipItem.ItemId);
            if (sysToolCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", chipItem.ItemId);
                return;
            }
            var toolType = sysToolCfg.GetToolType();
            if (toolType != ToolType.HeroChip && toolType != ToolType.ConcubineChip &&
                toolType != ToolType.EquipChip && toolType != ToolType.PetChip)
            {
                SetError(ResourceId.R_4004_NotChip);
                return;
            }
            //Param1 装备/骑宠/妃子/武将id
            if (toolType == ToolType.EquipChip)
            {
                var sysEquipCfg = SysEquipCfg.Find(sysToolCfg.Param1);
                if (sysEquipCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysEquipCfg", sysToolCfg.Param1);
                    return;
                }
            }
            else if (toolType == ToolType.ConcubineChip)
            {
                var sysConcubineCfg = SysConcubineCfg.Find(sysToolCfg.Param1);
                if (sysConcubineCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysConcubineCfg", sysToolCfg.Param1);
                    return;
                }
                if (sysConcubineCfg.VisitNeedCharm > userRole.Charm)
                {
                    //魅力不足
                    SetError(ResourceId.R_0000_CharmNotEnough);
                    return;
                }
            }
            else if (toolType == ToolType.HeroChip)
            {
                var sysHeroCfg = SysHeroCfg.Find(sysToolCfg.Param1);
                if (sysHeroCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysHeroCfg", sysToolCfg.Param1);
                    return;
                }
                if (sysHeroCfg.VisitNeedRepute > userRole.Repute)
                {
                    //声望不足
                    SetError(ResourceId.R_0000_ReputeNotEnough);
                    return;
                }
            }
            else if (toolType == ToolType.PetChip)
            {
                var sysPetCfg = SysPetCfg.Find(sysToolCfg.Param1);
                if (sysPetCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysPetCfg", sysToolCfg.Param1);
                    return;
                }
            }

            //Param2 合成装备/骑宠/妃子/武将所需的碎片数量
            if (sysToolCfg.Param2 > chipItem.Num)
            {
                SetError(ResourceId.R_4004_ChipNotEnough);
                return;
            }

            if (toolType == ToolType.EquipChip)
            {
                var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);

                if (userEquip.IsFull)
                {
                    //背包已满
                    SetError(ResourceId.R_0000_EquipBagIsFull);
                    return;
                }
                var equipItem = userEquip.AddEquipToUser(sysToolCfg.Param1, Request.OpCode);

                response.Id = equipItem.Id;
                response.ItemId = equipItem.ItemId;
            }
            else if (toolType == ToolType.ConcubineChip)
            {
                var userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                //TODO是否需要判断容量

                var concubineItem = userConcubine.AddConcubineToUser(sysToolCfg.Param1, Request.OpCode);
                if (concubineItem == null)
                {
                    //该妃子已存在
                    SetError(ResourceId.R_4004_ConcubineIsExist);
                    return;
                }

                response.Id = concubineItem.Id;
                response.ItemId = concubineItem.ConcubineId;
            }
            else if (toolType == ToolType.HeroChip)
            {
                var userHero = Storage.Load<UserHero>(CurrentUserId, true);
                //TODO是否需要判断容量

                var heroItem = userHero.AddHeroToUser(sysToolCfg.Param1, Request.OpCode);
                if (heroItem == null)
                {
                    //该武将已存在
                    SetError(ResourceId.R_4004_HeroIsExist);
                    return;
                }

                response.Id = heroItem.Id;
                response.ItemId = heroItem.HeroId;
            }
            else if (toolType == ToolType.PetChip)
            {
                var userPet = Storage.Load<UserPet>(CurrentUserId, true);

                var petItem = userPet.AddPetToUser(sysToolCfg.Param1, Request.OpCode);
                if (petItem == null)
                {
                    //该骑宠容量已满
                    SetError(ResourceId.R_0000_PetBagIsFull);
                    return;
                }

                response.Id = petItem.Id;
                response.ItemId = petItem.PetId;
            }

            var logItem = new GameLogItem();
            logItem.F3 = chipItem.Num;

            //减去碎片
            userChip.SubChip(ChipId, sysToolCfg.Param2, ToolType.None);

            logItem.F4 = chipItem.Num;


            logItem.F1 = ChipId;
            logItem.F2 = sysToolCfg.Param2;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion

    #region 获取用户骑宠列表 4005
    public class GetUserPetResponse
    {
        public GetUserPetResponse()
        {
            Items = new List<ReqUserPetItem>();
        }
        /// <summary>
        /// 骑宠列表
        /// </summary>
        [Tag(1)]
        public List<ReqUserPetItem> Items { get; set; }

        /// <summary>
        /// 骑宠容量
        /// </summary>
        [Tag(2)]
        public int Capacity { get; set; }

        public class ReqUserPetItem
        {
            public ReqUserPetItem()
            {
            }
            /// <summary>
            /// Id（用于出售传参）
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 骑宠Id（用于获取骑宠的详细信息）
            /// </summary>
            [Tag(2)]
            public int PetId { get; set; }
            /// <summary>
            /// 骑宠等级【暂时不显示】
            /// </summary>
            [Tag(3)]
            public int Level { get; set; }
            /// <summary>
            /// 系统武将id 【可获取武将名称】
            /// </summary>
            [Tag(4)]
            public int HeroId { get; set; }
        }
    }
    /// <summary>
    /// 获取用户骑宠列表
    /// </summary>
    [GameCode(OpCode = 4005, ResponseType = typeof(GetUserPetResponse))]
    public class GetUserPetRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetUserPetResponse();

            var userPet = Storage.Load<UserPet>(CurrentUserId);
            foreach (var userPetItem in userPet.Items.Where(o => o.HeroId == 0).ToList())
            {
                var item = new GetUserPetResponse.ReqUserPetItem();
                var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserPetItem, GetUserPetResponse.ReqUserPetItem>();
                mapper.Map(userPetItem, item);

                response.Items.Add(item);
            }
            response.Capacity = userPet.Capacity;

            ResultObj = response;
        }
    }
    #endregion

    #region 获取背包所有物品 4006
    public class GetBagResponse
    {
        public GetBagResponse()
        {
            ToolItems = new List<UserToolItem>();
            EquipItems = new List<ReqUserEquipItem>();
            ChipItems = new List<ChipItem>();
            PetItems = new List<ReqUserPetItem>();
        }
        /// <summary>
        /// 道具列表
        /// </summary>
        [Tag(1)]
        public List<UserToolItem> ToolItems { get; set; }

        /// <summary>
        /// 道具容量
        /// </summary>
        [Tag(2)]
        public int ToolCapacity { get; set; }

        /// <summary>
        /// 装备【无佩戴】列表
        /// </summary>
        [Tag(3)]
        public List<ReqUserEquipItem> EquipItems { get; set; }

        /// <summary>
        /// 装备容量
        /// </summary>
        [Tag(4)]
        public int EquipCapacity { get; set; }

        /// <summary>
        /// 碎片列表
        /// </summary>
        [Tag(5)]
        public List<ChipItem> ChipItems { get; set; }

        /// <summary>
        /// 骑宠列表
        /// </summary>
        [Tag(6)]
        public List<ReqUserPetItem> PetItems { get; set; }

        /// <summary>
        /// 骑宠容量
        /// </summary>
        [Tag(7)]
        public int PetCapacity { get; set; }

        public class ReqUserEquipItem
        {
            public ReqUserEquipItem()
            {
            }
            /// <summary>
            /// Id（用于使用、出售传参）
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 物品Id（用于获取装备的详细信息）
            /// </summary>
            [Tag(2)]
            public int ItemId { get; set; }
            /// <summary>
            /// 等级
            /// </summary>
            [Tag(3)]
            public int Level { get; set; }
            /// <summary>
            /// 系统武将id 
            /// </summary>
            [Tag(4)]
            public int HeroId { get; set; }
            /// <summary>
            /// 星级
            /// </summary>
            [Tag(5)]
            public int Star { get; set; }
            /// <summary>
            /// 星级经验 
            /// </summary>
            [Tag(6)]
            public int Exp { get; set; }
        }
        public class ReqUserPetItem
        {
            public ReqUserPetItem()
            {
            }
            /// <summary>
            /// Id（用于出售传参）
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 骑宠Id（用于获取骑宠的详细信息）
            /// </summary>
            [Tag(2)]
            public int PetId { get; set; }
            /// <summary>
            /// 骑宠等级【暂时不显示】
            /// </summary>
            [Tag(3)]
            public int Level { get; set; }
            /// <summary>
            /// 系统武将id 【可获取武将名称】
            /// </summary>
            [Tag(4)]
            public int HeroId { get; set; }
        }
    }
    /// <summary>
    /// 获取用户背包道具物品
    /// </summary>
    [GameCode(OpCode = 4006, ResponseType = typeof(GetBagResponse))]
    public class GetBagRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetBagResponse();

            UserTool userTool;
            UserEquip userEquip;
            UserChip userChip;
            UserPet userPet;
            Storage.Load(out userTool, out userEquip, out userChip, out userPet, CurrentUserId);

            //道具列表
            response.ToolItems = userTool.Items.Where(o => o.Num > 0).OrderByDescending(o => o.Quality).ToList();
            response.ToolCapacity = userTool.Capacity;

            //装备列表
            var equipList = userEquip.Items.OrderBy(o => o.Quality).ToList();
            //下发没有佩戴的装备
            equipList = equipList.Where(o => o.HeroId == 0).ToList();

            foreach (var userEquipItem in equipList.OrderByDescending(o => o.Quality).ThenByDescending(o => o.Level).ToList())
            {
                var item = new GetBagResponse.ReqUserEquipItem();
                var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserEquipItem, GetBagResponse.ReqUserEquipItem>();
                mapper.Map(userEquipItem, item);

                response.EquipItems.Add(item);
            }
            response.EquipCapacity = userEquip.Capacity;

            //碎片列表
            response.ChipItems = userChip.ChipItems.Where(o => o.Num > 0).OrderBy(o => o.ItemId).ToList();

            //骑宠列表
            foreach (var userPetItem in userPet.Items.Where(o => o.HeroId == 0).OrderByDescending(o => o.SysPetCfg.Quality).
                ThenByDescending(o => o.Level).ToList())
            {
                var item = new GetBagResponse.ReqUserPetItem();
                var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserPetItem, GetBagResponse.ReqUserPetItem>();
                mapper.Map(userPetItem, item);

                response.PetItems.Add(item);
            }
            response.PetCapacity = userPet.Capacity;

            ResultObj = response;
        }
    }
    #endregion

    #region 使用背包物品 4007
    public class UseBagToolResponse
    {
        public UseBagToolResponse()
        {
            ResItems = new List<ItemPair>();
        }
        /// <summary>
        /// 获得的资源列表[铜钱、木材、石头、铁矿、体力]
        /// </summary>
        [Tag(1)]
        public List<ItemPair> ResItems { get; set; }
    }
    /// <summary>
    /// 获取用户背包道具物品
    /// </summary>
    [GameCode(OpCode = 4007, ResponseType = typeof(UseBagToolResponse))]
    public class UseBagToolRequest : GameHandler
    {
        /// <summary>
        /// 道具系统ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 道具个数,可不传默认1
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int Num { get; set; }
        public override void Process(GameContext context)
        {
            var response = new UseBagToolResponse();
            if (Num == 0) Num = 1;

            UserTool userTool;
            UserRole userRole;
            Storage.Load(out userRole, out userTool, CurrentUserId, true);

            var userToolItem = userTool.Items.FirstOrDefault(o => o.ItemId == ItemId);
            if (userToolItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserTool:ItemId", ItemId);
                return;
            }

            if (userToolItem.Num < Num)
            {
                SetError(ResourceId.R_0000_ToolNotEnough);
                return;
            }

            var sysToolCfg = SysToolCfg.Find(o => o.Id == ItemId);
            if (sysToolCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", ItemId);
                return;
            }

            var opCode = Request.OpCode;
            response.ResItems = sysToolCfg.UseToUser(opCode, userRole, Num);
            //var toolType = sysToolCfg.GetToolType();
            //switch (toolType)
            //{
            //    case ToolType.CoinTool:
            //        {
            //            var moneyTypeList = new List<MoneyType>() { MoneyType.Coin, MoneyType.Iron, MoneyType.Stone, MoneyType.Wood };
            //            foreach (var moneyType in moneyTypeList)
            //            {
            //                if (Utility.CheckResourceIsFull(userRole, moneyType))
            //                {
            //                    var moneyName = Utility.GetMonenyTypeName(moneyType);
            //                    //资源达到上限了
            //                    SetError(ResourceId.R_0000_ResourceIsFull, moneyName);
            //                    return;
            //                }
            //            }

            //            response.ResItems.Add(new ItemPair()
            //            {
            //                ItemId = (int)Utility.GetSpecialToolId(MoneyType.Coin),
            //                Num = sysToolCfg.Param1 * Num
            //            });
            //            response.ResItems.Add(new ItemPair()
            //            {
            //                ItemId = (int)Utility.GetSpecialToolId(MoneyType.Wood),
            //                Num = sysToolCfg.Param2 * Num
            //            });
            //            response.ResItems.Add(new ItemPair()
            //            {
            //                ItemId = (int)Utility.GetSpecialToolId(MoneyType.Stone),
            //                Num = sysToolCfg.Param3 * Num
            //            });
            //            response.ResItems.Add(new ItemPair()
            //            {
            //                ItemId = (int)Utility.GetSpecialToolId(MoneyType.Iron),
            //                Num = sysToolCfg.Param4 * Num
            //            });

            //            Utility.AddResource(userRole, ItemType.Coin, opCode, sysToolCfg.Param1 * Num);
            //            Utility.AddResource(userRole, ItemType.Wood, opCode, sysToolCfg.Param2 * Num);
            //            Utility.AddResource(userRole, ItemType.Stone, opCode, sysToolCfg.Param3 * Num);
            //            Utility.AddResource(userRole, ItemType.Iron, opCode, sysToolCfg.Param4 * Num);
            //            break;
            //        }
            //    case ToolType.SpTool:
            //        {
            //            response.ResItems.Add(new ItemPair()
            //            {
            //                ItemId = (int)Utility.GetSpecialToolId(MoneyType.Sp),
            //                Num = sysToolCfg.Param1 * Num
            //            });
            //            Utility.AddResource(userRole, ItemType.Sp, opCode, sysToolCfg.Param1 * Num);
            //            break;
            //        }
            //    default:
            //        {
            //            SetError(ResourceId.R_4007_ToolCanNotUse);
            //            return;
            //        }
            //}
            //使用
            userTool.RemoveTool(ItemId, Num, opCode);

            ResultObj = response;
        }
    }
    #endregion
}
