using Common.Mapping;
using Common;
using Radzen;
using UI.Components;
using Common.JSProcessor;
using Common.Models.States;
using Common.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(o => { o.MaximumReceiveMessageSize = 25000000; });

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddRadzenComponents();

builder.Services.AddHttpClient();
builder.Services.AddJwtToken(builder);
builder.Services.AddAutoMapper(typeof(Mapping));

builder.Services.AddScoped<CurrentState>();
builder.Services.AddScoped(typeof(IRepository<,,>), typeof(Repository<,,>));
builder.Services.AddScoped<IJSProcessor, JSProcessor>();

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
