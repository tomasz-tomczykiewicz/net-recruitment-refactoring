using Bogus;
using WebApi.Entities;

namespace WebApi.Tests.TestFramework;

public sealed class BookFaker : Faker<Book>
{
    public BookFaker()
    {
        RuleFor(x => x.Isbn, f => f.Random.AlphaNumeric(13));
        RuleFor(x => x.Title, f => f.Lorem.Sentence());
        RuleFor(x => x.PagesCount, f => f.Random.Number(1, 1000));
        RuleFor(x => x.AuthorFirstName, f => f.Name.FirstName());
        RuleFor(x => x.AuthorLastName, f => f.Name.LastName());
    }
}