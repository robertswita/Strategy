using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Strategy.Diablo
{
    class BitStream
    {
        public byte[] Buffer;
        public int Position;
        public int Size;
        public BitStream(byte[] buffer) { Buffer = buffer; }
        public int Read(int bitCount)
        {
           //if (Position + bitCount >= Buffer.Length * 8) return 0;
            var result = 0;
            for (int i = 0; i < bitCount; i++)
            {
                if ((Buffer[Position >> 3] >> (Position & 7) & 1) != 0)
                    result |= 1 << i;
                Position++;
            }
            return result;
        }
        public int ReadSigned(int bitCount)
        {
            var result = Read(bitCount);
            if ((result >> (bitCount - 1) & 1) != 0) // negative
                result |= ~((1 << bitCount) - 1);
            return result;
        }

        public bool ReadBool() { return Read(1) != 0; }

        public void Align()
        {
            if (Position % 8 != 0) 
                Position += 8 - Position % 8;
        }
    }
}
