using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper.EmitInvoker;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 武将技能配置
    /// 编号规则6003001
    /// 600为技能开头
    /// 3为技能类型
    /// 01为编号
    /// </summary>
    public class SysSkillCfg : TableCfg<SysSkillCfg>
    {
        /// <summary>
        /// 技能名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 技能描述列表字符串
        /// </summary>
        public string Introduce { get; set; }

        /// <summary>
        /// 技能描述列表
        /// </summary>
        public List<string> Introduces { get { return JsonConvert.DeserializeObject<List<string>>(Introduce); } }

        /// <summary>
        /// 技能类型
        /// </summary>
        public SkillType Type { get; set; }

        /// <summary>
        /// 技能攻击类型
        /// </summary>
        public SkillAttackType AttackType { get; set; }

        /// <summary>
        /// 开始值[攻击]
        /// </summary>
        public int AttackValue { get; set; }

        /// <summary>
        /// 每级增加值
        /// </summary>
        public int AttackValueGrowup { get; set; }

        /// <summary>
        /// 加成系数，攻击和治疗类型有用
        /// </summary>
        public float Enhance { get; set; }

        /// <summary>
        /// 施法对象
        /// </summary>
        public SkillTargetType Target { get; set; }

        /// <summary>
        /// 技能BUFF类型
        /// </summary>
        public SkillBuffType BuffType { get; set; }

        /// <summary>
        /// BUFF持续时间（秒）
        /// </summary>
        public int BuffTime { get; set; }

        /// <summary>
        /// 开始值[buff]
        /// </summary>
        public float BuffValue { get; set; }

        /// <summary>
        /// buff每级增加值
        /// </summary>
        public float BuffValueGrowup { get; set; }

        /// <summary>
        /// buff加成系数，攻击和治疗类型有用
        /// </summary>
        public float BuffEnhance { get; set; }

        /// <summary>
        /// 受击效果
        /// </summary>
        public SkillEffectType Effects { get; set; }

        /// <summary>
        /// 光环效果类型
        /// </summary>
        public SkillRingsType RingsType { get; set; }
    }

    /// <summary>
    /// 技能类型
    /// </summary>
    public enum SkillType
    {
        /// <summary>
        /// 技能1-大招
        /// </summary>
        Skill1 = 0,
        /// <summary>
        /// 技能2
        /// </summary>
        Skill2 = 1,
        /// <summary>
        /// 技能3
        /// </summary>
        Skill3 = 2,
        /// <summary>
        /// 技能4
        /// </summary>
        Skill4 = 3,
    }

    /// <summary>
    /// 攻击类型
    /// </summary>
    public enum SkillAttackType
    {
        /// <summary>
        /// 物理攻击
        /// </summary>
        Ad = 1,
        /// <summary>
        /// 法术攻击
        /// </summary>
        Ap = 2,
        /// <summary>
        /// 光环 从RingsType字段读取光环类型
        /// </summary>
        Ring = 3,
        /// <summary>
        /// 克制
        /// </summary>
        Kezhi = 4,
        /// <summary>
        /// 光环——对全武将的特定兵种加成
        /// </summary>
        Armytype = 5,
        /// <summary>
        /// 光环——对特定防御（轻甲、重甲）兵种攻击增加
        /// </summary>
        Deftype = 6,
        /// <summary>
        /// 能量攻击(非光环)
        /// </summary>
        EnergyAttack = 7,
    }

    /// <summary>
    /// 施法对象
    /// </summary>
    public enum SkillTargetType
    {
        /// <summary>
        /// 自己
        /// </summary>
        Own = 0,
        /// <summary>
        /// 当前攻击对象
        /// </summary>
        CurTarget = 1,
        /// <summary>
        /// 己方全体
        /// </summary>
        FriendlyAll = 2,
        /// <summary>
        /// 敌方全体
        /// </summary>
        EnemyAll = 3,
        /// <summary>
        /// 附近敌方【一格之差】
        /// </summary>
        Nearby = 4,
        /// <summary>
        /// 直线
        /// </summary>
        StraightLine = 5,
        /// <summary>
        /// 智力最高
        /// </summary>
        HighestIntel = 6,
        /// <summary>
        /// 最虚弱友军
        /// </summary>
        WeakestFriendly = 7,
        /// <summary>
        /// 最虚弱敌军
        /// </summary>
        WeakestEnemy = 8,
        /// <summary>
        /// 目标周围
        /// </summary>
        TargetNearby = 9,
        /// <summary>
        /// 最高统帅
        /// </summary>
        HighestCommand = 10
    }

    /// <summary>
    /// 技能BUFF类型
    /// </summary>
    public enum SkillBuffType
    {
        /// <summary>
        /// 减甲
        /// </summary>
        LowerArm = 1,
        /// <summary>
        /// 眩晕
        /// </summary>
        Dizziness = 2,
        /// <summary>
        /// 魅惑
        /// </summary>
        Addict = 3,
        /// <summary>
        /// 增加攻速
        /// </summary>
        AddAttackSpeed = 4,
        /// <summary>
        /// 沉默
        /// </summary>
        Silence = 5,
        /// <summary>
        /// 增加物攻
        /// </summary>
        AddAd = 6,
        /// <summary>
        /// 增加法强
        /// </summary>
        AddAp = 7,
        /// <summary>
        /// 物理护盾
        /// </summary>
        AdShield = 8,
        /// <summary>
        /// 法术护盾
        /// </summary>
        ApShield = 9,
        /// <summary>
        /// 物理debuff
        /// </summary>
        AdBuff = 10,
        /// <summary>
        /// 法术debuff
        /// </summary>
        ApBuff = 11,
        /// <summary>
        /// 流失生命
        /// </summary>
        OutflowHp = 12,
        /// <summary>
        /// 减伤护盾（抵挡所有伤害）
        /// </summary>
        ReduceHurtShield = 13
    }

    public enum SkillEffectType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 击退
        /// </summary>
        Repel = 1,
        /// <summary>
        /// 击飞
        /// </summary>
        Knock = 2,
    }

    public enum SkillRingsType
    {
        /// <summary>
        /// 增加/减少物攻（光环效果，以目标判断是增加还是减少）
        /// </summary>
        AdAura = 1,
        /// <summary>
        /// 增加/减少法攻
        /// </summary>
        ApAura = 2,
        /// <summary>
        /// 增加/减少护甲
        /// </summary>
        AdArmAura = 3,
        /// <summary>
        /// 增加/减少法抗
        /// </summary>
        ApArmAura = 4,
        /// <summary>
        /// 增加/减少武力
        /// </summary>
        ForceAura = 5,
        /// <summary>
        /// 增加/减少智力
        /// </summary>
        IntelAura = 6,
        /// <summary>
        /// 增加/减少统帅
        /// </summary>
        CommandAura = 7,
        /// <summary>
        /// 增加/减少气势
        /// </summary>
        EnergyAura = 8,
        /// <summary>
        /// 增加/减少攻速
        /// </summary>
        AttackSpeed = 9,
        /// <summary>
        /// 增加/减少物理暴击
        /// </summary>
        AdCrit = 10,
        /// <summary>
        /// 增加/减少法术暴击
        /// </summary>
        ApCrit = 11,
        /// <summary>
        /// 增加/减少吸血
        /// </summary>
        Blood = 12,
        /// <summary>
        /// 增加/减少格挡
        /// </summary>
        Block = 13,
        /// <summary>
        /// 减少护甲&法抗
        /// </summary>
        DownDef = 14,
        /// <summary>
        /// 增加生命回复
        /// </summary>
        UpHpRecover = 15,

        /// <summary>
        /// 最大index
        /// </summary>
        MaxIndex = 15,
    }
}
