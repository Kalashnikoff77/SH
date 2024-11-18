using Common.JSProcessor;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MudBlazor.Services;
using UI.Components;
using UI.Components.Dialogs;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(o => { o.MaximumReceiveMessageSize = 25000000; });

builder.Services.AddHttpClient();
builder.Services.AddJwtToken(builder);

builder.Services.AddScoped<CurrentState>();
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<IJSProcessor, JSProcessor>();

builder.Services.AddScoped<ShowDialogs>();


// Максимальный размер загружаемых файлов = 35 Мб.
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 35 * 1024 * 1024;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
