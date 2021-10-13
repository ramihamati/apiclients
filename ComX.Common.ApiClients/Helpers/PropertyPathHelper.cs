using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ComX.Common.ApiClients.Helpers
{
    public static class PropertyPathHelper
    {
        public static string GetFullPropertyPath<T, TRes>(Expression<Func<T, TRes>> expression)
        {
            return ExtractPropertPath(expression);
        }

        /// <summary>
        /// A constant expression will yield empty :        GetFullPropertyPath<MTestModel>(x => null)                                      = ""
        /// A unary expression with method call on left :   GetFullPropertyPath<MTestModel>(x => x.MyLocation.Address.ToString() == "")     = "MyLocation.Address"
        /// A unary expression :                            GetFullPropertyPath<MTestModel>(x => x.MyLocation.Address == "")                = "MyLocation.Address"
        /// A method call expression :                      GetFullPropertyPath<MTestModel>(x => x.MyLocation.Address.ToString().Trim())    = "MyLocation.Address"
        /// A Member expresion :                            GetFullPropertyPath<MTestModel>(x => x.MyLocation.Address)                      = "MyLocation.Address"
        /// A member expression with ext model :            GetFullPropertyPath<MTestModel>(x => model.MyLocation.Address)                  = "MyLocation.Address"
        /// A mehod binary expression:                      GetFullPropertyPath<MTestModel>(x => x.MyLocation.Address + "a")                = "MyLocation.Address"
        /// </summary>
        public static string GetFullPropertyPath<T>(Expression<Func<T, object>> expression)
        {
            return ExtractPropertPath(expression);
        }

        public static string GetFullPropertyPath(LambdaExpression lambdaExpression)
        {

            return ExtractPropertPath(lambdaExpression);
        }

        /// <summary>
        /// Returns the last property name accessed as string. 
        /// Exp : X=>X.ABC.DEF ==> DEF
        /// </summary>
        public static string GetLastPropertyName<T>(Expression<Func<T, object>> expression)
        {
            return GetLastPropertyName(expression as LambdaExpression);
        }

        public static string GetLastPropertyName(LambdaExpression expression)
        {
            string propPath = GetFullPropertyPath(expression);

            if (string.IsNullOrEmpty(propPath))
            {
                return null;
            }
            else
            {
                List<string> components = propPath.Split(new char[] { '.' }).ToList();

                return components.Last();
            }
        }


        /// <summary>
        /// {{x=>model.p1.p2}}      {{will yield .p1.p2}}
        /// {{model=>model.p1.p2}}  {{will yield .p1.p2}}
        /// </summary>
        private static string __getMemberExpression(MemberExpression expr)
        {
            string expression = "";

            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                //if we have the expression x=>model.propr1.prop2 - model is another property and it's perceived as a constant. The next line will make sure not to add "model." in the final output
                //if we have the expression x=>x.propr1.prop2 - the next if statement will not enteru and the output will automatically be prop1.prop2
                if (expr.Expression != null)
                {
                    if (expr.Expression.NodeType != ExpressionType.Constant)
                    {
                        expression += expr.Member.Name;

                        if (expr.Expression is MemberExpression newExpr)
                        {
                            expression = __getMemberExpression(newExpr) + "." + expression;
                        }
                    }
                }
            }

            return expression;
        }

        private static string ExtractFromBinaryExpression(BinaryExpression binexpr)
        {
            if (binexpr.Left is MemberExpression _memberExpression)
            {
                return ExtractFromMemberExpression(_memberExpression);
            }

            if (binexpr.Left is MethodCallExpression _methodCallExpression)
            {
                return RemoveFunctionCallsInExpression(_methodCallExpression);
            }

            return string.Empty;
        }

        /// <summary>
        /// {{x=>model.p1.p2}}      will yield {{model.p1.p2}}
        /// {{model=>model.p1.p2}}  will yield {{model.p1.p2}}
        /// </summary>
        private static string ExtractFromMemberExpression(MemberExpression expr)
        {
            string expression = __getMemberExpression(expr);

            if (expression.StartsWith("."))
            {
                expression = expression.Substring(1);
            }

            return "model." + expression;
        }

        /// <summary>
        /// <<x => x.MyLocation.Address == "">> yields <<model.MyLocation.Address>>
        /// </summary>
        private static string ExtractFromUnaryExpression(UnaryExpression expr)
        {
            if (expr.Operand is BinaryExpression _binaryExpression)
            {
                return ExtractFromBinaryExpression(_binaryExpression);
            }

            if (expr.Operand is MemberExpression memberExpression)
            {
                return ExtractFromMemberExpression(memberExpression);
            }

            return "";
        }

        private static string ExtractPropertPath(LambdaExpression expression)
        {
            if (expression.Body is ConstantExpression)
            {
                return string.Empty;
            }

            string unformatted = expression.Body.ToString();
            List<char> skipChs = new List<char> { '(', '{', '[', ')', ']', '}' };
            List<char> exitChs = new List<char> { '=', ')', '}', ']', '!', '<', '>', '|', '&', '%', '#', '$', '^', '*', '\\', '/', ',', '-', '+', '~', '`' };

            // PropertyNav.GetPropertyPath<Person>(x => x.Name.ToString().ToUpper().ToLower());
            //returns Name
            if (expression.Body is MethodCallExpression)
            {
                //x => x.Name.ToString() - getting the object without method calling = member.Object
                unformatted = RemoveFunctionCallsInExpression((MethodCallExpression)expression.Body);
            }

            //x=>model.Location.Address
            if (expression.Body is MemberExpression mexpr)
            {
                unformatted = ExtractFromMemberExpression(mexpr);
            }

            if (expression.Body is UnaryExpression unExpr)
            {
                unformatted = ExtractFromUnaryExpression(unExpr);
            }

            if (expression.Body is BinaryExpression binaryExpression)
            {
                unformatted = ExtractFromBinaryExpression(binaryExpression);
            }

            if (string.IsNullOrEmpty(unformatted))
            {
                return string.Empty;
            }
            //a unary expression may start with Convert()
            if (unformatted.Contains("("))
            {
                StringBuilder sb = new StringBuilder();

                bool firstBracket = false;

                foreach (char ch in unformatted)
                {
                    if (ch == '(' || ch == '{' || ch == '[')
                    {
                        firstBracket = true;
                        continue;
                    }
                    if (exitChs.Contains(ch))
                    {
                        unformatted = sb.ToString().Trim();
                        break;
                    }

                    if (!skipChs.Contains(ch) && firstBracket)
                    {
                        if (firstBracket)
                        {
                            sb.Append(ch);
                        }
                    }
                }
                unformatted = sb.ToString().Trim();
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (char ch in unformatted)
                {
                    if (exitChs.Contains(ch))
                    {
                        unformatted = sb.ToString().Trim();
                        break;
                    }

                    if (!skipChs.Contains(ch))
                    {
                        sb.Append(ch);
                    }
                }
                unformatted = sb.ToString().Trim();
            }

            //a property must be accessed
            if (!unformatted.Contains("."))
                return string.Empty;

            unformatted = unformatted.Remove(0, unformatted.IndexOf('.') + 1);

            return unformatted.ToString();
        }

        private static string RemoveFunctionCallsInExpression(MethodCallExpression methodCallExpression)
        {
            //PropertyNav.GetPropertyPath<Person>(x => x.Car.Model.Brand.Name.Where(c => true));
            if (methodCallExpression.Object == null)
            {
                if (methodCallExpression.Arguments.Count() > 0)
                {
                    return methodCallExpression.Arguments[0].ToString();
                }
            }

            if (methodCallExpression.Object is MethodCallExpression)
            {
                return RemoveFunctionCallsInExpression((MethodCallExpression)methodCallExpression.Object);
            }
            else
            {
                return methodCallExpression.Object.ToString();
            }
        }
    }
}
