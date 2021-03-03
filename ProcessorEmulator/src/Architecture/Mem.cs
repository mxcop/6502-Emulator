using System.Linq;

// Assign custom types.
using Byte = System.Byte;
using Word = System.UInt16;
using s32 = System.Int32;
using u32 = System.UInt32;

namespace ProcessorEmulator
{
    public struct Mem
    {
        // Maximum Memory //
        public const u32 MAX_MEM = 1024 * 64;

        // Memory Data //
        Byte[] Data;

        // Debug Requests //
        #region Debug Methods
        public Byte[] Take(int start, int length) => Data.Skip(start).Take(length).ToArray();
        public Byte Get(int index) => Data[index];
        #endregion

        /// <summary> Initialize the memory. </summary>
        public void Initialize()
        {
            Data = new Byte[MAX_MEM];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = 0;
            }
        }

        /// <summary> Fetch the byte the program counter is pointing to. </summary>
        public Byte FetchByte(ref s32 cycles, ref CPU cpu)
        {
            // Fetch byte from memory.
            Byte data = Data[cpu.PC];
            // Increment the program counter.
            cpu.PC++;
            // Decrement the cycles.
            cycles--;
            // Return the byte.
            return data;
        }

        /// <summary> Read a byte from the given byte address. </summary>
        public Byte ReadByte(ref s32 cycles, Byte address)
        {
            // Read byte from address.
            Byte data = Data[address];
            // Decrement the cycles.
            cycles--;
            // Return the byte.
            return data;
        }

        /// <summary> Read a byte from the given word address. </summary>
        public Byte ReadByte(ref s32 cycles, Word address)
        {
            // Read byte from address.
            Byte data = Data[address];
            // Decrement the cycles.
            cycles--;
            // Return the byte.
            return data;
        }

        /// <summary> Read a word from the given byte address. </summary>
        public Word ReadWord(ref s32 cycles, Byte address)
        {
            return 0;
        }

        // Simple operator overload.
        public Byte this[Word key]
        {
            get => Data[key];
            set => Data[key] = value;
        }
    }
}
