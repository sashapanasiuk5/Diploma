using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBGuard.AdminApp.Pages.Users;

[Authorize(Roles = "Root")]
public class Index : PageModel
{
    public List<User> Users { get; set; } = new();
    
    private readonly IUserRepository _userRepository;

    public Index(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task OnGetAsync()
    {
        Users = await _userRepository.GetAllUsers();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int userId)
    {
        await _userRepository.ToggleUserStatusAsync(userId);
        return RedirectToPage();
    }
}