using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Scribe.Core.ConnectorApi;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class ExpressionVisitor
    {
        public ExpressionVisitor(bool throwOnOrLogicalOperator = false)
        {
            ThrowOnOrLogicalOperator = throwOnOrLogicalOperator;
        }
        
        public bool ThrowOnOrLogicalOperator { get; }
        
        public IEnumerable<IDictionary<string, string>> Visit(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var result = new List<IDictionary<string, string>>();
            switch (expression.ExpressionType)
            {
                case ExpressionType.Logical:
                    result.AddRange(
                        Visit((LogicalExpression) expression));
                    break;
                case ExpressionType.Comparison:
                    result.AddRange(
                        Visit((ComparisonExpression) expression));
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Expression type {expression.ExpressionType} is not supported");
            }
            return result;
        }
        
        private IEnumerable<IDictionary<string, string>> Visit(
            LogicalExpression expression)
        {
            if (expression.LeftExpression == null)
                throw new InvalidOperationException(
                    "Left expression in logical expression cannot be null");
            if (expression.RightExpression == null)
                throw new InvalidOperationException(
                    "Right expression in logical expression cannot be null");

            var leftResult = Visit(expression.LeftExpression);
            var rightResult = Visit(expression.RightExpression);
            var result = new List<IDictionary<string, string>>();

            switch (expression.Operator)
            {
                case LogicalOperator.And:
                    try
                    {
                        result.AddRange(
                            from leftCandidate in leftResult
                            from rightCandidate in rightResult
                            select leftCandidate
                                .Union(rightCandidate)
                                .ToDictionary(
                                    keySelector: kv => kv.Key,
                                    elementSelector: kv => kv.Value));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new InvalidOperationException(
                            "Expression contains mutually exclusive comparisons", ex);
                    }
                    break;
                case LogicalOperator.Or:
                    if (ThrowOnOrLogicalOperator)
                        goto default;
                    result.AddRange(leftResult);
                    result.AddRange(rightResult);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Logical operator {expression.Operator} is not supported");
            }

            return result;
        }

        private IEnumerable<IDictionary<string, string>> Visit(
            ComparisonExpression expression)
        {
            if (expression.Operator != ComparisonOperator.Equal)
                throw new InvalidOperationException(
                    $"Comparison operator {expression.LeftValue?.Value} is not supported for property {expression.Operator}");
            if (expression.LeftValue == null)
                throw new InvalidOperationException(
                    "Left value in comparison expression cannot be null");
            if (expression.LeftValue.ValueType != ComparisonValueType.Property)
                throw new InvalidOperationException(
                    $"Comparison value type {expression.LeftValue.ValueType} is not supported in left value of expression");
            if (expression.LeftValue.Value == null)
                throw new InvalidOperationException(
                    "Value of left value in comparison expression cannot be null");
            if (expression.RightValue == null)
                throw new InvalidOperationException(
                    "Right value in comparison expression cannot be null");
            if (expression.RightValue.ValueType != ComparisonValueType.Constant)
                throw new InvalidOperationException(
                    $"Comparison value type {expression.RightValue.ValueType} is not supported in right value of expression");

            var leftValue = expression.LeftValue.Value.ToString();
            var rightValue = expression.RightValue.Value is DateTime date
                ? date.ToString("O", CultureInfo.InvariantCulture)
                : expression.RightValue.Value?.ToString();
            return new List<Dictionary<string, string>>(0)
            {
                new Dictionary<string, string>(0) {{leftValue, rightValue}}
            };
        }
    }
}
