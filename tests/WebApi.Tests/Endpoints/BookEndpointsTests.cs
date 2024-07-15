using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Endpoints;
using WebApi.Entities;
using WebApi.Tests.TestFramework;

namespace WebApi.Tests.Endpoints;

public sealed class BookEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    [Fact]
    public async Task CreateBook_WhenValidRequest_ReturnsCreated()
    {
        // Given
        var client = factory.CreateClient();
        var requestBody = new CreateBookRequestBody("9780345332080", "The Fellowship of the Ring", 527, "J.R.R.", "Tolkien");

        // When
        var response = await client.PostAsJsonAsync("/books/create", requestBody);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var book = await dbContext.Books.FirstOrDefaultAsync(x => x.Isbn == "9780345332080");
        book.Should().NotBeNull();
        book!.Title.Should().Be("The Fellowship of the Ring");
        book.PagesCount.Should().Be(527);
        book.AuthorFirstName.Should().Be("J.R.R.");
        book.AuthorLastName.Should().Be("Tolkien");
    }
    
    [Fact]
    public async Task GetBooks_WhenNoBooks_ReturnsEmptyList()
    {
        // Given
        var client = factory.CreateClient();

        // When
        var response = await client.GetAsync("/books");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetBooks_WhenBooks_ReturnsList()
    {
        // Given
        await SeedBooksAsync(new BookFaker().Generate(10).ToArray());

        var client = factory.CreateClient();

        // When
        var response = await client.GetAsync("/books");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        books.Should().HaveCount(10);
    }
    
    [Fact]
    public async Task GetBook_WhenBookExists_ReturnsOk()
    {
        // Given
        await SeedBooksAsync(new Book
        {
            Isbn = "9788845270758",
            Title = "The Two Towers",
            PagesCount = 352,
            AuthorFirstName = "J.R.R.",
            AuthorLastName = "Tolkien",
        });
        
        var client = factory.CreateClient();

        // When
        var response = await client.GetAsync("/books/9788845270758");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var bookResponse = await response.Content.ReadFromJsonAsync<Book>();
        bookResponse.Should().NotBeNull();
        bookResponse!.Isbn.Should().Be("9788845270758");
        bookResponse.Title.Should().Be("The Two Towers");
        bookResponse.PagesCount.Should().Be(352);
        bookResponse.AuthorFirstName.Should().Be("J.R.R.");
        bookResponse.AuthorLastName.Should().Be("Tolkien");
    }
    
    [Fact]
    public async Task GetBook_WhenBookDoesNotExist_ReturnsNotFound()
    {
        // Given
        var client = factory.CreateClient();

        // When
        var response = await client.GetAsync("/books/9788845270751");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateBook_WhenBookExists_ReturnsCreated()
    {
        // Given
        await SeedBooksAsync(new Book
        {
            Isbn = "9780345296085",
            Title = "Wrong title",
            PagesCount = 1,
            AuthorFirstName = "Unknown",
            AuthorLastName = "Unknown",
        });

        var client = factory.CreateClient();
        var requestBody = new UpdateBookRequestBody("The Return of the King", 416, "J.R.R.", "Tolkien");

        // When
        var response = await client.PutAsJsonAsync("/books/9780345296085", requestBody);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var updatedBook = await dbContext.Books.FirstOrDefaultAsync(x => x.Isbn == "9780345296085");
        updatedBook.Should().NotBeNull();
        updatedBook!.Title.Should().Be("The Return of the King");
        updatedBook.PagesCount.Should().Be(416);
        updatedBook.AuthorFirstName.Should().Be("J.R.R.");
        updatedBook.AuthorLastName.Should().Be("Tolkien");
    }

    [Fact]
    public async Task UpdateBook_WhenBookDoesNotExist_ReturnsNotFound()
    {
        // Given
        var client = factory.CreateClient();
        var requestBody = new UpdateBookRequestBody("The Return of the King", 416, "J.R.R.", "Tolkien");

        // When
        var response = await client.PutAsJsonAsync("/books/9788845270751", requestBody);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_WhenBookExists_ReturnsCreated()
    {
        // Given
        await SeedBooksAsync(new Book
        {
            Isbn = "9780345296085",
            Title = "The Return of the King",
            PagesCount = 416,
            AuthorFirstName = "J.R.R.",
            AuthorLastName = "Tolkien",
        });
        
        var client = factory.CreateClient();

        // When
        var response = await client.DeleteAsync("/books/9780345296085");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var deletedBook = await dbContext.Books.FirstOrDefaultAsync(x => x.Isbn == "9780345296085");
        deletedBook.Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteBook_WhenBookDoesNotExist_ReturnsNotFound()
    {
        // Given
        var client = factory.CreateClient();

        // When
        var response = await client.DeleteAsync("/books/9788845270756");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        dbContext.Database.EnsureDeleted();
    }

    private async Task SeedBooksAsync(params Book[] books)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        await dbContext.Books.AddRangeAsync(books);
        await dbContext.SaveChangesAsync();
    }
}