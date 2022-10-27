namespace TicTacToe
{
    internal static class PentapatchGame
    {
        // ############
        // ## FIELDS ##
        // ############

        private static char[,] grid = new char[0, 0];

        private static int _row = 0;
        private static int _column = 0;

        private static int prevRow;
        private static int prevColumn;
        private static int boardLeft = 20;
        private static int boardTop;

        private static char turn = 'X';

        private static readonly Random rng = new();

        // ###################
        // ## Configuration ##
        // ###################

        private const char BlockerChar = '#';

        private const char SurpriseChar = '?';

        // ################
        // ## Properties ##
        // ################

        public static bool UseBlockers { get; set; } = true;

        public static double BlockerFactor { get; set; } = 0.6;

        public static int ConnectedTilesOffset { get; set; } = 0;

        public static Dictionary<int, int> ConnectedTileRequirements { get; set; } = new()
        {
            { 3, 3 },
            { 5, 4 },
            { 7, 5 },
            { 9, 5 },
        };

        public static bool UseSurprises { get; set; } = true;

        public static double SurpriseFactor { get; set; } = 0.9;

        private static int Row
        {
            get => _row;
            set => _row = Math.Clamp(value, 0, grid.GetLength(0) - 1);
        }

        private static int Column
        {
            get => _column;
            set => _column = Math.Clamp(value, 0, grid.GetLength(1) - 1);
        }

        // ##################
        // ## Constructors ##
        // ##################

        static PentapatchGame() { }

        // ####################
        // ## Public methods ##
        // ####################

        public static void Play() => Play(grid.GetLength(0));

        public static void Play(int size) => Play(size, size);

        public static void Play(int width, int height)
        {
            Reset(width, height);

            bool exitGame = false;
            while (!exitGame)
            {
                Reset(grid.GetLength(0));

                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();

                DrawLogotype();
                DrawBoard();
                RefreshAffectedTiles();

                var gameState = (State: -1, Tiles: new List<(int, int)>());
                bool gameOver = false;
                while (!gameOver)
                {
                    prevRow = Row;
                    prevColumn = Column;

                    ConsoleKeyInfo key = ExpectKey(ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.UpArrow, ConsoleKey.DownArrow,
                                                   ConsoleKey.Spacebar, ConsoleKey.Enter, ConsoleKey.F5, ConsoleKey.D3, ConsoleKey.D5,
                                                   ConsoleKey.D7, ConsoleKey.D9, ConsoleKey.F2, ConsoleKey.F1, ConsoleKey.Escape);

                    switch (key.Key)
                    {
                        case ConsoleKey.LeftArrow:
                            Column--;
                            break;
                        case ConsoleKey.RightArrow:
                            Column++;
                            break;
                        case ConsoleKey.UpArrow:
                            Row--;
                            break;
                        case ConsoleKey.DownArrow:
                            Row++;
                            break;
                        case ConsoleKey.F5:
                            Reset(grid.GetLength(0));
                            Play();
                            return;
                        case ConsoleKey.D3:
                            Reset(3);
                            Play();
                            return;
                        case ConsoleKey.D5:
                            Reset(5);
                            Play();
                            return;
                        case ConsoleKey.D7:
                            Reset(7);
                            Play();
                            return;
                        case ConsoleKey.D9:
                            Reset(9);
                            Restart();
                            return;
                        case ConsoleKey.F2:
                            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            {
                                SurpriseFactor += 0.1;
                            }
                            else if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                            {
                                SurpriseFactor -= 0.1;
                            }
                            else
                            {
                                UseSurprises = !UseSurprises;
                            }
                            Restart();
                            return;
                        case ConsoleKey.F1:
                            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            {
                                BlockerFactor += 0.1;
                            }
                            else if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                            {
                                BlockerFactor -= 0.1;
                            }
                            else
                            {
                                UseBlockers = !UseBlockers;
                            }
                            Restart();
                            return;
                        case ConsoleKey.Escape:
                            exitGame = true;
                            return;
                        case ConsoleKey.Spacebar:
                        case ConsoleKey.Enter:
                            // Check if the move is valid

                            bool validTurn = false;
                            if (grid[Row, Column] == ' ')
                            {
                                // Set the value
                                grid[Row, Column] = turn;
                                validTurn = true;
                            }
                            else if (grid[Row, Column] == '?')
                            {
                                SelectAndDoSurprise();
                                validTurn = true;
                            }

                            if (validTurn)
                            {
                                // Check the state of the game
                                gameState = GetGameState();

                                if (gameState.State != -1)
                                {
                                    gameOver = true;
                                    break;
                                }

                                // Flip turn
                                turn = turn == 'X' ? 'O' : 'X';
                            }
                            break;
                    }

                    RefreshAffectedTiles();
                }

                // Mark the winning tiles (if any)
                MarkGameStateTiles(gameState.State, gameState.Tiles);

                Console.ReadKey(true);
            }

            Console.CursorVisible = true;
        }

        public static void PrepareConsole(int width = 42, int height = 34)
        {
            Console.CursorVisible = false;
            Console.Title = "TicTacToe+ by Pentapatch";
            Console.WindowWidth = width;
            Console.WindowHeight = height;
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight); // <-- Remove the scrollbar
        }

        public static void Restart()
        {
            Reset(grid.GetLength(0));
            Play();
        }

        // ###################################
        // ## Private console write methods ##
        // ###################################

        private static void DrawBoard()
        {
            boardTop = Console.CursorTop;

            int width = 4 * grid.GetLength(1) + 1;
            boardLeft = 21 - width / 2;

            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write(new String(' ', boardLeft));
            Console.WriteLine("/" + new string('-', 4 * grid.GetLength(1) - 1) + @"\");

            for (int r = 0; r < grid.GetLength(0); r++)
            {
                Console.Write(new String(' ', boardLeft));

                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    Console.Write("| ");
                    Console.ForegroundColor = GetCharColor(grid[r, c]);
                    Console.Write(grid[r, c] + " ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                Console.WriteLine("|");

                if (r != grid.GetLength(0) - 1)
                {
                    string breaker = "|";
                    for (int i = 0; i < grid.GetLength(1); i++)
                    {
                        breaker += new string('-', 3) + "+";
                    }
                    Console.Write(new String(' ', boardLeft));
                    Console.WriteLine(breaker[..^1] + "|");
                }

            }

            Console.Write(new String(' ', boardLeft));
            Console.WriteLine(@"\" + new string('-', 4 * grid.GetLength(1) - 1) + "/");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            PrintOption("F5: Reset game");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            PrintOption("3: New 3x3 board");
            PrintOption("5: New 5x5 board");
            PrintOption("7: New 7x7 board");
            PrintOption("9: New 9x9 board");
            PrintOption("F1: " + (UseBlockers ? "Disable blocker " : "Enable blocker"));
            PrintOption("F2: " + (UseSurprises ? "Disable surprise" : "Enable surprise "));
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void DrawLogotype()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("  _____ _     ");
            Console.WriteLine(" |_   _|_|___");
            Console.WriteLine("   | | | |  _|");
            Console.WriteLine("   |_| |_|___|");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(14, 0);
            Console.Write("_____         ");
            Console.SetCursorPosition(13, 1);
            Console.Write("|_   _|___ ___");
            Console.SetCursorPosition(14, 2);
            Console.Write(" | | | .'|  _|");
            Console.SetCursorPosition(14, 3);
            Console.Write(" |_| |__,|___|");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(28, 0);
            Console.Write("_____         ");
            Console.SetCursorPosition(27, 1);
            Console.Write("|_   _|___ ___ ");
            Console.SetCursorPosition(28, 2);
            Console.Write(" | | | . | -_|");
            Console.SetCursorPosition(28, 3);
            Console.Write(" |_| |___|___|");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" --- A game upgrade by Dennis Hankvist ---\n");

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void MarkGameStateTiles(int state, List<(int Row, int Column)> tiles)
        {
            if (state == 1 || state == 2)
            {
                foreach (var tile in tiles)
                {
                    // Redraw the tile
                    Console.BackgroundColor = GetCharColor((state == 1 ? 'X' : 'O'));
                    Console.SetCursorPosition(tile.Column * 4 + 1 + boardLeft, tile.Row * 2 + 1 + boardTop);

                    Console.ForegroundColor = GetCharColor(grid[tile.Row, tile.Column]);
                    Console.Write(" " + grid[tile.Row, tile.Column] + " ");
                }
            }
            else
            {
                for (int r = 0; r < grid.GetLength(0); r++)
                {
                    for (int c = 0; c < grid.GetLength(1); c++)
                    {
                        // Redraw the tile
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.SetCursorPosition(c * 4 + 1 + boardLeft, r * 2 + 1 + boardTop);

                        Console.ForegroundColor = GetCharColor(grid[r, c]);
                        Console.Write(" " + grid[r, c] + " ");
                    }
                }
            }
        }

        private static void PrintOption(string text)
        {
            Console.Write(new String(' ', 21 - $"- {text} - ".Length / 2));
            Console.WriteLine($"- {text} -");
        }

        private static void RefreshAffectedTiles()
        {
            // Redraw the previous tile (if it needs to)
            if (!(prevRow == Row && prevColumn == Column))
            {
                Console.SetCursorPosition(prevColumn * 4 + 1 + boardLeft, prevRow * 2 + 1 + boardTop);
                Console.ForegroundColor = GetCharColor(grid[prevRow, prevColumn]);
                Console.Write($" {grid[prevRow, prevColumn]} ");
            }

            // Redraw the new tile
            Console.SetCursorPosition(Column * 4 + 1 + boardLeft, Row * 2 + 1 + boardTop);
            Console.ForegroundColor = GetCharColor(turn);
            Console.Write("[");

            Console.ForegroundColor = GetCharColor(grid[Row, Column]);
            Console.Write(grid[Row, Column]);

            Console.ForegroundColor = GetCharColor(turn);
            Console.Write("]");
        }

        // ################################
        // ## Private game logic methods ##
        // ################################

        private static (int State, List<(int Row, int Column)> Tiles) GetGameState()
        {
            int tilesRequired = (int)Math.Sqrt(grid.GetLength(0) * (double)grid.GetLength(1));

            tilesRequired = tilesRequired switch
            {
                9 => 5 - ConnectedTilesOffset,
                7 => 5 - ConnectedTilesOffset,
                5 => 4 - ConnectedTilesOffset,
                _ => 3,
            };

            if (tilesRequired < 3) tilesRequired = 3;

            int emptyCount = 0;
            for (int r = 0; r < grid.GetLength(0); r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    char value = grid[r, c];

                    if (value == ' ') emptyCount++;
                    if (value != 'X' && value != 'O') continue;

                    var horizontal = CountHorizontal(r, c);
                    var vertical = CountVertical(r, c);
                    var diagonalDown = CountDiagonalDown(r, c);
                    var diagonalUp = CountDiagonalUp(r, c);

                    if (horizontal.Count >= tilesRequired)
                    {
                        return (value == 'X' ? 1 : 2, horizontal.Tiles);
                    }
                    else if (vertical.Count >= tilesRequired)
                    {
                        return (value == 'X' ? 1 : 2, vertical.Tiles);
                    }
                    else if (diagonalDown.Count >= tilesRequired)
                    {
                        return (value == 'X' ? 1 : 2, diagonalDown.Tiles);
                    }
                    else if (diagonalUp.Count >= tilesRequired)
                    {
                        return (value == 'X' ? 1 : 2, diagonalUp.Tiles);
                    }
                }
            }

            return (emptyCount == 0 ? 0 : -1, new List<(int, int)>());
        }

        private static (int Count, List<(int Row, int Column)> Tiles) CountHorizontal(int row, int column)
        {
            char value = grid[row, column];
            int count = 0;

            List<(int Row, int Column)> tiles = new();

            for (int c = column; c < grid.GetLength(1); c++)
            {
                if (grid[row, c] == value)
                {
                    tiles.Add((row, c));
                    count++;
                }
                else
                    return (count, tiles);
            }

            return (count, tiles);
        }

        private static (int Count, List<(int Row, int Column)> Tiles) CountVertical(int row, int column)
        {
            char value = grid[row, column];
            int count = 0;

            List<(int Row, int Column)> tiles = new();

            for (int r = row; r < grid.GetLength(0); r++)
            {
                if (grid[r, column] == value)
                {
                    tiles.Add((r, column));
                    count++;
                }
                else
                    return (count, tiles);
            }

            return (count, tiles);
        }

        private static (int Count, List<(int Row, int Column)> Tiles) CountDiagonalDown(int row, int column)
        {
            char value = grid[row, column];
            int count = 0;

            List<(int Row, int Column)> tiles = new();

            int c = column;
            for (int r = row; r < grid.GetLength(0); r++)
            {
                if (grid[r, c] == value)
                {
                    tiles.Add((r, c));
                    count++;
                }
                else
                    return (count, tiles);

                c++;
                if (c >= grid.GetLength(1)) return (count, tiles);
            }

            return (count, tiles);
        }

        private static (int Count, List<(int Row, int Column)> Tiles) CountDiagonalUp(int row, int column)
        {
            char value = grid[row, column];
            int count = 0;

            List<(int Row, int Column)> tiles = new();

            int c = column;
            for (int r = row; r >= 0; r--)
            {
                if (grid[r, c] == value)
                {
                    tiles.Add((r, c));
                    count++;
                }
                else
                    return (count, tiles);

                c++;
                if (c >= grid.GetLength(1)) return (count, tiles);
            }

            return (count, tiles);
        }

        // ##############################
        // ## Suprise handling methods ##
        // ##############################

        private static void SelectAndDoSurprise()
        {
            switch (rng.NextDouble())
            {
                case <= 0.4:  // Set tile to a random player
                    grid[Row, Column] = rng.NextDouble() <= 0.5 ? 'X' : 'O';
                    break;
                case <= 0.55: // Convert to a blocker
                    grid[Row, Column] = BlockerChar;
                    break;
                case <= 0.7: // Clear the tile & remove random blocker
                    SurpriseRemoveRandomBlocker();
                    break;
                case <= 0.85:
                    SurpriseSwapRandomOpponentTile();
                    break;
                default:      // Clear the tile & randomly move the surprise
                    SurpriseMoveSurprise();
                    break;
            }
        }

        private static void SurpriseMoveSurprise()
        {
            List<(int Row, int Column)> emptyList = new List<(int, int)>();
            for (int r = 0; r < grid.GetLength(0); r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    if (grid[r, c] == ' ') emptyList.Add((r, c));
                }
            }

            if (emptyList.Count > 0)
            {
                var tile = emptyList[rng.Next(emptyList.Count)];
                grid[tile.Row, tile.Column] = SurpriseChar;

                prevColumn = tile.Column;
                prevRow = tile.Row;
            }

            grid[Row, Column] = ' ';
            RefreshAffectedTiles();
        }

        private static void SurpriseRemoveRandomBlocker()
        {
            List<(int Row, int Column)> blockerList = new List<(int, int)>();
            for (int r = 0; r < grid.GetLength(0); r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    if (grid[r, c] == BlockerChar) blockerList.Add((r, c));
                }
            }

            if (blockerList.Count > 0)
            {
                var blocker = blockerList[rng.Next(blockerList.Count)];
                grid[blocker.Row, blocker.Column] = ' ';

                prevColumn = blocker.Column;
                prevRow = blocker.Row;
            }

            grid[Row, Column] = ' ';
            RefreshAffectedTiles();
        }

        private static void SurpriseSwapRandomOpponentTile()
        {
            char oppenentChar = turn == 'X' ? 'O' : 'X';
            List<(int Row, int Column)> oppenentTiles = new List<(int, int)>();
            for (int r = 0; r < grid.GetLength(0); r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    if (grid[r, c] == oppenentChar) oppenentTiles.Add((r, c));
                }
            }

            if (oppenentTiles.Count > 0)
            {
                var tile = oppenentTiles[rng.Next(oppenentTiles.Count)];
                grid[tile.Row, tile.Column] = turn;

                prevColumn = tile.Column;
                prevRow = tile.Row;
            }

            grid[Row, Column] = oppenentChar;
            RefreshAffectedTiles();
        }

        // ############################
        // ## Private helper methods ##
        // ############################

        private static ConsoleKeyInfo ExpectKey(params ConsoleKey[] keys)
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (keys.Contains(key.Key)) return key;
            }
        }

        private static ConsoleColor GetCharColor(char value) => value switch
        {
            'X' => ConsoleColor.Blue,
            'O' => ConsoleColor.Green,
            BlockerChar => ConsoleColor.Red,
            SurpriseChar => ConsoleColor.Magenta,
            _ => ConsoleColor.Gray,
        };

        private static void Reset(int size = 3) => Reset(size, size);

        private static void Reset(int rows, int columns)
        {
            grid = new char[rows, columns];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    grid[r, c] = ' ';
                }
            }

            // Blockers
            if (UseBlockers && rows >= 5 && columns >= 5)
            {
                int numberOfBlockers = (int)(((rows * columns) * 0.075D) * BlockerFactor);

                while (numberOfBlockers > 0)
                {
                    int index = rng.Next(rows * columns);
                    int row = index / rows;
                    int column = index - row * columns;

                    if (grid[row, column] == ' ')
                    {
                        grid[row, column] = BlockerChar;
                        numberOfBlockers--;
                    }
                }
            }

            // Surprises
            if (UseSurprises && rows >= 5 && columns >= 5)
            {
                int numberOfSurprises = (int)(((rows * columns) * 0.075D) * SurpriseFactor);

                while (numberOfSurprises > 0)
                {
                    int index = rng.Next(rows * columns);
                    int row = index / rows;
                    int column = index - row * columns;

                    if (grid[row, column] == ' ')
                    {
                        grid[row, column] = SurpriseChar;
                        numberOfSurprises--;
                    }
                }
            }

            Row = rows / 2;
            Column = columns / 2;

            prevRow = Row;
            prevColumn = Column;

            turn = 'X';
        }

    }
}