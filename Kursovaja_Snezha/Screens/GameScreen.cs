using System;
using System.Threading;
using System.Threading.Tasks;
using Tetris.Components;
using Tetris.Definitions;
using Tetris.Enums;
using Tetris.IO;
using Tetris.Language;
using Tetris.Models;

namespace Tetris.Screens
{
    public class GameScreen : BaseComponent
    {
        private readonly Glass _glass;
        private readonly HelpBoard _helpBoard;
        private readonly ScoreBoard _scoreBoard;
        private bool _isGameActive;
        private bool _isInitialized;
        private bool _isTerminated;
        private int _levelSwitch;

        public GameScreen(InputOutput io,
            Text localizer,
            HelpBoard helpBoard,
            ScoreBoard scoreBoard,
            Glass glass) : base(io, localizer)
        {
            _scoreBoard = scoreBoard;
            _helpBoard = helpBoard;
            _glass = glass;
        }

        private int LoopDelay => Constants.LevelSpeedMultiplier * (10 - _scoreBoard.Level);

        private async Task<string> ReadPlayerNameAsync(CancellationToken cancellationToken = default)
        {
            await IO.OutAsync(0, 15, Text.ReadPlayerName, cancellationToken);
            var result = await IO.ReadLineAsync(cancellationToken);
            return await Task.FromResult(result);
        }

        private async Task InitKeyHandlerAsync(CancellationToken cancellationToken = default)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                while (_isGameActive && !cancellationToken.IsCancellationRequested)
                {
                    var playerAction = PlayerActionEnum.None;

                    var key = await IO.GetKeyAsync(cancellationToken);
                    if (key == null)
                    {
                        continue;
                    }

                    if (key == 3 || key == 26) // Ctrl+C || Ctrl+Z - terminate program
                    {
                        _isTerminated = true;
                        break;
                    }

                    switch (key)
                    {
                        case 48:
                            await _helpBoard.SetVisibleAsync(!_helpBoard.Visible, cancellationToken);
                            break;
                        case 49:
                            await _glass.SetNextVisibleAsync(!_glass.NextVisible, cancellationToken);
                            break;
                        case 52:
                            await _scoreBoard.NextLevelAsync(cancellationToken);
                            break;
                        case 55:
                        case 68:
                            playerAction = PlayerActionEnum.Left;
                            break;
                        case 57:
                        case 67:
                            playerAction = PlayerActionEnum.Right;
                            break;
                        case 56:
                        case 65:
                            playerAction = PlayerActionEnum.Rotate;
                            break;
                        case 53:
                        case 66:
                            playerAction = PlayerActionEnum.SoftDrop;
                            break;
                        case 32:
                            playerAction = PlayerActionEnum.Drop;
                            break;

                        default:
                            break;
                    }

                    if (playerAction != PlayerActionEnum.None)
                    {
                        await _glass.TickAsync(playerAction, cancellationToken);
                    }
                }
            }, cancellationToken);

            await Task.CompletedTask;
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _glass.OnFullLine += async (sender, args) =>
            {
                _scoreBoard.Lines++;
                _levelSwitch++;
                if (_levelSwitch != Constants.LinesNextLevelSwitch)
                {
                    return;
                }

                await _scoreBoard.NextLevelAsync(cancellationToken);
            };
            _glass.OnGameFinished += (sender, args) => { _isGameActive = false; };
            _glass.OnNewBlock += (sender, block) => { _scoreBoard.Score += 10; };

            _isInitialized = true;

            await Task.CompletedTask;
        }

        public async Task<LeaderBoardItem> PlayGameAsync(short playerLevel,
            CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
            {
                await InitializeAsync(cancellationToken);
            }

            _levelSwitch = 0;
            _isGameActive = true;
            _isTerminated = false;

            await IO.ClearAsync(cancellationToken);

            await _helpBoard.ResetAsync(cancellationToken);
            await _scoreBoard.ResetAsync(playerLevel, cancellationToken);
            await _glass.ResetAsync(cancellationToken);

            await InitKeyHandlerAsync(cancellationToken);

            // main loop
            while (_isGameActive && !_isTerminated)
            {
                await _glass.TickAsync(PlayerActionEnum.None, cancellationToken);
                await Task.Delay(LoopDelay, cancellationToken);
            }

            if (_isTerminated)
            {
                throw new OperationCanceledException();
            }

            var playerName = await ReadPlayerNameAsync(cancellationToken);
            var result = await _scoreBoard.ToLeaderBoardItemAsync(playerName, cancellationToken);
            return await Task.FromResult(result);
        }
    }
}