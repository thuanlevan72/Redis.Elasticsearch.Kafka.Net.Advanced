

using Microsoft.AspNetCore.Identity;

namespace IdentityServer4.Models
{
    public class ApplicationUser: IdentityUser<Guid>
    {
        // Thông tin cá nhân của người dùng
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string IpLoging {  get; set; }

        // Thông tin về thời gian và trạng thái
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
