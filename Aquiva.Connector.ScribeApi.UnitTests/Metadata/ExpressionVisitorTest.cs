using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Scribe.Core.ConnectorApi;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class ExpressionVisitorTest
    {
        private readonly ITestOutputHelper _output;

        public ExpressionVisitorTest(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Theory, AutoData]
        public void ExpressionVisitor_AllPublicMembers_Always_ShouldHaveNullGuards(
            [Frozen] Fixture fixture,
            GuardClauseAssertion assertion)
        {
            fixture.Register<Expression>(() => new ComparisonExpression());
            assertion.Verify(typeof(ExpressionVisitor));
        }
        
        [Theory, MemberData(nameof(UnsupportedExpressionTypes))]
        public void ExpressionVisitor_Visit_WithUnsupportedExpressionType_ShouldThrow(
            ExpressionType unsupportedExpressionType)
        {
            var sut = CreateSystemUnderTest();
            var input = new DummyExpression(unsupportedExpressionType);
           
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Expression type", actual.Message);
            Assert.Contains(input.ExpressionType.ToString(), actual.Message);
            Assert.Contains("not supported", actual.Message);
        }

        public static IEnumerable<object[]> UnsupportedExpressionTypes()
        {
            return Enum.GetNames(typeof(ExpressionType))
                .Where(t => t != nameof(ExpressionType.Comparison) && t != nameof(ExpressionType.Logical))
                .Select(t => new [] {Enum.Parse(typeof(ExpressionType), t)});
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithTruthyThrowOnOrLogicalOperator_ShouldThrow()
        {
            // Arrange
            var sut = CreateSystemUnderTest(throwOnOrLogicalOperator: true);
            Assert.True(sut.ThrowOnOrLogicalOperator);
            var input = CreateSimpleLogicalExpression(LogicalOperator.Or);
           
            // Act
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            // Assert
            Assert.Contains("Logical operator", actual.Message);
            Assert.Contains(input.ExpressionType.ToString(), actual.Message);
            Assert.Contains("not supported", actual.Message);
        }
        
        [Theory, MemberData(nameof(UnsupportedLogicalOperators))]
        public void ExpressionVisitor_Visit_WithUnsupportedLogicalOperators_ShouldThrow(
            LogicalOperator unsupportedLogicalOperator)
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleLogicalExpression(unsupportedLogicalOperator);
           
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Logical operator", actual.Message);
            Assert.Contains(input.ExpressionType.ToString(), actual.Message);
            Assert.Contains("not supported", actual.Message);
        }
        
        public static IEnumerable<object[]> UnsupportedLogicalOperators()
        {
            return Enum.GetNames(typeof(LogicalOperator))
                .Where(t => t != nameof(LogicalOperator.And) && t != nameof(LogicalOperator.Or))
                .Select(t => new [] {Enum.Parse(typeof(LogicalOperator), t)});
        }

        [Theory, MemberData(nameof(UnsupportedComparisonOperators))]
        public void ExpressionVisitor_Visit_WithUnsupportedComparisonOperators_ShouldThrow(
            ComparisonOperator unsupportedComparisonOperator)
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(unsupportedComparisonOperator);
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Comparison operator", actual.Message);
            Assert.Contains(input.ExpressionType.ToString(), actual.Message);
            Assert.Contains("not supported", actual.Message);
        }
        
        public static IEnumerable<object[]> UnsupportedComparisonOperators()
        {
            return Enum.GetNames(typeof(ComparisonOperator))
                .Where(t => t != nameof(ComparisonOperator.Equal))
                .Select(t => new [] {Enum.Parse(typeof(ComparisonOperator), t)});
        }

        [Theory, MemberData(nameof(UnsupportedComparisonValueTypesOnTheLeft))]
        public void ExpressionVisitor_Visit_WithUnsupportedComparisonValueTypeOnTheLeft_ShouldThrow(
            ComparisonValueType unsupportedComparisonValueType)
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(ComparisonOperator.Equal);
            input.LeftValue.ValueType = unsupportedComparisonValueType;
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Comparison value type", actual.Message);
            Assert.Contains(input.ExpressionType.ToString(), actual.Message);
            Assert.Contains("not supported", actual.Message);
        }
        
        public static IEnumerable<object[]> UnsupportedComparisonValueTypesOnTheLeft()
        {
            return Enum.GetNames(typeof(ComparisonValueType))
                .Where(t => t != nameof(ComparisonValueType.Property))
                .Select(t => new [] {Enum.Parse(typeof(ComparisonValueType), t)});
        }
        
        [Theory, MemberData(nameof(UnsupportedComparisonValueTypesOnTheRight))]
        public void ExpressionVisitor_Visit_WithUnsupportedComparisonValueTypeOnTheRight_ShouldThrow(
            ComparisonValueType unsupportedComparisonValueType)
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(ComparisonOperator.Equal);
            input.RightValue.ValueType = unsupportedComparisonValueType;
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Comparison value type", actual.Message);
            Assert.Contains(input.ExpressionType.ToString(), actual.Message);
            Assert.Contains("not supported", actual.Message);
        }
        
        public static IEnumerable<object[]> UnsupportedComparisonValueTypesOnTheRight()
        {
            return Enum.GetNames(typeof(ComparisonValueType))
                .Where(t => t != nameof(ComparisonValueType.Constant))
                .Select(t => new [] {Enum.Parse(typeof(ComparisonValueType), t)});
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithNullLeftValue_ShouldThrow()
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(ComparisonOperator.Equal);
            input.LeftValue = null;
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Left value in comparison expression cannot be null", actual.Message);
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithNullRightValue_ShouldThrow()
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(ComparisonOperator.Equal);
            input.RightValue = null;
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Right value in comparison expression cannot be null", actual.Message);
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithNullLeftValueValue_ShouldThrow()
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(ComparisonOperator.Equal);
            input.LeftValue.Value = null;
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Value of left value in comparison expression cannot be null", actual.Message);
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithNullRightValueValue_ShouldProperlyReturnIt()
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(ComparisonOperator.Equal);
            input.RightValue.Value = null;
            
            var actual = sut.Visit(input).ToList().Single().Single();
         
            Assert.Equal(input.LeftValue.Value.ToString(), actual.Key);
            Assert.Null(actual.Value);
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithNullLeftExpression_ShouldThrow()
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleLogicalExpression(LogicalOperator.And);
            input.LeftExpression = null;
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Left expression in logical expression cannot be null", actual.Message);
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithNullRightExpression_ShouldThrow()
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleLogicalExpression(LogicalOperator.And);
            input.RightExpression = null;
            
            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Right expression in logical expression cannot be null", actual.Message);
        }
        
        [Fact]
        public void ExpressionVisitor_Visit_WithMutuallyExclusiveComparisons_ShouldThrow()
        {
            var sut = CreateSystemUnderTest();
            var input = new LogicalExpression
            {
                ExpressionType = ExpressionType.Logical,
                LeftExpression = new ComparisonExpression
                {
                    ExpressionType = ExpressionType.Comparison,
                    LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                    RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                },
                RightExpression = new ComparisonExpression
                {
                    ExpressionType = ExpressionType.Comparison,
                    LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                    RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                }
            };

            var actual = Assert.Throws<InvalidOperationException>(() => sut.Visit(input));
            
            Assert.Contains("Expression contains mutually exclusive comparisons", actual.Message);
        }
        
        [Theory, MemberData(nameof(SupportedExpressions))]
        public void ExpressionVisitor_Visit_WithSupportedExpression_ShouldReturnExpectedResult(
            string humanReadableExpression,
            Expression input,
            IList<IDictionary<string, string>> expected)
        {
            _output.WriteLine(humanReadableExpression);

            // Arrange
            var sut = CreateSystemUnderTest();
            
            // Act
            var actual = sut.Visit(input).ToList();
            
            // Assert
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                var actualCandidate = actual[i];
                var expectedCandidate = expected[i];
                
                Assert.Equal(expectedCandidate.Count, actualCandidate.Count);
                foreach (var expectedProperty in expectedCandidate)
                {
                    Assert.Contains(expectedProperty.Key, actualCandidate.Keys);
                    Assert.Equal(expectedProperty.Value, actualCandidate[expectedProperty.Key]);
                }
            }
        }

        public static IEnumerable<object[]> SupportedExpressions()
        {
            yield return new object[]
            {
                "Foo.Id == 1",
                new ComparisonExpression
                {
                    Operator = ComparisonOperator.Equal,
                    LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                    RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"}
                    }
                }
            };
            
            yield return new object[]
            {
                $@"       AND    {NewLine}" +
                $@"      /   \   {NewLine}" +
                $@"Id = 1     Id = 1",
                new LogicalExpression
                {
                    Operator = LogicalOperator.And,
                    LeftExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                    },
                    RightExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"}
                    }
                }
            };
            
            yield return new object[]
            {
                $@"   AND    {NewLine}" +
                $@"  /   \   {NewLine}" +
                $@" 1     2",
                new LogicalExpression
                {
                    Operator = LogicalOperator.And,
                    LeftExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                    },
                    RightExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                        {"Bar.Id", "2"}
                    }
                }
            };
            
            yield return new object[]
            {
                $@"   OR    {NewLine}" +
                $@"  /  \   {NewLine}" +
                $@" 1    2",
                new LogicalExpression
                {
                    Operator = LogicalOperator.Or,
                    LeftExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                    },
                    RightExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                    },
                    new Dictionary<string, string>
                    {
                        {"Bar.Id", "2"}
                    }   
                }
            };

            yield return new object[]
            {
                $@"     OR     {NewLine}" +
                $@"    /  \    {NewLine}" +
                $@"   OR   3   {NewLine}" +
                $@"  /  \      {NewLine}" +
                $@" 1    2",
                new LogicalExpression
                {
                    Operator = LogicalOperator.Or,
                    LeftExpression = new LogicalExpression
                    {
                        Operator = LogicalOperator.Or,
                        LeftExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                        },
                        RightExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                        }
                    },
                    RightExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Baz.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "3")
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                    },
                    new Dictionary<string, string>
                    {
                        {"Bar.Id", "2"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Baz.Id", "3"}
                    }
                } 
            };

            yield return new object[]
            {
                $@"      OR    {NewLine}" +
                $@"     /  \   {NewLine}" +
                $@"   AND   3  {NewLine}" +
                $@"  /   \     {NewLine}" +
                $@" 1     2",
                new LogicalExpression
                {
                    Operator = LogicalOperator.Or,
                    LeftExpression = new LogicalExpression
                    {
                        Operator = LogicalOperator.And,
                        LeftExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                        },
                        RightExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                        }
                    },
                    RightExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Baz.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "3")
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                        {"Bar.Id", "2"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Baz.Id", "3"}
                    }
                } 
            };

            yield return new object[]
            {
                $@"      AND        {NewLine}" +
                $@"     /   \       {NewLine}" +
                $@"   OR     OR     {NewLine}" +
                $@"  / \     / \    {NewLine}" +
                $@" 1   2   3   4",
                new LogicalExpression
                {
                    Operator = LogicalOperator.And,
                    LeftExpression = new LogicalExpression
                    {
                        Operator = LogicalOperator.Or,
                        LeftExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                        },
                        RightExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                        }
                    },
                    RightExpression = new LogicalExpression
                    {
                        Operator = LogicalOperator.Or,
                        LeftExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Baz.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "3")
                        },
                        RightExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Qux.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "4")
                        }
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                        {"Baz.Id", "3"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                        {"Qux.Id", "4"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Bar.Id", "2"},
                        {"Baz.Id", "3"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Bar.Id", "2"},
                        {"Qux.Id", "4"}
                    }
                }
            };
            
            yield return new object[]
            {
                $@"         OR     {NewLine}" +
                $@"        /  \    {NewLine}" +
                $@"      AND   4   {NewLine}" +
                $@"     /  \       {NewLine}" +
                $@"    OR   3      {NewLine}" +
                $@"   / \          {NewLine}" +
                $@"  1   2",
                new LogicalExpression
                {
                    Operator = LogicalOperator.Or,
                    LeftExpression = new LogicalExpression
                    {
                        Operator = LogicalOperator.And,
                        LeftExpression = new LogicalExpression
                        {
                            Operator = LogicalOperator.Or,
                            LeftExpression = new ComparisonExpression
                            {
                                Operator = ComparisonOperator.Equal,
                                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                                RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                            },
                            RightExpression = new ComparisonExpression
                            {
                                Operator = ComparisonOperator.Equal,
                                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                                RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                            }
                        },
                        RightExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Baz.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "3")
                        }
                    },
                    RightExpression = new ComparisonExpression
                    {
                        Operator = ComparisonOperator.Equal,
                        LeftValue = new ComparisonValue(ComparisonValueType.Property, "Qux.Id"),
                        RightValue = new ComparisonValue(ComparisonValueType.Constant, "4")
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                        {"Baz.Id", "3"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Bar.Id", "2"},
                        {"Baz.Id", "3"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Qux.Id", "4"}
                    }
                }
            };
            
            yield return new object[]
            {
                $@"      OR         {NewLine}" +
                $@"     /   \       {NewLine}" +
                $@"   AND   AND     {NewLine}" +
                $@"  / \     / \    {NewLine}" +
                $@" 1   2   3   4",
                new LogicalExpression
                {
                    Operator = LogicalOperator.Or,
                    LeftExpression = new LogicalExpression
                    {
                        Operator = LogicalOperator.And,
                        LeftExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                        },
                        RightExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                        }
                    },
                    RightExpression = new LogicalExpression
                    {
                        Operator = LogicalOperator.And,
                        LeftExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Baz.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "3")
                        },
                        RightExpression = new ComparisonExpression
                        {
                            Operator = ComparisonOperator.Equal,
                            LeftValue = new ComparisonValue(ComparisonValueType.Property, "Qux.Id"),
                            RightValue = new ComparisonValue(ComparisonValueType.Constant, "4")
                        }
                    }
                },
                new List<IDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        {"Foo.Id", "1"},
                        {"Bar.Id", "2"}
                    },
                    new Dictionary<string, string>
                    {
                        {"Baz.Id", "3"},
                        {"Qux.Id", "4"}
                    }
                }
            };
        }

        [Theory, AutoData]
        public void ExpressionVisitor_Visit_Always_ShouldCorrectlyHandleDateTime(
            DateTime expected)
        {
            var sut = CreateSystemUnderTest();
            var input = CreateSimpleComparisonExpression(ComparisonOperator.Equal);
            input.RightValue.Value = expected;

            var actual = sut.Visit(input).Single().Single();

            var actualDateTime = DateTime.Parse(actual.Value);
            Assert.Equal(expected, actualDateTime);
        }

        private static ExpressionVisitor CreateSystemUnderTest(
            bool throwOnOrLogicalOperator = false)
        {
            return new ExpressionVisitor(throwOnOrLogicalOperator);
        }

        private static LogicalExpression CreateSimpleLogicalExpression(
            LogicalOperator logicalOperator)
        {
            return new LogicalExpression
            {
                Operator = logicalOperator,
                LeftExpression = new ComparisonExpression
                {
                    Operator = ComparisonOperator.Equal,
                    LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                    RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
                },
                RightExpression = new ComparisonExpression
                {
                    Operator = ComparisonOperator.Equal,
                    LeftValue = new ComparisonValue(ComparisonValueType.Property, "Bar.Id"),
                    RightValue = new ComparisonValue(ComparisonValueType.Constant, "2")
                }
            };
        }

        private static ComparisonExpression CreateSimpleComparisonExpression(
            ComparisonOperator comparisonOperator)
        {
            return new ComparisonExpression
            {
                Operator = comparisonOperator,
                LeftValue = new ComparisonValue(ComparisonValueType.Property, "Foo.Id"),
                RightValue = new ComparisonValue(ComparisonValueType.Constant, "1")
            };
        }

        private class DummyExpression : Expression
        {
            public DummyExpression(ExpressionType expressionType)
            {
                ExpressionType = expressionType;
            }
            
            public override void OnDeserialized(StreamingContext streamingContext)
            {
            }
        }
    }
}
