using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using PosCore.Data;
using PosCore.Models;
using PosCore.Services;
using PosCore.ViewModels;
using Xunit;

namespace PosCore.Tests.ViewModels;

public class MainViewModelTests
{
    private PosDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var dbContext = new PosDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    [Fact]
    public void AddToCart_ShouldIncreaseQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext();
        var mockApiService = new Mock<IApiService>();
        var settings = Options.Create(new AppSettings());
        var viewModel = new MainViewModel(dbContext, mockApiService.Object, settings);

        var product = new Product { Id = 1, Name = "Test Product", Price = 100 };
        
        // Act
        viewModel.AddToCartCommand.Execute(product); // Add first time
        viewModel.AddToCartCommand.Execute(product); // Add second time

        // Assert
        viewModel.Cart.Should().HaveCount(1);
        viewModel.Cart.First().Quantity.Should().Be(2);
        viewModel.Total.Should().Be(200);
    }
}
