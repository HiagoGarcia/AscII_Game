using RLNET;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscII_Game.Core
{
    private readonly List<Monster> _monsters;

    public class DungeonMap : Map
    {
        public List<Rectangle> Rooms { get; set; }
        public List<Door> Doors { get; set; }

        public DungeonMap()
        {
            Rooms = new List<Rectangle>();

            Doors = new List<Door>();

            _monsters = new List<Monster>();
        }


        public void Draw(RLConsole mapConsole, RLConsole statConsole)
        {
            // mapConsole.Clear();
            foreach (Cell cell in GetAllCells())
            {
                SetConsoleSymbolForCell(mapConsole, cell);
            }

            // Keep an index so we know which position to draw monster stats at
            int i = 0;

            foreach (Door door in Doors)
            {
                door.Draw(mapConsole, this);
            }

            // Iterate through each monster on the map and draw it after drawing the Cells
            foreach ( Monster monster in _monsters )
            {
                monster.Draw(mapConsole, this);
                // When the monster is in the field-of-view also draw their stats
                if ( IsInFov (monster.X, monster.Y))
                {
                    // Pass in the index to DrawStats and increment it afterwards
                    monster.DrawStats(statConsole, i);
                    i++;
                }
            }

        }

        private void SetConsoleSymbolForCell(RLConsole console, Cell cell)
        {
            if (!cell.IsExplored)
            {
                return;
            }
            if (IsInFov(cell.X, cell.Y))
            {
                if (cell.IsWalkable)
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
                if (cell.IsWalkable)
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
            foreach (Cell cell in GetAllCells())
            {
                if (IsInFov(cell.X, cell.Y))
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }
        public bool SetActorPostion(Actor actor, int x, int y)
        {
            if (GetCell(x, y).IsWalkable)
            {
                SetIsWalkable(actor.X, actor.Y, true);


                actor.X = x;
                actor.Y = y;

                SetIsWalkable(actor.X, actor.Y, false);
                OpenDoor(actor, x, y);

                if (actor is Player)
                {
                    UpdatePlayerFieldOfView();
                }
                return true;
            }
            return false;
        }
        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            ICell cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }

        public void AddPlayer(Player player)
        {
            Game.Player = player;
            SetIsWalkable(player.X, player.Y, false);
            UpdatePlayerFieldOfView();
            Game.SchedulingSystem.Add(player);
        }

        public Door GetDoor(int x, int y)
        {
            return Doors.SingleOrDefault(d => d.X == x && d.Y == y);
        }

        private void OpenDoor(Actor actor, int x, int y)
        {
            Door door = GetDoor(x, y);
            if (door != null && !door.IsOpen)
            {
                door.IsOpen = true;
                var cell = GetCell(x, y);
                SetCellProperties(x, y, true, cell.IsWalkable, cell.IsExplored);

                Game.MessageLog.Add($"{actor.Name} opened a door");
            }
        }

        public void AddMonster( Monster monster )
        {
            _monsters.Add( monster );
            // After adding the monster to the map make sure to make the cell not walkable
            SetIsWalkable( monster.X, monster.Y, false );
            Game.SchedulingSystem.Add(monster);
        }

        // Look for a random location in the room that is walkable
        public Point GetRandomWalkableLocationInRoom( Rectangle room )
        {
            if ( DoesRoomHaveWalkableSpace( room ) )
            {
                for ( int i = 0; i < 100; i++ )
                {
                    int x = Game.Random.Next( 1, room.Width - 2) + room.X;
                    int y = Game.Random.Next( 1, room.Height - 2) + room.Y;
                    if ( SetIsWalkable( x, y ) )
                    {
                        return new Point(x, y);
                    }
                }
            }
            // If we didn't find a walkable lecation in the room return null
            return null;
        }

        public void RemoveMonster ( Monster monster)
        {
            _monsters.Remove(monster);
            // After removing the monster from the map, make sure the cell is walkable again
            SetIsWalkable( monster.X, monster .Y, true );
            Game.SchedulingSystem.Remove(monster);
        }

        public Monster GetMonsterAt( int x, int y)
        {
            return _monster.FirstOfDefault(m => m.X == x && m.Y == y);
        }

        // Iterate through each Cell in the room and return true if any are walkable
        public bool DoesRoomHaveWalkableSpace( Rectangle room)
        {
            for ( int x = 1; x < room.Width; x++ )
            {
                for ( int y = 1; y < room.Height; y++ )
                {
                    if ( SetIsWalkable( x + room.X, y + room.Y ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}