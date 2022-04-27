using AscII_Game.Core;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int maxRooms, int roomMaxSize, int roomMinSize)
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
            _map.Initialize( _width, _height );

            for (int r = _maxRooms; r > 0; r-- )
            {
                //Will make the room size and position of the room random
                int roomWitdth = Game.Random.Next( _roomMinSize, _roomMaxSize );
                int roomHeight = Game.Random.Next( _roomMinSize, _roomMaxSize );
                int roomXPosition = Game.Random.Next(0, _width - roomWitdth - 1);
                int roomYPosition = Game.Random.Next(0, _height - roomHeight - 1);

                var newRoom = new Rectangle( roomXPosition, roomYPosition, 
                    roomWitdth, roomHeight );

                //A check to see if the room intersects with another room
                bool newRoomIntersects = _map.Rooms.Any( room => newRoom.Intersects( room ) );

                //If it doesn't then add the new room to the map
                if ( !newRoomIntersects )
                {
                    _map.Rooms.Add( newRoom );
                }
            }

            foreach (Rectangle room in _map.Rooms )
            {
                CreateRoom(room);
            }

            for(int r = 1; r < _map.Rooms.Count; r++)
            {
                int previousRoomCenterX = _map.Rooms[r - 1].Center.X;
                int previousRoomCenterY = _map.Rooms[r - 1].Center.Y;
                int currentRoomCenterX = _map.Rooms[r].Center.X;
                int currentRoomCenterY = _map.Rooms[r].Center.Y;

                if(Game.Random.Next(1,2) == 1)
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

            PlacePlayer();

            return _map;
        }

        private void CreateRoom(Rectangle room)
        {
            for(int x = room.Left + 1; x < room.Right; x++)
            {
                for(int y = room.Top + 1; y < room.Bottom; y++)
                {
                    _map.SetCellProperties(x, y, true, true, false);
                }
            }
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

        //Hallways
        private void CreateHorizontalTunnel( int xStart, int xEnd, int yPosition)
        {
            for( int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++ )
            {
                _map.SetCellProperties(x, yPosition, true, true);
            }    
        }
        
        private void CreateVerticalTunnel(int yStart, int yEnd, int xPostition)
        {
            for(int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart,yEnd); y++ )
            {
                _map.SetCellProperties(xPostition, y, true, true);
            }
        }
    }
}
