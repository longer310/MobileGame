using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 兑换码配置
    /// </summary>
    public class SysRedCodeCfg : TableCfg<SysRedCodeCfg>
    {
        /// <summary>
        /// 兑换码类型
        /// </summary>
        public RedCodeType Type { get; set; }

        /// <summary>
        /// 兑换码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 使用次数
        /// </summary>
        //public int UseNum { get; set; }

        public void SetUse()
        {
            //var sql = string.Format("UPDATE SysRedCodeCfg SET UseNum=UseNum+1 WHERE `Id`={0};", Id);

            //MySqlHelper.ExecuteNonQuery(ParamHelper.GameServer, CommandType.Text, sql);

            //Util.ClearCacheData(typeof (SysRedCodeCfg).Name);
        }
    }
}
