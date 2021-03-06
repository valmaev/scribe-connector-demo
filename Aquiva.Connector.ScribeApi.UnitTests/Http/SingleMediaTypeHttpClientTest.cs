using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Http
{
    public class SingleMediaTypeHttpClientTest : IDisposable
    {
        private SingleMediaTypeHttpClient _sut;
        
        [Theory, AutoData]
        public void SingleMediaTypeHttpClient_AllPublicConstructors_Always_ShouldHaveNullGuards(
            [Frozen] Fixture fixture,
            GuardClauseAssertion assertion)
        {
            fixture.Inject<MediaTypeFormatter>(new DummyMediaTypeFormatter());
            assertion.Verify(typeof(SingleMediaTypeHttpClient).GetConstructors());
        }

        [Fact]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_Always_ShouldIgnoreRequestBodyForGET()
        {
            var stubHandler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _sut = CreateSystemUnderTest(handler: stubHandler);

            await _sut.SendMediaContentAsync<object, object>(
                HttpMethod.Get,
                new Uri("http://foobar"),
                new object());

            var actual = stubHandler.Requests.Single();
            Assert.Null(actual.Content);
        }

        [Fact]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_WithNullResponseContent_ShouldReturnDefaultValueOfTResp()
        {
            var responseWithNullContent = new HttpResponseMessage(HttpStatusCode.OK) {Content = null};
            var stubHandler = new StubbedResponseHandler(responseWithNullContent);
            _sut = CreateSystemUnderTest(handler: stubHandler);

            await _sut.SendMediaContentAsync<object, object>(
                HttpMethod.Get,
                new Uri("http://foobar"),
                new object());

            var actual = stubHandler.Requests.Single();
            Assert.Equal(default(object), actual.Content);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("OPTIONS")]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_WithNotGETMethod_ShouldUseMediaTypeFormatterForRequests(
            string httpMethod)
        {
            var stubHandler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var expected = new DummyMediaTypeFormatter();
            _sut = CreateSystemUnderTest(handler: stubHandler, formatter: expected);

            await _sut.SendMediaContentAsync<object, object>(
                new HttpMethod(httpMethod),
                new Uri("http://foobar"),
                new object());

            var actual = stubHandler.Requests.Single();
            var actualContent = Assert.IsAssignableFrom<ObjectContent>(actual.Content);
            Assert.Same(expected, actualContent.Formatter);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("OPTIONS")]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_Always_ShouldUseMediaTypeFormatterForResponses(
            string httpMethod)
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
                {Content = new StringContent("baz", Encoding.UTF8, "text/plain")};
            var stubHandler = new StubbedResponseHandler(response);

            var expected = Guid.NewGuid().ToString();
            var stubFormatter = new StubbedReadMediaTypeFormatter(
                new[] {new MediaTypeHeaderValue("text/plain")},
                expected);

            _sut = CreateSystemUnderTest(handler: stubHandler, formatter: stubFormatter);

            // Act
            var actual = await _sut.SendMediaContentAsync<object, object>(
                new HttpMethod(httpMethod),
                new Uri("http://foobar"),
                new object());

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(NonSuccessfulStatusCodes))]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_WithFailedResponse_ShouldThrowByDefault(
            HttpStatusCode responseStatusCode)
        {
            var failedResponse = new HttpResponseMessage(responseStatusCode);
            var stubHandler = new StubbedResponseHandler(failedResponse);
            _sut = CreateSystemUnderTest(handler: stubHandler);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _sut.SendMediaContentAsync<object, object>(
                    HttpMethod.Get,
                    new Uri("http://foobar"),
                    new object()));
        }

        public static IEnumerable<object[]> NonSuccessfulStatusCodes()
        {
            return Enum.GetValues(typeof(HttpStatusCode))
                .Cast<int>()
                .Where(c => c < 200 || c > 299)
                .Distinct()
                .Select(c => new object[] {c});
        }

        [Fact]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_WithSuccessfulResponse_ShouldReturnDeserializedRequest()
        {
            // Arrange
            var expected = new Person {Name = "John", Age = 27};
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"{{\"name\": \"{expected.Name}\", \"age\": {expected.Age}}}",
                    Encoding.UTF8,
                    "application/json")
            };
            var stubHandler = new StubbedResponseHandler(response);

            _sut = CreateSystemUnderTest(
                handler: stubHandler,
                formatter: new JsonMediaTypeFormatter());

            // Act
            var actual = await _sut.SendMediaContentAsync<object, Person>(
                HttpMethod.Get,
                new Uri("http://foobar"),
                new object());

            // Assert
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Age, actual.Age);
        }

        [Fact]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_ForNoContentResponseStatusCode_ShouldReturnDefaultValue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent)
                {Content = new StringContent("foobar")};
            var stubHandler = new StubbedResponseHandler(response);

            _sut = CreateSystemUnderTest(handler: stubHandler);

            // Act
            var actual = await _sut.SendMediaContentAsync<object, Person>(
                HttpMethod.Get,
                new Uri("http://foobar"),
                new object());

            // Assert
            Assert.Null(actual);
        }

        [Theory, AutoData]
        public async Task SingleMediaTypeHttpClient_SendMediaContentAsync_Always_ShouldCallResponseAnalyzer(
            HttpStatusCode httpStatusCode)
        {
            var expected = new HttpResponseMessage(httpStatusCode);
            var stub = new StubbedResponseHandler(expected);

            _sut = CreateSystemUnderTest(handler: stub);
            _sut.ResponseAnalyzer = actual => Assert.Equal(expected, actual);

            await _sut.SendMediaContentAsync<object, object>(
                HttpMethod.Get,
                new Uri("http://foobar"),
                new object());
        }

        [Fact]
        public void SingleMediaTypeHttpClient_Dispose_Always_ShouldNotThrow()
        {
            var sut = CreateSystemUnderTest();
            var actual = Record.Exception(() => sut.Dispose());
            Assert.Null(actual);
        }

        private static SingleMediaTypeHttpClient CreateSystemUnderTest(
            HttpMessageHandler handler = null,
            MediaTypeFormatter formatter = null)
        {
            return new SingleMediaTypeHttpClient(
                new HttpClient(
                    handler ?? new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK))),
                formatter ?? new DummyMediaTypeFormatter());
        }

        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public void Dispose() => _sut?.Dispose();
    }
}
