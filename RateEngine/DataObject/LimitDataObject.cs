using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RateEngine.DataObject;

namespace RateEngine.DataObjects
{
    public class LimitDataObject
    {
        public string LimitHead { get; set; }
        public LimitCondition LimitCondition { get; set; }
        public LimitBody LimitBody { get; set; }
    }
}
