using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统商店各个品种概率配置
    /// </summary>
    public class SysShopPrecCfg : TableCfg<SysShopPrecCfg>
    {
        /// <summary>
        /// 类型id
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// 白色概率
        /// </summary>
        public int White { get; set; }
        /// <summary>
        /// 绿色概率
        /// </summary>
        public int Green { get; set; }
        /// <summary>
        /// 蓝色概率
        /// </summary>
        public int Blue { get; set; }
        /// <summary>
        /// 紫色概率
        /// </summary>
        public int Purple { get; set; }
        /// <summary>
        /// 橙色概率
        /// </summary>
        public int Orange { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Num { get; set; }
        /// <summary>
        /// 组概率
        /// </summary>
        public int TeamPre { get; set; }

        /// <summary>
        /// 购买所需的货币类型
        /// </summary>
        public ShopType Type
        {
            get
            {
                return (ShopType)(int)(TypeId / 100);
            }
        }

        /// <summary>
        /// 物品类型
        /// </summary>
        public ExtractItemType ItemType
        {
            get
            {
                return (ExtractItemType)(int)(TypeId % 100);
            }
        }
    }

    /// <summary>
    /// 商店类型
    /// </summary>
    public enum ShopType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 洛阳商店
        /// </summary>
        LuoYang = 1,
        /// <summary>
        /// 神秘商店
        /// </summary>
        Mysterious = 2,
        /// <summary>
        /// 西域商店
        /// </summary>
        Western = 3,
        /// <summary>
        /// 竞技场商店
        /// </summary>
        Pk = 4,


        /// <summary>
        /// 金币抽奖
        /// </summary>
        Coin = 21,
        /// <summary>
        /// 金币十抽奖
        /// </summary>
        TenCoin = 22,
        /// <summary>
        /// 妃子抽奖
        /// </summary>
        Concubine = 23,
        /// <summary>
        /// 妃子十抽奖
        /// </summary>
        TenConcubine = 24,
        /// <summary>
        /// 武将抽奖
        /// </summary>
        Hero = 25,
        /// <summary>
        /// 武将十抽奖
        /// </summary>
        TenHero = 26,

        Max = 4,
    }
}
