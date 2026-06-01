using DBGuard.BLL.Interfaces.Services;
using DBGuard.DataAccess.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBGuard.AdminApp.Pages.Account;

public class LoginModel : PageModel
{
    [BindProperty] public string Username { get; set; }
    [BindProperty] public string Password { get; set; }

    private readonly IUserService _userService;
    
    private readonly IJwtService _jwtService;

    public LoginModel(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userService.ValidateUser(Username, Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid username or password. Please try again.");
            return Page();
        }
        
        var token = _jwtService.GenerateToken(user);

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(2)
        });
        
        if (user.Role == UserRole.RootFirstLogin)
            return RedirectToPage("/Account/SetRootPassword");

        return RedirectToPage("/Rules/Index");
    }
}