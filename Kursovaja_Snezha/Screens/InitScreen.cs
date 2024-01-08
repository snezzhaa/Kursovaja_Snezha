using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Tetris.Components;
using Tetris.Definitions;
using Tetris.IO;
using Tetris.Language;

namespace Tetris.Screens
{
    public class InitScreen : BaseComponent
    {
        public InitScreen(InputOutput io,
            Text localizer) : base(io, localizer)
        {
        }

        private async Task DrawAsync(CancellationToken cancellationToken = default)
        {
            await IO.ClearAsync(cancellationToken);
        }

        private async Task<CultureInfo> GetGameLanguageAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new CultureInfo("en"));
        }

        public async Task<CultureInfo> SelectCultureAsync(CancellationToken cancellationToken)
        {
            CultureInfo culture = null;
            while (culture == null)
            {
                await DrawAsync(cancellationToken);
                culture = await GetGameLanguageAsync(cancellationToken);
            }

            return culture;
        }
    }
}