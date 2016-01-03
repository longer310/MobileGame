using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;

namespace MobileGame.tianzi.Entity
{
    #region 大地图活动（出现妃子、武将）
    /// <summary>
    /// 大地图活动（出现妃子、武将）
    /// </summary>
    public class SysBigMapActivity
    {
        /// <summary>
        /// 大地图活动工厂
        /// </summary>
        public interface IBigMapActivityFactory
        {
            /// <summary>
            /// 创建工厂
            /// </summary>
            /// <returns></returns>
            SysBigMapActivity Create();
        }
        #region 属性
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 第几轮【来回循环】
        /// </summary>
        public int Turn { get; set; }
        /// <summary>
        /// 来访的武将/妃子ID
        /// </summary>
        public int VisitorId { get; set; }
        /// <summary>
        /// 停留的时间（小时）
        /// </summary>
        public int StayTime { get; set; }
        /// <summary>
        /// 停留的城池ID
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// 最多拜访次数
        /// </summary>
        public int MaxVisitTimes { get; set; }
        #endregion

        /// <summary>
        /// 从数据库加载
        /// </summary>
        /// <returns></returns>
        private static List<SysBigMapActivity> LoadFromDb()
        {
            var bigMapActivityFactory = new BigMapActivityFactory();
            var dict = new List<SysBigMapActivity>();
            string sql = "select * from SysBigMapActivity;";
            var ds = MySqlHelper.ExecuteDataset(ParamHelper.GameServer, CommandType.Text, sql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var sysBigMapActivity = bigMapActivityFactory.Create();

                try
                {
                    sysBigMapActivity.InternalInit(row);
                    dict.Add(sysBigMapActivity);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Format("ActivityId[{0}]初始化异常", sysBigMapActivity.Id), ex);
                }
            }
            return dict;
        }

        protected virtual void Init()
        {

        }

        private void InternalInit(DataRow row)
        {
            this.Id = Convert.ToInt32(row["Id"]);
            this.Turn = Convert.ToInt32(row["Turn"]);
            this.VisitorId = Convert.ToInt32(row["VisitorId"]);
            this.StayTime = Convert.ToInt32(row["StayTime"]);
            this.CityId = Convert.ToInt32(row["CityId"]);
            this.MaxVisitTimes = Convert.ToInt32(row["MaxVisitTimes"]);
            Init();
        }

        /// <summary>
        /// 大地图活动列表
        /// </summary>
        public static List<SysBigMapActivity> Activitys
        {
            get
            {
                return (List<SysBigMapActivity>)Util.SyncGetOrLoad("Activitys", LoadFromDb);
            }
        }
    }

    public class BigMapActivityFactory : SysBigMapActivity.IBigMapActivityFactory
    {
        public SysBigMapActivity Create()
        {
            return new SysBigMapActivity();
        }
    }
    #endregion
}
