using Moq;
using System.Net;
using Moq.Protected;

namespace EcoMob.McpServer.Tests.Helpers
{
    public static class MockHelpers
    {
        public static HttpClient MockHttpClient(string responseContent = default!,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent)
                });

            return new HttpClient(handlerMock.Object);
        }
    }
}
