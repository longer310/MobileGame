using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 竞技场商店物品配置
    /// </summary>
    public class SysPkShopCfg : TableCfg<SysPkShopCfg>
    {
        /// <summary>
        /// 商店类型id
        /// </summary>
        public ShopType ShopType { get; set; }
        /// <summary>
        /// 物品id
        /// </summary>
        public int GoodsId { get; set; }

        /// <summary>
        /// 售价类型
        /// </summary>
        public MoneyType MoneyType { get; set; }

        /// <summary>
        /// 出售的价格
        /// </summary>
        public int SellPrice { get; set; }

        /// <summary>
        /// 出现要求主公的等级
        /// </summary>
        public int NeedLevel { get; set; }

        /// <summary>
        /// 出现的权重
        /// </summary>
        public int ExtractWeights { get; set; }
    }
}
