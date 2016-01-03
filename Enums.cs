using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileGame.tianzi
{
    #region 商店特殊物品id
    /// <summary>
    /// 特殊的商城物品Id
    /// </summary>
    public static class SpecialStoreId
    {
        /// <summary>
        /// 加速任务
        /// </summary>
        public static int AccTask = 1;
        /// <summary>
        /// 钻石一连抽-武将
        /// </summary>
        public static int OneMoneyExtract = 100;
        /// <summary>
        /// 钻石十连抽-武将
        /// </summary>
        public static int TenMoneyExtract = 101;
        /// <summary>
        /// 购买体力sp
        /// </summary>
        public static int BuySpNum = 102;
        /// <summary>
        /// 重置竞技场挑战次数
        /// </summary>
        public static int BuyPkNum = 103;
        /// <summary>
        /// 重置副本关卡挑战次数
        /// </summary>
        public static int BuyLevelNum = 104;
        /// <summary>
        /// 刷新洛阳商人物品列表
        /// </summary>
        public static int RefreshLuoYang = 105;
        /// <summary>
        /// 刷新神秘商人物品列表
        /// </summary>
        public static int RefreshMysterious = 106;
        /// <summary>
        /// 刷新西域商人物品列表
        /// </summary>
        public static int RefreshWestern = 107;
        /// <summary>
        /// 领地强征
        /// </summary>
        public static int StrGainDomainRes = 108;
        /// <summary>
        /// 修改昵称
        /// </summary>
        public static int ModifyNickname = 109;
        /// <summary>
        /// 重置寻访次数
        /// </summary>
        public static int BuySearchNum = 110;
        /// <summary>
        /// 重置内政次数
        /// </summary>
        public static int BuyInternalAffairsNum = 111;
        /// <summary>
        /// 重置翻牌次数
        /// </summary>
        public static int BuyFlopNum = 112;
        /// <summary>
        /// 刷新竞技场商人物品列表
        /// </summary>
        public static int RefreshPk = 113;
        /// <summary>
        /// 加速妃子训练
        /// </summary>
        public static int AccUpgradeConcubine = 114;
        /// <summary>
        /// 加速妃子生产
        /// </summary>
        public static int AccConcubineProduct = 115;
        /// <summary>
        /// 重置采花次数
        /// </summary>
        public static int BuyDeflowerNum = 116;
        /// <summary>
        /// 护盾购买
        /// </summary>
        public static int BuyProtectTime = 117;
        /// <summary>
        /// 钻石一连抽-妃子
        /// </summary>
        public static int Concubine_OneMoneyExtract = 118;
        /// <summary>
        /// 钻石十连抽-妃子
        /// </summary>
        public static int Concubine_TenMoneyExtract = 119;
    }
    #endregion

    #region 物品记录操作类型
    ///// <summary>
    ///// 物品记录操作类型
    ///// </summary>
    //public enum ItemLogOperateType
    //{
    //    /// <summary>
    //    /// 装备升级
    //    /// </summary>
    //    EquipUpgrade = 1,
    //    /// <summary>
    //    /// 熔炼装备
    //    /// </summary>
    //    MeltingEquip = 2,
    //    /// <summary>
    //    /// 抽奖
    //    /// </summary>
    //    Extract = 3,
    //    /// <summary>
    //    /// 武将技能升级
    //    /// </summary>
    //    SkillUpgrade = 4,
    //    /// <summary>
    //    /// GM命令
    //    /// </summary>
    //    Gm = 5,
    //    /// <summary>
    //    /// 装备碎片合成
    //    /// </summary>
    //    EquipChipMixture = 6,
    //    /// <summary>
    //    /// 竞技场宝箱
    //    /// </summary>
    //    PkBox = 7,
    //    /// <summary>
    //    /// 士兵升级
    //    /// </summary>
    //    ArmyUpgrade = 8,
    //    /// <summary>
    //    /// 系统邮件附件
    //    /// </summary>
    //    SysMailAttach = 9,
    //    /// <summary>
    //    /// 背包出售
    //    /// </summary>
    //    BagSell = 10,
    //    /// <summary>
    //    /// 使用经验球
    //    /// </summary>
    //    UseExpBall = 11,
    //}
    #endregion

    #region 武将状态
    /// <summary>
    /// 武将状态
    /// </summary>
    public enum HeroStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 0,
        /// <summary>
        /// 寻访
        /// </summary>
        Visitor = 1,
    }
    #endregion

    #region 背包物品类型
    /// <summary>
    /// 背包物品类型
    /// </summary>
    public enum BagItemType
    {
        /// <summary>
        /// 所有物品
        /// </summary>
        All = 0,
        /// <summary>
        /// 道具【经验球之类的】
        /// </summary>
        Tool = 1,
        /// <summary>
        /// 装备
        /// </summary>
        Equip = 2,
        /// <summary>
        /// 碎片
        /// </summary>
        Chip = 3,
        /// <summary>
        /// 骑宠
        /// </summary>
        Pet = 4,
        /// <summary>
        /// 武将
        /// </summary>
        Hero = 5,
        /// <summary>
        /// 妃子
        /// </summary>
        Concubine = 6,
    }
    #endregion

    #region 聊天类型
    /// <summary>
    /// 聊天类型
    /// </summary>
    public enum ChatType
    {
        /// <summary>
        /// 世界
        /// </summary>
        World = 0,
        /// <summary>
        /// 系统
        /// </summary>
        System = 1,
    }
    #endregion

    #region 特殊Log类型id
    /// <summary>
    /// 特殊Log类型id
    /// </summary>
    public enum SpecialLogType
    {
        /// <summary>
        /// 网站后台操作
        /// </summary>
        GameTask = 99999113,
        /// <summary>
        /// 留存率执行log
        /// </summary>
        KeepBackLog = 99999114,
        /// <summary>
        /// 竞技场每日奖励下发log
        /// </summary>
        PkDayRewardLog = 99999115,
        /// <summary>
        /// 刷新竞技场挑战列表异常log
        /// </summary>
        RefreshPkListLog = 99999116,
        /// <summary>
        /// 修复竞技场奖励错误log
        /// </summary>
        ModifyPkRewardErrorLog = 99999117,
        /// <summary>
        /// 主线任务错误log
        /// </summary>
       MainLineErrorLog = 99999118,
    }
    #endregion

    #region 物品品质
    /// <summary>
    /// 物品品质
    /// </summary>
    public enum ItemQuality
    {
        /// <summary>
        /// 白色
        /// </summary>
        White = 0,
        /// <summary>
        /// 绿色
        /// </summary>
        Green = 1,
        /// <summary>
        /// 蓝色
        /// </summary>
        Blue = 2,
        /// <summary>
        /// 紫色
        /// </summary>
        Purple = 3,
        /// <summary>
        /// 橙色
        /// </summary>
        Orange = 4,
    }
    #endregion

    #region VIP次数类型
    /// <summary>
    /// VIP次数类型
    /// </summary>
    public enum VipNumType
    {
        /// <summary>
        /// 寻访访客
        /// </summary>
        SearchVisitor = 1,
    }
    #endregion

    #region 货币、资源类型
    /// <summary>
    /// 货币、资源类型
    /// </summary>
    public enum MoneyType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 元宝
        /// </summary>
        Money = 1,
        /// <summary>
        /// 铜钱
        /// </summary>
        Coin = 2,
        /// <summary>
        /// 木材
        /// </summary>
        Wood = 3,
        /// <summary>
        /// 石头
        /// </summary>
        Stone = 4,
        /// <summary>
        /// 铁矿
        /// </summary>
        Iron = 5,
        /// <summary>
        /// 体力
        /// </summary>
        Sp = 6,
        /// <summary>
        /// 荣誉
        /// </summary>
        Honor = 7,
    }
    #endregion

    #region 道具类型
    public enum ToolType
    {
        /// <summary>
        /// 不明类型
        /// </summary>
        None = 0,
        /// <summary>
        /// 器魂道具（小、中、大）
        /// </summary>
        EquipExpTool = 100,
        /// <summary>
        /// 经验道具（初级、中级、高级）
        /// </summary>
        ExpTool = 110,
        /// <summary>
        /// 特殊道具（元宝、铜钱、魅力）
        /// </summary>
        SpecialTool = 120,
        /// <summary>
        /// 增加好感度的道具（鲜花、紫苑香囊、雷鸣手镯、湘妃吊坠）
        /// </summary>
        FavorTool = 130,
        /// <summary>
        /// 拜帖香囊
        /// </summary>
        VisitTool = 140,
        /// <summary>
        /// 升级VIP等级
        /// </summary>
        UpgradeVipTool = 150,
        /// <summary>
        /// 增加护盾
        /// </summary>
        AddProtect = 160,
        /// <summary>
        /// 军饷道具（小、中、大）、新手礼包
        /// </summary>
        CoinTool = 210,
        /// <summary>
        /// 体力道具（肉包、烧鸡、武昌鱼）
        /// </summary>
        SpTool = 220,
        /// <summary>
        /// 骑宠碎片
        /// </summary>
        PetChip = 60000000,
        /// <summary>
        /// 武将碎片
        /// </summary>
        HeroChip = 70000000,
        /// <summary>
        /// 妃子碎片
        /// </summary>
        ConcubineChip = 80000000,
        /// <summary>
        /// 装备碎片
        /// </summary>
        EquipChip = 90000000,
    }
    #endregion

    #region 抽取类型
    public enum ExtractType
    {
        /// <summary>
        /// 新用户（用于新手引导）
        /// </summary>
        NewUser = 0,
        /// <summary>
        /// 钻石——武将（普通）
        /// </summary>
        Money = 1,
        /// <summary>
        /// 铜币
        /// </summary>
        Coin = 2,
        /// <summary>
        /// 钻石——武将（十连抽）
        /// </summary>
        TenMoney = 3,
        /// <summary>
        /// 铜币十连抽
        /// </summary>
        TenCoin = 4,
        /// <summary>
        /// 钻石——妃子（普通）
        /// </summary>
        Concubine = 5,
        /// <summary>
        /// 钻石——妃子（十连抽）
        /// </summary>
        TenConcubine = 6,
    }
    /// <summary>
    /// 抽取/商店物品类型
    /// </summary>
    public enum ExtractItemType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 道具
        /// </summary>
        Tool = 1,
        /// <summary>
        /// 武将碎片
        /// </summary>
        HeroChip = 2,
        /// <summary>
        /// 妃子碎片
        /// </summary>
        ConcubineChip = 3,
        /// <summary>
        /// 装备碎片
        /// </summary>
        EquipChip = 4,
        /// <summary>
        /// 装备整卡
        /// </summary>
        Equip = 5,
        /// <summary>
        /// 武将整卡
        /// </summary>
        Hero = 6,
        /// <summary>
        /// 妃子整卡
        /// </summary>
        Concubine = 7,
        /// <summary>
        /// 骑宠碎片
        /// </summary>
        PetChip = 8,
        /// <summary>
        /// 骑宠
        /// </summary>
        Pet = 9,
    }
    #endregion

    #region 各表Id前缀
    public enum TableIdType
    {
        /// <summary>
        /// 妃子
        /// </summary>
        Concubine = 1000000,
        /// <summary>
        /// 技能
        /// </summary>
        Skill = 6000000,
        /// <summary>
        /// 装备
        /// </summary>
        Equip = 7000000,
        /// <summary>
        /// 士兵
        /// </summary>
        Soldier = 8000000,
        /// <summary>
        /// 武将
        /// </summary>
        Hero = 9000000,
    }
    #endregion

    #region 战斗类型
    public enum WarType
    {
        /// <summary>
        /// 竞技场布置防守阵容
        /// </summary>
        PkDefend = 1,
        /// <summary>
        /// 竞技场打真实玩家
        /// </summary>
        PkUser = 2,
        /// <summary>
        /// 竞技场打Npc
        /// </summary>
        PkNpc = 3,
        /// <summary>
        /// 副本关卡
        /// </summary>
        Levels = 4,
        /// <summary>
        /// 大地图领地Npc
        /// </summary>
        DomainNpc = 5,
        /// <summary>
        /// 大地图领地玩家
        /// </summary>
        DomainUser = 6,
        /// <summary>
        /// 大地图城池Npc
        /// </summary>
        CityNpc = 7,
        /// <summary>
        /// 大地图城池玩家
        /// </summary>
        CityUser = 8,
        /// <summary>
        /// 复仇
        /// </summary>
        Revenge = 9,
        /// <summary>
        /// 大地图新手引导战斗
        /// </summary>
        BigMapGuide = 10,
        /// <summary>
        /// 暗黑军团活动战
        /// </summary>
        Diablo = 11,
    }
    /// <summary>
    /// 阵型类型
    /// </summary>
    public enum FormationType
    {
        /// <summary>
        /// 竞技场防守阵型
        /// </summary>
        PkDefend = 0,
        /// <summary>
        /// 大地图防守阵型
        /// </summary>
        BigMapDefend = 1,
    }
    /// <summary>
    /// 战斗的目标类型
    /// </summary>
    public enum BattleTargetType
    {
        /// <summary>
        /// Npc
        /// </summary>
        Npc = 1,
        /// <summary>
        /// 真实玩家
        /// </summary>
        User = 2,
    }

    public enum PkResultType
    {
        /// <summary>
        /// 暂时无结果
        /// </summary>
        None = 0,
        /// <summary>
        /// 攻击方赢
        /// </summary>
        AttackerWin = 1,
        /// <summary>
        /// 防守方赢
        /// </summary>
        DefenderWin = 2,
    }
    #endregion

    #region 新消息模块
    /// <summary>
    /// 有新消息模块
    /// </summary>
    public enum NewMsgType
    {
        /// <summary>
        /// 竞技场
        /// </summary>
        Pk = 0,
        /// <summary>
        /// 邮件
        /// </summary>
        Mail = 1,
        /// <summary>
        /// 大地图
        /// </summary>
        BigMap = 2,
        /// <summary>
        /// 抽奖
        /// </summary>
        Extract = 3,
        /// <summary>
        /// 每日任务
        /// </summary>
        DailyTask = 4,
        /// <summary>
        /// 主线任务
        /// </summary>
        MainLineTask = 5,
        /// <summary>
        /// 将军府
        /// </summary>
        Manor = 6,
        /// <summary>
        /// 后宫
        /// </summary>
        Harem = 7,
        /// <summary>
        /// 充值奖励
        /// </summary>
        Recharge = 8,
        /// <summary>
        /// 登录奖励
        /// </summary>
        Login = 9,
        /// <summary>
        /// 今日充值
        /// </summary>
        TodayRecharge = 10,

        /// <summary>
        /// 最大index
        /// </summary>
        MaxIndex = 10,
    }
    /// <summary>
    /// 开启模块
    /// </summary>
    public enum OpenModuleType
    {
        /// <summary>
        /// 后宫 3
        /// </summary>
        Harem = 1,
        /// <summary>
        /// 大地图 5
        /// </summary>
        BigMap = 2,
        /// <summary>
        /// 每日任务 8
        /// </summary>
        DailyTask = 3,
        /// <summary>
        /// 竞技场 10
        /// </summary>
        Pk = 4,
        /// <summary>
        /// 炼炉 20
        /// </summary>
        Mail = 5,
    }
    #endregion

    #region 特殊toolid
    /// <summary>
    /// 特殊toolid
    /// </summary>
    public enum SpecialToolId
    {
        /// <summary>
        /// 元宝
        /// </summary>
        Money = 12000001,
        /// <summary>
        /// 铜币
        /// </summary>
        Coin = 12000002,
        /// <summary>
        /// 魅力
        /// </summary>
        Charm = 12000003,
        /// <summary>
        /// 声望
        /// </summary>
        Repute = 12000004,
        /// <summary>
        /// 木材
        /// </summary>
        Wood = 12000005,
        /// <summary>
        /// 石头
        /// </summary>
        Stone = 12000006,
        /// <summary>
        /// 铁矿
        /// </summary>
        Iron = 12000007,
        /// <summary>
        /// 体力
        /// </summary>
        Sp = 12000008,
        /// <summary>
        /// 主公经验
        /// </summary>
        UserExp = 12000009,
        /// <summary>
        /// 荣誉(竞技场功勋)
        /// </summary>
        Honor = 12000010,






        /// <summary>
        /// 拜帖
        /// </summary>
        BaiTie = 14000001,
        /// <summary>
        /// 香囊
        /// </summary>
        XiangNang = 14000002,


        /// <summary>
        /// 升级Vip
        /// </summary>
        UpgradeVip = 15000001,
    }
    #endregion

    #region 加速倒计时类型
    /// <summary>
    /// 加速倒计时类型
    /// </summary>
    public enum AccEndTimeType
    {
        /// <summary>
        /// 竞技场
        /// </summary>
        Pk = 500,
        /// <summary>
        /// 建筑升级
        /// </summary>
        BuildingUpgrade = 501,
        /// <summary>
        /// 妃子晋封
        /// </summary>
        ConcubineJinFeng = 502,
        /// <summary>
        /// 武将寻访
        /// </summary>
        HeroXunFang = 503,
    }
    #endregion

    #region 记录log使用——特殊opcode
    /// <summary>
    /// 记录log使用——特殊opcode
    /// </summary>
    public enum OpcodeType
    {
        /// <summary>
        /// UserCity函数MatchNpcOrUser
        /// </summary>
        UserCity_MatchNpcOrUser = 1000,
        /// <summary>
        /// InvestigateRequest
        /// </summary>
        InvestigateRequest = 1001,
    }
    #endregion

    #region 新手的步骤
    /// <summary>
    /// 新手的步骤
    /// </summary>
    public enum GuideStep
    {
        /// <summary>
        /// 第一步：送诸葛亮
        /// </summary>
        One = 1,
        /// <summary>
        /// 第二步：送新手礼包
        /// </summary>
        Two = 2,
        /// <summary>
        /// 第三步：送张飞
        /// </summary>
        Three = 3,
        /// <summary>
        /// 第四步：送孙尚香
        /// </summary>
        Four = 4,
        /// <summary>
        /// 第五步：打副本、战斗引导
        /// </summary>
        Five = 5,
        /// <summary>
        /// 第六步：攻打了第一个BOSS关掉落了装备
        /// </summary>
        Sex = 6,
        /// <summary>
        /// 第七步：佩戴装备
        /// </summary>
        Seven = 7,
        /// <summary>
        /// 第八步：强化装备
        /// </summary>
        Eight = 8,
        /// <summary>
        /// 第九步：升级第一个技能
        /// </summary>
        Nine = 9,
        /// <summary>
        /// 第十步：升级第二个技能
        /// </summary>
        Ten = 10,
        /// <summary>
        /// 第十一步：送妃子赵姬
        /// </summary>
        Eleven = 11,
        /// <summary>
        /// 第十二步：送妃子樊氏
        /// </summary>
        Twelve = 12,
        /// <summary>
        /// 第十三步：建造宫殿 长乐宫
        /// </summary>
        Thirteen = 13,
        /// <summary>
        /// 第十四步：赵姬入住凰宫
        /// </summary>
        Fourteen = 14,
        /// <summary>
        /// 第十五步：樊氏入住长乐宫
        /// </summary>
        Fifteen = 15,
        /// <summary>
        /// 第十六步：引导到大地图保存防守阵型
        /// </summary>
        Sixteen = 16,
        /// <summary>
        /// 第十七步：迎战大魔王
        /// </summary>
        Seventeen = 17,
        /// <summary>
        /// 第十八步：占领领地
        /// </summary>
        Eighteen = 18,
        /// <summary>
        /// 第十九步：占领城池
        /// </summary>
        Nineteen = 19,
        /// <summary>
        /// 第二十步:修葺城墙、招募士兵、治安巡逻
        /// </summary>
        Twenty = 20,
        /// <summary>
        /// 第二十一步：寻访洛阳
        /// </summary>
        TwentyOne = 21,
        /// <summary>
        /// 第二十二步：招募寻访到的妃子
        /// </summary>
        TwentyTwo = 22,
        /// <summary>
        /// 第二十三步：招募寻访到的武将
        /// </summary>
        //TwentyThree = 23,
        /// <summary>
        /// 第二十三步：解锁陈留
        /// </summary>
        TwentyThree = 23,
    }
    #endregion

    #region 客户端使用Tag类型
    /// <summary>
    /// 客户端使用Tag类型
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// 用户
        /// </summary>
        User = 1,
        /// <summary>
        /// 战役
        /// </summary>
        Battle = 2,
        /// <summary>
        /// 装备
        /// </summary>
        Equip = 3,
        /// <summary>
        /// 妃子
        /// </summary>
        Concubine = 4,
        /// <summary>
        /// 武将
        /// </summary>
        Hero = 5,
    }
    #endregion

    #region 邮件类型
    /// <summary>
    /// 邮件类型
    /// </summary>
    public enum MailType
    {
        None = 0,
        /// <summary>
        /// 竞技场
        /// </summary>
        Pk = 1,
        /// <summary>
        /// 公告
        /// </summary>
        Announcement = 2,
    }
    #endregion

    #region 兑换码类型

    public enum RedCodeType
    {
        /// <summary>
        /// 金山云小礼包
        /// </summary>
        JinshanSmall = 1,

        /// <summary>
        /// 金山云专家评测礼包
        /// </summary>
        JinshanExpert = 2,
    }
    #endregion

    #region 添加的数据库定时任务的类型
    public enum DbTaskType
    {
        /// <summary>
        /// 91充值回调
        /// </summary>
        AddIngot = 1,
        /// <summary>
        /// 管理员给特定人添加元宝 改变/不改变vip
        /// </summary>
        ManageAddIngot = 2,
        /// <summary>
        /// 修改密码
        /// </summary>
        ModifyPwd = 3,
        /// <summary>
        /// 添加铜钱、木材、青铜、石头、火药、铁矿
        /// </summary>
        AddRes = 4,
        /// <summary>
        /// 添加妃子、武将、骑宠、装备、道具碎片
        /// </summary>
        AddItems = 5,
    }
    #endregion
}