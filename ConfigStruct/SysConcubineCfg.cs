using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统妃子配置
    /// </summary>
    public class SysConcubineCfg : TableCfg<SysConcubineCfg>
    {
        /// <summary>
        /// 妃子名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 妃子品质
        /// </summary>
        public ItemQuality Quality { get; set; }

        /// <summary>
        /// 抽到权重（0为不可被抽到）
        /// </summary>
        public int ExtractWeights { get { return 1; } }

        /// <summary>
        /// 元素属性金1木2水3火4土5
        /// </summary>
        public Element Element { get; set; }

        /// <summary>
        /// 特长id
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 特长类型
        /// </summary>
        public MoneyType MoneyType { get { return (MoneyType)(Type + 1); } }

        /// <summary>
        /// 特长所增加的百分比
        /// </summary>
        public int Growth { get; set; }

        /// <summary>
        /// 初始生产速度
        /// </summary>
        public int InitProduct { get; set; }

        /// <summary>
        /// 生产周期（分钟）
        /// </summary>
        public int ProductPeriod { get; set; }

        /// <summary>
        /// 天赋列表字符串
        /// </summary>
        public string Talents { get; set; }

        /// <summary>
        /// 天赋列表
        /// </summary>
        public List<int> TalentList { get { return JsonConvert.DeserializeObject<List<int>>(Talents); } }

        /// <summary>
        /// 妃子简介
        /// </summary>
        public string Introduce { get; set; }

        /// <summary>
        /// 妃子详细信息
        /// </summary>
        //public string Detail { get; set; }

        /// <summary>
        /// 妃子碎片Id
        /// </summary>
        public int ChipId { get { return (int)ToolType.ConcubineChip + Id; } }

        /// <summary>
        /// 妃子图片名称
        /// </summary>
        public string PicName { get; set; }

        /// <summary>
        /// 拜访需要的魅力
        /// </summary>
        public int VisitNeedCharm { get; set; }

        /// <summary>
        /// 招募时对话
        /// </summary>
        public string Talk { get; set; }

        /// <summary>
        /// 妃子详细信息
        /// </summary>
        //public string BoneId { get; set; }

        /// <summary>
        /// 合成需要碎片数量
        /// </summary>
        //public int NeedChip { get; set; }
    }
}
