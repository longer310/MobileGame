using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统用户等级升级配置
    /// </summary>
    public class SysUserUpgradeCfg : TableCfg<SysUserUpgradeCfg>
    {
        /// <summary>
        /// 升级到下一级需要的经验值
        /// </summary>
        public int NextLvExp { get; set; }

        /// <summary>
        /// 最大的用户等级
        /// </summary>
        public static int MaxLevel
        {
            get { return Items.Max(p => p.Id); }
        }
        /// <summary>
        /// 最大的体力上限
        /// </summary>
        public int MaxSp { get; set; }
        /// <summary>
        /// 解锁的字符串
        /// </summary>
        public string OpenStr { get; set; }
    }
}
