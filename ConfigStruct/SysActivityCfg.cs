using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using MobileGame.Core;
using MobileGame.Core.Logging;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    public class SysActivityCfg : TableCfg<SysActivityCfg>
    {   
        /// <summary>
        /// 周几配置
        /// </summary>
        public string DaysOfWeek { get; set; }

        /// <summary>
        /// 活动
        /// </summary>
        public ActivityType Type { get; set; }

        /// <summary>
        /// 开始的小时数
        /// </summary>
        public int StartHour { get; set; }

        /// <summary>
        /// 开始的分钟数
        /// </summary>
        public int StartMin { get; set; }

        /// <summary>
        /// 持续的分钟数
        /// </summary>
        public int LastMinutes { get; set; }

        /// <summary>
        /// 活动时间描述
        /// </summary>
        public string TimeDescription { get; set; }

        /// <summary>
        /// 活动要求描述
        /// </summary>
        public string RequireDescription { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否关闭 0 活动开启 1 活动关闭
        /// </summary>
        public int IsClose { get; set; }

        /// <summary>
        /// 活动日期列表
        /// </summary>
        private List<int> DaysOfWeekList
        {
            get
            {
                if (!string.IsNullOrEmpty(DaysOfWeek))
                {
                    return JsonConvert.DeserializeObject<List<int>>(this.DaysOfWeek);
                }
                return new List<int>();
            }
        }

        /// <summary>
        /// 活动是否开始
        /// </summary>
        /// <param name="lastStartTime"></param>
        /// <returns></returns>
        public bool HasActivityStart(DateTime lastStartTime)
        {
            var i = (int)DateTime.Now.DayOfWeek;
            if (DaysOfWeekList.Contains(i))
            {
                //在活动日在内
                var startTime = DateTime.Now.Date.AddHours(StartHour).AddMinutes(StartMin);
                var endTime = startTime.AddMinutes(LastMinutes);

                var now = DateTime.Now;
                if (now >= startTime && now < endTime && now.Date != lastStartTime.Date)
                {
                    return true;
                }
            }
            return false;
        }

        public DateTime GetEndTime()
        {
            var i = (int)DateTime.Now.DayOfWeek;
            if (DaysOfWeekList.Contains(i))
            {
                //在活动日在内
                var startTime = DateTime.Now.Date.AddHours(StartHour).AddMinutes(StartMin);
                var endTime = startTime.AddMinutes(LastMinutes);

                return endTime;
            }
            return DateTime.Now;
        }

        public bool IsStart()
        {
            var i = (int)DateTime.Now.DayOfWeek;
            if (DaysOfWeekList.Contains(i))
            {
                //在活动日在内
                var startTime = DateTime.Now.Date.AddHours(StartHour).AddMinutes(StartMin);
                var endTime = startTime.AddMinutes(LastMinutes);

                var now = DateTime.Now;
                if (now >= startTime && now < endTime)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 活动类型
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// 暗黑军团
        /// </summary>
        Diablo = 1,
    }
}
