using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using RateEngine.Common;
using RateEngine.DataObjects;

namespace RateEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入 车型|入场时间|出场时间：");
            string inputStr = Console.ReadLine();
            string[] parameters = inputStr.Split('|');
            string carType = parameters[0];
            string entranceTime = parameters[1];
            string exitTime = parameters[2];

            IDictionary<string,string> input = new ConcurrentDictionary<string, string>();
            input.Add(Protocal.CarType,"大型车");
            input.Add(Protocal.WorkDayType,"workday");
            //input.Add(Protocal.EntranceTime, "2016-10-10 14:47:55");
            //input.Add(Protocal.ExitTime, "2016-10-12 14:57:47");
            input.Add(Protocal.EntranceTime, entranceTime);
            input.Add(Protocal.ExitTime, exitTime);
            Result result = Api.ApiImpl.Instance.CalculateRate(input);

            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("\n计算结果:\n车型:{0} 停车费用:{1}元", result.CarType, result.Fee);
            Console.ReadLine();
        }
    }
}
