using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 战役回合
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class BattleRound : KVEntity
    {
        /// <summary>
        /// 第几回合
        /// </summary>
        [Tag(1)]
        [ProtoMember(1)]
        public int Round { get; set; }

        [Tag(2)]
        [ProtoMember(2)]
        public string AttHeroId { get; set; }
        /// <summary>
        /// 攻击类型【0：普通攻击，1：大招，2：第二仅能，3：第三技能】
        /// </summary>
        [Tag(3)]
        [ProtoMember(3)]
        public int AttackType { get; set; }

        /// <summary>
        /// 经过的位置ID列表
        /// </summary>
        [Tag(4)]
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> PathList { get; set; }

        /// <summary>
        /// 目标方效果列表
        /// </summary>
        [Tag(5)]
        [ProtoMember(5), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<HeroEffectItem> TarEffectItems { get; set; }

        /// <summary>
        /// 攻击后发生的后续效果列表
        /// </summary>
        [Tag(6)]
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<HeroAEffectItem> AEffectItems { get; set; }
        /// <summary>
        /// 吸血
        /// </summary>
        [Tag(7)]
        [ProtoMember(7)]
        public int DrinkBlood { get; set; }

        public override void NewObjectInit()
        {
            PathList = new List<int>();
            TarEffectItems = new List<HeroEffectItem>();
            AEffectItems = new List<HeroAEffectItem>();
        }
    }

    /// <summary>
    /// 效果项
    /// </summary>
    [ProtoContract]
    public class HeroEffectItem
    {
        /// <summary>
        /// 效果类型【0：正常效果【受到普通攻击/技能】，1：受到暴击，2：命中BUF，3：被反击】
        /// </summary>
        [Tag(1)]
        [ProtoMember(1)]
        public int Type { get; set; }
        /// <summary>
        /// 影响到的武将id[攻击方：a+ID,防守方：d+ID]
        /// </summary>
        [Tag(2)]
        [ProtoMember(2)]
        public string HeroId { get; set; }
        /// <summary>
        /// 影响的血量【正负区分，加血扣血】
        /// </summary>
        [Tag(3)]
        [ProtoMember(3)]
        public int Blood { get; set; }
        /// <summary>
        /// 影响的血量【正负区分，加血扣血】
        /// </summary>
        [Tag(4)]
        [ProtoMember(4)]
        public int BackBlood { get; set; }
    }

    /// <summary>
    /// 正常攻击后续效果【反击等】的项
    /// </summary>
    [ProtoContract]
    public class HeroAEffectItem
    {
        /// <summary>
        /// 影响到的武将id[攻击方：a+ID,防守方：d+ID]
        /// </summary>
        [Tag(1)]
        [ProtoMember(1)]
        public string FjHeroId { get; set; }
        /// <summary>
        /// 效果的列表
        /// </summary>
        [Tag(2)]
        [ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<HeroEffectItem> FjEffectItems { get; set; }
    }
}
