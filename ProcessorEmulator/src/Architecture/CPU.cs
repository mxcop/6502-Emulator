using ProcessorEmulator.Extensions;

// Assign custom types.
using Byte = System.Byte;
using Word = System.UInt16;
using s32 = System.Int32;

namespace ProcessorEmulator
{
    public struct CPU
    {
        public Word PC;    // Program Counter //
        public Byte SP;    // Stack Pointer   //

        public Byte A;     // Accumulator Register //
        public Byte X;     // Index X Register     //
        public Byte Y;     // Index Y Register     //

        Byte PS;    // Processor Status //
        #region Status Flags
        bool C { get => BM.GetBit(PS, 0); set => BM.SetBit(ref PS, 0, value); } /* Carry Flag */
        bool Z { get => BM.GetBit(PS, 1); set => BM.SetBit(ref PS, 1, value); } /* Zero Flag */
        bool I { get => BM.GetBit(PS, 2); set => BM.SetBit(ref PS, 2, value); } /* Interrupt Disable */
        bool D { get => BM.GetBit(PS, 3); set => BM.SetBit(ref PS, 3, value); } /* Decimal Mode */
        bool B { get => BM.GetBit(PS, 4); set => BM.SetBit(ref PS, 4, value); } /* Break Command */
        bool V { get => BM.GetBit(PS, 5); set => BM.SetBit(ref PS, 5, value); } /* Overflow Flag */
        bool N { get => BM.GetBit(PS, 6); set => BM.SetBit(ref PS, 6, value); } /* Negative Flag */
        #endregion

        // Debug Requests //
        #region Debug Methods
        public Byte[] Registers() => new Byte[] { A, X, Y };
        public Word ProgramCounter() => PC;
        public Byte StackPointer() => SP;
        public bool[] StatusFlags() => new bool[] { C, Z, I, D, B, V, N };
        #endregion

        /// <notes MOS6502>
        /// The 6502 microprocessor is a relatively simple 8 bit CPU 
        /// with only a few internal registers capable of addressing at most 64Kb 
        /// of memory via its 16 bit address bus.
        /// !! The processor is little endian 
        /// !! and expects addresses to be stored in memory least significant byte first.
        /// </notes>

        /// <summary> Reset the program counter. </summary>
        public void Reset(ref Mem memory)
        {
            // Reset Vector Address //
            PC = 0xFFFC;    
            // Reset Stack Pointer //
            SP = 0x00;      
            // Reset Processor Status //
            C = Z = I = D = B = V = N = false;
            // Reset Registers //
            A = X = Y = 0;
            // Initialize Memory //
            memory.Initialize();
        }

        /// Modification methods:
        private Word AddBytes(ref s32 cycles, Byte a, Byte b)
        {
            // Decrement the cycles.
            cycles--;
            // Return result as word.
            return (Word)(a + b);
        }

        private Word AddBytes(ref s32 cycles, Word a, Byte b)
        {
            // Decrement the cycles.
            cycles--;
            // Return result as word.
            return (Word)(a + b);
        }

        private Word SwapBytes(Word x)
        {
            return (Word)((x >> 8) | (x << 8));
        }

        /// Generalized methods:
        private void SetLDAStatus()
        {
            Z = (A == 0); // Set zero flag.
            N = (A & 0b10000000) > 0; // Set negative flag.
        }

        private void PushPCToStack(ref s32 cycles, ref Mem memory)
        {
            memory.WriteWord(ref cycles, (Word)(256 + SP), (Word)(PC - 1));
            SP+=2;
        }

        /// <summary> Execute a number of cycles. </summary>
        public s32 Execute(s32 cycles, ref Mem memory)
        {
            while (cycles > 0)
            {
                // Fetch an intruction.
                Byte instr = memory.FetchByte(ref cycles, ref this);
                // Value.
                Byte val;
                // Zero page address.
                Byte zp_address;
                // Address.
                Word address;

                switch (instr)
                {
                    /// Loads a byte of memory into the accumulator setting the zero and negative flags as appropriate.
                    #region Load Accumulator

                    case INS.LDA_IM:
                        val = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.
                        A = val; // Load val into A register.

                        SetLDAStatus();
                        break;

                    case INS.LDA_ZP:
                        zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.
                        A = memory.ReadByte(ref cycles, zp_address); // Load the byte at the address into the a register.

                        SetLDAStatus();
                        break;

                    case INS.LDA_ZPX:
                        zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.
                        address = AddBytes(ref cycles, X, zp_address); // Add the zero page address to the x register.

                        A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                        SetLDAStatus();
                        break;

                    case INS.LDA_AB:
                        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                        address = SwapBytes(address); // Swap bytes to least significant byte first.

                        A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                        SetLDAStatus();
                        break;

                    case INS.LDA_ABX: /// NOT WORKING ! USED 1 TOO MANY CYCLES !
                        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                        address = SwapBytes(address); // Swap bytes to least significant byte first.

                        address = AddBytes(ref cycles, address, X); // Add the X register to the address.

                        A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                        SetLDAStatus();
                        break;

                    #endregion Load Accumulator

                    /// Loads a byte of memory into the accumulator setting the zero and negative flags as appropriate.
                    #region Jump to Subroutine

                    case INS.JSR_AB:
                        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                        address = SwapBytes(address); // Swap bytes to least significant byte first.

                        PushPCToStack(ref cycles, ref memory); // Push the PC to the stack.

                        PC = address; // Set the PC to the instructions address.
                        cycles--;

                        break;

                    #endregion

                    default:
                        System.Console.WriteLine("Instruction not handled %d", instr);
                        break;
                }
            }

            return cycles;
        }
    }
}
