using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper.EmitInvoker;

namespace MobileGame.tianzi.ConfigStruct
{
    public class SysTreeCfg : TableCfg<SysTreeCfg>
    {   
        /// <summary>
        /// 下一级需要的经验
        /// </summary>
        public int NeedExp { get; set; }
    }
}
