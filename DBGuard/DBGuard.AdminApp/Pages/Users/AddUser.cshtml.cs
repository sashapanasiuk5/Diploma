using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using static BCrypt.Net.BCrypt;

namespace DBGuard.AdminApp.Pages.Users;

[Authorize(Roles = "Root")]
public class AddUser : PageModel
{
    [BindProperty]
    public User Input { get; set; } = new();
    
    private readonly IUserRepository _userRepository;

    public AddUser(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Input.Password = HashPassword(Input.Password);

        await _userRepository.AddUserAsync(Input);
        return RedirectToPage("Index");
    }
}