using Ascii.Types;
using ProcessorEmulator.Extensions;

namespace ProcessorEmulator
{
    public class GameManager
    {
        private Mem mem;
        private CPU cpu;

        // Debug information.
        private byte[] registers;
        private byte stackPointer;
        private ushort programCounter;
        private bool[] statusFlags;
        private int cycles;

        public void Start()
        {
            // Create and reset cpu.
            mem = new Mem();
            cpu = new CPU();
            cpu.Reset(ref mem);

            // Start - Little inline program.
            mem[0xFFFC] = INS.LDA_AB;
            mem[0xFFFD] = 0x80;
            mem[0xFFFE] = 0x0F;
            ushort address = BM.CombineBytes(0x80, 0x0F);
            mem[address] = 0x64;
            // End - Little inline program.

            cycles = cpu.Execute(4, ref mem);

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
            ConsoleOutput.UpdateMemory(mem);
        }

        public void KeyPressed(AsciiInput input)
        {

        }
    }
}
