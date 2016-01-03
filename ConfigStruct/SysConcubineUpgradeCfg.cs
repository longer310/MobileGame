using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统妃子升级配置
    /// </summary>
    public class SysConcubineUpgradeCfg : TableCfg<SysConcubineUpgradeCfg>
    {
        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 妃子等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 需要主公的等级
        /// </summary>
        public int NeedLevel { get; set; }
        /// <summary>
        /// 升级需要的铜钱
        /// </summary>
        public int NeedCoin { get; set; }
        /// <summary>
        /// 升级需要的木材
        /// </summary>
        public int NeedWood { get; set; }
        /// <summary>
        /// 升级需要的石头
        /// </summary>
        public int NeedStone { get; set; }
        /// <summary>
        /// 升级需要的铁矿
        /// </summary>
        public int NeedIron { get; set; }
        /// <summary>
        /// 升级需要的时间（分钟）
        /// </summary>
        public int NeedTime { get; set; }

        public static SysConcubineUpgradeCfg GetCfg(MoneyType type, ItemQuality quality, int level)
        {
            var rtype = ((int)type - 1) * 10 + (int)quality;
            return Items.FirstOrDefault(o => o.Type == rtype && o.Level == level);
        }
    }
}
