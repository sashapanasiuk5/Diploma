using System.Text;
using DBGuard.AdminApp.Configuration;
using DBGuard.AdminApp.Infrastructure;
using DBGuard.BLL.Helpers;
using DBGuard.BLL.Interfaces.Helpers;
using DBGuard.BLL.Interfaces.Services;
using DBGuard.BLL.Services;
using DBGuard.Common.Constants;
using DBGuard.DataAccess.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterDependencies(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var myService = services.GetRequiredService<IAppInitialization>();
    await myService.Initialize();
}

await app.RunAsync();