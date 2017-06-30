using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RateEngine
{
    public class WorkDayType
    {
        #region 字符串常量

        public const string WORKDAY = "workday";
        public const string WEEKEND = "weekend";
        public const string HOLIDAY = "holiday";

        #endregion

        #region 单例

        private static WorkDayType _instance;

        private WorkDayType()
        {
        }

        public static WorkDayType Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WorkDayType();
                }
                return _instance;
            }
        }

        #endregion
    }
}
