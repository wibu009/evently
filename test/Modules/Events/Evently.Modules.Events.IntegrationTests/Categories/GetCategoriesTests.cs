using Evently.Common.Domain;
using Evently.Modules.Events.Application.Categories.GetCategories;
using Evently.Modules.Events.Application.Categories.GetCategory;
using Evently.Modules.Events.IntegrationTests.Abstractions;
using FluentAssertions;

namespace Evently.Modules.Events.IntegrationTests.Categories;

public class GetCategoriesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnEmptyCollection_WhenNoCategoriesExist()
    {
        // Arrange
        await CleanDatabaseAsync();

        var query = new GetCategoriesQuery();

        // Act
        Result<IReadOnlyCollection<CategoryResponse>> result = await Sender.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCategory_WhenCategoryExists()
    {
        // Arrange
        await CleanDatabaseAsync();

        Result<Guid> categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Result<Guid> categoryId2 = await Sender.CreateCategoryAsync(Faker.Music.Genre());

        var query = new GetCategoriesQuery();

        // Act
        Result<IReadOnlyCollection<CategoryResponse>> result = await Sender.Send(query);

        // Assert
        categoryId.IsSuccess.Should().BeTrue();
        categoryId2.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
