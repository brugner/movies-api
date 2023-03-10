namespace Movies.API.Auth;

public static class AuthConstants
{
    public static class Policies
    {
        public const string AdminUser = "Admin";
        public const string TrustedMember = "Trusted";
    }

    public static class Claims
    {
        public const string AdminUser = "admin";
        public const string TrustedMember = "trusted_member";
    }
}
