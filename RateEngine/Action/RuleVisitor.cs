using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Antlr4.Runtime;
using RateEngine.DataObject;
using RateEngine.DataObjects;

namespace RateEngine
{
    public class RuleVisitor : RateGrammarBaseVisitor<Result>
    {
        #region 属性

        private static List<string> CarTypeList { get; set; }
        private const string OUTDAY = "24h+";
        private CaseDataObject caseDataObject;
        private LimitDataObject limitDataObject;
        private Rule rule;
        private Dictionary<string, CaseDataObject> caseDataObjects = new Dictionary<string, CaseDataObject>();
        private Dictionary<string, LimitDataObject> limitDataObjects = new Dictionary<string, LimitDataObject>();
        private Dictionary<string, Rule> rules = new Dictionary<string, Rule>();

        #endregion

        #region 属性访问

        internal List<string> GetCarTypeList()
        {
            return CarTypeList;
        }

        internal Dictionary<string, CaseDataObject> GetCases()
        {
            return caseDataObjects;
        }

        internal Dictionary<string, LimitDataObject> GetLimits()
        {
            return limitDataObjects;
        }

        internal Dictionary<string, Rule> GetRules()
        {
            return rules;
        }

        #endregion

        #region 单例

        private static RuleVisitor _ruleVisitor = null;

        private RuleVisitor()
        {
        }

        public static RuleVisitor Instance
        {
            get
            {
                if (CarTypeList == null)
                {
                    CarTypeList = new List<string>();
                }
                if (_ruleVisitor == null)
                {
                    _ruleVisitor = new RuleVisitor();
                }
                return _ruleVisitor;
            }
        }

        #endregion

        #region 公有方法

        public override Result VisitConfigfile(RateGrammarParser.ConfigfileContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override Result VisitCartype_expressions(RateGrammarParser.Cartype_expressionsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override Result VisitCartype_expression(RateGrammarParser.Cartype_expressionContext context)
        {
            rule = new Rule();
            VisitChildren(context);
            rules.Add(rule.CarType, rule);
            return null;
        }

        public override Result VisitCartype_head(RateGrammarParser.Cartype_headContext context)
        {
            if (context.CARTYPE() != null)
            {
                string carType = context.CARTYPE().GetText();
                rule.CarType = carType;
                CarTypeList.Add(carType);
            }
            return null;
        }

        public override Result VisitCartype_body(RateGrammarParser.Cartype_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override Result VisitCase_expressions(RateGrammarParser.Case_expressionsContext context)
        {
            VisitChildren(context);
            rule.CaseDataObjects = caseDataObjects;
            return null;
        }

        public override Result VisitLimit_expressions(RateGrammarParser.Limit_expressionsContext context)
        {
            VisitChildren(context);
            rule.LimitDataObjects = limitDataObjects;
            return null;
        }

        #region parser case expression 

        public override Result VisitCase_expression(RateGrammarParser.Case_expressionContext context)
        {
            caseDataObject = new CaseDataObject();
            VisitChildren(context);
            caseDataObjects.Add(caseDataObject.CaseHead, caseDataObject);
            return null;
        }

        public override Result VisitCase_head(RateGrammarParser.Case_headContext context)
        {
            if (context.CASEHEAD() != null)
            {
                string caseHead = context.CASEHEAD().GetText();
                caseDataObject.CaseHead = caseHead;
            }
            return null;
        }

        public override Result VisitCase_condition(RateGrammarParser.Case_conditionContext context)
        {
            CaseCondition condition = new CaseCondition();
            if (context.left_parktime() != null)
            {
                int leftParkTime = Convert.ToInt32(context.left_parktime().GetChild(0).GetText());
                condition.ParkTimeRange.LeftParkTime = leftParkTime;
            }
            if (context.right_parktime() != null)
            {
                int rightParkTime = Convert.ToInt32(context.right_parktime().GetChild(0).GetText());
                condition.ParkTimeRange.RightParkTime = rightParkTime;
            }
            if (context.left_time() != null)
            {
                string leftTime = context.left_time().GetText();
                condition.TimeRegion.LeftTime = leftTime;
                rule.IsTimeRegion = true;
            }
            if (context.middle_time() != null)
            {
                string middleTime = context.middle_time().GetText();
                condition.TimeRegion.MiddleTime = middleTime;
            }
            if (context.right_time() != null)
            {
                string rightTime = context.right_time().GetText();
                condition.TimeRegion.RightTime = rightTime;
            }
            if (context.OUTDAY() != null)
            {
                string outDay = context.OUTDAY().GetText();
                condition.IsOutDay = outDay == OUTDAY ? true : false;
            }
            if (context.workday() != null)
            {
                string workday = context.workday().WORKDAY().GetText();
                condition.WorkDay = workday;
            }
            caseDataObject.CaseCondition = condition;
            return null;
        }

        public override Result VisitCase_body(RateGrammarParser.Case_bodyContext context)
        {
            CaseBody body = new CaseBody();
            if (context.value() != null)
            {
                double value = Convert.ToDouble(context.value().GetText());
                body.Vaule = value;
            }
            if (context.api() != null)
            {
                ApiDataObject apiDataObject = new ApiDataObject();
                if (context.api().APINAME() != null)
                {
                    string apiName = context.api().APINAME().GetText();
                    apiDataObject.ApiName = apiName;
                }
                if (context.api().parameter() != null)
                {
                    int parameter = Convert.ToInt32(context.api().parameter().GetText());
                    apiDataObject.Parameter = parameter;
                }
                body.ApiDataObject = apiDataObject;
            }
            if (context.MUL() != null)
            {
                if (context.NUMBER() != null)
                {
                    double number = Convert.ToDouble(context.NUMBER().GetText());
                    body.UnitMoney = number;
                }
            }
            caseDataObject.CaseBody = body;
            return null;
        }

        #endregion

        #region parser limit expression

        public override Result VisitLimit_expression(RateGrammarParser.Limit_expressionContext context)
        {
            limitDataObject = new LimitDataObject();
            VisitChildren(context);
            limitDataObjects.Add(limitDataObject.LimitHead, limitDataObject);
            return null;
        }

        public override Result VisitLimit_head(RateGrammarParser.Limit_headContext context)
        {
            if (context.LIMITHEAD() != null)
            {
                string limitHead = context.LIMITHEAD().GetText();
                limitDataObject.LimitHead = limitHead;
            }
            return null;
        }

        public override Result VisitLimit_condition(RateGrammarParser.Limit_conditionContext context)
        {
            LimitCondition condition = new LimitCondition();
            if (context.case_head() != null)
            {
                string limitedCase = context.case_head().GetText();
                condition.LimitedCase = limitedCase;
                limitDataObject.LimitCondition = condition;
                caseDataObjects[limitedCase].LimitDataObjects.Add(limitDataObject);
            }
            if (context.DAYLIMIT() != null)
            {
                condition.IsDayLimit = context.DAYLIMIT().GetText() == "day" ? true:false;
                limitDataObject.LimitCondition = condition;
                foreach (var _case in caseDataObjects)
                {
                    _case.Value.LimitDataObjects.Add(limitDataObject);
                }
            }
            if (context.MONTHLIMIT() != null)
            {
                condition.IsDayLimit = context.MONTHLIMIT().GetText() == "month" ? true : false;
                limitDataObject.LimitCondition = condition;
                foreach (var _case in caseDataObjects)
                {
                    _case.Value.LimitDataObjects.Add(limitDataObject);
                }
            }
            return null;
        }

        public override Result VisitLimit_body(RateGrammarParser.Limit_bodyContext context)
        {
            LimitBody body = new LimitBody();
            if (context.value() != null)
            {
                body.LimitFee = Convert.ToDouble(context.value().GetText());
                limitDataObject.LimitBody = body;
                if (limitDataObject.LimitCondition.IsMonthLimit)
                {
                    rule.MonthLimitFee = body.LimitFee;
                }
                if (limitDataObject.LimitCondition.IsDayLimit)
                {
                    rule.DayLimitFee = body.LimitFee;
                }
            }
            return null;
        }

        #endregion

        #endregion

        #region 私有方法

        private void VisitChildren(ParserRuleContext context)
        {
            int count = context.ChildCount;
            for (int i = 0; i < count; i++)
            {
                Visit(context.GetChild(i));
            }
        }

        #endregion
    }
}
