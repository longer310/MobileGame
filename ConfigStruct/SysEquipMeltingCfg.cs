using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统装备熔炼配置
    /// </summary>
    public class SysEquipMeltingCfg : TableCfg<SysEquipMeltingCfg>
    {
        /// <summary>
        /// 熔炼返还的道具id
        /// </summary>
        public int ToolId { get; set; }

        /// <summary>
        /// 熔炼返还的道具数量
        /// </summary>
        public int ToolNum { get; set; }

        /// <summary>
        /// AABB表示 AA表示装备的位置
        /// </summary>
        public EquipType EquipType
        {
            get
            {
                return (EquipType)(Id / 100);
            }
        }
        /// <summary>
        /// AABB表示 BB表示品质
        /// </summary>
        public ItemQuality Quality
        {
            get
            {
                return (ItemQuality)(Id % 100);
            }
        }
    }
}
