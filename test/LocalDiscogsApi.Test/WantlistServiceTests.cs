using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Database;
using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using LocalDiscogsApi.Test.Helpers;
using Moq;
using Xunit;
using Discogs = LocalDiscogsApi.Models.Discogs;

namespace LocalDiscogsApi.Test
{
    public class WantlistServiceTests
    {
        private readonly Mock<IDiscogsClient> discogsClientMock;
        private readonly Mock<IDbContext> dbContextMock;

        private readonly WantlistService service;

        public WantlistServiceTests()
        {
            discogsClientMock = new Mock<IDiscogsClient>();
            dbContextMock = new Mock<IDbContext>();

            service = new WantlistService(discogsClientMock.Object, dbContextMock.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Exists_EmptyUserName_ThrowsNullArgumentException(string emptyUsername)
        {
            // Arrange
            // Act
            ArgumentNullException result = await Assert.ThrowsAsync<ArgumentNullException>(() => service.Exists(emptyUsername));

            // Assert
            result.Message.Should().ContainAll("Value cannot be null", "username");
        }

        #region Exists
        [Fact]
        public async Task Exists_UserExists_ReturnsTrue()
        {
            // Arrange
            discogsClientMock
                .Setup(x => x.GetWantlistPageForUser(TestData.FakeSeller1, 1))
                .ReturnsAsync(TestData.WantlistRespone);

            // Act
            bool result = await service.Exists(TestData.FakeSeller1);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task Exists_UserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            discogsClientMock
                .Setup(x => x.GetWantlistPageForUser(TestData.FakeSeller1, 1))
                .ReturnsAsync((Discogs.WantlistResponse)null);

            // Act
            bool result = await service.Exists(TestData.FakeSeller1);

            // Assert
            result.Should().Be(false);
        }
        #endregion

        #region Get
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Get_EmptyUserName_ThrowsNullArgumentException(string emptyUsername)
        {
            // Arrange
            // Act
            ArgumentNullException result = await Assert.ThrowsAsync<ArgumentNullException>(() => service.Get(emptyUsername));

            // Assert
            result.Message.Should().ContainAll("Value cannot be null", "username");
        }

        [Fact]
        public async Task Get_WantlistExistsInDb_ReturnsDbWantlist()
        {
            // Arrange
            UserWantlist expectedWantlist = new UserWantlist();

            dbContextMock
                .Setup(x => x.GetUserWantlist(TestData.FakeUser))
                .ReturnsAsync(expectedWantlist);

            // Act
            UserWantlist result = await service.Get(TestData.FakeUser);

            // Assert
            result.Should().BeEquivalentTo(expectedWantlist);
        }

        [Fact]
        public async Task Get_WantlistDoesNotExistInDb_ReturnsWantlistFromDiscogs()
        {
            // Arrange
            Discogs.Wantlist discogsWantlist = new Discogs.Wantlist(new Discogs.Want[] { TestData.Want1, TestData.Want2 });

            UserWantlist expectedWantlist = new UserWantlist(
                id: "generatedId",
                username: TestData.FakeUser,
                releaseIds: new List<long>() { discogsWantlist[0].ReleaseId, discogsWantlist[1].ReleaseId },
                lastUpdated: DateTimeOffset.UtcNow);

            dbContextMock
                .Setup(x => x.GetUserWantlist(TestData.FakeUser))
                .ReturnsAsync((UserWantlist)null);

            dbContextMock
                .Setup(x => x.SetUserWantlist(It.IsAny<UserWantlist>()))
                .ReturnsAsync(expectedWantlist);

            discogsClientMock
                .Setup(x => x.GetWantlistForUser(TestData.FakeUser))
                .ReturnsAsync(discogsWantlist);

            // Act
            UserWantlist result = await service.Get(TestData.FakeUser);

            // Assert
            result.Should().BeEquivalentTo(expectedWantlist);
        }

        // todo: test for when db is OOD - queue update.
        #endregion

        private static class TestData
        {
            public static Discogs.Listing Listing1 =>
                new ListingBuilder()
                .WithListingId(1001)
                .WithReleaseId(101)
                .WithCondition("Mint (M)", "Mint (M)")
                .WithPrice("GBP", 19.99m)
                .WithSeller(FakeSeller1, "https://img.fake.fake/aaa/1.jpg")
                .WithReleaseDescription("Listing 1 - Listing One [1991]")
                .Build();

            public static Discogs.Listing Listing2 =>
                new ListingBuilder()
                .WithListingId(1002)
                .WithReleaseId(102)
                .WithCondition("Very Good (VG+)", "Near Mint (NM or M-)")
                .WithPrice("GBP", 6.99m)
                .WithSeller(FakeSeller1, "https://img.fake.fake/aaa/2.jpg")
                .WithReleaseDescription("Listing 2 - Listing Two [1992]")
                .Build();

            public static Discogs.Listing Listing3 =>
                new ListingBuilder()
                .WithListingId(1003)
                .WithReleaseId(103)
                .WithCondition("Very Good (VG+)", "Near Mint (NM or M-)")
                .WithPrice("USD", 8.99m)
                .WithSeller(FakeSeller2, "https://img.fake.fake/aaa/3.jpg")
                .WithReleaseDescription("Listing 3 - Listing Three [1993]")
                .Build();

            public static Discogs.Listing Listing4 =>
                new ListingBuilder()
                .WithListingId(1004)
                .WithReleaseId(104)
                .WithCondition("Mint (M)", "Near Mint (NM or M-)")
                .WithPrice("USD", 15.99m)
                .WithSeller(FakeSeller2, "https://img.fake.fake/aaa/4.jpg")
                .WithReleaseDescription("Listing 4 - Listing Four [1994]")
                .Build();

            public static Discogs.Listing Listing5 =>
                new ListingBuilder()
                .WithListingId(1005)
                .WithReleaseId(105)
                .Build();

            public static Discogs.WantlistResponse WantlistRespone =>
                new Discogs.WantlistResponse(
                    new Discogs.Want[] { Want1 },
                    new Discogs.Pagination(1, 1, new Discogs.Urls("nextpage"), 1)
                    );

            public static Discogs.Want Want1 => new Discogs.Want(ReleaseId1, DateTimeOffset.Parse("2019-01-01T01:01:01-07:00"));
            public static Discogs.Want Want2 => new Discogs.Want(ReleaseId2, DateTimeOffset.Parse("2019-02-02T02:02:02-07:00"));
            public static Discogs.Want Want3 => new Discogs.Want(ReleaseId3, DateTimeOffset.Parse("2019-03-03T03:03:03-07:00"));

            public const long ReleaseId1 = 23905811;
            public const long ReleaseId2 = 72644522;
            public const long ReleaseId3 = 69482533;

            public const string FakeUser = "fakeUser123";

            public const string FakeSeller1 = "fakeSeller1";
            public const string FakeSeller2 = "fakeSeller2";
        }
    }
}