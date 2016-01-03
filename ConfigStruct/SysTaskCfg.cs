using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 任务配置表
    /// </summary>
    public class SysTaskCfg : TableCfg<SysTaskCfg>
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }

        /// <summary>
        /// 目标数
        /// </summary>
        public int GoalValue { get; set; }

        /// <summary>
        /// 奖励值
        /// </summary>
        public string Rewards { get; set; }

        /// <summary>
        /// 奖励的资源【元宝，主公经验]，铜钱，木材，石头，铁矿,体力】
        /// </summary>
        public List<int> RewardList
        {
            get
            {
                var list = JsonConvert.DeserializeObject<List<int>>(Rewards);
                while (list.Count < 7)
                {
                    list.Add(0);
                }
                return list;
            }
        }

        /// <summary>
        /// 奖励的元宝数
        /// </summary>
        public int Money { get { return RewardList[0]; } }

        /// <summary>
        /// 奖励的用户经验
        /// </summary>
        public int UserExp { get { return RewardList[1]; } }

        /// <summary>
        /// 奖励的铜钱数
        /// </summary>
        public int Coin { get { return RewardList[2]; } }

        /// <summary>
        /// 奖励的木材数
        /// </summary>
        public int Wood { get { return RewardList[3]; } }

        /// <summary>
        /// 奖励的石头数
        /// </summary>
        public int Stone { get { return RewardList[4]; } }

        /// <summary>
        /// 奖励的铁矿数
        /// </summary>
        public int Iron { get { return RewardList[5]; } }

        /// <summary>
        /// 奖励的体力值
        /// </summary>
        public int Sp { get { return RewardList[6]; } }

        /// <summary>
        /// 奖励的道具1
        /// </summary>
        public string RewardTools { get; set; }

        /// <summary>
        /// 奖励的物品【道具、装备妃子武将碎片】[toolid，toolnum]
        /// </summary>
        public List<int> RewardToolList
        {
            get
            {
                var list = JsonConvert.DeserializeObject<List<int>>(RewardTools);
                while (list.Count < 2)
                {
                    list.Add(0);
                }
                return list;
            }
        }

        /// <summary>
        /// 奖励的物品ID(碎片、道具)
        /// </summary>
        public int ToolId { get { return RewardToolList[0]; } }

        /// <summary>
        /// 奖励的物品数量(碎片、道具)
        /// </summary>
        public int ToolNum
        {
            get
            {
                var num = 0;
                if (RewardToolList.Count > 1) num = RewardToolList[1];
                return num;
            }
        }

        /// <summary>
        /// 奖励的道具2
        /// </summary>
        public string RewardTools2 { get; set; }

        /// <summary>
        /// 奖励的物品2【道具、装备妃子武将碎片】[toolid，toolnum]
        /// </summary>
        public List<int> RewardToolList2
        {
            get
            {
                var list = JsonConvert.DeserializeObject<List<int>>(RewardTools2);
                while (list.Count < 2)
                {
                    list.Add(0);
                }
                return list;
            }
        }

        /// <summary>
        /// 奖励的物品ID2(碎片、道具)
        /// </summary>
        public int ToolId2 { get { return RewardToolList2[0]; } }

        /// <summary>
        /// 奖励的物品数量2(碎片、道具)
        /// </summary>
        public int ToolNum2
        {
            get
            {
                var num = 0;
                if (RewardToolList.Count > 1) num = RewardToolList2[1];
                return num;
            }
        }

        /// <summary>
        /// 任务分类
        /// </summary>
        public TaskSort TaskSort { get { return (TaskSort)(Id / 100000); } }

        /// <summary>
        /// 任务类型（int)
        /// </summary>
        public int TaskType
        {
            get
            {
                if (MainLineType != MainLineType.None) return (int)MainLineType;
                return (int)DailyType;
            }
        }

        /// <summary>
        /// 主线任务分类
        /// </summary>
        public MainLineType MainLineType
        {
            get
            {
                if (TaskSort == TaskSort.MainLine)
                    return (MainLineType)((Id - (int)(TaskSort) * 100000) / 1000);
                return ConfigStruct.MainLineType.None;
            }
        }

        /// <summary>
        /// 每日任务分类
        /// </summary>
        public DailyType DailyType
        {
            get
            {
                if (TaskSort == TaskSort.Daily)
                    return (DailyType)((Id - (int)(TaskSort) * 100000) / 1000);
                return ConfigStruct.DailyType.None;
            }
        }

        /// <summary>
        /// 是否显示进度/任务触发的城池ID
        /// </summary>
        public int ShowProRate { get; set; }
    }

    /// <summary>
    /// 任务分类
    /// </summary>
    public enum TaskSort
    {
        /// <summary>
        /// 主线任务
        /// </summary>
        MainLine = 1,
        /// <summary>
        /// 每日任务
        /// </summary>
        Daily = 2,
        /// <summary>
        /// 登录任务
        /// </summary>
        Login = 3,
        /// <summary>
        /// 充值任务
        /// </summary>
        Recharge = 4,
        /// <summary>
        /// 剧情任务
        /// </summary>
        Story = 5,
        /// <summary>
        /// 签到
        /// </summary>
        Sign = 6,
        /// <summary>
        /// 今日充值
        /// </summary>
        TodayRecharge = 7,
    }

    /// <summary>
    /// 主线任务类型
    /// </summary>
    public enum MainLineType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 通过指定关卡
        /// </summary>
        PassLevels = 1,
        /// <summary>
        /// 达到指定主公等级
        /// </summary>
        UserLevel = 2,
        /// <summary>
        /// 占领指定城池
        /// </summary>
        OccupiedCity = 3,
        /// <summary>
        /// 招募英雄数量
        /// </summary>
        RecruitHeroNum = 4,
        /// <summary>
        /// 招募妃子数量
        /// </summary>
        RecruitConcubineNum = 5,
        /// <summary>
        /// 声望达到
        /// </summary>
        ReputeReach = 6,
        /// <summary>
        /// 魅力达到
        /// </summary>
        CharmReach = 7,

        Max = 7,
    }

    /// <summary>
    /// 每日任务类型
    /// </summary>
    public enum DailyType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 挑战竞技场
        /// </summary>
        Pk = 1,
        /// <summary>
        /// 挑战副本 
        /// </summary>
        Levels = 2,
        /// <summary>
        /// 技能升级
        /// </summary>
        SkillUpgrade = 3,
        /// <summary>
        /// 装备附魔
        /// </summary>
        EquipUpgradeStar = 4,
        /// <summary>
        /// 抽奖 
        /// </summary>
        Extract = 5,
        /// <summary>
        /// 领取体力
        /// </summary>
        ReceiveSp = 6,
        /// <summary>
        /// 采花
        /// </summary>
        Deflower = 7,
        /// <summary>
        /// 领地战斗
        /// </summary>
        DomainBattle = 8,
        /// <summary>
        /// 寻访
        /// </summary>
        SearchVisitor = 9,
        /// <summary>
        /// 翻牌
        /// </summary>
        Flop = 10,

        Max = 10,
    }
}
