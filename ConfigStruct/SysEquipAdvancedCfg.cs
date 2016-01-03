using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{   
    /// <summary>
    /// 系统装备进阶配置
    /// </summary>
    public class SysEquipAdvancedCfg : TableCfg<SysEquipAdvancedCfg>
    {   
        /// <summary>
        /// 进阶等级
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 装备品质
        /// </summary>
        public ItemQuality Quality { get; set; }

        /// <summary>
        /// 装备类型
        /// </summary>
        public EquipType EquipType { get; set; }

        /// <summary>
        /// 进阶需要的资源类型id1 1：元宝，2：铜币，大于7代表道具
        /// </summary>
        public int NeedId1 { get; set; }

        /// <summary>
        /// 进阶需要数量
        /// </summary>
        public int NeedNum1 { get; set; }

        /// <summary>
        /// 进阶需要的资源类型id2 1：元宝，2：铜币，大于7代表道具
        /// </summary>
        public int NeedId2 { get; set; }

        /// <summary>
        /// 进阶需要数量2
        /// </summary>
        public int NeedNum2 { get; set; }

        /// <summary>
        /// 生命成长增加
        /// </summary>
        public int HpGrowUp { get; set; }

        /// <summary>
        /// 物攻成长增加
        /// </summary>
        public int PhysicalAttGrowUp { get; set; }

        /// <summary>
        /// 法功成长增加
        /// </summary>
        public int MagicAttGrowUp { get; set; }

        /// <summary>
        /// 物理防御成长增加
        /// </summary>
        public int PhysicalDefGrowUp { get; set; }

        /// <summary>
        /// 法术防御成长增加
        /// </summary>
        public int MagicDefGrowUp { get; set; }

        /// <summary>
        /// 暴击增加
        /// </summary>
        public int Crit { get; set; }

        /// <summary>
        /// 闪避增加
        /// </summary>
        public int Avo { get; set; }

        /// <summary>
        /// 格挡增加
        /// </summary>
        public int Block { get; set; }

        /// <summary>
        /// 命中增加
        /// </summary>
        public int Hit { get; set; }

        /// <summary>
        /// 韧性增加
        /// </summary>
        public int Toughness { get; set; }

        /// <summary>
        /// 破击增加
        /// </summary>
        public int Wreck { get; set; }
    }
}
