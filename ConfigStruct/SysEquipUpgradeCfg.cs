using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统装备升级配置
    /// AB
    /// A表示装备的位置
    /// B表示品质
    /// </summary>
    public class SysEquipUpgradeCfg : TableCfg<SysEquipUpgradeCfg>
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

        /// <summary>
        /// AB表示 A表示装备的位置
        /// </summary>
        public EquipType EquipType
        {
            get
            {
                return (EquipType)(Id / 10);
            }
        }
        /// <summary>
        /// AB表示 B表示品质
        /// </summary>
        public ItemQuality Quality
        {
            get
            {
                return (ItemQuality)(Id % 10);
            }
        }
    }
}
