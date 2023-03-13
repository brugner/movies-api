using Microsoft.Extensions.DependencyInjection;
using Movies.API.SDK;
using Movies.Contracts.Requests;
using Refit;
using System.Text.Json;

var services = new ServiceCollection();

services.AddRefitClient<IMoviesAPI>(x => new RefitSettings
{
    AuthorizationHeaderValueGetter = () => Task.FromResult("jwt token")
})
    .ConfigureHttpClient(x => x.BaseAddress = new Uri("https://localhost:5001"));

var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesAPI>();

var request = new GetAllMoviesRequest
{
    Title = null,
    YearOfRelease = null,
    SortBy = null,
    Page = 1,
    PageSize = 3
};

var movies = await moviesApi.GetAllMoviesAsync(request);

foreach (var movie in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(movie));
}

Console.ReadLine();