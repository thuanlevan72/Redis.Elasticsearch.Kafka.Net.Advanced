using Microsoft.AspNetCore.Identity;
using System.Net;

namespace AuthServices.Model
{
    public class ApplicationUser: IdentityUser<Guid>
    {
        public string? IPAddress { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
