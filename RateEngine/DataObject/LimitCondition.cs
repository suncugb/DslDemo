using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RateEngine.DataObjects
{
   public class LimitCondition
    {
       public string LimitedCase { get; set; }
       public bool IsDayLimit { get; set; }
       public bool IsMonthLimit { get; set; }
    }
}
