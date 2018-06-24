using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using KatlaSport.DataAccess;
using KatlaSport.DataAccess.ProductStoreHive;
using KatlaSport.Services.HiveManagement;
using Moq;
using Xunit;

namespace KatlaSport.Services.Tests.HiveManagement
{
    public class HiveServiceTests
    {
        [Theory]
        [AutoMoqData]
        public async Task GetHivesTestAsync([Frozen] Mock<IProductStoreHiveContext> context, HiveService service, IFixture fixture)
        {
            IList<StoreHive> hives = fixture.CreateMany<StoreHive>(10).ToList();
            context.Setup(c => c.Hives).ReturnsEntitySet(hives);
            var amount = await service.GetHivesAsync();
            amount.Should().HaveSameCount(hives);
        }

        [Fact]
        public async Task GetHivesTestAsync_UsingDataDouble()
        {
            var data = new List<StoreHive>
            {
                new StoreHive { Name = "Hive1" },
                new StoreHive { Name = "Hive2" },
                new StoreHive { Name = "Hive3" },
            }.AsQueryable();

            var mockSet = new Mock<IEntitySet<StoreHive>>();
            mockSet.As<IQueryable<StoreHive>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<StoreHive>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<StoreHive>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<StoreHive>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<IProductStoreHiveContext>();
            mockContext.Setup(c => c.Hives).Returns(mockSet.Object);

            Mock<IUserContext> userContext = new Mock<IUserContext>();
            var service = new HiveService(mockContext.Object, userContext.Object);
            var hives = await service.GetHivesAsync();

            Assert.Equal(3, hives.Count());
            Assert.Equal("Hive1", hives[0].Name);
            Assert.Equal("Hive2", hives[1].Name);
            Assert.Equal("Hive3", hives[2].Name);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveTestAsync([Frozen] Mock<IProductStoreHiveContext> context, HiveService service, IFixture fixture)
        {
            IList<StoreHive> hives = fixture.CreateMany<StoreHive>(10).ToList();
            context.Setup(c => c.Hives).ReturnsEntitySet(hives);
            var createRequest = fixture.Create<UpdateHiveRequest>();
            var newHive = service.CreateHiveAsync(createRequest).Result;
            Assert.NotNull(newHive);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveTestAsync_AcceptsMatchCodeAndMismatchId_ThrowsException([Frozen] Mock<IProductStoreHiveContext> context, HiveService service, IFixture fixture)
        {
            var hiveId = 1;
            var updateRequest = fixture.Create<UpdateHiveRequest>();
            var hives = fixture.CreateMany<StoreHive>(1).ToList();
            hives[0].Id = 2;
            hives[0].Code = updateRequest.Code;
            context.Setup(s => s.Hives).ReturnsEntitySet(hives);
            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => service.UpdateHiveAsync(hiveId, updateRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveTestAsync_ReturnsUpdatedHive([Frozen] Mock<IProductStoreHiveContext> context, HiveService service, IFixture fixture)
        {
            var hiveId = 1;
            var updateRequest = fixture.Create<UpdateHiveRequest>();
            var hives = fixture.CreateMany<StoreHive>(1).ToList();
            hives[0].Id = hiveId;
            hives[0].Code = updateRequest.Code + "new code info";
            context.Setup(s => s.Hives).ReturnsEntitySet(hives);
            var updateHive = await service.UpdateHiveAsync(hiveId, updateRequest);
            Assert.Equal(updateRequest.Name, updateHive.Name);
            Assert.Equal(updateRequest.Code, updateHive.Code);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveTestAsync_AcceptsWrongId_ThrowsException([Frozen] Mock<IProductStoreHiveContext> context, HiveService service, IFixture fixture)
        {
            var hiveId = 1;
            var hives = fixture.CreateMany<StoreHive>(1).ToList();
            hives[0].Id = 2;
            context.Setup(s => s.Hives).ReturnsEntitySet(hives);
            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => service.DeleteHiveAsync(hiveId));
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveTestAsync_WithWrongStatus_ThrowsException([Frozen] Mock<IProductStoreHiveContext> context, HiveService service, IFixture fixture)
        {
            var hiveId = 1;
            var hives = fixture.CreateMany<StoreHive>(1).ToList();
            hives[0].Id = hiveId;
            hives[0].IsDeleted = false;
            context.Setup(s => s.Hives).ReturnsEntitySet(hives);
            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(() => service.DeleteHiveAsync(hiveId));
        }
    }
}
