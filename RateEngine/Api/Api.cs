using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RateEngine.DataObjects;

namespace RateEngine.Api
{
    public interface Api
    {
        IList<string> GetCarTypeList();

        Result CalculateRate(IDictionary<string,string> input);
    }
}
