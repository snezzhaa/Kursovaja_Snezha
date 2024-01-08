using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tetris.Components;
using Tetris.IO;
using Tetris.Language;
using Tetris.Screens;
using Tetris.Services;

namespace Tetris
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            await TerminalHost
                .CreateDefaultBuilder()
                .ConfigureServices(services =>
                    {
                        services.AddScoped<Text>();
                        services.AddSingleton<IHostedService, TetrisService>();
                        services.AddSingleton<InputOutput>();
                        services.AddTransient<InitScreen>();
                        services.AddTransient<SplashScreen>();
                        services.AddTransient<LeaderBoardScreen>();
                        services.AddTransient<GameScreen>();
                        services.AddTransient<HelpBoard>();
                        services.AddTransient<ScoreBoard>();
                        services.AddTransient<Glass>();
                    }
                )
                .RunTerminalAsync(options =>
                {
                    options.Title = nameof(Tetris);
                    options.SuppressStatusMessages = true;
                });

            return 0;
        }
    }
}