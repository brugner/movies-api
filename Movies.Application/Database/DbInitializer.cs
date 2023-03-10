using Dapper;
using Movies.Application.Models;
using System.Text.Json;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("""
                create table if not exists movies (
                id UUID primary key,
                title TEXT not null,
                slug TEXT not null,
                yearofrelease integer not null);
            """);

        await connection.ExecuteAsync("""
                create unique index concurrently if not exists movies_slug_idx
                on movies
                using btree(slug);
            """);

        await connection.ExecuteAsync("""
                create table if not exists genres (
                movieId UUID references movies (id),
                name TEXT not null);
            """);

        await connection.ExecuteAsync("""
                create table if not exists ratings (
                userid UUID,
                movieid UUID references movies (id),
                rating integer not null,
                primary key (userid, movieid));
            """);
    }

    public async Task SeedAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>("select count(*) from movies");

        if (count > 0)
        {
            return;
        }

        var fileName = $"{Directory.GetCurrentDirectory()}\\..\\movies.json";
        var text = File.ReadAllText(fileName);
        var movies = JsonSerializer.Deserialize<List<MovieSeed>>(text) ?? new();

        foreach (var movie in movies)
        {
            var movieExists = await connection.ExecuteScalarAsync<bool>("select exists(select 1 from movies where slug = @Slug)", new { movie.Slug });

            if (movieExists)
            {
                continue;
            }

            await connection.ExecuteAsync(new CommandDefinition("""
                insert into movies (id, title, slug, yearofrelease)
                values (@Id, @Title, @Slug, @YearOfRelease)
                """, movie));

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    insert into genres (movieId, name)
                    values (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre }));
            }
        }
    }
}