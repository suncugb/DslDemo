using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using RateEngine.Common;
using RateEngine.DataObjects;

/*待优化点：
 * 1.需要增加对workday的支持
 * 2.需要把免费时段、首付时段等放到limit部分
 */
namespace RateEngine
{
    public class RateCalculator
    {
        #region 属性

        private Rule CurrentRule { get; set; }

        private string leftTime;

        private string LeftTime
        {
            get
            {
                if (CurrentRule != null)
                {
                    foreach (var _case in CurrentRule.CaseDataObjects)
                    {
                        string left = _case.Value.CaseCondition.TimeRegion.LeftTime;
                        if (!string.IsNullOrEmpty(left))
                        {
                            leftTime = left;
                            return leftTime;
                        }
                    }
                }
                return null;
            }
        }

        private string middleTime;

        private string MiddleTime
        {
            get
            {
                if (CurrentRule != null)
                {
                    foreach (var _case in CurrentRule.CaseDataObjects)
                    {
                        string middle = _case.Value.CaseCondition.TimeRegion.MiddleTime;
                        if (!string.IsNullOrEmpty(middle))
                        {
                            middleTime = middle;
                            return middleTime;
                        }
                    }
                }
                return null;
            }
        }

        private string rightTime;

        private string RightTime
        {
            get
            {
                if (CurrentRule != null)
                {
                    foreach (var _case in CurrentRule.CaseDataObjects)
                    {
                        string right = _case.Value.CaseCondition.TimeRegion.RightTime;
                        if (!string.IsNullOrEmpty(right))
                        {
                            rightTime = right;
                            return rightTime;
                        }
                    }
                }
                return null;
            }
        }

        private bool NotContinue { get; set; }

        #endregion

        #region 单例

        private static RateCalculator _calcultor;

        private RateCalculator()
        {
        }

        public static RateCalculator Instance
        {
            get
            {
                if (_calcultor == null)
                {
                    _calcultor = new RateCalculator();
                }
                return _calcultor;
            }
        }

        #endregion

        #region 公有方法

        public Result ToCalculate(Rule rule, string workDayType)
        {
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("\n计算过程:");
            CurrentRule = rule;
            Result result = new Result();
            result.CarType = rule.CarType;
            result.Fee = Calculate(ParkTime.Instance.EntranceTime,ParkTime.Instance.ExiTime);
            //foreach (var _case in rule.CaseDataObjects)
            //{
            //    CaseCondition condition = _case.Value.CaseCondition;
            //    if (condition.WorkDay == null ||
            //        (condition.WorkDay != null && condition.WorkDay.ToLower() == workDayType))
            //    {
            //        result.Fee = Calculate(_case.Value);
            //    }
            //}
            return result;
        }

        #endregion

        #region 私有方法

        private double Calculate(DateTime entranceTime,DateTime exitTime)
        {
            //////////////////////////////////////////////////////
            //todo:case1-免费时段更合适以limit的形式出现，此处以后需要去掉
            TimeSpan ts = exitTime - entranceTime;
            if (ts.TotalMinutes <= 15)
            {
                return 0;
            }
            //////////////////////////////////////////////////////
            double fee = 0;
            fee = SplitMonth( entranceTime,exitTime);
            return fee;
        }

        private double SplitMonth(DateTime startTime, DateTime endTime)
        {
            double montLimitFee = CurrentRule.MonthLimitFee;
            double fee = 0;
            if (montLimitFee > 0) //有月限额
            {
                TimeSpan ts = endTime - startTime;
                int nMonth = ts.Days/30;
                if (nMonth > 0)
                {
                    Console.WriteLine("停车时长:{0}月{1}天{2}时{3}分", nMonth,ts.Days, ts.Hours, ts.Minutes);
                    double fee1 = nMonth*montLimitFee;
                    double fee2 = SplitDay(startTime.AddMonths(nMonth), endTime);
                    if (fee2 > montLimitFee)
                    {
                        Console.WriteLine("费用总额:{0}元，月限额:{1}元，实际结果:{2}元", fee2, montLimitFee, montLimitFee);
                        fee2 = montLimitFee;
                    }
                    fee = fee1 + fee2;
                }
            }
            else //没有月限额
            {
                fee = SplitDay( startTime, endTime);
            }
            return fee;
        }

        private double SplitDay(DateTime startTime, DateTime endTime)
        {
            double dayLimitFee = CurrentRule.DayLimitFee;
            double fee = 0;
            TimeSpan ts = endTime - startTime;
            int nDay = ts.Days;
            if (nDay > 0) //停车时长超过1天
            {
                Console.WriteLine("停车时长:{0}天{1}时{2}分", nDay, ts.Hours, ts.Minutes);
                DateTime subStartTime = startTime;
                double timeRegionFee1 = 0;
                //Console.WriteLine("------按天循环分段-------");
                //整数天按天循环分段
                for (int i = 0; i < nDay; i++)
                {
                    DateTime subEndTime = subStartTime.AddDays(1);
                    Console.WriteLine("\n第{0}天:[{1}，{2}]", i + 1, subStartTime, subEndTime);
                    timeRegionFee1 = SplitTimeRegion(subStartTime, subEndTime);
                    if (dayLimitFee > 0)
                    {
                        if (timeRegionFee1 > dayLimitFee)
                        {
                            Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee1, dayLimitFee, dayLimitFee);
                            timeRegionFee1 = dayLimitFee;
                        }
                        else
                        {
                            Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee1, dayLimitFee, timeRegionFee1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee1, dayLimitFee, timeRegionFee1);
                    }
                    fee += timeRegionFee1;
                    subStartTime = subEndTime;
                }
                //非整数天分段
                Console.WriteLine("\n第{0}天:[{1}，{2}]", nDay + 1, startTime.AddDays(nDay), endTime);
                double timeRegionFee2 = SplitTimeRegion(startTime.AddDays(nDay), endTime);
                if (dayLimitFee > 0)
                {
                    if (timeRegionFee2 > dayLimitFee)
                    {
                        Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee2, dayLimitFee, dayLimitFee);
                        timeRegionFee2 = dayLimitFee;
                    }
                    else
                    {
                        Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee2, dayLimitFee, timeRegionFee2);
                    }
                }
                else
                {
                    Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee2, dayLimitFee, timeRegionFee2);
                }
                fee += timeRegionFee2;
            }
            else //停车时长不足1天
            {
                Console.WriteLine("不足一天:[{0}，{1}]",startTime,endTime);
                double timeRegionFee3 = SplitTimeRegion(startTime, endTime);
                if (dayLimitFee > 0)
                {
                    if (timeRegionFee3 > dayLimitFee)
                    {
                        Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee3, dayLimitFee, dayLimitFee);
                        timeRegionFee3 = dayLimitFee;
                    }
                    else
                    {
                        Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee3, dayLimitFee, timeRegionFee3);
                    }
                }
                else
                {
                    Console.WriteLine("费用总额:{0}元，日限额:{1}元，实际结果:{2}元", timeRegionFee3, dayLimitFee, timeRegionFee3);
                }
                fee = timeRegionFee3;
            }
            return fee;
        }

        private double SplitTimeRegion(DateTime startTime, DateTime endTime)
        {
            double fee = 0;
            //针对没有定义时间段的情况
            if (!CurrentRule.IsTimeRegion)
            {
                fee = CalculateBy(startTime, endTime);
            }
            else
            {
                //针对定义时间段的情况
                string left = LeftTime;
                string middle = MiddleTime;
                string right = RightTime;
                string sEntranceDate = startTime.ToShortDateString();
                string sExitDate = endTime.ToShortDateString();
                DateTime dtLeft = Convert.ToDateTime(sEntranceDate + " " + left);
                DateTime dtMiddle = Convert.ToDateTime(sEntranceDate + " " + middle);
                DateTime dtRight;
                if (sEntranceDate == sExitDate)
                {
                    dtRight = Convert.ToDateTime(sExitDate + " " + right).AddDays(1);
                }
                else
                {
                    dtRight = Convert.ToDateTime(sExitDate + " " + right);
                }
                Console.WriteLine("白天时段:[{0}-{1}]，夜间时段:[{2}-{3}]",left,middle,middle,right);
                Console.WriteLine("------开始切分时间段--------");
                //-----------------------------------------------------------------|
                //                      按时间轴划分时间段                            |
                //                                                                 |             
                //               left            middle           right            |
                //----------------|----------------|----------------|--------------|
                //               8:00            19:00             8:00            | 
                //                                                                 |         
                //-----------------------------------------------------------------|
                if (startTime < dtLeft)
                {
                    if (endTime < dtLeft)
                    {
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]",subStartTime,subEndTime);
                        fee = CalculateBy(subStartTime, subEndTime);
                    }
                    else if (endTime >= dtLeft && endTime <= dtMiddle)
                    {
                        //the first
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = dtLeft;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee1 = CalculateBy(subStartTime, subEndTime);
                        //the second
                        subStartTime = dtLeft;
                        subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee2 = CalculateBy(subStartTime, subEndTime);
                        fee = fee1 + fee2;
                    }
                    else if (endTime > dtMiddle)
                    {
                        //the first
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = dtLeft;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee1 = CalculateBy(subStartTime, subEndTime);
                        //the second
                        subStartTime = dtLeft;
                        subEndTime = dtMiddle;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee2 = CalculateBy(subStartTime, subEndTime);
                        //the third
                        subStartTime = dtMiddle;
                        subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee3 = CalculateBy(subStartTime, subEndTime);
                        fee = fee1 + fee2 + fee3;
                    }
                }
                if (startTime >= dtLeft && startTime <= dtMiddle)
                {
                    if (endTime >= dtLeft && endTime <= dtMiddle)
                    {
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        fee = CalculateBy(subStartTime, subEndTime);
                    }
                    else if (endTime > dtMiddle && endTime < dtRight)
                    {
                        //the first
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = dtMiddle;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee1 = CalculateBy(subStartTime, subEndTime);
                        //the second
                        subStartTime = dtMiddle;
                        subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee2 = CalculateBy(subStartTime, subEndTime);
                        fee = fee1 + fee2;
                    }
                    else if (endTime >= dtRight)
                    {
                        //the first
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = dtMiddle;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee1 = CalculateBy(subStartTime, subEndTime);
                        //the second
                        subStartTime = dtMiddle;
                        subEndTime = dtRight;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee2 = CalculateBy(subStartTime, subEndTime);
                        //the third
                        subStartTime = dtRight;
                        subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee3 = CalculateBy(subStartTime, subEndTime);
                        fee = fee1 + fee2 + fee3;
                    }
                }
                if (startTime > dtMiddle && startTime < dtRight)
                {
                    if (endTime > dtMiddle && endTime < dtRight)
                    {
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        fee = CalculateBy(subStartTime, subEndTime);
                    }
                    else if (endTime >= dtRight)
                    {
                        //the first
                        DateTime subStartTime = startTime;
                        DateTime subEndTime = dtRight;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee1 = CalculateBy(subStartTime, subEndTime);
                        //the second
                        subStartTime = dtRight;
                        subEndTime = endTime;
                        Console.WriteLine("时间段:[{0}-{1}]", subStartTime, subEndTime);
                        double fee2 = CalculateBy(subStartTime, subEndTime);
                        fee = fee1 + fee2;
                    }
                }
            }
            return fee;
        }

        private double SplitTime(int nMinutes, int unitTime, double unitMoney = 1)
        {
            if (nMinutes == 0 || unitTime == 0)
            {
                return 0;
            }
            double fee = 0;
            int n = 0;
            if (nMinutes%unitTime == 0)
            {
                n = nMinutes/unitTime;
            }
            else
            {
                n = nMinutes / unitTime + 1;
            }
            fee = n * unitMoney;
            return fee;
        }

        //private double MonthLimitFee()
        //{
        //    foreach (var limit in CurrentRule.LimitDataObjects)
        //    {
        //        if (limit.Value.LimitCondition.IsMonthLimit)
        //        {
        //            double fee = limit.Value.LimitBody.LimitFee;
        //            return fee;
        //        }
        //    }
        //    return 0;
        //}

        //private double DayLimitFee(CaseDataObject caseDataObject)
        //{
        //    if (caseDataObject == null)
        //    {
        //        return 0;
        //    }
        //    foreach (var limit in caseDataObject.LimitDataObjects)
        //    {
        //        if (limit.LimitCondition.IsDayLimit)
        //        {
        //            double fee = limit.LimitBody.LimitFee;
        //            return fee;
        //        }
        //    }
        //    return 0;
        //}

        //private double NotToCalculate(DateTime startTime, DateTime endtTime)
        //{
        //    foreach (var _case in CurrentRule.CaseDataObjects)
        //    {
        //        CaseDataObject caseDataObject = _case.Value;
        //        foreach (var _limit in caseDataObject.LimitDataObjects)
        //        {
        //            if (_limit.LimitCondition.LimitedCase == null)
        //            {
        //                if (caseDataObject.CaseBody.ApiDataObject == null)
        //                {
        //                    NotContinue = true;
        //                    double fee = caseDataObject.CaseBody.Vaule;
        //                }
        //            }
        //        }
        //    }
        //    return 0;
        //}

        private double CalculateBy(DateTime startTime, DateTime endTime)
        {
            CaseDataObject _case = GetMatchCase(startTime, endTime);
            if (_case == null)
            {
                return 0;
            }
            int unitTime = _case.CaseBody.ApiDataObject.Parameter;
            double unitMoney = _case.CaseBody.UnitMoney;
            int nMinutes = (int)(endTime - startTime).TotalMinutes;
            double calFee = SplitTime(nMinutes, unitTime, unitMoney);
            Console.WriteLine("本时段计费:{0}元 (单位时长:{1}分 单位金额:{2}元 停车时长:{3}分钟)", calFee, unitTime, unitMoney, nMinutes);
            double fee = CompareToLimit(_case, calFee);
            return fee;
        }

        private double CompareToLimit(CaseDataObject caseDataObject, double fee)
        {
            foreach (var limit in caseDataObject.LimitDataObjects)
            {
                if (limit.LimitCondition.LimitedCase != null)
                {
                    if (fee > limit.LimitBody.LimitFee)
                    {
                        Console.WriteLine("计算限额:实际结果:{0}元，时段限额:{1}元", limit.LimitBody.LimitFee, limit.LimitBody.LimitFee);
                        fee = limit.LimitBody.LimitFee;
                    }
                }
            }
            return fee;
        }

        private CaseDataObject GetMatchCase(DateTime startTime, DateTime endTime)
        {
            foreach (var _case in CurrentRule.CaseDataObjects)
            {
                if (InTimeRegion(_case.Value,startTime, endTime))
                {
                    return _case.Value;
                }
            }
            return null;
        }

        private bool InTimeRegion(CaseDataObject caseDataObject,DateTime startTime, DateTime endTime)
        {
            string sStartTime = startTime.ToShortTimeString();
            string[] sArray = sStartTime.Split(':');
            int sHour = Convert.ToInt32(sArray[0]);
            string sEndTime = endTime.ToShortTimeString();
            string[] eArray = sEndTime.Split(':');
            int eHour = Convert.ToInt32(eArray[0]);

            string leftTime = caseDataObject.CaseCondition.TimeRegion.LeftTime;
            string middleTime = caseDataObject.CaseCondition.TimeRegion.MiddleTime;
            string rightTime = caseDataObject.CaseCondition.TimeRegion.RightTime;
            if (leftTime != null && middleTime == null && rightTime != null)
            {
                string[] leftArray = leftTime.Split(':');
                int leftHour = Convert.ToInt32(leftArray[0]);
                string[] rightArray = rightTime.Split(':');
                int rightHour = Convert.ToInt32(rightArray[0]);
                //通过判断差值是否同为正或负数
                int sub1 = rightHour - leftHour;
                int sub2 = eHour - sHour;
                if (sub1*sub2 >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
