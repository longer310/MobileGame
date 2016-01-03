using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统充值配置
    /// </summary>
    public class SysRechargeCfg : TableCfg<SysRechargeCfg>
    {
        /// <summary>
        /// 手机平台
        /// </summary>
        public MobileType Type { get; set; }

        /// <summary>
        /// 人民币【真实货币】
        /// </summary>
        public double Rmb { get; set; }

        /// <summary>
        /// 元宝【游戏货币】
        /// </summary>
        public int Money { get; set; }

        /// <summary>
        /// 充值赠送的元宝数
        /// </summary>
        public int GiveMoney { get; set; }

        /// <summary>
        /// 充值唯一ID
        /// </summary>
        public string Id { get; set; }
    }
}
