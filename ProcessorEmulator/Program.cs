using Ascii.Engine;
using Ascii.Rendering;
using System;

namespace ProcessorEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            AsciiRenderer.Initialize(80, 50, "Processor");

            // Ascii engine events :
            GameManager gameManager = new GameManager();
            AsciiEngine.OnStart += gameManager.Start;
            AsciiEngine.OnUpdate += gameManager.Update;
            AsciiEngine.OnKeyPressed += gameManager.KeyPressed;
            AsciiEngine.OnRender += gameManager.Render;

            AsciiEngine.Initialize(60);
        }
    }
}
