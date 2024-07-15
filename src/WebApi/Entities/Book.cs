using System.ComponentModel.DataAnnotations;

namespace WebApi.Entities;

public class Book
{
    [Key]
    public string Isbn { get; set; }
    public string Title { get; set; }
    public int PagesCount { get; set; }
    public string AuthorFirstName { get; set; }
    public string AuthorLastName { get; set; }
}