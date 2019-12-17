using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LocalDiscogsApi.Clients;
using LocalDiscogsApi.Database;
using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
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

        #region Exists

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

        [Fact]
        public async Task Exists_UserExistsInDb_ReturnsTrue()
        {
            // Arrange
            dbContextMock
                .Setup(x => x.GetUserWantlist(TestData.FakeUser))
                .ReturnsAsync(new UserWantlist());

            // Act
            bool result = await service.Exists(TestData.FakeUser);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task Exists_UserExistsInDiscogs_ReturnsTrue()
        {
            // Arrange
            dbContextMock
                .Setup(x => x.GetUserWantlist(TestData.FakeUser))
                .ReturnsAsync((UserWantlist)null);

            discogsClientMock
                .Setup(x => x.GetWantlistPageForUser(TestData.FakeUser, 1))
                .ReturnsAsync(TestData.WantlistRespone);

            // Act
            bool result = await service.Exists(TestData.FakeUser);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public async Task Exists_UserDoesNotExistInDbOrDiscogs_ReturnsFalse()
        {
            // Arrange
            dbContextMock
                .Setup(x => x.GetUserWantlist(TestData.FakeUser))
                .ReturnsAsync((UserWantlist)null);

            discogsClientMock
                .Setup(x => x.GetWantlistPageForUser(TestData.FakeUser, 1))
                .ReturnsAsync((Discogs.WantlistResponse)null);

            // Act
            bool result = await service.Exists(TestData.FakeUser);

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
            Discogs.Wantlist discogsWantlist = new Discogs.Wantlist(TestData.Want1, TestData.Want2);

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
        }
    }
}