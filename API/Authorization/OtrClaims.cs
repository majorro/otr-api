using System.Security.Claims;

namespace API.Authorization;

/// <summary>
/// String values that represent claims encoded into JWTs produced for authentication
/// </summary>
/// <remarks>
/// Functionally these serve as custom <see cref="ClaimTypes"/> consumed by the API
/// </remarks>
public static class OtrClaims
{
    /// <summary>
    /// Claim that describes a permission granted to a subject
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// Valid values for the <see cref="OtrClaims.Role"/> claim
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Role for all users
        /// </summary>
        public const string User = "user";

        /// <summary>
        /// Role for all clients
        /// </summary>
        public const string Client = "client";

        /// <summary>
        /// Role for internal privileged clients
        /// </summary>
        /// <example>o!TR processor</example>
        public const string System = "system";

        /// <summary>
        /// Role for privileged users
        /// </summary>
        public const string Admin = "admin";

        /// <summary>
        /// Role for users with permission to verify matches
        /// </summary>
        public const string Verifier = "verifier";

        // TODO: Convert this to work in the inverse
        // Instead of granting all users "submit", we can grant restricted users "restricted" which would
        // flag the submission flow to check the user for potential submission restriction
        /// <summary>
        /// Role for users with permission to submit matches
        /// </summary>
        public const string Submitter = "submit";

        /// <summary>
        /// Role for users and clients to restrict api access
        /// </summary>
        public const string Whitelist = "whitelist";

        /// <summary>
        /// Collection of all valid <see cref="Roles"/>
        /// </summary>
        public static readonly string[] ValidRoles =
        [
            User,
            Client,
            System,
            Admin,
            Verifier,
            Submitter,
            Whitelist
        ];

        /// <summary>
        /// Collection of <see cref="Roles"/> assignable to <see cref="User"/>s
        /// </summary>
        /// <remarks>
        /// The <see cref="User"/> role itself is not considered assignable because it is only used in the JWT
        /// as an identifier and is not saved in the database in <see cref="Database.Entities.User.Scopes"/>
        /// </remarks>
        public static readonly string[] UserAssignableRoles =
        [
            Admin,
            Verifier,
            Submitter,
            Whitelist
        ];

        /// <summary>
        /// Denotes the given role is assignable to a user
        /// </summary>
        public static bool IsUserAssignableRole(string role) =>
            UserAssignableRoles.Contains(role);

        /// <summary>
        /// Collection of <see cref="Roles"/> assignable to <see cref="Client"/>s
        /// </summary>
        /// <remarks>
        /// The <see cref="Client"/> role itself is not considered assignable because it is only used in the JWT
        /// as an identifier and is not saved in the database in <see cref="Database.Entities.OAuthClient.Scopes"/>
        /// </remarks>
        public static readonly string[] ClientAssignableRoles =
        [
            System,
            Whitelist
        ];

        /// <summary>
        /// Denotes the given role is assignable to a client
        /// </summary>
        public static bool IsClientAssignableRole(string role) =>
            ClientAssignableRoles.Contains(role);

        /// <summary>
        /// Denotes the given role is valid
        /// </summary>
        public static bool IsValidRole(string role) =>
            ValidRoles.Contains(role);
    }

    /// <summary>
    /// Claim that describes the functional type of the JWT
    /// </summary>
    public const string TokenType = "token-typ";

    /// <summary>
    /// Valid values for the <see cref="OtrClaims.TokenType"/> claim
    /// </summary>
    public static class TokenTypes
    {
        /// <summary>
        /// Denotes the JWT as being an access token
        /// </summary>
        public const string AccessToken = "access";

        /// <summary>
        /// Denotes the JWT as being a refresh token
        /// </summary>
        public const string RefreshToken = "refresh";
    }

    /// <summary>
    /// Claim that describes the subject of the JWT
    /// </summary>
    /// <remarks>
    /// In our case, this is the id of the <see cref="Database.Entities.User"/>
    /// or <see cref="Database.Entities.OAuthClient"/>
    /// </remarks>
    public const string Subject = "sub";

    /// <summary>
    /// Claim encoded into all JWTs to induce randomness in the payload
    /// </summary>
    public const string Instance = "inst";

    /// <summary>
    /// Claim that describes an override to the default rate limit
    /// </summary>
    public const string RateLimitOverrides = "rlo";
}
