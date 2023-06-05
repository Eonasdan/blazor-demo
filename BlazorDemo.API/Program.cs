using BlazorDemo.API.ApiModules;
using BlazorDemo.API.Hubs;
using BlazorDemo.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace BlazorDemo.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var azureADConfig = builder.Configuration.GetSection("AzureAd");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(azureADConfig);
        
        builder.Services.AddApplicationAuthorization();
        
        builder.Services.AddAuthorization();

        builder.Services.AddControllers();

        builder.Services.AddSignalR();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors();

        var app = builder.Build();

        app.UseCors(policy =>
        {
            policy.WithOrigins("https://localhost:5183").AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });


        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapApiModules();
        app.MapHub<ChatHub>("/ChatHub");

        app.Run();
    }
}