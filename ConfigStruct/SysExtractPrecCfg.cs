using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统抽取各个品种概率配置
    /// </summary>
    public class SysExtractPrecCfg : TableCfg<SysExtractPrecCfg>
    {
        /// <summary>
        /// 类型id
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// 白色概率
        /// </summary>
        public int White { get; set; }
        /// <summary>
        /// 绿色概率
        /// </summary>
        public int Green { get; set; }
        /// <summary>
        /// 蓝色概率
        /// </summary>
        public int Blue { get; set; }
        /// <summary>
        /// 紫色概率
        /// </summary>
        public int Purple { get; set; }
        /// <summary>
        /// 橙色概率
        /// </summary>
        public int Orange { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Num { get; set; }
        /// <summary>
        /// 组概率
        /// </summary>
        public int TeamPre { get; set; }

        /// <summary>
        /// 抽取类型
        /// </summary>
        public ExtractType Type
        {
            get
            {
                return (ExtractType) (int) (TypeId/100);
            }
        }

        /// <summary>
        /// 物品类型
        /// </summary>
        public ExtractItemType ItemType
        {
            get
            {
                return (ExtractItemType)(int)(TypeId % 100);
            }
        }
    }
}
