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
            LDA_IN = 0xA9,
            LDA_ZP = 0xA5,
            LDA_ZPX = 0xB5,
            LDA_AB = 0xAD;
    }
}
