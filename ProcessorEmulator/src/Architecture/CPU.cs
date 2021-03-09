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

        /// <summary> Reset the cpu and memory. </summary>
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

        /// <summary> Reset the cpu. </summary>
        public void Reset()
        {
            // Reset Vector Address //
            PC = 0xFFFC;
            // Reset Stack Pointer //
            SP = 0x00;
            // Reset Processor Status //
            C = Z = I = D = B = V = N = false;
            // Reset Registers //
            A = X = Y = 0;
        }

        /// Modification methods:

        /// <summary>
        /// Adds two bytes (1 cycle).
        /// </summary>
        /// <returns> The result as a word. </returns>
        private Word AddBytes(ref s32 cycles, Byte a, Byte b)
        {
            // Decrement the cycles.
            cycles--;
            // Return result as word.
            return (Word)(a + b);
        }

        /// <summary>
        /// Adds a byte to a word considering page crossing taking one extra cycle.
        /// </summary>
        private Word AddBytes(ref s32 cycles, Word a, Byte b)
        {
            // Reference the high byte.
            Byte h = (Byte)(a >> 8);
            // Add the byte to the word.
            Word w = (Word)(a + b);

            // Check if page boundary was crossed.
            if (h != (Byte)(w >> 8))
                cycles--; // Use extra cycle.

            // Return result as word.
            return w;
        }

        /// <summary>
        /// Adds a byte to a word without considering page crossings.
        /// The byte will be added to the low byte and wrapped if overflown.
        /// </summary>
        private Word AddBytes(Word a, Byte b)
        {
            // Get the low byte.
            Byte l = (Byte)a;
            // Add the byte to the low byte.
            l += b;
            // Combine the high and low byte again.
            Word w = BM.CombineBytes((Byte)(a >> 8), l);
            // Return result as word.
            return w;
        }

        /// <summary>
        /// Adds two bytes into a word (1 cycle)
        /// </summary>
        private Word AddByteToAddress(ref s32 cycles, Word a, Byte b)
        {
            // Use one cycle.
            cycles--;
            // Return the result as word.
            return (Word)(a + b);
        }

        /// Generalized methods:
        private void SetLDAStatus()
        {
            Z = (A == 0); // Set zero flag.
            N = (A & 0b10000000) > 0; // Set negative flag.
        }

        /// <summary> Execute a number of cycles. </summary>
        public s32 ExecuteCycles(s32 cycles, ref Mem memory)
        {
            //s32 notHandled = 0;

            while (cycles > 0)
            {
                cycles -= Execute(ref memory);

                #region OLD
                //// Fetch an intruction.
                //Byte instr = memory.FetchByte(ref cycles, ref this);
                //// Value.
                //Byte val;
                //// Zero page address.
                //Byte zp_address;
                //// Address.
                //Word address;

                //switch (instr)
                //{
                //    /// The JSR instruction pushes the address (minus one) of the return point on to the stack and then sets the program counter to the target memory address.
                //    #region Jump

                //    case INS.JMP_AB:
                //        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                //        address = SwapBytes(address); // Swap bytes to least significant byte first.

                //        PC = address; // Set the PC to the instructions address.

                //        break;

                //    case INS.JMP_IN:
                //        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                //        address = SwapBytes(address); // Swap bytes to least significant byte first.

                //        address = memory.ReadWord(ref cycles, address); // Read the address at the instructions address.
                //        address = SwapBytes(address); // Swap bytes to least significant byte first.

                //        PC = address; // Set the PC to the instructions address.

                //        break;

                //    #endregion

                //    /// The JSR instruction pushes the address (minus one) of the return point on to the stack and then sets the program counter to the target memory address.
                //    #region Jump to Subroutine

                //    case INS.JSR_AB:
                //        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                //        address = SwapBytes(address); // Swap bytes to least significant byte first.

                //        PC--; // Decrement the PC by one.
                //        cycles--;

                //        memory.WriteWord(ref cycles, (Word)(256 + SP), PC); // Push the PC (minus one) to the stack.

                //        SP += 2; // Increment the stack point by 2.
                //        PC = address; // Set the PC to the instructions address.

                //        break;

                //    #endregion

                //    /// Loads a byte of memory into the accumulator setting the zero and negative flags as appropriate.
                //    #region Load Accumulator

                //    case INS.LDA_IM:
                //        val = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.
                //        A = val; // Load val into A register.

                //        SetLDAStatus();
                //        break;

                //    case INS.LDA_ZP:
                //        zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.
                //        A = memory.ReadByte(ref cycles, zp_address); // Load the byte at the address into the a register.

                //        SetLDAStatus();
                //        break;

                //    case INS.LDA_ZPX:
                //        zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.
                //        address = AddBytes(ref cycles, X, zp_address); // Add the zero page address to the x register.

                //        A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                //        SetLDAStatus();
                //        break;

                //    case INS.LDA_AB:
                //        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                //        address = SwapBytes(address); // Swap bytes to least significant byte first.

                //        A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                //        SetLDAStatus();
                //        break;

                //    case INS.LDA_ABX: /// NOT WORKING ! USED 1 TOO MANY CYCLES !
                //        address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.
                //        address = SwapBytes(address); // Swap bytes to least significant byte first.

                //        address = AddBytes(ref cycles, address, X); // Add the X register to the address.

                //        A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                //        SetLDAStatus();
                //        break;

                //    #endregion Load Accumulator

                //    /// The RTS instruction is used at the end of a subroutine to return to the calling routine. It pulls the program counter (minus one) from the stack.
                //    #region Return from Subroutine

                //    case INS.RTS_IP:
                //        SP -= 2; // Decrement the stack pointer to prepare to read a word.
                //        cycles--;

                //        address = memory.FetchStackWord(ref cycles, SP, ref this); // Fetch the return address from the stack.

                //        PC = address; // Set the program counter to the return address.
                //        PC++; // Increment the program counter.
                //        cycles--;

                //        break;

                //    #endregion

                //    default:
                //        notHandled++;
                //        System.Console.WriteLine("Instruction not handled %d", instr);
                //        break;
                //}
                #endregion
            }

            return cycles /*+ notHandled*/;
        }

        /// <summary> Execute the next instruction. </summary>
        /// <returns> The amount of cycles used. </returns>
        public s32 Execute(ref Mem memory)
        {
            // Create cycles variable for debugging.
            s32 cycles = 0;

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

                    PC = address; // Set the PC to the instructions address.

                    break;

                case INS.JMP_IN:
                    address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.

                    address = memory.ReadWord(ref cycles, address); // Read the address at the instructions address.

                    PC = address; // Set the PC to the instructions address.

                    break;

                #endregion

                /// The JSR instruction pushes the address (minus one) of the return point on to the stack and then sets the program counter to the target memory address.
                #region Jump to Subroutine

                case INS.JSR_AB:
                    address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.

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

                    A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                    SetLDAStatus();
                    break;

                case INS.LDA_ABX:
                    address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.

                    address = AddBytes(ref cycles, address, X); // Add the X register to the address.

                    A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                    SetLDAStatus();
                    break;

                case INS.LDA_ABY:
                    address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.

                    address = AddBytes(ref cycles, address, Y); // Add the Y register to the address.

                    A = memory.ReadByte(ref cycles, address); // Load the byte at the address into the a register.

                    SetLDAStatus();
                    break;

                case INS.LDA_INX:
                    zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.

                    val = (Byte)AddBytes(ref cycles, zp_address, X); // Add the X register to the zero page address.

                    address = memory.ReadWord(ref cycles, val); // Load the target address from the zero page.

                    A = memory.ReadByte(ref cycles, address); // Load the byte at target address into the a register.

                    SetLDAStatus();
                    break;

                case INS.LDA_INY:
                    zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.

                    address = memory.ReadWord(ref cycles, zp_address); // Load the 16bit address from zero page.

                    address = AddBytes(ref cycles, address, Y); // Add the Y register to the address from the zero page.

                    A = memory.ReadByte(ref cycles, address); // Load the byte at target address into the a register.

                    SetLDAStatus();
                    break;

                #endregion

                /// Stores the contents of the accumulator into memory.
                #region Store Accumulator

                case INS.STA_ZP:
                    zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.

                    memory.WriteByteZeroPage(ref cycles, zp_address, A); // Write A register into the zero page address.
                    
                    break;

                case INS.STA_ZPX:
                    zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.

                    address = AddBytes(ref cycles, zp_address, X); // Add the X register to the zero page address.

                    memory.WriteByte(ref cycles, address, A); // Write A register into the target address.

                    break;

                case INS.STA_AB:
                    address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.

                    memory.WriteByte(ref cycles, address, A); // Write A register into the target address.

                    break;

                case INS.STA_ABX:
                    address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.

                    address = AddByteToAddress(ref cycles, address, X); ; // Add the X register to the address.

                    memory.WriteByte(ref cycles, address, A); // Write A register into the target address.

                    break;

                case INS.STA_ABY:
                    address = memory.FetchWord(ref cycles, ref this); // Fetch the next word.

                    address = AddByteToAddress(ref cycles, address, Y); // Add the Y register to the address.

                    memory.WriteByte(ref cycles, address, A); // Write A register into the target address.

                    break;

                case INS.STA_INX:
                    zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.

                    val = (Byte)AddBytes(ref cycles, zp_address, X); // Add the X register to the zero page address.

                    address = memory.ReadWord(ref cycles, val); // Load the target address from the zero page.

                    memory.WriteByte(ref cycles, address, A); // Load the byte at target address into the a register.

                    break;

                case INS.STA_INY:
                    zp_address = memory.FetchByte(ref cycles, ref this); // Fetch the next byte.

                    address = memory.ReadWord(ref cycles, zp_address); // Load the 16bit address from zero page.

                    address = AddByteToAddress(ref cycles, address, Y); // Add the Y register to the address from the zero page.

                    memory.WriteByte(ref cycles, address, A); // Load the byte at target address into the a register.

                    break;

                #endregion

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
                    System.Console.WriteLine($"Instruction not handled {instr}");
                    return 1;
            }

            return 0 - cycles;
        }
    }
}
