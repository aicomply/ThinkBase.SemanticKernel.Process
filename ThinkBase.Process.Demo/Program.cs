using Microsoft.Extensions.AI;
using Microsoft.FluentUI.AspNetCore.Components;
using ThinkBase.Process.Demo.Components;
using ThinkBase.Process.Demo.Connectivity;
using ThinkBase.Process.Demo.Processes;

namespace ThinkBase.Process.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddFluentUIComponents();
            builder.Services.AddSingleton<IChatClient, ChatClient>();
            builder.Services.AddSingleton<IProcessFactory, IntentProcess>();
            builder.Services.AddBlazorAdaptiveCards();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
