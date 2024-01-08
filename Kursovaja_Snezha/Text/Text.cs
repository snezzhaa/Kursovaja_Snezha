using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Tetris.Definitions;

namespace Tetris.Language
{
    public class Text
    {
        private const string DefaultCultureName = "en";

        private readonly Dictionary<string, Dictionary<string, string>> _resources;

        private CultureInfo _culture = string.IsNullOrEmpty(CultureInfo.CurrentCulture.Name)
            ? new CultureInfo(DefaultCultureName)
            : CultureInfo.CurrentCulture;

        public Text()
        {
            var en = new Dictionary<string, string>
            {
                {nameof(SplashLogo), "Kursovaja TETRIS"},
                {nameof(YourLevel), "ENTER YOUR LEVEL? (0-9) - "},
                {nameof(Yes), "YES"},
                {nameof(No), "NO"},
                {nameof(PlayAgain), "PLAY AGAIN? (YES/NO) - "},
                {nameof(LinesCount), "FULL LINES"},
                {nameof(Name), "NAME"},
                {nameof(Level), "LEVEL"},
                {nameof(Score), "SCORE"},
                {nameof(ReadPlayerName), "YOUR NAME?"},
                {nameof(MoveLeft), "7: LEFT"},
                {nameof(MoveRight), "9: RIGHT"},
                {nameof(Rotate), "8: ROTATE"},
                {nameof(NextLevel), "4: SPEEDUP"},
                {nameof(SoftDrop), "5: DROP"},
                {nameof(ShowNext), "1: SHOW NEXT"},
                {nameof(ClearHelp), "0: CLEAR THIS TEXT"},
                {nameof(Drop), "  SPACE - QUICK DROP"},
                {
                    nameof(ScreenResolutionError),
                    "The game has been designed for screen {0} x {1} symbols. Please adjust terminal window size."
                }
            };

            _resources = new Dictionary<string, Dictionary<string, string>>
            {
                {DefaultCultureName, en}
            };
        }

        public string SplashLogo => this[nameof(SplashLogo)];
        public string YourLevel => this[nameof(YourLevel)];
        public string Yes => this[nameof(Yes)];
        public string No => this[nameof(No)];
        public string PlayAgain => this[nameof(PlayAgain)];
        public string LinesCount => this[nameof(LinesCount)];
        public string Name => this[nameof(Name)];
        public string Level => this[nameof(Level)];
        public string Score => this[nameof(Score)];
        public string ReadPlayerName => this[nameof(ReadPlayerName)];
        public string MoveLeft => this[nameof(MoveLeft)];
        public string MoveRight => this[nameof(MoveRight)];
        public string Rotate => this[nameof(Rotate)];
        public string NextLevel => this[nameof(NextLevel)];
        public string SoftDrop => this[nameof(SoftDrop)];
        public string ShowNext => this[nameof(ShowNext)];
        public string ClearHelp => this[nameof(ClearHelp)];
        public string Drop => this[nameof(Drop)];
        public string ScreenResolutionError => this[nameof(ScreenResolutionError)];

        private string this[string name]
        {
            get
            {
                var val = string.Empty;
                if (!_resources.ContainsKey(_culture.Name)) 
                    val = _resources[DefaultCultureName][name];
                else
                {
                    if (_resources[_culture.Name].ContainsKey(name))
                        val = _resources[_culture.Name][name];
                }
                return val;
            }
        }

        public async Task SetCultureAsync(CultureInfo culture, CancellationToken cancellationToken = default)
        {
            _culture = culture;
            await Task.CompletedTask;
        }
    }
}