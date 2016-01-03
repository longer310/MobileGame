using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统剧情任务
    /// </summary>
    public class SysStoryCfg : TableCfg<SysStoryCfg>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 战役的关卡id
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 剧情类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 头像id
        /// </summary>
        public int HeadId { get; set; }

        /// <summary>
        /// 对话
        /// </summary>
        public string Talk { get; set; }

        /// <summary>
        /// 步骤数
        /// </summary>
        public int Step { get; set; }
    }
}
