using Ascii.Rendering;
using Ascii.Types;
using ProcessorEmulator.Extensions;

namespace ProcessorEmulator
{
    public static class ConsoleOutput
    {
        private static readonly AsciiColor black = AsciiColor.FromRGB(24, 20, 37);
        private static readonly AsciiColor dark_gray = AsciiColor.FromRGB(38, 43, 68);
        private static readonly AsciiColor gray = AsciiColor.FromRGB(58, 68, 102);
        private static readonly AsciiColor light_gray = AsciiColor.FromRGB(90, 105, 136);
        private static readonly AsciiColor white = AsciiColor.FromRGB(139, 155, 180);

        private static readonly AsciiColor red = AsciiColor.FromRGB(228, 59, 68);
        private static readonly AsciiColor green = AsciiColor.FromRGB(99, 199, 77);
        private static readonly AsciiColor blue = AsciiColor.FromRGB(18, 78, 137);

        /// <summary>
        /// Setup the const parts of the console.
        /// </summary>
        public static void SetupConst()
        {
            AsciiRenderer.SetBackground(black);

            // Header:
            AsciiRenderer.SetString(1, 1, "MOS 6502", light_gray);

            // Main Info:
            int p_baseX = 1, p_baseY = 4;
            AsciiRenderer.SetString(p_baseX, p_baseY, "Processor:", gray);
            AsciiRenderer.SetString(p_baseX + 1, p_baseY + 2, "PC ----", light_gray);
            AsciiRenderer.SetString(p_baseX + 10, p_baseY + 2, "SP --", light_gray);
            AsciiRenderer.SetString(p_baseX + 17, p_baseY + 2, "CC 0", light_gray);

            // Registers:
            int r_baseX = 40, r_baseY = 4;
            AsciiRenderer.SetString(r_baseX, r_baseY, "Registers:", gray);
            AsciiRenderer.SetString(r_baseX + 1, r_baseY + 2, "A --", light_gray);
            AsciiRenderer.SetString(r_baseX + 7, r_baseY + 2, "X --", light_gray);
            AsciiRenderer.SetString(r_baseX + 13, r_baseY + 2, "Y --", light_gray);

            // Status Flags:
            int s_baseX = 1, s_baseY = 10;
            AsciiRenderer.SetString(s_baseX, s_baseY, "Processor Status:", gray);
            string flags = "CZIDBVN";
            for (int i = 0; i < flags.Length; i++)
            {
                AsciiRenderer.SetString(s_baseX + 1 + 5 * i, s_baseY + 2, $"{flags[i]} -", light_gray);
            }

            // Memory:
            int pa_baseX = 40, pa_baseY = 10;
            AsciiRenderer.SetString(pa_baseX, pa_baseY, "End Page ($FFFA-$FFFF):", gray);
            for (int x = 0; x < 6; x++)
            {
                AsciiRenderer.SetString(pa_baseX + 1 + 3 * x, pa_baseY + 2, "--", light_gray);
            }

            int zp_baseX = 1, zp_baseY = 16;
            AsciiRenderer.SetString(zp_baseX, zp_baseY, "Zero Page ($0000-$00FF):", gray);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    AsciiRenderer.SetString(zp_baseX + 1 + 3 * x, zp_baseY + 2 + 2 * y, "--", light_gray);
                }
            }
        }

        /// <summary>
        /// Update the main processor values in the console.
        /// </summary>
        public static void UpdateMain(ushort programCounter, byte stackPointer, int cycles)
        {
            int baseX = 1, baseY = 4;
            AsciiRenderer.SetString(baseX + 4, baseY + 2, BM.UShortToHex(programCounter), (programCounter == 0xFFFC ? blue : white));
            AsciiRenderer.SetString(baseX + 13, baseY + 2, BM.ByteToHex(stackPointer), (stackPointer == 0x00 ? dark_gray : white));
            AsciiRenderer.SetString(baseX + 20, baseY + 2, cycles.ToString(), (cycles == 0 ? dark_gray : red));
        }

        /// <summary>
        /// Update the register values in the console.
        /// </summary>
        public static void UpdateRegisters(byte[] registers)
        {
            int baseX = 40, baseY = 6;
            AsciiRenderer.SetString(baseX + 3,  baseY, BM.ByteToHex(registers[0]), (registers[0] == 0x00 ? dark_gray : white));
            AsciiRenderer.SetString(baseX + 9,  baseY, BM.ByteToHex(registers[1]), (registers[1] == 0x00 ? dark_gray : white));
            AsciiRenderer.SetString(baseX + 15, baseY, BM.ByteToHex(registers[2]), (registers[2] == 0x00 ? dark_gray : white));
        }

        /// <summary>
        /// Update the status flags in the console.
        /// </summary>
        public static void UpdateStatusFlags(bool[] statusFlags)
        {
            int baseX = 1, baseY = 10;
            for (int i = 0; i < statusFlags.Length; i++)
            {
                AsciiRenderer.SetChar(baseX + 3 + 5 * i, baseY + 2, 
                    (statusFlags[i] == true ? '1' : '0'),
                    (statusFlags[i] == true ? green : red));
            }
        }

        /// <summary>
        /// Update the memory in the console.
        /// </summary>
        public static void UpdateMemory(Mem memory)
        {
            int baseX = 1, baseY = 16;

            // Zero page:
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    byte b = memory.Get(y * 16 + x);
                    AsciiRenderer.SetString(baseX + 1 + 3 * x, baseY + 2 + 2 * y, BM.ByteToHex(b), b == 0x00 ? dark_gray : white);
                }
            }

            int pa_baseX = 40, pa_baseY = 10;

            // Programmed Addresses:
            for (int x = 0; x < 6; x++)
            {
                byte b = memory.Get((int)Mem.MAX_MEM - 6 + x);
                AsciiRenderer.SetString(pa_baseX + 1 + 3 * x, pa_baseY + 2, BM.ByteToHex(b), b == 0x00 ? dark_gray : white);
            }
        }
    }
}
