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
        bool V { get => BM.GetBit(PS, 5); set => BM.SetBit(ref PS, 6, value); } /* Overflow Flag */
        bool N { get => BM.GetBit(PS, 6); set => BM.SetBit(ref PS, 7, value); } /* Negative Flag */
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

        /// <summary> Execute a number of cycles. </summary>
        public s32 Execute(s32 cycles, ref Mem memory)
        {
            s32 notHandled = 0;

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
                    /// The JSR instruction pushes the address (minus one) of the return point on to the stack and then sets the program counter to the target memory address.
                    #region Jump

                    case INS.JMP_AB:
                        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                        address = SwapBytes(address); // Swap bytes to least significant byte first.

                        PC = address; // Set the PC to the instructions address.

                        break;

                    case INS.JMP_IN:
                        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                        address = SwapBytes(address); // Swap bytes to least significant byte first.

                        address = memory.ReadWord(ref cycles, address); // Read the address at the instructions address.
                        address = SwapBytes(address); // Swap bytes to least significant byte first.

                        PC = address; // Set the PC to the instructions address.

                        break;

                    #endregion

                    /// The JSR instruction pushes the address (minus one) of the return point on to the stack and then sets the program counter to the target memory address.
                    #region Jump to Subroutine

                    case INS.JSR_AB:
                        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                        address = SwapBytes(address); // Swap bytes to least significant byte first.

                        PC--; // Decrement the PC by one.
                        cycles--;

                        memory.WriteWord(ref cycles, (Word)(256 + SP), PC); // Push the PC (minus one) to the stack.

                        SP += 2; // Increment the stack point by 2.
                        PC = address; // Set the PC to the instructions address.

                        break;

                    #endregion

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

                    /// The RTS instruction is used at the end of a subroutine to return to the calling routine. It pulls the program counter (minus one) from the stack.
                    #region Return from Subroutine

                    case INS.RTS_IP:
                        SP -= 2; // Decrement the stack pointer to prepare to read a word.
                        cycles--;

                        address = memory.FetchStackWord(ref cycles, SP, ref this); // Fetch the return address from the stack.

                        PC = address; // Set the program counter to the return address.
                        PC++; // Increment the program counter.
                        cycles--;

                        break;

                    #endregion

                    default:
                        notHandled++;
                        System.Console.WriteLine("Instruction not handled %d", instr);
                        break;
                }
            }

            return cycles + notHandled;
        }
    }
}
