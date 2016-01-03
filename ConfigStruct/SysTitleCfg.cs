using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统爵位配置
    /// </summary>
    public class SysTitleCfg : TableCfg<SysTitleCfg>
    {
        /// <summary>
        /// 爵位等级
        /// </summary>
        public TitleType TitleType
        {
            get { return (TitleType)Id; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string TitleName { get; set; }

        /// <summary>
        /// 初始等级【领地的话 等级不会变】
        /// </summary>
        public int NeedRepute { get; set; }

        /// <summary>
        /// 俸禄
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// 爵位名称颜色对应的品质
        /// </summary>
        public ItemQuality Quality { get; set; }
    }

    public enum TitleType
    {
        /// <summary>
        /// 布衣
        /// </summary>
        None = 0,
        /// <summary>
        /// 伯
        /// </summary>
        Bo = 1,
        /// <summary>
        /// 亭侯
        /// </summary>
        TingHou = 2,
        /// <summary>
        /// 乡侯
        /// </summary>
        XiangHou = 3,
        /// <summary>
        /// 郡公
        /// </summary>
        JunGong = 4,
        /// <summary>
        /// 国公
        /// </summary>
        GuoGong = 5,
        /// <summary>
        /// 郡王
        /// </summary>
        JunWang = 6,
        /// <summary>
        /// 亲王
        /// </summary>
        QinWang = 7,
    }
}
