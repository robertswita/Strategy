using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TScript
    {
        public TGame Game;
        //TDispelMap Map;
        public List<List<string>> Commands;
        public Dictionary<string, int> Variables;
        int ActCommandNo;
        public bool IsWaiting;
        int Delay;
        bool Condition;
        public void Run()
        {
            if (Delay > 0)
            {
                Delay--;
                if (Delay == 0) IsWaiting = false;
            }
            if (IsWaiting) return;
            if (ActCommandNo >= Commands.Count)
            {
                Game.ActiveScript = null;
                return;
            }
            var command = Commands[ActCommandNo];
            ActCommandNo++;
            int arg1 = 0;
            if (command.Count > 1 && command[1] != "" && !int.TryParse(command[1], out arg1))
                arg1 = Variables[command[1]];
            switch (command[0])
            {
                case "teleport":
                    var x = int.Parse(command[1]);
                    var y = int.Parse(command[2]);
                    var mapIdx = int.Parse(command[3]);
                    SetMap(mapIdx, x, y);
                    break;
                case "setmap":
                    var scr = new TIniReader($"{TMap.GamePath}/Ref/Map.ini");
                    var mapLine = scr[""][int.Parse(command[1])];
                    x = int.Parse(mapLine[2]);
                    y = int.Parse(mapLine[3]);
                    mapIdx = int.Parse(mapLine[4]);
                    SetMap(mapIdx, x, y);
                    break;
                case "chractivate":
                    Game.MainChar.Enabled = int.Parse(command[1]) == 1;
                    break;
                case "chrmoveto":
                    NpcMoveTo(Game.MainChar, int.Parse(command[1]), int.Parse(command[2]));
                    break;
                case "chrchangedir":
                    Game.MainChar.ViewAngle = int.Parse(command[1]);
                    break;
                case "npcmoveto":
                    var npc = Game.Npcs[arg1];
                    NpcMoveTo(npc, int.Parse(command[2]), int.Parse(command[3]));
                    break;
                case "npcchangedir":
                    npc = Game.Npcs[arg1];
                    npc.ViewAngle = int.Parse(command[2]);
                    break;
                case "delay":
                    IsWaiting = true;
                    Delay = int.Parse(command[1]);
                    break;
                case "createnpc":
                    Variables[command[1]] = Game.Npcs.Count;
                    scr = new TIniReader($"{TMap.GamePath}/NpcInGame/Eventnpc.ref");
                    var npcLine = scr[""][int.Parse(command[2])];
                    //npc = Game.Npcs[int.Parse(npcLine[1])];
                    var npcNew = new TNpc();
                    //npcNew.Map = Game.Map;
                    var pos = Game.Map.World2MapTransform(int.Parse(npcLine[8]), int.Parse(npcLine[9]));
                    npcNew.Cell = Game.Map.Cells[(int)pos.Y, (int)pos.X];
                    //npcNew.Animation = npc.Animation;
                    npcNew.Animation = (Game.Map as TDispelMap).NpcAnims[int.Parse(npcLine[1])];
                    Game.Npcs.Add(npcNew);
                    npcNew.Index = Game.Map.Sprites.Count;
                    Game.Map.Sprites.Add(npcNew);
                    break;
                case "deletenpc":
                    npc = Game.Npcs[arg1];
                    Game.Map.Sprites.RemoveAt(npc.Index);
                    Game.Npcs.RemoveAt(arg1);
                    break;
                case "refreshnpc":
                    break;
                case "dialogtonpc":
                    npc = Game.Npcs[arg1];
                    Game.ActiveNpc = npc;
                    Game.Board.DialogId = int.Parse(command[2]);
                    Game.Board.ProcessDialog();
                    IsWaiting = true;
                    break;
                case "dialog":
                    //npc = Game.Npcs[arg1];
                    Game.ActiveNpc = Game.MainChar;
                    Game.Board.DialogId = arg1;
                    Game.Board.ProcessDialog();
                    IsWaiting = true;
                    break;
                case "if":
                    Condition = arg1 == int.Parse(command[3]);
                    if (!Condition) SkipBlock();
                    break;
                case "else":
                    if (Condition) SkipBlock();
                    break;
                case "getitem":
                    Game.ActivePlayer.Items.Add(Game.Items[arg1][int.Parse(command[2])]);
                    break;
                case "chkitem":
                    Variables[command[1]] = Game.ActivePlayer.Items.Contains(Game.Items[int.Parse(command[2])][int.Parse(command[3])]) ? 1: 0;
                    break;
                case "return":
                    Game.ActiveScript = null;
                    break;
            }
        }

        void SkipBlock()
        {
            var command = new List<string>() { "{" };
            while (command[0] != "}")
            {
                command = Commands[ActCommandNo];
                ActCommandNo++;
            }
        }

        void SetMap(int mapIdx, float x, float y)
        {
            var mapName = Game.MapNames[mapIdx];
            if (mapName != Game.Map.MapName)
                Game.Map.ReadMap($"{TMap.GamePath}/Map/{mapName}.map");
            var pos = (Game.Map as TDispelMap).World2MapTransform(x, y);
            pos.X /= Game.Map.Width;
            pos.Y /= Game.Map.Height;
            Game.Board.ScrollPos = new PointF(pos.X, pos.Y);
        }

        void NpcMoveTo(TNpc npc, float x, float y)
        {
            var destP = new Vector2(x, y);
            if (destP.X != 0 || destP.Y != 0)
                destP = Game.Map.World2MapTransform(destP.X, destP.Y + 1);
            var startP = new Vector2(npc.Cell.X, npc.Cell.Y);
            if (startP != destP)
            {
                Game.ActiveNpc = npc;
                npc.CalcPath(new List<Vector2>() { startP, destP });
                IsWaiting = true;
            }
        }
    }
}
