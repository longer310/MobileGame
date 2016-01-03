using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统购买次数价格配置
    /// </summary>
    public class SysBuyNumCfg : TableCfg<SysBuyNumCfg>
    {
        /// <summary>
        /// 购买竞技场次数价格
        /// </summary>
        public int BuyPkNumMoney { get; set; }
        /// <summary>
        /// 购买副本关卡次数价格
        /// </summary>
        public int BuyLevelNumMoney { get; set; }
        /// <summary>
        /// 购买体力次数价格
        /// </summary>
        public int BuySpNumMoney { get; set; }
        /// <summary>
        /// 刷新洛阳商人物品列表
        /// </summary>
        public int RefreshLuoYangMoney { get; set; }
        /// <summary>
        /// 刷新神秘商人物品列表
        /// </summary>
        public int RefreshMysteriousMoney { get; set; }
        /// <summary>
        /// 刷新西域商人物品列表
        /// </summary>
        public int RefreshWesternMoney { get; set; }
        /// <summary>
        /// 竞技场商店
        /// </summary>
        public int RefreshPkMoney { get; set; }
        /// <summary>
        /// 强征一次领地资源
        /// </summary>
        public int BuyStrGainMoney { get; set; }
        /// <summary>
        /// 重置寻访次数
        /// </summary>
        public int BuySearchNumMoney { get; set; }
        /// <summary>
        /// 重置内政次数
        /// </summary>
        public int BuyInternalAffairsNumMoney { get; set; }

        /// <summary>
        /// 重置翻牌次数所需元宝
        /// </summary>
        public int BuyFlopNumMoney { get; set; }

        /// <summary>
        ///募兵购买元宝
        /// </summary>
        public int MubinMoney { get; set; }

        /// <summary>
        /// 采花购买元宝
        /// </summary>
        public int DeflowerMoney { get; set; }
    }
}
