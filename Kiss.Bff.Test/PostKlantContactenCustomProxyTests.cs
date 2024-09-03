using Moq;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class PostKlantContactenCustomProxyTests
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<GetMedewerkerIdentificatie> _getMedewerkerIdentificatieMock;
        private Mock<IAuthenticationHeaderProvider> _authProviderMock;
        private PostKlantContactenCustomProxy _controller;
        private DefaultHttpContext _httpContext;

        [TestInitialize]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _getMedewerkerIdentificatieMock = new Mock<GetMedewerkerIdentificatie>();
            _authProviderMock = new Mock<IAuthenticationHeaderProvider>();

            _configurationMock.Setup(config => config["KLANTCONTACTEN_BASE_URL"]).Returns("https://fakeurl.com");

            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                var requestUrl = request.RequestUri.ToString();
                HttpResponseMessage response;

                if (requestUrl.Contains("/actoren") && request.Method == HttpMethod.Post)
                {
                    // Mock PostActoren
                    var jsonResponse = @"
                    {
                        ""uuid"": ""095be615-a8ad-4c33-8e9c-c7612fbf6c9f"",
                        ""url"": ""http://example.com"",
                        ""naam"": ""string"",
                        ""soortActor"": ""medewerker"",
                        ""indicatieActief"": true,
                        ""actoridentificator"": {
                            ""objectId"": ""string"",
                            ""codeObjecttype"": ""string"",
                            ""codeRegister"": ""string"",
                            ""codeSoortObjectId"": ""string""
                        }
                    }";

                    response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = new StringContent(jsonResponse)
                    };
                }
                else if (requestUrl.Contains("/klantcontacten") && request.Method == HttpMethod.Post)
                {
                    // Mock PostKlantContact
                    var jsonResponse = @"
                    {
                        ""uuid"": ""095be615-a8ad-4c33-8e9c-c7612fbf6c9f"",
                        ""url"": ""http://example.com"",
                        ""nummer"": ""string"",
                        ""kanaal"": ""string"",
                        ""onderwerp"": ""string"",
                        ""inhoud"": ""string"",
                        ""indicatieContactGelukt"": true,
                        ""taal"": ""str"",
                        ""vertrouwelijk"": true,
                        ""plaatsgevondenOp"": ""2019-08-24T14:15:22Z""
                    }";

                    response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = new StringContent(jsonResponse)
                    };
                }
                else if (requestUrl.Contains("/actoren") && request.Method == HttpMethod.Get)
                {
                    if (requestUrl.Contains("nonexistent@example.com"))
                    {
                        // Mock CheckIfActorExists - Geen Actor gevonden
                        var jsonResponse = @"
                        {
                            ""count"": 0,
                            ""results"": []
                        }";

                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(jsonResponse)
                        };
                    }
                    else
                    {
                        // Mock CheckIfActorExists - Actor gevonden
                        var jsonResponse = @"
                        {
                            ""count"": 1,
                            ""results"": [
                                {
                                    ""uuid"": ""095be615-a8ad-4c33-8e9c-c7612fbf6c9f"",
                                    ""url"": ""http://example.com"",
                                    ""naam"": ""string"",
                                    ""soortActor"": ""medewerker"",
                                    ""indicatieActief"": true,
                                    ""actoridentificator"": {
                                        ""objectId"": ""string"",
                                        ""codeObjecttype"": ""string"",
                                        ""codeRegister"": ""string"",
                                        ""codeSoortObjectId"": ""string""
                                    }
                                }
                            ]
                        }";

                        response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(jsonResponse)
                        };
                    }
                }
                else if (requestUrl.Contains("/actorklantcontacten") && request.Method == HttpMethod.Post)
                {
                    // Mock response LinkActorWithKlantContact
                    var jsonResponse = @"
                    {
                        ""uuid"": ""link-uuid"",
                        ""url"": ""http://example.com"",
                        ""actor"": {
                            ""uuid"": ""095be615-a8ad-4c33-8e9c-c7612fbf6c9f"",
                            ""url"": ""http://example.com""
                        },
                        ""klantcontact"": {
                            ""uuid"": ""095be615-a8ad-4c33-8e9c-c7612fbf6c9f"",
                            ""url"": ""http://example.com""
                        }
                    }";

                    response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = new StringContent(jsonResponse)
                    };
                }
                else
                {
                    // Default response unexpected requests
                    response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent("Not Found")
                    };
                }

                return Task.FromResult(response);
            });

            var httpClient = new HttpClient(clientHandlerStub);

            _controller = new PostKlantContactenCustomProxy(
                _getMedewerkerIdentificatieMock.Object,
                httpClient,
                new Extern.Klantinteracties.KlantinteractiesProxyConfig("", "")
            );

            _httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
                }))
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }


        [TestMethod]
        public async Task CheckIfActorExists_ShouldReturnActorUuid_WhenActorExists()
        {
            // Arrange
            var actorUuid = "095be615-a8ad-4c33-8e9c-c7612fbf6c9f";
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                var jsonResponse = $@"
                {{
                    ""count"": 1,
                    ""results"": [
                        {{
                            ""uuid"": ""{actorUuid}"",
                            ""url"": ""http://example.com"",
                            ""naam"": ""string"",
                            ""soortActor"": ""medewerker"",
                            ""indicatieActief"": true,
                            ""actoridentificator"": {{
                                ""objectId"": ""string"",
                                ""codeObjecttype"": ""string"",
                                ""codeRegister"": ""string"",
                                ""codeSoortObjectId"": ""string""
                            }}
                        }}
                    ]
                }}";

                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                };
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(clientHandlerStub);
            _authProviderMock.Setup(a => a.ApplyAuthorizationHeader(It.IsAny<HttpRequestHeaders>(), _httpContext.User));

            // Act
            var result = await _controller.GetActorId("test@example.com");

            // Assert
            Assert.AreEqual(actorUuid, result);
        }

        [TestMethod]
        public async Task CheckIfActorExists_ShouldReturnNull_WhenActorDoesNotExist()
        {
            // Arrange
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                // Mock response with no actor found
                var jsonResponse = @"
                {
                    ""count"": 0,
                    ""results"": []
                }";

                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                };
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(clientHandlerStub);
            _authProviderMock.Setup(a => a.ApplyAuthorizationHeader(It.IsAny<HttpRequestHeaders>(), _httpContext.User));

            // Act
            var result = await _controller.GetActorId("nonexistent@example.com");

            // Assert
            Assert.IsNull(result);
        }


        [TestMethod]
        public async Task PostKlantContact_ShouldReturnKlantContactUuid_WhenSuccessful()
        {
            // Arrange
            var klantContactUuid = "095be615-a8ad-4c33-8e9c-c7612fbf6c9f";
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                Assert.AreEqual(HttpMethod.Post, request.Method);
                Assert.AreEqual("https://fakeurl.com/klantinteracties/api/v1/klantcontacten", request.RequestUri.ToString());

                var expectedContent = JsonContent.Create(new JsonObject
                {
                    ["nummer"] = "string",
                    ["kanaal"] = "string",
                    ["onderwerp"] = "string",
                    ["inhoud"] = "string",
                    ["indicatieContactGelukt"] = true,
                    ["taal"] = "str",
                    ["vertrouwelijk"] = true,
                    ["plaatsgevondenOp"] = "2019-08-24T14:15:22Z"
                }).ReadAsStringAsync().Result;

                var actualContent = request.Content.ReadAsStringAsync().Result;
                Assert.AreEqual(expectedContent, actualContent);

                var jsonResponse = $@"
                {{
                    ""uuid"": ""{klantContactUuid}"",
                    ""url"": ""http://example.com"",
                    ""nummer"": ""string"",
                    ""kanaal"": ""string"",
                    ""onderwerp"": ""string"",
                    ""inhoud"": ""string"",
                    ""indicatieContactGelukt"": true,
                    ""taal"": ""str"",
                    ""vertrouwelijk"": true,
                    ""plaatsgevondenOp"": ""2019-08-24T14:15:22Z""
                }}";

                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(jsonResponse)
                });
            });

            using var httpClient = new HttpClient(clientHandlerStub);
            _authProviderMock.Setup(a => a.ApplyAuthorizationHeader(It.IsAny<HttpRequestHeaders>(), _httpContext.User));

            var parsedModel = new JsonObject
            {
                ["nummer"] = "string",
                ["kanaal"] = "string",
                ["onderwerp"] = "string",
                ["inhoud"] = "string",
                ["indicatieContactGelukt"] = true,
                ["taal"] = "str",
                ["vertrouwelijk"] = true,
                ["plaatsgevondenOp"] = "2019-08-24T14:15:22Z"
            };

            // Act
            var result = await _controller.PostKlantContact(parsedModel);

            // Assert
            Assert.AreEqual(klantContactUuid, result);
            _authProviderMock.Verify(a => a.ApplyAuthorizationHeader(It.IsAny<HttpRequestHeaders>(), _httpContext.User), Times.Once);
        }


        [TestMethod]
        public async Task PostActoren_ShouldReturnCreatedActorUuid_WhenSuccessful()
        {
            // Arrange
            var actorUuid = "095be615-a8ad-4c33-8e9c-c7612fbf6c9f";
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                var jsonResponse = $@"
                {{
                    ""uuid"": ""{actorUuid}"",
                    ""url"": ""http://example.com"",
                    ""naam"": ""string"",
                    ""soortActor"": ""medewerker"",
                    ""indicatieActief"": true,
                    ""actoridentificator"": {{
                        ""objectId"": ""string"",
                        ""codeObjecttype"": ""string"",
                        ""codeRegister"": ""string"",
                        ""codeSoortObjectId"": ""string""
                    }}
                }}";

                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(jsonResponse)
                };
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(clientHandlerStub);
            _authProviderMock.Setup(a => a.ApplyAuthorizationHeader(It.IsAny<HttpRequestHeaders>(), _httpContext.User));

            // Act
            var result = await _controller.PostActoren();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var jsonResponse = JsonDocument.Parse(okResult.Value.ToString());
            var returnedUuid = jsonResponse.RootElement.GetProperty("uuid").GetString();

            Assert.AreEqual(actorUuid, returnedUuid);
        }


        [TestMethod]
        public void LinkActorWithKlantContact_ShouldReturnOkResult_WhenLinkedSuccessfully()
        {
            // Arrange
            var actorUuid = "actor-uuid";
            var klantcontactUuid = "klantcontact-uuid";
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                var jsonResponse = $@"
                {{
                    ""uuid"": ""link-uuid"",
                    ""url"": ""http://example.com"",
                    ""actor"": {{
                        ""uuid"": ""{actorUuid}"",
                        ""url"": ""http://example.com""
                    }},
                    ""klantcontact"": {{
                        ""uuid"": ""{klantcontactUuid}"",
                        ""url"": ""http://example.com""
                    }}
                }}";

                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(jsonResponse)
                };
                return Task.FromResult(response);
            });

            using var httpClient = new HttpClient(clientHandlerStub);
            _authProviderMock.Setup(a => a.ApplyAuthorizationHeader(It.IsAny<HttpRequestHeaders>(), _httpContext.User));

            // Act
            var result = _controller.LinkActorWithKlantContact(actorUuid, klantcontactUuid);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ProxyResult));
        }

    }

    // Stub voor HttpClient voor mocke responses
    public class DelegatingHandlerStub : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc ?? throw new ArgumentNullException(nameof(handlerFunc));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }
    }
}
