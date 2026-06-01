namespace DBGuard.AdminApp.Infrastructure.EmailHelpers;

public interface IEmailRenderer
{
    Task<string> RenderEmailAsync<TModel>(string viewPath, TModel model);
}