using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RateEngine.DataObjects
{
    public class TimeRange
    {
        public int LeftParkTime { get; set; }

        public int RightParkTime { get; set; }
        public bool InTimeRange(int parkTime)
        {
            if (parkTime >= LeftParkTime && parkTime <= RightParkTime)
            {
                return true;
            }
            return false;
        }
    }
}
