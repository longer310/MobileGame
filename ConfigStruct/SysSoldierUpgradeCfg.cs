using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 士兵升级配置
    /// </summary>
    public class SysSoldierUpgradeCfg : TableCfg<SysSoldierUpgradeCfg>
    {
        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 所需铜钱
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// 所需木材
        /// </summary>
        public int Wood { get; set; }

        /// <summary>
        /// 所需石头
        /// </summary>
        public int Stone { get; set; }

        /// <summary>
        /// 所需铁矿
        /// </summary>
        public int Iron { get; set; }
    }
}
