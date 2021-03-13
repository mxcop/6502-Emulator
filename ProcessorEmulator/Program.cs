using Ascii.Engine;
using Ascii.Rendering;
using Ascii.Types;
using System;

namespace ProcessorEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            AsciiRenderer.Initialize(80, 50, "Processor");

            // Ascii engine events :
            SimManager gameManager = new SimManager();
            AsciiEngine.OnStart += gameManager.Start;
            AsciiEngine.OnUpdate += gameManager.Update;
            AsciiEngine.OnKeyPressed += gameManager.KeyPressed;
            AsciiEngine.OnRender += gameManager.Render;

            //AsciiEngine.OnStart += () =>
            //{
            //    Random rand = new Random();

            //    AsciiRenderer.SetBackground(AsciiColor.FromRGB(24, 20, 37));

            //    for (int y = 0; y < 50; y++)
            //    {
            //        for (int x = 0; x < 80; x++)
            //        {
            //            int r = rand.Next(0, 16);
            //            char c = ' ';

            //            c = r switch
            //            {
            //                10 => 'A',
            //                11 => 'B',
            //                12 => 'C',
            //                13 => 'D',
            //                14 => 'E',
            //                15 => 'F',
            //                _ => ("" + r)[0],
            //            };

            //            int r2 = rand.Next(-15, 5);
            //            AsciiRenderer.SetChar(x, y, c, AsciiColor.FromRGB((byte)(38 + r2), (byte)(43 + r2), (byte)(68 + r2)));
            //        }
            //    }
            //};

            AsciiEngine.Initialize(60);
            
        }
    }
}
