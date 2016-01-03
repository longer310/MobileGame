using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;
using Newtonsoft.Json;

namespace MobileGame.tianzi.Repository
{
    #region 10000 获取副本关卡界面信息
    public class GetLevelsInfoResponse
    {
        public GetLevelsInfoResponse()
        {
            Items = new List<LevelsItem>();
        }
        /// <summary>
        /// 关卡(大关)列表
        /// </summary>
        [Tag(1)]
        public List<LevelsItem> Items { get; set; }
        /// <summary>
        /// 开启的最后一个关卡的Id
        /// </summary>
        [Tag(2)]
        public int OpenedMaxLevelId { get; set; }

        public class LevelsItem
        {
            /// <summary>
            /// 关卡id
            /// </summary>
            [Tag(1)]
            public int LevelId { get; set; }

            /// <summary>
            /// 关卡星级
            /// </summary>
            [Tag(2)]
            public int LevelStar { get; set; }

            /// <summary>
            /// 今日已购买次数
            /// </summary>
            [Tag(3)]
            public int BuyNum { get; set; }

            /// <summary>
            /// 今日已使用次数
            /// </summary>
            [Tag(4)]
            public int UseNum { get; set; }
        }
    }
    /// <summary>
    /// 获取副本关卡界面信息
    /// </summary>
    [GameCode(OpCode = 10000, ResponseType = typeof(GetLevelsInfoResponse))]
    public class GetLevelsInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var userLevels = Storage.Load<UserLevels>(CurrentUserId, true);
            var response = new GetLevelsInfoResponse();
            var list =
                userLevels.Items.OrderByDescending(o => o.SysLevelCfg.BattleId)
                    .ThenByDescending(o => o.SysLevelCfg.Index)
                    .ToList();
            foreach (var userLevelsItem in list)
            {
                response.Items.Add(new GetLevelsInfoResponse.LevelsItem()
                {
                    LevelId = userLevelsItem.LevelId,
                    LevelStar = userLevelsItem.LevelStar > 3 ? 3 : userLevelsItem.LevelStar,
                    BuyNum = (int)userLevelsItem.BuyNum.Value,
                    UseNum = (int)userLevelsItem.UseNum.Value
                });
            }

            response.OpenedMaxLevelId = userLevels.OpenedMaxLevelId;

            ResultObj = response;
        }
    }
    #endregion

    #region 10001 挑战副本关卡
    /// <summary>
    /// 挑战副本关卡需要下发的数据
    /// </summary>
    public class PkLevelsResponse
    {
        public PkLevelsResponse()
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
        /// 防守方武将列表【以列表的长度判断是否有NPC】
        /// </summary>
        [Tag(3)]
        public List<ResBattleHeroList> DefenderHeroItemsList { get; set; }
    }

    public class ResBattleHeroList
    {
        public ResBattleHeroList()
        {
            HeroItems = new List<BattleHeroItem>();
        }
        /// <summary>
        /// 防守方武将列表【以列表的长度判断是否有NPC】
        /// </summary>
        [Tag(1)]
        public List<BattleHeroItem> HeroItems { get; set; }
    }
    /// <summary>
    /// 挑战副本关卡
    /// </summary>
    [GameCode(OpCode = 10001, ResponseType = typeof(PkLevelsResponse))]
    public class PkLevelsRequest : BattleHandler
    {
        public override void Process(GameContext context)
        {
            if (HeroIdList.Count <= 0)
            {
                SetError(ResourceId.R_0000_BattleAtleastNeedOneHero);
                return;
            }

            var sysLevelsCfg = SysLevelCfg.Find(TargetId);
            if (sysLevelsCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysLevelCfg:Id", TargetId);
                return;
            }

            var sysBattleCfg = SysBattleCfg.Find(sysLevelsCfg.BattleId);
            if (sysBattleCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBattleCfg:Id", sysLevelsCfg.BattleId);
                return;
            }

            var npcHeroList = SysLevelHeroCfg.Items.Where(o => o.NpcId == sysLevelsCfg.NpcId).ToList();
            if (npcHeroList.Count <= 0)
            {
                SetError(ResourceId.R_0000_HaveNotNpcHero);
                return;
            }

            var response = new PkLevelsResponse();

            UserRole userRole;
            UserLevels userLevels;
            UserHero userHero;
            UserFormation userFormation;
            //UserTool userTool;, out userTool
            Storage.Load(out userRole, out userLevels, out userHero, out userFormation, CurrentUserId, true);


            if (sysBattleCfg.NeedLevel > userRole.Level)
            {
                //等级不足
                SetError(ResourceId.R_0000_UserLowLevel, sysBattleCfg.NeedLevel);
                return;
            }

            //if (userTool.IsFull)
            //{
            //    //背包容量不足
            //    SetError(ResourceId.R_0000_ToolBagIsFull);
            //    return;
            //}

            //装备背包不足
            //if (userEquip.IsFull)
            //{
            //    SetError(ResourceId.R_0000_EquipBagIsFull);
            //    return;
            //}

            var userLevelsItem = userLevels.Items.FirstOrDefault(o => o.LevelId == TargetId);

            if (TargetId > userLevels.OpenedMaxLevelId)
            {
                //该关卡未开启
                SetError(ResourceId.R_10001_LevelNotOpen);
                return;
            }
            if (userLevelsItem != null)
            {
                if (userLevelsItem.LaveNum <= 0)
                {
                    //关卡挑战次数不足
                    SetError(ResourceId.R_10001_LevelTimesNotEnough);
                    return;
                }
            }
            else
            {
                //小关逻辑
                if (TargetId < userLevels.OpenedMaxLevelId)
                {
                    //已挑战过该关卡
                    SetError(ResourceId.R_10001_PassedLevel);
                    return;
                }
            }
            if (userRole.Sp < sysLevelsCfg.NeedSp)
            {
                //体力不足
                SetError(ResourceId.R_0000_SpNotEnough);
                return;
            }

            //添加副本任务次数
            Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.Levels);

            var battleId = Util.GetSequence(typeof(AllBattle), 0);
            var battle = Storage.Load<AllBattle>(battleId, true);
            //var battle = KVEntity.CreateNew<AllBattle>();//TIP:需要保存的实体 使用load createnew适合实体中的列表项！！！
            battle.WarType = WarType.Levels;

            battle.AttackerId = CurrentUserId;
            battle.AttackerLevel = userRole.Level;
            battle.AttackerHeadId = userRole.HeadId;
            battle.AttackerName = userRole.NickName;

            //攻击方武将详细信息
            var index = 0;
            userFormation.AttFormation.Clear();
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

                //保存最新的阵容列表
                userFormation.AttFormation.Add(new FormationItem() { HeroId = i, Location = item.Location });
            }

            if (newTotalCombat > userFormation.AttMaxCombat)
            {
                userFormation.SetMaxComatAndFormation(newTotalCombat,userFormation.AttFormation);
            }

            battle.DefenderId = TargetId;
            battle.DefenderLevel = sysLevelsCfg.GuardLevel;
            battle.DefenderHeadId = sysLevelsCfg.GuardPic;
            battle.DefenderName = sysLevelsCfg.GuardName;
            index = 0;
            var turnList = new List<int>() { 1, 2, 3 };
            var saveLevelsData = ConfigHelper.SaveLevelsData;
            foreach (var turn in turnList)
            {
                if (turn <= 0) break;

                var hList = new ResBattleHeroList();
                var tempList = npcHeroList.Where(o => o.Turn == turn).ToList();
                if (tempList.Count == 0) break;
                //NPC防守阵型
                var defenderHeroItemIdList = new ListIntItem();
                foreach (var sysLevelHeroCfg in tempList)
                {
                    if (saveLevelsData == 1)
                    {
                        int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                        var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                        item.LoadDataFromSysNpcHeroCfg(sysLevelHeroCfg);

                        defenderHeroItemIdList.IdItems.Add(battleHeroItemId);

                        hList.HeroItems.Add(item);
                    }
                    else
                    {
                        //攻打NPC不用存储武将信息
                        var item = new BattleHeroItem();
                        item.LoadDataFromSysNpcHeroCfg(sysLevelHeroCfg);
                        hList.HeroItems.Add(item);
                    }
                }
                if (saveLevelsData == 1 && defenderHeroItemIdList.IdItems.Count > 0)
                    battle.DefenderHeroItemIdListList.Add(defenderHeroItemIdList);

                response.DefenderHeroItemsList.Add(hList);

                index++;
            }

            //增加战斗技能光环效果
            Utility.AddBattleSkillRing(response.AttackerHeroItems);

            response.BattleId = battle.Id;
            ResultObj = response;
        }
    }
    #endregion

    #region 10002 提交副本挑战结果
    public class SubmitLevelResultResponse
    {
        public SubmitLevelResultResponse()
        {
            ToolList = new List<ItemPair>();
        }
        /// <summary>
        /// 攻击方是否胜利
        /// </summary>
        //[Tag(1)]
        //public int IsWin { get; set; }
        ///// <summary>
        ///// 战斗评分
        ///// </summary>
        //[Tag(2)]
        //public int BattleScore { get; set; }
        /// <summary>
        /// 获得的用户经验
        /// </summary>
        [Tag(3)]
        public int UserExp { get; set; }
        /// <summary>
        /// 获得的英雄经验
        /// </summary>
        [Tag(4)]
        public int HeroExp { get; set; }
        /// <summary>
        /// 获得的道具列表
        /// </summary>
        [Tag(5)]
        public List<ItemPair> ToolList { get; set; }
    }
    /// <summary>
    /// 提交副本挑战结果
    /// </summary>
    [GameCode(OpCode = 10002, ResponseType = typeof(SubmitLevelResultResponse))]
    public class SubmitLevelResultRequest : GameHandler
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }
        /// <summary>
        /// 攻击方是否胜利
        /// </summary>
        public int IsWin { get; set; }
        /// <summary>
        /// 战斗评分[0、1、2、3]
        /// </summary>
        public int BattleScore { get; set; }
        /// <summary>
        /// 回合字符串列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string RoundsStr { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (BattleId == 0 || (IsWin != 0 && IsWin != 1) || BattleScore > 3) return false;//IsWin != -1 && 
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new SubmitLevelResultResponse();

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
            if (battle.WarType != WarType.Levels)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            var logItem = new GameLogItem();

            #region 副本
            UserRole userRole;
            UserLevels userLevels;
            UserHero userHero;
            UserTool userTool;
            UserChip userChip;
            Storage.Load(out userRole, out userHero,out userChip, out userTool, out userLevels, CurrentUserId, true);
            userLevels.AddBattle(battle.Id);

            var attackerHeroList = new List<BattleHeroItem>();
            //跳过战斗
            if (IsWin == -1)
            {
                if (userRole.Level < 30 || userRole.RealVipLevel < 1)
                {
                    SetError(ResourceId.R_10002_NeedLevelAndVip);
                    return;
                }
                if (userLevels.OpenedMaxLevelId > battle.DefenderId)
                {
                    SetError(ResourceId.R_10002_CanNotJumpBattle);
                    return;
                }
                attackerHeroList = Storage.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
                var defenderHeroList = Storage.LoadList<BattleHeroItem>(battle.BattleDefenderHeroItemIdList.ToArray());
                var attackerCombat = attackerHeroList.Sum(o => o.Combat);
                var defenderCombat = defenderHeroList.Sum(o => o.Combat);

                //简单判断输赢 TODO:
                if (attackerCombat > defenderCombat)
                {
                    IsWin = 1;
                    if (attackerCombat < (int)(1.1 * defenderCombat)) BattleScore = 1;
                    else if (attackerCombat < (int)(1.2 * defenderCombat)) BattleScore = 2;
                    else if (attackerCombat < (int)(1.3 * defenderCombat)) BattleScore = 3;
                    else if (attackerCombat < (int)(1.4 * defenderCombat)) BattleScore = 4;
                    else BattleScore = 5;
                }
                else IsWin = 0;
            }
            else
            {
                attackerHeroList = Storage.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
            }

            battle.IsWin = IsWin;
            battle.BattleScore = BattleScore;

            //response.IsWin = IsWin;
            //response.BattleScore = BattleScore;

            var levelId = battle.DefenderId;
            var sysLevelsCfg = SysLevelCfg.Find(levelId);
            if (sysLevelsCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysLevelCfg:Id", levelId);
                return;
            }
            var userExp = 0;
            if (IsWin == 1)
            {
                var oldMaxLevelsId = userLevels.OpenedMaxLevelId;

                //扣除体力
                Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, sysLevelsCfg.NeedSp);
                //userRole.Sp -= sysLevelsCfg.NeedSp;

                //添加武将经验
                var heroExp = sysLevelsCfg.HeroExp;
                userExp = sysLevelsCfg.NeedSp * 1;
                var coin = sysLevelsCfg.Coin;
                foreach (var i in attackerHeroList)
                {
                    var heroItem = userHero.FindByHeroId(i.HeroId);
                    heroItem.AddExp(heroExp, Request.OpCode);
                }
                //添加用户经验
                //Utility.AddUserExp(userRole, userExp, Request.OpCode);
                Utility.AddResource(userRole, ItemType.Coin, Request.OpCode, coin);
                response.ToolList.Add(new ItemPair((int)SpecialToolId.Coin, coin));

                //添加概率道具
                var prob = sysLevelsCfg.Probability;
                foreach (var i in sysLevelsCfg.ToolList)
                {
                    if (i == 0) break;
                    var hit = false;

                    //if (sysLevelsCfg.BattleId == 1 && sysLevelsCfg.Index == 2 && userLevels.OpenedMaxLevelId == sysLevelsCfg.Id)
                    //{
                    //    //第一次打赢 第一个BOSS关卡 必掉落装备
                    //    hit = true;
                    //}
                    //else
                    //{
                    hit = Util.IsHit(prob * 1.0 / 100);
                    //}

                    if (hit)
                    {
                        if (i > 9999999)
                        {//道具

                            //userTool.TryAdd(i, 1, Request.OpCode);
                            //TaskCommon.AddReward(userRole, i, 1, Request.OpCode);
                            var sysToolCfg = SysToolCfg.Find(o => o.Id == i);
                            if (sysToolCfg == null)
                            {
                                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", i);
                                return;
                            }
                            sysToolCfg.AddToUser(Request.OpCode, userChip, userRole, userTool, 1);
                        }
                        else if (i > 7000000 && i < 7999999)
                        {
                            //白色装备
                            var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);
                            userEquip.AddEquipToUser(i, Request.OpCode);
                        }
                        else if (i > 5000000 && i < 5001000)
                        {
                            //骑宠
                            var userPet = Storage.Load<UserPet>(CurrentUserId, true);
                            userPet.AddPetToUser(i, Request.OpCode);
                        }
                        else if (i > 5001000 && i < 5999999)
                        {
                            //妃子
                            var userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                            userConcubine.AddConcubineToUser(i, Request.OpCode);
                        }
                        response.ToolList.Add(new ItemPair(i, 1));
                    }
                }

                //开启新关卡：函数内判断
                userLevels.OpenNewLevel(sysLevelsCfg);

                var userLevelsItem = userLevels.Items.FirstOrDefault(o => o.LevelId == levelId);
                if (userLevelsItem != null)
                {
                    //大关有次数 成功才扣次数
                    userLevelsItem.UseNum += 1;
                    if (userLevelsItem.LevelStar < BattleScore)
                    {
                        //星星改变
                        userLevelsItem.LevelStar = BattleScore;
                    }
                }

                response.HeroExp = heroExp;

                logItem.F3 = heroExp;
                logItem.F5 = coin;
                logItem.S1 = JsonConvert.SerializeObject(response.ToolList);

                if (oldMaxLevelsId < userLevels.OpenedMaxLevelId)
                {
                    //添加到任务里面去
                    Utility.AddMainLineTaskGoalData(CurrentUserId, MainLineType.PassLevels, userLevels.OpenedMaxLevelId - 1);
                }
            }
            else
            {
                //失败扣除的体力为胜利的一半
                userExp = (int)(sysLevelsCfg.NeedSp * 0.5);
                Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, userExp);
                //userRole.Sp -= (int)(sysLevelsCfg.NeedSp * 0.5);
            }
            #endregion

            response.UserExp = userExp;
            logItem.F4 = userExp;
            try
            {
                var saveLevelsData = ConfigHelper.SaveLevelsData;
                if (saveLevelsData == 1)
                {
                    //保存回合过程
                    var roundIdListList = Utility.AddBattleRound(RoundsStr);
                    battle.RoundIdListList = roundIdListList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            logItem.F1 = BattleId;
            logItem.F2 = IsWin;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion

    #region 10003 购买副本次数
    /// <summary>
    /// 购买副本次数
    /// </summary>
    [GameCode(OpCode = 10003)]
    public class BuyLevelNumRequest : GameHandler
    {
        /// <summary>
        /// 关卡ID
        /// </summary>
        public int LevelId { get; set; }
        public override void Process(GameContext context)
        {
            var sysLevelsCfg = SysLevelCfg.Find(LevelId);
            if (sysLevelsCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysLevelCfg:Id", LevelId);
                return;
            }
            if (sysLevelsCfg.Times == 0)
            {
                //小关无法购买次数
                SetError(ResourceId.R_10003_LevelCanNotBuyTimes);
                return;
            }

            UserLevels userLevels;
            UserRole userRole;
            Storage.Load(out userRole, out userLevels, CurrentUserId, true);

            if (userLevels.OpenedMaxLevelId < LevelId)
            {
                //关卡还未开启
                SetError(ResourceId.R_10001_LevelNotOpen);
                return;
            }

            var userLevelsItem = userLevels.Items.FirstOrDefault(o => o.LevelId == LevelId);

            if (userLevelsItem == null)
            {
                userLevelsItem = userLevels.AddLevelsItem(sysLevelsCfg.Id);
            }

            if (userLevelsItem.LaveNum > 0)
            {
                //次数还有剩余，无需购买
                SetError(ResourceId.R_7000_HavePkNumCannotBuy);
                return;
            }

            var sysVipCfg = SysVipCfg.Find(o => o.VipLevel == userRole.VipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg", userRole.VipLevel);
                return;
            }
            if (sysVipCfg.LevelBuyNum <= userLevelsItem.BuyNum.Value)
            {
                //购买次数已用完
                SetError(ResourceId.R_0000_NoBuyNum);
                return;
            }
            var sysBuyNumCfg = SysBuyNumCfg.Find((int)userLevelsItem.BuyNum.Value + 1);
            if (sysBuyNumCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBuyNumCfg", (int)userLevelsItem.BuyNum.Value + 1);
                return;
            }
            var needMoney = sysBuyNumCfg.BuyLevelNumMoney;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.BuyLevelNum);

            //添加挑战次数
            userLevelsItem.BuyNum += 1;
            userLevelsItem.BuyNums++;
            userLevelsItem.UseNum -= userLevelsItem.UseNum.Value;

            var logItem = new GameLogItem();
            logItem.F1 = sysLevelsCfg.Times;
            logItem.F2 = needMoney;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 10004 扫荡副本
    public class SweepingResponse
    {
        public SweepingResponse()
        {
            Items = new List<SweepingItem>();
        }

        /// <summary>
        /// 扫荡列表
        /// </summary>
        [Tag(1)]
        public List<SweepingItem> Items { get; set; }
        public class SweepingItem
        {
            public SweepingItem()
            {
                ToolList = new List<ItemPair>();
            }
            /// <summary>
            /// 获得的用户经验
            /// </summary>
            [Tag(1)]
            public int UserExp { get; set; }
            /// <summary>
            /// 获得的道具列表
            /// </summary>
            [Tag(2)]
            public List<ItemPair> ToolList { get; set; }

        }
    }
    /// <summary>
    /// 提交副本挑战结果
    /// </summary>
    [GameCode(OpCode = 10004, ResponseType = typeof(SweepingResponse))]
    public class SweepingRequest : GameHandler
    {
        /// <summary>
        /// 关卡id
        /// </summary>
        public int LevelId { get; set; }
        /// <summary>
        /// 扫荡次数
        /// </summary>
        public int Num { get; set; }
        public override void Process(GameContext context)
        {
            var sysLevelsCfg = SysLevelCfg.Find(LevelId);
            if (sysLevelsCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysLevelCfg:Id", LevelId);
                return;
            }
            UserLevels userLevels;
            UserRole userRole;
            UserTool userTool;
            UserChip userChip;
            Storage.Load(out userLevels, out userRole, out userTool,out userChip, CurrentUserId, true);
            var userLevelsItem = userLevels.Items.FirstOrDefault(o => o.LevelId == LevelId);
            if (userLevelsItem == null)
            {
                //关卡还未开启
                SetError(ResourceId.R_10001_LevelNotOpen);
                return;
            }
            var sysBattleCfg = SysBattleCfg.Find(o => o.Id == sysLevelsCfg.BattleId);
            if (sysBattleCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "sysBattleCfg:Id", sysLevelsCfg.BattleId);
                return;
            }
            if (sysBattleCfg.NeedLevel > userRole.Level)
            {
                //等级不足
                SetError(ResourceId.R_0000_UserLowLevel, sysBattleCfg.NeedLevel);
                return;
            }
            if (userLevelsItem.LevelStar < 3)
            {
                //关卡未完美通关
                SetError(ResourceId.R_10002_LevelNotPerfectPass);
                return;
            }
            if (userLevelsItem.LaveNum < Num)
            {
                //挑战次数不足
                SetError(ResourceId.R_10001_LevelTimesNotEnough);
                return;
            }
            var needSp = Num * sysLevelsCfg.NeedSp;
            if (userRole.Sp < needSp)
            {
                //体力不足
                SetError(ResourceId.R_0000_SpNotEnough);
                return;
            }

            var coin = sysLevelsCfg.Coin * Num;
            var response = new SweepingResponse();
            while (Num > 0)
            {
                var item = new SweepingResponse.SweepingItem();
                item.ToolList.Add(new ItemPair((int)SpecialToolId.Coin, sysLevelsCfg.Coin));

                //添加概率道具
                var prob = sysLevelsCfg.Probability;
                foreach (var i in sysLevelsCfg.ToolList)
                {
                    if (i == 0) break;
                    var hit = false;
                    hit = Util.IsHit(prob * 1.0 / 100);

                    if (hit)
                    {
                        if (i > 9999999)
                        {//道具
                            //userTool.TryAdd(i, 1, Request.OpCode);
                            var sysToolCfg = SysToolCfg.Find(o => o.Id == i);
                            if (sysToolCfg == null)
                            {
                                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", i);
                                return;
                            }
                            sysToolCfg.AddToUser(Request.OpCode, userChip, userRole, userTool, 1);
                        }
                        else if (i > 7000000 && i < 7999999)
                        {
                            //白色装备
                            var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);
                            userEquip.AddEquipToUser(i, Request.OpCode);
                        }
                        else if (i > 5000000 && i < 5001000)
                        {
                            //骑宠
                            var userPet = Storage.Load<UserPet>(CurrentUserId, true);
                            userPet.AddPetToUser(i, Request.OpCode);
                        }
                        else if (i > 5001000 && i < 5999999)
                        {
                            //妃子
                            var userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                            userConcubine.AddConcubineToUser(i, Request.OpCode);
                        }
                        item.ToolList.Add(new ItemPair(i, 1));
                    }
                }

                var toolId = (int)ToolType.ExpTool * 100000 + 1;
                foreach (var i in sysLevelsCfg.SweepingToolNumsList)
                {
                    if (i > 0)
                    {
                        //添加道具【额外奖励武将经验】
                        var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == toolId);
                        if (sysToolCfg == null)
                        {
                            SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", toolId);
                            return;
                        }
                        var toolItem = item.ToolList.FirstOrDefault(o => o.ItemId == toolId);
                        if (toolItem == null) item.ToolList.Add(new ItemPair(toolId, i));
                        else toolItem.Num += i;
                        userTool.TryAdd(toolId, i, Request.OpCode);
                    }
                    toolId++;
                }
                userLevelsItem.UseNum += 1;
                item.UserExp = sysLevelsCfg.NeedSp;
                Num--;

                response.Items.Add(item);
            }
            //扣除体力
            Utility.ConsumeResource(userRole, ItemType.Sp, Request.OpCode, needSp);
            Utility.AddResource(userRole, ItemType.Coin, Request.OpCode, coin);

            //添加副本任务次数
            Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.Levels);

            ResultObj = response;
        }
    }
    #endregion
}
