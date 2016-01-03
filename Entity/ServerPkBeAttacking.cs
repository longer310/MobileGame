using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 竞技场正在战斗的战役
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class ServerPkBeAttacking : KVEntity
    {
        /// <summary>
        /// NPC武将阵型信息
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<PkBeAttackingItem> Items { get; set; }

        public override void NewObjectInit()
        {
            Items = new List<PkBeAttackingItem>();
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<PkBeAttackingItem>();
            Items.RemoveAll(o => o.AttackEndTimestamp < DateTime.Now.ToUnixTime());
        }
    }

    /// <summary>
    /// 武将阵型信息【位置、武将id、等级、星级】
    /// </summary>
    [ProtoContract]
    public class PkBeAttackingItem
    {
        /// <summary>
        /// Pk对手类型
        /// </summary>
        [ProtoMember(1)]
        public int Type { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        [ProtoMember(2)]
        public int UserId { get; set; }
        /// <summary>
        /// 遭受攻击截止时间戳
        /// </summary>
        [ProtoMember(3)]
        public int AttackEndTimestamp { get; set; }
        /// <summary>
        /// 加入时的排名
        /// </summary>
        [ProtoMember(4)]
        public int Rank { get; set; }
    }
}
