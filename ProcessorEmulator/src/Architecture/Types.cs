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
            // LDA
            LDA_IM = 0xA9,
            LDA_ZP = 0xA5,
            LDA_ZPX = 0xB5,
            LDA_AB = 0xAD,
            LDA_ABX = 0xBD,
            LDA_ABY = 0xB9,
            LDA_INX = 0xA1,
            LDA_INY = 0xB1,
            // JSR
            JSR_AB = 0x20;
    }
}
