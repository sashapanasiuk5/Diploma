using System.Security.Claims;
using DBGuard.BLL.Interfaces.Services;
using DBGuard.DataAccess.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBGuard.AdminApp.Pages.Account;

public class SetRootPasswordModel : PageModel
{
    [BindProperty] public string Password { get; set; }
    [BindProperty] public string ConfirmPassword { get; set; }

    private readonly IUserService _userService;
    
    private readonly IJwtService _jwtService;

    public SetRootPasswordModel(IUserService userService, IJwtService jwtService)
   {
       _userService = userService;
       _jwtService = jwtService;
   }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match");
            return Page();
        }

        var userId = Int32.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var updatedUser = await _userService.SetRoot(userId, Password);

        var newToken = _jwtService.GenerateToken(updatedUser);

        Response.Cookies.Append("jwt", newToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        });

        return RedirectToPage("/Rules/Index");
    }
}