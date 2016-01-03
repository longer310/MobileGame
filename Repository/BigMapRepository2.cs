using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
    #region 11021 城池概览
    public class GetBigMapOverviewResponse
    {
        public GetBigMapOverviewResponse()
        {
            CityItems = new List<OverviewItem>();
            GainTime = DateTime.Now.AddDays(1);
        }
        /// <summary>
        /// 领地数量
        /// </summary>
        [Tag(1)]
        public int DomainNum { get; set; }
        /// <summary>
        /// 每次征收获得的铜钱
        /// </summary>
        [Tag(2)]
        public int Coin { get; set; }
        /// <summary>
        /// 今日已征收获次数
        /// </summary>
        [Tag(3)]
        public int GainDomainResNum { get; set; }
        /// <summary>
        /// 今日可以征收的次数
        /// </summary>
        [Tag(4)]
        public int MaxGainDomainResNum { get; set; }
        /// <summary>
        /// 今日已强征的次数
        /// </summary>
        [Tag(5)]
        public int StrGainDomainResNum { get; set; }
        /// <summary>
        /// 自己的城池列表
        /// </summary>
        [Tag(6)]
        public List<OverviewItem> CityItems { get; set; }
        /// <summary>
        /// 城池可以征收的时间
        /// </summary>
        [Tag(7)]
        public DateTime GainTime { get; set; }

        /// <summary>
        /// 是否已领取过爵位奖励
        /// </summary>
        [Tag(10)]
        public int HasGetTitleReward { get; set; }
        /// <summary>
        /// 总共领地数量
        /// </summary>
        [Tag(11)]
        public int TotalDomainNum { get; set; }
        /// <summary>
        /// 总共城池数量
        /// </summary>
        [Tag(12)]
        public int TotalCityNum { get; set; }

        public class OverviewItem
        {
            /// <summary>
            /// 城池Id
            /// </summary>
            [Tag(1)]
            public int CityId { get; set; }
            /// <summary>
            /// 城防【大于50 每5点提升2%的城防 小于50 每5点降低3%的城防】
            /// </summary>
            [Tag(2)]
            public int Defense { get; set; }
            /// <summary>
            /// 军备【大于50 每5点提升2%的带兵 小于50 每5点降低3%的带兵】
            /// </summary>
            [Tag(3)]
            public int Army { get; set; }
            /// <summary>
            /// 士气 5点提升10点怒气
            /// </summary>
            [Tag(4)]
            public int Morale { get; set; }
        }
    }
    /// <summary>
    /// 城池概览
    /// </summary>
    [GameCode(OpCode = 11021, ResponseType = typeof(GetBigMapOverviewResponse))]
    public class GetBigMapOverviewRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);

            var response = new GetBigMapOverviewResponse();

            response.DomainNum = userCity.DomainItems.Count(o => o.OwnerType == OwnerType.Own);
            response.Coin = userCity.GainDomainItems();
            response.GainDomainResNum = (int)userCity.GainDomainResNum.Value;
            response.MaxGainDomainResNum = ConfigHelper.BigMapCfgData.MaxGainDomainResNum;
            response.StrGainDomainResNum = (int)userCity.StrGainDomainResNum.Value;

            //检测爵位封号
            //BigMapCommon.CheckAndSetTitleLevel(userRole);
            //response.TitleType = (TitleType)userRole.TitleLevel;
            //response.TitleName = userRole.TitleName;
            response.HasGetTitleReward = (int)userCity.HasGetTitleReward.Value;

            var ownCityList = userCity.CityItems.Where(o => o.OwnerType == OwnerType.Own).ToList();
            if (ownCityList.Any())
            {
                //城池
                var serverMapCityItemIdList =
                    ownCityList.Where(o => o.ServerMapCityItemId > 0).Select(o => o.ServerMapCityItemId).ToList();

                var serverMapCityItemList = DataStorage.Current.LoadList<ServerMapCityItem>(serverMapCityItemIdList.ToArray(), true);
                foreach (var userCityItem in ownCityList)
                {
                    var item = new GetBigMapOverviewResponse.OverviewItem();

                    var serverMapCityItem =
                        serverMapCityItemList.FirstOrDefault(o => o.Id == userCityItem.ServerMapCityItemId);
                    if (serverMapCityItem != null)
                    {
                        if (serverMapCityItem.OwnerId == CurrentUserId)
                        {
                            item.Defense = serverMapCityItem.Defense;
                            item.Army = serverMapCityItem.Army;
                            item.Morale = (int)serverMapCityItem.Morale.Value;
                            item.CityId = userCityItem.CityId;

                            response.CityItems.Add(item);
                        }
                        else
                        {
                            userCityItem.OwnerType = serverMapCityItem.OwnerType;
                        }
                    }
                }
                response.GainTime = userCity.GetRealGainTime();
            }
            response.TotalDomainNum = userCity.DomainItems.Count;
            response.TotalCityNum = userCity.CityItems.Count;

            ResultObj = response;
        }
    }
    #endregion

    #region 11022 查看城池信息
    public class GetCityInfoResponse
    {
        public GetCityInfoResponse()
        {
        }
        /// <summary>
        /// 城防【大于50 每5点提升2%的城防 小于50 每5点降低3%的城防】
        /// </summary>
        [Tag(1)]
        public int Defense { get; set; }

        /// <summary>
        /// 军备【大于50 每5点提升2%的带兵 小于50 每5点降低3%的带兵】
        /// </summary>
        [Tag(2)]
        public int Army { get; set; }

        /// <summary>
        /// 士气 5点提升10点怒气
        /// </summary>
        [Tag(3)]
        public int Morale { get; set; }
    }
    /// <summary>
    /// 查看城池信息
    /// </summary>
    [GameCode(OpCode = 11022, ResponseType = typeof(GetCityInfoResponse))]
    public class GetCityInfoRequest : GameHandler
    {
        /// <summary>
        /// 城池ID
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
            var serverMapCityItem = Storage.Load<ServerMapCityItem>(userCityItem.ServerMapCityItemId);
            if (serverMapCityItem.IsNew)
                throw new ApplicationException(string.Format("Not Find ServerMapCityItem,ServerMapCityItemId:{0}",
                    userCityItem.ServerMapCityItemId));

            var response = new GetCityInfoResponse();
            response.Defense = serverMapCityItem.Defense;
            response.Army = serverMapCityItem.Army;
            response.Morale = (int)serverMapCityItem.Morale.Value;

            ResultObj = response;
        }
    }
    #endregion

    #region 11023 领地征收/强征
    public class GainBigMapDomainResResponse
    {
        public GainBigMapDomainResResponse()
        {
        }
        /// <summary>
        /// 得到的铜钱
        /// </summary>
        [Tag(1)]
        public int Coin { get; set; }
    }
    /// <summary>
    /// 领地征收/强征
    /// </summary>
    [GameCode(OpCode = 11023, ResponseType = typeof(GainBigMapDomainResResponse))]
    public class GainBigMapDomainResRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);

            if (!userCity.DomainItems.Exists(o => o.OwnerType == OwnerType.Own))
            {
                //无法征收，请先去占领一些领地
                SetError(ResourceId.R_11023_NotOccupiedDomain);
                return;
            }

            var maxGainDomainResNum = ConfigHelper.BigMapCfgData.MaxGainDomainResNum;
            var needMoney = 0;
            if (userCity.GainDomainResNum.Value >= maxGainDomainResNum)
            {
                //免费征收次数已用完
                var sysVipCfg = SysVipCfg.Items.FirstOrDefault(o => o.VipLevel == userRole.RealVipLevel);
                if (sysVipCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg:VipLevel", userRole.RealVipLevel);
                    return;
                }
                var maxStrGainDomainResNum = sysVipCfg.StrGainDomainResNum;
                if (userCity.StrGainDomainResNum.Value >= maxStrGainDomainResNum)
                {
                    //强征次数已用完
                    SetError(ResourceId.R_0000_StrGainNotEnough);
                    return;
                }
                var sysBuyNumCfg =
                    SysBuyNumCfg.Items.FirstOrDefault(o => o.Id == (int)userCity.StrGainDomainResNum.Value + 1);
                if (sysBuyNumCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "sysBuyNumCfg:Id", (int)userCity.StrGainDomainResNum.Value + 1);
                    return;
                }
                needMoney = sysBuyNumCfg.BuyStrGainMoney;
            }
            if (needMoney > 0 && userRole.TotalMoney < needMoney)
            {
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            if (needMoney > 0)
            {
                var specialStoreId = SpecialStoreId.StrGainDomainRes;
                Utility.Concume(userRole, needMoney, specialStoreId);
            }
            var response = new GainBigMapDomainResResponse();
            response.Coin = userCity.GainDomainItems();

            //添加铜钱
            Utility.AddResource(userRole, ItemType.Coin, Request.OpCode, response.Coin);

            if (needMoney == 0)
            {
                userCity.GainDomainResNum += 1;
            }
            else
            {
                userCity.StrGainDomainResNum += 1;
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 11024 大地图复仇
    /// <summary>
    /// 大地图复仇
    /// </summary>
    [GameCode(OpCode = 11024, ResponseType = typeof(BigMapPkNpcResponse))]
    public class BigMapRevengeRequest : BattleHandler
    {
        public override void Process(GameContext context)
        {
            var response = new BigMapPkNpcResponse();

            var revengeBattle = Storage.Load<AllBattle>(TargetId, true);
            if (revengeBattle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "AllBattle:Id", TargetId);
                return;
            }
            if (revengeBattle.Param2 == 1)
            {
                SetError(ResourceId.R_11024_BigMapRevengeed);
                return;
            }

            if (revengeBattle.DefenderId != CurrentUserId)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            if (revengeBattle.WarType != WarType.DomainUser && revengeBattle.WarType != WarType.Revenge)
            {
                //只有领地玩家战役 才有 复仇
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            //攻击方武将详细信息
            UserHero userHero;
            UserFormation userFormation;
            UserRole userRole;
            Storage.Load(out userHero, out userFormation, out userRole, CurrentUserId, true);

            var needSp = ConfigHelper.BigMapCfgData.BattleNeedSp;
            if (userRole.Sp < needSp)
            {
                //体力不足
                SetError(ResourceId.R_0000_SpNotEnough);
                return;
            }
            //扣除体力
            Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, needSp);

            var battleId = Util.GetSequence(typeof(AllBattle), 0);
            var battle = Storage.Load<AllBattle>(battleId, true);
            battle.Param1 = TargetId;

            battle.AttackerId = CurrentUserId;
            battle.AttackerLevel = userRole.Level;
            battle.AttackerHeadId = userRole.HeadId;
            battle.AttackerName = userRole.NickName;

            var index = 0;
            var newTotalCombat = 0;
            foreach (var i in HeroIdList)
            {
                var userHeroItem = userHero.FindByHeroId(i);
                if (userHeroItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", i);
                    return;
                }
                //string errorMsg;
                //if (!userHeroItem.JudgeHeroConform(sysCityCfg.BattleType, out errorMsg))
                //{
                //    //武将不符合条件
                //    SetError(errorMsg, userHeroItem.SysHeroCfg.Name);
                //    return;
                //}
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
            userFormation.SetAttFormations(BattleType.None, HeroIdList, LocationIdList, newTotalCombat);

            //攻打领地中匹配到的玩家
            battle.WarType = WarType.Revenge;
            UserRole targetUserRole;
            UserHero targetUserHero;
            UserFormation targetUserFormation;
            Storage.Load(out targetUserRole, out targetUserHero, out targetUserFormation, revengeBattle.AttackerId, true);
            battle.DefenderId = revengeBattle.AttackerId;
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

            //攻打玩家时锁定玩家的时间
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

            //增加战斗技能光环效果
            Utility.AddBattleSkillRing(response.AttackerHeroItems, response.DefenderHeroItemsList[0].HeroItems);

            response.BattleId = battle.Id;
            ResultObj = response;
        }
    }
    #endregion

    #region 11025 提交大地图复仇挑战结果
    /// <summary>
    /// 提交大地图复仇挑战结果
    /// </summary>
    [GameCode(OpCode = 11025, ResponseType = typeof(SubmitBigMapResultResponse))]
    public class SubmitRevengeResultRequest : GameHandler
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }
        /// <summary>
        /// 攻击方是否胜利
        /// </summary>
        [ParamCheck(IsPositive = false)]
        public int IsWin { get; set; }
        /// <summary>
        /// 战斗评分
        /// </summary>
        public int BattleScore { get; set; }
        /// <summary>
        /// 回合字符串列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string RoundsStr { get; set; }
        public override bool InitParams(GameContext context)
        {
            // IsWin < -4 || || IsWin > 1
            if (BattleId == 0) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new SubmitBigMapResultResponse();

            if (IsWin < 0) IsWin = 0;

            var battle = Storage.Load<AllBattle>(BattleId, true);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "BattleId", BattleId);
                return;
            }
            if (battle.IsWin != -1)
            {
                SetError(ResourceId.R_0000_BattleHaveResult);
                return;
            }
            if (battle.AttackerId != CurrentUserId || battle.Param1 == 0)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            //复仇战报找不到了
            var revengeBattleId = battle.Param1;
            var revengeBattle = Storage.Load<AllBattle>(revengeBattleId, true);
            if (revengeBattle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "AllBattle;Id", revengeBattleId);
                return;
            }
            if (revengeBattle.Param2 == 1)
            {
                SetError(ResourceId.R_11024_BigMapRevengeed);
                return;
            }
            revengeBattle.Param2 = 1;

            var warType = battle.WarType;
            if (warType != WarType.Revenge)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            battle.IsWin = IsWin;
            battle.BattleScore = BattleScore;

            var defendUserCity = new UserCity();

            var needSp = ConfigHelper.BigMapCfgData.BattleNeedSp;
            var heroExp = 0;
            var repute = 0;

            ShopType shopType = ShopType.None;
            int bussinessShowCityId = 0;
            if (IsWin == 1)
            {
                UserRole userRole;
                UserHero userHero;
                UserTool userTool;
                UserCity userCity;
                Storage.Load(out userCity, out userRole, out userHero, out userTool, CurrentUserId, true);

                //获取胜利抢到的资源，直接添加给玩家
                repute = ConfigHelper.BigMapCfgData.AttackUserRepute;
                heroExp = battle.DefenderLevel * 10;

                //防守方减少声望
                //Utility.ConsumeResource(null, ItemType.Repute, Request.OpCode,
                //ConfigHelper.BigMapCfgData.BeRobedRepute, battle.DefenderId);

                //获取胜利抢到的资源，直接添加给玩家, sysCityCfg.Level
                response.ToolList = BigMapCommon.GetCanRobResList(battle.DefenderId, 1, Request.OpCode);
                response.ToolList.Add(new ItemPair((int)SpecialToolId.Repute, repute));

                //保护时间、且重置战斗时间
                UserRole defenderUserRole;
                //添加事件——通知被复仇的玩家“有人打他成功且抢资源了”
                Storage.Load(out defendUserCity, out defenderUserRole, battle.DefenderId, true);
                defendUserCity.AddEvent(EventType.Main, IsWin, CurrentUserId, 0, null,
                    null, BattleId, response.ToolList);

                //添加护盾时间
                if (defenderUserRole.ProtectEndTime.ToTs() > 0)
                    defenderUserRole.ProtectEndTime = defenderUserRole.ProtectEndTime.AddHours(1);
                else defenderUserRole.ProtectEndTime = DateTime.Now.AddHours(1);

                //护盾有变化 通知大地图有匹配这个玩家的所有玩家！
                defendUserCity.NoticeOtherUserList();

                //修正锁定战斗时间
                defenderUserRole.BeAttackEndTime = DateTime.Now.AddSeconds(ConfigHelper.ResultExpireTime);
                defenderUserRole.BeAttackType = BeAttackType.Result;

                //广播
                var msg = LangResource.GetLangResource(ResourceId.R_0000_AttackDomainWinMsg, userRole.Id,
                                                     userRole.NickName, defenderUserRole.Id, defenderUserRole.NickName);
                if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);

                //神秘/西域商人出现的地方
                bussinessShowCityId = userCity.ShowMysteriousOrWesternShop(0, out shopType);

                //失败了也会有主公经验得到，之前接口就添加了经验了！这里只是显示一下
                response.UserExp = needSp * 1;
                response.HeroExp = heroExp;
                //添加战利品
                BigMapCommon.AddReward(battle, userRole, userHero, userTool, response.ToolList,
                     heroExp, Request.OpCode);
            }
            else
            {
                //失败
                //添加事件——通知领地里匹配到的玩家“有人打他但失败了”
                UserRole defenderUserRole;
                Storage.Load(out defendUserCity, out defenderUserRole, battle.DefenderId, true);
                defendUserCity.AddEvent(EventType.Main, IsWin, CurrentUserId, 0, null, null, BattleId);

                //失败后直接解锁
                defenderUserRole.BeAttackEndTime = DateTime.Now;
                defenderUserRole.BeAttackType = BeAttackType.None;
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

    #region 11026 刷新城池玩家
    /// <summary>
    /// 刷新城池玩家
    /// </summary>
    public class RefreshCityUserResponse
    {
        public RefreshCityUserResponse()
        {
            CityItem = new GetBigMapInfoResponse.ResCityItem();
        }
        /// <summary>
        /// 刷新后的城池
        /// </summary>
        [Tag(1)]
        public GetBigMapInfoResponse.ResCityItem CityItem { get; set; }
    }
    /// <summary>
    /// 刷新城池玩家
    /// </summary>
    [GameCode(OpCode = 11026, ResponseType = typeof(RefreshCityUserResponse))]
    public class RefreshCityUserRequest : GameHandler
    {
        /// <summary>
        /// 刷新的城池
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            var userCity = Storage.Load<UserCity>(CurrentUserId, true);
            var response = new RefreshCityUserResponse();
            var userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "CityItems:CityId", CityId);
                return;
            }

            var serverMapCityItem = DataStorage.Current.Load<ServerMapCityItem>(userCityItem.ServerMapCityItemId, true);

            //城池为Npc占领无法刷新
            if (serverMapCityItem.OwnerType == OwnerType.Npc)
            {
                SetError(ResourceId.R_11026_CityNpcCanNotRefresh);
                return;
            }

            //城主无法刷新城池
            if (serverMapCityItem.OwnerType == OwnerType.User && serverMapCityItem.OwnerId == CurrentUserId)
            {
                SetError(ResourceId.R_11026_CityOwnerCanNotRefresh);
                return;
            }

            //时间未到，无法刷新城池
            if (userCityItem.MatchOrBeOccupiedTime.AddHours(8) > DateTime.Now)
            {
                SetError(ResourceId.R_11026_CanNotRefreshCityUser);
                return;
            }

            //刷新
            userCityItem.Refresh();

            userCityItem = userCity.CityItems.FirstOrDefault(o => o.CityId == CityId);
            if (userCityItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "CityItems:CityId", CityId);
                return;
            }
            var list = new List<UserCityItem>();
            list.Add(userCityItem);
            response.CityItem = BigMapCommon.GetResCityItems(CurrentUserId, list, userCity).FirstOrDefault();
            ResultObj = response;
        }
    }
    #endregion

    #region 11027 获取被复仇玩家防守阵型
    /// <summary>
    /// 获取被复仇玩家防守阵型
    /// </summary>
    [GameCode(OpCode = 11027, ResponseType = typeof(GetDefendFormationResponse))]
    public class GetBeRevengedFormationRequest : GameHandler
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }
        public override void Process(GameContext context)
        {
            var revengeBattle = Storage.Load<AllBattle>(BattleId, true);
            if (revengeBattle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "AllBattle:Id", BattleId);
                return;
            }
            if (revengeBattle.Param2 == 1)
            {
                //已复仇
                SetError(ResourceId.R_11024_BigMapRevengeed);
                return;
            }

            if (revengeBattle.DefenderId != CurrentUserId)
            {
                //复仇战报中 不是防守方
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            var response = new GetDefendFormationResponse();
            response.TargetUserItem.CityId = 0;

            UserCity userCity;
            UserFormation userFormation;
            Storage.Load(out userCity, out userFormation, CurrentUserId, true);

            UserRole defenderUserRole;
            UserHero defenderUserHero;
            UserFormation defenderUserFormation;
            Storage.Load(out defenderUserRole, out defenderUserHero, out defenderUserFormation, revengeBattle.AttackerId);
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

            ResultObj = response;
        }
    }
    #endregion

    #region 11028 刷新领地玩家
    /// <summary>
    /// 刷新领地玩家
    /// </summary>
    public class RefreshDomainUserResponse
    {
        public RefreshDomainUserResponse()
        {
            DomainItem = new GetBigMapInfoResponse.ResDomainItem();
        }
        /// <summary>
        /// 刷新后的领地
        /// </summary>
        [Tag(1)]
        public GetBigMapInfoResponse.ResDomainItem DomainItem { get; set; }
    }
    /// <summary>
    /// 刷新领地玩家
    /// </summary>
    [GameCode(OpCode = 11028, ResponseType = typeof(RefreshDomainUserResponse))]
    public class RefreshDomainUserRequest : GameHandler
    {
        /// <summary>
        /// 刷新的领地
        /// </summary>
        public int CityId { get; set; }
        public override void Process(GameContext context)
        {
            var userCity = Storage.Load<UserCity>(CurrentUserId, true);
            var response = new RefreshDomainUserResponse();
            var userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == CityId);
            if (userDomainItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "CityItems:CityId", CityId);
                return;
            }

            //Npc占领无法刷新
            if (userDomainItem.OwnerType == OwnerType.Npc)
            {
                SetError(ResourceId.R_11026_CityNpcCanNotRefresh);
                return;
            }

            //城主无法刷新城池或领地
            if (userDomainItem.OwnerType == OwnerType.Own)
            {
                SetError(ResourceId.R_11026_CityOwnerCanNotRefresh);
                return;
            }

            //时间未到，无法刷新城池或领地
            if (userDomainItem.MatchOrBeOccupiedTime.AddDays(1) > DateTime.Now)
            {
                SetError(ResourceId.R_11026_CanNotRefreshCityUser);
                return;
            }

            var ownerId = userDomainItem.OwnerId;

            //刷新
            userCity.RefreshDomainUser(CityId);

            //删除玩家，在该玩家大地图已经不显示自己了
            var ouserCity = Storage.Load<UserCity>(ownerId, true);
            ouserCity.ShowMineOfOtherUserIdList.Remove(CurrentUserId);

            userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == CityId);
            if (userDomainItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "CityItems:CityId", CityId);
                return;
            }


            var list = new List<UserDomainItem>();
            list.Add(userDomainItem);
            response.DomainItem = BigMapCommon.GetResDomainItems(list).FirstOrDefault();
            ResultObj = response;
        }
    }
    #endregion

    #region 重置寻访次数 11029
    /// <summary>
    /// 重置寻访次数
    /// </summary>
    [GameCode(OpCode = 11029)]
    public class ResetCitySearchNumRequest : GameHandler
    {
        /// <summary>
        /// 重置寻访次数的 城池ID
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

            var vipLevel = userRole.RealVipLevel;
            var sysVipCfg = SysVipCfg.Find(o => o.VipLevel == vipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg", vipLevel);
                return;
            }
            var curBuyNum = (int)userCityItem.BuySearchNum.Value;
            if (curBuyNum >= sysVipCfg.SearchBuyNum)
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
            var needMoney = sysBuyNumCfg.BuySearchNumMoney;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.BuySearchNum);

            //添加体力 及 购买体力次数
            userCityItem.UseSearchNum -= userCityItem.UseSearchNum.Value;
            userCityItem.BuySearchNum += 1;
        }
    }
    #endregion

    #region 重置内政次数 11030
    /// <summary>
    /// 重置内政次数
    /// </summary>
    [GameCode(OpCode = 11030)]
    public class ResetCityInternalAffairsNumRequest : GameHandler
    {
        /// <summary>
        /// 重置内政次数的 城池ID
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

            var vipLevel = userRole.RealVipLevel;
            var sysVipCfg = SysVipCfg.Find(o => o.VipLevel == vipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg", vipLevel);
                return;
            }
            var curBuyNum = (int)userCityItem.BuyInternalAffairsNum.Value;
            if (curBuyNum >= sysVipCfg.InternalAffairsBuyNum)
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
            var needMoney = sysBuyNumCfg.BuyInternalAffairsNumMoney;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.BuyInternalAffairsNum);

            //重置次数
            userCityItem.UseInternalAffairsNum -= userCityItem.UseInternalAffairsNum.Value;
            userCityItem.BuyInternalAffairsNum += 1;
        }
    }
    #endregion

    #region 11031 领取爵位奖励
    /// <summary>
    /// 领取爵位奖励
    /// </summary>
    [GameCode(OpCode = 11031)]
    public class GetTitleRewardRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);

            if (userRole.TitleLevel == 0)
            {
                SetError(ResourceId.R_11031_NotHaveTitleNoReward);
                return;
            }

            if (userCity.HasGetTitleReward.Value > 0)
            {
                SetError(ResourceId.R_11031_TitleRewardIsGeted);
                return;
            }

            var sysTitleCfg = SysTitleCfg.Find(o => o.Id == userRole.TitleLevel);
            if (sysTitleCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysTitleCfg:Id", userRole.TitleLevel);
                return;
            }

            userRole.AddResource(ItemType.Coin, Request.OpCode, sysTitleCfg.Coin);

            userCity.HasGetTitleReward += 1;
        }
    }
    #endregion

    #region 11032 寻访请求的武将列表
    public class GetIntelHeroListResponse
    {
        public GetIntelHeroListResponse()
        {
            Items = new List<ResHeroItem>();
        }
        /// <summary>
        /// 武将列表
        /// </summary>
        [Tag(1)]
        public List<ResHeroItem> Items { get; set; }
        public class ResHeroItem
        {
            /// <summary>
            /// 武将ID
            /// </summary>
            [Tag(1)]
            public int HeroId { get; set; }
            /// <summary>
            /// 智力
            /// </summary>
            [Tag(2)]
            public int Intel { get; set; }
        }
    }
    /// <summary>
    /// 寻访请求的武将列表
    /// </summary>
    [GameCode(OpCode = 11032, ResponseType = typeof(GetIntelHeroListResponse))]
    public class GetIntelHeroListRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetIntelHeroListResponse();
            var userHero = Storage.Load<UserHero>(CurrentUserId);
            foreach (var userHeroItem in userHero.Items)
            {
                var item = new GetIntelHeroListResponse.ResHeroItem();
                item.HeroId = userHeroItem.HeroId;
                item.Intel = userHeroItem.Intel;

                response.Items.Add(item);
            }
            response.Items = response.Items.OrderByDescending(o => o.Intel).ToList();
            ResultObj = response;
        }
    }
    #endregion

    #region 11033 领取军情里面防守成功的元宝奖励
    /// <summary>
    /// 领取军情里面防守成功的元宝奖励
    /// </summary>
    [GameCode(OpCode = 11033)]
    public class GetEventRewardRequest : GameHandler
    {
        /// <summary>
        /// 军情项ID
        /// </summary>
        public int EventItemId { get; set; }
        public override void Process(GameContext context)
        {
            UserCity userCity;
            UserRole userRole;
            Storage.Load(out userCity, out userRole, CurrentUserId, true);
            var userEventItem = userCity.EventItems.FirstOrDefault(o => o.Id == EventItemId);
            if (userEventItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserCity:EventItems:Id", EventItemId);
                return;
            }

            if (userEventItem.Money == 0)
            {
                //无奖励或者奖励已领取过
                SetError(ResourceId.R_11033_EventRewardIsGeted);
                return;
            }

            RoleManager.AddGiveMoney(userRole, userEventItem.Money);
            userEventItem.Money = 0;
        }
    }
    #endregion

    #region 11034 挑战暗黑活动NPC
    /// <summary>
    /// 挑战暗黑活动NPC
    /// </summary>
    [GameCode(OpCode = 11034, ResponseType = typeof(BigMapPkNpcResponse))]
    public class DiabloPkNpcRequest : BattleHandler
    {
        public override void Process(GameContext context)
        {
            var response = new BigMapPkNpcResponse();

            UserRole userRole;
            UserCity userCity;
            Storage.Load(out userRole, out userCity, CurrentUserId, true);

            var sysActivityCfg = SysActivityCfg.Items.FirstOrDefault(o => o.Type == ActivityType.Diablo && o.IsClose == 0);
            if (sysActivityCfg == null)
            {
                SetError(ResourceId.R_0000_ActivityNotOenOrClose);
                return;
            }

            if (!sysActivityCfg.IsStart())
            {
                SetError(ResourceId.R_0000_ActivityNotStart);
                return;
            }

            var npcId = userCity.DiabloNpcId;
            var sysDiabloCfg = SysDiabloCfg.Items.FirstOrDefault(o => o.Id == npcId);
            if (sysDiabloCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysDiabloCfg:Id", npcId);
                return;
            }

            var needSp = ConfigHelper.BigMapCfgData.BattleNeedSp;
            if (userRole.Sp < needSp)
            {
                //体力不足
                SetError(ResourceId.R_0000_SpNotEnough);
                return;
            }

            //扣除体力,只扣体力，不加经验，在结算的时候加经验
            Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, needSp, 0, 0);

            var battleId = Util.GetSequence(typeof(AllBattle), 0);
            var battle = Storage.Load<AllBattle>(battleId, true);
            battle.Param1 = npcId;
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
            var newTotalCombat = 0;
            foreach (var i in HeroIdList)
            {
                var userHeroItem = userHero.FindByHeroId(i);
                if (userHeroItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", i);
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
            }

            //设置攻击阵型
            userFormation.SetAttFormations(BattleType.None, HeroIdList, LocationIdList, newTotalCombat);

            battle.WarType = WarType.Diablo;
            battle.DefenderId = sysDiabloCfg.Id;
            battle.DefenderLevel = sysDiabloCfg.Level;
            battle.DefenderHeadId = sysDiabloCfg.HeadId;
            battle.DefenderName = sysDiabloCfg.NickName;

            var turnList = new List<int>() { 1, 2, 3 };
            var npcHeroList = SysDiabloHeroCfg.Items.Where(o => o.NpcId == npcId).ToList();
            foreach (var turn in turnList)
            {
                if (turn <= 0) break;
                var tempList = npcHeroList.Where(o => o.Turn == turn).ToList();
                if (tempList.Count == 0) break;

                var battleDefenderHeroItemIdList = new ListIntItem();
                var hList = new ResBattleHeroList();
                //NPC防守阵型
                foreach (var sysDiabloHeroCfg in tempList)
                {
                    int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                    var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                    item.LoadDataFromSysDiabloHeroCfg(sysDiabloHeroCfg);

                    battleDefenderHeroItemIdList.IdItems.Add(battleHeroItemId);
                    hList.HeroItems.Add(item);
                }
                battle.DefenderHeroItemIdListList.Add(battleDefenderHeroItemIdList);
                response.DefenderHeroItemsList.Add(hList);
            }

            //增加战斗技能光环效果
            Utility.AddBattleSkillRing(response.AttackerHeroItems);

            response.BattleId = battle.Id;
            ResultObj = response;
        }
    }
    #endregion

    #region 11035 提交暗黑军团挑战结果
    public class SubmitDiabloResultResponse
    {
        public SubmitDiabloResultResponse()
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
        /// 下一个暗黑军团的NpcId
        /// </summary>
        [Tag(4)]
        public int NpcId { get; set; }
    }
    /// <summary>
    /// 提交暗黑军团挑战结果
    /// </summary>
    [GameCode(OpCode = 11035, ResponseType = typeof(SubmitDiabloResultResponse))]
    public class SubmitDiabloResultRequest : GameHandler
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
            var response = new SubmitDiabloResultResponse();

            var battle = Storage.Load<AllBattle>(BattleId, true);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "BattleId", BattleId);
                return;
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
            if (warType != WarType.Diablo)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            battle.IsWin = IsWin;
            battle.BattleScore = BattleScore;

            UserCity userCity;
            UserRole userRole;
            UserHero userHero;
            UserTool userTool;
            Storage.Load(out userCity, out userRole, out userHero, out userTool, CurrentUserId, true);

            var needSp = ConfigHelper.BigMapCfgData.BattleNeedSp;
            var heroExp = 0;

            var npcId = battle.Param1;
            var sysDiabloCfg = SysDiabloCfg.Items.FirstOrDefault(o => o.Id == npcId);
            if (sysDiabloCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysDiabloCfg:Id", npcId);
                return;
            }

            //失败了也会有主公经验得到，之前接口就添加了经验了！这里只是显示一下
            response.UserExp = needSp * 1;
            if (IsWin == 1)
            {
                //打NPC获得的物品【铜钱、道具、武将经验】
                response.ToolList = BigMapCommon.GetDiabloTooList(npcId, Request.OpCode, out needSp,
                    out heroExp);

                userCity.DiabloNpcId++;
                sysDiabloCfg = SysDiabloCfg.Items.FirstOrDefault(o => o.Id == userCity.DiabloNpcId);
                if (sysDiabloCfg == null)
                {
                    userCity.DiabloNpcId = 0;
                }

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

            response.NpcId = userCity.DiabloNpcId;
            ResultObj = response;
        }
    }
    #endregion

    #region 11036 购买护盾
    public class BuyProtectTimeResponse
    {
        /// <summary>
        /// 护盾截止时间
        /// </summary>
        [Tag(1)]
        public DateTime ProtectEndTime { get; set; }
    }
    /// <summary>
    /// 购买护盾
    /// </summary>
    [GameCode(OpCode = 11036, ResponseType = typeof(BuyProtectTimeResponse))]
    public class BuyProtectTimeRequest : GameHandler
    {
        /// <summary>
        /// 购买的道具id
        /// </summary>
        public int ToolId { get; set; }
        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserCity userCity;
            Storage.Load(out userRole, out userCity, CurrentUserId, true);

            var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == ToolId);
            if (sysToolCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", ToolId);
                return;
            }

            var index = ToolId % 10;
            while (userRole.CoolLaveTimeList.Count < index + 1)
            {
                userRole.CoolLaveTimeList.Add(DateTime.Now.AddMinutes(-1));
            }
            var coolLaveTime = userRole.CoolLaveTimeList[index].ToTs();
            if (coolLaveTime > 0)
            {
                //该道具购买冷却中
                SetError(ResourceId.R_11036_ToolIsInCoolBuy);
                return;
            }

            var needMoney = sysToolCfg.BuyPrice;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            var logItem = new GameLogItem();
            logItem.F1 = ToolId;
            logItem.F2 = needMoney;
            logItem.S1 = userRole.ProtectEndTime.ToLongDateString();

            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.BuyProtectTime);

            //购买冷却时间
            userRole.CoolLaveTimeList[index] = DateTime.Now.AddHours(sysToolCfg.Param2);
            //护盾时间叠加
            var startTime = DateTime.Now;
            if (userRole.ProtectEndTime > DateTime.Now)
                startTime = userRole.ProtectEndTime;
            userRole.ProtectEndTime = startTime.AddHours(sysToolCfg.Param1);

            logItem.F3 = sysToolCfg.Param2;
            logItem.S2 = userRole.ProtectEndTime.ToLongDateString();
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            //护盾有变化 通知大地图有匹配这个玩家的所有玩家！
            userCity.NoticeOtherUserList();

            var response = new BuyProtectTimeResponse();
            response.ProtectEndTime = userRole.ProtectEndTime;

            ResultObj = response;
        }
    }
    #endregion
}
