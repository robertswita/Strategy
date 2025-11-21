using Common;
using Strategy.Dispel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;

namespace Strategy
{
    class TDispelMap: TMap
    {
        public static int ChunkSize = 25;
        public int WorldWidth;
        public int WorldHeight;
        public Vector2 GridOffset;
        public static Encoding Encoding = Encoding.GetEncoding(1250);
        public List<TAnimation> InteractiveAnims = new List<TAnimation>();
        public List<TAnimation> MonsterAnims = new List<TAnimation>();
        public List<TAnimation> NpcAnims = new List<TAnimation>();
        TDispelCellsSerializer CellsSerializer = new TDispelCellsSerializer();
        public byte[] BTilesProps;

        public TDispelMap()
        {
            Walls.Owner = this;
            Walls.ItemType = typeof(TDispelWall);
            Animations.ItemType = typeof(TDispelAnimation);
            CellsSerializer.Map = this;
            Sprites.Owner = this;
            Sprites.ItemType = typeof(TDispelSprite);
        }
        public TCollect<TTile> ReadTileSet(string filename, string ext)
        {
            filename = filename.Substring(0, filename.Length - 4) + ext;
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var tiles = new TCollect<TTile>();
            var reader = new BinaryReader(fStream);
            var tilesCount = fStream.Length / TDispelTile.Width / TDispelTile.Height;
            for (int i = 0; i < tilesCount; i++)
            {
                var tile = new TDispelTile();
                tile.ReadImage(reader);
                //tile.Index = i;
                tiles.Add(tile);
            }
            fStream.Close();
            return tiles;
        }

        public override void WriteMap(string filename)
        {
            var fStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var writer = new BinaryWriter(fStream);
            writer.Write((WorldWidth + 1) / ChunkSize);
            writer.Write((WorldHeight + 1) / ChunkSize);
            int propCount = 2;
            writer.Write(propCount);
            writer.Write(Floors.Count);
            var bytes = new byte[(Floors.Count - 1) * propCount * sizeof(int)];
            writer.Write(bytes);
            writer.Write(WallTiles.Count);
            bytes = new byte[WallTiles.Count * propCount];
            writer.Write(bytes);
            Animations.Write(writer);
            var mapSprites = new TCollect<TSprite>();
            foreach (var sprite in Sprites)
            {
                if (sprite is TInteractive || sprite is TMonster || sprite is TNpc)
                    continue;
                mapSprites.Add(sprite);
            }
            mapSprites.Write(writer);
            Walls.Write(writer);
            CellsSerializer.Write(writer);
            writer.Close();
        }

        public override void ReadMap(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            MapName = Path.GetFileNameWithoutExtension(filename);
            GamePath = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            Floors = ReadTileSet(filename, ".gtl");
            WallTiles = ReadTileSet(filename, ".btl");
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            WorldWidth = width * ChunkSize - 1;
            WorldHeight = height * ChunkSize - 1;
            Width = WorldWidth * 4 / 5;
            Height = WorldHeight * 3 / 5;
            GridOffset.X = Height;
            GridOffset.Y = -Height - 1;

            int propCount = reader.ReadInt32();
            int tileCount = reader.ReadInt32();
            reader.ReadBytes((tileCount - 1) * propCount * sizeof(int));
            tileCount = reader.ReadInt32();
            BTilesProps = reader.ReadBytes(tileCount * propCount);
            Animations.Read(reader);
            Sprites.Read(reader);
            Walls.Read(reader);
            CellsSerializer.Read(reader);
            reader.Close();
            LoadExtras(filename);
            LoadMonsters(filename);
            LoadNpc(filename);
            if (Game.Items.Count == 0)
                Game.Items = LoadItems();
            Game.MapNames = LoadInfo($"{GamePath}/AllMap.ini", 1);     
            Dialogs = new TIniReader($"{GamePath}/NpcInGame/Pgp{Game.Map.MapName}.pgp", '|')[""];
            DialogTree = new TIniReader($"{GamePath}/NpcInGame/Dlg{Game.Map.MapName}.dlg")[""];
            RebuildMapView();
            Cursor.Current = Cursors.Default;
        }

        List<TCollect<TItem>> LoadItems()
        {
            var path = $"{GamePath}/CharacterInGame/";
            var items = new List<TCollect<TItem>>();
            var editItems = new TCollect<TItem>();
            editItems.ItemType = typeof(TEditItem);
            editItems.LoadFromFile($"{path}/EditItem.db");
            items.Add(editItems);
            var healItems = new TCollect<TItem>();
            healItems.ItemType = typeof(THealItem);
            healItems.LoadFromFile($"{path}/HealItem.db");
            items.Add(healItems);
            var weaponItems = new TCollect<TItem>();
            weaponItems.ItemType = typeof(TWeapon);
            weaponItems.LoadFromFile($"{path}/WeaponItem.db");
            items.Add(weaponItems);
            var miscItems = new TCollect<TItem>();
            miscItems.ItemType = typeof(TMiscItem);
            miscItems.LoadFromFile($"{path}/MiscItem.db");
            items.Add(miscItems);
            var eventItems = new TCollect<TItem>();
            eventItems.LoadFromFile($"{path}/EventItem.db");
            items.Add(eventItems);
            return items;
        }

        private Dictionary<int, string> LoadInfo(string path, int spriteNameColumnIndex)
        {
            return File.ReadLines(path)
                    .Where(line => !line.StartsWith(";"))
                    .Select(line => line.Split(','))
                    .Where(fields => fields[1] != "null")
                    .ToDictionary(fields => int.Parse(fields[0]), fields => fields[spriteNameColumnIndex]);
        }

        void ReadSpriteModels(List<TAnimation> spriteModels, Dictionary<int, string> names, string path)
        {
            for (var i = 0; i < names.Count; i++)
            {
                var filename = path + names.Values.ElementAt(i);
                if (File.Exists(filename))
                {
                    var file = File.OpenRead(filename);
                    var reader = new BinaryReader(file);
                    var animation = new TDispelAnimation();
                    animation.Read(reader);
                    var index = names.Keys.ElementAt(i);
                    animation.Name = names[index];
                    spriteModels[index] = animation;
                    reader.Close();
                }
            }
        }
        public void LoadExtras(string filename)
        {
            var path = $"{GamePath}/ExtraInGame/";
            var mapRefPath = $"{path}/Ext{MapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (InteractiveAnims.Count == 0)
            {
                var names = LoadInfo($"{GamePath}/Extra.ini", 1);
                InteractiveAnims = new List<TAnimation>(new TAnimation[names.Keys.ElementAt(names.Count - 1) + 1]);
                ReadSpriteModels(InteractiveAnims, names, path);
            }
            var inters = new TCollect<TDispelInteractive>();
            inters.Owner = this;
            inters.LoadFromFile(mapRefPath);
        }

        public void LoadMonsters(string filename)
        {
            var path = $"{GamePath}/MonsterInGame/";
            var mapRefPath = $"{path}/Mon{MapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (MonsterAnims.Count == 0)
            {
                var names = LoadInfo($"{GamePath}/Monster.ini", 2);
                //var objInfo = new TIniReader($"{GamePath}/Monster.ini", '\t')[""];
                MonsterAnims = new List<TAnimation>(new TAnimation[names.Keys.ElementAt(names.Count - 1) + 1]);
                ReadSpriteModels(MonsterAnims, names, path);
            }
            var monsters = new TCollect<TDispelMonster>();
            monsters.Owner = this;
            monsters.LoadFromFile(mapRefPath);
        }

        public void LoadNpc(string filename)
        {
            var path = $"{GamePath}/NpcInGame/";
            var mapRefPath = $"{path}/Npc{MapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (NpcAnims.Count == 0)
            {
                var names = LoadInfo($"{GamePath}/Npc.ini", 1);
                NpcAnims = new List<TAnimation>(new TAnimation[names.Keys.ElementAt(names.Count - 1) + 1]);
                var main = new Dictionary<int, string>();
                main.Add(0, "M_Body1.spr");
                ReadSpriteModels(NpcAnims, main, $"{GamePath}/CharacterInGame/");
                //var mainAnim = new TDispelAnimation();
                //var reader = new BinaryReader(File.OpenRead($"{GamePath}/CharacterInGame/M_Body1.spr"));
                //mainAnim.Read(reader);
                //reader.Close();
                //NpcAnims.Add(mainAnim);
                ReadSpriteModels(NpcAnims, names, path);
            }
            var mainChar = new TNpc();
            Game.MainChar = mainChar;
            //mainChar.Map = this;
            mainChar.Name = "MainChar";
            mainChar.Cell = Cells[0, 0];
            mainChar.Path = new List<TCell>();
            mainChar.DefaultPath = new List<TCell>();
            mainChar.Animation = NpcAnims[0];
            Sprites.Add(mainChar);
            Game.Npcs.Owner = this;
            Game.Npcs.ItemType = typeof(TDispelNpc);
            Game.Npcs.LoadFromFile(mapRefPath);
            Game.Npcs.Add(mainChar);
        }

        public override Vector2 World2ViewTransform(float x, float y)
        {
            var v = new Vector2(x + y, y - x) - GridOffset;
            return new Vector2(v.X * TDispelTile.Width / 2, v.Y * TDispelTile.Height / 2);
        }
        public override Vector2 View2MapTransform(float x, float y)
        {
            x /= TDispelTile.Width / 2;
            y /= TDispelTile.Height / 2;
            return new Vector2(x, (y - ((int)x & 1)) / 2);
        }
        public override Vector2 Map2WorldTransform(float x, float y)
        {
            y = 2 * y + ((int)x & 1);
            x += GridOffset.X;
            y += GridOffset.Y;
            return new Vector2(x - y, y + x) / 2;
        }

        public override void RebuildMapView()
        {
            Game.MapView = new Bitmap(Game.MapView.Width, Game.MapView.Height);
            var sx = TDispelTile.Width / 2;
            var sy = TDispelTile.Height;
            var gc = Graphics.FromImage(Game.MapView);
            gc.ScaleTransform((float)Game.MapView.Width / (Width * sx), (float)Game.MapView.Height / (Height * sy));           
            var vp = new Rectangle(0, 0, Width - 2, Height - 1);
            if (Cells != null)
                for (int y = vp.Top; y < vp.Bottom; y++)
                    for (int x = vp.Left; x < vp.Right; x++)
                    {
                        var cell = Cells[y, x];
                        var bounds = cell.Bounds;
                        if (cell.Floor != null)
                            gc.DrawImage(cell.Floor.Image, bounds.X, bounds.Y);
                    }
        }


    }
}
