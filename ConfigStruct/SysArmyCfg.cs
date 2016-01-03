using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper.EmitInvoker;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 士兵配置
    /// 编号规则8003201
    /// 800为士兵开头
    /// 3为职业
    /// 2为品质
    /// 01为编号
    /// </summary>
    public class SysArmyCfg : TableCfg<SysArmyCfg>
    {
        /// <summary>
        /// 士兵名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 士兵描述
        /// </summary>
        public string Introduce { get; set; }
        /// <summary>
        /// 士兵类型
        /// </summary>
        public ArmyType ArmyType { get; set; }
        /// <summary>
        /// 护甲类型
        /// </summary>
        public DefendType DefendType { get; set; }
        /// <summary>
        /// 元素属性金1木2水3火4土5
        /// </summary>
        public Element Element { get; set; }
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
        public int HpGrowup { get; set; }
        /// <summary>
        /// 基础物攻成长
        /// </summary>
        public int AdGrowup { get; set; }
        /// <summary>
        /// 基础物品防御成长
        /// </summary>
        public int ApGrowup { get; set; }
        /// <summary>
        /// 基础法术攻击成长
        /// </summary>
        public int AdArmGrowup { get; set; }
        /// <summary>
        /// 基础法术防御成长
        /// </summary>
        public int ApArmGrowup { get; set; }
        /// <summary>
        /// 攻击音效
        /// </summary>
        public string AttackSound { get; set; }
    }

    /// <summary>
    /// 兵种系钟
    /// </summary>
    public enum ArmyType
    {
        /// <summary>
        /// 步兵
        /// </summary>
        Infantry = 1,
        /// <summary>
        /// 骑兵
        /// </summary>
        Rider = 2,
        /// <summary>
        /// 弓兵
        /// </summary>
        Armor = 3,
        /// <summary>
        /// 策士
        /// </summary>
        Spell = 4,
        /// <summary>
        /// 机械
        /// </summary>
        Machinery = 5
    }

    /// <summary>
    /// 护甲类型
    /// </summary>
    public enum DefendType
    {
        /// <summary>
        /// 布甲
        /// </summary>
        Cloth = 1,
        /// <summary>
        /// 轻甲
        /// </summary>
        LightArmor = 2,
        /// <summary>
        /// 重甲
        /// </summary>
        HeavyArmor = 3,
    }
    
    /// <summary>
    /// 元素属性，会在一些特定的战场地形有影响
    /// </summary>
    public enum Element
    {
        /// <summary>
        /// 金
        /// </summary>
        Glod = 1,
        /// <summary>
        /// 木
        /// </summary>
        Wood = 2,
        /// <summary>
        /// 水
        /// </summary>
        Water = 3,
        /// <summary>
        /// 火
        /// </summary>
        Fire = 4,
        /// <summary>
        /// 土
        /// </summary>
        Dust = 5,
    }
}
