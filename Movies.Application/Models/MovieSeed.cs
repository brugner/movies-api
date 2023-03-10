namespace Movies.Application.Models;

public class MovieSeed
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required int YearOfRelease { get; set; }
    public required List<string> Genres { get; init; } = new();
    public required List<string> Links { get; init; } = new();
}