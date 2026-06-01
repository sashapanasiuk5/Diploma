using DBGuard.BLL.Interfaces.Services;
using DBGuard.Contracts.Models.RuleModels;
using DBGuard.DataAccess.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DBGuard.AdminApp.Pages.Rules;

[Authorize(Roles = "User,Root")]
public class Index : PageModel
{
    [BindProperty]
    public SqlInjectionRuleModel SqlInjectionRule { get; set; }
    
    [BindProperty]
    public BruteForceRuleModel BruteForceRule { get; set; }
    
    [BindProperty]
    public BulkOperationsRuleModel BulkOperationsRule { get; set; }
    
    [BindProperty]
    public EmailSendingRuleModel EmailSendingRule { get; set; }
    
    [BindProperty]
    public string? SmtpPassword { get; set; }
    
    
    private readonly IRuleService _ruleService;
    
    private readonly IEncryptionService _encryptionService;

    public Index(IRuleService ruleService, IEncryptionService encryptionService)
    {
        _ruleService = ruleService;
        _encryptionService = encryptionService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        (SqlInjectionRule, BruteForceRule, BulkOperationsRule, EmailSendingRule) = await _ruleService.GetRules();
        
        ModelState.Clear();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("EmailSendingRule.RuleData.PasswordEncrypted");

        if (!ModelState.IsValid)
            return Page();
        
        if (!string.IsNullOrWhiteSpace(SmtpPassword))
        {
            EmailSendingRule.RuleData.PasswordEncrypted = _encryptionService.Encrypt(SmtpPassword);
        }

        await _ruleService.SaveRules(SqlInjectionRule, BruteForceRule, BulkOperationsRule, EmailSendingRule);

        TempData["Success"] = "Security rules saved successfully!";
        return RedirectToPage();
    }
}