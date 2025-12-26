using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TPlayer
    {
        public TGame Game;
        public Color ID;
        public static int MaxHeros = 10;
        public List<THero> Heroes = new List<THero>();
        public THero SelectedHero;
        public List<TPlayer> Enemies = new List<TPlayer>();
        public List<TCell> VisibleCells = new List<TCell>();
        public int[] Resources = new int[Enum.GetNames(typeof(TResource.ResType)).Length];
        public List<TItem> Items = new List<TItem>();
        public static TPlayer Generate()
        {
            var player = new TPlayer();
            var heroesCount = TGame.Random.Next(MaxHeros - 1) + 1;
            for (int i = 0; i < heroesCount; i++)
            {
                var hero = new THero();
                hero.Player = player;
                hero.Name = i.ToString();
                hero.MovesCount = hero.MovesCountMax;
                player.Heroes.Add(hero);
            }
            return player;
        }
    }
}
