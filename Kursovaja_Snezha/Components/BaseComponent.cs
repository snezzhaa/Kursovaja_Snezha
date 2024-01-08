using Tetris.IO;
using Tetris.Language;

namespace Tetris.Components
{
    public abstract class BaseComponent
    {
        protected BaseComponent(InputOutput io, Text text)
        {
            IO = io;
            Text = text;
        }

        protected InputOutput IO { get; }
        protected Text Text { get; }
    }
}