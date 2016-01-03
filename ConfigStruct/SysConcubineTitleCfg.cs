using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统妃子封号配置
    /// </summary>
    public class SysConcubineTitleCfg : TableCfg<SysConcubineTitleCfg>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 需要主公的等级
        /// </summary>
        public int NeedLevel { get; set; }
        /// <summary>
        /// 增加的产量
        /// </summary>
        public int AddProduct { get; set; }
        /// <summary>
        /// 升级所需好感度
        /// </summary>
        public int NeedFavor { get; set; }
        /// <summary>
        /// 晋封需要的铜钱
        /// </summary>
        public int NeedCoin { get; set; }
        /// <summary>
        /// 晋封需要的木材
        /// </summary>
        public int NeedWood { get; set; }
        /// <summary>
        /// 晋封需要的石头
        /// </summary>
        public int NeedStone { get; set; }
        /// <summary>
        /// 晋封需要的铁矿
        /// </summary>
        public int NeedIron { get; set; }
        /// <summary>
        /// 晋封需要的时间（分钟）
        /// </summary>
        public int NeedTime { get; set; }
    }
}
