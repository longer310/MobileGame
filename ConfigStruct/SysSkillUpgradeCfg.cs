using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统技能等级升级配置
    /// </summary>
    public class SysSkillUpgradeCfg : TableCfg<SysSkillUpgradeCfg>
    {
        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 技能类型
        /// </summary>
        public SkillType Type { get; set; }

        /// <summary>
        /// 所需铜钱
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// 所需木材
        /// </summary>
        public int Wood { get; set; }

        /// <summary>
        /// 所需石头
        /// </summary>
        public int Stone { get; set; }

        /// <summary>
        /// 所需铁矿
        /// </summary>
        public int Iron { get; set; }

        /// <summary>
        /// 最大的用户等级
        /// </summary>
        public static int MaxLevel(int index)
        {
            return Items.Where(o => (int)o.Type == index).Max(o => o.Level);
        }
    }
}
