using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class AttributeBasedMetadataProviderTest
    {
        [Theory, AutoData]
        public void AttributeBasedMetadataProvider_AllPublicConstructors_Always_ShouldHaveNullGuards(
            [Frozen] Fixture fixture,
            GuardClauseAssertion assertion)
        {
            fixture.Inject(Assembly.GetExecutingAssembly());
            assertion.Verify(typeof(AttributeBasedMetadataProvider).GetConstructors());
        }

        [Fact]
        public void AttributeBasedMetadataProvider_Always_ShouldImplementIMetadataProvider()
        {
            var sut = CreateSystemUnderTest();
            Assert.IsAssignableFrom<IMetadataProvider>(sut);
        }

        [Fact]
        public void AttributeBasedMetadataProvider_EntityAssembly_Always_ShouldReturnCtorArgument()
        {
            var expected = Assembly.GetExecutingAssembly();
            var sut = CreateSystemUnderTest(entityAssembly: expected);
            Assert.Same(expected, sut.EntityAssembly);
        }

        [Fact]
        public void AttributeBasedMetadataProvider_TypeFilter_Always_ShouldReturnCtorArgument()
        {
            var expected = new Func<Type, bool>(t => true);
            var sut = CreateSystemUnderTest(typeFilter: expected);
            Assert.Equal(expected, sut.TypeFilter);
        }

        [Theory]
        [InlineData(typeof(SupportedActions.ClassWithPropertyDefinitions))]
        public void AttributeBasedMetadataProvider_RetrieveActionDefinitions_Always_ShouldReturnAllUniqueSupportedActionAttributeUsagesAttachedToNonHiddenObjectDefinitions(
            Type seedType)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut.RetrieveActionDefinitions().ToList();

            // Assert
            var expected = sut.EntityAssembly.GetTypes()
                .Where(t => sut.TypeFilter(t) 
                    && t.GetCustomAttributes<ObjectDefinitionAttribute>()
                        .Any(o => !o.Hidden))
                .SelectMany(t => t.GetCustomAttributes<SupportedActionAttribute>())
                .Distinct()
                .ToList();

            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(ObjectDefinitions.BlankFullNameTestData.ShouldUseTypeName_ByDefault), false)]
        [InlineData(typeof(ObjectDefinitions.BlankFullNameTestData.ShouldUseTypeName_ByDefault), true)]
        public void AttibuteBaseMetadataProvider_RetrieveObjectDefinitions_WithBlankFullNameInAttribute_ShouldUseTypeName(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .Select(o => o.FullName)
                .ToList();

            // Assert
            var expected = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .Select(t => t.Name)
                .Distinct()
                .ToList();

            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(ObjectDefinitions.NonBlankFullNameTestData.ShouldUseTrimmedFullName_NonBlankStringCase), false)]
        [InlineData(typeof(ObjectDefinitions.NonBlankFullNameTestData.ShouldUseTrimmedFullName_NonBlankStringCase), true)]
        public void AttibuteBaseMetadataProvider_RetrieveObjectDefinitions_WithNonBlankFullNameInAttribute_ShouldUseTrimmedFullName(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .Select(o => o.FullName)
                .ToList();

            // Assert
            var expected = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetCustomAttributes<ObjectDefinitionAttribute>())
                .Select(o => o.FullName.Trim())
                .Distinct()
                .ToList();

            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.BlankFullNameTestData.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.BlankFullNameTestData.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_WithBlankPropertyFullNames_ShouldUseMemberName(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .Select(p => p.FullName)
                .ToList();

            // Assert
            var expectedProperties = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetProperties())
                .Select(m => m.Name);

            var expectedFields = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetFields())
                .Select(m => m.Name);

            var expected = expectedProperties.Union(expectedFields).ToList();
            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.NonBlankFullNameTestData.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.NonBlankFullNameTestData.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_WithNonBlankPropertyFullNames_ShouldUseTrimmedFullName(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .Select(p => p.FullName)
                .ToList();

            // Assert
            var expected = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetMembers())
                .SelectMany(p => p.GetCustomAttributes<PropertyDefinitionAttribute>())
                .Select(o => o.FullName.Trim())
                .Distinct()
                .ToList();

            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.PrimaryKeysAndNullables.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.PrimaryKeysAndNullables.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForAllPrimaryKeys_IsNullableShouldBeFalsey(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToList();

            // Assert
            Assert.NotEmpty(actual);
            foreach (var a in actual)
            {
                Assert.True(a.IsPrimaryKey);
                Assert.False(a.Nullable);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.NullableForNullableAndReferenceTypesNotPrimaryKeys.PropertyHost), false, true)]
        [InlineData(typeof(PropertyDefinitions.NullableForNullableAndReferenceTypesNotPrimaryKeys.PropertyHost), true, true)]
        [InlineData(typeof(PropertyDefinitions.NullableForValueTypes.PropertyHost), false, false)]
        [InlineData(typeof(PropertyDefinitions.NullableForValueTypes.PropertyHost), true, false)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForAllNullableAndReferenceNotPrimaryKeyProperties_IsNullableShouldBeTruthy(
            Type seedType,
            bool shouldGetRelations,
            bool expected)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToList();

            // Assert
            Assert.NotEmpty(actual);
            foreach (var a in actual)
            {
                Assert.Equal(false, a.IsPrimaryKey);
                Assert.Equal(expected, a.Nullable);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.TypeForReferenceAndValueTypes.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.TypeForReferenceAndValueTypes.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForReferenceAndValueTypes_ShouldUseTypeNameForType(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToDictionary(
                    keySelector: p => p.FullName,
                    elementSelector: p => p.PropertyType);

            // Assert
            var expectedProperties = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetProperties())
                .ToDictionary(
                    keySelector: p => p.Name,
                    elementSelector: p => p.PropertyType.FullName);

            var expectedFields = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetFields())
                .ToDictionary(
                    keySelector: f => f.Name,
                    elementSelector: f => f.FieldType.FullName);

            var expected = expectedProperties.Union(expectedFields).ToList();

            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.TypeForNullableValueTypes.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.TypeForNullableValueTypes.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForNullableValueTypes_ShouldUseTypeNameForType(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToDictionary(
                    keySelector: p => p.FullName,
                    elementSelector: p => p.PropertyType);

            // Assert
            var expectedProperties = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetProperties())
                .ToDictionary(
                    keySelector: p => p.Name,
                    elementSelector: p => p.PropertyType.GetTypeName());

            var expectedFields = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetFields())
                .ToDictionary(
                    keySelector: f => f.Name,
                    elementSelector: f => f.FieldType.GetTypeName());

            var expected = expectedProperties.Union(expectedFields).ToList();

            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.ArraysOfSimpleTypes.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.ArraysOfSimpleTypes.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForArraysOfSimpleTypes_ShouldUseTypeNameForType(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToDictionary(
                    keySelector: p => p.FullName,
                    elementSelector: p => p.PropertyType);

            // Assert
            var expectedProperties = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetProperties())
                .ToDictionary(
                    keySelector: p => p.Name,
                    elementSelector: p => p.PropertyType.GetPropertyDefinitionType().FullName);

            var expectedFields = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetFields())
                .ToDictionary(
                    keySelector: f => f.Name,
                    elementSelector: f => f.FieldType.GetPropertyDefinitionType().FullName);

            var expected = expectedProperties.Union(expectedFields).ToList();

            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.ArraysOfComplexTypes.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.ArraysOfComplexTypes.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForArraysOfComplexTypes_ShouldUseObjectDefinitionFullNameForType(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToDictionary(
                    keySelector: p => p.FullName,
                    elementSelector: p => p.PropertyType);

            // Assert
            var expectedProperties = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetProperties())
                .ToDictionary(
                    keySelector: p => p.Name,
                    elementSelector: p => p.PropertyType.GetPropertyDefinitionType().GetObjectDefinitionFullName());

            var expectedFields = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetFields())
                .ToDictionary(
                    keySelector: f => f.Name,
                    elementSelector: f => f.FieldType.GetPropertyDefinitionType().GetObjectDefinitionFullName());

            var expected = expectedProperties.Union(expectedFields).ToList();

            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.ComplexTypes.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.ComplexTypes.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForComplexTypesNotFromMscorlib_ShouldUseObjectDefinitionFullNameForType(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToDictionary(
                    keySelector: p => p.FullName,
                    elementSelector: p => p.PropertyType);

            // Assert
            var expectedProperties = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetProperties())
                .ToDictionary(
                    keySelector: p => p.Name,
                    elementSelector: p => p.PropertyType.GetPropertyDefinitionType().GetObjectDefinitionFullName());

            var expectedFields = sut.EntityAssembly.GetTypes()
                .Where(sut.TypeFilter)
                .SelectMany(t => t.GetFields())
                .ToDictionary(
                    keySelector: f => f.Name,
                    elementSelector: f => f.FieldType.GetPropertyDefinitionType().GetObjectDefinitionFullName());

            var expected = expectedProperties.Union(expectedFields).ToList();

            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);
            foreach (var e in expected)
            {
                Assert.Contains(e, actual);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.ArraysOfSimpleTypes.PropertyHost), -1, false)]
        [InlineData(typeof(PropertyDefinitions.ArraysOfSimpleTypes.PropertyHost), -1, true)]
        [InlineData(typeof(PropertyDefinitions.ArraysOfComplexTypes.PropertyHost), -1, false)]
        [InlineData(typeof(PropertyDefinitions.ArraysOfComplexTypes.PropertyHost), -1, true)]
        [InlineData(typeof(PropertyDefinitions.TypeForNullableValueTypes.PropertyHost), 1, false)]
        [InlineData(typeof(PropertyDefinitions.TypeForNullableValueTypes.PropertyHost), 1, true)]
        [InlineData(typeof(PropertyDefinitions.ComplexTypes.PropertyHost), 1, false)]
        [InlineData(typeof(PropertyDefinitions.ComplexTypes.PropertyHost), 1, true)]
        [InlineData(typeof(PropertyDefinitions.TypeForReferenceAndValueTypes.PropertyHost), 1, false)]
        [InlineData(typeof(PropertyDefinitions.TypeForReferenceAndValueTypes.PropertyHost), 1, true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_ForArrays_ShouldCorrectlySetMaxOccurs(
            Type seedType,
            int expected,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(true, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions)
                .ToList();

            // Assert
            Assert.NotEmpty(actual);
            foreach (var actualProperty in actual)
            {
                Assert.Equal(expected, actualProperty.MaxOccurs);
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.TypeForNullableValueTypes.PropertyHost), false)]
        [InlineData(typeof(PropertyDefinitions.TypeForNullableValueTypes.PropertyHost), true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_WithFalseyShouldGetProperties_ShouldNotReturnAnyPropertyDefinitions(
            Type seedType,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(false, shouldGetRelations)
                .SelectMany(o => o.PropertyDefinitions);

            // Assert
            Assert.Empty(actual);
        }

        [Theory]
        [InlineData(typeof(SupportedActionFullNames.ClassWithTwoSupportedActions), false, false)]
        [InlineData(typeof(SupportedActionFullNames.ClassWithTwoSupportedActions), false, true)]
        [InlineData(typeof(SupportedActionFullNames.ClassWithTwoSupportedActions), true, false)]
        [InlineData(typeof(SupportedActionFullNames.ClassWithTwoSupportedActions), true, true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinitions_Always_ShouldFillSupportedActionFullNames(
            Type seedType,
            bool shouldGetProperties,
            bool shouldGetRelations)
        {
            // Arrange
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == seedType.Namespace);

            // Act
            var actual = sut
                .RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations)
                .ToDictionary(
                    keySelector: t => t.FullName,
                    elementSelector: t => t.SupportedActionFullNames);

            // Assert
            var expected = sut.EntityAssembly.GetTypes()
                .Where(t => sut.TypeFilter(t)
                    && t.GetCustomAttributes<ObjectDefinitionAttribute>().Any())
                .ToDictionary(
                    keySelector: t => t.Name,
                    elementSelector: t => t.GetCustomAttributes<SupportedActionAttribute>()
                        .Select(a => a.FullName)
                        .ToList());

            Assert.NotEmpty(actual);
            Assert.NotEmpty(expected);
            Assert.Equal(expected.Count, actual.Count);

            foreach (var expectedObject in expected)
            {
                var actualActions = actual[expectedObject.Key];
                Assert.Equal(expectedObject.Value.Count, actualActions.Count);

                foreach (var actualAction in actualActions)
                {
                    Assert.Contains(actualAction, expectedObject.Value);
                }
            }
        }

        [Theory]
        [InlineData(typeof(PropertyDefinitions.BlankFullNameTestData.PropertyHost), false, false)]
        [InlineData(typeof(PropertyDefinitions.BlankFullNameTestData.PropertyHost), false, true)]
        [InlineData(typeof(PropertyDefinitions.BlankFullNameTestData.PropertyHost), true, false)]
        [InlineData(typeof(PropertyDefinitions.BlankFullNameTestData.PropertyHost), true, true)]
        public void AttributeBasedMetadataProvider_RetrieveObjectDefinition_WithBlankPropertyFullName_ShouldReturnObjectDefinitionWithProperties(
            Type expected,
            bool shouldGetProperties,
            bool shouldGetRelations)
        {
            var sut = CreateSystemUnderTest(typeFilter: t => t.Namespace == expected.Namespace);

            // Act
            var actual = sut.RetrieveObjectDefinition(expected.Name, shouldGetProperties, shouldGetRelations);

            // Assert
            Assert.Equal(expected.Name, actual.FullName);
            Assert.Equal(shouldGetProperties, actual.PropertyDefinitions.Any());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AttributeBasedMetadataProvider_RetrieveMethodDefinitions_Always_ShouldThrow(
            bool shouldGetParameters)
        {
            var sut = CreateSystemUnderTest();
            var actual = Assert.Throws<InvalidOperationException>(
                () => sut.RetrieveMethodDefinitions(shouldGetParameters));
            Assert.Contains(
                "Replication Services are not supported",
                actual.Message,
                StringComparison.InvariantCultureIgnoreCase);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AttributeBasedMetadataProvider_RetrieveMethodDefinition_Always_ShouldThrow(
            bool shouldGetParameters)
        {
            var sut = CreateSystemUnderTest();
            var objectName = Guid.NewGuid().ToString();

            var actual = Assert.Throws<InvalidOperationException>(
                () => sut.RetrieveMethodDefinition(objectName, shouldGetParameters));

            Assert.Contains(
                "Replication Services are not supported",
                actual.Message,
                StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void AttributeBasedMetadataProvider_ResetMetadata_Always_ShouldNotThrow()
        {
            var sut = CreateSystemUnderTest();
            var actual = Record.Exception(() => sut.ResetMetadata());
            Assert.Null(actual);
        }

        [Fact]
        public void AttributeBasedMetadataProvider_Dispose_Always_ShouldNotThrow()
        {
            var sut = CreateSystemUnderTest();
            var actual = Record.Exception(() => sut.Dispose());
            Assert.Null(actual);
        }

        private static AttributeBasedMetadataProvider CreateSystemUnderTest(
            Assembly entityAssembly = null,
            Func<Type, bool> typeFilter = null)
        {
            return new AttributeBasedMetadataProvider(
                entityAssembly ?? Assembly.GetExecutingAssembly(),
                typeFilter ?? (type => true));
        }
    }

    namespace ObjectDefinitions.BlankFullNameTestData
    {
        [ObjectDefinition]
        public class ShouldUseTypeName_ByDefault
        {
        }

        [ObjectDefinition(FullName = null)]
        public class ShouldUseTypeName_NullStringCase
        {
        }

        [ObjectDefinition(FullName = "")]
        public class ShouldUseTypeName_EmptyStringCase
        {
        }

        [ObjectDefinition(FullName = "   ")]
        public class ShouldUseTypeName_BlankStringCase
        {
        }
    }

    namespace ObjectDefinitions.NonBlankFullNameTestData
    {
        [ObjectDefinition(FullName = "Foo")]
        public class ShouldUseTrimmedFullName_NonBlankStringCase
        {
        }

        [ObjectDefinition(FullName = "   Bar")]
        public class ShouldUseTrimmedFullName_WhitespacesOnLeftSideCase
        {
        }

        [ObjectDefinition(FullName = "Baz   ")]
        public class ShouldUseTrimmedFullName_WhitespacesOnRightSideCase
        {
        }
    }

    namespace PropertyDefinitions.BlankFullNameTestData
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public object ShouldUseFieldName_ByDefault;

            [PropertyDefinition(FullName = null)]
            public object ShouldUseFieldName_NullStringCase;

            [PropertyDefinition(FullName = "")]
            public object ShouldUseFieldName_EmptyStringCase;

            [PropertyDefinition(FullName = "   ")]
            public object ShouldUseFieldName_BlankStringCase;

            [PropertyDefinition]
            public object ShouldUsePropertyTypeName_ByDefault { get; set; }

            [PropertyDefinition(FullName = null)]
            public object ShouldUsePropertyTypeName_NullStringCase { get; set; }

            [PropertyDefinition(FullName = "")]
            public object ShouldUsePropertyTypeName_EmptyStringCase { get; set; }

            [PropertyDefinition(FullName = "   ")]
            public object ShouldUsePropertyTypeName_BlankStringCase { get; set; }
        }
    }

    namespace PropertyDefinitions.NonBlankFullNameTestData
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition(FullName = "FooField")]
            public object ShouldUseTrimmedFieldName_NonBlankStringCase;

            [PropertyDefinition(FullName = "   BarField")]
            public object ShouldUseTrimmedFieldName_WhitespacesOnLeftSideCase;

            [PropertyDefinition(FullName = "BazField   ")]
            public object ShouldUseTrimmedFieldName_WhitespacesOnRightSideCase;

            [PropertyDefinition(FullName = "FooProperty")]
            public object ShouldUseTrimmedPropertyName_NonBlankStringCase { get; set; }

            [PropertyDefinition(FullName = "   BarProperty")]
            public object ShouldUseTrimmedPropertyName_WhitespacesOnLeftSideCase { get; set; }

            [PropertyDefinition(FullName = "BazProperty   ")]
            public object ShouldUseTrimmedPropertyName_WhitespacesOnRightSideCase { get; set; }
        }
    }

    namespace PropertyDefinitions.PrimaryKeysAndNullables
    {
        [ObjectDefinition]
        public class FieldHost
        {
            [PropertyDefinition(IsPrimaryKey = true)]
            public object ShouldSetIsNullableToFalse_TruthyIsPrimaryKey_DefaultIsNullable;
        }

        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition(IsPrimaryKey = true)]
            public object ShouldSetIsNullableToFalse_TruthyIsPrimaryKey_DefaultIsNullable { get; set; }
        }
    }

    namespace PropertyDefinitions.NullableForNullableAndReferenceTypesNotPrimaryKeys
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public object ShouldSetIsNullableToTrue_ReferenceTypeField_DefaultIsPrimaryKey;

            [PropertyDefinition]
            public IEnumerable<string> ShouldSetIsNullableToTrue_InterfaceTypeField_DefaultIsPrimaryKey;

            [PropertyDefinition]
            public int? ShouldSetIsNullableToTrue_NullableTypeField_DefaultIsPrimaryKey;

            [PropertyDefinition(IsPrimaryKey = false)]
            public object ShouldSetIsNullableToTrue_ReferenceTypeField_FalseyIsPrimaryKey;

            [PropertyDefinition]
            public object ShouldSetIsNullableToTrue_ReferenceTypeProperty { get; set; }

            [PropertyDefinition]
            public int? ShouldSetIsNullableToTrue_NullableTypeProperty { get; set; }

            [PropertyDefinition]
            public IEnumerable<string> ShouldSetIsNullableToTrue_InterfaceTypeProperty { get; set; }

            [PropertyDefinition(IsPrimaryKey = false)]
            public object ShouldSetIsNullableToTrue_ReferenceTypeProperty_FalseyIsPrimaryKey { get; set; }
        }
    }

    namespace PropertyDefinitions.NullableForValueTypes
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public int ShouldSetIsNullableToFalse_StructField;

            [PropertyDefinition]
            public int ShouldSetIsNullableToFalse_StructProperty;

            [PropertyDefinition]
            public KnownActions ShouldSetIsNullableToFalse_EnumField;

            [PropertyDefinition]
            public KnownActions ShouldSetIsNullableToFalse_EnumProperty;
        }
    }

    namespace PropertyDefinitions.TypeForReferenceAndValueTypes
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public object ShouldUseNameAsType_ReferenceTypeField;

            [PropertyDefinition]
            public int ShouldUseNameAsType_ValueTypeField;

            [PropertyDefinition]
            public object ShouldUseNameAsType_ReferenceTypeProperty { get; set; }

            [PropertyDefinition]
            public int ShouldUseNameAsType_ValueTypeProperty { get; set; }
        }
    }

    namespace PropertyDefinitions.TypeForNullableValueTypes
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public int? ShouldUseNameAsType_NullableValueTypeField;

            [PropertyDefinition]
            public KnownActions? ShouldUseNameAsType_NullableEnumField;

            [PropertyDefinition]
            public int? ShouldUseNameAsType_NullableValueTypeProperty { get; set; }

            [PropertyDefinition]
            public KnownActions? ShouldUseNameAsType_NullableEnumProperty { get; set; }
        }
    }

    namespace PropertyDefinitions.ComplexTypes
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public PropertyHost ParentField;

            [PropertyDefinition]
            public ReferenceTypePropertyHost ReferenceTypePropertyHostField;
            
            [PropertyDefinition]
            public ValueTypePropertyHost ValueTypePropertyHostField;
            
            [PropertyDefinition]
            public ValueTypePropertyHost? NullableValueTypePropertyHostField;
            
            [PropertyDefinition]
            public PropertyHost ParentProperty { get; set; }

            [PropertyDefinition]
            public ReferenceTypePropertyHost ReferenceTypePropertyHostProperty { get; set; }
            
            [PropertyDefinition]
            public ValueTypePropertyHost ValueTypePropertyHostProperty { get; set; }
            
            [PropertyDefinition]
            public ValueTypePropertyHost? NullableValueTypePropertyHostProperty { get; set; }
        }

        [ObjectDefinition]
        public class ReferenceTypePropertyHost
        {
        }

        [ObjectDefinition]
        public struct ValueTypePropertyHost
        {
        }
    }

    namespace PropertyDefinitions.ArraysOfSimpleTypes
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public string[] ArrayOfStringsField;
            
            [PropertyDefinition]
            public int[] ArrayOfIntsField;
            
            [PropertyDefinition]
            public IEnumerable<string> EnumerableOfStringField;
            
            [PropertyDefinition]
            public IEnumerable<int> EnumerableOfIntField;
            
            [PropertyDefinition]
            public string[] ArrayOfStringsProperty { get; set; }
            
            [PropertyDefinition]
            public int[] ArrayOfIntsProperty { get; set; }
            
            [PropertyDefinition]
            public IEnumerable<string> EnumerableOfStringProperty { get; set; }
            
            [PropertyDefinition]
            public IEnumerable<int> EnumerableOfIntProperty { get; set; }
        }
    }

    namespace PropertyDefinitions.ArraysOfComplexTypes
    {
        [ObjectDefinition]
        public class PropertyHost
        {
            [PropertyDefinition]
            public PropertyHost[] ArrayOfPropertyHostsField;
            
            [PropertyDefinition]
            public IEnumerable<PropertyHost> EnumerableOfPropertyHostField;
            
            [PropertyDefinition]
            public AnotherPropertyHost[] ArrayOfAnotherPropertyHostsField;
            
            [PropertyDefinition]
            public IEnumerable<AnotherPropertyHost> EnumerableOfAnotherPropertyHostField;
            
            [PropertyDefinition]
            public PropertyHost[] ArrayOfPropertyHostsProperty { get; set; }
            
            [PropertyDefinition]
            public IEnumerable<PropertyHost> EnumerableOfPropertyHostProperty { get; set; }
            
            [PropertyDefinition]
            public AnotherPropertyHost[] ArrayOfAnotherPropertyHostsProperty { get; set; }
            
            [PropertyDefinition]
            public IEnumerable<AnotherPropertyHost> EnumerableOfAnotherPropertyHostProperty { get; set; }
        }

        [ObjectDefinition]
        public class AnotherPropertyHost
        {
        }
    }

    namespace SupportedActionFullNames
    {
        [ObjectDefinition]
        [SupportedAction(KnownActions.Create)]
        [SupportedAction(KnownActions.Delete)]
        public class ClassWithTwoSupportedActions
        {
        }

        [ObjectDefinition]
        [SupportedAction(KnownActions.Create)]
        [SupportedAction(KnownActions.InsertUpdate)]
        [SupportedAction(KnownActions.Update)]
        public struct StructWithThreeSupportedActions
        {
        }

        [ObjectDefinition]
        [SupportedAction(KnownActions.Query)]
        [SupportedAction(KnownActions.UpdateInsert)]
        public interface IInterfaceWithTwoSupportedActions
        {
        }

        [ObjectDefinition]
        public class ClassWithoutSupportedActionAttribute
        {
        }
        
        [SupportedAction(KnownActions.NativeQuery)]
        public class ClassWithSupportedActionAttributeButWithoutObjectDefinitionAttributeShouldBeIgnored
        {
        }

        public class ClassThatShouldBeIgnored
        {
        }
    }

    namespace SupportedActions
    {
        [ObjectDefinition]
        [SupportedAction(knownActionType: KnownActions.Create)]
        [SupportedAction(knownActionType: KnownActions.Delete)]
        public class ClassWithDefaultObjectDefinitionFullName
        {
        }

        [ObjectDefinition(FullName = "ClassWithPropertyDefinitions")]
        public class ClassWithPropertyDefinitions
        {
            [PropertyDefinition]
            public int Age;

            [PropertyDefinition]
            public string Name { get; set; }
        }

        [ObjectDefinition(FullName = "StructWithCustomObjectDefinitionFullName")]
        [SupportedAction(KnownActions.Create)]
        [SupportedAction(KnownActions.InsertUpdate)]
        [SupportedAction(KnownActions.Update)]
        public struct StructWithCustomObjectDefinitionFullName
        {
        }

        [ObjectDefinition(FullName = "AwesomeContact")]
        [SupportedAction(KnownActions.Query)]
        [SupportedAction(KnownActions.UpdateInsert)]
        public interface IInterfaceWithCustomDefinitionFullName
        {
        }

        [ObjectDefinition]
        public class ClassWithoutSupportedActionAttribute
        {
        }
        
        [SupportedAction(KnownActions.NativeQuery)]
        public class ClassWithSupportedActionAttributeButWithoutObjectDefinitionAttributeShouldBeIgnored
        {
        }

        public class ClassThatShouldBeIgnored
        {
        }

        [ObjectDefinition(Hidden = true)]
        [SupportedAction(KnownActions.CreateWith)]
        public class HiddenClassThatShouldBeIgnored
        {
        }
    }
}
