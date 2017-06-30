using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RateEngine.DataObjects
{
    public class CaseCondition
    {
        #region 属性

        public TimeRange ParkTimeRange{ get; set; }
        public TimeRegion TimeRegion { get; set; }
        public bool IsOutDay { get; set; }
        public string WorkDay { get; set; }

        #endregion

        public CaseCondition()
        {
            if (ParkTimeRange == null)
            {
                ParkTimeRange = new TimeRange();
            }
            if (TimeRegion == null)
            {
                TimeRegion = new TimeRegion();
            }
        }

        public bool InTimeRegion(DateTime startTime,DateTime endTime)
        {
            string sStartDate = startTime.ToShortDateString();
            string sEndDate = endTime.ToShortDateString();
            if (TimeRegion.LeftTime != null && 
                    TimeRegion.MiddleTime == null &&
                    TimeRegion.RightTime != null)
            {
                DateTime dtLefTime = Convert.ToDateTime(sStartDate + " " + TimeRegion.LeftTime);
                DateTime dtRighTime = Convert.ToDateTime(sEndDate + " " + TimeRegion.RightTime);
                if (startTime >= dtLefTime && endTime <= dtRighTime)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
