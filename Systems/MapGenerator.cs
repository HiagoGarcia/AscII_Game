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

        private readonly DungeonMap _map;
        
        //Need to set the parameters of the map to make a new map
        public MapGenerator( int width, int height)
        {
            _width = width;
            _height = height;
            _map = new DungeonMap();
        }

        //An open room with walls
        public DungeonMap CreateMap()
        {
            _map.Initialize( _width, _height );
            //The floor of the room
            foreach ( Cell cell in _map.GetAllCells())
            {
                _map.SetCellProperties( cell.X, cell.Y, true, true, true );
            }
            //The left and right walls of the room, cannot be walked through and seen through
            foreach ( Cell cell in _map.GetCellsInRows( 0, _height -1))
            {
                _map.SetCellProperties( cell.X, cell.Y, false, false, true );
            }
            //The top and bottom walls of the room, cannot be walked through and seen through
            foreach ( Cell cell in _map.GetCellsInColumns( 0, _width -1))
            {
                _map.SetCellProperties( cell.X, cell.Y, false, false, true );
            }
            return _map;
        }
    }
}
