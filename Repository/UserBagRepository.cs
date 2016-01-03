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
            response.Items = userTool.Items.OrderBy(o => o.Quality).ToList();
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
        [Tag(3)]
        public int AdvancedStone { get; set; }

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
            /// 出售价格
            /// </summary>
            [Tag(5)]
            public int SellPrice { get; set; }
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

            UserEquip userEquip;
            UserTool userTool;
            Storage.Load(out userEquip, out userTool, CurrentUserId);

            var equipList = userEquip.ItemList.OrderBy(o => o.Quality).ToList();
            if (Type == 1)
            {
                var needQuality = ConfigHelper.EquipCfgData.NeedQuality;
                equipList =
                userEquip.ItemList.Where(o => o.Quality > needQuality && o.HeroId == 0) //未佩戴并且品质大于等于3
                    .OrderByDescending(o => o.Quality)      //品质高
                    //.ThenByDescending(o => o.Rank)          //进阶级高
                    .ThenByDescending(o => o.Level)         //强化等级高
                    .ToList();
            }
            if (EquipType != EquipType.None)
            {
                equipList = equipList.Where(o => o.EquipType == EquipType).ToList();
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

            var toolItem = userTool.Items.FirstOrDefault(o => o.ItemId == (int)ToolType.EquipAdvabcedStone);
            if (toolItem != null)
            {
                response.AdvancedStone = toolItem.Num;
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 获取用户背包装备碎片物品 4002
    public class GetUserEquipChipResponse
    {
        public GetUserEquipChipResponse()
        {
            Items = new List<ChipItem>();
        }
        /// <summary>
        /// 装备碎片列表
        /// </summary>
        [Tag(1)]
        public List<ChipItem> Items { get; set; }
    }
    /// <summary>
    /// 获取用户背包装备碎片物品
    /// </summary>
    [GameCode(OpCode = 4002, ResponseType = typeof(GetUserEquipChipResponse))]
    public class GetUserEquipChipRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetUserEquipChipResponse();

            var userChip = Storage.Load<UserChip>(CurrentUserId);
            response.Items = userChip.EquipChipItems;

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

        public override bool InitParams(GameContext context)
        {
            if (Type != BagItemType.Equip && Type != BagItemType.Tool) return false;
            if (BagItemIdList.Count == 0) return false;
            return true;
        }

        public override void Process(GameContext context)
        {
            var response = new BuyBagItemsResponse();
            if (Type == BagItemType.Tool)
            {
                //TODO:道具出售
                var userTool = Storage.Load<UserTool>(CurrentUserId, true);
                foreach (var i in BagItemIdList)
                {
                    var item = userTool.Items.FirstOrDefault(o => o.Id == i);
                }
            }
            else if (Type == BagItemType.Equip)
            {
                var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);
                foreach (var i in BagItemIdList)
                {
                    var item = userEquip.ItemList.FirstOrDefault(o => o.Id == i);
                    if (item != null)
                    {
                        if (item.HeroId > 0)
                        {
                            SetError(ResourceId.R_4003_CanNotSellEquipedEquip);
                            return;
                        }
                        userEquip.RemoveEquip(item);

                        response.TotalPrice += item.SellPrice;
                    }
                }
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 碎片合成装备 4004
    /// <summary>
    /// 碎片合成装备
    /// </summary>
    [GameCode(OpCode = 4004, ResponseType = typeof(GetUserEquipResponse.ReqUserEquipItem))]
    public class MixtureEquipRequest : GameHandler
    {
        /// <summary>
        /// 背包装备碎片id
        /// </summary>
        public int ChipId { get; set; }

        public override void Process(GameContext context)
        {
            var response = new GetUserEquipResponse.ReqUserEquipItem();

            var userChip = Storage.Load<UserChip>(CurrentUserId, true);

            var chipItem = userChip.EquipChipItems.FirstOrDefault(o => o.ItemId == ChipId);
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
            //是否是装备碎片
            if (sysToolCfg.GetToolType() != ToolType.EquipChip)
            {
                SetError(ResourceId.R_4004_NotEquipChip);
                return;
            }
            //Param1 装备id
            var sysEquipCfg = SysEquipCfg.Find(sysToolCfg.Param1);
            if (sysEquipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysEquipCfg", sysToolCfg.Param1);
                return;
            }

            //Param2 合成装备所需的碎片数量
            if (sysToolCfg.Param2 > chipItem.Num)
            {
                SetError(ResourceId.R_4004_EquipChipNotEnough);
                return;
            }

            //减去碎片
            userChip.SubChip(ChipId, sysToolCfg.Param2, ToolType.EquipChip);
            //chipItem.Num -= equip.NeedChip;

            var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);

            if (userEquip.IsFull)
            {
                //背包已满
                SetError(ResourceId.R_0000_EquipBagIsFull);
                return;
            }
            var equipItem = userEquip.AddEquipToUser(sysEquipCfg.Id, (int)ItemLogOperateType.EquipChipMixture);

            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserEquipItem, GetUserEquipResponse.ReqUserEquipItem>();
            mapper.Map(equipItem, response);

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
            /// Id（用于使用传参）
            /// </summary>
            [Tag(1)]
            public int Id { get; set; }
            /// <summary>
            /// 骑宠Id（用于获取骑宠的详细信息）
            /// </summary>
            [Tag(2)]
            public int PetId { get; set; }
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

            var userPet= Storage.Load<UserPet>(CurrentUserId);
            foreach (var userPetItem in userPet.ItemList)
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
}
