using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Diablo
{
    internal class DiabloAnimation : TAnimation
    {       
        enum TriggerType { None, Attack, Missile, Sound, Skill, Max };
        class MacroBlock
        {
            int[] pixelValues = new int[4];
            public int[] PixelValues
            {
                get { return pixelValues; }
                set
                {
                    pixelValues = value;
                    BitCount = 2;
                    if (pixelValues[0] == pixelValues[1])
                        BitCount = 0;
                    else if (pixelValues[1] == pixelValues[2])
                        BitCount = 1;
                }
            }
            public int PosX, PosY;
            public int Width, Height;
            public bool Still;
            public int BitCount;
        }
        class LAY_INF_S
        {
            public byte IsCastingShadow;
            public byte IsSelectable;
            public byte trans_a;
            public byte trans_b;
            public string wclass;
            public int Order;
        }
        public string BasePath;
        public string Token;
        public string Mode;
        public string ClassType;
        public int Type;
        public int ColormapIdx;
        public string[] ArmorClass = new string[ArmorClassNames.Length];
        byte[] Colormap;
        //public int Flags;
        public byte LayersCount;
        public byte FramesCount;
        public byte DirectionsCount;
        //public int UserDir;
        LAY_INF_S[] lay_inf;
        static string[] ArmorClassNames = { "HD", "TR", "LG", "RA", "LA", "RH", "LH", "SH",
                                      "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8"};
        public static int[] Directions = {
                      4, 16,  8, 17,  0, 18,  9, 19,
                      5, 20, 10, 21,  1, 22, 11, 23,
                      6, 24, 12, 25,  2, 26, 13, 27,
                      7, 28, 14, 29,  3, 30, 15, 31 };
        static int[] FrameBitsCount = { 0, 1, 2, 4, 6, 8, 10, 12, 14, 16, 20, 24, 26, 28, 30, 32 };
        Rectangle[] LayerDirBounds;
        public Rectangle Bounds;

        public void Read()
        {
            var basePath = $"{TMap.GamePath}/D2/{BasePath}/";
            // load colormap
            if (ColormapIdx > 0)
            {
                var cmapPath = TMap.GamePath + "/D2/data/global/Monsters/RandTransforms.dat";
                var cmapIdx = ColormapIdx - 1;
                if (cmapIdx >= 30)
                {
                    cmapPath = basePath + "Cof/palshift.dat";
                    cmapIdx -= 30;
                }
                Colormap = new byte[256];
                var buffer = File.ReadAllBytes(cmapPath);
                Array.Copy(buffer, cmapIdx * 256, Colormap, 0, Colormap.Length);
            }
            // load cof
            var cofName = basePath + $"Cof/{Token}{Mode}{ClassType}.cof";
            //if (!File.Exists(cofName))
            //    return;
            var stream = new FileStream(cofName, FileMode.Open);
            var reader = new BinaryReader(stream);
            LayersCount = reader.ReadByte();
            FramesCount = reader.ReadByte();
            DirectionsCount = reader.ReadByte();
            var version = reader.ReadByte(); //== 20
            var flags = reader.ReadInt32();   //< Possible bitfield values : loopAnim / underlay color when hit
            Bounds = new Rectangle();
            Bounds.X = reader.ReadInt32();
            Bounds.Width = reader.ReadInt32() - Bounds.X;
            Bounds.Y = reader.ReadInt32();
            Bounds.Height = reader.ReadInt32() - Bounds.Y;
            var animRate = reader.ReadInt16(); //< Default animation rate(speed) in 8-bit fixed-point: 256 == 1.f.
            var zeros = reader.ReadInt16();
            lay_inf = new LAY_INF_S[16];
            var dirBounds = new Rectangle[DirectionsCount];
            for (var i = 0; i < LayersCount; i++)
            {
                LayerDirBounds = new Rectangle[DirectionsCount];
                var idx = reader.ReadByte();
                lay_inf[idx] = new LAY_INF_S();
                lay_inf[idx].Order = i;
                lay_inf[idx].IsCastingShadow = reader.ReadByte();
                lay_inf[idx].IsSelectable = reader.ReadByte();
                var transparencyOverride = reader.ReadByte();
                var transparencyLevel = reader.ReadByte();
                var palette = (int[])TDiabloMap.Palette.Clone();
                if (transparencyOverride > 0)
                {
                    for (int palIdx = 0; palIdx < palette.Length; palIdx++)
                    {
                        var color = Color.FromArgb(palette[palIdx]);
                        var alpha = 255;
                        switch (transparencyLevel)
                        {
                            case 0: alpha = 191; break;
                            case 1: alpha = 127; break;
                            case 2: alpha = 127; break;
                            case 3:
                                alpha = (color.R + color.G + color.B) / 3;
                                break;
                            case 4: alpha = 127; break;
                            case 6: alpha = 127; break;
                        }
                        palette[palIdx] = Color.FromArgb(alpha, color.R, color.G, color.B).ToArgb();
                    }
                }
                var weaponClass = TDiabloMap.ReadZString(reader);// Encoding.Default.GetString(reader.ReadBytes(4));
                lay_inf[idx].wclass = weaponClass;
                var armor = ArmorClass[idx];
                var dccName = basePath + $"{ArmorClassNames[idx]}/{Token}{ArmorClassNames[idx]}{armor}{Mode}{weaponClass}.dcc";
                TFrame[][] layerSeq = null;
                if (File.Exists(dccName))
                    layerSeq = ReadDcc(dccName, palette);
                else
                {
                    dccName = dccName.Substring(0, dccName.Length - 4) + ".dc6";
                    //ReadDc6(dccName, palette);
                    //for (int d = 0; d < DirectionsCount; d++)
                    //    LayerDirBounds[d] = BoundingBox;
                }
                Sequences.Add(layerSeq);
                if (i == 0)
                    dirBounds = (Rectangle[])LayerDirBounds.Clone();
                else
                    for (int d = 0; d < DirectionsCount; d++)
                        dirBounds[d] = Rectangle.Union(dirBounds[d], LayerDirBounds[d]);
            }
            // skip triggerTypes of each frame
            reader.ReadBytes(FramesCount);
            var seq = new TFrame[DirectionsCount][];
            for (int d = 0; d < DirectionsCount; d++)
            {
                var frames = new TFrame[FramesCount];
                for (int f = 0; f < FramesCount; f++)
                {
                    var frame = new TFrame();
                    frame.Bounds = dirBounds[d];
                    //frame.Bounds = Bounds;
                    frame.Image = new Bitmap(frame.Bounds.Width, frame.Bounds.Height);
                    var gc = Graphics.FromImage(frame.Image);
                    for (var i = 0; i < LayersCount; i++)
                    {
                        var layerIdx = reader.ReadByte();
                        var layerSeq = Sequences[lay_inf[layerIdx].Order];
                        if (layerSeq == null) continue;
                        var layerFrame = layerSeq[d][f];
                        var offsetX = layerFrame.Bounds.X - layerFrame.Offset.X - frame.Bounds.X;
                        var offsetY = layerFrame.Bounds.Y - layerFrame.Offset.Y - frame.Bounds.Y;
                        gc.DrawImage(layerFrame.Image, offsetX, offsetY);
                    }
                    frames[f] = frame;
                }
                seq[d] = frames;
            }
            Sequences.Clear();
            Sequences.Add(seq);


            //// default animation speed
            //var spd_mul = 1;
            //var spd_div = 256;

            //// default x and y offsets
            //var xoffset = yoffset = 0;

            // speed info : try in animdata.d2
            //   sprintf(animdata_name, "%s%s%s", tok, mod, clas);
            //   if (animdata_get_cof_info(animdata_name, & animdata_fpd, & animdata_speed) == 0)
            //   {
            //      // found
            ////      cof->fpd     = animdata_fpd;
            //      spd_mul = animdata_speed; // can be overriden by objects.txt values
            //      spd_div = 256;
            //   }

            //    // objects.txt ID of that obj
            //    sptr = txt->data +
            //          (obj_line* txt->line_size) +
            //          txt->col[glb_ds1edit.col_obj_id].offset;
            //   lptr = (long*) sptr;
            //    id   = * lptr;
            //    printf("object %s ID = %li\n", name, id);


            //   // which mode is this obj ?
            //   if (stricmp(mod, "NU") == 0)
            //      mode = 0;
            //   else if (stricmp(mod, "OP") == 0)
            //      mode = 1;
            //   else if (stricmp(mod, "ON") == 0)
            //      mode = 2;
            //   else if (stricmp(mod, "S1") == 0)
            //      mode = 3;
            //   else if (stricmp(mod, "S2") == 0)
            //      mode = 4;
            //   else if (stricmp(mod, "S3") == 0)
            //      mode = 5;
            //   else if (stricmp(mod, "S4") == 0)
            //      mode = 6;
            //   else if (stricmp(mod, "S5") == 0)
            //      mode = 7;
            //   else
            //   {
            //      // invalid object's mode, or simply not an object COF (like a monster COF)
            //      // end
            //      free(buff);
            //      if (pal_buff)
            //         free(pal_buff);
            //      return cof;
            //   }

            //// search line in objects.txt for this ID
            //if (id)
            //{
            //    done = FALSE;
            //    i = 0;
            //    line = 0;
            //    glb_ds1edit.obj_desc[obj_line].objects_line = -1;
            //    while (!done)
            //    {
            //        sptr = txt2->data +
            //               (i * txt2->line_size) +
            //               txt2->col[glb_ds1edit.col_objects_id].offset;
            //        lptr = (long*)sptr;
            //        if ((*lptr) == id)
            //        {
            //            done = TRUE;
            //            line = i;
            //        }
            //        else
            //        {
            //            i++;
            //            if (i >= txt2->line_num)
            //            {
            //                // end
            //                free(buff);
            //                if (pal_buff)
            //                    free(pal_buff);
            //                return cof;
            //            }
            //        }
            //    }
            //    glb_ds1edit.obj_desc[obj_line].objects_line = line;

            //    // speed multiplicator
            //    sptr =
            //       txt2->data +
            //       (line * txt2->line_size) +
            //       txt2->col[glb_ds1edit.col_frame_delta[mode]].offset;
            //    lptr = (long*)sptr;
            //    cof->spd_mul = (*lptr) == 0 ? 256 : (*lptr);

            //    // speed divisor
            //    cof->spd_div = 256;

            //    // xoffset & yoffset
            //    if (txt2->col[misc_get_txt_column_num(RQ_OBJECTS, "Xoffset")].size)
            //    {
            //        sptr = txt2->data + (line * txt2->line_size) +
            //               txt2->col[misc_get_txt_column_num(RQ_OBJECTS, "Xoffset")].offset;
            //        lptr = (long*)sptr;
            //        cof->xoffset = *lptr;
            //    }
            //    if (txt2->col[misc_get_txt_column_num(RQ_OBJECTS, "Yoffset")].size)
            //    {
            //        sptr = txt2->data + (line * txt2->line_size) +
            //               txt2->col[misc_get_txt_column_num(RQ_OBJECTS, "Yoffset")].offset;
            //        lptr = (long*)sptr;
            //        cof->yoffset = *lptr;
            //    }

            //    // orderflag
            //    if (txt2->col[glb_ds1edit.col_orderflag[mode]].size)
            //    {
            //        sptr =
            //           txt2->data +
            //           (line * txt2->line_size) +
            //           txt2->col[glb_ds1edit.col_orderflag[mode]].offset;
            //        lptr = (long*)sptr;
            //        cof->orderflag = *lptr;

            //        // if 0, check NU
            //        // because Mephisto bridge only have a 1 in the NU mode
            //        if (*lptr == 0)
            //        {
            //            if (txt2->col[glb_ds1edit.col_orderflag[0]].size)
            //            {
            //                sptr =
            //                   txt2->data +
            //                   (line * txt2->line_size) +
            //                   txt2->col[glb_ds1edit.col_orderflag[0]].offset;
            //                lptr = (long*)sptr;
            //                cof->orderflag = *lptr;
            //            }
            //        }

            //        printf("object %s orderflag = %li\n", name, cof->orderflag);
            //}
            //}

            reader.Close();
        }

        TFrame[][] ReadDcc(string path, int[] palette)
        {
            var stream = new FileStream(path, FileMode.Open);
            var reader = new BinaryReader(stream);
            // file header
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
            var seq = new TFrame[directionsCount][];
            for (int d = 0; d < directionsCount; d++)
            {
                var direction = seq[d];
                var streamSize = dirStartFilePos[d + 1] - dirStartFilePos[d];
                var bs = new BitStream(reader.ReadBytes(streamSize));
                bs.Size = streamSize;
                var dirDecodedSize = bs.Read(32); // sum(frmDecodedSize[f]) + 280
                var hasRawPixels = bs.ReadBool();
                var hasStillBlocks = bs.ReadBool();
                var variable0BitCount = FrameBitsCount[bs.Read(4)];
                var widthBitCount = FrameBitsCount[bs.Read(4)];
                var heightBitCount = FrameBitsCount[bs.Read(4)];
                var xoffsetBitCount = FrameBitsCount[bs.Read(4)];
                var yoffsetBitCount = FrameBitsCount[bs.Read(4)];
                var optionalBytesBitCount = FrameBitsCount[bs.Read(4)];
                var codedBytes = FrameBitsCount[bs.Read(4)];
                var frames = new TFrame[framesCount];
                seq[d] = frames;
                var optionalBytesCount = 0;
                var bounds = new Rectangle();
                for (var f = 0; f < frames.Length; f++)
                {
                    var frame = new TFrame();
                    frames[f] = frame;
                    var frmVariable0 = bs.Read(variable0BitCount);
                    frame.Bounds.Width = bs.Read(widthBitCount);
                    frame.Bounds.Height = bs.Read(heightBitCount);
                    frame.Bounds.X = bs.ReadSigned(xoffsetBitCount);
                    frame.Bounds.Y = bs.ReadSigned(yoffsetBitCount);
                    optionalBytesCount += bs.Read(optionalBytesBitCount);
                    var frmDecodedSize = bs.Read(codedBytes);
                    var frmBottomUp = bs.ReadBool();
                    if (!frmBottomUp)
                        frame.Bounds.Y -= frame.Bounds.Height - 1;
                    if (f == 0) bounds = frame.Bounds;
                    else bounds = Rectangle.Union(bounds, frame.Bounds);
                }
                LayerDirBounds[d] = bounds;
                if (optionalBytesCount > 0)
                {
                    bs.Align();
                    bs.Read(optionalBytesCount * 8);
                }
                var stillBlockStream = new BitStream(bs.Buffer);
                if (hasStillBlocks)
                    stillBlockStream.Size = bs.Read(20);
                var pixelMaskBitStream = new BitStream(bs.Buffer);
                pixelMaskBitStream.Size = bs.Read(20);
                var rawPixelsUsageStream = new BitStream(bs.Buffer);
                var rawPixelsStream = new BitStream(bs.Buffer);
                if (hasRawPixels)
                {
                    rawPixelsUsageStream.Size = bs.Read(20);
                    rawPixelsStream.Size = bs.Read(20);
                }
                var pixelCodesBitStream = new BitStream(bs.Buffer);
                var pixelBlock = new List<int>();
                for (var i = 0; i < 256; i++)
                    if (bs.Read(1) != 0)
                        pixelBlock.Add(i);
                stillBlockStream.Position = bs.Position;
                pixelMaskBitStream.Position = stillBlockStream.Position + stillBlockStream.Size;
                rawPixelsUsageStream.Position = pixelMaskBitStream.Position + pixelMaskBitStream.Size;
                rawPixelsStream.Position = rawPixelsUsageStream.Position + rawPixelsUsageStream.Size;
                pixelCodesBitStream.Position = rawPixelsStream.Position + rawPixelsStream.Size;
                // stage 1 : retrieve palette indices used in macroblocks
                var pixelBuffer = new int[(bounds.Height + 3) >> 2, (bounds.Width + 3) >> 2][];
                var framesBlocks = new MacroBlock[frames.Length][,];
                for (var f = 0; f < frames.Length; f++)
                {
                    var frame = frames[f];
                    frame.Offset.X = frame.Bounds.X - bounds.X;
                    frame.Offset.Y = frame.Bounds.Y - bounds.Y;
                    framesBlocks[f] = CreateMacroblocks(frame);
                    foreach (var block in framesBlocks[f])
                    {
                        var pixelMask = 0xF;
                        var pixelValues = pixelBuffer[block.PosY >> 2, block.PosX >> 2];
                        if (pixelValues == null)
                        {
                            pixelValues = new int[4];
                            pixelBuffer[block.PosY >> 2, block.PosX >> 2] = pixelValues;
                        }
                        else
                        {
                            if (stillBlockStream.Size == 0 || !stillBlockStream.ReadBool())
                                pixelMask = pixelMaskBitStream.Read(4);
                            else
                            {
                                block.Still = true;
                                continue;
                            }
                        }
                        var stackPixelsCount = pixelMask - (pixelMask >> 1) - (pixelMask >> 2) - (pixelMask >> 3);
                        var stackPixels = new Stack<int>(stackPixelsCount);
                        var useRawPixels = false;
                        if (pixelMask > 0 && rawPixelsUsageStream.Size > 0)
                            useRawPixels = rawPixelsUsageStream.ReadBool();
                        var prevPixel = 0;
                        for (var i = 0; i < stackPixelsCount; i++)
                        {
                            var stackPixel = prevPixel;
                            if (useRawPixels)
                                stackPixel = rawPixelsStream.Read(8);
                            else
                            {
                                var displacement = 0;
                                do
                                {
                                    displacement = pixelCodesBitStream.Read(4);
                                    stackPixel += displacement;
                                } while (displacement == 0xF);
                            }
                            if (stackPixel == prevPixel) break;
                            stackPixels.Push(stackPixel);
                            prevPixel = stackPixel;
                        }
                        for (int i = 0; i < pixelValues.Length; i++)
                            if ((pixelMask >> i & 1) != 0)
                            {
                                int code = stackPixels.Count > 0 ? stackPixels.Pop() : 0;
                                pixelValues[i] = pixelBlock[code];
                            }
                        block.PixelValues = (int[])pixelValues.Clone();
                    }
                }
                // stage 2 : build frames
                TPixmap prevPixmap = null;
                for (var f = 0; f < frames.Length; f++)
                {
                    var pixmap = new TPixmap(bounds.Width, bounds.Height);
                    foreach (var block in framesBlocks[f])
                    {
                        for (int y = 0; y < block.Height; y++)
                            for (int x = 0; x < block.Width; x++)
                            {
                                int color = 0;
                                if (block.Still)
                                    color = prevPixmap[block.PosX + x, block.PosY + y];
                                else
                                {
                                    var code = pixelCodesBitStream.Read(block.BitCount);
                                    color = palette[block.PixelValues[code]];
                                }
                                pixmap[block.PosX + x, block.PosY + y] = color;
                            }
                    }
                    frames[f].Image = pixmap.Image;
                    prevPixmap = pixmap;
                }
            }
            reader.Close();
            return seq;
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
