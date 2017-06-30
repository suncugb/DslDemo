using System;
using System.Collections.Generic;
using RateEngine.Common;
using RateEngine.DataObjects;

namespace RateEngine.Api
{
    public class ApiImpl : Api
    {
        #region 单例

        private static ApiImpl _instance;

        private ApiImpl()
        {
        }

        public static ApiImpl Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApiImpl();
                }
                return _instance;
            }
        }

        #endregion

        #region 提供接口

        public IList<string> GetCarTypeList()
        {
            return RuleVisitor.Instance.GetCarTypeList();
        }

        public Result CalculateRate(IDictionary<string, string> input)
        {
            if (input == null || input.Count <= 0)
            {
                return null;
            }
            string carType = string.Empty;
            string workDayType = string.Empty;
            string entranceTime = string.Empty;
            string exitTime = string.Empty;
            foreach (var de in input)
            {
                switch (de.Key.ToLower())
                {
                    case Protocal.CarType:
                        carType = de.Value;
                        break;
                    case  Protocal.WorkDayType:
                        workDayType = de.Value;
                        break;
                    case Protocal.EntranceTime:
                        entranceTime = de.Value;
                        if (!string.IsNullOrEmpty(entranceTime))
                        {
                            ParkTime.Instance.EntranceTime = Convert.ToDateTime(entranceTime);
                        }
                        break;
                    case Protocal.ExitTime:
                        exitTime = de.Value;
                        if (!string.IsNullOrEmpty(exitTime))
                        {
                            ParkTime.Instance.ExiTime = Convert.ToDateTime(exitTime);
                        }
                        break;
                }
            }
            Console.WriteLine("车型:{0} 入场时间:{1} 出场时间:{2}", carType, ParkTime.Instance.EntranceTime, ParkTime.Instance.ExiTime);
            RateParser.Instance.ToParser();
            Rule rule = RuleVisitor.Instance.GetRules()[carType];
            Result result = RateCalculator.Instance.ToCalculate(rule, workDayType);
            return result;
        }
        #endregion
    }
}
