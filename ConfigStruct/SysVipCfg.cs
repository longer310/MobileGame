using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统VIP配置
    /// </summary>
    public class SysVipCfg : TableCfg<SysVipCfg>
    {
        /// <summary>
        /// vip等级
        /// </summary>
        public int VipLevel { get; set; }

        /// <summary>
        /// 充值总数
        /// </summary>
        public int Recharge { get; set; }

        /// <summary>
        /// 竞技场每天购买次数
        /// </summary>
        public int PkBuyNum { get; set; }

        /// <summary>
        /// 副本每天购买次数
        /// </summary>
        public int LevelBuyNum { get; set; }

        /// <summary>
        /// 体力每天购买次数
        /// </summary>
        public int SpBuyNum { get; set; }

        /// <summary>
        /// 重置寻访次数
        /// </summary>
        public int SearchBuyNum { get; set; }

        /// <summary>
        /// 采花次数个数
        /// </summary>
        public int FlowersNum { get; set; }

        /// <summary>
        /// 重置内政次数
        /// </summary>
        public int InternalAffairsBuyNum { get; set; }

        /// <summary>
        /// 洛阳商人刷新次数
        /// </summary>
        public int LuoYangRefreshNum { get; set; }

        /// <summary>
        /// 神秘商人刷新次数
        /// </summary>
        public int MysteriousRefreshNum { get; set; }

        /// <summary>
        /// 西域商人刷新次数
        /// </summary>
        public int WesternRefreshNum { get; set; }

        /// <summary>
        /// 竞技场商人刷新次数
        /// </summary>
        public int PkRefreshNum { get; set; }

        /// <summary>
        /// 强征次数
        /// </summary>
        public int StrGainDomainResNum { get; set; }

        /// <summary>
        /// 解锁描述
        /// </summary>
        public string UnLockDescription { get; set; }

        /// <summary>
        /// 翻牌每天重置次数
        /// </summary>
        public int FlopBuyNum { get; set; }
    }
}
