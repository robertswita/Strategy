using Common;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Diablo
{
    class DiabloAnimationLayer
    {
        class MacroBlock
        {
            public int PosX, PosY;
            public int Width, Height;
            public Rectangle Bounds {  get { return new Rectangle(PosX, PosY, Width, Height); } }
            public int[] Palette = new int[4];
            public int BitCount
            {
                get
                {
                    var bitCount = 2;
                    if (Palette[0] == Palette[1])
                        bitCount = 0;
                    else if (Palette[1] == Palette[2])
                        bitCount = 1;
                    return bitCount;
                }
            }
            public bool IsCopy;
        }

        public string Filename;
        public byte IsCastingShadow;
        public byte IsSelectable;
        public byte TransparencyOverride;
        public byte TransparencyLevel;
        public string WeaponClass;
        public Rectangle[] DirBounds;
        public TFrame[][] Direction;
        public int[] Palette;
        static int[] BitSizes = { 0, 1, 2, 4, 6, 8, 10, 12, 14, 16, 20, 24, 26, 28, 30, 32 };

        public void ReadDcc()
        {
            var stream = new FileStream(Filename, FileMode.Open);
            var reader = new BinaryReader(stream);
            var signature = reader.ReadByte();
            var version = reader.ReadByte();
            var directionsCount = reader.ReadByte();
            var framesCount = reader.ReadInt32();
            var tag = reader.ReadInt32();
            var decodedSize = reader.ReadInt32(); // sum(dirDecodedSize[d]) + 280
            var dirStartFilePos = new int[directionsCount + 1];
            for (int i = 0; i < directionsCount; i++)
                dirStartFilePos[i] = reader.ReadInt32();
            dirStartFilePos[directionsCount] = (int)reader.BaseStream.Length;
            Direction = new TFrame[directionsCount][];
            for (int d = 0; d < directionsCount; d++)
            {
                var direction = Direction[d];
                var streamSize = dirStartFilePos[d + 1] - dirStartFilePos[d];
                var bs = new BitStream(reader.ReadBytes(streamSize));
                bs.Size = streamSize;
                var dirDecodedSize = bs.Read(32); // sum(frmDecodedSize[f]) + 280
                var hasRawPixels = bs.ReadBool();
                var hasStillBlocks = bs.ReadBool();
                var variable0BitSize = BitSizes[bs.Read(4)];
                var widthBitSize = BitSizes[bs.Read(4)];
                var heightBitSize = BitSizes[bs.Read(4)];
                var xoffsetBitSize = BitSizes[bs.Read(4)];
                var yoffsetBitSize = BitSizes[bs.Read(4)];
                var optionalDataSizeBitSize = BitSizes[bs.Read(4)];
                var frmDecodedSizeBitSize = BitSizes[bs.Read(4)];
                var frames = new TFrame[framesCount];
                Direction[d] = frames;
                var optionalDataSize = 0;
                var bounds = new Rectangle();
                for (var f = 0; f < frames.Length; f++)
                {
                    var frame = new TFrame();
                    frames[f] = frame;
                    var frmVariable0 = bs.Read(variable0BitSize);
                    frame.Bounds.Width = bs.Read(widthBitSize);
                    frame.Bounds.Height = bs.Read(heightBitSize);
                    frame.Bounds.X = bs.ReadSigned(xoffsetBitSize);
                    frame.Bounds.Y = bs.ReadSigned(yoffsetBitSize);
                    optionalDataSize += bs.Read(optionalDataSizeBitSize);
                    var frmDecodedSize = bs.Read(frmDecodedSizeBitSize);
                    var frmBottomUp = bs.ReadBool();
                    if (!frmBottomUp)
                        frame.Bounds.Y -= frame.Bounds.Height - 1;
                    if (f == 0) bounds = frame.Bounds;
                    else bounds = Rectangle.Union(bounds, frame.Bounds);
                }
                DirBounds[d] = bounds;
                if (optionalDataSize > 0)
                {
                    bs.Align();
                    bs.Read(optionalDataSize * 8);
                }
                var copyBlocksStream = new BitStream(bs.Buffer);
                if (hasStillBlocks)
                    copyBlocksStream.Size = bs.Read(20);
                var codesMaskStream = new BitStream(bs.Buffer);
                codesMaskStream.Size = bs.Read(20);
                var rawCodesEnabledStream = new BitStream(bs.Buffer);
                var rawCodesStream = new BitStream(bs.Buffer);
                if (hasRawPixels)
                {
                    rawCodesEnabledStream.Size = bs.Read(20);
                    rawCodesStream.Size = bs.Read(20);
                }
                var codesStream = new BitStream(bs.Buffer);
                var palEntries = new List<int>();
                for (var i = 0; i < 256; i++)
                    if (bs.Read(1) != 0)
                        palEntries.Add(i);
                copyBlocksStream.Position = bs.Position;
                codesMaskStream.Position = copyBlocksStream.Position + copyBlocksStream.Size;
                rawCodesEnabledStream.Position = codesMaskStream.Position + codesMaskStream.Size;
                rawCodesStream.Position = rawCodesEnabledStream.Position + rawCodesEnabledStream.Size;
                codesStream.Position = rawCodesStream.Position + rawCodesStream.Size;
                // stage 1 : retrieve palette entries used in macroblocks
                var boundBlocks = new MacroBlock[(bounds.Height + 3) >> 2, (bounds.Width + 3) >> 2];
                var framesBlocks = new MacroBlock[frames.Length][,];
                for (var f = 0; f < frames.Length; f++)
                {
                    var frame = frames[f];
                    frame.Offset.X = frame.Bounds.X - bounds.X;
                    frame.Offset.Y = frame.Bounds.Y - bounds.Y;
                    framesBlocks[f] = CreateMacroblocks(frame);
                    foreach (var block in framesBlocks[f])
                    {
                        var codesMask = 0;
                        var prevBlock = boundBlocks[block.PosY >> 2, block.PosX >> 2];
                        if (prevBlock == null)
                        {
                            prevBlock = block;
                            codesMask = 0xF;
                        }
                        else if (copyBlocksStream.Size == 0 || !copyBlocksStream.ReadBool())
                            codesMask = codesMaskStream.Read(4);
                        else
                            block.IsCopy = true;
                        block.Palette = (int[])prevBlock.Palette.Clone();
                        if (codesMask == 0) continue;
                        var rawCodesEnabled = rawCodesEnabledStream.Size > 0 ? rawCodesEnabledStream.ReadBool() : false;
                        var prevCode = 0;
                        //var stackCodesCount = codesMask - (codesMask >> 1) - (codesMask >> 2) - (codesMask >> 3);
                        var stackCodes = new Stack<int>();
                        for (int i = 0; i < block.Palette.Length; i++)
                        {
                            if ((codesMask >> i & 1) == 0) continue;
                            var stackCode = prevCode;
                            if (rawCodesEnabled)
                                stackCode = rawCodesStream.Read(8);
                            else
                                do
                                {
                                    var displacement = codesStream.Read(4);
                                    stackCode += displacement;
                                    if (displacement != 0xF) break;
                                } while (true);
                            if (stackCode == prevCode) break;
                            stackCodes.Push(stackCode);
                            prevCode = stackCode;
                        }
                        for (int i = 0; i < block.Palette.Length; i++)
                        {
                            if ((codesMask >> i & 1) == 0) continue;
                            int code = stackCodes.Count > 0 ? stackCodes.Pop() : 0;
                            block.Palette[i] = palEntries[code];
                        }
                        boundBlocks[block.PosY >> 2, block.PosX >> 2] = block;
                    }
                }
                // stage 2 : build frames
                TPixmap prevPixmap = null;
                for (var f = 0; f < frames.Length; f++)
                {
                    var pixmap = new TPixmap(bounds.Width, bounds.Height);
                    foreach (var block in framesBlocks[f])
                    {
                        if (block.IsCopy)
                        {
                            var prevBlock = boundBlocks[block.PosY >> 2, block.PosX >> 2];
                            if (block.Width == prevBlock.Width && block.Height == prevBlock.Height)
                                pixmap.Copy(prevPixmap, prevBlock.Bounds, block.Bounds.Location);
                        }
                        else
                        {
                            var blockBitCount = block.BitCount;
                            for (int y = 0; y < block.Height; y++)
                                for (int x = 0; x < block.Width; x++)
                                {
                                    var code = codesStream.Read(blockBitCount);
                                    var color = Palette[block.Palette[code]];
                                    pixmap[block.PosX + x, block.PosY + y] = color;
                                }
                        }
                        boundBlocks[block.PosY >> 2, block.PosX >> 2] = block;
                    }
                    frames[f].Image = pixmap.Image;
                    prevPixmap = pixmap;
                }
            }
            reader.Close();
        }

        MacroBlock[,] CreateMacroblocks(TFrame frame)
        {
            int heightFirstRow = 4 - (frame.Offset.Y & 3);
            var blocksYCount = (frame.Bounds.Height - heightFirstRow + 6) >> 2;
            if (blocksYCount == 0) blocksYCount = 1;
            int widthFirstColumn = 4 - (frame.Offset.X & 3);
            var blocksXCount = (frame.Bounds.Width - widthFirstColumn + 6) >> 2;
            if (blocksXCount == 0) blocksXCount = 1;
            var blocks = new MacroBlock[blocksYCount, blocksXCount];
            var currHeight = 0;
            for (int y = 0; y < blocksYCount; y++)
            {
                var currWidth = 0;
                for (int x = 0; x < blocksXCount; x++)
                {
                    var block = new MacroBlock();
                    block.PosX = currWidth + frame.Offset.X;
                    block.PosY = currHeight + frame.Offset.Y;
                    block.Height = 4;
                    block.Width = 4;
                    if (y == 0) block.Height = heightFirstRow;
                    if (x == 0) block.Width = widthFirstColumn;
                    if (y == blocksYCount - 1) block.Height = frame.Bounds.Height - currHeight;
                    if (x == blocksXCount - 1) block.Width = frame.Bounds.Width - currWidth;
                    blocks[y, x] = block;
                    currWidth += block.Width;
                }
                currHeight += blocks[y, 0].Height;
            }
            return blocks;
        }
    }

}
