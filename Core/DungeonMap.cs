using RLNET;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscII_Game.Core
{
    public class DungeonMap : Map
    {
        public List<Rectangle> Rooms { get; set; }
        public List<Door> Doors { get; set; }
        public Stairs StairsUp { get; set; }
        public Stairs StairsDown { get; set; }

        public DungeonMap()
        {
            //Game.SchedulingSystem.Clear();

            Rooms = new List<Rectangle>();

            Doors = new List<Door>();
        }


        public void Draw(RLConsole mapConsole)
        {
            mapConsole.Clear();
            foreach(Cell cell in GetAllCells() )
            {
                SetConsoleSymbolForCell(mapConsole, cell);
            }

            foreach(Door door in Doors)
            {
                door.Draw(mapConsole, this);
            }

            StairsUp.Draw(mapConsole, this);
            StairsDown.Draw(mapConsole, this);
        }

        private void SetConsoleSymbolForCell( RLConsole console, Cell cell )
        {
            if ( !cell.IsExplored)
            {
                return;
            }
            if (IsInFov(cell.X, cell.Y))
            {
                if(cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.FloorBackgroundFov, '.');
                }    
                else
                {
                    console.Set(cell.X, cell.Y, Colors.WallFov, Colors.WallBackgroundFov, '#');
                }
            }
            else
            {
                if(cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.Floor, Colors.FloorBackground, '.');
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.Wall, Colors.WallBackground, '#');
                }
            }
        }
        public void UpdatePlayerFieldOfView()
        {
            Player player = Game.Player;
            ComputeFov(player.X, player.Y, player.Awareness, true);
            foreach(Cell cell in GetAllCells())
            {
                if(IsInFov( cell.X, cell.Y))
                {
                    SetCellProperties( cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }
        public bool SetActorPostion(Actor actor, int x, int y)
        {
            if (GetCell( x, y).IsWalkable)
            {
                SetIsWalkable(actor.X, actor.Y, true);


                actor.X = x;
                actor.Y = y;

                SetIsWalkable(actor.X, actor.Y, false);
                OpenDoor(actor, x, y);

                if( actor is Player )
                {
                    UpdatePlayerFieldOfView();
                }
                return true;
            }
            return false;
        }
        public void SetIsWalkable( int x, int y, bool isWalkable)
        {
            ICell cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }

        public void AddPlayer(Player player)
        {
            Game.Player = player;
            SetIsWalkable(player.X, player.Y, false);
            UpdatePlayerFieldOfView();
        }

        public Door GetDoor( int x, int y)
        {
            return Doors.SingleOrDefault( d => d.X == x && d.Y == y );
        }

        private void OpenDoor(Actor actor, int x, int y)
        {
            Door door = GetDoor(x, y);
            if(door != null && !door.IsOpen)
            {
                door.IsOpen = true;
                var cell = GetCell(x, y);
                SetCellProperties(x, y, true, cell.IsWalkable, cell.IsExplored);

                Game.MessageLog.Add($"{actor.Name} opened a door");
            }
        }

        public bool CanMoveDownToNextLevel()
        {
            Player player = Game.Player;
            return StairsDown.X == player.X && StairsDown.Y == player.Y;
        }
    }
}
