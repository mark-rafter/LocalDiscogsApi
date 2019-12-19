using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
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
    public class InventoryServiceTests
    {
        private readonly Mock<IDiscogsClient> discogsClientMock;
        private readonly Mock<IDbContext> dbContextMock;
        private readonly Mock<IMapper> mapperMock;

        private readonly InventoryService service;

        public InventoryServiceTests()
        {
            discogsClientMock = new Mock<IDiscogsClient>();
            dbContextMock = new Mock<IDbContext>();
            mapperMock = new Mock<IMapper>();

            service = new InventoryService(discogsClientMock.Object, dbContextMock.Object, mapperMock.Object);
        }

        #region Get
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Get_EmptySellerName_ThrowsNullArgumentException(string emptySellername)
        {
            // Arrange
            // Act
            ArgumentNullException result = await Assert.ThrowsAsync<ArgumentNullException>(() => service.Get(emptySellername));

            // Assert
            result.Message.Should().ContainAll("Value cannot be null");
        }

        [Fact]
        public async Task Get_InventoryExistsInDb_ReturnsDbInventory()
        {
            // Arrange
            SellerInventory expectedInventory = new SellerInventory();

            dbContextMock
                .Setup(x => x.GetSellerInventory(TestData.FakeSeller1))
                .ReturnsAsync(expectedInventory);

            // Act
            SellerInventory result = await service.Get(TestData.FakeSeller1);

            // Assert
            result.Should().BeEquivalentTo(expectedInventory);
        }

        [Fact]
        public async Task Get_InventoryDoesNotExistInDbAndDoesNotExistOnDiscogs_ReturnsNull()
        {
            // Arrange
            dbContextMock
                .Setup(x => x.GetSellerInventory(TestData.FakeSeller1))
                .ReturnsAsync((SellerInventory)null);

            discogsClientMock
                .Setup(x => x.GetInventoryPageForUser(TestData.FakeSeller1, 1))
                .ReturnsAsync((Discogs.InventoryResponse)null);

            // Act
            SellerInventory result = await service.Get(TestData.FakeSeller1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Get_InventoryDoesNotExistInDb_ReturnsInventoryFromDiscogs()
        {
            // Arrange
            Discogs.Inventory discogsInventory = new Discogs.Inventory(TestData.Listing1, TestData.Listing2);

            var expectedInventory = new SellerInventory();

            dbContextMock
                .Setup(x => x.GetSellerInventory(TestData.FakeSeller1))
                .ReturnsAsync((SellerInventory)null);

            dbContextMock
                .Setup(x => x.SetSellerInventory(It.IsAny<SellerInventory>()))
                .ReturnsAsync(expectedInventory);

            discogsClientMock
                .Setup(x => x.GetInventoryPageForUser(TestData.FakeSeller1, 1))
                .ReturnsAsync(TestData.InventoryResponse);

            discogsClientMock
                .Setup(x => x.GetInventoryForUser(TestData.FakeSeller1))
                .ReturnsAsync(discogsInventory);

            mapperMock
                .Setup(x => x.Map<List<SellerListing>>(It.IsAny<Discogs.Inventory>()))
                .Returns(new List<SellerListing>()); // not testing mapping logic here

            // Act
            SellerInventory result = await service.Get(TestData.FakeSeller1);

            // Assert
            result.Should().BeEquivalentTo(expectedInventory);
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

            public static Discogs.InventoryResponse InventoryResponse =>
                new Discogs.InventoryResponse(
                    new Discogs.Listing[] { Listing1 },
                    new Discogs.Pagination(1, 1, new Discogs.Urls("nextpage"), 1)
                    );

            public const long ReleaseId1 = 23905811;
            public const long ReleaseId2 = 72644522;
            public const long ReleaseId3 = 69482533;

            public const string FakeSeller1 = "fakeSeller1";
            public const string FakeSeller2 = "fakeSeller2";
        }
    }
}