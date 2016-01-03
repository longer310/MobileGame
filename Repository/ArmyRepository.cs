using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Entity;
using MobileGame.tianzi;
using MobileGame.tianzi.ConfigStruct;

namespace MobileGame.tianzi.Repository
{
    #region 13000 获取兵营界面数据
    /// <summary>
    /// 兵营界面信息
    /// </summary>
    public class GetArmyInfoResponse
    {
        
    }
    /// <summary>
    /// 获取兵营界面数据
    /// </summary>
    [GameCode(OpCode = 13000)]
    public class GetArmyInfoRequest : GameHandler
    {
        public override void Process(GameContext context)
        {

        }
    }
    #endregion
}