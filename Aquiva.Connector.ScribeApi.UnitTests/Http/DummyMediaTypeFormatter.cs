using System;
using System.Net.Http.Formatting;

namespace Aquiva.Connector.ScribeApi.Http
{
    public class DummyMediaTypeFormatter : MediaTypeFormatter
    {
        public override bool CanReadType(Type type) => true;
        public override bool CanWriteType(Type type) => true;
    }
}
