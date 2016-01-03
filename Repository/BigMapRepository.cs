using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using MobileGame.Core;
using MobileGame.Core.Logging;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;
using Newtonsoft.Json;

namespace MobileGame.tianzi.Repository
{
    #region 公用方法
    public class BigMapCommon
    {
        /// <summary>
        /// 获取城池的列表
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="cityItems"></param>
        /// <returns></returns>
        public static List<GetBigMapInfoResponse.ResCityItem> GetResCityItems(int currentUserId, List<UserCityItem> cityItems, UserCity userCity)
        {
            var response = new List<GetBigMapInfoResponse.ResCityItem>();

            var userIdList = new List<int>();

            var serverMapCityActivity = Utility.GetServerMapCityActivity();
            //城池
            var serverMapCityItemIdList =
                cityItems.Where(o => o.ServerMapCityItemId > 0).Select(o => o.ServerMapCityItemId).ToList();

            var serverMapCityItemList = DataStorage.Current.LoadList<ServerMapCityItem>(serverMapCityItemIdList.ToArray(), true);
            foreach (var userCityItem in cityItems)
            {
                var serverMapCityItem = serverMapCityItemList.Find(o => o.Id == userCityItem.ServerMapCityItemId);
                if (serverMapCityItem == null)
                    throw new ApplicationException(string.Format("Not Find ServerMapCityItem,ServerMapCityItemId:{0}",
                        userCityItem.ServerMapCityItemId));

                serverMapCityItem.MatchIdList = serverMapCityItem.MatchIdList ?? new List<int>();
                if (!serverMapCityItem.MatchIdList.Contains(currentUserId)) serverMapCityItem.MatchIdList.Add(currentUserId);

                var item = new GetBigMapInfoResponse.ResCityItem();
                item.CityId = userCityItem.CityId;
                item.UserOrNpcLevel = serverMapCityItem.CityLevel;
                item.OwnerId = serverMapCityItem.OwnerId;
                item.UseSearchNum = (int)userCityItem.UseSearchNum.Value;
                item.BuySearchNum = (int)userCityItem.BuySearchNum.Value;
                //item.Product = userCityItem.Product;
                item.LastGainTime = serverMapCityItem.OccupiedOrGainTime;
                //item.Defense = serverMapCityItem.Defense;
                //item.Army = serverMapCityItem.Army;
                //item.Morale = (int)serverMapCityItem.Morale.Value;
                if (serverMapCityItem.OwnerType == OwnerType.User)
                    item.OwnerType = item.OwnerId == currentUserId ? OwnerType.Own : OwnerType.User;
                else
                {
                    item.OwnerType = OwnerType.Npc;
                }

                //检查是否对应于同一个城池ID，不一样改过来。刷新时是根据组去刷新的所以会变动这个无所谓
                //if (!serverMapCityItem.CityId.Equals(userCityItem.CityId))
                //{
                //    serverMapCityItem.CityId = userCityItem.CityId;
                //}

                if (item.OwnerType == OwnerType.Own)
                {
                    item.CanRefreshTime = DateTime.Now.AddYears(1);
                }
                else
                {
                    item.CanRefreshTime = userCityItem.MatchOrBeOccupiedTime.AddHours(8);
                }

                if (item.CanRefreshTime < DateTime.Now.AddYears(-1)) item.CanRefreshTime = DateTime.Now;

                if (serverMapCityItem.OwnerId > 0)
                {
                    if (serverMapCityItem.OwnerType == OwnerType.Npc)
                    {
                        var sysBigMapCfg = SysBigMapCfg.Find(serverMapCityItem.OwnerId);
                        if (sysBigMapCfg != null)
                            item.OwnerName = sysBigMapCfg.NickName;
                    }
                    else
                    {
                        userIdList.Add(serverMapCityItem.OwnerId);
                    }
                }

                //if (userCityItem.VisitorItems != null && userCityItem.VisitorItems.Any()) item.HaveVisitor = 1;
                //if (item.HaveVisitor == 0)
                //{
                //    var serverMapCityActivityItem =
                //        serverMapCityActivity.Items.FirstOrDefault(o => o.CityId == userCityItem.CityId);
                //    if (serverMapCityActivityItem != null && serverMapCityActivityItem.ActivityVisitorItems.Any())
                //    {
                //        item.HaveVisitor = 1;
                //    }
                //}
                //foreach (var visitorItem in userCityItem.VisitorItems)
                //{
                //    var type = Utility.GetIdExtractType(visitorItem.VisitorId);
                //    //if (type == ExtractItemType.Hero) item.HeroIdList.Add(visitorItem.VisitorId);
                //    //if (type == ExtractItemType.Concubine) item.ConcubineIdList.Add(visitorItem.VisitorId);
                //    //if (type == ExtractItemType.Hero) item.CityStatusList[(int)CityStatusType.Hero] = 1;
                //    //if (type == ExtractItemType.Concubine) item.CityStatusList[(int)CityStatusType.Concubine] = 1;
                //}

                //var serverMapCityActivityItem =
                //    serverMapCityActivity.Items.FirstOrDefault(o => o.CityId == userCityItem.CityId);
                //if (serverMapCityActivityItem != null)
                //{
                //    foreach (var visitorItem in serverMapCityActivityItem.ActivityVisitorItems)
                //    {
                //        var type = Utility.GetIdExtractType(visitorItem.VisitorId);

                //        if (type == ExtractItemType.Hero) item.HeroIdList.Add(visitorItem.VisitorId);
                //        if (type == ExtractItemType.Concubine) item.ConcubineIdList.Add(visitorItem.VisitorId);
                //        //if (type == ExtractItemType.Hero) item.CityStatusList[(int)CityStatusType.Hero] = 1;
                //        //if (type == ExtractItemType.Concubine) item.CityStatusList[(int)CityStatusType.Concubine] = 1;
                //    }
                //}

                foreach (var shopItem in userCity.ShopItems)
                {
                    if (shopItem.ShopType == ShopType.LuoYang && shopItem.CityId == userCityItem.CityId &&
                        shopItem.LeaveTime.ToTs() > 0 && shopItem.GoodsItems.Count > 0)
                        item.CityStatusList[(int)CityStatusType.LuoYang] = 1;

                    if (shopItem.ShopType == ShopType.Mysterious && shopItem.CityId == userCityItem.CityId &&
                        shopItem.LeaveTime.ToTs() > 0 && shopItem.GoodsItems.Count > 0)
                        item.CityStatusList[(int)CityStatusType.Mysterious] = 1;

                    if (shopItem.ShopType == ShopType.Western && shopItem.CityId == userCityItem.CityId &&
                        shopItem.LeaveTime.ToTs() > 0 && shopItem.GoodsItems.Count > 0)
                        item.CityStatusList[(int)CityStatusType.Western] = 1;
                }

                //item.HasBussiness =
                //    userCity.ShopItems.Exists(
                //        o => o.CityId == userCityItem.CityId && o.LeaveTime.ToTs() > 0)
                //        ? 1
                //        : 0;

                response.Add(item);
            }

            //获取实时昵称
            var roleList = DataStorage.Current.LoadList<UserRole>(userIdList.ToArray());
            foreach (var resCityItem in response)
            {
                var role = roleList.FirstOrDefault(o => o.Id == resCityItem.OwnerId);
                if (role != null)
                {
                    resCityItem.OwnerName = role.NickName;
                    resCityItem.UserOrNpcLevel = role.Level;
                    //resCityItem.ProtectEndTime = role.ProtectEndTime;
                    //resCityItem.OwnerHeadId = role.HeadId;
                }
            }

            return response;
        }

        /// <summary>
        /// 获取领地列表
        /// </summary>
        /// <param name="domainItems"></param>
        /// <returns></returns>
        public static List<GetBigMapInfoResponse.ResDomainItem> GetResDomainItems(List<UserDomainItem> domainItems)
        {
            var response = new List<GetBigMapInfoResponse.ResDomainItem>();
            var userIdList = new List<int>();

            var investigateSaveSenconds = ConfigHelper.BigMapCfgData.InvestigateSaveSenconds;
            //领地
            foreach (var userDomainItem in domainItems)
            {
                var item = new GetBigMapInfoResponse.ResDomainItem();


                var sysCityCfg = userDomainItem.SysCityCfg;
                item.UserOrNpcLevel = sysCityCfg.Level;
                //item.Product = sysCityCfg.Product;

                item.CityId = userDomainItem.CityId;
                item.OwnerId = userDomainItem.OwnerId;
                item.OwnerType = userDomainItem.OwnerType;
                item.CanRefreshTime = userDomainItem.MatchOrBeOccupiedTime.AddDays(1);
                item.InvestigateTime = userDomainItem.InvestigateTime.AddSeconds(investigateSaveSenconds);
                //设置为未侦查过
                if (DateTime.Now.AddSeconds(-investigateSaveSenconds) > item.InvestigateTime)
                    item.InvestigateTime = DateTime.Now.AddDays(-1);

                if (item.InvestigateTime > DateTime.Now)
                {
                    item.CanRobResList = userDomainItem.CanRobResList;
                }

                if (userDomainItem.OwnerId > 0)
                {
                    if (userDomainItem.OwnerType == OwnerType.Npc)
                    {

                        var sysBigMapCfg = SysBigMapCfg.Find(userDomainItem.OwnerId);
                        if (sysBigMapCfg != null)
                        {
                            item.OwnerName = sysBigMapCfg.NickName;
                            item.UserOrNpcLevel = sysBigMapCfg.Level;
                        }
                    }
                    else
                    {
                        userIdList.Add(userDomainItem.OwnerId);
                    }
                }

                response.Add(item);
            }

            //获取实时昵称
            var roleList = DataStorage.Current.LoadList<UserRole>(userIdList.ToArray());
            foreach (var resDomainItem in response)
            {
                var role = roleList.FirstOrDefault(o => o.Id == resDomainItem.OwnerId);
                if (role != null)
                {
                    resDomainItem.OwnerName = role.NickName;
                    resDomainItem.OwnerHeadId = role.HeadId;
                    resDomainItem.UserOrNpcLevel = role.Level;
                    resDomainItem.ProtectEndTime = role.ProtectEndTime;
                }
            }

            return response;
        }

        /// <summary>
        /// 获取可以抢夺的资源列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="subtract">是否直接扣除被抢方资源然后添加给抢劫方</param>
        /// <param name="opCode">影响的接口</param>
        /// <returns></returns>
        public static List<ItemPair> GetCanRobResList(int userId, int subtract = 0, int opCode = 0)
        {
            var canRobResList = new List<ItemPair>();
            UserBuilding userBuilding;
            UserConcubine userConcubine;
            try
            {
                DataStorage.Current.Load(out userBuilding, out userConcubine, userId, true);
            }
            catch (Exception)
            {
                Utility.ClearRdata(userId);
                DataStorage.Current.Load(out userBuilding, out userConcubine, userId, true);
            }

            var buildingItems = userBuilding.Items.OrderBy(o => o.MoneyType).ToList();
            //var bigMapCfgData = ConfigHelper.BigMapCfgData;
            var robBuildingPrec = 0; //bigMapCfgData.RobBuildPrec;
            var robConcubinePrec = 0; //bigMapCfgData.RobConcubinePrec;
            foreach (var userBuildingItem in buildingItems)
            {
                //这里显示资源保护百分比，抢劫后宫时，抢去后宫资源的（1-保护百分比），抢去妃子身上资源（1-保护百分比）
                //modify by hql at 2015.9.26
                var protectResPrec = userBuildingItem.SysBuildingCfg.Protect; //保护百分比
                robBuildingPrec = 100 - protectResPrec;
                robConcubinePrec = robBuildingPrec;

                //modify by hql at 2015.10.24
                //直接在获取被抢劫数函数里头减去敌方资源
                var canRobRes = userBuildingItem.GetBuildingRobNum(robBuildingPrec,opCode,subtract);
                //canRobRes += cityLevel * bigMapCfgData.MinRobResModulus;
                AddResToList(canRobResList, userBuildingItem.MoneyType, canRobRes);

                canRobRes = userBuildingItem.GetConcubinesRobNum(robConcubinePrec, userConcubine, subtract);
                AddResToList(canRobResList, userBuildingItem.MoneyType, canRobRes);

                //var index = (int)userBuildingItem.MoneyType - 2;
                //抢建筑里头的资源
                //canRobRes = (int)(userBuildingItem.Storage * 1.0 * robBuildingPrec / 100);
                //if (subtract == 1)
                //{
                //    if (canRobRes < 0) canRobRes = 0;
                //    if (subtract == 1 && canRobRes > 0)
                //    {
                //        //扣除玩家身上的资源，并且不立即分配建筑存储量
                //        Utility.ConsumeResource(null, (ItemType)(index + 2), opCode, canRobRes, userId, 0);
                //    }

                //    userBuildingItem.ConcubineIdList = userBuildingItem.ConcubineIdList ?? new List<int>();
                //    //抢妃子身上未收获的资源
                //    if (userBuildingItem.ConcubineIdList.Count > 0)
                //    {
                //        var concubineList =
                //            userConcubine.Items.Where(o => userBuildingItem.ConcubineIdList.Contains(o.Id)).ToList();
                //        foreach (var userConcubineItem in concubineList)
                //        {
                //            //先把产量计算出来
                //            userConcubineItem.GainToOutput((int)OpcodeType.InvestigateRequest);
                //            canRobRes = (int)(userConcubineItem.OutPut * 1.0 * robConcubinePrec / 100);
                //            if (canRobRes > userConcubineItem.OutPut) canRobRes = userConcubineItem.OutPut;
                //            if (canRobRes < 0) canRobRes = 0;
                //            AddResToList(canRobResList, userBuildingItem.MoneyType, canRobRes);

                //            if (subtract == 1)
                //            {
                //                //扣除妃子身上的资源
                //                userConcubineItem.OutPut -= canRobRes;
                //            }
                //        }
                //    }
                //}
            }
            if (subtract == 1)
            {
                //重新分配建筑里头的资源
                Utility.ReassignBuildingResource(null, ItemType.Coin, userId);
                Utility.ReassignBuildingResource(null, ItemType.Wood, userId);
                Utility.ReassignBuildingResource(null, ItemType.Stone, userId);
                Utility.ReassignBuildingResource(null, ItemType.Iron, userId);
            }
            return canRobResList;
        }

        /// <summary>
        /// 添加抢夺的资源到列表中取
        /// </summary>
        /// <param name="robedResList"></param>
        /// <param name="type"></param>
        /// <param name="num"></param>
        public static void AddResToList(List<ItemPair> robedResList, MoneyType type, int num)
        {
            var specialToolType = SpecialToolId.Coin;
            switch (type)
            {
                case MoneyType.Coin: specialToolType = SpecialToolId.Coin; break;
                case MoneyType.Wood: specialToolType = SpecialToolId.Wood; break;
                case MoneyType.Stone: specialToolType = SpecialToolId.Stone; break;
                case MoneyType.Iron: specialToolType = SpecialToolId.Iron; break;
            }
            var item = robedResList.FirstOrDefault(o => o.ItemId == (int)specialToolType);
            if (item == null)
            {
                robedResList.Add(new ItemPair((int)specialToolType, num));
            }
            else
            {
                item.Num += num;
            }
        }

        /// <summary>
        /// 获取获取到的道具【包括铜钱】列表
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="opCode"></param>
        /// <param name="needSp"></param>
        /// <param name="heroExp"></param>
        /// <returns></returns>
        public static List<ItemPair> GetTooList(int npcId, int opCode, out int needSp, out int heroExp)
        {
            var coin = 0;
            var sysBigMapCfg = SysBigMapCfg.Find(npcId);
            if (sysBigMapCfg == null) throw new ApplicationException(string.Format("SysBigMapCfg:Id:{0}找不到", npcId));
            needSp = sysBigMapCfg.NeedSp;
            heroExp = sysBigMapCfg.HeroExp;
            coin = sysBigMapCfg.Coin;
            var wood = sysBigMapCfg.Wood;
            var stone = sysBigMapCfg.Stone;
            var iron = sysBigMapCfg.Iron;
            var toolIdList = new List<ItemPair>();
            if (coin > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Coin, coin));
            if (wood > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Wood, wood));
            if (stone > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Stone, stone));
            if (iron > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Iron, iron));


            //添加概率道具
            var prob = sysBigMapCfg.Probability;
            foreach (var i in sysBigMapCfg.ToolList)
            {
                if (i == 0) break;
                var hit = Util.IsHit(prob * 1.0 / 100);
                if (hit)
                {
                    toolIdList.Add(new ItemPair(i, 1));
                }
            }
            return toolIdList;
        }

        /// <summary>
        /// 获取获取到的道具【包括铜钱】列表
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="opCode"></param>
        /// <param name="needSp"></param>
        /// <param name="heroExp"></param>
        /// <returns></returns>
        public static List<ItemPair> GetDiabloTooList(int npcId, int opCode, out int needSp, out int heroExp)
        {
            var coin = 0;
            var sysDiabloCfg = SysDiabloCfg.Find(npcId);
            if (sysDiabloCfg == null) throw new ApplicationException(string.Format("sysDiabloCfg:Id:{0}找不到", npcId));
            needSp = sysDiabloCfg.NeedSp;
            heroExp = sysDiabloCfg.HeroExp;
            coin = sysDiabloCfg.Coin;
            var wood = sysDiabloCfg.Wood;
            var stone = sysDiabloCfg.Stone;
            var iron = sysDiabloCfg.Iron;
            var toolIdList = new List<ItemPair>();
            if (coin > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Coin, coin));
            if (wood > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Wood, wood));
            if (stone > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Stone, stone));
            if (iron > 0) toolIdList.Add(new ItemPair((int)SpecialToolId.Iron, iron));


            //添加概率道具
            var prob = sysDiabloCfg.Probability;
            foreach (var i in sysDiabloCfg.ToolList)
            {
                if (i == 0) break;
                var hit = Util.IsHit(prob * 1.0 / 100);
                if (hit)
                {
                    toolIdList.Add(new ItemPair(i, 1));
                }
            }
            return toolIdList;
        }

        /// <summary>
        /// 添加战斗的战利品
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="userRole"></param>
        /// <param name="userHero"></param>
        /// <param name="userTool"></param>
        /// <param name="tooList"></param>
        /// <param name="heroExp"></param>
        /// <param name="opCode"></param>
        public static void AddReward(AllBattle battle, UserRole userRole, UserHero userHero, UserTool userTool, List<ItemPair> tooList,
             int heroExp, int opCode)//int userExp,
        {
            //添加武将经验
            var heroList = DataStorage.Current.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
            foreach (var i in heroList)
            {
                var heroItem = userHero.FindByHeroId(i.HeroId);
                heroItem.AddExp(heroExp, opCode);
            }

            //添加资源或者道具
            for (int i = 0; i < tooList.Count; i++)
            {
                var robedRes = tooList[i].Num;
                var specialToolType = (SpecialToolId)tooList[i].ItemId;
                switch (specialToolType)
                {
                    case SpecialToolId.Coin:
                        Utility.AddResource(userRole, ItemType.Coin, opCode, robedRes);
                        break;
                    case SpecialToolId.Wood:
                        Utility.AddResource(userRole, ItemType.Wood, opCode, robedRes);
                        break;
                    case SpecialToolId.Stone:
                        Utility.AddResource(userRole, ItemType.Stone, opCode, robedRes);
                        break;
                    case SpecialToolId.Iron:
                        Utility.AddResource(userRole, ItemType.Iron, opCode, robedRes);
                        break;
                    case SpecialToolId.Repute:
                        Utility.AddResource(userRole, ItemType.Repute, opCode, robedRes);
                        break;
                    case SpecialToolId.Charm:
                        Utility.AddResource(userRole, ItemType.Charm, opCode, robedRes);
                        break;
                    case SpecialToolId.Money:
                        RoleManager.AddGiveMoney(userRole, robedRes);
                        break;
                    default:
                        {
                            var itemId = (int)specialToolType;
                            if (itemId > 9999999)
                            {//道具
                                userTool.TryAdd(itemId, 1, opCode);
                            }
                            else
                            {
                                //白色装备会掉落整件
                                UserEquip userEquip = DataStorage.Current.Load<UserEquip>(userRole.Id, true);
                                userEquip.AddEquipToUser(itemId, opCode);
                            }
                            break;
                        }
                }
            }
        }


        /// <summary>
        /// 检测设置爵位封号
        /// </summary>
        /// <param name="userRole"></param>
        public static void CheckAndSetTitleLevel(UserRole userRole)
        {
            var titleLevel = 0;
            var list = SysTitleCfg.Items.OrderBy(o => o.NeedRepute).ToList();
            foreach (var sysTitleCfg in list)
            {
                if (userRole.Repute >= sysTitleCfg.NeedRepute) titleLevel++;
                else break;
            }
            if (titleLevel != userRole.TitleLevel && titleLevel != 0)
            {
                userRole.TitleLevel = titleLevel;
                var nameList = SysTitleNameCfg.Items.Select(o => o.Bo).ToList();
                switch ((TitleType)titleLevel)
                {
                    case TitleType.Bo: nameList = SysTitleNameCfg.Items.Select(o => o.Bo).ToList(); break;
                    case TitleType.TingHou: nameList = SysTitleNameCfg.Items.Select(o => o.TingHou).ToList(); break;
                    case TitleType.XiangHou: nameList = SysTitleNameCfg.Items.Select(o => o.XiangHou).ToList(); break;
                    case TitleType.JunGong: nameList = SysTitleNameCfg.Items.Select(o => o.JunGong).ToList(); break;
                    case TitleType.GuoGong: nameList = SysTitleNameCfg.Items.Select(o => o.GuoGong).ToList(); break;
                    case TitleType.JunWang: nameList = SysTitleNameCfg.Items.Select(o => o.JunWang).ToList(); break;
                    case TitleType.QinWang: nameList = SysTitleNameCfg.Items.Select(o => o.QinWang).ToList(); break;
                    default: break;
                }
                var count = nameList.Count;
                var index = Util.GetRandom(0, count);
                userRole.TitleName = nameList[index];
            }
        }

        /// <summary>
        /// 获取下发的商店物品列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<ResShopGoodTtem> GetShopItemList(List<ShopGoodTtem> list)
        {
            var llist = new List<ResShopGoodTtem>();
            foreach (var shopGoodTtem in list)
            {
                var item = new ResShopGoodTtem();
                item.Id = shopGoodTtem.Id;
                item.Buyed = shopGoodTtem.Buyed;

                var sysShopCfg = SysShopCfg.Find(o => o.Id == shopGoodTtem.Id);
                if (sysShopCfg != null)
                {
                    item.ItemId = sysShopCfg.GoodsId;
                    item.ItemType = sysShopCfg.ItemType;
                    item.MoneyType = sysShopCfg.MoneyType;
                    item.Num = shopGoodTtem.SNum == 0 ? sysShopCfg.Num : shopGoodTtem.SNum;
                    item.TotalPrice = sysShopCfg.SellPrice * item.Num;
                }
                llist.Add(item);
            }
            return llist;
        }
    }
    #endregion

    #region 11000 获取大地图界面信息
    public enum CityStatusType
    {
        /// <summary>
        /// 是否有到访的武将
        /// </summary>
        //Hero = 0,
        /// <summary>
        /// 是否有到访的妃子
        /// </summary>
        //Concubine = 1,
        /// <summary>
        /// 是否洛阳商人
        /// </summary>
        LuoYang = 0,
        /// <summary>
        /// 是否神秘商人
        /// </summary>
        Mysterious = 1,
        /// <summary>
        /// 是否西域商人
        /// </summary>
        Western = 2,
    }
    /// <summary>
    /// 大地图界面信息
    /// </summary>
    public class GetBigMapInfoResponse
    {
        public GetBigMapInfoResponse()
        {
            OpenedAreaIdList = new List<int>();
            CityItems = new List<ResCityItem>();
            //SearchItems = new List<ResSearchItem>();
            DomainItems = new List<ResDomainItem>();
        }
        /// <summary>
        /// 已经探索过的区域id列表
        /// </summary>
        [Tag(1)]
        public List<int> OpenedAreaIdList { get; set; }
        /// <summary>
        /// 已开启的城池列表
        /// </summary>
        [Tag(2)]
        public List<ResCityItem> CityItems { get; set; }
        /// <summary>
        /// 已开启的领地列表
        /// </summary>
        [Tag(3)]
        public List<ResDomainItem> DomainItems { get; set; }

        /// <summary>
        /// 侦查消耗系数【消耗铜钱=系数*等级】
        /// </summary>
        [Tag(4)]
        public int InvestigateCoinModulus { get; set; }

        /// <summary>
        /// 可以征收的时间
        /// </summary>
        [Tag(5)]
        public DateTime GainTime { get; set; }

        /// <summary>
        /// 未读的事件数量
        /// </summary>
        [Tag(6)]
        public int UnReadEventNum { get; set; }

        /// <summary>
        /// 寻访列表
        /// </summary>
        //[Tag(7)]
        //public List<ResSearchItem> SearchItems { get; set; }

        /// <summary>
        /// 未读的寻访结果数量
        /// </summary>
        [Tag(8)]
        public int UnReadSearchResultNum { get; set; }

        /// <summary>
        /// 每元宝清除的秒数【计算所需元宝】
        /// </summary>
        [Tag(9)]
        public int EveryMoneyRemoveSecond { get; set; }

        /// <summary>
        /// 剧情任务的ID,-1说明剧情任务都完成了
        /// </summary>
        [Tag(10)]
        public int StoryTaskId { get; set; }

        /// <summary>
        /// 自己护盾截止的时间
        /// </summary>
        [Tag(11)]
        public DateTime ProtectEndTime { get; set; }

        /// <summary>
        /// 暗黑军团活动NpcId，为零说明还没开启活动或者已经通关
        /// </summary>
        [Tag(12)]
        public int DiabloNpcId { get; set; }

        /// <summary>
        /// 暗黑军团活动截止时间
        /// </summary>
        [Tag(13)]
        public DateTime DiabloEndTime { get; set; }

        /// <summary>
        /// 护盾道具购买冷却时间列表（[0,0,0]对应三个道具16000000、16000001、16000002）
        /// </summary>
        [Tag(14)]
        public List<int> CoolLaveTimeList { get; set; }

        public class ResCityItem
        {
            public ResCityItem()
            {
                OwnerName = "";
                CityStatusList = new List<int>() { 0, 0, 0 };//0, 0, 
                //ConcubineIdList = new List<int>();
                //HeroIdList = new List<int>();
                //ProtectEndTime = DateTime.Now.AddMinutes(-10);
            }
            /// <summary>
            /// 系统的城池ID
            /// </summary>
            [Tag(1)]
            public int CityId { get; set; }
            /// <summary>
            /// 玩家或者NPC的等级
            /// </summary>
            [Tag(2)]
            public int UserOrNpcLevel { get; set; }
            /// <summary>
            /// 占领者类型【0：自己，1：NPC,2：其他玩家】
            /// </summary>
            [Tag(3)]
            public OwnerType OwnerType { get; set; }
            /// <summary>
            /// 占领者Id
            /// </summary>
            [Tag(4)]
            public int OwnerId { get; set; }
            /// <summary>
            /// 占领者【玩家/NPC】昵称
            /// </summary>
            [Tag(5)]
            public string OwnerName { get; set; }
            /// <summary>
            /// 产量
            /// </summary>
            //[Tag(6)]
            //public int Product { get; set; }
            /// <summary>
            /// 到访的系统妃子Id列表——活动的（动态出现）
            /// </summary>
            //[Tag(7)]
            //public List<int> ConcubineIdList { get; set; }
            /// <summary>
            /// 到访的系统武将Id列表——活动的（动态出现）
            /// </summary>
            //[Tag(8)]
            //public List<int> HeroIdList { get; set; }
            /// <summary>
            /// 城池状态列表【是否有洛阳商人、是否有神秘商人、是否有西域商人 eg[1,0,0]】
            /// 1代表有，0代表无
            /// </summary>
            [Tag(9)]
            public List<int> CityStatusList { get; set; }
            /// <summary>
            /// 可以刷新玩家的时间
            /// </summary>
            [Tag(10)]
            public DateTime CanRefreshTime { get; set; }
            /// <summary>
            /// 上次收获的时间
            /// </summary>
            [Tag(11)]
            public DateTime LastGainTime { get; set; }
            /// <summary>
            /// 已使用的寻访次数（总次数固定三次客户端写死）
            /// </summary>
            [Tag(12)]
            public int UseSearchNum { get; set; }
            /// <summary>
            /// 今日已重置的寻访次数
            /// </summary>
            [Tag(13)]
            public int BuySearchNum { get; set; }
            /// <summary>
            /// 护盾截止的时间
            /// </summary>
            //[Tag(14)]
            //public DateTime ProtectEndTime { get; set; }
        }

        public class ResDomainItem
        {
            public ResDomainItem()
            {
                OwnerName = "";
                CanRobResList = new List<int>();
                ProtectEndTime = DateTime.Now.AddMinutes(-10);
            }
            /// <summary>
            /// 系统的城池ID
            /// </summary>
            [Tag(1)]
            public int CityId { get; set; }
            /// <summary>
            /// 玩家或者NPC的等级
            /// </summary>
            [Tag(2)]
            public int UserOrNpcLevel { get; set; }
            /// <summary>
            /// 城池等级
            /// </summary>
            //[Tag(2)]
            //public int CityLevel { get; set; }
            /// <summary>
            /// 占领者类型【0：自己，1：NPC,2：其他玩家】
            /// </summary>
            [Tag(3)]
            public OwnerType OwnerType { get; set; }
            /// <summary>
            /// 占领者Id
            /// </summary>
            [Tag(4)]
            public int OwnerId { get; set; }
            /// <summary>
            /// 占领者【玩家/NPC】昵称
            /// </summary>
            [Tag(5)]
            public string OwnerName { get; set; }
            /// <summary>
            /// 产量
            /// </summary>
            //[Tag(6)]
            //public int Product { get; set; }
            /// <summary>
            /// 侦查截止时间[用于按钮显示侦查还是查看，侦查需要消耗铜钱]
            /// </summary>
            [Tag(7)]
            public DateTime InvestigateTime { get; set; }

            /// <summary>
            /// 可获得的资源列表
            /// </summary>
            [Tag(8)]
            public List<int> CanRobResList { get; set; }
            /// <summary>
            /// 占领者的头像ID(是玩家的时候 才有值)
            /// </summary>
            [Tag(9)]
            public int OwnerHeadId { get; set; }
            /// <summary>
            /// 可以刷新玩家的时间
            /// </summary>
            [Tag(10)]
            public DateTime CanRefreshTime { get; set; }
            /// <summary>
            /// 护盾截止的时间
            /// </summary>
            [Tag(11)]
            public DateTime ProtectEndTime { get; set; }
        }
    }
    /// <summary>
    /// 获取大地图界面信息
    /// </summary>
    [GameCode(OpCode = 11000, ResponseType = typeof(GetBigMapInfoResponse))]
    public class GetBigMapInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);

            //检查
            userCity.CheckAndAddCityDomain();

            var response = new GetBigMapInfoResponse();
            response.OpenedAreaIdList = userCity.OpenedAreaIdList;
            response.CityItems = BigMapCommon.GetResCityItems(CurrentUserId, userCity.CityItems, userCity);

            //GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0,
            //    new GameLogItem() {S1 = JsonConvert.SerializeObject(userCity.CityItems)});

            response.DomainItems = BigMapCommon.GetResDomainItems(userCity.DomainItems);
            response.InvestigateCoinModulus = ConfigHelper.BigMapCfgData.InvestigateCoinModulus;

            //寻访列表
            //var searchList = userCity.CityItems.Where(o => o.SearchHeroId > 0 && o.SearchEndTime.ToTs() > 0).ToList();
            //foreach (var userCityItem in searchList)
            //{
            //    response.SearchItems.Add(new GetBigMapInfoResponse.ResSearchItem()
            //    {
            //        SearchCityId = userCityItem.CityId,
            //        SearchHeroId = userCityItem.SearchHeroId,
            //        SearchEndTime = userCityItem.SearchEndTime,
            //    });
            //}

            response.EveryMoneyRemoveSecond = ConfigHelper.ClearCdCfgData.HeroXunFang;
            response.GainTime = userCity.GetRealGainTime();
            response.UnReadEventNum = userCity.EventItems.Count(o => o.IsRead == 0);
            response.UnReadSearchResultNum = userCity.SearchItems.Count(o => o.IsRead == 0);
            response.StoryTaskId = userCity.StoryTaskId < 0 ? 501001 : userCity.StoryTaskId;
            response.StoryTaskId = userCity.StoryTaskId > 10000000 ? 501001 : userCity.StoryTaskId;
            //response.StoryTaskId = userCity.StoryTaskId > 10000000 ? -1 : userCity.StoryTaskId;
            response.ProtectEndTime = userRole.ProtectEndTime;
            response.CoolLaveTimeList = userRole.CoolLaveTimeList.Select(o=>o.ToTs()).ToList();
            while (response.CoolLaveTimeList.Count < 3)
            {
                response.CoolLaveTimeList.Add(0);
            }
            
            response.DiabloNpcId = userCity.DiabloNpcId;
            response.DiabloEndTime = userCity.DiabloEndTime;

            userCity.LastReqDataTime = DateTime.Now;

            ResultObj = response;
        }
    }
    #endregion

    #region 11001 探索区域
    /// <summary>
    /// 探索后区域的信息
    /// </summary>
    public class OpenAreaResponse
    {
        /// <summary>
        /// 探索区域的城池列表
        /// </summary>
        [Tag(1)]
        public List<GetBigMapInfoResponse.ResCityItem> CityItems { get; set; }
        /// <summary>
        /// 探索区域的领地列表
        /// </summary>
        [Tag(2)]
        public List<GetBigMapInfoResponse.ResDomainItem> DomainItems { get; set; }
    }
    /// <summary>
    /// 探索区域【探索完重新请求11000接口】
    /// </summary>
    [GameCode(OpCode = 11001, ResponseType = typeof(OpenAreaResponse))]
    public class OpenAreaRequest : GameHandler
    {
        /// <summary>
        /// 探索的区域id
        /// </summary>
        public int AreaId { get; set; }
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);

            string errorMsg;
            if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
            {
                SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                return;
            }
            if (userCity.OpenedAreaIdList.Contains(AreaId))
            {
                //已探索过该区域
                SetError(ResourceId.R_11001_AreaAlreadyOpened);
                return;
            }

            var sysAreaCfy = SysAreaCfg.Find(AreaId);
            if (sysAreaCfy == null)
            {
                //找不到该id的区域配置文件
                SetError(ResourceId.R_0000_IdNotExist, "SysAreaCfg:Id", AreaId);
                return;
            }
            if (sysAreaCfy.NeedLevel > userRole.Level)
            {
                //等级不足
                SetError(ResourceId.R_0000_UserLowLevel, sysAreaCfy.NeedLevel);
                return;
            }

            if (userRole.Coin < sysAreaCfy.Coin)
            {
                //铜钱不足
                SetError(ResourceId.R_0000_CoinNotEnough);
                return;
            }

            var cityItems = new List<UserCityItem>();
            var domainItems = new List<UserDomainItem>();
            foreach (var i in sysAreaCfy.CityIdList)
            {
                var sysCityCfg = SysCityCfg.Find(i);
                if (sysCityCfg == null)
                {
                    //找不到该id的城池配置文件
                    SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", i);
                    return;
                }

                if (sysCityCfg.Type == CityType.City)
                    cityItems.Add(userCity.OpenCity(i));
                else if (sysCityCfg.Type == CityType.Domain)
                    domainItems.Add(userCity.OpenDomain(i));
            }
            userCity.OpenedAreaIdList.Add(AreaId);

            //扣铜币
            Utility.ConsumeResource(userRole, ItemType.Coin, Request.OpCode, sysAreaCfy.Coin);

            //返回新探索出来的城池信息
            ResultObj = new OpenAreaResponse()
            {
                CityItems = BigMapCommon.GetResCityItems(CurrentUserId, cityItems, userCity),
                DomainItems = BigMapCommon.GetResDomainItems(domainItems),
            };
        }
    }
    #endregion

    #region 11002 城主请求城池防守信息
    public class GetCityDefendResponse
    {
        public GetCityDefendResponse()
        {
            DefendItems = new List<DefendItem2>();
        }
        /// <summary>
        /// 城池防守阵容列表
        /// </summary>
        [Tag(1)]
        public List<DefendItem2> DefendItems { get; set; }
    }
    /// <summary>
    /// 请求城池防守信息
    /// </summary>
    [GameCode(OpCode = 11002, ResponseType = typeof(GetCityDefendResponse))]
    public class GetCityDefendRequest : GameHandler
    {
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserHero userHero;
            Storage.Load(out userCity, out userHero, CurrentUserId, true);

            var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                //城池未开启
                SetError(ResourceId.R_11002_CityNotOpen);
                return;
            }

            var serverMapCityItemId = userCityItem.ServerMapCityItemId;
            var serverMapCityItem = Storage.Load<ServerMapCityItem>(serverMapCityItemId, true);
            if (serverMapCityItem.IsNew)
            {
                //全服的城池信息未创建
                SetError(ResourceId.R_11002_ServerCityNotCreate);
                return;
            }
            if (serverMapCityItem.OwnerId != CurrentUserId)
            {
                //未占领该城池，无权布防
                SetError(ResourceId.R_11003_ServerCityOccupiedByOther);
                return;
            }

            var response = new GetCityDefendResponse();
            foreach (var defendItem in serverMapCityItem.DefendItems)
            {
                var userHeroItem = userHero.Items.FirstOrDefault(o => o.HeroId == defendItem.HeroId);
                if (userHeroItem != null)
                {
                    var item = new DefendItem2();
                    item.Turn = defendItem.Turn;
                    item.Location = defendItem.Location;

                    item.Combat = userHeroItem.Combat;
                    item.HeroId = userHeroItem.HeroId;
                    item.Level = userHeroItem.Level;
                    item.Id = userHeroItem.Id;
                    item.Star = userHeroItem.StarLevel;

                    response.DefendItems.Add(item);
                }
            }
            ResultObj = response;
        }
    }
    #endregion

    #region 11003 城主保存城池防守阵容
    /// <summary>
    /// 保存城池防守阵容
    /// </summary>
    [GameCode(OpCode = 11003)]
    public class SaveCityDefendRequest : GameHandler
    {
        /// <summary>
        /// 系统城池ID
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// 武将Id列表【9001401,9001401,9001401,9001401,9001401,9001401,9001401,9001401】
        /// </summary>
        public string HeroIdArray { get; set; }
        /// <summary>
        /// 武将Id列表
        /// </summary>
        public List<int> HeroIdList;
        /// <summary>
        /// 位置Id列表【11,12,23,11,12,23,11,12,23】
        /// </summary>
        public string LocationIdArray { get; set; }
        /// <summary>
        /// 位置Id列表
        /// </summary>
        public List<int> LocationIdList;

        public override bool InitParams(GameContext context)
        {
            if (HeroIdList.Count != LocationIdList.Count) return false;
            return true;
        }
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserHero userHero;
            Storage.Load(out userCity, out userHero, CurrentUserId, true);

            var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                //城池未开启
                SetError(ResourceId.R_11002_CityNotOpen);
                return;
            }
            var sysCityCfg = SysCityCfg.Find(CityId);
            if (sysCityCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", CityId);
                return;
            }

            var serverMapCityItemId = userCityItem.ServerMapCityItemId;
            var serverMapCityItem = Storage.Load<ServerMapCityItem>(serverMapCityItemId, true);
            if (serverMapCityItem.IsNew)
            {
                //全服的城池信息未创建
                SetError(ResourceId.R_11002_ServerCityNotCreate);
                return;
            }
            if (serverMapCityItem.OwnerId != CurrentUserId)
            {
                //未占领该城池，无权布防
                SetError(ResourceId.R_11003_ServerCityOccupiedByOther);
                return;
            }

            //清空布阵信息
            serverMapCityItem.DefendItems.Clear();

            var index = 0;
            foreach (var i in HeroIdList)
            {
                var userHeroItem = userHero.Items.FirstOrDefault(o => o.HeroId == i);
                if (userHeroItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "userHero:HeroId", i);
                    return;
                }
                string errorMsg;
                if (!userHeroItem.JudgeHeroConform(sysCityCfg.BattleType, out errorMsg))
                {
                    //武将不符合条件
                    SetError(errorMsg, userHeroItem.SysHeroCfg.Name);
                    return;
                }
                var item = new DefendItem();
                item.Turn = 1;
                item.Location = (LocationNumber)LocationIdList[index];
                item.HeroId = userHeroItem.HeroId;
                item.Level = userHeroItem.Level;
                item.Id = userHeroItem.Id;
                serverMapCityItem.DefendItems.Add(item);

                index++;
            }

            //var tempList = new List<DefendItem>();
            //foreach (var defendItem in serverMapCityItem.DefendItems)
            //{
            //    tempList.Add(defendItem);
            //}
            //serverMapCityItem.DefendItems = new List<DefendItem>();

            //var index = 0;
            //foreach (var i in TurnIdList)
            //{
            //    var heroId = HeroIdList[index];
            //    var heroLevel = LevelIdList[index];
            //    var id = 0;
            //    if (IdList.Count > index) id = IdList[index];
            //    var location = (LocationNumber)LocationIdList[index];

            //    var tempItem = tempList.FirstOrDefault(o => o.HeroId == heroId && o.Level == heroLevel);
            //    if (tempItem == null)
            //    {
            //        tempItem = tempList.FirstOrDefault(o => o.Id == id);
            //        if (tempItem == null)
            //        {
            //            SetError(ResourceId.R_0000_IllegalParam);
            //            return;
            //        }
            //    }

            //    //添加新的布阵
            //    var item = new DefendItem();
            //    item.Turn = i;
            //    item.Location = location;
            //    item.HeroId = heroId;
            //    item.Level = heroLevel;
            //    item.Id = id;
            //    serverMapCityItem.DefendItems.Add(item);

            //    tempList.Remove(tempItem);
            //    index++;
            //}

            ////没有上阵的武将
            //if (tempList.Count > 0)
            //{
            //    foreach (var defendItem in tempList)
            //    {
            //        var item = new DefendItem();
            //        item.Turn = 0;
            //        item.Location = LocationNumber.None;
            //        item.HeroId = defendItem.HeroId;
            //        item.Level = defendItem.Level;
            //        item.Id = defendItem.Id;
            //        serverMapCityItem.DefendItems.Add(item);
            //    }
            //}
        }
    }
    #endregion

    #region 11004 获取领地/城池防守阵型
    public class GetDefendFormationResponse
    {
        public GetDefendFormationResponse()
        {
            OwnFormationItems = new List<FormationItem>();
            TargetUserItem = new BattleUserItem();
        }
        /// <summary>
        /// 自己武将列表
        /// </summary>
        [Tag(1)]
        public List<FormationItem> OwnFormationItems { get; set; }
        /// <summary>
        /// 防守方用户信息
        /// </summary>
        [Tag(2)]
        public BattleUserItem TargetUserItem { get; set; }
    }
    /// <summary>
    /// 获取领地/城池防守阵型
    /// </summary>
    [GameCode(OpCode = 11004, ResponseType = typeof(GetDefendFormationResponse))]
    public class GetDefendFormationRequest : GameHandler
    {
        /// <summary>
        /// 领地/城池系统ID
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            var sysCityCfg = SysCityCfg.Find(CityId);
            if (sysCityCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", CityId);
                return;
            }

            var response = new GetDefendFormationResponse();
            response.TargetUserItem.CityId = CityId;

            UserCity userCity;
            UserFormation userFormation;
            Storage.Load(out userCity, out userFormation, CurrentUserId, true);

            #region 领地
            if (sysCityCfg.Type == CityType.Domain)
            {
                var userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == CityId);
                if (userDomainItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "DomainItemList:CityId", CityId);
                    return;
                }
                if (userDomainItem.OwnerType == OwnerType.Own)
                {
                    SetError(ResourceId.R_11004_DomainAlreadyOwn);
                    return;
                }
                else if (userDomainItem.OwnerType == OwnerType.Npc)
                {
                    var sysBigMapCfg = SysBigMapCfg.Find(userDomainItem.OwnerId);
                    response.TargetUserItem.HeadId = sysBigMapCfg.HeadId;
                    response.TargetUserItem.Level = sysBigMapCfg.Level;
                    response.TargetUserItem.NickName = sysBigMapCfg.NickName;

                    var npcHeroList = SysBigMapHeroCfg.Items.Where(o => o.NpcId == userDomainItem.OwnerId && o.Turn == 1).ToList();
                    foreach (var sysBigMapHeroCfg in npcHeroList)
                    {
                        response.TargetUserItem.Combat += sysBigMapHeroCfg.Combat;

                        //武将
                        response.TargetUserItem.HeroList.Add(new FormationDetailItem()
                        {
                            Location = sysBigMapHeroCfg.Location,
                            HeroId = sysBigMapHeroCfg.HeroId,
                            StarLevel = sysBigMapHeroCfg.StarLevel,
                            Level = sysBigMapHeroCfg.Level,
                            AttackSpeed = sysBigMapHeroCfg.RealAttackSpeed,
                        });
                    }
                }
                else
                {
                    UserRole defenderUserRole;
                    UserHero defenderUserHero;
                    UserFormation defenderUserFormation;
                    Storage.Load(out defenderUserRole, out defenderUserHero, out defenderUserFormation, userDomainItem.OwnerId);
                    response.TargetUserItem.HeadId = defenderUserRole.HeadId;
                    response.TargetUserItem.Level = defenderUserRole.Level;
                    response.TargetUserItem.NickName = defenderUserRole.NickName;

                    foreach (var formationItem in defenderUserFormation.BigMapDefFormation)
                    {
                        var heroItem = defenderUserHero.Items.FirstOrDefault(o => o.HeroId == formationItem.HeroId);
                        if (heroItem != null)
                        {
                            response.TargetUserItem.Combat += heroItem.Combat;
                            //武将
                            response.TargetUserItem.HeroList.Add(new FormationDetailItem()
                            {
                                Location = formationItem.Location,
                                HeroId = heroItem.HeroId,
                                StarLevel = heroItem.StarLevel,
                                Level = heroItem.Level,
                                AttackSpeed = heroItem.AttackSpeed,
                            });
                        }
                    }
                }
            }
            #endregion

            #region 城池
            else
            {
                response.OwnFormationItems = userFormation.GetFormationItemsByBattleType(sysCityCfg.BattleType);

                var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
                if (userCityItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "CityItemList:CityId", CityId);
                    return;
                }

                var serverMapCityItem = DataStorage.Current.Load<ServerMapCityItem>(userCityItem.ServerMapCityItemId, true);
                if (serverMapCityItem.IsNew)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "ServerMapCityItem:Id", userCityItem.ServerMapCityItemId);
                    return;
                }

                if (userCityItem.OwnerType == OwnerType.Npc)
                {
                    var sysBigMapCfg = SysBigMapCfg.Find(serverMapCityItem.OwnerId);
                    if (sysBigMapCfg == null)
                    {
                        if (serverMapCityItem.OwnerId > 0)
                        {
                            var userRole = Storage.Load<UserRole>(serverMapCityItem.OwnerId);
                            if (!userRole.IsNew)
                            {
                                userCityItem.OwnerType = OwnerType.User;
                                response.TargetUserItem.HeadId = userRole.HeadId;
                                response.TargetUserItem.Level = userRole.Level;
                                response.TargetUserItem.NickName = userRole.NickName;
                            }
                            else
                            {
                                SetError(ResourceId.R_0000_IdNotExist, "SysBigMapCfg:Id:", serverMapCityItem.OwnerId);
                                return;
                            }
                        }
                        else
                        {
                            SetError(ResourceId.R_0000_IdNotExist, "SysBigMapCfg:Id:", serverMapCityItem.OwnerId);
                            return;
                        }
                    }
                    else
                    {
                        response.TargetUserItem.HeadId = sysBigMapCfg.HeadId;
                        response.TargetUserItem.Level = sysBigMapCfg.Level;
                        response.TargetUserItem.NickName = sysBigMapCfg.NickName;
                    }

                    //城防是NPC时
                    //foreach (var defendItem in serverMapCityItem.DefendItems.
                    //    Where(o => o.Turn == 1 && o.Location != LocationNumber.None).ToList())
                    foreach (var sysBigMapHeroCfg in SysBigMapHeroCfg.Items.
                        Where(o => o.NpcId == serverMapCityItem.OwnerId && o.Turn == 1).ToList())
                    {
                        //var sysBigMapHeroCfg = SysBigMapHeroCfg.Items.FirstOrDefault(o => o.Id == defendItem.Id && o.Turn == 1);
                        //if (sysBigMapHeroCfg == null)
                        //{
                        //    SetError(ResourceId.R_0000_IdNotExist, "sysBigMapHeroCfg:Id", defendItem.Id);
                        //    return;
                        //}

                        response.TargetUserItem.Combat += sysBigMapHeroCfg.Combat;
                        //武将
                        response.TargetUserItem.HeroList.Add(new FormationDetailItem()
                        {
                            Location = sysBigMapHeroCfg.Location,
                            HeroId = sysBigMapHeroCfg.HeroId,
                            StarLevel = sysBigMapHeroCfg.StarLevel,
                            Level = sysBigMapHeroCfg.Level
                        });
                    }
                }
                else
                {
                    var userId = serverMapCityItem.OwnerId;
                    UserRole userRole;
                    UserHero userHero;
                    Storage.Load(out userRole, out userHero, userId);
                    response.TargetUserItem.HeadId = userRole.HeadId;
                    response.TargetUserItem.Level = userRole.Level;
                    response.TargetUserItem.NickName = userRole.NickName;

                    //城防是玩家
                    foreach (var defendItem in serverMapCityItem.DefendItems)
                    {
                        var userHeroItem = userHero.Items.FirstOrDefault(o => o.HeroId == defendItem.HeroId);
                        if (userHeroItem != null)
                        {
                            response.TargetUserItem.Combat += userHeroItem.Combat;
                            //武将
                            response.TargetUserItem.HeroList.Add(new FormationDetailItem()
                            {
                                Location = defendItem.Location,
                                HeroId = userHeroItem.HeroId,
                                StarLevel = userHeroItem.StarLevel,
                                Level = userHeroItem.Level
                            });
                        }
                    }
                }
            }
            #endregion

            ResultObj = response;
        }
    }
    #endregion

    #region 11005 挑战领地/城池
    /// <summary>
    /// 挑战领地【非玩家占领】/城池
    /// </summary>
    public class BigMapPkNpcResponse
    {
        public BigMapPkNpcResponse()
        {
            AttackerHeroItems = new List<BattleHeroItem>();

            DefenderHeroItemsList = new List<ResBattleHeroList>();
        }
        /// <summary>
        /// 战役id
        /// </summary>
        [Tag(1)]
        public int BattleId { get; set; }
        /// <summary>
        /// 攻击方武将列表
        /// </summary>
        [Tag(2)]
        public List<BattleHeroItem> AttackerHeroItems { get; set; }
        /// <summary>
        /// 防守方武将列表3【以列表的长度判断是否有NPC】
        /// </summary>
        [Tag(3)]
        public List<ResBattleHeroList> DefenderHeroItemsList { get; set; }
    }
    /// <summary>
    /// 挑战领地/城池
    /// </summary>
    [GameCode(OpCode = 11005, ResponseType = typeof(BigMapPkNpcResponse))]
    public class BigMapPkNpcRequest : BattleHandler
    {
        public override void Process(GameContext context)
        {
            var response = new BigMapPkNpcResponse();

            var sysCityCfg = SysCityCfg.Find(TargetId);
            if (sysCityCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", TargetId);
                return;
            }

            UserRole userRole;
            UserCity userCity;
            Storage.Load(out userRole, out userCity, CurrentUserId, true);

            var needSp = ConfigHelper.BigMapCfgData.BattleNeedSp;
            if (userRole.Sp < needSp)
            {
                //体力不足
                SetError(ResourceId.R_0000_SpNotEnough);
                return;
            }

            //扣除体力,只扣体力，不加经验，在结算的时候加经验
            Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, needSp, 0, 0);
            //扣除体力
            //Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, needSp);
            //userRole.Sp -= needSp;

            var battleId = Util.GetSequence(typeof(AllBattle), 0);
            var battle = Storage.Load<AllBattle>(battleId, true);
            battle.Param1 = TargetId;
            //var battle = KVEntity.CreateNew<AllBattle>();//TIP:需要保存的实体 使用load createnew适合实体中的列表项！！！

            battle.AttackerId = CurrentUserId;
            battle.AttackerLevel = userRole.Level;
            battle.AttackerHeadId = userRole.HeadId;
            battle.AttackerName = userRole.NickName;

            //攻击方武将详细信息
            UserHero userHero;
            UserFormation userFormation;
            Storage.Load(out userHero, out userFormation, CurrentUserId, true);
            var index = 0;
            //userFormation.AttFormation.Clear();
            var newTotalCombat = 0;
            foreach (var i in HeroIdList)
            {
                var userHeroItem = userHero.FindByHeroId(i);
                if (userHeroItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", i);
                    return;
                }
                //if (userHeroItem.Status != HeroStatus.Idle && userHeroItem.StatusEndTime.ToTs() > 0)
                //{
                //    SetError(ResourceId.R_0000_HeroNotIdle);
                //    return;
                //}
                string errorMsg;
                if (!userHeroItem.JudgeHeroConform(sysCityCfg.BattleType, out errorMsg))
                {
                    //武将不符合条件
                    SetError(errorMsg, userHeroItem.SysHeroCfg.Name);
                    return;
                }
                newTotalCombat += userHeroItem.Combat;
                int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                item.LoadDataFromUserHeroItem(userHeroItem);
                item.Location = (LocationNumber)LocationIdList[index];

                battle.BattleAttackerHeroItemIdList.Add(battleHeroItemId);
                response.AttackerHeroItems.Add(item);

                index++;

                //保存最新的阵容列表
                //userFormation.AttFormation.Add(new FormationItem() { HeroId = i, Location = item.Location });
            }

            //设置攻击阵型
            userFormation.SetAttFormations(sysCityCfg.BattleType, HeroIdList, LocationIdList, newTotalCombat);

            //if (newTotalCombat > userFormation.AttMaxCombat)
            //{
            //    userFormation.SetMaxComatAndFormation(newTotalCombat, userFormation.AttFormation);
            //    //userFormation.StrongestFormation = userFormation.AttFormation;
            //    //userFormation.AttMaxCombat = newTotalCombat;
            //}

            #region 领地
            if (sysCityCfg.Type == CityType.Domain)
            {
                var userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == TargetId);
                if (userDomainItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "DomainItemList:CityId", TargetId);
                    return;
                }
                if (userDomainItem.OwnerType == OwnerType.Own)
                {
                    SetError(ResourceId.R_11004_DomainAlreadyOwn);
                    return;
                }
                else if (userDomainItem.OwnerType == OwnerType.Npc)
                {
                    battle.WarType = WarType.DomainNpc;
                    //TODO:查找配置
                    var sysBigMapCfg = SysBigMapCfg.Find(userDomainItem.OwnerId);
                    if (sysBigMapCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysBigMapCfg:Id", userDomainItem.OwnerId);
                        return;
                    }
                    battle.DefenderId = sysBigMapCfg.Id;
                    battle.DefenderLevel = sysBigMapCfg.Level;
                    battle.DefenderHeadId = sysBigMapCfg.HeadId;
                    battle.DefenderName = sysBigMapCfg.NickName;

                    var turnList = new List<int>() { 1, 2, 3 };
                    //var npcHeroList = SysNpcFormationCfg.Items.Where(o => o.NpcId == userDomainItem.OwnerId).ToList();
                    var npcHeroList = SysBigMapHeroCfg.Items.Where(o => o.NpcId == userDomainItem.OwnerId).ToList();
                    foreach (var turn in turnList)
                    {
                        if (turn <= 0) break;
                        var tempList = npcHeroList.Where(o => o.Turn == turn).ToList();
                        if (tempList.Count == 0) break;

                        var battleDefenderHeroItemIdList = new ListIntItem();
                        var hList = new ResBattleHeroList();
                        //NPC防守阵型
                        foreach (var sysBigMapHeroCfg in tempList)
                        {
                            int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                            var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                            item.LoadDataFromSysNpcHeroCfg(sysBigMapHeroCfg);

                            //battle.BattleDefenderHeroItemIdList.Add(battleHeroItemId);
                            battleDefenderHeroItemIdList.IdItems.Add(battleHeroItemId);
                            //hList.HeroItems.Add(item);

                            //攻打NPC不用存储武将信息
                            //var item = new BattleHeroItem();
                            //item.LoadDataFromSysNpcHeroCfg(sysNpcFormationCfg);
                            hList.HeroItems.Add(item);
                        }
                        battle.DefenderHeroItemIdListList.Add(battleDefenderHeroItemIdList);
                        response.DefenderHeroItemsList.Add(hList);
                    }
                }
                else
                {
                    //攻打领地中匹配到的玩家
                    battle.WarType = WarType.DomainUser;
                    UserRole targetUserRole;
                    UserHero targetUserHero;
                    UserFormation targetUserFormation;
                    Storage.Load(out targetUserRole, out targetUserHero, out targetUserFormation, userDomainItem.OwnerId, true);
                    battle.DefenderId = userDomainItem.OwnerId;
                    battle.DefenderLevel = targetUserRole.Level;
                    battle.DefenderHeadId = targetUserRole.HeadId;
                    battle.DefenderName = targetUserRole.NickName;

                    if (targetUserRole.BeAttackEndTime.ToTs() > 0)
                    {
                        //正在被攻打
                        SetError(ResourceId.R_11005_BeAttacking);
                        return;
                    }

                    var laveProtectSencond = targetUserRole.ProtectEndTime.ToTs();
                    if (laveProtectSencond > 0)
                    {
                        //不能攻击，玩家开启了护盾
                        SetError(ResourceId.R_11005_IsProtecting);
                        return;
                    }

                    //清空自己的护盾时间
                    userRole.ProtectEndTime = DateTime.Now;

                    //大地图攻打领地战斗时锁定玩家的时间
                    targetUserRole.BeAttackEndTime = DateTime.Now.AddSeconds(ConfigHelper.BattleExpireTime);
                    targetUserRole.BeAttackType = BeAttackType.Attacking;

                    var hList = new ResBattleHeroList();
                    var battleDefenderHeroItemIdList = new ListIntItem();
                    foreach (var formationItem in targetUserFormation.BigMapDefFormation)
                    {
                        var targetHeroItem = targetUserHero.FindByHeroId(formationItem.HeroId);
                        if (targetHeroItem == null)
                        {
                            SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", formationItem.HeroId);
                            return;
                        }
                        int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                        var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                        item.LoadDataFromUserHeroItem(targetHeroItem);
                        item.Location = formationItem.Location;

                        battleDefenderHeroItemIdList.IdItems.Add(battleHeroItemId);

                        hList.HeroItems.Add(item);
                    }
                    battle.DefenderHeroItemIdListList.Add(battleDefenderHeroItemIdList);

                    response.DefenderHeroItemsList.Add(hList);
                }
            }
            #endregion

            #region 城池
            else
            {
                var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == TargetId);
                if (userCityItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "CityItemList:CityId", TargetId);
                    return;
                }

                var serverMapCityItem = DataStorage.Current.Load<ServerMapCityItem>(userCityItem.ServerMapCityItemId, true);
                if (serverMapCityItem.IsNew)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "ServerMapCityItem:Id", userCityItem.ServerMapCityItemId);
                    return;
                }

                UserHero defenderUserHero = null;
                UserRole defenderUserRole = null;
                if (serverMapCityItem.OwnerType == OwnerType.Npc)
                {
                    var npcId = serverMapCityItem.OwnerId;
                    var sysBigMapCfg = SysBigMapCfg.Find(npcId);
                    if (sysBigMapCfg == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "SysBigMapCfg:Id", npcId);
                        return;
                    }
                    battle.DefenderId = npcId;
                    battle.DefenderLevel = sysBigMapCfg.Level;
                    battle.DefenderHeadId = sysBigMapCfg.HeadId;
                    battle.DefenderName = sysBigMapCfg.NickName;
                    battle.WarType = WarType.CityNpc;
                }
                else
                {
                    if (TargetId == CurrentUserId)
                    {
                        //不能和自己战斗
                        SetError(ResourceId.R_0000_CanNotBattleOwn);
                        return;
                    }

                    //清空自己的护盾时间
                    //userRole.ProtectEndTime = DateTime.Now;

                    var userId = serverMapCityItem.OwnerId;
                    Storage.Load(out defenderUserHero, out defenderUserRole, userId);
                    battle.DefenderId = userId;
                    battle.DefenderLevel = defenderUserRole.Level;
                    battle.DefenderHeadId = defenderUserRole.HeadId;
                    battle.DefenderName = defenderUserRole.NickName;
                    battle.WarType = WarType.CityUser;

                    battle.ListIntParam1.Add(serverMapCityItem.Defense);
                    battle.ListIntParam1.Add(serverMapCityItem.Army);
                    battle.ListIntParam1.Add((int)serverMapCityItem.Morale.Value);
                    battle.ListIntParam1.Add((int)userCityItem.UseInternalAffairsNum.Value);
                    //battle.ListIntParam1.Add((int)serverMapCityItem.UseNum.Value);
                }
                var turnList = new List<int>() { 1, 2, 3 };

                if (serverMapCityItem.OwnerType == OwnerType.Npc)
                {
                    foreach (var turn in turnList)
                    {
                        if (turn <= 0) break;

                        var defendItems =
                                SysBigMapHeroCfg.Items.Where(o => o.NpcId == serverMapCityItem.OwnerId && o.Turn == turn)
                                    .ToList();
                        if (defendItems.Count == 0) break;

                        var hList = new ResBattleHeroList();
                        var defenderHeroItemIdList = new ListIntItem();
                        foreach (var defendItem in defendItems)
                        {
                            //攻打NPC不用存储武将信息
                            var item = new BattleHeroItem();
                            item.LoadDataFromSysNpcHeroCfg(defendItem);
                            item.Location = defendItem.Location;
                            hList.HeroItems.Add(item);
                        }
                        if (defenderHeroItemIdList.IdItems.Count > 0)
                            battle.DefenderHeroItemIdListList.Add(defenderHeroItemIdList);
                        response.DefenderHeroItemsList.Add(hList);
                    }
                }
                else
                {
                    foreach (var turn in turnList)
                    {
                        if (turn <= 0) break;

                        var defendItems = serverMapCityItem.DefendItems.
                            Where(o => o.Turn == turn && o.Location != LocationNumber.None).ToList();
                        if (defendItems.Count == 0) break;

                        var hList = new ResBattleHeroList();
                        var defenderHeroItemIdList = new ListIntItem();
                        foreach (var defendItem in defendItems)
                        {
                            if (defenderUserHero != null)
                            {
                                var userHeroItem = defenderUserHero.Items.FirstOrDefault(o => o.HeroId == defendItem.HeroId);
                                if (userHeroItem != null)
                                {
                                    int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                                    var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                                    item.LoadDataFromUserHeroItem(userHeroItem, serverMapCityItem);

                                    defenderHeroItemIdList.IdItems.Add(battleHeroItemId);
                                    item.Location = defendItem.Location;
                                    hList.HeroItems.Add(item);
                                }
                            }
                        }
                        if (defenderHeroItemIdList.IdItems.Count > 0)
                            battle.DefenderHeroItemIdListList.Add(defenderHeroItemIdList);
                        response.DefenderHeroItemsList.Add(hList);
                    }
                }


            }
            #endregion

            if (battle.WarType == WarType.CityUser || battle.WarType == WarType.DomainUser)
            {
                //增加战斗技能光环效果
                Utility.AddBattleSkillRing(response.AttackerHeroItems, response.DefenderHeroItemsList[0].HeroItems);
            }
            else
            {
                //增加战斗技能光环效果
                Utility.AddBattleSkillRing(response.AttackerHeroItems);
            }


            response.BattleId = battle.Id;
            ResultObj = response;
        }
    }
    #endregion

    #region 11006 提交大地图挑战结果
    public class SubmitBigMapResultResponse
    {
        public SubmitBigMapResultResponse()
        {
            ToolList = new List<ItemPair>();
        }
        /// <summary>
        /// 获得的用户经验
        /// </summary>
        [Tag(1)]
        public int UserExp { get; set; }
        /// <summary>
        /// 获得的英雄经验
        /// </summary>
        [Tag(2)]
        public int HeroExp { get; set; }
        /// <summary>
        /// 获得的道具及资源列表
        /// </summary>
        [Tag(3)]
        public List<ItemPair> ToolList { get; set; }
        /// <summary>
        /// 出现的商人类型
        /// </summary>
        [Tag(4)]
        public ShopType ShopType { get; set; }
        /// <summary>
        /// 商人出现在的城池Id
        /// </summary>
        [Tag(5)]
        public int BussinessShowCityId { get; set; }
    }
    /// <summary>
    /// 提交大地图挑战结果
    /// </summary>
    [GameCode(OpCode = 11006, ResponseType = typeof(SubmitBigMapResultResponse))]
    public class SubmitBigMapResultRequest : GameHandler
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }
        /// <summary>
        /// 攻击方是否胜利【如果失败，传防守方死亡人数的负数过来】
        /// </summary>
        [ParamCheck(IsPositive = false)]
        public int IsWin { get; set; }
        /// <summary>
        /// 战斗评分[-1、-2、-3、-4、1]
        /// </summary>
        public int BattleScore { get; set; }
        /// <summary>
        /// 回合字符串列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string RoundsStr { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (BattleId == 0 || IsWin < -4 || IsWin > 1) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new SubmitBigMapResultResponse();

            var battle = Storage.Load<AllBattle>(BattleId, true);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "BattleId", BattleId);
                return;
            }
            var defenderDieHeroNum = 0;
            if (IsWin < 0)
            {
                defenderDieHeroNum = -IsWin;
                IsWin = 0;
            }
            if (battle.IsWin != -1)
            {
                SetError(ResourceId.R_0000_BattleHaveResult);
                return;
            }
            if (battle.AttackerId != CurrentUserId)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }
            var warType = battle.WarType;
            if (warType != WarType.DomainNpc && warType != WarType.DomainUser &&
                warType != WarType.CityNpc && warType != WarType.CityUser)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            battle.IsWin = IsWin;
            battle.BattleScore = BattleScore;

            var defendUserCity = new UserCity();

            UserCity userCity;
            UserRole userRole;
            UserHero userHero;
            UserTool userTool;
            Storage.Load(out userCity, out userRole, out userHero, out userTool, CurrentUserId, true);

            var needSp = 0;
            var heroExp = 0;

            var cityId = battle.Param1;
            var sysCityCfg = SysCityCfg.Find(cityId);
            if (sysCityCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", cityId);
                return;
            }
            ShopType shopType = ShopType.None;
            int bussinessShowCityId = 0;
            if (sysCityCfg.Type == CityType.Domain)
            {
                var userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == cityId);
                if (userDomainItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserDomainItem:CityId", cityId);
                    return;
                }

                if (IsWin == 1)
                {
                    #region 领地NPC
                    if (warType == WarType.DomainNpc)
                    {
                        //打NPC获得的物品【铜钱、道具、武将经验】
                        response.ToolList = BigMapCommon.GetTooList(userDomainItem.OwnerId, Request.OpCode, out needSp,
                            out heroExp);
                        var repute = ConfigHelper.BigMapCfgData.AttackNpcRepute;
                        response.ToolList.Add(new ItemPair((int)SpecialToolId.Repute, repute));
                    }
                    #endregion

                    #region 领地玩家
                    else if (warType == WarType.DomainUser)
                    {
                        //获取胜利抢到的资源，直接添加给玩家, sysCityCfg.Level
                        response.ToolList = BigMapCommon.GetCanRobResList(battle.DefenderId, 1, Request.OpCode);
                        var repute = ConfigHelper.BigMapCfgData.AttackUserRepute;
                        //主公经验 武将经验
                        needSp = 6;
                        heroExp = battle.DefenderLevel * 10;

                        //防守方减少声望
                        //Utility.ConsumeResource(null, ItemType.Repute, Request.OpCode,
                        //ConfigHelper.BigMapCfgData.BeRobedRepute, battle.DefenderId);

                        //保护时间、且重置战斗时间
                        UserRole defenderUserRole;
                        //添加事件——通知领地里匹配到的玩家“有人打他成功且抢资源了”
                        Storage.Load(out defendUserCity, out defenderUserRole, userDomainItem.OwnerId, true);
                        defendUserCity.AddEvent(EventType.Main, IsWin, CurrentUserId, 0, null,
                            null, BattleId, response.ToolList);

                        response.ToolList.Add(new ItemPair((int)SpecialToolId.Repute, repute));

                        //添加护盾时间
                        if (defenderUserRole.ProtectEndTime.ToTs() > 0)
                            defenderUserRole.ProtectEndTime = defenderUserRole.ProtectEndTime.AddHours(1);
                        else defenderUserRole.ProtectEndTime = DateTime.Now.AddHours(1);

                        //删除玩家，在该玩家大地图已经不显示自己了
                        defendUserCity.ShowMineOfOtherUserIdList.Remove(CurrentUserId);

                        //护盾有变化 通知大地图有匹配这个玩家的所有玩家！
                        defendUserCity.NoticeOtherUserList();

                        //修正锁定战斗时间
                        defenderUserRole.BeAttackEndTime = DateTime.Now.AddSeconds(ConfigHelper.ResultExpireTime);
                        defenderUserRole.BeAttackType = BeAttackType.Result;

                        //广播
                        var msg = LangResource.GetLangResource(ResourceId.R_0000_AttackDomainWinMsg, userRole.Id,
                                                             userRole.NickName, defenderUserRole.Id, defenderUserRole.NickName);
                        if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);
                    }
                    #endregion

                    if (userCity.DomainItems.Count(o => o.OwnerType == OwnerType.Own) == 0)
                    {
                        //之前全部领地已经不是自己的时候，赋值最后请求的时间从现在开始计算起，领地匹配NPC或者玩家
                        userCity.LastReqTime = DateTime.Now;
                    }

                    //当前进攻方占领领地
                    userDomainItem.OwnerId = CurrentUserId;
                    userDomainItem.OwnerType = OwnerType.Own;
                    //userDomainItem.OccupiedOrGainTime = DateTime.Now;

                    //神秘/西域商人出现的地方
                    bussinessShowCityId = userCity.ShowMysteriousOrWesternShop(0, out shopType);
                }
                else
                {
                    //失败
                    if (warType == WarType.DomainUser)
                    {
                        //添加事件——通知领地里匹配到的玩家“有人打他但失败了”
                        UserRole defenderUserRole;
                        Storage.Load(out defendUserCity, out defenderUserRole, userDomainItem.OwnerId, true);
                        defendUserCity.AddEvent(EventType.Main, IsWin, CurrentUserId, 0, null, null, BattleId);

                        //失败后直接解锁
                        //var defenderUserRole = Storage.Load<UserRole>(userDomainItem.OwnerId, true);
                        defenderUserRole.BeAttackEndTime = DateTime.Now;
                        defenderUserRole.BeAttackType = BeAttackType.None;
                    }
                }

                //添加领地战斗次数
                Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.DomainBattle);
            }
            else if (sysCityCfg.Type == CityType.City)
            {
                var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == cityId);
                if (userCityItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserCityItem:CityId", cityId);
                    return;
                }

                var serverMapCityItem = DataStorage.Current.Load<ServerMapCityItem>(
                    userCityItem.ServerMapCityItemId, true);
                if (serverMapCityItem.IsNew)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "ServerMapCityItem:Id", userCityItem.ServerMapCityItemId);
                    return;
                }

                if (IsWin == 1)
                {
                    //通知其他可以看到该城的玩家有变化
                    var userCityList = Storage.LoadList<UserCity>(serverMapCityItem.MatchIdList.ToArray(), true);
                    foreach (var city in userCityList)
                    {
                        if (city.Id == CurrentUserId) continue;
                        var otherCityItem = city.CityItems.FirstOrDefault(o => o.CityId == cityId);
                        if (otherCityItem == null)
                        {
                            serverMapCityItem.MatchIdList.Remove(city.Id);
                            continue;
                        }
                        //if (otherCityItem.OwnerType == OwnerType.Own && serverMapCityItem.OwnerId == city.Id)
                        //{
                        //    city.GainCityItems(otherCityItem.Id);
                        //}
                        otherCityItem.Status = 1;
                        otherCityItem.OwnerType = OwnerType.User;
                    }

                    if (warType == WarType.CityNpc)
                    {
                        //打NPC获得的物品【铜钱、道具、武将经验】
                        response.ToolList = BigMapCommon.GetTooList(serverMapCityItem.OwnerId, Request.OpCode,
                            out needSp, out heroExp);
                    }
                    else if (warType == WarType.CityUser)
                    {
                        //主公经验 武将经验
                        needSp = 6;
                        var battleHeroExpModulus = ConfigHelper.BigMapCfgData.BattleHeroExpModulus;
                        heroExp = battle.DefenderLevel * battleHeroExpModulus;


                        //添加事件——有人来攻打城池成功！
                        defendUserCity = Storage.Load<UserCity>(serverMapCityItem.OwnerId, true);
                        var defendUserCityItem =
                            defendUserCity.CityItems.FirstOrDefault(o => o.ServerMapCityItemId == serverMapCityItem.Id);
                        if (defendUserCityItem != null)
                        {
                            //抢夺城池中100%资源
                            var robedResNum = defendUserCity.GainCityItems(defendUserCityItem.Id, 0);
                            var specialToolId = Utility.GetSpecialToolId(defendUserCityItem.MoneyType);
                            response.ToolList.Add(new ItemPair((int)specialToolId, robedResNum));


                            //var defendSysCityCfg = SysCityCfg.Items.FirstOrDefault(o => o.Id == serverMapCityItem.CityId);
                            defendUserCity.AddEvent(EventType.City, IsWin, CurrentUserId, defendUserCityItem.CityId, null,
                                defendUserCityItem.SysCityCfg.Name, BattleId, response.ToolList);
                        }

                        //if (CurrentUserId != battle.AttackerId)
                        //defendUserCity.AddEvent(EventType.City, IsWin, CurrentUserId, cityId, null,
                        //    userCityItem.SysCityCfg.Name, BattleId, response.ToolList);
                    }

                    //当前进攻方占领城池
                    serverMapCityItem.OwnerId = CurrentUserId;
                    serverMapCityItem.OwnerType = OwnerType.User;
                    serverMapCityItem.CityLevel = userRole.Level;
                    serverMapCityItem.OccupiedOrGainTime = DateTime.Now;
                    //userCityItem.OccupiedOrGainTime = DateTime.Now;
                    userCityItem.OwnerType = OwnerType.Own;

                    //添加任务新达成
                    Utility.AddMainLineTaskGoalData(CurrentUserId, MainLineType.OccupiedCity, userCityItem.CityId);

                    //另外玩家占领了 内政都设置为50，管理内政次数清零
                    serverMapCityItem.Defense = serverMapCityItem.Army = 50;
                    serverMapCityItem.Morale -= serverMapCityItem.Morale.Value;
                    serverMapCityItem.Morale += 50;
                    //serverMapCityItem.UseNum -= serverMapCityItem.UseNum.Value;
                    //替换城防为刚才攻击的阵型
                    serverMapCityItem.DefendItems = new List<DefendItem>();
                    var attackerHeroList =
                        Storage.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
                    foreach (var battleHeroItem in attackerHeroList)
                    {
                        var defendItem = new DefendItem();
                        defendItem.Turn = 1;
                        defendItem.Location = battleHeroItem.Location;
                        defendItem.HeroId = battleHeroItem.HeroId;
                        defendItem.Id = battleHeroItem.HeroId;
                        defendItem.Level = battleHeroItem.Level;

                        serverMapCityItem.DefendItems.Add(defendItem);
                    }

                    //神秘/西域商人出现的地方
                    bussinessShowCityId = userCity.ShowMysteriousOrWesternShop(sysCityCfg.Id, out shopType);
                }
                else
                {
                    if (warType == WarType.CityUser)// && CurrentUserId != battle.AttackerId
                    {
                        //添加事件——有人来攻打城池但失败~
                        defendUserCity = Storage.Load<UserCity>(serverMapCityItem.OwnerId, true);

                        var defendUserCityItem =
                            defendUserCity.CityItems.FirstOrDefault(o => o.ServerMapCityItemId == serverMapCityItem.Id);
                        if (defendUserCityItem != null)
                        {
                            defendUserCity.AddEvent(EventType.City, IsWin, CurrentUserId, defendUserCityItem.CityId, null,
                                defendUserCityItem.SysCityCfg.Name, BattleId);
                        }

                        if (defenderDieHeroNum >= 1)
                        {
                            //至少死一个，城防降低
                            var descendingDefenseArmy =
                                ConfigHelper.BigMapDefenseCfgData.DescendingDefenseArmyList[defenderDieHeroNum - 1];
                            serverMapCityItem.Defense -= descendingDefenseArmy;
                            if (serverMapCityItem.Defense < 0) serverMapCityItem.Defense = 0;
                            serverMapCityItem.Army -= descendingDefenseArmy;
                            if (serverMapCityItem.Army < 0) serverMapCityItem.Army = 0;
                        }
                    }
                }
            }

            //失败了也会有主公经验得到，之前接口就添加了经验了！这里只是显示一下
            response.UserExp = needSp * 1;
            if (IsWin == 1)
            {
                //添加主公经验
                Utility.AddUserExp(userRole, response.UserExp, Request.OpCode);

                response.HeroExp = heroExp;
                //添加战利品
                BigMapCommon.AddReward(battle, userRole, userHero, userTool, response.ToolList,
                    heroExp, Request.OpCode);
            }
            else
            {
                response.UserExp = (int)(response.UserExp * 0.5);
                //添加主公经验
                Utility.AddUserExp(userRole, response.UserExp, Request.OpCode);
            }

            try
            {
                //保存回合过程
                var roundIdListList = Utility.AddBattleRound(RoundsStr);
                battle.RoundIdListList = roundIdListList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            var logItem = new GameLogItem();

            logItem.F1 = BattleId;
            logItem.F2 = IsWin;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            response.ShopType = shopType;
            response.BussinessShowCityId = bussinessShowCityId;
            ResultObj = response;
        }
    }
    #endregion

    #region 11007 征收城池资源
    public class GainBigMapCityResResponse
    {
        public GainBigMapCityResResponse()
        {
            Items = new List<ItemPair>();
        }
        /// <summary>
        /// 征收到的资源列表
        /// </summary>
        [Tag(1)]
        public List<ItemPair> Items { get; set; }
        /// <summary>
        /// 可以征收的时间
        /// </summary>
        [Tag(2)]
        public DateTime GainTime { get; set; }
    }
    /// <summary>
    /// 征收领地and城池资源
    /// </summary>
    [GameCode(OpCode = 11007, ResponseType = typeof(GainBigMapCityResResponse))]
    public class GainBigMapCityResRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);

            var gainIntervalHour = ConfigHelper.BigMapCfgData.GainIntervalHour;
            if (userCity.LastGainTime.AddHours(gainIntervalHour) > DateTime.Now)
            {
                //征收冷却中
                SetError(ResourceId.R_11007_GainCoolTime);
                return;
            }

            //收获城池资源
            //userCity.GainDomainItems();
            userCity.GainCityItems();

            var response = new GainBigMapCityResResponse();
            foreach (var resourceItem in userCity.OutPut)
            {
                response.Items.Add(new ItemPair()
                {
                    ItemId = (int)Utility.GetSpecialToolId(resourceItem.Type),
                    Num = resourceItem.Num,
                });

                Utility.AddResource(userRole, (ItemType)(resourceItem.Type), Request.OpCode, resourceItem.Num);
            }
            userCity.OutPut = new List<ResourceItem>();
            userCity.LastGainTime = DateTime.Now;

            userCity.ChangeNewMsg(userRole);//大地图新消息

            response.GainTime = userCity.GetRealGainTime();

            ResultObj = response;
        }
    }
    #endregion

    #region 11008 领地侦查可抢资源
    /// <summary>
    /// 领地侦查可抢资源
    /// </summary>
    public class InvestigateResponse
    {
        public InvestigateResponse()
        {
            CanRobResList = new List<int>();
        }
        /// <summary>
        /// 可获得的资源列表[铜钱、木材、石头、铁矿]
        /// </summary>
        [Tag(1)]
        public List<int> CanRobResList { get; set; }
        /// <summary>
        /// 侦查截止时间[用于按钮显示侦查还是查看，侦查需要消耗铜钱]
        /// </summary>
        [Tag(2)]
        public DateTime InvestigateTime { get; set; }
    }
    /// <summary>
    /// 领地侦查可抢资源
    /// </summary>
    [GameCode(OpCode = 11008, ResponseType = typeof(InvestigateResponse))]
    public class InvestigateRequest : GameHandler
    {
        /// <summary>
        /// 系统城ID
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            var sysCityCfg = SysCityCfg.Find(CityId);
            if (sysCityCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", CityId);
                return;
            }

            var userCity = Storage.Load<UserCity>(CurrentUserId, true);

            if (sysCityCfg.Type != CityType.Domain)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }
            var response = new InvestigateResponse();

            var userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == CityId);
            if (userDomainItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "DomainItemList:CityId", CityId);
                return;
            }
            if (userDomainItem.OwnerType != OwnerType.User)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            var investigateSaveSenconds = ConfigHelper.BigMapCfgData.InvestigateSaveSenconds;
            if (DateTime.Now >
                userDomainItem.InvestigateTime.AddSeconds(investigateSaveSenconds))
            {
                //重新从用户那边获取
                var userRole = Storage.Load<UserRole>(CurrentUserId, true);
                string errorMsg;
                if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
                {
                    SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                    return;
                }
                var needCoin = ConfigHelper.BigMapCfgData.InvestigateCoinModulus * userRole.Level;
                if (userRole.Coin < needCoin)
                {
                    SetError(ResourceId.R_0000_CoinNotEnough);
                    return;
                }
                //扣除侦查花费的铜钱, level
                Utility.ConsumeResource(userRole, ItemType.Coin, Request.OpCode, needCoin);

                var userId = userDomainItem.OwnerId;
                var defendUserRole = Storage.Load<UserRole>(userId);
                var level = defendUserRole.Level;
                var robResList = BigMapCommon.GetCanRobResList(userId);
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
                response.CanRobResList = robedResList;

                userDomainItem.CanRobResList = response.CanRobResList;
                userDomainItem.InvestigateTime = DateTime.Now;

                response.InvestigateTime = userDomainItem.InvestigateTime.AddSeconds(investigateSaveSenconds);
            }
            else
            {
                //规定时间内请求，不重新计算直接获取之前的侦查结果
                response.CanRobResList = userDomainItem.CanRobResList;
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 11009 定时获取城池【全服】变化列表
    /// <summary>
    /// 大地图界面信息
    /// </summary>
    public class GetBigMapChangedInfoResponse
    {
        public GetBigMapChangedInfoResponse()
        {
            CityItems = new List<GetBigMapInfoResponse.ResCityItem>();
            DomainItems = new List<GetBigMapInfoResponse.ResDomainItem>();
        }
        /// <summary>
        /// 有变化的城池列表
        /// </summary>
        [Tag(1)]
        public List<GetBigMapInfoResponse.ResCityItem> CityItems { get; set; }
        /// <summary>
        /// 可以征收的时间
        /// </summary>
        [Tag(2)]
        public DateTime GainTime { get; set; }
        /// <summary>
        /// 未读的事件数量
        /// </summary>
        [Tag(3)]
        public int UnReadEventNum { get; set; }
        /// <summary>
        /// 有变化的领地列表
        /// </summary>
        [Tag(4)]
        public List<GetBigMapInfoResponse.ResDomainItem> DomainItems { get; set; }

        /// <summary>
        /// 自己护盾截止的时间
        /// </summary>
        [Tag(5)]
        public DateTime ProtectEndTime { get; set; }

        /// <summary>
        /// 暗黑军团活动NpcId，为零说明还没开启活动或者已经通关
        /// </summary>
        [Tag(6)]
        public int DiabloNpcId { get; set; }

        /// <summary>
        /// 暗黑军团活动截止时间
        /// </summary>
        [Tag(7)]
        public DateTime DiabloEndTime { get; set; }
    }
    /// <summary>
    /// 定时获取城池【全服】变化列表
    /// </summary>
    [GameCode(OpCode = 11009, ResponseType = typeof(GetBigMapChangedInfoResponse))]
    public class GetBigMapChangedInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);
            //var userCity = Storage.Load<UserCity>(CurrentUserId, true);

            var changedCityList = userCity.CityItems.ToList();
            if (userCity.LastReqDataTime.Date.Equals(DateTime.Now.Date))
            {
                //在同一天就只是把有变化的数据下发，跨天则全部下发
                changedCityList = userCity.CityItems.Where(o => o.Status > 0).ToList();
            }

            var response = new GetBigMapChangedInfoResponse();
            if (changedCityList.Count > 0)
            {
                response.CityItems = BigMapCommon.GetResCityItems(CurrentUserId, changedCityList, userCity);

                foreach (var userCityItem in changedCityList)
                {
                    //下发后设置为无变化
                    userCityItem.Status = 0;
                }
            }

            var changedDomainList = userCity.DomainItems.Where(o => o.Status > 0).ToList();
            if (changedDomainList.Count > 0)
            {
                response.DomainItems = BigMapCommon.GetResDomainItems(changedDomainList);

                foreach (var userDomainItem in changedDomainList)
                {
                    //下发后设置为无变化
                    userDomainItem.Status = 0;
                }
            }

            response.GainTime = userCity.GetRealGainTime();
            response.UnReadEventNum = userCity.EventItems.Count(o => o.IsRead == 0);
            response.ProtectEndTime = userRole.ProtectEndTime;

            userCity.LastReqDataTime = DateTime.Now;

            response.DiabloNpcId = userCity.DiabloNpcId;
            response.DiabloEndTime = userCity.DiabloEndTime;

            ResultObj = response;
        }
    }
    #endregion

    #region 11010 获取大地图事件详细列表
    public class GetEventListResponse
    {
        /// <summary>
        /// 事件列表
        /// </summary>
        [Tag(1)]
        public List<EventItem> EventItems { get; set; }
    }
    /// <summary>
    /// 获取大地图事件详细列表
    /// </summary>
    [GameCode(OpCode = 11010, ResponseType = typeof(GetEventListResponse))]
    public class GetEventListRequest : GameHandler
    {
        /// <summary>
        /// 是否是进入大地图界面后第一次请求
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int IsFirst { get; set; }
        public override void Process(GameContext context)
        {
            var userCity = Storage.Load<UserCity>(CurrentUserId, true);
            var unReqList = userCity.EventItems.ToList();
            //不是第一次不用下发所有
            if (IsFirst == 0) unReqList = unReqList.Where(o => o.IsReq == 0).ToList();
            foreach (var eventItem in unReqList)
            {
                eventItem.IsReq = 1;
                if (eventItem.Type == EventType.Domain) eventItem.IsRead = 1;
            }

            var response = new GetEventListResponse();
            response.EventItems = unReqList.OrderByDescending(o => o.CreateTime).ToList();

            var battleList =
                Storage.LoadList<AllBattle>(
                    response.EventItems.Where(o => o.BattleId > 0).Select(o => o.BattleId).ToArray());
            foreach (var eventItem in response.EventItems)
            {
                eventItem.SysConcubineIdList = eventItem.SysConcubineIdList ?? new List<int>();
                eventItem.ItemPairList = eventItem.ItemPairList ?? new List<ItemPair>();
                if (eventItem.BattleId > 0)
                {
                    eventItem.HasRevenge = 0;
                    var battle = battleList.FirstOrDefault(o => o.Id == eventItem.BattleId);
                    if (battle != null)
                    {
                        //不存在则表示被删除了记录，此战报不可回放了。
                        if (battle.BattleAttackerHeroItemIdList == null ||
                            battle.BattleAttackerHeroItemIdList.Count == 0)
                        {
                            eventItem.IsCanPalyback = 0;
                        }
                        else
                        {
                            var heroId = battle.BattleAttackerHeroItemIdList[0];
                            var battleAttackerHeroItem = DataStorage.Current.Load<BattleHeroItem>(heroId);
                            if (battleAttackerHeroItem.IsNew) eventItem.IsCanPalyback = 0;
                        }
                        if (battle.AttackerId == CurrentUserId)
                            eventItem.TargetName = battle.DefenderName;
                        else
                        {
                            eventItem.TargetName = battle.AttackerName;
                        }
                        //是否已经复仇
                        eventItem.HasRevenge = battle.Param2;
                    }
                }
            }

            userCity.ChangeNewMsg();

            ResultObj = response;
        }
    }
    #endregion

    #region 11011 设置事件已读
    /// <summary>
    /// 设置事件已读
    /// </summary>
    [GameCode(OpCode = 11011)]
    public class SetEventReadRequest : GameHandler
    {
        /// <summary>
        /// 事件ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 类型（0：前方军情，1：寻访结果）
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int Type { get; set; }
        public override void Process(GameContext context)
        {
            var userCity = Storage.Load<UserCity>(CurrentUserId, true);
            if (Type == 0)
            {
                var eventItem = userCity.EventItems.FirstOrDefault(o => o.Id == Id);
                if (eventItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserCity:EventItems:Id", Id);
                    return;
                }
                if (eventItem.IsRead == 1)
                {
                    SetError(ResourceId.R_11011_EventIsRead);
                    return;
                }
                eventItem.IsRead = 1;
            }
            else
            {
                var searchItem = userCity.SearchItems.FirstOrDefault(o => o.Id == Id);
                if (searchItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserCity:SearchItems:Id", Id);
                    return;
                }
                if (searchItem.IsRead == 1)
                {
                    SetError(ResourceId.R_11011_EventIsRead);
                    return;
                }
                searchItem.IsRead = 1;
            }

            userCity.ChangeNewMsg();
        }
    }
    #endregion

    #region 11012 寻访
    public class SearchVisitorResponse
    {
        public SearchVisitorResponse()
        {
            Items = new List<ItemPair>();
        }
        /// <summary>
        /// 得到的物品列表（碎片、道具）
        /// </summary>
        [Tag(1)]
        public List<ItemPair> Items { get; set; }
        ///// <summary>
        ///// 寻访武将
        ///// </summary>
        //[Tag(1)]
        //public int SearchHeroId { get; set; }
        ///// <summary>
        ///// 消息
        ///// </summary>
        //[Tag(2)]
        //public string Msg { get; set; }
        ///// <summary>
        ///// 寻访的城池id
        ///// </summary>
        //[Tag(3)]
        //public int CityId { get; set; }
        ///// <summary>
        ///// 到访的系统妃子Id列表
        ///// </summary>
        //[Tag(4)]
        //public List<int> ConcubineIdList { get; set; }
        ///// <summary>
        ///// 到访的系统武将Id列表
        ///// </summary>
        //[Tag(5)]
        //public List<int> HeroIdList { get; set; }
    }
    /// <summary>
    /// 寻访
    /// </summary>
    [GameCode(OpCode = 11012, ResponseType = typeof(SearchVisitorResponse))]
    public class SearchVisitorRequest : GameHandler
    {
        /// <summary>
        /// 寻访使用的武将ID
        /// </summary>
        public int HeroId { get; set; }
        /// <summary>
        /// 寻访的城池
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserHero userHero;
            UserRole userRole;
            UserChip userChip;
            Storage.Load(out userCity, out userHero, out userRole, out userChip, CurrentUserId, true);

            var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserCity:CityItems:CityId", CityId);
                return;
            }

            if (userCityItem.LaveSearchNum <= 0)
            {
                //寻访次数不足
                SetError(ResourceId.R_0000_SearchNotEnough);
                return;
            }
            var needSp = 6;
            if (userRole.Sp < needSp)
            {
                //体力不足
                SetError(ResourceId.R_0000_SpNotEnough);
                return;
            }

            var userHeroItem = userHero.Items.Find(o => o.HeroId == HeroId);
            if (userHeroItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "userHero:Items:HeroId", HeroId);
                return;
            }

            var sysCityCfg = SysCityCfg.Find(CityId);
            if (sysCityCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysCityCfg:Id", CityId);
                return;
            }
            //立即寻访出结果
            userCityItem.SearchVisitor(HeroId);

            var response = new SearchVisitorResponse();
            //response.SearchHeroId = HeroId;
            //response.Msg = userCity.GetSearchMsg(userCityItem.VisitorItems, sysCityCfg.Name);

            foreach (var visitorItem in userCityItem.VisitorItems)
            {
                var type = Utility.GetIdExtractType(visitorItem.VisitorId);
                if (type == ExtractItemType.Hero)
                {
                    //response.HeroIdList.Add(visitorItem.VisitorId);

                    //添加武将碎片
                    var heroChipId = (int)ToolType.HeroChip + visitorItem.VisitorId;
                    userChip.AddChip(heroChipId, 1, ToolType.HeroChip, Request.OpCode);
                    response.Items.Add(new ItemPair() { ItemId = heroChipId, Num = 1 });
                }
                if (type == ExtractItemType.Concubine)
                {
                    //response.ConcubineIdList.Add(visitorItem.VisitorId);

                    //添加妃子碎片
                    var concubineChipId = (int)ToolType.ConcubineChip + visitorItem.VisitorId;
                    userChip.AddChip(concubineChipId, 1, ToolType.ConcubineChip, Request.OpCode);
                    response.Items.Add(new ItemPair() { ItemId = concubineChipId, Num = 1 });
                }
            }
            //扣除体力
            Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, needSp);

            //response.CityId = CityId;

            ResultObj = response;
        }
    }
    #endregion

    #region 11013 入城
    public class InCityResponse
    {
        public InCityResponse()
        {
            //VisitorItems = new List<VisitorItem>();
            ShopTypeItems = new List<ShopType>();
        }
        /// <summary>
        /// 到访武将/妃子列表
        /// </summary>
        //[Tag(1)]
        //public List<VisitorItem> VisitorItems { get; set; }
        /// <summary>
        /// 存在的商人列表
        /// </summary>
        [Tag(2)]
        public List<ShopType> ShopTypeItems { get; set; }
        /// <summary>
        /// 城防【大于50 每5点提升2%的城防 小于50 每5点降低3%的城防】
        /// </summary>
        [Tag(3)]
        public int Defense { get; set; }

        /// <summary>
        /// 军备【大于50 每5点提升2%的带兵 小于50 每5点降低3%的带兵】
        /// </summary>
        [Tag(4)]
        public int Army { get; set; }

        /// <summary>
        /// 士气 5点提升10点怒气
        /// </summary>
        [Tag(5)]
        public int Morale { get; set; }

        /// <summary>
        /// 今日已使用的内政次数
        /// </summary>
        [Tag(6)]
        public int UseNum { get; set; }

        /// <summary>
        /// 今日已购买的重置内政次数
        /// </summary>
        [Tag(7)]
        public int BuyNum { get; set; }
    }
    /// <summary>
    /// 入城
    /// </summary>
    [GameCode(OpCode = 11013, ResponseType = typeof(InCityResponse))]
    public class InCityRequest : GameHandler
    {
        /// <summary>
        /// 进入的城池ID
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            var userCity = Storage.Load<UserCity>(CurrentUserId, true);

            var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserCity:CityItems:CityId", CityId);
                return;
            }

            var response = new InCityResponse();
            //response.VisitorItems = Utility.GetVisitorItems(userCityItem, CityId);

            foreach (var shopItem in userCity.ShopItems)
            {
                if (shopItem.CityId == CityId)
                    response.ShopTypeItems.Add(shopItem.ShopType);
            }

            var serverMapCityItem = Storage.Load<ServerMapCityItem>(userCityItem.ServerMapCityItemId);
            if (serverMapCityItem.IsNew)
                throw new ApplicationException(string.Format("Not Find ServerMapCityItem,ServerMapCityItemId:{0}",
                    userCityItem.ServerMapCityItemId));

            response.Defense = serverMapCityItem.Defense;
            response.Army = serverMapCityItem.Army;
            response.Morale = (int)serverMapCityItem.Morale.Value;
            response.UseNum = (int)userCityItem.UseInternalAffairsNum.Value;//(int)serverMapCityItem.UseNum.Value;
            response.BuyNum = (int)userCityItem.BuyInternalAffairsNum.Value;

            ResultObj = response;
        }
    }
    #endregion

    #region 11014 拜访
    public class VisitVisitorResponse
    {
        public VisitVisitorResponse()
        {
            ToolList = new List<ItemPair>();
        }
        /// <summary>
        /// 获得的道具列表
        /// </summary>
        [Tag(1)]
        public List<ItemPair> ToolList { get; set; }
    }
    /// <summary>
    /// 拜访
    /// </summary>
    [GameCode(OpCode = 11014, ResponseType = typeof(VisitVisitorResponse))]
    public class VisitVisitorRequest : GameHandler
    {
        /// <summary>
        /// 拜访的来访者ID(妃子/武将)
        /// </summary>
        public int VisitorId { get; set; }
        /// <summary>
        /// 寻访的城池
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserTool userTool;
            UserChip userChip;
            Storage.Load(out userCity, out userTool, out userChip, CurrentUserId, true);

            var cityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (cityItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserCity:CityItems:CityId", CityId);
                return;
            }

            var visitorList = SysVisitorCfg.Items.Where(o => o.CityId == CityId).ToList();
            if (!visitorList.Exists(o => o.VisitorId == VisitorId))
            {
                SetError(ResourceId.R_0000_IdNotExist, string.Format("Not Find Visitor In City={0} and VisitorId", CityId), VisitorId);
                return;
            }

            var response = new VisitVisitorResponse();
            bool isHero = (VisitorId > 6000000) ? true : false;
            if (isHero)
            {
                var sysHeroCfg = SysHeroCfg.Items.Find(o => o.HeadId == VisitorId);
                if (sysHeroCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysHeroCfg:HeadId", VisitorId);
                    return;
                }
                var toolItem = userTool.Items.FirstOrDefault(o => o.ItemId == (int)SpecialToolId.BaiTie);
                if (toolItem == null || toolItem.Num <= 0)
                {
                    SetError(ResourceId.R_0000_BaiTieNotEnough);
                    return;
                }
                toolItem.Num--;

                //添加武将碎片
                var heroChipId = (int)ToolType.HeroChip + VisitorId;
                response.ToolList.Add(new ItemPair() { ItemId = heroChipId, Num = 1 });
                userChip.AddChip(heroChipId, 1, ToolType.HeroChip, Request.OpCode);
            }
            else
            {
                var sysConcubineCfg = SysConcubineCfg.Items.Find(o => o.Id == VisitorId);
                if (sysConcubineCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysConcubineCfg:Id", VisitorId);
                    return;
                }
                var toolItem = userTool.Items.FirstOrDefault(o => o.ItemId == (int)SpecialToolId.XiangNang);
                if (toolItem == null || toolItem.Num <= 0)
                {
                    SetError(ResourceId.R_0000_XiangnangNotEnough);
                    return;
                }
                toolItem.Num--;

                //添加妃子碎片
                var concubineChipId = (int)ToolType.ConcubineChip + VisitorId;
                response.ToolList.Add(new ItemPair() { ItemId = concubineChipId, Num = 1 });
                userChip.AddChip(concubineChipId, 1, ToolType.ConcubineChip, Request.OpCode);
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 11016 获取大地图开启的城池ID_用于将军府获取碎片界面显示城池是否开启
    public class GetOpenedCityIdListResponse
    {
        /// <summary>
        /// 已开启的城池Id列表
        /// </summary>
        [Tag(1)]
        public List<int> CityIdList { get; set; }
    }
    /// <summary>
    /// 获取大地图开启的城池ID
    /// </summary>
    [GameCode(OpCode = 11016, ResponseType = typeof(GetOpenedCityIdListResponse))]
    public class GetOpenedCityIdListRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var userCity = Storage.Load<UserCity>(CurrentUserId);
            var response = new GetOpenedCityIdListResponse()
            {
                CityIdList = userCity.CityItems.Select(o => o.CityId).ToList()
            };

            ResultObj = response;
        }
    }
    #endregion

    #region 11017 处理内政
    public class DealInternalAffairsResponse
    {
        /// <summary>
        /// 当前值
        /// </summary>
        [Tag(1)]
        public int CurValue { get; set; }
    }
    /// <summary>
    /// 处理内政【修葺城墙、招募士兵、治安巡逻】
    /// </summary>
    [GameCode(OpCode = 11017, ResponseType = typeof(DealInternalAffairsResponse))]
    public class DealInternalAffairsRequest : GameHandler
    {
        /// <summary>
        /// 内政类型
        /// </summary>
        public InternalAffairsType Type { get; set; }
        /// <summary>
        /// 城池Id
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserCity userCity;
            Storage.Load(out userRole, out userCity, CurrentUserId, true);

            var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserCity:CityItems:CityId", CityId);
                return;
            }

            var serverMapCityItemId = userCityItem.ServerMapCityItemId;
            var serverMapCityItem = Storage.Load<ServerMapCityItem>(serverMapCityItemId, true);
            if (serverMapCityItem.IsNew)
            {
                //全服的城池信息未创建
                SetError(ResourceId.R_11002_ServerCityNotCreate);
                return;
            }
            if (serverMapCityItem.OwnerId != CurrentUserId)
            {
                //未占领该城池，无权操作
                SetError(ResourceId.R_11003_ServerCityOccupiedByOther);
                return;
            }
            var sysVipCfg = SysVipCfg.Items.FirstOrDefault(o => o.VipLevel == userRole.RealVipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg:VipLevel", userRole.RealVipLevel);
                return;
            }
            //if (serverMapCityItem.UseNum.Value >= sysVipCfg.InternalAffairsNum)
            if (userCityItem.LaveInternalAffairsNum <= 0)
            {
                //内政次数不足
                SetError(ResourceId.R_11017_InternalAffairsNumNotEnough);
                return;
            }
            var bigMapDefenseCfgData = ConfigHelper.BigMapDefenseCfgData;
            var needSp = bigMapDefenseCfgData.NeedSp;
            var addValue = bigMapDefenseCfgData.EachAdd;

            //if (userRole.Sp < needSp)
            //{
            //    //体力不足
            //    SetError(ResourceId.R_0000_SpNotEnough);
            //    return;
            //}

            var response = new DealInternalAffairsResponse();
            switch (Type)
            {
                case InternalAffairsType.Defense:
                    if (serverMapCityItem.Defense >= 100)
                    {
                        SetError(ResourceId.R_11017_DefenseIsFull);
                        return;
                    }
                    serverMapCityItem.Defense += addValue;
                    if (serverMapCityItem.Defense > 100) serverMapCityItem.Defense = 100;
                    response.CurValue = serverMapCityItem.Defense;
                    break;
                case InternalAffairsType.Army:
                    if (serverMapCityItem.Army >= 100)
                    {
                        SetError(ResourceId.R_11017_ArmyIsFull);
                        return;
                    }
                    serverMapCityItem.Army += addValue;
                    if (serverMapCityItem.Army > 100) serverMapCityItem.Army = 100;
                    response.CurValue = serverMapCityItem.Army;
                    break;
                case InternalAffairsType.Morale:
                    if (serverMapCityItem.Morale.Value >= 100)
                    {
                        SetError(ResourceId.R_11017_MoraleIsFull);
                        return;
                    }
                    serverMapCityItem.Morale += addValue;
                    if (serverMapCityItem.Morale.Value > 100)
                    {
                        serverMapCityItem.Morale -= serverMapCityItem.Morale.Value;
                        serverMapCityItem.Morale += 100;
                    }
                    response.CurValue = (int)serverMapCityItem.Morale.Value;
                    break;
            }

            //serverMapCityItem.UseNum += 1;
            userCityItem.UseInternalAffairsNum += 1;
            //扣除体力
            //Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, needSp);

            ResultObj = response;
        }
    }
    #endregion

    #region 11018 获得商人物品列表
    public class GetShopGoodsItemsResponse
    {
        public GetShopGoodsItemsResponse()
        {
            GoodsItems = new List<ResShopGoodTtem>();
            RefreshDescription = "";
        }
        /// <summary>
        /// 今日已刷新的次数
        /// </summary>
        [Tag(1)]
        public int RefreshNum { get; set; }
        /// <summary>
        /// 神秘/西域商人离开时间
        /// </summary>
        [Tag(2)]
        public DateTime LeaveTime { get; set; }
        /// <summary>
        /// 商人处物品列表
        /// </summary>
        [Tag(3)]
        public List<ResShopGoodTtem> GoodsItems { get; set; }
        /// <summary>
        /// 刷新描述【如果为空字符串则表示是会离开的商人不是固定商人】
        /// </summary>
        [Tag(4)]
        public string RefreshDescription { get; set; }
    }
    /// <summary>
    /// 获得商人物品列表
    /// </summary>
    [GameCode(OpCode = 11018, ResponseType = typeof(GetShopGoodsItemsResponse))]
    public class GetShopGoodsItemsRequest : GameHandler
    {
        /// <summary>
        /// 商人类型
        /// </summary>
        public ShopType ShopType { get; set; }
        public override void Process(GameContext context)
        {
            var response = new GetShopGoodsItemsResponse();

            var userCity = Storage.Load<UserCity>(CurrentUserId, true);
            var shop = userCity.ShopItems.FirstOrDefault(o => o.ShopType == ShopType);
            if (shop == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "ShopItems:ShopType", (int)ShopType);
                return;
            }

            response.RefreshDescription = shop.GetRefreshRefreshDescription();
            response.LeaveTime = shop.LeaveTime;
            response.RefreshNum = (int)userCity.TodayShopRefreshNumList[(int)ShopType - 1].Value;

            response.GoodsItems = BigMapCommon.GetShopItemList(shop.GoodsItems);
            ResultObj = response;
        }
    }
    #endregion

    #region 11019 刷新商人物品列表
    /// <summary>
    /// 刷新商人物品列表
    /// </summary>
    [GameCode(OpCode = 11019, ResponseType = typeof(GetShopGoodsItemsResponse))]
    public class RefreshShopGoodsItemsRequest : GameHandler
    {
        /// <summary>
        /// 商人类型
        /// </summary>
        public ShopType ShopType { get; set; }
        public override void Process(GameContext context)
        {
            var response = new GetShopGoodsItemsResponse();

            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId);

            //判断刷新次数是否足够
            var refreshNum = (int)userCity.TodayShopRefreshNumList[(int)ShopType - 1].Value;
            var sysVipCfg = SysVipCfg.Items.FirstOrDefault(o => o.VipLevel == userRole.RealVipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg:VipLevel", userRole.RealVipLevel);
                return;
            }
            var canRefreshNum = sysVipCfg.LuoYangRefreshNum;
            if (ShopType == ShopType.Mysterious) canRefreshNum = sysVipCfg.MysteriousRefreshNum;
            else if (ShopType == ShopType.Western) canRefreshNum = sysVipCfg.WesternRefreshNum;
            else if (ShopType == ShopType.Pk) canRefreshNum = sysVipCfg.PkRefreshNum;

            if (refreshNum >= canRefreshNum)
            {
                SetError(ResourceId.R_0000_RefreshNotEnough);
                return;
            }

            var sysBuyNumCfg = SysBuyNumCfg.Items.FirstOrDefault(o => o.Id == refreshNum + 1);
            if (sysBuyNumCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBuyNumCfg:Id", refreshNum + 1);
                return;
            }
            var needMoney = sysBuyNumCfg.RefreshLuoYangMoney;
            if (ShopType == ShopType.Mysterious) needMoney = sysBuyNumCfg.RefreshMysteriousMoney;
            else if (ShopType == ShopType.Western) needMoney = sysBuyNumCfg.RefreshWesternMoney;
            else if (ShopType == ShopType.Pk) needMoney = sysBuyNumCfg.RefreshPkMoney;

            //花费元宝刷新
            if (userRole.TotalMoney < needMoney)
            {
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            var specialStoreId = SpecialStoreId.RefreshLuoYang;
            if (ShopType == ShopType.Mysterious) specialStoreId = SpecialStoreId.RefreshMysterious;
            else if (ShopType == ShopType.Western) specialStoreId = SpecialStoreId.RefreshWestern;
            else if (ShopType == ShopType.Pk) specialStoreId = SpecialStoreId.RefreshPk;
            Utility.Concume(userRole, needMoney, specialStoreId);

            var shop = userCity.ShopItems.FirstOrDefault(o => o.ShopType == ShopType);
            if (shop == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "ShopItems:ShopType", (int)ShopType);
                return;
            }

            //if (ShopType == ShopType.LuoYang)
            //    userCity.RefreshLuoYangShop(1);
            //else
            //{
            //    userCity.RefreshMysteriousOrWesternShop(ShopType, 0, 1);
            //}
            userCity.RefreshShop(ShopType, 0, 1);

            shop = userCity.ShopItems.FirstOrDefault(o => o.ShopType == ShopType);
            if (shop == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "ShopItems:ShopType", (int)ShopType);
                return;
            }
            response.RefreshDescription = shop.GetRefreshRefreshDescription();
            response.LeaveTime = shop.LeaveTime;
            response.RefreshNum = (int)userCity.TodayShopRefreshNumList[(int)ShopType - 1].Value;
            response.GoodsItems = BigMapCommon.GetShopItemList(shop.GoodsItems);

            ResultObj = response;
        }
    }
    #endregion

    #region 11020 购买商人物品
    /// <summary>
    /// 购买竞技场次数
    /// </summary>
    [GameCode(OpCode = 11020)]
    public class BuyShopGoodsRequest : GameHandler
    {
        /// <summary>
        /// id（唯一标识符）
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 物品的id【为了判断商店是否刷新了】
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 物品数量【为了判断商店是否刷新了】
        /// </summary>
        public int Num { get; set; }
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserCity userCity;
            Storage.Load(out userRole, out userCity, CurrentUserId, true);

            var sysShopCfg = SysShopCfg.Find(o => o.Id == Id);
            if (sysShopCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysShopCfg:Id", Id);
                return;
            }

            var shopType = sysShopCfg.ShopType;//(ShopType)(Id / 100);
            var shop = userCity.ShopItems.FirstOrDefault(o => o.ShopType == shopType);
            if (shop == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "userCity:ShopItems:ShopType", (int)shopType);
                return;
            }

            var shopGoodsItem = shop.GoodsItems.FirstOrDefault(o => o.Id == Id);
            if (shopGoodsItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "userCity:shop:GoodsItems:Id", Id);
                return;
            }
            if (shopGoodsItem.Buyed == 1)
            {
                //该物品已购买过
                SetError(ResourceId.R_11020_ShopGoodsBuyed);
                return;
            }
            var shopNum = shopGoodsItem.SNum == 0 ? sysShopCfg.Num : shopGoodsItem.SNum;
            if (sysShopCfg.GoodsId != ItemId || shopNum != Num || shopNum == 0)
            {
                //商店物品已刷新
                SetError(ResourceId.R_11020_ShopAlreadyRefresh);
                return;
            }
            var needCoin = 0;
            var needMoney = 0;
            var needHonor = 0;
            SysToolCfg sysToolCfg = null;
            if (sysShopCfg.ItemType == ExtractItemType.Tool ||
                sysShopCfg.ItemType == ExtractItemType.HeroChip ||
                sysShopCfg.ItemType == ExtractItemType.ConcubineChip ||
                sysShopCfg.ItemType == ExtractItemType.EquipChip ||
                sysShopCfg.ItemType == ExtractItemType.PetChip)
            {
                sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == sysShopCfg.GoodsId);
            }
            else
            {
                throw new ApplicationException("shop no sell not tool goods");
            }
            if (sysToolCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", sysShopCfg.GoodsId);
                return;
            }

            if (sysShopCfg.MoneyType == MoneyType.Coin) needCoin = sysShopCfg.SellPrice * shopNum;// 
            else if (sysShopCfg.MoneyType == MoneyType.Money) needMoney = sysShopCfg.SellPrice * shopNum;// * sysShopCfg.Num
            else if (sysShopCfg.MoneyType == MoneyType.Honor) needHonor = sysShopCfg.SellPrice * shopNum; //* sysShopCfg.Num
            else
            {
                throw new ApplicationException(string.Format("systoolcfg sell moneytype is null.MoneyType:{0}",
                    (int)sysToolCfg.MoneyType));
            }

            if (needCoin > 0 && userRole.Coin < needCoin)
            {
                SetError(ResourceId.R_0000_CoinNotEnough);
                return;
            }
            if (needMoney > 0 && userRole.TotalMoney < needMoney)
            {
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            if (needHonor > 0 && userRole.Honor < needHonor)
            {
                //荣誉值不足
                SetError(ResourceId.R_0000_HonorNotEnough);
                return;
            }

            Utility.ConsumeResource(userRole, ItemType.Coin, Request.OpCode, needCoin);
            Utility.ConsumeResource(userRole, ItemType.Honor, Request.OpCode, needHonor);

            if (needMoney > 0)
            {
                Utility.Concume(userRole, needMoney, sysToolCfg.Id);
            }

            UserTool userTool;
            UserChip userChip;
            Storage.Load(out userTool, out userChip, CurrentUserId, true);
            sysToolCfg.AddToUser(Request.OpCode, userChip, userRole, userTool, shopNum);

            shopGoodsItem.Buyed = 1;
        }
    }
    #endregion
}
