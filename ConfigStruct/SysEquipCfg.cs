using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper.EmitInvoker;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 装备配置
    /// 编号规则7003201
    /// 700为装备开头
    /// 3为位置类型
    /// 2为品质
    /// 01为编号
    /// </summary>
    public class SysEquipCfg : TableCfg<SysEquipCfg>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Introduce { get; set; }
        /// <summary>
        /// 武器类型
        /// </summary>
        public EquipType Type { get; set; }
        /// <summary>
        /// 使用等级限制
        /// </summary>
        public int NeedLevel { get; set; }
        /// <summary>
        /// 品质
        /// </summary>
        public ItemQuality Quality { get; set; }
        /// <summary>
        /// 基础生命值
        /// </summary>
        public int Hp { get; set; }
        /// <summary>
        /// 基础物攻
        /// </summary>
        public int Ad { get; set; }
        /// <summary>
        /// 基础物攻防御
        /// </summary>
        public int Ap { get; set; }
        /// <summary>
        /// 基础法术攻击
        /// </summary>
        public int AdArm { get; set; }
        /// <summary>
        /// 基础法术防御
        /// </summary>
        public int ApArm { get; set; }
        /// <summary>
        /// 基础生命值成长
        /// </summary>
        public double HpGrowup { get; set; }
        /// <summary>
        /// 基础物攻成长
        /// </summary>
        public double AdGrowup { get; set; }
        /// <summary>
        /// 基础物品防御成长
        /// </summary>
        public double ApGrowup { get; set; }
        /// <summary>
        /// 基础法术攻击成长
        /// </summary>
        public double AdArmGrowup { get; set; }
        /// <summary>
        /// 基础法术防御成长
        /// </summary>
        public double ApArmGrowup { get; set; }
        /// <summary>
        /// 碎片物品id
        /// </summary>
        public int ChipId { get; set; }
        /// <summary>
        /// 出售价格
        /// </summary>
        public int SellPrice { get; set; }
        /// <summary>
        /// 权重
        /// </summary>
        public int ExtractWeights { get; set; }
    }

    #region 装备类型
    /// <summary>
    /// 装备类型
    /// </summary>
    public enum EquipType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 武器
        /// </summary>
        Weapons = 1,
        /// <summary>
        /// 头盔
        /// </summary>
        Clothes = 2,
        /// <summary>
        /// 铠甲
        /// </summary>
        Jewelry = 3,
        /// <summary>
        /// 兵书
        /// </summary>
        Magic = 4,
        /// <summary>
        /// 骑宠
        /// </summary>
        Pet = 5,
    }
    #endregion
}
