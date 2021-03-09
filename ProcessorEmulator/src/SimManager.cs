using Ascii.Types;
using ProcessorEmulator.Extensions;

namespace ProcessorEmulator
{
    public class SimManager
    {
        private Mem mem;
        private CPU cpu;

        // Debug information.
        private byte[] registers;
        private byte stackPointer;
        private ushort programCounter;
        private bool[] statusFlags;
        private int cycles;
        private int page;

        public void Start()
        {
            // Debug second page.
            page = 2;

            // Create and reset cpu.
            mem = new Mem();
            cpu = new CPU();
            cpu.Reset(ref mem);

            #region JSR LDA RTS
            /// Start - Little inline program.

            mem[0xFFFC] = INS.JSR_AB; // Jump to Subroutine [6]
            mem[0xFFFD] = 0x03;
            mem[0xFFFE] = 0x02;
            ushort address = BM.CombineBytes(0x02, 0x03); // flip bytes for address

            mem[address] = INS.LDA_IM; // Load A Immediate [2]
            mem[address + 1] = 0x42;
            mem[address + 2] = INS.RTS_IP;// Return from Subroutine [6]

            /// End - Little inline program.
            #endregion

            #region JMP_IN LDA
            /// Start - Little inline program.

            //mem[0xFFFC] = INS.JMP_IN; // Jump Indirect [5]
            //mem[0xFFFD] = 0x00;
            //mem[0xFFFE] = 0x03;
            //ushort indirect_address = BM.CombineBytes(0x03, 0x00); // flip bytes for address

            //mem[indirect_address] = 0x03;
            //mem[indirect_address + 1] = 0x02;
            //ushort address = BM.CombineBytes(0x02, 0x03); // flip bytes for address

            //mem[address] = INS.LDA_IM; // Load A Immediate [2]
            //mem[address + 1] = 0x42;

            /// End - Little inline program.
            #endregion

            /// Start - Little inline program.

            //mem[0xFFFC] = INS.JMP_AB; // Jump to 0x0200 (big endian)
            //mem[0xFFFD] = 0x00;
            //mem[0xFFFE] = 0x02;

            //mem[0x0200] = INS.LDA_IM; // Load Immediate 0x34
            //mem[0x0201] = 0x34;

            //mem[0x0202] = INS.STA_ZP; // Store to zero page 0x05
            //mem[0x0203] = 0x05;

            /// End - Little inline program.

            //cycles = cpu.ExecuteCycles(5 + 2, ref mem);

            // Request debug information.
            registers = cpu.Registers();
            stackPointer = cpu.StackPointer();
            programCounter = cpu.ProgramCounter();
            statusFlags = cpu.StatusFlags();

            // Setup the console.
            ConsoleOutput.SetupConst();
        }

        public void Update(float deltaTime)
        {
            // Request debug information.
            registers = cpu.Registers();
            stackPointer = cpu.StackPointer();
            programCounter = cpu.ProgramCounter();
            statusFlags = cpu.StatusFlags();
        }

        public void Render()
        {
            // Update main info:
            ConsoleOutput.UpdateMain(programCounter, stackPointer, cycles);

            // Update registers:
            ConsoleOutput.UpdateRegisters(registers);

            // Update processor status:
            ConsoleOutput.UpdateStatusFlags(statusFlags);

            // Update memory:
            ConsoleOutput.UpdateMemory(programCounter, stackPointer, page, mem);
        }

        public void KeyPressed(AsciiInput input)
        {
            if (input.Key == "=") // Increment page
            {
                if (page < 255)
                    page++;
                else
                    page = 0;
            }

            if (input.Key == "-") // Decrement page
            {
                if (page > 0)
                    page--;
                else
                    page = 255;
            }

            if (input.Key == "Space") // Execute next instruction
            {
                cycles += cpu.Execute(ref mem);
            }

            if (input.Key == "r") // Reset the cpu
            {
                cpu.Reset();
                cycles = 0;
            }
        }
    }
}
