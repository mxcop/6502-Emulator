using System;
using System.Collections.Generic;
using System.Text;

// Assign custom types.
using Byte = System.Byte;

namespace ProcessorEmulator
{
    public static class INS
    {
        /// Opcodes:
        public const Byte
            // JMP - Jump
            JMP_AB = 0x4C,
            JMP_IN = 0x6C,
            // JSR - Jump to Subroutine
            JSR_AB = 0x20,
            // LDA - Load Accumelator
            LDA_IM = 0xA9,
            LDA_ZP = 0xA5,
            LDA_ZPX = 0xB5,
            LDA_AB = 0xAD,
            LDA_ABX = 0xBD,
            LDA_ABY = 0xB9,
            LDA_INX = 0xA1,
            LDA_INY = 0xB1,
            // STA - Store Accumulator
            STA_ZP = 0x85,
            STA_ZPX = 0x95,
            STA_AB = 0x8D,
            STA_ABX = 0x9D,
            STA_ABY = 0x99,
            STA_INX = 0x81,
            STA_INY = 0x91,
            // RTS - Return from Subroutine
            RTS_IP = 0x60;
    }
}
