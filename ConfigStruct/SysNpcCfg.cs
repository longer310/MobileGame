using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// NPC配置
    /// </summary>
    public class SysNpcCfg : TableCfg<SysNpcCfg>
    {
        /// <summary>
        /// NPC名称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// NPC等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 头像id
        /// </summary>
        public int HeadId { get; set; }

        /// <summary>
        /// 战斗力
        /// </summary>
        public int Combat { get; set; }
    }
}
