using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Xunit;
using static Aquiva.Connector.ScribeApi.Metadata.ReflectionExtensions;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class ReflectionExtensionsTest
    {
        [Theory, AutoData]
        public void ReflectionExtensions_AllPublicMembers_Always_ShouldHaveNullGuards(
            GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(ReflectionExtensions));
        }

        [Theory]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(IEnumerable<string>), false)]
        [InlineData(typeof(string[]), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(IEnumerable<int>), false)]
        [InlineData(typeof(int[]), false)]
        [InlineData(typeof(IEnumerable<int?>), false)]
        [InlineData(typeof(int?[]), false)]
        [InlineData(typeof(int?), true)]
        public void ReflectionExtensions_IsNullable_Always_ShouldReturnExpectedResult(
            Type sut,
            bool expected)
        {
            Assert.Equal(expected, sut.IsNullable());
        }

        [Theory]
        // String should not be treated as an array of chars
        [InlineData(typeof(string), false)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(StringSplitOptions), false)]
        [InlineData(typeof(int?), false)]
        [InlineData(typeof(IEnumerable), true)]
        [InlineData(typeof(IEnumerable<string>), true)]
        [InlineData(typeof(IEnumerable<int>), true)]
        [InlineData(typeof(IEnumerable<StringSplitOptions>), true)]
        [InlineData(typeof(IEnumerable<int?>), true)]
        [InlineData(typeof(List<object>), true)]
        [InlineData(typeof(List<string>), true)]
        [InlineData(typeof(List<int>), true)]
        [InlineData(typeof(List<StringSplitOptions>), true)]
        [InlineData(typeof(List<int?>), true)]
        [InlineData(typeof(object[]), true)]
        [InlineData(typeof(string[]), true)]
        [InlineData(typeof(int[]), true)]
        [InlineData(typeof(StringSplitOptions[]), true)]
        [InlineData(typeof(int?[]), true)]
        public void ReflectionExtensions_IsEnumerable_ForGivenTypes_ShouldReturnExpectedResult(
            Type sut,
            bool expected)
        {
            Assert.Equal(expected, sut.IsEnumerable());
        }

        [Theory]
        [InlineData(typeof(int?), typeof(int))]
        [InlineData(typeof(IEnumerable), typeof(object))]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        [InlineData(typeof(IEnumerable<int>), typeof(int))]
        [InlineData(typeof(IEnumerable<StringSplitOptions>), typeof(StringSplitOptions))]
        [InlineData(typeof(IEnumerable<int?>), typeof(int))]
        [InlineData(typeof(List<object>), typeof(object))]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(List<int>), typeof(int))]
        [InlineData(typeof(List<StringSplitOptions>), typeof(StringSplitOptions))]
        [InlineData(typeof(List<int?>), typeof(int))]
        [InlineData(typeof(object[]), typeof(object))]
        [InlineData(typeof(string[]), typeof(string))]
        [InlineData(typeof(int[]), typeof(int))]
        [InlineData(typeof(StringSplitOptions[]), typeof(StringSplitOptions))]
        [InlineData(typeof(int?[]), typeof(int))]
        public void ReflectionExtensions_GetPropertyDefinitionType_ForNullableAndEnumerableTypes_ShouldReturnGenericTypeArgument(
            Type sut,
            Type expected)
        {
            Assert.Equal(expected, sut.GetPropertyDefinitionType());
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(StringSplitOptions))]
        public void ReflectionExtensions_GetPropertyDefinitionType_ForSimpleTypes_ShouldReturnInput(
            Type sut)
        {
            Assert.Equal(sut, sut.GetPropertyDefinitionType());
        }

        [Theory]
        [InlineData(typeof(string), typeof(string))]
        [InlineData(typeof(int), typeof(int))]
        [InlineData(typeof(int?), typeof(int))]
        public void ReflectionExtensions_GetTypeName_Always_ShouldReturnNameOfNonNullableType(
            Type sut,
            Type expected)
        {
            Assert.Equal(expected.FullName, sut.GetTypeName());
        }

        [Fact]
        public void ReflectionExtensions_GetObjectDefinitionFullName_ForTypeWithoutFullNameInAttribute_ShouldReturnTypeName()
        {
            var sut = typeof(ObjectDefinition_ShouldReturnTypeName);
            var expected = sut.Name;
            var actual = sut.GetObjectDefinitionFullName();
            Assert.Equal(expected, actual);

            actual = GetObjectDefinitionFullName<ObjectDefinition_ShouldReturnTypeName>();
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ReflectionExtensions_GetObjectDefinitionFullName_ForTypeWithFullNameInAttribute_ShouldReturnTrimmedFullName()
        {
            var sut = typeof(ObjectDefinition_ShouldReturnObjectDefinitionFullName);
            var expected = sut.GetCustomAttribute<ObjectDefinitionAttribute>().FullName.Trim();
            var actual = sut.GetObjectDefinitionFullName();
            Assert.Equal(expected, actual);

            actual = GetObjectDefinitionFullName<ObjectDefinition_ShouldReturnObjectDefinitionFullName>();
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ReflectionExtensions_GetObjectDefinitionFullName_ForTypeWithoutObjectDefinitionAttribute_ShouldThrow()
        {
            var sut = typeof(ObjectDefinition_ShouldThrow);
            
            var actual = Assert.Throws<ArgumentException>(
                () => sut.GetObjectDefinitionFullName());
            
            Assert.Contains("Can't find object definition", actual.Message);
            Assert.Contains(sut.FullName, actual.Message);
            Assert.Equal("type", actual.ParamName);

            
            actual = Assert.Throws<ArgumentException>(
                () => GetObjectDefinitionFullName<ObjectDefinition_ShouldThrow>());
            
            Assert.Contains("Can't find object definition", actual.Message);
            Assert.Contains(sut.FullName, actual.Message);
            Assert.Equal("type", actual.ParamName);
        }

        [Fact]
        public void ReflectionExtensions_GetPropertyDefinitionFullName_ForMemberWithoutFullNameInAttribute_ShouldReturnPropertyName()
        {
            var sut = typeof(PropertyDefinitionHost);
            var expected = nameof(PropertyDefinitionHost.ShouldReturnPropertyName);

            var actual = sut.GetPropertyDefinitionFullName(expected);
            
            Assert.Equal(expected, actual);

            actual = GetPropertyDefinitionFullName<PropertyDefinitionHost>(expected);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ReflectionExtensions_GetPropertyDefinitionFullName_ForMemberWithFullNameInAttribute_ShouldReturnTrimmedFullName()
        {
            var sut = typeof(PropertyDefinitionHost);
            var input = nameof(PropertyDefinitionHost.ShouldReturnFullName);
            var expected = sut.GetMember(input)
                .Single()
                .GetCustomAttribute<PropertyDefinitionAttribute>()
                .FullName
                .Trim();
            
            var actual = sut.GetPropertyDefinitionFullName(input);
            
            Assert.Equal(expected, actual);

            
            actual = GetPropertyDefinitionFullName<PropertyDefinitionHost>(input);

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ReflectionExtensions_GetPropertyDefinitionFullName_ForMemberWithoutPropertyDefinitionAttribute_ShouldThrow()
        {
            var sut = typeof(PropertyDefinitionHost);
            var input = nameof(PropertyDefinitionHost.ShouldThrow);

            var actual = Assert.Throws<ArgumentException>(
                () => sut.GetPropertyDefinitionFullName(input));
            Assert.Contains("Can't find property definition", actual.Message);
            Assert.Contains(input, actual.Message);
            Assert.Contains(sut.FullName, actual.Message);
            
            
            actual = Assert.Throws<ArgumentException>(
                () => GetPropertyDefinitionFullName<PropertyDefinitionHost>(input));
            Assert.Contains("Can't find property definition", actual.Message);
            Assert.Contains(input, actual.Message);
            Assert.Contains(sut.FullName, actual.Message);
        }

        // ReSharper disable InconsistentNaming
        [ObjectDefinition]
        private class ObjectDefinition_ShouldReturnTypeName
        {
        }
        
        [ObjectDefinition(FullName = "  foo   ")]
        private class ObjectDefinition_ShouldReturnObjectDefinitionFullName
        {
        }

        private class ObjectDefinition_ShouldThrow
        {
        }

        private class PropertyDefinitionHost
        {
            [PropertyDefinition]
            public object ShouldReturnPropertyName => "";

            [PropertyDefinition(FullName = "   foo  ")]
            public object ShouldReturnFullName => "";

            public object ShouldThrow => "";
        }
        // ReSharper restore InconsistentNaming
    }
}
