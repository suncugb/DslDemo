using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RateEngine.DataObjects
{
    public class CaseBody
    {
        public double Vaule { get; set; }

        public ApiDataObject ApiDataObject { get; set; }

        public double UnitMoney { get; set; }

        public CaseBody()
        {
            Vaule = -1;
            UnitMoney = 1;
        }
    }
}
