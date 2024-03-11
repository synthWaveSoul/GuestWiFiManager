using GuestWiFiManager.Components;
using GuestWiFiManager.Components.Models;
using GuestWiFiManager.Components.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//store response from python api 
builder.Services.AddScoped<GetPythonApiResponseDetails>();

//base URI for python api
builder.Services.AddHttpClient<IResponseDetailsService, ResponseDetailsService>(client => {
    client.BaseAddress = new Uri("http://127.0.0.1:8000/");
});

//needed to call python api just once
builder.Services.AddScoped<StateObject>();

builder.Services.AddBlazorBootstrap();

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
