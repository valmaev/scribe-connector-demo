using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Aquiva.Connector.ScribeApi.Http
{
    public class StubbedReadMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly object _returnFromRead;

        public StubbedReadMediaTypeFormatter(
            IReadOnlyCollection<MediaTypeHeaderValue> supportedMediaTypes,
            object returnFromRead)
        {
            _returnFromRead = returnFromRead;
            foreach (var type in supportedMediaTypes)
                SupportedMediaTypes.Add(type);
        }

        public override bool CanReadType(Type type) => true;
        public override bool CanWriteType(Type type) => true;

        public override Task<object> ReadFromStreamAsync(
            Type type,
            Stream readStream,
            HttpContent content,
            IFormatterLogger formatterLogger,
            CancellationToken cancellationToken) => Task.FromResult(_returnFromRead);
    }
}
