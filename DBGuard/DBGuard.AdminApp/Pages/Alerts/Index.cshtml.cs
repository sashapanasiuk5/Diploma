using DBGuard.DataAccess.Data.Entities;
using DBGuard.DataAccess.Data.Enums;
using DBGuard.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBGuard.AdminApp.Pages.Alerts;

[Authorize(Roles = "User,Root")]
public class AlertsModel : PageModel
{
    private readonly IAlertRepository _repository;

    public AlertsModel(IAlertRepository repository)
    {
        _repository = repository;
    }

    public List<Alert> Alerts { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public DateTime? From { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public DateTime? To { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public AlertType? Type { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? Username { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? IpAddress { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync(DateTime? from, DateTime? to, AlertType? type, 
        string? username, string? ipAddress, string? search)
    {
        From = from;
        To = to;
        Type = type;
        Username = username;
        IpAddress = ipAddress;
        Search = search;

        Alerts = await _repository.GetAlertListAsync(from, to, type, username, ipAddress, search);
    }
}