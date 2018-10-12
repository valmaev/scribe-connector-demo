using AutoFixture;
using AutoFixture.Xunit2;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class AutoMetaDataAttribute : AutoDataAttribute
    {
        private static IFixture CreateFixture(IFixture fixture)
        {
            fixture.Register<IActionDefinition>(fixture.Create<ActionDefinition>);
            fixture.Register<IObjectDefinition>(fixture.Create<ObjectDefinition>);
            fixture.Register<IPropertyDefinition>(fixture.Create<PropertyDefinition>);
            fixture.Register<IMethodDefinition>(fixture.Create<MethodDefinition>);
            fixture.Register<IRelationshipDefinition>(fixture.Create<RelationshipDefinition>);
            return fixture;
        }

        public AutoMetaDataAttribute()
            : base(() => CreateFixture(new Fixture()))
        {
        }
    }

    public class InlineAutoMetaDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMetaDataAttribute(params object[] values)
            : base(new AutoMetaDataAttribute(), values) { }
    }
}
