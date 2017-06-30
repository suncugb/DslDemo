using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RateEngine.DataObjects
{
    public class Rule
    {
        public string CarType { get; set; }
        public Dictionary<string, CaseDataObject> CaseDataObjects { get; set; }
        public Dictionary<string, LimitDataObject> LimitDataObjects { get; set; }
        /// <summary>
        /// 标识是否定义了时段(需要考虑多时段)
        /// </summary>
        public bool IsTimeRegion { get; set; }
        public double MonthLimitFee { get; set; }
        public double DayLimitFee { get; set; }
    }
}
