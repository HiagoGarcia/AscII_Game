using AscII_Game.Interfaces;
using RLNET;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscII_Game.Core
{
    public class Actor : IActor, IDrawable
    {
        public string Name { get; set; }
        public int Awareness { get; set; }

        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public void Draw(RLConsole console, IMap map)
        {
            if (!map.GetCell( X, Y).IsExplored)
            {
                return;
            }
            if (map.IsInFov( X, Y))
            {
                console.Set(X, Y, Color, Colors.FloorBackground, Symbol);
            }
            else
            {
                console.Set(X, Y, Colors.Floor, Colors.FloorBackground, '.');
            }
        }
    }
}
