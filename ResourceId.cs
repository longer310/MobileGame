using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi
{
    public class ResourceId : BaseResourceId
    {
        #region 公用
        /// <summary>
        /// Id[{0}={1}]不存在
        /// </summary>
        public static string R_0000_IdNotExist = "R_0000_IdNotExist";
        /// <summary>
        /// 战斗已提交过结果
        /// </summary>
        public static string R_0000_BattleHaveResult = "R_0000_BattleHaveResult";
        /// <summary>
        /// 铜币不足
        /// </summary>
        public static string R_0000_CoinNotEnough = "R_0000_CoinNotEnough";
        /// <summary>
        /// 木材不足
        /// </summary>
        public static string R_0000_WoodNotEnough = "R_0000_WoodNotEnough";
        /// <summary>
        /// 石头不足
        /// </summary>
        public static string R_0000_StoneNotEnough = "R_0000_StoneNotEnough";
        /// <summary>
        /// 铁矿不足
        /// </summary>
        public static string R_0000_IronNotEnough = "R_0000_IronNotEnough";
        /// <summary>
        /// 元宝不足
        /// </summary>
        public static string R_0000_MoneyNotEnough = "R_0000_MoneyNotEnough";
        /// <summary>
        /// 荣誉值不足
        /// </summary>
        public static string R_0000_HonorNotEnough = "R_0000_HonorNotEnough";
        /// <summary>
        /// 体力不足
        /// </summary>
        public static string R_0000_SpNotEnough = "R_0000_SpNotEnough";
        /// <summary>
        /// 武将星级不足
        /// </summary>
        public static string R_0000_HeroStarLevelNotEnough = "R_0000_HeroStarLevelNotEnough";
        /// <summary>
        /// 武将为非空闲状态
        /// </summary>
        public static string R_0000_HeroNotIdle = "R_0000_HeroNotIdle";
        /// <summary>
        /// 声望不足
        /// </summary>
        public static string R_0000_ReputeNotEnough = "R_0000_ReputeNotEnough";
        /// <summary>
        /// 魅力不足
        /// </summary>
        public static string R_0000_CharmNotEnough = "R_0000_CharmNotEnough";
        /// <summary>
        /// 根据某个条件没有找到相应的数据
        /// 条件:{0},在表{1}中没有找到相应的数据
        /// </summary>
        public static string R_0000_NotFindData = "R_0000_NotFindData";
        /// <summary>
        /// 主公等级不足
        /// 需主公{0}级开放
        /// </summary>
        public static string R_0000_UserLowLevel = "R_0000_UserLowLevel";
        /// <summary>
        /// 昵称太长
        /// </summary>
        public static string R_0000_NickNameTooLong = "R_0000_NickNameTooLong";
        /// <summary>
        /// 今天购买次数已用完
        /// </summary>
        public static string R_0000_NoBuyNum = "R_0000_NoBuyNum";
        /// <summary>
        /// 不合法的参数
        /// </summary>
        public static string R_0000_IllegalParam = "R_0000_IllegalParam";
        /// <summary>
        /// VIP等级不足
        /// </summary>
        public static string R_0000_VIPNotEnough = "R_0000_VIPNotEnough";
        /// <summary>
        /// 昵称非法
        /// </summary>
        public static string R_0000_IllegalName = "R_0000_IllegalName";
        /// <summary>
        /// 旧数据异常，请刷新
        /// </summary>
        public static string R_0000_OldDataError = "R_0000_OldDataError";
        /// <summary>
        /// 装备容量已满
        /// </summary>
        public static string R_0000_EquipBagIsFull = "R_0000_EquipBagIsFull";
        /// <summary>
        /// 骑宠容量已满
        /// </summary>
        public static string R_0000_PetBagIsFull = "R_0000_PetBagIsFull";
        /// <summary>
        /// 道具容量已满
        /// </summary>
        public static string R_0000_ToolBagIsFull = "R_0000_ToolBagIsFull";
        /// <summary>
        /// 挑战次数不足
        /// </summary>
        public static string R_0000_BattleNumNotEnough = "R_0000_BattleNumNotEnough";
        /// <summary>
        /// {0}已达上限
        /// </summary>
        public static string R_0000_ResourceIsFull = "R_0000_ResourceIsFull";
        /// <summary>
        /// 铜钱
        /// </summary>
        public static string R_0000_CoinName = "R_0000_CoinName";
        /// <summary>
        /// 石头
        /// </summary>
        public static string R_0000_StoneName = "R_0000_StoneName";
        /// <summary>
        /// 木材
        /// </summary>
        public static string R_0000_WoodName = "R_0000_WoodName";
        /// <summary>
        /// 铁矿
        /// </summary>
        public static string R_0000_IronName = "R_0000_IronName";
        /// <summary>
        /// 道具不足
        /// </summary>
        public static string R_0000_ToolNotEnough = "R_0000_ToolNotEnough";
        /// <summary>
        /// 好感度不足
        /// </summary>
        public static string R_0000_FavorNotEnough = "R_0000_FavorNotEnough";
        /// <summary>
        /// 数据异常无法回放
        /// </summary>
        public static string R_0000_BattleDataError = "R_0000_BattleDataError";
        /// <summary>
        /// 你的体力太多啦，先消耗一些吧。
        /// </summary>
        public static string R_0000_SpIsFull = "R_0000_SpIsFull";
        /// <summary>
        /// 战斗至少需要一个武将
        /// </summary>
        public static string R_0000_BattleAtleastNeedOneHero = "R_0000_BattleAtleastNeedOneHero";
        /// <summary>
        /// 暂时未配置NPC武将防守
        /// </summary>
        public static string R_0000_HaveNotNpcHero = "R_0000_HaveNotNpcHero";
        /// <summary>
        /// 不可与自己战斗
        /// </summary>
        public static string R_0000_CanNotBattleOwn = "R_0000_CanNotBattleOwn";
        /// <summary>
        /// 正在被攻击，资源处于锁定状态，倒计时%d秒
        /// </summary>
        public static string R_0000_BeAttackingCanNotUseRes = "R_0000_BeAttackingCanNotUseRes";
        /// <summary>
        /// 正在等待抢劫，资源处于锁定状态，倒计时%d秒
        /// </summary>
        public static string R_0000_WaitResultCanNotUseRes = "R_0000_WaitResultCanNotUseRes";
        /// <summary>
        /// 后宫正在被抢，资源处于锁定状态，倒计时%d秒
        /// </summary>
        public static string R_0000_BeRobingCanNotUseRes = "R_0000_BeRobingCanNotUseRes";
        /// <summary>
        /// 该类型战报不可分享
        /// </summary>
        public static string R_0000_BattleCanNotShare = "R_0000_BattleCanNotShare";
        /// <summary>
        /// 战斗未结束，不能分享
        /// </summary>
        public static string R_0000_BattleNotEnd = "R_0000_BattleNotEnd";
        /// <summary>
        /// 您已分享了一段视频回放，请在 XX分钟XX秒 后再次分享
        /// </summary>
        public static string R_0000_ShareCooling = "R_0000_ShareCooling";
        /// <summary>
        /// 刷新次数不足，请提升Vip等级
        /// </summary>
        public static string R_0000_RefreshNotEnough = "R_0000_RefreshNotEnough";
        /// <summary>
        /// 强征次数不足，请提升Vip等级
        /// </summary>
        public static string R_0000_StrGainNotEnough = "R_0000_StrGainNotEnough";
        /// <summary>
        /// 昵称没有改变，不用保存
        /// </summary>
        public static string R_0000_NickNameNotChange = "R_0000_NickNameNotChange";
        /// <summary>
        /// 昵称重复
        /// </summary>
        public static string R_0000_NickNameDuplex = "R_0000_NickNameDuplex";
        /// <summary>
        /// 原密码错误
        /// </summary>
        public static string R_0000_OldPwdError = "R_0000_OldPwdError";
        /// <summary>
        /// 新密码不能为空
        /// </summary>
        public static string R_0000_NewPwdNotEmpty = "R_0000_NewPwdNotEmpty";
        /// <summary>
        /// 寻访次数不足
        /// </summary>
        public static string R_0000_SearchNotEnough = "R_0000_SearchNotEnough";
        /// <summary>
        /// 拜帖不足
        /// </summary>
        public static string R_0000_BaiTieNotEnough = "R_0000_BaiTieNotEnough";
        /// <summary>
        /// 香囊不足
        /// </summary>
        public static string R_0000_XiangnangNotEnough = "R_0000_XiangnangNotEnough";
        /// <summary>
        /// 活动未开启或者已关闭
        /// </summary>
        public static string R_0000_ActivityNotOenOrClose = "R_0000_ActivityNotOenOrClose";
        /// <summary>
        /// 活动未开始
        /// </summary>
        public static string R_0000_ActivityNotStart = "R_0000_ActivityNotStart";
        #endregion

        #region 武将
        /// <summary>
        /// 还有技能点，无需购买
        /// </summary>
        public static string R_2002_SkillPointIsEnough = "R_2002_SkillPointIsEnough";
        /// <summary>
        /// 今日购买次数已用完
        /// </summary>
        public static string R_2002_TodayBuyNumUsed = "R_2002_TodayBuyNumUsed";
        /// <summary>
        /// 技能已达到最高级
        /// </summary>
        public static string R_2003_HeroIsMaxSkill = "R_2003_HeroIsMaxSkill";
        /// <summary>
        /// 技能等级不能超过英雄等级
        /// </summary>
        public static string R_2003_SkillLevelCannotHigherHero = "R_2003_SkillLevelCannotHigherHero";
        /// <summary>
        /// 星级已达到最高级
        /// </summary>
        public static string R_2004_HeroIsMaxStar = "R_2004_HeroIsMaxStar";
        /// <summary>
        /// 碎片不足
        /// </summary>
        public static string R_2004_ChisNotEnough = "R_2004_ChisNotEnough";
        /// <summary>
        /// 不是经验球道具
        /// </summary>
        public static string R_2006_IsNotExpTool = "R_2006_IsNotExpTool";
        /// <summary>
        /// 经验球不足
        /// </summary>
        public static string R_2006_ExpBallNotEnough = "R_2006_ExpBallNotEnough";
        /// <summary>
        /// 士兵等级不能超过英雄等级
        /// </summary>
        public static string R_2003_ArmyLevelCannotHigherHero = "R_2003_ArmyLevelCannotHigherHero";
        #endregion

        #region 背包
        /// <summary>
        /// 不能出售已佩戴的装备
        /// </summary>
        public static string R_4003_CanNotSellEquipedEquip = "R_4003_CanNotSellEquipedEquip";
        /// <summary>
        /// 碎片不足
        /// </summary>
        public static string R_4004_ChipNotEnough = "R_4004_ChipNotEnough";
        /// <summary>
        /// 传参ID不是碎片
        /// </summary>
        public static string R_4004_NotChip = "R_4004_NotChip";
        /// <summary>
        /// 该武将已存在
        /// </summary>
        public static string R_4004_HeroIsExist = "R_4004_HeroIsExist";
        /// <summary>
        /// 该妃子已存在
        /// </summary>
        public static string R_4004_ConcubineIsExist = "R_4004_ConcubineIsExist";
        /// <summary>
        /// 不能出售已佩戴的骑宠
        /// </summary>
        public static string R_4003_CanNotSellEquipedPet = "R_4003_CanNotSellEquipedPet";
        /// <summary>
        /// 该物品无法使用
        /// </summary>
        public static string R_4007_ToolCanNotUse = "R_4007_ToolCanNotUse";
        #endregion

        #region 装备
        /// <summary>
        /// 装备等级已达到最高级
        /// </summary>
        public static string R_6000_EquipIsMaxLevel = "R_6000_EquipIsMaxLevel";
        /// <summary>
        /// 装备等级不能大于主公等级
        /// </summary>
        public static string R_6000_EquipLevelCannotUpUserRole = "R_6000_EquipLevelCannotUpUserRole";
        /// <summary>
        /// 装备星级已达到最高级
        /// </summary>
        public static string R_6001_EquipIsMaxStar = "R_6001_EquipIsMaxStar";
        /// <summary>
        /// 只有绿色及以上品质的装备才能升星
        /// </summary>
        public static string R_6002_EquipQualityTooLow = "R_6002_EquipQualityTooLow";
        /// <summary>
        /// 非器魂道具
        /// </summary>
        public static string R_6002_NotEquipExpTool = "R_6002_NotEquipExpTool";
        /// <summary>
        /// 英雄等级未达到，不能装备
        /// </summary>
        public static string R_6002_HeroNotEnoughLevel = "R_6002_HeroNotEnoughLevel";
        /// <summary>
        /// 该英雄未佩戴该装备
        /// </summary>
        public static string R_6003_HeroNotEquipThisEqip = "R_6003_HeroNotEquipThisEqip";
        /// <summary>
        /// 不能熔炼已佩戴的装备
        /// </summary>
        public static string R_6005_CanNotMeltingEquipedEquip = "R_6005_CanNotMeltingEquipedEquip";
        /// <summary>
        /// 不能熔炼蓝色品质以下的装备
        /// </summary>
        public static string R_6005_CanNotMeltingUnderBlueEquip = "R_6005_CanNotMeltingUnderBlueEquip";
        /// <summary>
        /// 该英雄未佩戴该骑宠
        /// </summary>
        public static string R_6006_HeroNotEquipThisPet = "R_6006_HeroNotEquipThisPet";
        #endregion

        #region 好友
        /// <summary>
        /// 玩家昵称申请添加您为好友，请到【系统】->【好友申请】里面处理。
        /// </summary>
        public static string R_ApplyFriend = "R_ApplyFriend";

        /// <summary>
        /// 玩家昵称和您已经互相添加为好友了,你们可以发私信啦。
        /// </summary>
        public static string R_AlreadyFriend = "R_AlreadyFriend";
        #endregion

        #region 竞技场
        /// <summary>
        /// 次数还有剩余，无需购买
        /// </summary>
        public static string R_7000_HavePkNumCannotBuy = "R_7000_HavePkNumCannotBuy";
        /// <summary>
        /// 需先清除冷却时间
        /// </summary>
        public static string R_7000_HaveCoolTimeCannotBuy = "R_7000_HaveCoolTimeCannotBuy";
        /// <summary>
        /// 排名发生改变
        /// </summary>
        public static string R_7002_RankChanged = "R_7002_RankChanged";
        /// <summary>
        /// 玩家正在被攻击
        /// </summary>
        //public static string R_7002_RankChanged = "R_7002_RankChanged";
        /// <summary>
        /// 挑战之星数量不足
        /// </summary>
        public static string R_7008_StarNumNotEnough = "R_7008_StarNumNotEnough"; 
        #endregion

        #region 邮件
        /// <summary>
        /// 此消息没有附件
        /// </summary>
        public static string R_1303_MailHaveNotAttach = "R_1303_MailHaveNotAttach";
        /// <summary>
        /// 竞技场最高排名奖励邮件内容
        /// </summary>
        public static string R_HighestRankMsg = "R_HighestRankMsg";
        /// <summary>
        /// 竞技场每日排名奖励邮件内容
        /// </summary>
        public static string R_DayRankMsg = "R_DayRankMsg";
        /// <summary>
        /// 邮件附件已领取
        /// </summary>
        public static string R_1303_AnnGetedAttach = "R_1303_AnnGetedAttach";
        #endregion

        #region 后宫
        /// <summary>
        /// 该建筑已建
        /// </summary>
        public static string R_9001_BuildIsExist = "R_9001_BuildIsExist";
        /// <summary>
        /// 宫殿升级冷却中
        /// </summary>
        public static string R_9001_BuildingUpgradeCding = "R_9001_BuildingUpgradeCding";
        /// <summary>
        /// 妃子已经入住宫殿
        /// </summary>
        public static string R_9004_ConcubineAlreadyInBuilding = "R_9004_ConcubineAlreadyInBuilding";
        /// <summary>
        /// 宫殿妃子已满
        /// </summary>
        public static string R_9004_BuildingIsFull = "R_9004_BuildingIsFull";
        /// <summary>
        /// 妃子正在休息
        /// </summary>
        public static string R_9005_ConcubineAlreadyIdle = "R_9005_ConcubineAlreadyIdle";
        /// <summary>
        /// 妃子不在该宫殿
        /// </summary>
        public static string R_9005_ConcubineNotInBuilding = "R_9005_ConcubineNotInBuilding";
        /// <summary>
        /// 妃子还不能收获
        /// </summary>
        public static string R_9005_ConcubineCanNotGain = "R_9005_ConcubineCanNotGain";
        /// <summary>
        /// 妃子无需加速收获
        /// </summary>
        public static string R_9026_ConcubineCanNotAccGain = "R_9026_ConcubineCanNotAccGain";
        /// <summary>
        /// 翻牌次数不足
        /// </summary>
        public static string R_9008_FlopNumNotEnough = "R_9008_FlopNumNotEnough";
        /// <summary>
        /// 妃子等级不能高于主公等级
        /// </summary>
        public static string R_9008_ConcubineLevelCanNotHigherUser = "R_9008_ConcubineLevelCanNotHigherUser";
        /// <summary>
        /// 妃子满级了~
        /// </summary>
        public static string R_9008_ConcubineLevelFull = "R_9008_ConcubineLevelFull";
        /// <summary>
        /// 不是增加好感度的道具
        /// </summary>
        public static string R_9010_IsNotFavorTool = "R_9010_IsNotFavorTool";
        /// <summary>
        /// 好感度已满
        /// </summary>
        public static string R_9010_FavorIsFull = "R_9010_FavorIsFull";
        /// <summary>
        /// 晋封冷却中
        /// </summary>
        public static string R_9009_JinFengCding = "R_9009_JinFengCding";
        /// <summary>
        /// 封号已满
        /// </summary>
        public static string R_9009_ConcubineTitleIsFull = "R_9009_ConcubineTitleIsFull";
        /// <summary>
        /// 本次战斗抢钱抢粮抢女人时间到
        /// </summary>
        public static string R_9017_RobExpired = "R_9017_RobExpired";
        /// <summary>
        /// 该建筑不存在
        /// </summary>
        public static string R_9018_BuildIsNotExist = "R_9018_BuildIsNotExist";
        /// <summary>
        /// 该建筑已被抢劫过
        /// </summary>
        public static string R_9019_BuildIsRobed = "R_9019_BuildIsRobed";
        /// <summary>
        /// 采花次数已用完
        /// </summary>
        public static string R_9019_FlowersNumIsOver = "R_9019_FlowersNumIsOver";
        /// <summary>
        /// 训练位置不存在
        /// </summary>
        public static string R_9023_TrainLocationNotExist = "R_9023_TrainLocationNotExist";
        /// <summary>
        /// 妃子不在训练位置上
        /// </summary>
        public static string R_9023_ConcubineNotAtTrainLocation = "R_9023_ConcubineNotAtTrainLocation";
        /// <summary>
        /// 训练位置还未开启
        /// </summary>
        public static string R_9023_TrainLocationNotOpen = "R_9023_TrainLocationNotOpen";
        /// <summary>
        /// 该位置已有妃子训练
        /// </summary>
        public static string R_9023_TrainLocationIsFull = "R_9023_TrainLocationIsFull";
        /// <summary>
        /// 该位置没有妃子在训练
        /// </summary>
        public static string R_9024_TrainLocationNotHaveConcubine = "R_9024_TrainLocationNotHaveConcubine";
        /// <summary>
        /// 训练已完成，无需加速
        /// </summary>
        public static string R_9024_TrainOver = "R_9024_TrainOver";
        /// <summary>
        /// 训练未完成
        /// </summary>
        public static string R_9025_TrainNotOver = "R_9025_TrainNotOver";
        #endregion

        #region 副本
        /// <summary>
        /// 关卡挑战次数不足
        /// </summary>
        public static string R_10001_LevelTimesNotEnough = "R_10001_LevelTimesNotEnough";
        /// <summary>
        /// 已挑战过该关卡
        /// </summary>
        public static string R_10001_PassedLevel = "R_10001_PassedLevel";
        /// <summary>
        /// 该关卡还未开启
        /// </summary>
        public static string R_10001_LevelNotOpen = "R_10001_LevelNotOpen";
        /// <summary>
        /// 该关卡无需购买次数
        /// </summary>
        public static string R_10003_LevelCanNotBuyTimes = "R_10003_LevelCanNotBuyTimes";
        /// <summary>
        /// 未通过该关卡，还不能扫荡
        /// </summary>
        public static string R_10002_CanNotJumpBattle = "R_10002_CanNotJumpBattle";
        /// <summary>
        /// 需要主公等级大于30级或者vip1以上
        /// </summary>
        public static string R_10002_NeedLevelAndVip = "R_10002_NeedLevelAndVip";
        /// <summary>
        /// 关卡未完美通关不能扫荡
        /// </summary>
        public static string R_10002_LevelNotPerfectPass = "R_10002_LevelNotPerfectPass";
        /// <summary>
        /// 装备
        /// </summary>
        public static string R_10001_EquipFullCanNotBattle = "R_10001_EquipFullCanNotBattle";
        #endregion

        #region 大地图
        /// <summary>
        /// 已经探索过该区域
        /// </summary>
        public static string R_11001_AreaAlreadyOpened = "R_11001_AreaAlreadyOpened";
        /// <summary>
        /// 城池未开启
        /// </summary>
        public static string R_11002_CityNotOpen = "R_11002_CityNotOpen";
        /// <summary>
        /// 城池的全服信息未创建
        /// </summary>
        public static string R_11002_ServerCityNotCreate = "R_11002_ServerCityNotCreate";
        /// <summary>
        /// 该玩家正在被攻打
        /// </summary>
        public static string R_11005_BeAttacking = "R_11005_BeAttacking";
        /// <summary>
        /// 不能攻击，玩家开启了护盾
        /// </summary>
        public static string R_11005_IsProtecting = "R_11005_IsProtecting";
        /// <summary>
        /// 未占领该城池，无权操作
        /// </summary>
        public static string R_11003_ServerCityOccupiedByOther = "R_11003_ServerCityOccupiedByOther";
        /// <summary>
        /// 该领地已是自己的
        /// </summary>
        public static string R_11004_DomainAlreadyOwn = "R_11004_DomainAlreadyOwn";
        /// <summary>
        /// 征收冷却中
        /// </summary>
        public static string R_11007_GainCoolTime = "R_11007_GainCoolTime";
        /// <summary>
        /// 事件已读
        /// </summary>
        public static string R_11011_EventIsRead = "R_11011_EventIsRead";
        /// <summary>
        /// 该武将已经在寻访
        /// </summary>
        public static string R_11012_HeroUsedSearch = "R_11012_HeroUsedSearch";
        /// <summary>
        /// 该城池已经在寻访
        /// </summary>
        public static string R_11012_CitySearched = "R_11012_CitySearched";
        /// <summary>
        /// 寻访队列已满
        /// </summary>
        public static string R_11012_SearchQueueIsFull = "R_11012_SearchQueueIsFull";
        /// <summary>
        /// 来访者已离开
        /// </summary>
        public static string R_11014_VisitorLeaved = "R_11014_VisitorLeaved";
        /// <summary>
        /// 已拜访过该来访者
        /// </summary>
        public static string R_11014_VisitedVisitor = "R_11014_VisitedVisitor";
        /// <summary>
        /// 来晚了~已被人提前拜访完了
        /// </summary>
        public static string R_11014_VisitLateNoTimes = "R_11014_VisitLateNoTimes";
        /// <summary>
        /// 本城今日处理内政的次数已用光
        /// </summary>
        public static string R_11017_InternalAffairsNumNotEnough = "R_11017_InternalAffairsNumNotEnough";
        /// <summary>
        /// 城防很坚固，无需加固
        /// </summary>
        public static string R_11017_DefenseIsFull = "R_11017_DefenseIsFull";
        /// <summary>
        /// 士兵充裕，无需招募
        /// </summary>
        public static string R_11017_ArmyIsFull = "R_11017_ArmyIsFull";
        /// <summary>
        /// 士气很盛，无需鼓舞
        /// </summary>
        public static string R_11017_MoraleIsFull = "R_11017_MoraleIsFull";
        /// <summary>
        /// 商店已刷新，该物品已不存在
        /// </summary>
        public static string R_11020_ShopAlreadyRefresh = "R_11020_ShopAlreadyRefresh";
        /// <summary>
        /// 该物品已购买过
        /// </summary>
        public static string R_11020_ShopGoodsBuyed = "R_11020_ShopGoodsBuyed";

        /// <summary>
        /// 无法征收，请先去占领一些领地
        /// </summary>
        public static string R_11023_NotOccupiedDomain = "R_11023_NotOccupiedDomain";

        /// <summary>
        /// 时间未到，无法刷新城池
        /// </summary>
        public static string R_11026_CanNotRefreshCityUser = "R_11026_CanNotRefreshCityUser";

        /// <summary>
        /// 城主无法刷新城池
        /// </summary>
        public static string R_11026_CityOwnerCanNotRefresh = "R_11026_CityOwnerCanNotRefresh";

        /// <summary>
        /// 城池为Npc占领无法刷新
        /// </summary>
        public static string R_11026_CityNpcCanNotRefresh = "R_11026_CityNpcCanNotRefresh";

        /// <summary>
        /// 已复仇,无法攻打第二次
        /// </summary>
        public static string R_11024_BigMapRevengeed = "R_11024_BigMapRevengeed";

        /// <summary>
        /// 今日已领取爵位奖励
        /// </summary>
        public static string R_11031_TitleRewardIsGeted = "R_11031_TitleRewardIsGeted";

        /// <summary>
        /// 未册封爵位没有俸禄
        /// </summary>
        public static string R_11031_NotHaveTitleNoReward = "R_11031_NotHaveTitleNoReward";

        /// <summary>
        /// 军情元宝奖励已领取
        /// </summary>
        public static string R_11033_EventRewardIsGeted = "R_11033_EventRewardIsGeted";

        /// <summary>
        /// 该道具购买冷却中
        /// </summary>
        public static string R_11036_ToolIsInCoolBuy = "R_11036_ToolIsInCoolBuy";



        /// <summary>
        /// 城池被攻打胜利
        /// </summary>
        public static string R_BeCityUserWin = "R_BeCityUserWin";
        /// <summary>
        /// 城池被攻打失败
        /// </summary>
        public static string R_BeCityUserFail = "R_BeCityUserFail";
        /// <summary>
        /// 领地被攻打胜利
        /// </summary>
        public static string R_BeDomainUserWin = "R_BeDomainUserWin";
        /// <summary>
        /// 领地被攻打失败
        /// </summary>
        public static string R_BeDomainUserFail = "R_BeDomainUserFail";
        /// <summary>
        /// 领地被NPC攻陷
        /// </summary>
        public static string R_DomainBeMatchNpc = "R_DomainBeMatchNpc";
        /// <summary>
        /// 领地被玩家攻陷
        /// </summary>
        public static string R_DomainBeMatchUser = "R_DomainBeMatchUser";
        /// <summary>
        /// 寻访失败
        /// </summary>
        public static string R_SearchFail = "R_SearchFail";
        /// <summary>
        /// 寻访成功
        /// </summary>
        public static string R_SearchSuccess = "R_SearchSuccess";
        /// <summary>
        /// 不是女将
        /// </summary>
        public static string R_NotFemaleHero = "R_NotFemaleHero";
        /// <summary>
        /// 不是武力型武将
        /// </summary>
        public static string R_NotWuHero = "R_NotWuHero";
        /// <summary>
        /// 不是智力型武将
        /// </summary>
        public static string R_NotZhiero = "R_NotZhiero";
        /// <summary>
        /// 不是统率型武将
        /// </summary>
        public static string R_NotTongHero = "R_NotTongHero";
        /// <summary>
        /// 所带兵种不是骑兵
        /// </summary>
        public static string R_TakeArmyNotRider = "R_TakeArmyNotRider";
        /// <summary>
        /// 所带兵种不是步兵
        /// </summary>
        public static string R_TakeArmyNotInfantry = "R_TakeArmyNotInfantry";
        /// <summary>
        /// 所带兵种不是弓兵
        /// </summary>
        public static string R_TakeArmyNotArmor = "R_TakeArmyNotArmor";
        /// <summary>
        /// 所带兵种不是策士
        /// </summary>
        public static string R_TakeArmyNotSpell = "R_TakeArmyNotSpell";
        /// <summary>
        /// 所带兵种不是机械兵
        /// </summary>
        public static string R_TakeArmyNotMachinery = "R_TakeArmyNotMachinery";
        /// <summary>
        /// 所带兵种不是布甲兵
        /// </summary>
        public static string R_TakeArmyNotCloth = "R_TakeArmyNotCloth";
        /// <summary>
        /// 所带兵种不是轻甲兵
        /// </summary>
        public static string R_TakeArmyNotLightArmor = "R_TakeArmyNotLightArmor";
        /// <summary>
        /// 所带兵种不是重甲兵
        /// </summary>
        public static string R_TakeArmyNotHeavyArmor = "R_TakeArmyNotHeavyArmor";
        /// <summary>
        /// 不是近战武将
        /// </summary>
        public static string R_NotFrontHero = "R_NotFrontHero";
        /// <summary>
        /// 不是远程武将
        /// </summary>
        public static string R_NotAfterHero = "R_NotAfterHero";
        #endregion

        #region 任务
        /// <summary>
        /// 任务未完成或奖励已领取
        /// </summary>
        public static string R_12001_RewardGainedOrNotReach = "R_12001_RewardGainedOrNotReach";
        /// <summary>
        /// 任务奖励已领取
        /// </summary>
        public static string R_12001_RewardGaine = "R_12001_RewardGained";
        /// <summary>
        /// 任务未完成
        /// </summary>
        public static string R_12001_NotReach = "R_12001_NotReach";
        /// <summary>
        /// 领取时间未到
        /// </summary>
        public static string R_12001_TimeNotReach = "R_12001_TimeNotReach";
        /// <summary>
        /// 今日已签到
        /// </summary>
        public static string R_12008_TodaySigned = "R_12008_TodaySigned";
        #endregion

        #region 新手引导
        /// <summary>
        /// 该步骤已设置过
        /// </summary>
        public static string R_5002_GuideStepSeted = "R_5002_GuideStepSeted";
        /// <summary>
        /// 不能跳跃设置新手步骤
        /// </summary>
        public static string R_5002_CanNotJumpGuideStep = "R_5002_CanNotJumpGuideStep";
        #endregion

        #region 系统消息
        /// <summary>
        /// 合成品质3及以上装备系统消息
        /// </summary>
        public static string R_0000_MixtureEquipMsg = "R_0000_MixtureEquipMsg";
        /// <summary>
        /// 合成品质3及以上妃子系统消息
        /// </summary>
        public static string R_0000_MixtureConcubineMsg = "R_0000_MixtureConcubineMsg";
        /// <summary>
        /// 合成品质3及以上武将系统消息
        /// </summary>
        public static string R_0000_MixtureHeroMsg = "R_0000_MixtureHeroMsg";
        /// <summary>
        /// 寻访品质3及以上装备系统消息
        /// </summary>
        public static string R_0000_ExtractEquipMsg = "R_0000_ExtractEquipMsg";
        /// <summary>
        /// 寻访品质3及以上妃子系统消息
        /// </summary>
        public static string R_0000_ExtractConcubineMsg = "R_0000_ExtractConcubineMsg";
        /// <summary>
        /// 寻访品质3及以上武将系统消息
        /// </summary>
        public static string R_0000_ExtractHeroMsg = "R_0000_ExtractHeroMsg";
        /// <summary>
        /// 攻打领地内玩家成功系统消息
        /// </summary>
        public static string R_0000_AttackDomainWinMsg = "R_0000_AttackDomainWinMsg";
        /// <summary>
        /// 装备升星到4星及以上系统消息
        /// </summary>
        public static string R_0000_AdvancedEquipMsg = "R_0000_AdvancedEquipMsg";
        /// <summary>
        /// 武将升星到4星及以上系统消息
        /// </summary>
        public static string R_0000_AdvancedHeroMsg = "R_0000_AdvancedHeroMsg";
        /// <summary>
        /// 分享战报消息
        /// </summary>
        public static string R_0000_ShareBattleMsg = "R_0000_ShareBattleMsg";
        /// <summary>
        /// 竞技场
        /// </summary>
        public static string R_0000_PkName = "R_0000_PkName";
        /// <summary>
        /// 大地图
        /// </summary>
        public static string R_0000_BigMapName = "R_0000_BigMapName";
        /// <summary>
        /// 今日
        /// </summary>
        public static string R_0000_Today = "R_0000_Today";
        /// <summary>
        /// 明日
        /// </summary>
        public static string R_0000_Tomorrow = "R_0000_Tomorrow";

        /// <summary>
        /// 爵位变化时下发的内容
        /// </summary>
        public static string R_TitleChange = "R_TitleChange";


        #endregion

        #region 系统消息
        /// <summary>
        /// 不存在该兑换码
        /// </summary>
        public static string R_1501_RedCodeNotExist = "R_1501_RedCodeNotExist";
        /// <summary>
        /// 已领取过该类型的兑换码
        /// </summary>
        public static string R_1501_RedCodeTypeIsGeted = "R_1501_RedCodeTypeIsGeted";
        /// <summary>
        /// 该兑换码已失效
        /// </summary>
        public static string R_1501_RedCodeIsGeted = "R_1501_RedCodeIsGeted";
        #endregion
    }
}
