using RLNET;
using AscII_Game.Core;
using AscII_Game.Systems;
using RogueSharp.Random;
using System;

namespace AscII_Game
{
    public static class Game
    {
        //Allows access to the dungeon generator

        public static CommandSystem CommandSystem { get; private set; }

        public static Player Player { get; set; }

        public static DungeonMap DungeonMap { get; private set; }

        public static MessageLog MessageLog { get; private set; }

        // The screen height and width are in number of tiles
        private static readonly int _screenWidth = 100;
        private static readonly int _screenHeight = 70;
        private static RLRootConsole _rootConsole;

        //The main part of the console is the map
        private static readonly int _mapWidth = 80;
        private static readonly int _mapHeight = 48;
        private static RLConsole _mapConsole;

        //Displays actions happening in the game
        private static readonly int _messageWidth = 80;
        private static readonly int _messageHeight = 11;
        private static RLConsole _messageConsole;

        // Shows the Player stats and enemy stats
        private static readonly int _statWidth = 80;
        private static readonly int _statHeight = 11;
        private static RLConsole _statConsole;

        //Displays the players current inventory, their belt, and their pack
        private static readonly int _inventoryWidth = 20;
        private static readonly int _inventoryHeight = 70;
        private static RLConsole _inventoryConsole;

        public static IRandom Random { get; private set; }

        public static SchedulingSystem SchedulingSystem { get; private set; }

        private static int _mapLevel = 1;
        private static bool _renderRequired = true;


        public static void Main()
        {
            //Creates a new seed based on the current time
            int seed = (int)DateTime.UtcNow.Ticks;
            Random = new DotNetRandom(seed);

            // This must be the exact name of the bitmap font file we are using or it will error.
            string fontFileName = "terminal8x8.png";

            // The title will appear at the top of the console window
            string consoleTitle = $"AscII Game - Level {_mapLevel} - Seed {seed}";

            //Creating the new message log
            MessageLog = new MessageLog();
            MessageLog.Add("The rogue arrives on level 1");
            MessageLog.Add($"Level created with seed {seed}");

            Player = new Player();
            SchedulingSystem = new SchedulingSystem();

            MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 20, 13, 7, _mapLevel);
            DungeonMap = mapGenerator.CreateMap();

            // Tell RLNet to use the bitmap font that we specified and that each tile is 8 x 8 pixels
            _rootConsole = new RLRootConsole(fontFileName, _screenWidth, _screenHeight,
              8, 8, 1f, consoleTitle);
            _mapConsole = new RLConsole(_mapWidth, _mapHeight);
            _messageConsole = new RLConsole(_messageWidth, _messageHeight);
            _statConsole = new RLConsole(_statWidth, _statHeight);
            _inventoryConsole = new RLConsole(_inventoryWidth, _inventoryHeight);

            CommandSystem = new CommandSystem();

            _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Swatch.DbOldStone);
            _statConsole.Print(1, 1, "Stats", Colors.TextHeading);

            _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Swatch.DbWood);
            _inventoryConsole.Print(1, 1, "Inventory", Colors.TextHeading);



            //What allows the dungeon to be created
            DungeonMap.UpdatePlayerFieldOfView();

            // Set up a handler for RLNET's Update event
            _rootConsole.Update += OnRootConsoleUpdate;

            // Set up a handler for RLNET's Render event
            _rootConsole.Render += OnRootConsoleRender;

            // Begin RLNET's game loop
            _rootConsole.Run();

        }

        // Event handler for RLNET's Update event
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            bool didPlayerAct = false;
            RLKeyPress keyPress = _rootConsole.Keyboard.GetKeyPress();

            if (CommandSystem.IsPlayerTurn)
            {
                if (keyPress != null)
                {
                    if (keyPress.Key == RLKey.Up)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Up);
                    }
                    else if (keyPress.Key == RLKey.Down)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Down);
                    }
                    else if (keyPress.Key == RLKey.Left)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Left);
                    }
                    else if (keyPress.Key == RLKey.Right)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Right);
                    }
                    else if (keyPress.Key == RLKey.Escape)
                    {
                        _rootConsole.Close();
                    }
                    else if (keyPress.Key == RLKey.Period)
                    {
                        if (DungeonMap.CanMoveDownToNextLevel())
                        {   
                        MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 20, 13, 7, ++_mapLevel);
                        DungeonMap = mapGenerator.CreateMap();
                        MessageLog = new MessageLog();
                        CommandSystem = new CommandSystem();
                        _rootConsole.Title = $"AscII Game - Level {_mapLevel}";
                        didPlayerAct = true;
                        }
                    }
                }
                if (didPlayerAct)
                {
                    _renderRequired = true;
                    CommandSystem.EndPlayerTurn();
                }
            }
            else
            {
                CommandSystem.ActivateMonsters();
                _renderRequired = true;
            }
        }

        // Event handler for RLNET's Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {
            if(_renderRequired)
            {
            DungeonMap.Draw(_mapConsole, _statConsole);
            Player.Draw(_mapConsole, DungeonMap);
            Player.DrawStats(_statConsole);
            MessageLog.Draw(_messageConsole);

            RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight,
              _rootConsole, 0, _statHeight);
            RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight,
              _rootConsole, _mapWidth, 0);
            RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight,
              _rootConsole, 0, _screenHeight - _messageHeight);
            RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight,
              _rootConsole, 0, 0);
            // Tell RLNET to draw the console that we set
            _rootConsole.Draw();

            _renderRequired = false;
            }
        }

 
    }
}