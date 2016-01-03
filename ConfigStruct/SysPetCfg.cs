using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper.EmitInvoker;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 骑宠配置
    /// 编号规则5000301
    /// 5000为骑宠开头
    /// 3为品质
    /// 01为编号
    /// </summary>
    public class SysPetCfg : TableCfg<SysPetCfg>
    {
        /// <summary>
        /// 骨骼id
        /// </summary>
        public int BoneId { get; set; }
        /// <summary>
        /// 骑宠名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 品质
        /// </summary>
        public ItemQuality Quality { get; set; }
        /// <summary>
        /// 元素属性金1木2水3火4土5
        /// </summary>
        public Element Element { get; set; }
        /// <summary>
        /// 武
        /// </summary>
        public int Force { get; set; }
        /// <summary>
        /// 智
        /// </summary>
        public int Intel { get; set; }
        /// <summary>
        /// 统
        /// </summary>
        public int Command { get; set; }
        /// <summary>
        /// 移动速度
        /// </summary>
        //public int MoveSpeed { get; set; }
        /// <summary>
        /// 出售价格
        /// </summary>
        public int SellPrice { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Introduce { get; set; }
        /// <summary>
        /// 需求的武将等级
        /// </summary>
        public int NeedLevel { get; set; }
        /// <summary>
        /// 攻击速度（加到武将身上）
        /// </summary>
       public int AttackSpeed { get; set; }
    }
}
