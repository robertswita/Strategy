using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Diablo
{
    class TDiabloAnimation : TAnimation
    {       
        enum TriggerType { None, Attack, Missile, Sound, Skill, Max };
        public string BasePath;
        public string Token;
        public string Mode;
        public string ClassType;
        //public string Filename {  get { return Token + Mode + ClassType; } }
        public int Type;
        public int ColormapIdx;
        public string[] ArmorClass = new string[LayerNames.Length];
        byte[] Colormap;
        public int Flags;
        public byte LayersCount;
        public byte FramesCount;
        public byte DirectionsCount;
        TDiabloAnimationLayer[] Layers = new TDiabloAnimationLayer[16];
        public static string[] LayerNames = { "HD", "TR", "LG", "RA", "LA", "RH", "LH", "SH",
                                      "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8"};
        public static int[] Directions = {
                      4, 16,  8, 17,  0, 18,  9, 19,
                      5, 20, 10, 21,  1, 22, 11, 23,
                      6, 24, 12, 25,  2, 26, 13, 27,
                      7, 28, 14, 29,  3, 30, 15, 31 };
        public Rectangle Bounds;

        public void ReadCof()
        {
            var basePath = $"{TMap.GamePath}/D2/{BasePath}/";
            //if (ColormapIdx > 0)
            //{
            //    var cmapPath = TMap.GamePath + "/D2/data/global/Monsters/RandTransforms.dat";
            //    var cmapIdx = ColormapIdx - 1;
            //    if (cmapIdx >= 30)
            //    {
            //        cmapPath = basePath + "Cof/palshift.dat";
            //        cmapIdx -= 30;
            //    }
            //    Colormap = new byte[256];
            //    var buffer = File.ReadAllBytes(cmapPath);
            //    Array.Copy(buffer, cmapIdx * 256, Colormap, 0, Colormap.Length);
            //}
            var cofName = basePath + $"Cof/{Token}{Mode}{ClassType}.cof";
            //if (!File.Exists(cofName))
            //    return;
            var stream = new FileStream(cofName, FileMode.Open);
            var reader = new BinaryReader(stream);
            LayersCount = reader.ReadByte();
            FramesCount = reader.ReadByte();
            DirectionsCount = reader.ReadByte();
            var version = reader.ReadByte();
            if (version != 20)
            {
                reader.Close();
                return;
            }
            Flags = reader.ReadInt32();   //< Possible bitfield values : loopAnim / underlay color when hit
            Bounds = new Rectangle();
            Bounds.X = reader.ReadInt32();
            Bounds.Width = reader.ReadInt32() - Bounds.X;
            Bounds.Y = reader.ReadInt32();
            Bounds.Height = reader.ReadInt32() - Bounds.Y;
            var animRate = reader.ReadInt16(); //< Default animation rate(speed) in 8-bit fixed-point: 256 == 1.f.
            var zeros = reader.ReadInt16();
            var dirBounds = new Rectangle[DirectionsCount];
            int layerDirCount = DirectionsCount;
            for (var i = 0; i < LayersCount; i++)
            {
                var idx = reader.ReadByte();
                var layer = new TDiabloAnimationLayer();
                Layers[idx] = layer;
                layer.Name = LayerNames[idx];
                layer.IsCastingShadow = reader.ReadByte();
                layer.IsSelectable = reader.ReadByte();
                layer.TransparencyOverride = reader.ReadByte();
                layer.TransparencyLevel = reader.ReadByte();
                layer.Palette = (int[])TDiabloMap.Palette.Clone();
                for (int palIdx = 0; palIdx < layer.Palette.Length; palIdx++)
                {
                    var mapIdx = Colormap == null ? palIdx : Colormap[palIdx];
                    var code = TDiabloMap.Palette[mapIdx];
                    if (layer.TransparencyOverride > 0)
                    {
                        var color = Color.FromArgb(code);
                        var alpha = 255;
                        switch (layer.TransparencyLevel)
                        {
                            case 0: alpha = 64; break;
                            case 1: alpha = 128; break;
                            case 2: alpha = 192; break;
                            case 3:
                                //alpha = Math.Max(Math.Max(color.R, color.G), color.B);
                                alpha = (color.R + color.G + color.B) / 2;
                                if (alpha > 255) alpha = 255;
                                //alpha = Math.Max(Math.Max((color.R + color.G) / 2, (color.R + color.B) / 2), (color.G + color.B) / 2);
                                break;
                            case 4: alpha = 127; break;
                            case 6: alpha = 127; break;
                        }
                        code = Color.FromArgb(alpha, color.R, color.G, color.B).ToArgb();
                    }
                    layer.Palette[palIdx] = code;
                }
                layer.WeaponClass = TDiabloMap.ReadZString(reader);// Encoding.Default.GetString(reader.ReadBytes(4));
                var mode = Mode;
                var armor = ArmorClass[idx];
                if (armor == null || armor == "")
                {
                    var dir = basePath + layer.Name;
                    var mask = $"{Token}{layer.Name}*{mode}{layer.WeaponClass}.dc*";
                    var layerFiles = Directory.GetFiles(dir, mask);
                    if (layerFiles.Length == 0)
                    {
                        mask = $"{Token}{layer.Name}*{mode[0]}?{layer.WeaponClass}.dc*";
                        layerFiles = Directory.GetFiles(dir, mask);
                    //    //    if (layerFiles.Length == 0)
                    //    //    {
                    //    //        mask = $"{anim.Token}{layer}*NU*.dc*";
                    //    //        layerFiles = Directory.GetFiles(dir, mask);
                    //    //    }
                    }
                    var layerFile = Path.GetFileName(layerFiles[TGame.Random.Next(layerFiles.Length)]);
                    armor = layerFile.Substring(4, 3);
                    mode = layerFile.Substring(7, 2);
                }
                layer.Filename = basePath + $"{LayerNames[idx]}/{Token}{LayerNames[idx]}{armor}{mode}{layer.WeaponClass}.dcc";
                if (File.Exists(layer.Filename))
                    layer.ReadDcc();
                else
                {
                    layer.Filename = layer.Filename.Substring(0, layer.Filename.Length - 4) + ".dc6";
                    layer.ReadDc6();
                }
                Sequences.Add(layer.Directions);
                //if (layer.Directions == null) continue;
                layerDirCount = layer.Directions.Length;
                for (int d = 0; d < layer.DirBounds.Length; d++)
                    dirBounds[d] = i > 0 ? Rectangle.Union(dirBounds[d], layer.DirBounds[d]) : layer.DirBounds[d];
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
                    //var layerDir = Directions[32 / layerDirCount * d + (layerDirCount & 7)];
                    frame.Bounds = dirBounds[d];
                    //frame.Bounds = Bounds;
                    frame.Image = new Bitmap(frame.Bounds.Width, frame.Bounds.Height);
                    var gc = Graphics.FromImage(frame.Image);
                    for (var i = 0; i < LayersCount; i++)
                    {
                        var layer = Layers[reader.ReadByte()];
                        if (layer.Directions == null) continue;
                        var layerFrame = layer.Directions[d][f];
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

    }
}
