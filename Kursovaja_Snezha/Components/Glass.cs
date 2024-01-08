using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tetris.Definitions;
using Tetris.Enums;
using Tetris.IO;
using Tetris.Language;

namespace Tetris.Components
{
    public class Glass : BaseComponent
    {
        private static readonly IList<short[,]> Masks = new List<short[,]>(new[]
        {
            new short[,] {{1, 1, 1, 1}},
            new short[,] {{2, 2}, {2, 2}},
            new short[,] {{0, 0, 3}, {3, 3, 3}},
            new short[,] {{4, 0, 0}, {4, 4, 4}},
            new short[,] {{0, 5, 5}, {5, 5, 0}},
            new short[,] {{6, 6, 0}, {0, 6, 6}},
            new short[,] {{7, 7, 7}, {0, 7, 0}},
            new short[,] {{0, 8, 0}, {8, 8, 8}}
        });

        private readonly Random _random = new Random();
        private Block _block;

        private short[,] _glassArray;
        private Block _nextBlock;

        public Glass(InputOutput io, Text localizer) : base(io, localizer)
        {
        }

        public bool NextVisible { get; private set; }

        public async Task SetNextVisibleAsync(bool visible, CancellationToken cancellationToken)
        {
            NextVisible = visible;
            await DisplayNextBlockAsync(cancellationToken);
        }

        public event EventHandler OnFullLine;
        public event EventHandler OnGameFinished;
        public event EventHandler<Block> OnNewBlock;

        private async Task<Block> GetNextBlockAsync()
        {
            var index = _random.Next(Masks.Count - 1);
            var result = new Block(Masks[index]);
            return await Task.FromResult(result);
        }

        private async Task GenerateNewBlockPairAsync()
        {
            _nextBlock ??= await GetNextBlockAsync();

            _block = _nextBlock;
            _block.X = (int) Math.Abs(Math.Ceiling((Constants.GlassWidth - _block.Width) / 2.0));
            _block.Y = 0;

            _nextBlock = await GetNextBlockAsync();
            _nextBlock.X = 0;
            _nextBlock.Y = (int) Math.Abs(Math.Ceiling((Constants.GlassHeight + Constants.GlassDeltaY) / 2.0));

            OnNewBlock?.Invoke(this, _block);
        }

        private async Task DisplayNextBlockAsync(CancellationToken cancellationToken = default)
        {
            var x = Constants.GlassDeltaX - 11;
            var cleanLine = new string(' ', 8);
            for (var i = 0; i <= 3; i++)
            {
                await IO.OutAsync(x, _nextBlock.Y + i, cleanLine, cancellationToken);
            }

            if (NextVisible)
            {
                await DrawBlockAsync(_nextBlock, x, 0, cancellationToken);
            }
        }


        public async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            _glassArray = new short[Constants.GlassWidth, Constants.GlassHeight];
            _block = null;
            NextVisible = false;
            await Task.CompletedTask;
        }

        private async Task DrawGlassAsync(bool init, CancellationToken cancellationToken = default)
        {
            if (init)
            {
                int i;
                for (i = Constants.GlassDeltaY; i < _glassArray.GetUpperBound(1) + 1 + Constants.GlassDeltaY; i++)
                {
                    await IO.OutAsync(Constants.GlassDeltaX - 2, i, Constants.GlassItem, cancellationToken);
                }

                await IO.OutAsync(Constants.GlassDeltaX - 2, i, Constants.GlassBottom1, cancellationToken);
                await IO.OutAsync(Constants.GlassDeltaX - 2, i + 1, Constants.GlassBottom2, cancellationToken);
            }
            else
            {
                for (var y = 0; y <= _glassArray.GetUpperBound(1); y++)
                {
                    string line = null;
                    for (var x = 0; x <= _glassArray.GetUpperBound(0); x++)
                    {
                        if (_glassArray[x, y] == 0)
                        {
                            line += Constants.EmptyBox;
                        }
                        else
                        {
                            line += Constants.BlockBox;
                        }
                    }
                    await IO.OutAsync(Constants.GlassDeltaX, y + Constants.GlassDeltaY, line, cancellationToken);
                }
            }
        }

        public async Task TickAsync(PlayerActionEnum action, CancellationToken cancellationToken = default)
        {
            Block block;

            if (_block == null) // first run init 
            {
                await GenerateNewBlockPairAsync();
                block = (Block) _block.Clone();
                await DrawGlassAsync(true, cancellationToken);
            }
            else
            {
                if (action == PlayerActionEnum.None && BlockHasStuck(_block)) // check stuck
                {
                    await ApplyBlockAsync(_block);
                    await RecalculateLinesAsync(cancellationToken);
                    await GenerateNewBlockPairAsync();
                    await DisplayNextBlockAsync(cancellationToken);
                }

                block = (Block) _block.Clone();
                switch (action)
                {
                    case PlayerActionEnum.None:
                    case PlayerActionEnum.SoftDrop:
                        block.Y++;
                        break;
                    case PlayerActionEnum.Left:
                    {
                        block.X--;
                        break;
                    }
                    case PlayerActionEnum.Right:
                    {
                        block.X++;
                        break;
                    }
                    case PlayerActionEnum.Rotate:
                    {
                        block = await _block.RotateAsync(cancellationToken);
                        break;
                    }
                    case PlayerActionEnum.Drop:
                        var inc = false;
                        while (CanApply(block))
                        {
                            inc = true;
                            block.Y++;
                        }

                        if (inc && block.Y > 0)
                        {
                            block.Y--;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }
            }

            if (CanApply(block))
            {
                _block = block;

                await DrawGlassAsync(false, cancellationToken);
                await DrawBlockAsync(_block, Constants.GlassDeltaX, Constants.GlassDeltaY, cancellationToken);
            }
            else
            {
                if (_block.Y == 0)
                    OnGameFinished?.Invoke(this, new EventArgs());
            }

            await Task.CompletedTask;
        }

        private bool BlockHasStuck(Block block)
        {
            // test for next tick
            var testBlock = (Block) block.Clone();
            testBlock.Y++;
            return !CanApply(testBlock);
        }

        private async Task RecalculateLinesAsync(CancellationToken cancellationToken = default)
        {
            var newArray = new short[Constants.GlassWidth, Constants.GlassHeight];
            var yy = newArray.GetUpperBound(1);

            for (var y = _glassArray.GetUpperBound(1); y >= 0; y--)
            {
                var isFullLine = true;
                for (var x = 0; x <= _glassArray.GetUpperBound(0); x++)
                {
                    newArray[x, yy] = _glassArray[x, y];
                    if (isFullLine && _glassArray[x, y] == 0)
                        isFullLine = false;
                }

                if (isFullLine)
                {
                    for (var x = 0; x <= newArray.GetUpperBound(0); x++)
                        newArray[x, yy] = 0;
                    OnFullLine?.Invoke(this, new EventArgs());
                }
                else
                {
                    yy--;
                }
            }

            _glassArray = newArray;

            await Task.CompletedTask;
        }

        private async Task ApplyBlockAsync(Block block)
        {
            for (var x = 0; x < block.Width; x++)
            {
                for (var y = 0; y < block.Height; y++)
                {
                    if (block[x, y] != 0)
                    {
                        _glassArray[block.X + x, block.Y + y] = 1;
                    }
                }
            }

            await Task.CompletedTask;
        }

        private bool CanApply(Block block)
        {
            var result = block.X >= 0 &&
                         block.X + block.Width <= Constants.GlassWidth &&
                         block.Y + block.Height <= Constants.GlassHeight;

            if (result)
            {
                for (var x = 0; x < block.Width; x++)
                {
                    for (var y = 0; y < block.Height; y++)
                    {
                        if (block[x, y] != 0 && _glassArray[block.X + x, block.Y + y] != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return result;
        }

        private async Task DrawBlockAsync(Block block, int deltaX, int deltaY,
            CancellationToken cancellationToken)
        {
            for (var x = 0; x < block.Width; x++)
            {
                for (var y = 0; y < block.Height; y++)
                {
                    if (block[x, y] != 0)
                    {
                        await IO.OutAsync(deltaX + (block.X + x) * 2,
                            deltaY + block.Y + y,
                            Constants.BlockBox,
                            cancellationToken);
                    }
                }
            }
        }
    }
}