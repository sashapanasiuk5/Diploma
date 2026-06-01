using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RazorLight;

namespace DBGuard.AdminApp.Infrastructure.EmailHelpers;


public class EmailRenderer: IEmailRenderer
{

    private readonly IRazorLightEngine _razorEngine;

    public EmailRenderer()
    {
        _razorEngine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(Program).Assembly)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderEmailAsync<TModel>(string templateKey, TModel model)
    {
        string result = await _razorEngine.CompileRenderAsync(templateKey, model);
        return result;
    }
}