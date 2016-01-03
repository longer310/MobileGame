using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Repository;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{

    /// <summary>
    /// 采花类型
    /// </summary>
    public enum DeflowerType
    {   
        /// <summary>
        /// 你采别人花
        /// </summary>
        DeflowerOther = 1,
        /// <summary>
        /// 别人采你的花
        /// </summary>
        OtherDeflower = 2
    }

    [ProtoContract]
    class DeflowerRecord
    {   
        /// <summary>
        /// 采花类型
        /// </summary>
        [ProtoMember(1)]
        public DeflowerType Type { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [ProtoMember(2)]
        public int UserId { get; set; }

        /// <summary>
        /// 妃子列表
        /// </summary>
        [ProtoMember(3)]
        public List<int> ConcubineIds { get; set; }


        /// <summary>
        /// 获得的魅力
        /// </summary>
        [ProtoMember(4)]
        public int Valueget { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        [ProtoMember(5)]
        public DateTime Time { get; set; }

        public DeflowerRecord()
        {
            ConcubineIds = new List<int>();
            Time = DateTime.Now;
        }

    }

    /// <summary>
    /// 用户采花
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    class UserDeflower:KVEntity
    {   
        /// <summary>
        /// 购买次数
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue BuyNum { get; set; }
        
        /// <summary>
        /// 今天使用次数
        /// </summary>
        [ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public  DayZeorValue UseNum { get; set; }

        
        /// <summary>
        /// 下一次采花时间
        /// </summary>
        [ProtoMember(3)]
        public DateTime NextDeflowerTime { get; set; }


        /// <summary>
        /// 待采花的列表采花列表
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> DeflowerUserlist { get; set; }


        /// <summary>
        /// 采花记录列表
        /// </summary>
        [ProtoMember(5), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<DeflowerRecord> DeflowerList { get; set; }

        /// <summary>
        /// 临时采花用户
        /// </summary>
        [ProtoMember(6)]
        public int TempDeflowerUser { get; set; }

        /// <summary>
        /// 临时采花成功的列表
        /// </summary>
        [ProtoMember(7), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> TempConcubineList { get; set; }

        /// <summary>
        /// 魅力获得
        /// </summary>
        [ProtoMember(8)]
        public int TempValueget { get; set; }

        //TOOD 临时记录

        public override void NewObjectInit()
        {
            NextDeflowerTime = DateTime.Now;
            DeflowerUserlist = new List<int>();
            TempConcubineList = new List<int>();
            DeflowerList = new List<DeflowerRecord>();
        }

        public override void LoadInit()
        {
            if (DeflowerUserlist == null)
            {
                DeflowerUserlist = new List<int>();
            }
            if (NextDeflowerTime == DateTime.MinValue)
            {
                NextDeflowerTime = DateTime.Now;
            }
            if (TempConcubineList == null)
            {
                TempConcubineList = new List<int>();
            }
            if (DeflowerList == null)
            {
                DeflowerList = new List<DeflowerRecord>();
            }
            if (DeflowerList.Count > ConfigHelper.DeflowCfgData.ListCount)
            {
                DeflowerList.RemoveAt(DeflowerList.Count - 1);
            }

        }

        /// <summary>
        /// 获得采花列表
        /// </summary>
        /// <param name="needflesh"></param>
        /// <returns></returns>
        public List<int> GetDeflowerList(bool needflesh = false)
        {
            if (DeflowerUserlist.Count == 0|| needflesh)
            {
                //TODO 抽取上下等级为5的 6个列表 
                var userrole = DataStorage.Current.Load<UserRole>(Id, true);
                var list = new List<int>();
                
                //TODO 预先取其中30个然后，在其中随机取
                var minlevel = userrole.Level - 5;
                var maxlevel = userrole.Level + 5;
                minlevel = minlevel > 3 ? minlevel : 1;
                for (int i = minlevel; i <= maxlevel; i++)
                {
                    list.AddRange(Utility.GetLevelUserIdList(i, 500));
                }
                list.Remove(Id);
                var result = new List<int>();
                if (list.Count > 0)
                {
                    if (list.Count <= ConfigHelper.DeflowCfgData.ListCount)
                    {
                        result.AddRange(list);
                    }
                    else
                    {

                        while (result.Count < ConfigHelper.DeflowCfgData.ListCount)
                        {
                            var rd = new Random(DateTime.Now.Millisecond);
                            int index = rd.Next(0, list.Count - 1);
                            var value = list[index];
                            if (!result.Contains(value) && value != Id)
                            {
                                result.Add(value);
                                list.RemoveAt(index);
                            }

                            if (result.Count == list.Count)
                                break;
                        }
                    }
                }

                DeflowerUserlist = result;

            }
            return DeflowerUserlist;
        }






    }


    
}
