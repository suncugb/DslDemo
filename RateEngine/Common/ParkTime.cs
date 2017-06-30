using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RateEngine.Common
{
    public class ParkTime
    {
        #region 属性

        public DateTime EntranceTime { get; set; }
        public DateTime ExiTime { get; set; }

        #endregion

        #region 单例

        private static ParkTime _instance;

        private ParkTime()
        {
        }

        public static ParkTime Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ParkTime();
                }
                return _instance;
            }
        }

        #endregion

    }
}
