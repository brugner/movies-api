namespace Movies.Identity.API;

public class TokenGenerationRequest
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = default!;

    public Dictionary<string, object> CustomClaims { get; set; } = new();
}
