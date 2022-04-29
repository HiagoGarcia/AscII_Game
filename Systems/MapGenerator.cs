using System;
using System.Collections.Generic;
using System.Linq;
using RogueSharp;
using AscII_Game.Core;
using RogueSharp.DiceNotation;
using AscII_Game.Monsters;

namespace AscII_Game.Systems
{
    

    public class MapGenerator
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;

        private readonly DungeonMap _map;

        //Need to set the parameters of the map to make a new map

        public MapGenerator( int width, int height,
            int maxRooms, int roomMaxSize, int roomMinSize, int mapLevel)

        {
            _width = width;
            _height = height;
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _map = new DungeonMap();
        }

        //An open room with walls
        public DungeonMap CreateMap()
        {
            _map.Initialize(_width, _height);

            for (int r = _maxRooms; r > 0; r--)
            {
                //Will make the room size and position of the room random
                int roomWitdth = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = Game.Random.Next(0, _width - roomWitdth - 1);
                int roomYPosition = Game.Random.Next(0, _height - roomHeight - 1);

                var newRoom = new Rectangle(roomXPosition, roomYPosition,
                    roomWitdth, roomHeight);

                //A check to see if the room intersects with another room
                bool newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                //If it doesn't then add the new room to the map
                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }


            for (int r = 1; r < _map.Rooms.Count; r++)
            {
                if (r == 0)

                {
                    continue;
                }

                int previousRoomCenterX = _map.Rooms[r - 1].Center.X;
                int previousRoomCenterY = _map.Rooms[r - 1].Center.Y;
                int currentRoomCenterX = _map.Rooms[r].Center.X;
                int currentRoomCenterY = _map.Rooms[r].Center.Y;

                if (Game.Random.Next(1, 2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
                }
            }


            foreach (Rectangle room in _map.Rooms )

            {
                CreateRoom(room);
                CreateDoors(room);
            }

            CreateStairs();
            PlacePlayer();
            PlaceMonsters();

            return _map;
        }
        private void PlaceMonsters()
        {
            foreach (var room in _map.Rooms)
            {
                // Each room has a 60% chance of having monsters
                if (Dice.Roll("1D10") < 7)
                {
                    // Generate between 1 and 4 monsters
                    var numberOfMonsters = Dice.Roll("1D4");
                    for (int i = 0; i < numberOfMonsters; i++)
                    {
                        Point randomRoomLocation = _map.GetRandomLocationInRoom(room);
                        if (randomRoomLocation != null)
                        {
                            // Temporarily hard code this monster to be created at level 1
                            var monster = Kobold.Create(1);
                            monster.X = randomRoomLocation.X;
                            monster.Y = randomRoomLocation.Y;
                            _map.AddMonster(monster);
                        }
                    }
                }
            }
        }

        private void CreateRoom(Rectangle room)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                for (int y = room.Top + 1; y < room.Bottom; y++)
                {
                    _map.SetCellProperties(x, y, true, true, false);
                }
            }
        }



        //Hallways
        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _map.SetCellProperties(x, yPosition, true, true);
            }
        }

        private void CreateVerticalTunnel(int yStart, int yEnd, int xPostition)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _map.SetCellProperties(xPostition, y, true, true);
            }
        }

        //Doors
        private void CreateDoors(Rectangle room)
        {
            int xMin = room.Left;
            int xMax = room.Right;
            int yMin = room.Top;
            int yMax = room.Bottom;

            List<ICell> borderCells = _map.GetCellsAlongLine(xMin, yMin, xMax, yMin).ToList();
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMin, xMin, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMax, xMax, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMax, yMin, xMax, yMax));

            foreach(Cell cell in borderCells)
            {
                if(IsPotentialDoor(cell))
                {
                    _map.SetCellProperties(cell.X, cell.Y, false, true);
                    _map.Doors.Add(new Door
                    {
                        X = cell.X,
                        Y = cell.Y,
                        IsOpen = false
                    });
                }    
            }
        }

        private bool IsPotentialDoor(Cell cell)
        {
            if(!cell.IsWalkable)
            {
                return false;
            }

            Cell right = (Cell)_map.GetCell(cell.X + 1, cell.Y);
            Cell left = (Cell)_map.GetCell(cell.X - 1, cell.Y);
            Cell top = (Cell)_map.GetCell(cell.X, cell.Y - 1);
            Cell bottom = (Cell)_map.GetCell(cell.X, cell.Y + 1);

            if(_map.GetDoor(cell.X, cell.Y) != null ||
                _map.GetDoor(right.X, right.Y) != null ||
                _map.GetDoor(left.X, left.Y) != null ||
                _map.GetDoor(top.X, top.Y) != null || 
                _map.GetDoor(bottom.X, bottom.Y) != null )
            {
                return false;
            }

            if (right.IsWalkable && left.IsWalkable && !top.IsWalkable && !bottom.IsWalkable)
            {
                return true;
            }

            // This is a good place for a door on the top or bottom of the room
            if (!right.IsWalkable && !left.IsWalkable && top.IsWalkable && bottom.IsWalkable)
            {
                return true;
            }
            return false;
        }

        //Stairs
        private void CreateStairs()
        {
            _map.StairsUp = new Stairs
            {
                X = _map.Rooms.First().Center.X + 1,
                Y = _map.Rooms.First().Center.Y,
                IsUp = true
            };
            _map.StairsDown = new Stairs
            {
                X = _map.Rooms.Last().Center.X,
                Y = _map.Rooms.Last().Center.Y,
                IsUp = false
            };
        }

        private void PlacePlayer()
        {
            Player player = Game.Player;
            if (player == null)
            {
                player = new Player();
            }

            player.X = _map.Rooms[0].Center.X;
            player.Y = _map.Rooms[0].Center.Y;

            _map.AddPlayer(player);
        }
    }
}