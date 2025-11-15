// Project: aguacongas/Identity.Redis
// Copyright (c) 2025 @Olivier Lefebvre
using IdentitySample.Models;
using IdentitySample.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
// Add framework services.
services.AddLogging(builder =>
    {
        builder.AddDebug()
            .AddConsole()
            .SetMinimumLevel(LogLevel.Warning)
            .AddFilter("Aguacongas.Identity.Redis", LogLevel.Trace);
    })
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddRedisStores(builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection"))
    .AddDefaultTokenProviders();

var twitterConsumerKey = builder.Configuration["Authentication:Twitter:ConsumerKey"];
if (!string.IsNullOrEmpty(twitterConsumerKey))
{
    services.AddAuthentication().AddTwitter(twitterOptions =>
    {
        twitterOptions.ConsumerKey = twitterConsumerKey;
        twitterOptions.ConsumerSecret = builder.Configuration["Authentication:Twitter:ConsumerSecret"];
    });
}

services.AddMvc(options => options.EnableEndpointRouting = false);

// Add application services.
services.AddTransient<IEmailSender, AuthMessageSender>()
    .AddTransient<ISmsSender, AuthMessageSender>();

var app = builder
    .Build();

var env = app.Environment;
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseAuthentication();

app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});

await app.RunAsync().ConfigureAwait(false);