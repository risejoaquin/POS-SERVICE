using FluentAssertions;
using PosCore.Models;
using Xunit;

namespace PosCore.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Product_ShouldHaveDefaultStock_WhenCreated()
    {
        var product = new Product();
        product.StockQuantity.Should().Be(0);
    }
    
    [Fact]
    public void Product_Price_ShouldBePositive()
    {
        var product = new Product { Price = 15.50m };
        product.Price.Should().BeGreaterThan(0);
    }
}
