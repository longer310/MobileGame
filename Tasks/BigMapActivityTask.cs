using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using MobileGame.Core;
using MobileGame.Core.Dapper;
using System.Data;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;

namespace MobileGame.tianzi.Tasks
{
    /// <summary>
    /// 大地图活动任务（每天五点刷新）
    /// </summary>
    class BigMapActivityTask : IntervalTask
    {
        public BigMapActivityTask()
        {
            int dueTime, period;
            dueTime = period = ParamHelper.Get<int>("BigMapActivityTaskTick", 5) * 3600 * 1000;
            base.DueTime = dueTime;
            base.Period = period;
        }

        public override void Start()
        {
            base.Start();

            var midnight = DateTime.Now.AddDays(1).Date;
            var midnightts = (int)(midnight.Subtract(DateTime.Now).TotalMilliseconds);
            Timer.Change(midnightts, ParamHelper.Get<int>("BigMapActivityTaskTick", 5) * 3600 * 1000);
        }

        protected override void DoWork(object state)
        {
            try
            {
                Utility.RefreshMapCityActivity();
            }
            catch (Exception exception)
            {
                Logger.ErrorFormat("msg:{0}", exception.Message);
            }
        }
    }
}
