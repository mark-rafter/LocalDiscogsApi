using FluentAssertions;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Exceptions;
using LocalDiscogsApi.Test.Helpers;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Discogs = LocalDiscogsApi.Models.Discogs;

namespace LocalDiscogsApi.Test
{
    public class DiscogsClientTests
    {
        private readonly Mock<HttpMessageHandler> mockHandler;

        private readonly IDiscogsClient client;

        public DiscogsClientTests()
        {
            mockHandler = new Mock<HttpMessageHandler>();

            HttpClient fakeHttpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri(TestData.ApiBaseAddress)
            };

            client = new DiscogsClient(fakeHttpClient);
        }

        #region GetInventoryForUser

        [Fact]
        public async Task GetInventoryForUser_InventoryHasOnePage_CallsDiscogsGetInventoryEndpointOnce()
        {
            // Arrange
            var expectedUri = new Uri($"{TestData.ApiBaseAddress}/users/{TestData.UserName}/inventory?sort=listed&sort_order=desc&per_page=100&page=1");

            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeInventoryResponseEmpty
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            await client.GetInventoryForUser(TestData.UserName);

            // Assert
            mockHandler.Protected().Verify(
                nameof(HttpClient.SendAsync),
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetInventoryForUser_UserDoesNotExist_ThrowsRestRequestException()
        {
            // Arrange
            var expectedUri = new Uri($"{TestData.ApiBaseAddress}/users/{TestData.UserName}/inventory?sort=listed&sort_order=desc&per_page=100&page=1");

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage r, CancellationToken t) => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = TestData.Fake404,
                    RequestMessage = r
                });

            // Act
            RestRequestException result = await Assert.ThrowsAsync<RestRequestException>(() => client.GetInventoryForUser(TestData.UserName));

            // Assert
            result.Message.Should().StartWith($"GET Request: '{expectedUri}' failed. Status Code: NotFound.");
        }

        [Fact]
        public async Task GetInventoryForUser_UserHasNoInventory_ReturnsEmptyList()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeInventoryResponseEmpty
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            Discogs.Inventory result = await client.GetInventoryForUser(TestData.UserName);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetInventoryForUser_UserHasInventory_ReturnsDeserializedDiscogsGetInventoryEndpoint()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeInventoryResponsePopulated
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var expectedResult = new Discogs.Inventory(TestData.Listing1, TestData.Listing2);

            // Act
            Discogs.Inventory result = await client.GetInventoryForUser(TestData.UserName);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, "the inventory should match the returned JSON");
        }

        #endregion

        #region GetWantlistForUser

        [Fact]
        public async Task GetWantlistForUser_WantlistHasTwoPages_CallsDiscogsGetWantlistEndpointTwice()
        {
            // Arrange
            var httpResponsePage1 = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeWantlistResponsePopulatedPage1
            };

            var httpResponsePage2 = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeWantlistResponsePopulatedPage2
            };

            mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponsePage1)
                .ReturnsAsync(httpResponsePage2);

            // Act
            await client.GetWantlistForUser(TestData.UserName);

            // Assert
            mockHandler.Protected().Verify(
                nameof(HttpClient.SendAsync),
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(TestData.WantsPage1Url)),
                ItExpr.IsAny<CancellationToken>());

            mockHandler.Protected().Verify(
                nameof(HttpClient.SendAsync),
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(TestData.WantsPage2Url)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetWantlistForUser_UserDoesNotExist_ThrowsRestRequestException()
        {
            // Arrange
            var expectedUri = new Uri($"{TestData.ApiBaseAddress}/users/{TestData.UserName}/wants?per_page=100&page=1");

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage r, CancellationToken t) => new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = TestData.Fake404,
                    RequestMessage = r
                });

            // Act
            RestRequestException result = await Assert.ThrowsAsync<RestRequestException>(() => client.GetWantlistForUser(TestData.UserName));

            // Assert
            result.Message.Should().StartWith($"GET Request: '{expectedUri}' failed. Status Code: NotFound.");
        }

        [Fact]
        public async Task GetWantlistForUser_UserHasNoInventory_ReturnsEmptyList()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeWantlistResponseEmpty
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            Discogs.Wantlist result = await client.GetWantlistForUser(TestData.UserName);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWantlistForUser_UserHasInventory_ReturnsDeserializedDiscogsGetWantlistEndpoint()
        {
            // Arrange
            var httpResponsePage1 = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeWantlistResponsePopulatedPage1
            };

            var httpResponsePage2 = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = TestData.FakeWantlistResponsePopulatedPage2
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(TestData.WantsPage1Url)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponsePage1);

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(TestData.WantsPage2Url)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponsePage2);

            var expectedResult = new Discogs.Wantlist(TestData.Want1, TestData.Want2, TestData.Want3);

            // Act
            Discogs.Wantlist result = await client.GetWantlistForUser(TestData.UserName);

            // Assert
            result.Should().BeEquivalentTo(expectedResult, "the wantlist should match the returned JSON");
        }

        #endregion

        private static class TestData
        {
            public const string ApiBaseAddress = "https://api.discogs.fake";
            public const string WwwBaseAddress = "https://www.discogs.fake";

            public const string UserName = "testUser123";
            public const string AvatarUrl = "https://img.discogs.fake/aaA00aA=/500x500/filters:strip_icc():format(jpeg):quality(40)/discogs-avatars/U-111-222.jpeg.jpg";

            public static Discogs.Listing Listing1 =>
                new ListingBuilder()
                .WithListingId(1001)
                .WithReleaseId(101)
                .WithCondition("Mint (M)", "Mint (M)")
                .WithPrice("GBP", 19.99m)
                .WithPosted("2019-07-02T01:56:07-07:00")
                .WithReleaseDescription(@"Roots Manuva - Facety 2:11 (12"", Single)")
                .WithSeller(UserName, AvatarUrl)
                .Build();

            public static Discogs.Listing Listing2 =>
                new ListingBuilder()
                .WithListingId(1002)
                .WithReleaseId(102)
                .WithCondition("Very Good (VG+)", "Near Mint (NM or M-)")
                .WithPrice("GBP", 6.99m)
                .WithPosted("2019-07-02T01:56:07-07:00")
                .WithReleaseDescription(@"The Standells - Live On Tour - 1966 (LP)")
                .WithSeller(UserName, AvatarUrl)
                .Build();

            public const long SellerId = 6969;

            public static Discogs.Want Want1 => new Discogs.Want(201, DateTimeOffset.Parse("2018-06-20T11:05:02-07:00"));
            public static Discogs.Want Want2 => new Discogs.Want(202, DateTimeOffset.Parse("2018-08-01T05:18:32-07:00"));
            public static Discogs.Want Want3 => new Discogs.Want(203, DateTimeOffset.Parse("2019-06-06T12:40:35-07:00"));

            // per_page=100 is hardcoded, however test data behaves as though per_page=2
            public static string WantsPage1Url => $"{ApiBaseAddress}/users/{UserName}/wants?per_page=100&page=1";
            public static string WantsPage2Url => $"{ApiBaseAddress}/users/{UserName}/wants?per_page=100&page=2";

            public static StringContent Fake404 => new StringContent($@"{{
                    ""message"": ""User does not exist or may have been deleted.""
                }}",
                Encoding.UTF8,
                "application/json");

            public static StringContent FakeInventoryResponseEmpty => new StringContent($@"{{
                    ""pagination"": {{
                        ""per_page"": 50,
                        ""items"": 0,
                        ""page"": 1,
                        ""urls"": {{}},
                        ""pages"": 1
                    }},
                    ""listings"": []
                }}",
                Encoding.UTF8,
                "application/json");

            public static StringContent FakeInventoryResponsePopulated => new StringContent($@"{{
                    ""pagination"": {{
                        ""per_page"": 50,
                        ""items"": 2,
                        ""page"": 1,
                        ""urls"": {{}},
                        ""pages"": 1
                    }},
                    ""listings"": [
                        {{
                            ""status"": ""For Sale"",
                            ""ships_from"": ""United Kingdom"",
                            ""original_shipping_price"": {{}},
                            ""price"": {{
                                ""currency"": ""{Listing1.Price.Currency}"",
                                ""value"": {Listing1.Price.Value}
                            }},
                            ""allow_offers"": false,
                            ""uri"": ""{WwwBaseAddress}/sell/item/{Listing1.Id}"",
                            ""comments"": ""unplayed"",
                            ""seller"": {{
                                ""username"": ""{UserName}"",
                                ""stats"": {{
                                    ""rating"": ""99.8"",
                                    ""total"": 614,
                                    ""stars"": 4.5
                                }},
                                ""uid"": {SellerId},
                                ""url"": ""{ApiBaseAddress}/users/{UserName}"",
                                ""html_url"": ""{WwwBaseAddress}/user/{UserName}"",
                                ""shipping"": """",
                                ""payment"": ""PayPal"",
                                ""avatar_url"": ""{AvatarUrl}"",
                                ""resource_url"": ""{ApiBaseAddress}/users/{UserName}"",
                                ""id"": {SellerId}
                            }},
                            ""sleeve_condition"": ""{Listing1.SleeveCondition}"",
                            ""shipping_price"": {{}},
                            ""release"": {{
                                ""description"": ""{Listing1.Release.Description.Replace(@"""", @"\""")/* escape the quotation mark */}"",
                                ""format"": ""12\"", Single"",
                                ""year"": 2015,
                                ""images"": [],
                                ""id"": {Listing1.Release.Id},
                                ""stats"": {{
                                    ""community"": {{
                                        ""in_collection"": 206,
                                        ""in_wantlist"": 40
                                    }}
                                }},
                                ""catalog_number"": ""BD263"",
                                ""artist"": ""Roots Manuva"",
                                ""title"": ""Facety 2:11"",
                                ""resource_url"": ""{ApiBaseAddress}/releases/{Listing1.Release.Id}"",
                                ""thumbnail"": """"
                            }},
                            ""resource_url"": ""{ApiBaseAddress}/marketplace/listings/{Listing1.Id}"",
                            ""audio"": false,
                            ""id"": {Listing1.Id},
                            ""condition"": ""{Listing1.Condition}"",
                            ""posted"": ""{Listing1.Posted.ToString("o")}""
                        }},
                        {{
                            ""status"": ""For Sale"",
                            ""ships_from"": ""United Kingdom"",
                            ""original_shipping_price"": {{}},
                            ""price"": {{
                                ""currency"": ""{Listing2.Price.Currency}"",
                                ""value"": {Listing2.Price.Value}
                            }},
                            ""allow_offers"": false,
                            ""uri"": ""{WwwBaseAddress}/sell/item/{Listing2.Id}"",
                            ""comments"": """",
                            ""seller"": {{
                                ""username"": ""{UserName}"",
                                ""stats"": {{
                                    ""rating"": ""99.8"",
                                    ""total"": 614,
                                    ""stars"": 4.5
                                }},
                                ""uid"": {SellerId},
                                ""url"": ""{ApiBaseAddress}/users/{UserName}"",
                                ""html_url"": ""{WwwBaseAddress}/user/{UserName}"",
                                ""shipping"": """",
                                ""payment"": ""PayPal"",
                                ""avatar_url"": ""{AvatarUrl}"",
                                ""resource_url"": ""{ApiBaseAddress}/users/{UserName}"",
                                ""id"": {SellerId}
                            }},
                            ""sleeve_condition"": ""{Listing2.SleeveCondition}"",
                            ""shipping_price"": {{}},
                            ""release"": {{
                                ""description"": ""{Listing2.Release.Description}"",
                                ""format"": ""LP"",
                                ""year"": 2015,
                                ""images"": [],
                                ""id"": {Listing2.Release.Id},
                                ""stats"": {{
                                    ""community"": {{
                                        ""in_collection"": 54,
                                        ""in_wantlist"": 23
                                    }}
                                }},
                                ""catalog_number"": ""LP-5472"",
                                ""artist"": ""The Standells"",
                                ""title"": ""Live On Tour - 1966"",
                                ""resource_url"": ""{ApiBaseAddress}/releases/{Listing2.Release.Id}"",
                                ""thumbnail"": """"
                            }},
                            ""resource_url"": ""{ApiBaseAddress}/marketplace/listings/{Listing2.Id}"",
                            ""audio"": false,
                            ""id"": {Listing2.Id},
                            ""condition"": ""{Listing2.Condition}"",
                            ""posted"": ""{Listing2.Posted.ToString("o")}""
                        }}
                    ]
                }}",
                Encoding.UTF8,
                "application/json");

            public static StringContent FakeWantlistResponseEmpty => new StringContent($@"{{
                    ""pagination"": {{
                        ""per_page"": 50,
                        ""items"": 0,
                        ""page"": 1,
                        ""urls"": {{}},
                        ""pages"": 1
                    }},
                    ""wants"": []
                }}",
                Encoding.UTF8,
                "application/json");

            public static StringContent FakeWantlistResponsePopulatedPage1 => new StringContent($@"{{
                ""wants"": [
                    {{
                        ""rating"": 0,
                        ""resource_url"": ""{ApiBaseAddress}/users/{UserName}/wants/{Want1.ReleaseId}"",
                        ""basic_information"": {{
                            ""labels"": [
                                {{
                                    ""name"": ""10 Records"",
                                    ""entity_type"": ""1"",
                                    ""catno"": ""TENXDJ 408"",
                                    ""resource_url"": ""{ApiBaseAddress}/labels/902"",
                                    ""id"": 902,
                                    ""entity_type_name"": ""Label""
                                }}
                            ],
                            ""year"": 1992,
                            ""master_url"": ""{ApiBaseAddress}/masters/7436436"",
                            ""artists"": [
                                {{
                                    ""join"": """",
                                    ""name"": ""Inner City"",
                                    ""anv"": """",
                                    ""tracks"": """",
                                    ""role"": """",
                                    ""resource_url"": ""{ApiBaseAddress}/artists/3868"",
                                    ""id"": 3868
                                }}
                            ],
                            ""id"": {Want1.ReleaseId},
                            ""thumb"": """",
                            ""title"": ""Praise"",
                            ""formats"": [
                                {{
                                    ""descriptions"": [
                                        ""12\"""",
                                        ""Promo"",
                                        ""33 ⅓ RPM""
                                    ],
                                    ""name"": ""Vinyl"",
                                    ""qty"": ""1""
                                }}
                            ],
                            ""cover_image"": """",
                            ""resource_url"": ""{ApiBaseAddress}/releases/{Want1.ReleaseId}"",
                            ""master_id"": 7436436
                        }},
                        ""id"": {Want1.ReleaseId},
                        ""date_added"": ""{Want1.DateAdded.ToString("o")}""
                    }},
                    {{
                        ""rating"": 0,
                        ""resource_url"": ""{ApiBaseAddress}/users/{UserName}/wants/{Want2.ReleaseId}"",
                        ""basic_information"": {{
                            ""labels"": [
                                {{
                                    ""name"": ""7 Days Ent."",
                                    ""entity_type"": ""1"",
                                    ""catno"": ""7DAYSGN 1006"",
                                    ""resource_url"": ""{ApiBaseAddress}/labels/245458"",
                                    ""id"": 245458,
                                    ""entity_type_name"": ""Label""
                                }}
                            ],
                            ""year"": 2018,
                            ""master_url"": null,
                            ""artists"": [
                                {{
                                    ""join"": """",
                                    ""name"": ""Generation Next (2)"",
                                    ""anv"": """",
                                    ""tracks"": """",
                                    ""role"": """",
                                    ""resource_url"": ""{ApiBaseAddress}/artists/3016794"",
                                    ""id"": 3016794
                                }}
                            ],
                            ""id"": {Want2.ReleaseId},
                            ""thumb"": """",
                            ""title"": ""Phoenix "",
                            ""formats"": [
                                {{
                                    ""descriptions"": [
                                        ""12\"""",
                                        ""33 ⅓ RPM""
                                    ],
                                    ""name"": ""Vinyl"",
                                    ""qty"": ""1""
                                }}
                            ],
                            ""cover_image"": """",
                            ""resource_url"": ""{ApiBaseAddress}/releases/{Want2.ReleaseId}"",
                            ""master_id"": null
                        }},
                        ""id"": {Want2.ReleaseId},
                        ""date_added"": ""{Want2.DateAdded.ToString("o")}""
                    }}
                ],
                ""pagination"": {{
                    ""per_page"": 2,
                    ""items"": 3,
                    ""page"": 1,
                    ""urls"": {{
                        ""next"": ""{WantsPage2Url}""
                    }},
                    ""pages"": 2
                }}
            }}",
            Encoding.UTF8,
            "application/json");

            public static StringContent FakeWantlistResponsePopulatedPage2 => new StringContent($@"{{
                ""wants"": [
                    {{
	                    ""rating"": 0,
	                    ""resource_url"": ""{ApiBaseAddress}/users/{UserName}/wants/{Want3.ReleaseId}"",
	                    ""basic_information"": {{
		                    ""labels"": [
			                    {{
				                    ""name"": ""Relative"",
				                    ""entity_type"": ""1"",
				                    ""catno"": ""RTV 001.1"",
				                    ""resource_url"": ""{ApiBaseAddress}/labels/188982"",
				                    ""id"": 188982,
				                    ""entity_type_name"": ""Label""
			                    }}
		                    ],
		                    ""year"": 2015,
		                    ""master_url"": ""{ApiBaseAddress}/masters/859121"",
		                    ""artists"": [
			                    {{
				                    ""join"": ""/"",
				                    ""name"": ""John Swing"",
				                    ""anv"": """",
				                    ""tracks"": """",
				                    ""role"": """",
				                    ""resource_url"": ""{ApiBaseAddress}/artists/1716110"",
				                    ""id"": 1716110
			                    }},
			                    {{
				                    ""join"": ""/"",
				                    ""name"": ""EMG"",
				                    ""anv"": """",
				                    ""tracks"": """",
				                    ""role"": """",
				                    ""resource_url"": ""{ApiBaseAddress}/artists/1140542"",
				                    ""id"": 1140542
			                    }},
			                    {{
				                    ""join"": """",
				                    ""name"": ""Vinalog"",
				                    ""anv"": """",
				                    ""tracks"": """",
				                    ""role"": """",
				                    ""resource_url"": ""{ApiBaseAddress}/artists/1770925"",
				                    ""id"": 1770925
			                    }}
		                    ],
		                    ""id"": {Want3.ReleaseId},
		                    ""thumb"": """",
		                    ""title"": ""Relative 001.1"",
		                    ""formats"": [
			                    {{
				                    ""descriptions"": [
					                    ""12\"""",
					                    ""33 ⅓ RPM""
				                    ],
				                    ""name"": ""Vinyl"",
				                    ""qty"": ""1""
			                    }}
		                    ],
		                    ""cover_image"": """",
		                    ""resource_url"": ""{ApiBaseAddress}/releases/{Want3.ReleaseId}"",
		                    ""master_id"": 859121
	                    }},
	                    ""id"": {Want3.ReleaseId},
	                    ""date_added"": ""{Want3.DateAdded.ToString("o")}""
                    }}
                ],
                ""pagination"": {{
                    ""per_page"": 2,
                    ""items"": 3,
                    ""page"": 2,
                    ""urls"": {{
                        ""prev"": ""{WantsPage1Url}""
                    }},
                    ""pages"": 2
                }}
            }}",
            Encoding.UTF8,
            "application/json");
        }
    }
}
