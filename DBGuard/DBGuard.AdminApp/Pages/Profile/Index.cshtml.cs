using DBGuard.DataAccess.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBGuard.AdminApp.Pages.Profile;

[Authorize(Roles = "User,Root")]
public class ProfileModel : PageModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }

    public void OnGet()
    {
        // get from JWT / context
        Username = "admin";
        Email = "admin@mail.com";
        Role = UserRole.Root;
    }
}