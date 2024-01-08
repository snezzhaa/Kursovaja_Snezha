using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tetris.Components;
using Tetris.Definitions;
using Tetris.IO;
using Tetris.Language;
using Tetris.Screens;

namespace Tetris.Services
{
    public class TetrisService : BaseComponent, IHostedService
    {
        private readonly GameScreen _gameScreen;
        private readonly InitScreen _initScreen;
        private readonly LeaderBoardScreen _leaderBoardScreen;
        private readonly SplashScreen _splashScreen;

        public TetrisService(InputOutput io,
            Text localizer,
            InitScreen initScreen,
            SplashScreen splashScreen,
            GameScreen gameScreen,
            LeaderBoardScreen leaderBoardScreen)
            : base(io, localizer)
        {
            _initScreen = initScreen;
            _splashScreen = splashScreen;
            _gameScreen = gameScreen;
            _leaderBoardScreen = leaderBoardScreen;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                try
                {
                    var (width, height) = await IO.GetWidthHeightAsync(cancellationToken);
                    if (width < Constants.ScreenWidth || height < Constants.ScreenHeight)
                        throw new ArgumentException(string.Format(Text.ScreenResolutionError,
                            Constants.ScreenWidth, Constants.ScreenHeight));

                    var culture = await _initScreen.SelectCultureAsync(cancellationToken);
                    await Text.SetCultureAsync(culture, cancellationToken);

                    var playAgain = true;

                    while (playAgain)
                    {
                        var playerLevel = await _splashScreen.GetPlayerLevelAsync(cancellationToken);
                        var scores = await _gameScreen.PlayGameAsync(playerLevel, cancellationToken);
                        playAgain = await _leaderBoardScreen.ShowAsync(scores, cancellationToken);
                    }

                    await IO.TerminateAsync(cancellationToken);
                }
                catch (OperationCanceledException) // handling cancellation
                {
                    await IO.TerminateAsync(cancellationToken);
                }
            }, cancellationToken);

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await IO.ClearAsync(cancellationToken);
        }
    }
}