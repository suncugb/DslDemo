using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RateEngine.DataObjects
{
    public class CaseDataObject
    {
        public string CaseHead { get; set; }
        public CaseCondition CaseCondition { get; set; }
        public CaseBody CaseBody { get; set; }
        public List<LimitDataObject>  LimitDataObjects { get; set; }

        public CaseDataObject()
        {
            if (LimitDataObjects == null)
            {
                LimitDataObjects = new List<LimitDataObject>();
            }
        }
    }
}
