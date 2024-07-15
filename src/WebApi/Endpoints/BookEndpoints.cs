using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApi.Entities;

namespace WebApi.Endpoints;

public static class BookCreateEndpoint
{
    public static void UseBookEndpoints(this WebApplication app)
    {
        app.MapPost("/books/create", (
                [FromBody] CreateBookRequestBody requestBody,
                [FromServices] LibraryDbContext dbContext
            ) =>
            {
                var book = new Book
                {
                    Isbn = requestBody.Isbn,
                    Title = requestBody.Title,
                    PagesCount = requestBody.PagesCount,
                    AuthorFirstName = requestBody.AuthorFirstName,
                    AuthorLastName = requestBody.AuthorLastName,
                };
                
                dbContext.Books.Add(book);
                dbContext.SaveChanges();

                return TypedResults.Created();
            })
            .WithName("CreateBook")
            .WithOpenApi();
        
        app.MapGet("/books", ([
                FromServices] LibraryDbContext dbContext
            ) =>
            {
                return dbContext.Books.ToList();
            })
            .WithName("GetBooks")
            .WithOpenApi();
        
        app.MapGet("/books/{isbn}", Results<Ok<Book>, NotFound> (
                [FromRoute] string isbn,
                [FromServices] LibraryDbContext dbContext
            ) =>
            {
                var books = dbContext.Books.ToList();
                var book = books.SingleOrDefault(x => x.Isbn == isbn);
                
                return book is not null ? TypedResults.Ok(book) : TypedResults.NotFound();
            })
            .WithName("GetBook")
            .WithOpenApi();
        
        app.MapPut("/books/{isbn}", Results<Created, NotFound> (
                [FromRoute] string isbn,
                [FromBody] UpdateBookRequestBody requestBody,
                [FromServices] LibraryDbContext dbContext
            ) =>
            {
                var book = dbContext.Books.FirstOrDefault(x => x.Isbn == isbn);
                if (book is null)
                {
                    return TypedResults.NotFound();
                }
                
                book.Title = requestBody.Title;
                book.PagesCount = requestBody.PagesCount;
                book.AuthorFirstName = requestBody.AuthorFirstName;
                book.AuthorLastName = requestBody.AuthorLastName;
                dbContext.SaveChanges();

                return TypedResults.Created();
            })
            .WithName("UpdateBooks")
            .WithOpenApi();
        
        app.MapDelete("/books/{isbn}", Results<Created, NotFound> (
                [FromRoute] string isbn,
                [FromServices] LibraryDbContext dbContext
            ) =>
            {
                var book = dbContext.Books.FirstOrDefault(x => x.Isbn == isbn);
                if (book is null)
                {
                    return TypedResults.NotFound();
                }
                
                dbContext.Books.Remove(book);
                dbContext.SaveChanges();

                return TypedResults.Created();
            })
            .WithName("DeleteBooks")
            .WithOpenApi();
    }
}

public record CreateBookRequestBody(string Isbn, string Title, int PagesCount, string AuthorFirstName, string AuthorLastName);

public record UpdateBookRequestBody(string Title, int PagesCount, string AuthorFirstName, string AuthorLastName);
