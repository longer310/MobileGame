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
    /// 系统武将配置
    /// 编号规则9003201
    /// 900为武将开头
    /// 3为武将类型【1武，2智，3统】
    /// 2为武将品质
    /// 01是编号
    /// </summary>
    public class SysHeroCfg : TableCfg<SysHeroCfg>
    {   
        /// <summary>
        /// 武将名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 武将头像图片Id
        /// </summary>
        public int HeadId { get; set; }

        /// <summary>
        /// 武将骨骼动画Id
        /// </summary>
        public int BonesId { get; set; }

        /// <summary>
        /// 武将描述
        /// </summary>
        public string Introduce { get; set; }

        /// <summary>
        /// 武将描述列表
        /// </summary>
        public List<string> Introduces { get { return JsonConvert.DeserializeObject<List<string>>(Introduce); } }

        /// <summary>
        /// 武将品质
        /// </summary>
        public ItemQuality Quality { get; set; }

        /// <summary>
        /// 武将类型
        /// </summary>
        public HeroType Type { get; set; }

        /// <summary>
        /// 兵种id 
        /// </summary>
        public int ArmyId { get; set; }

        /// <summary>
        /// 抽到权重（0为不可被抽到）
        /// </summary>
        public int ExtractWeights { get; set; }

        /// <summary>
        /// 武将碎片Id
        /// </summary>
        public int ChipId { get; set; }

        /// <summary>
        /// 最大星级
        /// </summary>
        public int MaxStarLevel { get; set; }

        /// <summary>
        /// 初始星级
        /// </summary>
        public int InitStarLevel { get; set; }

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
        /// 武成长
        /// </summary>
        public float ForceGrowup { get; set; }

        /// <summary>
        /// 智成长
        /// </summary>
        public float IntelGrowup { get; set; }

        /// <summary>
        /// 统成长
        /// </summary>
        public float CommandGrowup { get; set; }

        /// <summary>
        /// 初始物理伤害
        /// </summary>
        public int Ad { get; set; }

        /// <summary>
        /// 初始法术伤害
        /// </summary>
        public int Ap { get; set; }

        /// <summary>
        /// 初始物理护甲
        /// </summary>
        public int AdArm { get; set; }

        /// <summary>
        /// 初始法术护甲
        /// </summary>
        public int ApArm { get; set; }

        /// <summary>
        /// 生命
        /// </summary>
        public int Hp { get; set; }

        /// <summary>
        /// 物理暴击-伤害加倍
        /// </summary>
        public int AdCrit { get; set; }

        /// <summary>
        /// 法术暴击-伤害加倍
        /// </summary>
        public int ApCrit { get; set; }

        /// <summary>
        /// 物理格挡
        /// </summary>
        public int Block { get; set; }

        /// <summary>
        /// 物理吸血
        /// </summary>
        public int Blood { get; set; }

        /// <summary>
        /// 攻击距离 1表示前排，2表示中排，3表示后排
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// 攻击间隔:毫秒 例如800毫秒，则表示每800毫秒攻击一次
        /// </summary>
        public int AttackSpeed { get; set; }

        /// <summary>
        /// 移动速度:毫秒 例如600毫秒，则表示每移动一格需要600毫秒
        /// </summary>
        public int MoveSpeed { get; set; }

        /// <summary>
        /// 生命回复-表示每回合过关回复的生命值
        /// </summary>
        public int HpRecovery { get; set; }

        /// <summary>
        /// 气势回复-表示没回合过关回复的气势值
        /// </summary>
        public int EnergyRecovery { get; set; }

        /// <summary>
        /// 攻击方式12112
        /// 1代表普通攻击；2第一个技能；3第二个技能
        /// </summary>
        public int AttackMode { get; set; }

        /// <summary>
        /// 技能id列表
        /// </summary>
        public string SkillIds { get; set; }

        /// <summary>
        /// 技能id列表
        /// </summary>
        public List<int> SkillIdList { get { return JsonConvert.DeserializeObject<List<int>>(SkillIds); } }

        /// <summary>
        /// 移动距离
        /// </summary>
        public int MoveRange { get; set; }

        /// <summary>
        /// 是否可以碎片招募
        /// </summary>
        public int Recruit { get; set; }

        /// <summary>
        /// 拜访需要的声望——改为招募的声望
        /// </summary>
        public int VisitNeedRepute { get; set; }

        /// <summary>
        /// 性别（0：无性别，1：男性，2：女性）
        /// </summary>
        public GenderType Gender { get; set; }

        /// <summary>
        /// 音效
        /// </summary>
        public string SoundAtk { get; set; }

        /// <summary>
        /// 招募时对话
        /// </summary>
        public string Talk { get; set; }

        /// <summary>
        /// 技能1音效
        /// </summary>
        public string SoundSkill1 { get; set; }

        /// <summary>
        /// 技能2音效
        /// </summary>
        public string SoundSkill2 { get; set; }
    }

    /// <summary>
    /// 性别类型
    /// </summary>
    public enum GenderType
    {
        /// <summary>
        /// 无性别
        /// </summary>
        None = 0,
        /// <summary>
        /// 男性
        /// </summary>
        Male = 1,
        /// <summary>
        /// 女性
        /// </summary>
        Female = 2,
    }

    /// <summary>
    /// 系统武将升级配置
    /// </summary>
    public class SysHeroUpgradeCfg : TableCfg<SysHeroUpgradeCfg>
    {   
        /// <summary>
        /// 升级所要经验
        /// </summary>
        public int NextLvExp { get; set; }
    }

    /// <summary>
    /// 攻击类型
    /// </summary>
    public enum AttackType
    {
        /// <summary>
        /// 物理系
        /// </summary>
        Physical = 0,
        /// <summary>
        /// 法术系
        /// </summary>
        Power = 1
    }

    /// <summary>
    /// 英雄专题
    /// </summary>
    public enum HeroStateType
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Free = 0,
        /// <summary>
        /// 已上阵
        /// </summary>
        InBattle = 1
    }

    /// <summary>
    /// 位置类型
    /// </summary>
    public enum SiteType
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 0,
        /// <summary>
        /// 前排
        /// </summary>
        Front = 100,
        /// <summary>
        /// 中排
        /// </summary>
        Cnter = 200,
        /// <summary>
        /// 后排
        /// </summary>
        After = 300
    }

    /// <summary>
    /// 武将类型
    /// </summary>
    public enum HeroType
    {
        /// <summary>
        /// 武
        /// </summary>
        Wu = 1,
        /// <summary>
        /// 智
        /// </summary>
        Zhi = 2,
        /// <summary>
        /// 统
        /// </summary>
        Tong = 3,
        /// <summary>
        /// npc
        /// </summary>
        Npc = 4,
    }
}
