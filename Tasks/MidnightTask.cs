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
    /// 每天凌晨12点时候开始执行的任务
    /// </summary>
    class MidnightTask : IntervalTask
    {
        public MidnightTask()
        {
            int dueTime, period;
            dueTime = period = ParamHelper.Get<int>("MidnightTaskTick", 24) * 3600 * 1000;
            base.DueTime = dueTime;
            base.Period = period;
        }

        public override void Start()
        {
            base.Start();

            var midnight = DateTime.Now.AddDays(1).Date;
            var midnightts = (int)(midnight.Subtract(DateTime.Now).TotalMilliseconds);
            Timer.Change(midnightts, 24 * 3600 * 1000);
        }

        protected override void DoWork(object state)
        {
            Util.RetryUntilTrue(() =>
            {
                Storage.BeginTransaction();

                var dict = Storage.Hashes.GetAll<string>("TodayPkRank");
                if (dict.Count == 0) return true;
                var tmpDict = new Dictionary<string, object>();

                foreach (var kv in dict)
                {
                    tmpDict[kv.Key] = kv.Value;
                }

                Storage.Hashes.Set("OriPkRank", tmpDict, true);

                return Storage.Commit();
            });

            try
            {
                Compute(DateTime.Now);
            }
            catch (Exception exception)
            {
                Logger.ErrorFormat("msg:{0}", exception.Message);
            }

            try
            {
                //添加log表
                CheckLogTable();
            }
            catch (Exception exception)
            {
                Logger.ErrorFormat("msg:{0}", exception.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="now">当前时间</param>
        /// <param name="Day">计算几日留存率</param>
        /// <returns></returns>
        internal static string GetUnionString(DateTime now, int Day)
        {
            var dateStart = now.AddDays(-Day);
            var dateEnd = now.AddDays(-1);
            string result = "";
            var strtmp = "select * from LoginLog{0}";
            var datestrStart = dateStart.ToString("yyyyMM");
            var datestrend = dateEnd.ToString("yyyyMM");
            result = string.Format(strtmp, datestrStart);
            while (datestrStart.CompareTo(datestrend) != 0)
            {
                dateStart = dateStart.AddMonths(1);
                datestrStart = dateStart.ToString("yyyyMM");
                result += " union all ";
                result += string.Format(strtmp, datestrStart);
            }
            return result;
        }

        public void TestCompute()
        {
            Compute(DateTime.Now);
        }

        internal static void Compute(DateTime now)
        {
            // 留存率算法修改,
            // 3日留存率 3日后持续两天内 当时注册数的登录情况
            // 后面以此类推~~

            // 持续2天
            int stillDay = 2;

            #region 次日留存率计算
            var sql1 = string.Format("select Id from UserRole where RegTime between '{0}' and '{1}'", now.AddDays(-2).Date, now.AddDays(-1).Date);
            var sql2 = string.Format("select count(distinct(UserId)) from LoginLog{2} where LoginTime between '{0}' and '{1}' and find_in_set(`UserId`,@ids);", now.AddDays(-1).Date, now.Date, now.AddDays(-1).ToString("yyyyMM"));
            Console.WriteLine(sql1);
            Console.WriteLine(sql2);
            double nextDayKeepBack = 0, next3DayKeepBack = 0, next7DayKeepBack = 0, next15DayKeepBack = 0, next30DayKeepBack = 0;
            int regnum = 0;
            using (var conn = Util.ObtainConn(ParamHelper.GameServer))
            using (var conn2 = Util.ObtainConn(ParamHelper.GameLog))
            {
                var reguidlist = conn.Query<int>(sql1).ToList();
                regnum = reguidlist.Count;
                if (regnum > 0)
                {
                    var lognum =(int)conn2.Query<Int64>(sql2, new {ids = String.Join(",", reguidlist.ToArray())}).FirstOrDefault();
                    nextDayKeepBack = (double)lognum / reguidlist.Count;
                }
            }
            #endregion

            #region 3日留存率计算
            var sql3 = string.Format("select Id from UserRole where RegTime between '{0}' and '{1}'", now.AddDays(-4 - stillDay).Date, now.AddDays(-3 - stillDay).Date);
            var sql4 = string.Format("select count(distinct(UserId)) from ({2})as T1 where LoginTime between '{0}' and '{1}' and find_in_set(T1.`UserId`,@ids)",
                now.AddDays(-stillDay).Date, now.Date, GetUnionString(now, stillDay));
            Console.WriteLine(sql3);
            Console.WriteLine(sql4);
            using (var conn = Util.ObtainConn(ParamHelper.GameServer))
            using (var conn2 = Util.ObtainConn(ParamHelper.GameLog))
            {
                var reguidlist = conn.Query<int>(sql3).ToList();
                if (reguidlist.Count > 0)
                {
                    var lognum = (int)conn2.Query<Int64>(sql4, new { ids = String.Join(",", reguidlist.ToArray()) }).FirstOrDefault();
                    next3DayKeepBack = (double)lognum / reguidlist.Count;
                }
            }
            #endregion

            #region 7日留存率计算
            var sql5 = string.Format("select Id from UserRole where RegTime between '{0}' and '{1}'", now.AddDays(-8 - stillDay).Date, now.AddDays(-7 - stillDay).Date);
            var sql6 = string.Format("select count(distinct(UserId)) from ({2})as T1 where LoginTime between '{0}' and '{1}' and find_in_set(T1.`UserId`,@ids)", now.AddDays(-stillDay).Date, now.Date, GetUnionString(now, stillDay));
            Console.WriteLine(sql5);
            Console.WriteLine(sql6);
            using (var conn = Util.ObtainConn(ParamHelper.GameServer))
            using (var conn2 = Util.ObtainConn(ParamHelper.GameLog))
            {
                var reguidlist = conn.Query<int>(sql5).ToList();
                if (reguidlist.Count > 0)
                {
                    var lognum = (int)conn2.Query<Int64>(sql6, new { ids = String.Join(",", reguidlist.ToArray()) }).FirstOrDefault();
                    next7DayKeepBack = (double)lognum / reguidlist.Count;
                }
            }
            #endregion

            #region 15日留存率计算
            var sql7 = string.Format("select Id from UserRole where RegTime between '{0}' and '{1}'", now.AddDays(-16 - stillDay).Date, now.AddDays(-15 - stillDay).Date);
            var sql8 = string.Format("select count(distinct(UserId)) from ({2})as T1 where LoginTime between '{0}' and '{1}' and find_in_set(T1.`UserId`,@ids)", now.AddDays(-stillDay).Date, now.Date, GetUnionString(now, stillDay));
            Console.WriteLine(sql7);
            Console.WriteLine(sql8);
            using (var conn = Util.ObtainConn(ParamHelper.GameServer))
            using (var conn2 = Util.ObtainConn(ParamHelper.GameLog))
            {
                var reguidlist = conn.Query<int>(sql7).ToList();
                if (reguidlist.Count > 0)
                {
                    var lognum = (int)conn2.Query<Int64>(sql8, new { ids = String.Join(",", reguidlist.ToArray()) }).FirstOrDefault();
                    next15DayKeepBack = (double)lognum / reguidlist.Count;
                }
            }

            #endregion

            #region 30日留存率计算
            var sql9 = string.Format("select Id from UserRole where RegTime between '{0}' and '{1}'", now.AddDays(-31 - stillDay).Date, now.AddDays(-30 - stillDay).Date);
            var sql10 = string.Format("select count(distinct(UserId)) from ({2}) as T1 where LoginTime between '{0}' and '{1}' and find_in_set(T1.`UserId`,@ids)", now.AddDays(-stillDay).Date, now.Date, GetUnionString(now, stillDay));
            Console.WriteLine(sql9);
            Console.WriteLine(sql10);
            using (var conn = Util.ObtainConn(ParamHelper.GameServer))
            using (var conn2 = Util.ObtainConn(ParamHelper.GameLog))
            {
                var reguidlist = conn.Query<int>(sql9).ToList();
                if (reguidlist.Count > 0)
                {
                    var lognum = (int)conn2.Query<Int64>(sql10, new { ids = String.Join(",", reguidlist.ToArray()) }).FirstOrDefault();
                    next30DayKeepBack = (double)lognum / reguidlist.Count;
                }
            }

            #endregion

            var sql = string.Format("insert UserKeepBack values('{0}',{1},{2},{3},{4},{5},{6});", now.AddDays(-1).Date, regnum, nextDayKeepBack, 0, 0, 0, 0);
            sql += string.Format("update UserKeepBack set Next3Day={1} where DateCode='{0}';", now.AddDays(-4 - stillDay).Date, next3DayKeepBack);
            sql += string.Format("update UserKeepBack set Next7Day={1} where DateCode='{0}';", now.AddDays(-8 - stillDay).Date, next7DayKeepBack);
            sql += string.Format("update UserKeepBack set Next15Day={1} where DateCode='{0}';", now.AddDays(-16 - stillDay).Date, next15DayKeepBack);
            sql += string.Format("update UserKeepBack set Next30Day={1} where DateCode='{0}';", now.AddDays(-31 - stillDay).Date, next30DayKeepBack);

            GameLogItem logItem = new GameLogItem();
            logItem.S1 = sql;
            //先记录sql语句防止出错
            GameLogManager.CommonLog((int)SpecialLogType.KeepBackLog, 0, 0, logItem);

            int num = 0;
            using (var conn = Util.ObtainConn(ParamHelper.GameServer))
            {
                int result = conn.Execute(sql, null, null, 60);
                while (result < 0 && num < 3)
                {//执行失败 重新执行 最多重新执行三次
                    Thread.Sleep(20 * 1000);
                    result = conn.Execute(sql, null, null, 60);
                    num++;
                }
            }
        }

        /// <summary>
        /// 检查是否跳跃了月份
        /// </summary>
        internal static void CheckLogTable()
        {
            var fMonth = DateTime.Now.AddDays(-1).Month;
            var nowMonth = DateTime.Now.Month;
            var lMonth = DateTime.Now.AddDays(1).Month;

            var cMonth = 0;
            var date = DateTime.Now;
            if (fMonth != nowMonth)
            {
                cMonth = nowMonth;
                date = DateTime.Now;
            }
            else if (nowMonth != lMonth)
            {
                cMonth = lMonth;
                date = DateTime.Now.AddDays(1);
            }

            if (cMonth == 0) return;
            var dateStr = date.ToString("yyyyMM");

            Utility.AddLogTable(dateStr);
        }
    }
}
