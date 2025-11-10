using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Strategy
{
    public class TGame
    {
        public static int FPS = 25;
        public TBoard Board;
        public int RoundCount;
        public List<TPlayer> Players = new List<TPlayer>();
        public static int MaxPlayers = 6;
        public static Color[] PlayersID = new Color[] { Color.Cyan, Color.Magenta, Color.Yellow, Color.Red, Color.Green, Color.Blue }; 
        public static Random Random = new Random();
        public EventHandler OnResourceChanged;
        public TMap Map;
        public Image MapView;
        public TCollect<TNpc> Npcs = new TCollect<TNpc>();
        public TNpc ActiveNpc;
        public TNpc MainChar;
        public TScript ActiveScript;
        public Dictionary<int, string> MapNames;
        public List<TCollect<TItem>> Items = new List<TCollect<TItem>>();


        public void OnEvent(int eventIdx)
        {
            var eventName = eventIdx.ToString("D4");
            var scr = new TIniReader($"{TMap.GamePath}/Ref/Event{eventName}.scr");
            ActiveScript = new TScript();
            ActiveScript.Game = this;
            ActiveScript.Commands = scr["ACT"];
            ActiveScript.Variables = scr.LoadVars();
            ActiveScript.Run();
            Board.Invalidate();
        }

        TPlayer activePlayer;
        public TPlayer ActivePlayer
        {
            get { return activePlayer; }
            set
            {
                if (activePlayer != null)
                    for (int i = 0; i < activePlayer.VisibleCells.Count; i++)
                        activePlayer.VisibleCells[i].IsVisible = false;
                activePlayer = value;
                if (activePlayer != null)
                    for (int i = 0; i < activePlayer.VisibleCells.Count; i++)
                        activePlayer.VisibleCells[i].IsVisible = true;
            }
        }

        //public TGame()
        //{
        //    Map = new TMap();
        //    Map.Game = this;
        //}

        public void Restart()
        {
            Players.Clear();
            var playerCount = Random.Next(MaxPlayers - 1) + 1;
            for (int i = 0; i < playerCount; i++)
            {
                var player = TPlayer.Generate();
                player.Game = this;
                player.ID = PlayersID[i];
                Players.Add(player);
            }
            ActivePlayer = Players[0];

                //game.Players[0].Enemies.AddRange(game.Players.GetRange(1, game.Players.Count));
        }

        internal void NextTurn()
        {
            ActivePlayer = Players[(Players.IndexOf(ActivePlayer) + 1) % Players.Count];
            ActivePlayer.SelectedHero = ActivePlayer.Heroes[0];
            for (int i = 0; i < ActivePlayer.Heroes.Count; i++)
                ActivePlayer.Heroes[i].MovesCount = ActivePlayer.Heroes[i].MovesCountMax;
            //ActivePlayer.Heroes.ForEach(item => item.MovesCount = item.MovesCountMax);
        }

    }
}
