using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Strategy
{
    class TPalette
    {
        public int[] Colors = new int[256];
        public byte[,,] Refs;
        public static int Rgb16To32(int byte0, int byte1)
        {
            var b = (byte0 & 0x1F) << 3;
            var g = (byte0 & 0xE0) >> 3 | (byte1 & 7) << 5;
            var r = byte1 & 0xF8;
            int color = b | b >> 5 | g << 8 | (g & 0xC0) << 2 | r << 16 | (r & 0xE0) << 11;
            if (color > 0) color |= unchecked((int)0xFF000000);
            return color;
        }
        public static byte[] Rgb32To16(int color)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)((color & 0xF8) >> 3 | (color & 0x1C00) >> 5);
            bytes[1] = (byte)((color & 0xF8) >> 16 | (color & 0xE000) >> 13);
            return bytes;
        }
        public static int Rgb32To8(int color)
        {
            return (color & 0xC0) >> 6 | (color & 0xE000) >> 11 | (color & 0xE00000) >> 16;
        }

        public byte GetNearestColor(int color)
        {
            if (Refs != null)
                return Refs[color >> 19 & 0x1F, color >> 10 & 0x3F, color >> 3 & 0x1F];
            byte palIdx = 0;
            var minDist = double.MaxValue;
            for (int i = 0; i < Colors.Length; i++)
            {
                var palColor = Colors[i];
                var dist = Math.Abs((palColor >> 16 & 0xFF) - (color >> 16 & 0xFF));
                dist += Math.Abs((palColor >> 8 & 0xFF) - (color >> 8 & 0xFF));
                dist += Math.Abs((palColor & 0xFF) - (color & 0xFF));
                if (dist < minDist)
                {
                    minDist = dist;
                    palIdx = (byte)i;
                }
            }
            return palIdx;
        }

        public void CreateRefs()
        {
            var refs = new byte[32, 64, 32];
            for (int r = 0; r < 32; r++)
                for (int g = 0; g < 64; g++)
                    for (int b = 0; b < 32; b++)
                        refs[r, g, b] = GetNearestColor(r << 19 | g << 10 | b << 3);
            Refs = refs;
        }

        public static int[] CreateRGB332()
        {
            var palette = new int[256];
            for (int r = 0; r < 8; r++)
                for (int g = 0; g < 8; g++)
                    for (int b = 0; b < 4; b++)
                    {
                        var R = r << 5 | r << 2 | r >> 1;
                        var G = g << 5 | g << 2 | g >> 1;
                        var B = b << 6 | b << 4 | b << 2 | b;
                        palette[r * 32 + g * 4 + b] = Color.FromArgb(R, G, B).ToArgb();
                    }
            return palette;
        }

        public void Load(string filename)
        {
            var reader = new BinaryReader(File.OpenRead(filename));
            for (int i = 0; i < Colors.Length; i++)
            {
                var b = reader.ReadByte();
                var g = reader.ReadByte();
                var r = reader.ReadByte();
                var a = r + g + b > 0 ? 255 : 0;
                Colors[i] = Color.FromArgb(a, r, g, b).ToArgb();
            }
            reader.Close();
        }
        public void Save(string filename)
        {
            var writer = new BinaryWriter(File.OpenWrite(filename));
            for (int i = 0; i < Colors.Length; i++)
            {
                var color = Color.FromArgb(Colors[i]);
                writer.Write(color.B);
                writer.Write(color.G);
                writer.Write(color.R);
            }
            writer.Close();
        }

        public void LoadRefs(string filename)
        {
            Refs = new byte[32, 64, 32];
            var buffer = File.ReadAllBytes(filename);
            Buffer.BlockCopy(buffer, 0, Refs, 0, buffer.Length);
        }

        public void SaveRefs(string filename)
        {
            CreateRefs();
            var buffer = new byte[Refs.Length];
            Buffer.BlockCopy(Refs, 0, buffer, 0, buffer.Length);
            File.WriteAllBytes(filename, buffer);
        }

    }
}
