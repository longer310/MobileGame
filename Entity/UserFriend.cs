using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 用户好友关系
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserFriend : KVEntity
    {
        /// <summary>
        /// 好友列表
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Friends { get; set; }
        /// <summary>
        /// 申请列表
        /// </summary>
        [ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Begs { get; set; }
        /// <summary>
        /// 仇人列表
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Enemies { get; set; }

        public override void NewObjectInit()
        {
            Friends = new List<int>();
            Begs = new List<int>();
            Enemies = new List<int>();
        }

        public override void LoadInit()
        {
            Friends = Friends ?? new List<int>();
            Begs = Begs ?? new List<int>();
            Enemies = Enemies ?? new List<int>();
        }
    }
}
