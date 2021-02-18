using System.Collections.Generic;

namespace Audiochan.Core.Features.Users.GetCurrentUser
{
    public record CurrentUserViewModel
    {
        public string Id { get; init; }
        public string Username { get; init; }
        public string Email { get; init; }
        public ICollection<string> Roles { get; init; } = new List<string>();
    }
}