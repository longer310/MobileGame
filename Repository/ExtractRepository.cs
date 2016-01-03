using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;

namespace MobileGame.tianzi.Repository
{
    #region 获取抽取界面信息 5000
    public class GetExtractInfoResponse
    {
        public GetExtractInfoResponse()
        {
            Items = new List<ExtractItem>();
        }
        /// <summary>
        /// 抽取列表
        /// </summary>
        [Tag(1)]
        public List<ExtractItem> Items { get; set; }

        public class ExtractItem
        {
            /// <summary>
            /// 抽取类型 1:钻石(武将)，2：铜币，5:钻石(妃子)
            /// </summary>
            [Tag(1)]
            public ExtractType Type { get; set; }
            /// <summary>
            /// 今天最多免费次数
            /// </summary>
            [Tag(2)]
            public int MaxNum { get; set; }
            /// <summary>
            /// 剩余免费次数
            /// </summary>
            [Tag(3)]
            public int LaveNum { get; set; }
            /// <summary>
            /// 一次价格
            /// </summary>
            [Tag(4)]
            public int OnePrice { get; set; }
            /// <summary>
            /// 十次价格
            /// </summary>
            [Tag(5)]
            public int TenPrice { get; set; }
            /// <summary>
            /// 剩余时间（秒）
            /// </summary>
            [Tag(6)]
            public int LaveTime { get; set; }
        }
    }
    /// <summary>
    /// 获取抽取界面信息 5000
    /// </summary>
    [GameCode(OpCode = 5000, ResponseType = typeof(GetExtractInfoResponse))]
    public class GeExtractInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetExtractInfoResponse();

            var userExtract = Storage.Load<UserExtract>(CurrentUserId, true);
            var list = new List<ExtractType>() { ExtractType.Money, ExtractType.Coin, ExtractType.Concubine };
            var listindex = new List<int>() { 0, 1, 2 };
            var index = 0;
            foreach (var extractType in list)
            {
                var extractItem = new GetExtractInfoResponse.ExtractItem();
                var eindex = listindex[index];
                extractItem.Type = extractType;
                extractItem.MaxNum = ConfigHelper.ExtractCfgData.MaxExtracts[eindex];
                extractItem.OnePrice = ConfigHelper.ExtractCfgData.OneExtractPrices[eindex];
                extractItem.TenPrice = ConfigHelper.ExtractCfgData.TenExtractPrices[eindex];

                if (extractType == ExtractType.Coin)
                {
                    extractItem.LaveNum = userExtract.CoinExtractLaveNum;
                    extractItem.LaveTime = userExtract.CoinExtractLaveTime;
                }
                else if (extractType == ExtractType.Money)
                {
                    extractItem.LaveNum = userExtract.MoneyExtractLaveNum;
                    extractItem.LaveTime = userExtract.MoneyExtractLaveTime;
                }
                else if (extractType == ExtractType.Concubine)
                {
                    extractItem.LaveNum = userExtract.Concubine_MoneyExtractLaveNum;
                    extractItem.LaveTime = userExtract.Concubine_MoneyExtractLaveTime;
                }

                response.Items.Add(extractItem);
                index++;
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 获取抽取结果 5001
    public class GetExtractResultResponse
    {
        public GetExtractResultResponse()
        {
            Items = new List<ExtractItemsItem>();
        }
        /// <summary>
        /// 抽取列表
        /// </summary>
        [Tag(1)]
        public List<ExtractItemsItem> Items { get; set; }

        public class ExtractItemsItem
        {
            /// <summary>
            /// 抽取类型
            /// </summary>
            [Tag(1)]
            public ExtractItemType Type { get; set; }
            /// <summary>
            /// 物品id/系统id（武将、妃子）
            /// </summary>
            [Tag(2)]
            public int SysId { get; set; }
            /// <summary>
            /// 抽取到的个数（物品个数可能大于1）
            /// </summary>
            [Tag(3)]
            public int Num { get; set; }
            /// <summary>
            /// 碎片个数（如果大于零则说明玩家已拥有抽取到的武将/妃子）
            /// </summary>
            [Tag(4)]
            public int ChipNum { get; set; }
        }
    }
    /// <summary>
    /// 获取抽取结果 5001
    /// </summary>
    [GameCode(OpCode = 5001, ResponseType = typeof(GetExtractResultResponse))]
    public class GeExtractResultRequest : GameHandler
    {
        /// <summary>
        /// 抽取类型
        /// </summary>
        public ExtractType Type { get; set; }

        /// <summary>
        /// 抽取物品
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type">元宝、铜钱</param>
        /// <param name="userEquip">装备</param>
        /// <param name="userChip">碎片</param>
        /// <param name="userTool">道具</param>
        /// <param name="userRole">用户</param>
        /// <param name="userHero">武将</param>
        /// <param name="userConcubine">妃子</param>
        /// <param name="special">十连抽特殊一次，得到整卡【装备】或者英雄</param>
        /// <returns></returns>
        public GetExtractResultResponse.ExtractItemsItem GetExtractItem(GameContext context, ExtractType type,
            UserEquip userEquip, UserChip userChip, UserTool userTool, UserRole userRole, UserHero userHero,
            UserConcubine userConcubine, int special = 0)
        {
            var opCode = Request.OpCode;
            var extractItemsItem = new GetExtractResultResponse.ExtractItemsItem();

            var sysExtractPrecCfgItems = SysExtractPrecCfg.Items.Where(o => o.Type == type).ToList();
            var randExtractPrec = new SysExtractPrecCfg();
            if (special == 0)
            {
                int index = Utility.GetIndexFromWeightsList(sysExtractPrecCfgItems.Select(o => o.TeamPre).ToList());
                randExtractPrec = sysExtractPrecCfgItems[index];
            }
            else
            {
                //十连抽特殊获得
                if (type == ExtractType.Coin)
                {
                    //随机一件整卡
                    var randExtractPrecList =
                        sysExtractPrecCfgItems.OrderByDescending(o => (int)o.ItemType > (int)ExtractItemType.Equip).ToList();
                    if (randExtractPrecList.Any())
                    {
                        var random = Util.GetRandom(0, randExtractPrecList.Count());
                        randExtractPrec = randExtractPrecList[random];
                    }
                }
                //随机一个英雄
                else if (type == ExtractType.Money)
                    randExtractPrec = sysExtractPrecCfgItems.FirstOrDefault(o => o.ItemType == ExtractItemType.Hero);
            }
            if (randExtractPrec == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysExtractPrecCfg:type", type);
                return extractItemsItem;
            }

            //出来的组类型
            extractItemsItem.Type = randExtractPrec.ItemType;
            extractItemsItem.Num = randExtractPrec.Num;

            List<int> rankPrecList = new List<int>()
            {
                randExtractPrec.White,
                randExtractPrec.Green,
                randExtractPrec.Blue,
                randExtractPrec.Purple,
                randExtractPrec.Orange
            };
            var quality = Utility.GetIndexFromWeightsList(rankPrecList);
            var eType = extractItemsItem.Type;
            if (eType == ExtractItemType.Tool || eType == ExtractItemType.HeroChip ||
                eType == ExtractItemType.ConcubineChip || eType == ExtractItemType.EquipChip || eType == ExtractItemType.PetChip)
            {
                //随机获取道具
                var itemList = SysToolCfg.Items;
                if (eType == ExtractItemType.Tool)
                    itemList = SysToolCfg.Items.Where(
                        o =>
                            o.ExtractWeights > 0 && o.GetToolType() < ToolType.PetChip &&
                            o.Quality == (ItemQuality)quality).ToList();
                else if (eType == ExtractItemType.PetChip)
                {
                    itemList = SysToolCfg.Items.Where(
                        o =>
                            o.ExtractWeights > 0 && o.GetToolType() == ToolType.PetChip &&
                            o.Quality == (ItemQuality)quality).ToList();
                }
                else if (eType == ExtractItemType.HeroChip)
                {
                    itemList = SysToolCfg.Items.Where(
                        o =>
                            o.ExtractWeights > 0 && o.GetToolType() == ToolType.HeroChip &&
                            o.Quality == (ItemQuality)quality).ToList();
                }
                else if (eType == ExtractItemType.ConcubineChip)
                {
                    itemList = SysToolCfg.Items.Where(
                        o =>
                            o.ExtractWeights > 0 && o.GetToolType() == ToolType.ConcubineChip &&
                            o.Quality == (ItemQuality)quality).ToList();
                }
                else if (eType == ExtractItemType.EquipChip)
                {
                    itemList = SysToolCfg.Items.Where(
                        o =>
                            o.ExtractWeights > 0 && o.GetToolType() == ToolType.EquipChip &&
                            o.Quality == (ItemQuality)quality).ToList();
                }
                if (itemList.Count == 0)
                {
                    SetError(ResourceId.R_0000_IdNotExist, string.Format("eType:{0},SysToolCfg:Quality", eType), quality);
                    return extractItemsItem;
                }
                var rItemIndex = Utility.GetIndexFromWeightsList(itemList.Select(o => o.ExtractWeights).ToList());
                if (rItemIndex < itemList.Count)
                {
                    var rItem = itemList[rItemIndex];
                    //rItem.AddToUser(CurrentUserId, opCode, randExtractPrec.Num);
                    if (userChip == null) userChip = Storage.Load<UserChip>(CurrentUserId, true);
                    rItem.AddToUser(opCode, userChip, userRole, userTool, randExtractPrec.Num);
                    extractItemsItem.SysId = rItem.Id;
                }
            }
            else if (extractItemsItem.Type == ExtractItemType.Equip)
            {
                //前后十级的装备
                var equipList =
                    SysEquipCfg.Items.Where(
                        o => o.ExtractWeights > 0 &&
                            o.Quality == (ItemQuality)quality &&
                            o.NeedLevel < userRole.Level + 11).ToList();
                if (equipList.Count == 0)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysEquipCfg:Quality", quality);
                    return extractItemsItem;
                }
                var rEquipIndex = Utility.GetIndexFromWeightsList(equipList.Select(o => o.ExtractWeights).ToList());
                //var rEquipIndex = Util.GetRandom(0, equipList.Count);
                if (rEquipIndex < equipList.Count)
                {
                    var rEquip = equipList[rEquipIndex];
                    userEquip.AddEquipToUser(rEquip.Id, Request.OpCode);
                    //rEquip.AddToUser(CurrentUserId, opCode, randExtractPrec.Num);
                    extractItemsItem.SysId = rEquip.Id;
                }
            }
            else if (extractItemsItem.Type == ExtractItemType.Hero)
            {
                //随机获取武将
                var sysHeroList =
                    SysHeroCfg.Items.Where(
                        o => o.Quality == (ItemQuality)quality && o.ExtractWeights > 0 && o.Type != HeroType.Npc)
                        .ToList();
                var rItemIndex = Utility.GetIndexFromWeightsList(sysHeroList.Select(o => o.ExtractWeights).ToList());
                if (rItemIndex < sysHeroList.Count)
                {
                    var rItem = sysHeroList[rItemIndex];

                    if (userHero == null) userHero = Storage.Load<UserHero>(CurrentUserId, true);
                    if (userHero.Items.Exists(o => o.HeroId == rItem.Id))
                    {
                        //存在武将给武将碎片
                        var heroItem = userHero.FindByHeroId(rItem.Id);
                        if (heroItem != null)
                        {
                            //添加武将碎片
                            var chipNum = 1;//ConfigHelper.ExtractCfgData.ExchangeHeroChips[quality];
                            var toolId = rItem.Id + (int)ToolType.HeroChip;
                            var sysToolCfg = SysToolCfg.Find(o => o.Id == toolId);
                            if (sysToolCfg != null)
                            {
                                chipNum = sysToolCfg.Param3;
                            }

                            if (userChip == null) userChip = Storage.Load<UserChip>(CurrentUserId, true);
                            userChip.AddChip(rItem.ChipId, chipNum, ToolType.HeroChip, opCode);
                            extractItemsItem.SysId = rItem.Id;
                            extractItemsItem.Num = 0;
                            extractItemsItem.ChipNum = chipNum; //randExtractPrec.Num;
                        }
                    }
                    else
                    {
                        userHero.AddHeroToUser(rItem.Id, opCode);
                        extractItemsItem.SysId = rItem.Id;
                    }
                }
            }
            else if (extractItemsItem.Type == ExtractItemType.Concubine)
            {
                //随机获取妃子
                var sysConcubineList =
                    SysConcubineCfg.Items.Where(o => o.Quality == (ItemQuality)quality && o.ExtractWeights > 0).ToList();
                if (sysConcubineList.Count > 0)
                {
                    var rItemIndex =
                        Utility.GetIndexFromWeightsList(sysConcubineList.Select(o => o.ExtractWeights).ToList());
                    if (rItemIndex < sysConcubineList.Count)
                    {
                        var rItem = sysConcubineList[rItemIndex];

                        if (userConcubine == null) userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                        if (userConcubine.Items.Exists(o => o.ConcubineId == rItem.Id))
                        {
                            //存在妃子给妃子碎片
                            var concubineItem = userConcubine.Items.Find(o => o.ConcubineId == rItem.Id);
                            if (concubineItem != null)
                            {
                                var chipNum = 1;//ConfigHelper.ExtractCfgData.ExchangeConcubineChips[quality];
                                var toolId = rItem.Id + (int)ToolType.ConcubineChip;
                                var sysToolCfg = SysToolCfg.Find(o => o.Id == toolId);
                                if (sysToolCfg != null)
                                {
                                    chipNum = sysToolCfg.Param3;
                                }

                                if (userChip == null) userChip = Storage.Load<UserChip>(CurrentUserId, true);
                                userChip.AddChip(rItem.ChipId, chipNum, ToolType.ConcubineChip, opCode);
                                extractItemsItem.SysId = rItem.Id;
                                extractItemsItem.Num = 0;
                                extractItemsItem.ChipNum = chipNum;//randExtractPrec.Num;
                            }
                        }
                        else
                        {
                            //添加新妃子
                            var concubineItem = KVEntity.CreateNew<UserConcubineItem>();
                            //映射属性
                            var mapper =
                                ObjectMapperManager.DefaultInstance.GetMapper<SysConcubineCfg, UserConcubineItem>(
                                    new DefaultMapConfig().IgnoreMembers<SysConcubineCfg, UserConcubineItem>(new[] { "Id" }));
                            mapper.Map(rItem, concubineItem);
                            concubineItem.ConcubineId = rItem.Id;
                            concubineItem.Pid = CurrentUserId;
                            userConcubine.Items.Add(concubineItem);
                            extractItemsItem.SysId = rItem.Id;
                        }
                    }
                }
                else
                {
                    SetError(ResourceId.R_0000_IdNotExist, string.Format("eType:{0},SysToolCfg:Quality", eType), quality);
                    return extractItemsItem;
                }

            }

            return extractItemsItem;
        }

        /// <summary>
        /// 从商店里面抽取一个物品
        /// </summary>
        /// <param name="shopType"></param>
        /// <returns></returns>
        public GetExtractResultResponse.ExtractItemsItem GetExtractItem(ShopType shopType)
        {
            var sysShopCfgList =
                SysShopCfg.Items.Where(
                    o => o.ShopType == shopType && o.ExtractWeights > 0).ToList();
            var rItemIndex = Utility.GetIndexFromWeightsList(sysShopCfgList.Select(o => o.ExtractWeights).ToList());
            if (rItemIndex < sysShopCfgList.Count)
            {
                var sysShopCfg = sysShopCfgList[rItemIndex];

                var userId = CurrentUserId;
                var id = sysShopCfg.GoodsId;
                var maxnum = sysShopCfg.Num;

                var num = Util.GetRandom(1, maxnum + 1);
                var eItem = new GetExtractResultResponse.ExtractItemsItem();
                eItem.SysId = id;
                eItem.Num = num;
                var opCode = Request.OpCode;
                if (id > 10000000)
                {
                    var sysToolCfg = SysToolCfg.Find(o => o.Id == id);
                    if (sysToolCfg == null)
                    {
                        return new GetExtractResultResponse.ExtractItemsItem() { Type = ExtractItemType.None };
                    }
                    UserTool userTool;
                    UserChip userChip;
                    UserRole userRole;
                    Storage.Load(out userTool, out userChip, out userRole, userId, true);
                    sysToolCfg.AddToUser(opCode, userChip, userRole, userTool, num);

                    var type = sysToolCfg.GetToolType();
                    var isChip = sysToolCfg.IsChip(type);
                    if (isChip)
                    {
                        if (type == ToolType.PetChip) eItem.Type = ExtractItemType.PetChip;
                        if (type == ToolType.HeroChip) eItem.Type = ExtractItemType.HeroChip;
                        if (type == ToolType.ConcubineChip) eItem.Type = ExtractItemType.ConcubineChip;
                        if (type == ToolType.EquipChip) eItem.Type = ExtractItemType.EquipChip;
                    }
                    else
                    {
                        eItem.Type = ExtractItemType.Tool;
                    }
                }
                else
                {
                    var bType = Utility.GetItemType(id);
                    if (bType == BagItemType.Equip)
                    {
                        var sysEquipCfg = SysEquipCfg.Items.Find(o => o.Id == id);
                        if (sysEquipCfg == null)
                            return new GetExtractResultResponse.ExtractItemsItem() { Type = ExtractItemType.None };

                        var userEquip = DataStorage.Current.Load<UserEquip>(userId, true);
                        userEquip.AddEquipToUser(sysEquipCfg.Id, opCode);

                        eItem.Type = ExtractItemType.Equip;
                    }
                    else if (bType == BagItemType.Pet)
                    {
                        var sysPetCfg = SysPetCfg.Items.Find(o => o.Id == id);
                        if (sysPetCfg == null)
                            return new GetExtractResultResponse.ExtractItemsItem() { Type = ExtractItemType.None };

                        var userPet = DataStorage.Current.Load<UserPet>(userId, true);
                        userPet.AddPetToUser(sysPetCfg.Id, opCode);

                        eItem.Type = ExtractItemType.Pet;
                    }
                    else if (bType == BagItemType.Hero)
                    {
                        var sysHeroCfg = SysHeroCfg.Items.Find(o => o.Id == id);
                        if (sysHeroCfg == null)
                            return new GetExtractResultResponse.ExtractItemsItem() { Type = ExtractItemType.None };

                        var userHero = DataStorage.Current.Load<UserHero>(userId, true);
                        if (userHero.Items.Exists(o => o.HeroId == sysHeroCfg.Id))
                        {
                            //存在武将给武将碎片
                            var heroItem = userHero.FindByHeroId(sysHeroCfg.Id);
                            if (heroItem != null)
                            {
                                //添加武将碎片
                                var chipNum = 1;//ConfigHelper.ExtractCfgData.ExchangeHeroChips[quality];
                                var toolId = sysHeroCfg.Id + (int)ToolType.HeroChip;
                                var sysToolCfg = SysToolCfg.Find(o => o.Id == toolId);
                                if (sysToolCfg != null)
                                {
                                    chipNum = sysToolCfg.Param3;
                                }

                                var userChip = DataStorage.Current.Load<UserChip>(userId, true);
                                userChip.AddChip(sysHeroCfg.ChipId, chipNum, ToolType.HeroChip, opCode);

                                eItem.ChipNum = chipNum;
                            }
                        }
                        else
                        {
                            userHero.AddHeroToUser(sysHeroCfg.Id, opCode);
                        }

                        eItem.Type = ExtractItemType.Hero;
                    }
                    else if (bType == BagItemType.Concubine)
                    {
                        var sysConcubineCfg = SysConcubineCfg.Items.Find(o => o.Id == id);
                        if (sysConcubineCfg == null)
                            return new GetExtractResultResponse.ExtractItemsItem() { Type = ExtractItemType.None };

                        var userConcubine = DataStorage.Current.Load<UserConcubine>(userId, true);
                        if (userConcubine.Items.Exists(o => o.ConcubineId == sysConcubineCfg.Id))
                        {
                            //存在妃子给妃子碎片
                            var concubineItem = userConcubine.Items.Find(o => o.ConcubineId == sysConcubineCfg.Id);
                            if (concubineItem != null)
                            {
                                //添加妃子碎片
                                var chipNum = 1;
                                var toolId = sysConcubineCfg.Id + (int)ToolType.ConcubineChip;
                                var sysToolCfg = SysToolCfg.Find(o => o.Id == toolId);
                                if (sysToolCfg != null)
                                {
                                    chipNum = sysToolCfg.Param3;
                                }

                                var userChip = DataStorage.Current.Load<UserChip>(userId, true);
                                userChip.AddChip(sysConcubineCfg.ChipId, chipNum, ToolType.HeroChip, opCode);

                                eItem.ChipNum = chipNum;
                            }
                        }
                        else
                        {
                            userConcubine.AddConcubineToUser(sysConcubineCfg.Id, opCode);
                        }

                        eItem.Type = ExtractItemType.Concubine;
                    }
                }

                return eItem;
            }
            return new GetExtractResultResponse.ExtractItemsItem();
        }

        /// <summary>
        /// 判断背包格子是否满
        /// </summary>
        /// <param name="userEquip"></param>
        /// <param name="userTool"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool JudgeBagIsFull(UserEquip userEquip, UserTool userTool, ExtractType type)
        {
            var num = 1;
            if (type == ExtractType.TenCoin || type == ExtractType.TenMoney) num = 10;
            //装备背包不足
            if (userEquip.IsFull || userEquip.LaveCapacity < num)
            {
                SetError(ResourceId.R_0000_EquipBagIsFull);
                return false;
            }
            //道具背包不足
            if (userTool.IsFull || userTool.LaveCapacity < num)
            {
                SetError(ResourceId.R_0000_ToolBagIsFull);
                return false;
            }
            return true;
        }

        public override void Process(GameContext context)
        {
            var addValue = 1;
            UserEquip userEquip;
            UserTool userTool;
            UserExtract userExtract;
            UserRole userRole;
            Storage.Load(out userEquip, out userTool, out userExtract, out userRole, CurrentUserId);
            if (!JudgeBagIsFull(userEquip, userTool, Type)) return;

            UserChip userChip = null;
            UserHero userHero = null;
            UserConcubine userConcubine = null;

            var logItem = new GameLogItem();
            logItem.F1 = (int)Type;
            var opCode = Request.OpCode;

            var response = new GetExtractResultResponse();
            if (Type == ExtractType.Coin)
            {
                string errorMsg;
                if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
                {
                    SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                    return;
                }

                if (userExtract.CoinExtractLaveNum > 0 && userExtract.CoinExtractLaveTime <= 0)
                {
                    //免费铜币抽取一次并更新免费间隔时间
                    userExtract.CoinExtractEndTime =
                        DateTime.Now.AddSeconds(ConfigHelper.ExtractCfgData.ExtractIntervals[(int)ExtractType.Coin - 1]);
                    userExtract.CoinExtractNum += 1;

                    userExtract.ChangeNewMsg(userRole);//抽奖新信息
                }
                else
                {
                    //花费铜币抽取一次
                    var needCoin = ConfigHelper.ExtractCfgData.OneExtractPrices[(int)ExtractType.Coin - 1];
                    if (userRole.Coin < needCoin)
                    {
                        SetError(ResourceId.R_0000_CoinNotEnough);
                        return;
                    }

                    Utility.ConsumeResource(userRole, ItemType.Coin, opCode, needCoin);
                    logItem.F2 = needCoin;
                    //userRole.Coin -= needCoin;
                }

                //var item = GetExtractItem(context, Type, userEquip, userChip, userTool, userRole, userHero, userConcubine);
                var item = GetExtractItem(ShopType.Coin);
                response.Items.Add(item);
            }
            else if (Type == ExtractType.TenCoin)
            {
                string errorMsg;
                if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
                {
                    SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                    return;
                }

                addValue = 10;
                var needCoin = ConfigHelper.ExtractCfgData.TenExtractPrices[(int)ExtractType.Coin - 1];
                if (userRole.Coin < needCoin)
                {
                    SetError(ResourceId.R_0000_CoinNotEnough);
                    return;
                }
                //userRole.Coin -= needCoin;
                Utility.ConsumeResource(userRole, ItemType.Coin, opCode, needCoin);
                logItem.F2 = needCoin;

                //10连抽随机一次特殊处理
                var random = Util.GetRandom(0, 10);
                for (int i = 0; i < 10; i++)
                {
                    //var item = GetExtractItem(context, ExtractType.Coin, userEquip, userChip, userTool, userRole, userHero, userConcubine,
                    //    i == random ? 1 : 0);
                    var item = GetExtractItem(i == random ? ShopType.TenCoin : ShopType.Coin);
                    response.Items.Add(item);
                }
            }
            else if (Type == ExtractType.Money)
            {
                if (userExtract.MoneyExtractLaveNum > 0 && userExtract.MoneyExtractLaveTime <= 0)
                {
                    //免费铜币抽取一次并更新免费间隔时间
                    userExtract.MoneyExtractEndTime =
                        DateTime.Now.AddSeconds(ConfigHelper.ExtractCfgData.ExtractIntervals[(int)ExtractType.Money - 1]);
                    userExtract.MoneyExtractNum += 1;

                    userExtract.ChangeNewMsg(userRole);//抽奖新信息
                }
                else
                {
                    //花费钻石抽取一次
                    var needMoney = ConfigHelper.ExtractCfgData.OneExtractPrices[(int)ExtractType.Money - 1];
                    if (userRole.TotalMoney < needMoney)
                    {
                        SetError(ResourceId.R_0000_MoneyNotEnough);
                        return;
                    }
                    Utility.Concume(userRole, needMoney, SpecialStoreId.OneMoneyExtract);
                    logItem.F2 = needMoney;
                }

                //var item = GetExtractItem(context, Type, userEquip, userChip, userTool, userRole, userHero, userConcubine);
                var item = GetExtractItem(ShopType.Hero);
                response.Items.Add(item);
            }
            else if (Type == ExtractType.TenMoney)
            {
                addValue = 10;
                var needMoney = ConfigHelper.ExtractCfgData.TenExtractPrices[(int)ExtractType.Money - 1];
                if (userRole.TotalMoney < needMoney)
                {
                    SetError(ResourceId.R_0000_MoneyNotEnough);
                    return;
                }
                Utility.Concume(userRole, needMoney, SpecialStoreId.TenMoneyExtract);
                logItem.F2 = needMoney;

                //10连抽随机一次特殊处理
                var random = Util.GetRandom(0, 10);
                for (int i = 0; i < 10; i++)
                {
                    //var item = GetExtractItem(context, ExtractType.Money, userEquip, userChip, userTool, userRole, userHero, userConcubine,
                    //    i == random ? 1 : 0);
                    var item = GetExtractItem(i == random ? ShopType.TenHero : ShopType.Hero);
                    response.Items.Add(item);
                }
            }
            else if (Type == ExtractType.Concubine)
            {
                if (userExtract.Concubine_MoneyExtractLaveNum > 0 && userExtract.Concubine_MoneyExtractLaveTime <= 0)
                {
                    //免费铜币抽取一次并更新免费间隔时间
                    userExtract.Concubine_MoneyExtractEndTime =
                        DateTime.Now.AddSeconds(ConfigHelper.ExtractCfgData.ExtractIntervals[2]);//写为固定index ！！！
                    userExtract.Concubine_MoneyExtractNum += 1;

                    userExtract.ChangeNewMsg(userRole);//抽奖新信息
                }
                else
                {
                    //花费钻石抽取一次
                    var needMoney = ConfigHelper.ExtractCfgData.OneExtractPrices[2];//TODO:固定下来了
                    if (userRole.TotalMoney < needMoney)
                    {
                        SetError(ResourceId.R_0000_MoneyNotEnough);
                        return;
                    }
                    Utility.Concume(userRole, needMoney, SpecialStoreId.Concubine_OneMoneyExtract);
                    logItem.F2 = needMoney;
                }

                //var item = GetExtractItem(context, Type, userEquip, userChip, userTool, userRole, userHero, userConcubine);
                var item = GetExtractItem(ShopType.Concubine);
                response.Items.Add(item);
            }
            else if (Type == ExtractType.TenConcubine)
            {
                addValue = 10;
                var needMoney = ConfigHelper.ExtractCfgData.TenExtractPrices[2];//写为固定index ！！！！
                if (userRole.TotalMoney < needMoney)
                {
                    SetError(ResourceId.R_0000_MoneyNotEnough);
                    return;
                }
                Utility.Concume(userRole, needMoney, SpecialStoreId.Concubine_TenMoneyExtract);
                logItem.F2 = needMoney;

                //10连抽随机一次特殊处理
                var random = Util.GetRandom(0, 10);
                for (int i = 0; i < 10; i++)
                {
                    //var item = GetExtractItem(context, ExtractType.Money, userEquip, userChip, userTool, userRole, userHero, userConcubine,
                    //    i == random ? 1 : 0);
                    var item = GetExtractItem(i == random ? ShopType.TenConcubine : ShopType.Concubine);
                    response.Items.Add(item);
                }
            }

            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            //添加抽奖任务次数
            Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.Extract, addValue);

            ResultObj = response;
        }
    }
    #endregion

    #region 设置新手步骤/或者请求获得的武将/妃子/物品列表 5002
    public class RecruitGuideHeroResponse
    {
        public RecruitGuideHeroResponse()
        {
            Items = new List<ItemPair>();
        }
        /// <summary>
        /// 完成步骤得到的物品
        /// </summary>
        [Tag(1)]
        public ExtractItemType Type { get; set; }
        /// <summary>
        /// 获取到的武将/妃子/物品
        /// </summary>
        [Tag(2)]
        public List<ItemPair> Items { get; set; }
    }
    /// <summary>
    /// 获取新手引导的武将
    /// </summary>
    [GameCode(OpCode = 5002, ResponseType = typeof(RecruitGuideHeroResponse))]
    public class RecruitGuideHeroRequest : GameHandler
    {
        /// <summary>
        /// 类型
        /// </summary>
        public GuideStep GuideStep { get; set; }
        public override void Process(GameContext context)
        {
            UserRole userRole = Storage.Load<UserRole>(CurrentUserId, true);

            var guideStep = userRole.GuideStepList[0];

            var haveSetStep = guideStep >= (int)GuideStep ? true : false;
            //if ()
            //{//该步骤已设置过
            //    //SetError(ResourceId.R_5002_GuideStepSeted);
            //    //return;
            //    haveSetStep = true;
            //}
            //if ((guideStep + 1) != (int)GuideStep)
            //{//不能跳跃设置新手步骤
            //    SetError(ResourceId.R_5002_CanNotJumpGuideStep);
            //    return;
            //}
            var response = new RecruitGuideHeroResponse();
            if (!haveSetStep)
            {
                if (GuideStep == GuideStep.One || GuideStep == GuideStep.Three || GuideStep == GuideStep.Four)
                {
                    //第一、第三、第四步 获得 诸葛亮、张飞、孙尚香武将
                    var userHero = Storage.Load<UserHero>(CurrentUserId, true);
                    var guideGetIdList = ConfigHelper.GuideGetIdList;
                    var heroId = guideGetIdList[(int)GuideStep - 1];

                    if (heroId == 0)
                    {
                        SetError(ResourceId.R_0000_IllegalParam);
                        return;
                    }
                    var userHeroItem = userHero.Items.FirstOrDefault(o => o.HeroId == heroId);
                    if (userHeroItem != null)
                    {
                        //SetError(ResourceId.R_4004_HeroIsExist);
                        //return;
                    }
                    else
                    {
                        userHero.AddHeroToUser(heroId, Request.OpCode);
                        response.Type = ExtractItemType.Hero;
                        response.Items.Add(new ItemPair()
                        {
                            ItemId = heroId,
                            Num = 1,
                        });
                    }

                    if (GuideStep == GuideStep.Three || GuideStep == GuideStep.Four)
                    {
                        var userExtract = Storage.Load<UserExtract>(CurrentUserId, true);
                        if (GuideStep == GuideStep.Three)
                        {
                            //免费铜币抽取一次并更新免费间隔时间
                            userExtract.CoinExtractEndTime =
                                DateTime.Now.AddSeconds(ConfigHelper.ExtractCfgData.ExtractIntervals[(int)ExtractType.Coin - 1]);
                            userExtract.CoinExtractNum += 1;
                        }
                        else
                        {
                            //免费铜币抽取一次并更新免费间隔时间
                            userExtract.MoneyExtractEndTime =
                                DateTime.Now.AddSeconds(ConfigHelper.ExtractCfgData.ExtractIntervals[(int)ExtractType.Money - 1]);
                            userExtract.MoneyExtractNum += 1;
                        }

                        userExtract.ChangeNewMsg(userRole);//抽奖新信息
                    }
                }
                else if (GuideStep == GuideStep.Two)
                {
                    //第二步，获得新手礼包
                    var guideGetIdList = ConfigHelper.GuideGetIdList;
                    var itemId = guideGetIdList[(int)GuideStep - 1];
                    var sysToolCfg = SysToolCfg.Find(o => o.Id == itemId);
                    if (sysToolCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", itemId);
                        return;
                    }

                    var opCode = Request.OpCode;
                    response.Items = sysToolCfg.UseToUser(opCode, userRole, 1);

                    //var toolType = sysToolCfg.GetToolType();
                    //switch (toolType)
                    //{
                    //    case ToolType.CoinTool:
                    //        {
                    //            response.Items.Add(new ItemPair()
                    //            {
                    //                ItemId = (int)SpecialToolId.Coin,
                    //                Num = sysToolCfg.Param1
                    //            });
                    //            response.Items.Add(new ItemPair()
                    //            {
                    //                ItemId = (int)SpecialToolId.Wood,
                    //                Num = sysToolCfg.Param2
                    //            });
                    //            response.Items.Add(new ItemPair()
                    //            {
                    //                ItemId = (int)SpecialToolId.Stone,
                    //                Num = sysToolCfg.Param3
                    //            });
                    //            response.Items.Add(new ItemPair()
                    //            {
                    //                ItemId = (int)SpecialToolId.Iron,
                    //                Num = sysToolCfg.Param4
                    //            });
                    //            response.Items.Add(new ItemPair()
                    //            {
                    //                ItemId = (int)SpecialToolId.Money,
                    //                Num = sysToolCfg.Param5
                    //            });

                    //            Utility.AddResource(userRole, ItemType.Coin, opCode, sysToolCfg.Param1);
                    //            Utility.AddResource(userRole, ItemType.Wood, opCode, sysToolCfg.Param2);
                    //            Utility.AddResource(userRole, ItemType.Stone, opCode, sysToolCfg.Param3);
                    //            Utility.AddResource(userRole, ItemType.Iron, opCode, sysToolCfg.Param4);

                    //            RoleManager.AddGiveMoney(userRole, sysToolCfg.Param5);
                    //            break;
                    //        }
                    //    default: break;
                    //}
                    response.Type = ExtractItemType.Tool;
                }
                else if (GuideStep == GuideStep.Eleven || GuideStep == GuideStep.Twelve)
                {
                    //第十一、十二步，获得妃子赵姬、杜十娘
                    var guideGetIdList = ConfigHelper.GuideGetIdList;
                    var concubineId = guideGetIdList[(int)GuideStep - 1];
                    var sysConcubineCfg = SysConcubineCfg.Items.FirstOrDefault(o => o.Id == concubineId);
                    if (sysConcubineCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysConcubineCfg:Id", concubineId);
                        return;
                    }

                    var userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                    //添加新妃子
                    var concubineItem = KVEntity.CreateNew<UserConcubineItem>();
                    //映射属性
                    var mapper =
                        ObjectMapperManager.DefaultInstance.GetMapper<SysConcubineCfg, UserConcubineItem>(
                            new DefaultMapConfig().IgnoreMembers<SysConcubineCfg, UserConcubineItem>(new[] { "Id" }));
                    mapper.Map(sysConcubineCfg, concubineItem);
                    concubineItem.ConcubineId = sysConcubineCfg.Id;
                    concubineItem.Pid = CurrentUserId;
                    userConcubine.Items.Add(concubineItem);

                    response.Type = ExtractItemType.Concubine;
                    response.Items.Add(new ItemPair()
                    {
                        ItemId = concubineId,
                        Num = 1,
                    });
                }
            }

            userRole.SetGuid(0, (int)GuideStep);
            //userRole.GuideStep = (int)GuideStep;

            ResultObj = response;
        }
    }
    #endregion

    #region 5003 仅仅设置新手引导步骤
    /// <summary>
    /// 仅仅设置新手引导步骤
    /// </summary>
    [GameCode(OpCode = 5003)]
    public class SetGuideRequest : GameHandler
    {
        /// <summary>
        /// 步骤类型，0：大步，1：0的子集，2:1的子集
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 步骤数
        /// </summary>
        public int GuideStep { get; set; }
        public override void Process(GameContext context)
        {
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);

            userRole.SetGuid(Index, GuideStep);
        }
    }
    #endregion

    #region 5004 大地图新手引导战斗
    /// <summary>
    /// 大地图新手引导战斗
    /// </summary>
    public class BigMapGuideBattleResponse
    {
        public BigMapGuideBattleResponse()
        {
            AllBattle = new AllBattle();

            AttackerHeroItems = new List<BattleHeroItem>();

            DefenderHeroItems = new List<BattleHeroItem>();
        }
        /// <summary>
        /// 战役信息
        /// </summary>
        [Tag(1)]
        public AllBattle AllBattle { get; set; }
        /// <summary>
        /// 攻击方武将列表
        /// </summary>
        [Tag(2)]
        public List<BattleHeroItem> AttackerHeroItems { get; set; }
        /// <summary>
        /// 防守方武将列表
        /// </summary>
        [Tag(3)]
        public List<BattleHeroItem> DefenderHeroItems { get; set; }
    }
    /// <summary>
    /// 大地图新手引导战斗
    /// </summary>
    [GameCode(OpCode = 5004, ResponseType = typeof(BigMapGuideBattleResponse))]
    public class BigMapGuideBattleRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new BigMapGuideBattleResponse();

            var npcId = ConfigHelper.BigMapGuideBattleNpcId;
            var sysBigMapCfg = SysBigMapCfg.Find(npcId);
            if (sysBigMapCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBigMapCfg:Id", npcId);
                return;
            }
            response.AllBattle.WarType = WarType.BigMapGuide;

            response.AllBattle.AttackerId = CurrentUserId;
            response.AllBattle.AttackerLevel = sysBigMapCfg.Level;
            response.AllBattle.AttackerHeadId = sysBigMapCfg.HeadId;
            response.AllBattle.AttackerName = sysBigMapCfg.NickName;

            var npcHeroList = SysBigMapHeroCfg.Items.Where(o => o.NpcId == npcId && o.Turn == 1).ToList();
            foreach (var sysBigMapHeroCfg in npcHeroList)
            {
                int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                item.LoadDataFromSysNpcHeroCfg(sysBigMapHeroCfg);

                response.AttackerHeroItems.Add(item);
            }

            UserRole userRole;
            UserHero userHero;
            UserFormation userFormation;
            Storage.Load(out userRole, out userHero, out userFormation, CurrentUserId);

            response.AllBattle.DefenderId = CurrentUserId;
            response.AllBattle.DefenderLevel = userRole.Level;
            response.AllBattle.DefenderHeadId = userRole.HeadId;
            response.AllBattle.DefenderName = userRole.NickName;

            //自己为防守方武将详细信息
            foreach (var formationItem in userFormation.BigMapDefFormation)
            {
                var heroId = formationItem.HeroId;
                var userHeroItem = userHero.FindByHeroId(heroId);
                if (userHeroItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", heroId);
                    return;
                }
                int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                item.LoadDataFromUserHeroItem(userHeroItem);
                item.Location = formationItem.Location;

                response.DefenderHeroItems.Add(item);
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 5005 获取红粉佳人、英雄豪杰 ID列表
    public class GetConcubineHeroActivityIdInfoResponse
    {
        /// <summary>
        /// 系统英雄id列表
        /// </summary>
        [Tag(1)]
        public List<int> HeroIdList { get; set; }
        /// <summary>
        /// 系统妃子id列表
        /// </summary>
        [Tag(2)]
        public List<int> ConcubineIdList { get; set; }
    }
    /// <summary>
    /// 获取红粉佳人、英雄豪杰 ID列表
    /// </summary>
    [GameCode(OpCode = 5005, ResponseType = typeof(GetConcubineHeroActivityIdInfoResponse))]
    public class GetConcubineHeroActivityIdInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetConcubineHeroActivityIdInfoResponse();
            response.HeroIdList =
                SysShopCfg.Items.Where(o => o.ShopType == ShopType.TenHero).Select(o => o.GoodsId).ToList();
            response.ConcubineIdList =
                SysShopCfg.Items.Where(o => o.ShopType == ShopType.TenConcubine).Select(o => o.GoodsId).ToList();

            response.HeroIdList = response.HeroIdList ?? new List<int>();
            response.ConcubineIdList = response.ConcubineIdList ?? new List<int>();

            ResultObj = response;
        }
    }
    #endregion
}
