using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Refit;

namespace Movies.API.SDK;

[Headers("Authorization: Bearer")]
public interface IMoviesAPI
{
    [Get(ApiEndpoints.V1.Movies.Get)]
    Task<MovieResponse> GetMovieAsync(string idOrSlug);

    [Get(ApiEndpoints.V1.Movies.GetAll)]
    Task<MoviesResponse> GetAllMoviesAsync(GetAllMoviesRequest request);
}