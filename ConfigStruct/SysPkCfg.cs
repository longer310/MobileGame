using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 竞技场匹配及奖励配置
    /// </summary>
    public class SysPkCfg : TableCfg<SysPkCfg>
    {
        /// <summary>
        /// 名次1
        /// </summary>
        public int StarRank { get; set; }
        /// <summary>
        /// 名次2
        /// </summary>
        public int EndRank { get; set; }
        /// <summary>
        /// 等级1
        /// </summary>
        public int StarLevel { get; set; }
        /// <summary>
        /// 等级2
        /// </summary>
        public int EndLevel { get; set; }

        /// <summary>
        /// 排名奖励元宝
        /// </summary>
        public int Money { get; set; }

        /// <summary>
        /// 排名奖励荣誉
        /// </summary>
        public int Honor { get; set; }

        /// <summary>
        /// 排名奖励铜钱
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// 排名奖励道具id
        /// </summary>
        public int ToolId { get; set; }

        /// <summary>
        /// 排名奖励道具数量
        /// </summary>
        public int ToolNum { get; set; }

        /// <summary>
        /// 没一级奖励的元宝数
        /// </summary>
        public float UpOneRewardMoney { get; set; }
    }
}
