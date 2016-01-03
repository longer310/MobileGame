using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统大地图城池领地配置
    /// </summary>
    public class SysCityCfg : TableCfg<SysCityCfg>
    {
        /// <summary>
        /// 类型【1：城池，2：领地】
        /// </summary>
        public CityType Type { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 初始等级【领地的话 等级不会变】
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 生产类型[1-4 铜钱、木头、石头、铁矿]
        /// </summary>
        public int Mtype { get; set; }

        /// <summary>
        /// 产量【每小时】
        /// </summary>
        public int Product { get; set; }

        /// <summary>
        /// 1：是NPC,0：系统根据规则匹配玩家
        /// </summary>
        public int IsNpc { get; set; }

        /// <summary>
        /// npcid【2001-2999】
        /// </summary>
        public int NpcId { get; set; }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// 城池图片Id
        /// </summary>
        public int PicId { get; set; }

        /// <summary>
        /// 生产类型
        /// </summary>
        public MoneyType MoneyType { get { return (MoneyType)(Mtype + 1); } }

        /// <summary>
        /// 组Id
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// 防守攻击使用的武将类型
        /// </summary>
        public BattleType BattleType { get; set; }
    }

    /// <summary>
    /// 地标类型
    /// </summary>
    public enum CityType
    {
        /// <summary>
        /// 城池
        /// </summary>
        City = 1,
        /// <summary>
        /// 领地
        /// </summary>
        Domain = 2,
    }

    /// <summary>
    /// 防守攻击使用的武将类型
    /// </summary>
    public enum BattleType
    {
        /// <summary>
        /// 无限制
        /// </summary>
        None = 0,
        /// <summary>
        /// 巾帼之城 女将
        /// </summary>
        Woman = 1,
        /// <summary>
        /// 武力之城
        /// </summary>
        Wu = 2,
        /// <summary>
        /// 智慧之城
        /// </summary>
        Zhi = 3,
        /// <summary>
        /// 统帅之城
        /// </summary>
        Tong = 4,
        /// <summary>
        /// 骑兵之城
        /// </summary>
        Rider = 5,
        /// <summary>
        /// 步兵之城
        /// </summary>
        Infantry = 6,
        /// <summary>
        /// 弓兵之城
        /// </summary>
        Armor = 7,
        /// <summary>
        /// 策士之城
        /// </summary>
        Spell = 8,
        /// <summary>
        /// 机械之城
        /// </summary>
        Machinery = 9,
        /// <summary>
        /// 轻甲之城
        /// </summary>
        LightArmor = 10,
        /// <summary>
        /// 布甲之城
        /// </summary>
        Cloth = 11,
        /// <summary>
        /// 重甲之城
        /// </summary>
        HeavyArmor = 12,
        /// <summary>
        /// 近战之城
        /// </summary>
        Front = 13,
        /// <summary>
        /// 远程之城
        /// </summary>
        After = 14,
    }
}
