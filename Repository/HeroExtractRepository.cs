using System;
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
            /// 抽取类型 1:钻石，2：铜币
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

            var heroExtract = Storage.Load<HeroExtract>(CurrentUserId, true);
            var list = new List<ExtractType>() { ExtractType.Money, ExtractType.Coin };
            foreach (var extractType in list)
            {
                var extractItem = new GetExtractInfoResponse.ExtractItem();
                extractItem.Type = extractType;
                extractItem.MaxNum = ConfigHelper.ExtractCfgData.MaxExtracts[(int)extractType - 1];
                extractItem.OnePrice = ConfigHelper.ExtractCfgData.OneExtractPrices[(int)extractType - 1];
                extractItem.TenPrice = ConfigHelper.ExtractCfgData.TenExtractPrices[(int)extractType - 1];

                if (extractType == ExtractType.Coin)
                {
                    extractItem.LaveNum = heroExtract.CoinExtractLaveNum;
                    extractItem.LaveTime = heroExtract.CoinExtractLaveTime;
                }
                else if (extractType == ExtractType.Money)
                {
                    extractItem.LaveNum = heroExtract.MoneyExtractLaveNum;
                    extractItem.LaveTime = heroExtract.MoneyExtractLaveTime;
                }

                response.Items.Add(extractItem);
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
        /// <param name="type"></param>
        /// <param name="special"></param>
        /// <returns></returns>
        public GetExtractResultResponse.ExtractItemsItem GetExtractItem(GameContext context, ExtractType type, int special = 0)
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
                if (type == ExtractType.Coin)
                    randExtractPrec = sysExtractPrecCfgItems.OrderByDescending(o => o.Num).FirstOrDefault();
                else if (type == ExtractType.Money)
                    randExtractPrec = sysExtractPrecCfgItems.FirstOrDefault(o => o.ItemType == ExtractItemType.Hero);
            }
            if (randExtractPrec == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "", type);
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
            if (extractItemsItem.Type == ExtractItemType.Item)
            {
                //随机获取道具
                var itemList =
                    SysToolCfg.Items.Where(o => o.ExtractWeights > 0 && o.Quality == (ItemQuality)quality).ToList();
                var rItemIndex = Utility.GetIndexFromWeightsList(itemList.Select(o => o.ExtractWeights).ToList());
                if (rItemIndex < itemList.Count)
                {
                    var rItem = itemList[rItemIndex];
                    rItem.AddToUser(CurrentUserId, opCode, 1);
                    extractItemsItem.SysId = rItem.Id;
                }
            }
            else if (extractItemsItem.Type == ExtractItemType.Hero)
            {
                //随机获取武将
                var sysHeroList =
                    SysHeroCfg.Items.Where(o => o.Quality == (ItemQuality)quality && o.ExtractWeights > 0).ToList();
                var rItemIndex = Utility.GetIndexFromWeightsList(sysHeroList.Select(o => o.ExtractWeights).ToList());
                if (rItemIndex < sysHeroList.Count)
                {
                    var rItem = sysHeroList[rItemIndex];

                    var userHero = Storage.Load<UserHero>(CurrentUserId, true);
                    if (userHero.ItemList.Exists(o => o.HeroId == rItem.Id))
                    {
                        //存在武将给武将碎片
                        var heroItem = userHero.FindByHeroId(rItem.Id);
                        if (heroItem != null)
                        {
                            //添加武将碎片
                            var chipNum = ConfigHelper.ExtractCfgData.ExchangeHeroChips[quality];
                            var userChip = Storage.Load<UserChip>(CurrentUserId, true);
                            userChip.AddChip(rItem.ChipId, chipNum, ToolType.HeroChip, opCode);
                            extractItemsItem.SysId = rItem.Id;
                            extractItemsItem.Num = 0;
                            extractItemsItem.ChipNum = randExtractPrec.Num;
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
                var rItemIndex = Utility.GetIndexFromWeightsList(sysConcubineList.Select(o => o.ExtractWeights).ToList());
                if (rItemIndex < sysConcubineList.Count)
                {
                    var rItem = sysConcubineList[rItemIndex];

                    var userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                    if (userConcubine.Items.Exists(o => o.ConcubineId == rItem.Id))
                    {
                        //存在妃子给妃子碎片
                        var concubineItem = userConcubine.Items.Find(o => o.ConcubineId == rItem.Id);
                        if (concubineItem != null)
                        {
                            var chipNum = ConfigHelper.ExtractCfgData.ExchangeConcubineChips[quality];
                            var userChip = Storage.Load<UserChip>(CurrentUserId, true);
                            userChip.AddChip(rItem.ChipId, chipNum, ToolType.ConcubineChip, opCode);
                            extractItemsItem.SysId = rItem.Id;
                            extractItemsItem.Num = 0;
                            extractItemsItem.ChipNum = randExtractPrec.Num;
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

            return extractItemsItem;
        }

        /// <summary>
        /// 判断背包格子是否满
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool JudgeBagIsFull(ExtractType type)
        {
            UserEquip userEquip;
            UserTool userTool;
            Storage.Load(out userEquip, out userTool, CurrentUserId);

            var num = 1;
            if (type == ExtractType.TenCoin || type == ExtractType.TenMoney) num = 10;
            //装备背包不足
            if (userEquip.IsFull || userEquip.LaveCapacity < num)
            {
                SetError(ResourceId.R_0000_EquipBagIsFull);
                return false;
            }
            //道具背包不足
            if (userTool.IsFull || userEquip.LaveCapacity < num)
            {
                SetError(ResourceId.R_0000_ToolBagIsFull);
                return false;
            }
            return true;
        }

        public override void Process(GameContext context)
        {
            if (!JudgeBagIsFull(Type)) return;

            var logItem = new GameLogItem();
            logItem.F1 = (int)Type;
            var opCode = Request.OpCode;

            var response = new GetExtractResultResponse();
            var heroExtract = Storage.Load<HeroExtract>(CurrentUserId, true);
            if (Type == ExtractType.Coin)
            {
                if (heroExtract.CoinExtractLaveNum > 0 && heroExtract.CoinExtractLaveTime <= 0)
                {
                    //免费铜币抽取一次并更新免费间隔时间
                    heroExtract.CoinExtractEndTime =
                        DateTime.Now.AddSeconds(ConfigHelper.ExtractCfgData.ExtractIntervals[(int)ExtractType.Coin - 1]);
                    heroExtract.CoinExtractNum += 1;
                }
                else
                {
                    //花费铜币抽取一次
                    var userRole = Storage.Load<UserRole>(CurrentUserId, true);
                    var needCoin = ConfigHelper.ExtractCfgData.OneExtractPrices[(int)ExtractType.Coin - 1];
                    if (userRole.Coin <= needCoin)
                    {
                        SetError(ResourceId.R_0000_CoinNotEnough);
                        return;
                    }

                    userRole.ConsumeResource(ItemType.Coin, opCode, needCoin);
                    logItem.F2 = needCoin;
                    //userRole.Coin -= needCoin;
                }

                var item = GetExtractItem(context, Type);
                response.Items.Add(item);
            }
            else if (Type == ExtractType.TenCoin)
            {
                var userRole = Storage.Load<UserRole>(CurrentUserId, true);
                var needCoin = ConfigHelper.ExtractCfgData.TenExtractPrices[(int)ExtractType.Coin - 1];
                if (userRole.Coin <= needCoin)
                {
                    SetError(ResourceId.R_0000_CoinNotEnough);
                    return;
                }
                //userRole.Coin -= needCoin;
                userRole.ConsumeResource(ItemType.Coin, opCode, needCoin);
                logItem.F2 = needCoin;

                //10连抽随机一次特殊处理
                var random = Util.GetRandom(0, 10);
                for (int i = 0; i < 10; i++)
                {
                    var item = GetExtractItem(context, ExtractType.Coin, i == random ? 1 : 0);
                    response.Items.Add(item);
                }
            }
            else if (Type == ExtractType.Money)
            {
                if (heroExtract.MoneyExtractLaveNum > 0 && heroExtract.MoneyExtractLaveTime <= 0)
                {
                    //免费铜币抽取一次并更新免费间隔时间
                    heroExtract.MoneyExtractEndTime =
                        DateTime.Now.AddSeconds(ConfigHelper.ExtractCfgData.ExtractIntervals[(int)ExtractType.Money - 1]);
                    heroExtract.MoneyExtractNum += 1;
                }
                else
                {
                    //花费钻石抽取一次
                    var userRole = Storage.Load<UserRole>(CurrentUserId, true);
                    var needMoney = ConfigHelper.ExtractCfgData.OneExtractPrices[(int)ExtractType.Money - 1];
                    if (userRole.TotalMoney <= needMoney)
                    {
                        SetError(ResourceId.R_0000_MoneyNotEnough);
                        return;
                    }
                    Utility.Concume(userRole, needMoney, SpecialStoreId.OneMoneyExtract);
                    logItem.F2 = needMoney;
                }

                var item = GetExtractItem(context, Type);
                response.Items.Add(item);
            }
            else if (Type == ExtractType.TenMoney)
            {
                var userRole = Storage.Load<UserRole>(CurrentUserId, true);
                var needMoney = ConfigHelper.ExtractCfgData.TenExtractPrices[(int)ExtractType.Money - 1];
                if (userRole.TotalMoney <= needMoney)
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
                    var item = GetExtractItem(context, ExtractType.Money, i == random ? 1 : 0);
                    response.Items.Add(item);
                }
            }

            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion
}
