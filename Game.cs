namespace TicTacToe
{
    internal static class Game
    {
        // Properties

        private static int CurrentColumn { get; set; } = 1;

        private static int CurrentRow { get; set; } = 1;

        public static int PlayerXScore { get; private set; } = 0;

        public static int PlayerOScore { get; private set; } = 0;

        private static readonly char[,] array = new char[,]
        {
            { ' ', ' ', ' ' },
            { ' ', ' ', ' ' },
            { ' ', ' ', ' ' },
        };

        // Public methods

        public static void Run()
        {
            while (true)
            {
                Console.CursorVisible = false;
                Reset();
                PlayGame();
            }
        }

        // Private methods

        private static void PlayGame()
        {
            Console.Clear();

            bool playerX = true;
            int turnCounter = 1;
            while (!IsGameOver() && turnCounter != 10)
            {

                bool exitLoop = false;
                while (!exitLoop)
                {
                    // Draw the grid
                    Draw();

                    Console.Title = (playerX ? "Player X" : "Player O") + "'s turn";

                    // Navigate the grid
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.UpArrow:
                            CurrentRow--;
                            break;
                        case ConsoleKey.DownArrow:
                            CurrentRow++;
                            break;
                        case ConsoleKey.LeftArrow:
                            CurrentColumn--;
                            break;
                        case ConsoleKey.RightArrow:
                            CurrentColumn++;
                            break;
                        case ConsoleKey.Spacebar:
                        case ConsoleKey.Enter:
                            if (array[CurrentRow, CurrentColumn] == ' ')
                            {
                                // Set the tile
                                array[CurrentRow, CurrentColumn] = playerX ? 'X' : 'O';

                                // Switch turn
                                playerX = !playerX;

                                // Increment the turn counter
                                turnCounter++;

                                exitLoop = true;
                            }
                            break;
                    }

                    // Check range
                    CurrentRow = Math.Clamp(CurrentRow, 0, 2);
                    CurrentColumn = Math.Clamp(CurrentColumn, 0, 2);
                }
            }

            Console.Clear();

            // Increment scoring
            if (turnCounter != 10)
            {
                if (!playerX)
                    PlayerXScore++;
                else
                    PlayerOScore++;

                // Print out the victor
                Console.ForegroundColor = !playerX ? ConsoleColor.Blue : ConsoleColor.Green;
                Console.WriteLine("Congratulations to player " + (!playerX ? "X" : "O") + " for the victory!\n");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("It's a draw..");
            }

            // Write out scoring
            Console.WriteLine($"Player X score: {PlayerXScore}");
            Console.WriteLine($"Player O score: {PlayerOScore}");

            // Wait for the user to acknowledge
            Console.ReadKey(true);
        }

        private static void Reset()
        {
            // Reset the game
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    array[row, column] = ' ';
                }
            }

            CurrentColumn = 1;
            CurrentRow = 1;
        }

        private static void Draw()
        {
            Console.Clear();
            for (int row = 0; row < array.GetLength(0); row++)
            {
                if (row != 0) Console.WriteLine("----------");
                for (int column = 0; column < array.GetLength(1); column++)
                {
                    // Write the value of the current position in the array (or the cursor)
                    if (CurrentColumn == column && CurrentRow == row)
                    {
                        Console.Write("[");
                    }
                    else
                    {
                        Console.Write(" ");
                    }

                    char currentChar = array[row, column];
                    Console.ForegroundColor = currentChar == 'O' ? ConsoleColor.Green : currentChar == 'X' ? ConsoleColor.Blue : ConsoleColor.Gray;
                    Console.Write(array[row, column]);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    if (CurrentColumn == column && CurrentRow == row)
                    {
                        Console.Write("]");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    // Write separator
                    if (column != array.GetLength(1) - 1) Console.Write("|");
                }
                Console.WriteLine("");
            }
        }

        private static bool IsGameOver()
        {
            //TODO: logic here

            for (int i = 0; i < 2; i++)
            {
                char compareChar = i == 0 ? 'X' : 'O';

                // X | X | X
                // ---------
                // X | X | X
                // ---------
                // X | X | X

                if (array[0, 0] == compareChar && array[0, 1] == compareChar && array[0, 2] == compareChar) return true;
                if (array[1, 0] == compareChar && array[1, 1] == compareChar && array[1, 2] == compareChar) return true;
                if (array[2, 0] == compareChar && array[2, 1] == compareChar && array[2, 2] == compareChar) return true;

                if (array[0, 0] == compareChar && array[1, 0] == compareChar && array[2, 0] == compareChar) return true;
                if (array[0, 1] == compareChar && array[1, 1] == compareChar && array[2, 1] == compareChar) return true;
                if (array[0, 2] == compareChar && array[1, 2] == compareChar && array[2, 2] == compareChar) return true;

                if (array[0, 0] == compareChar && array[1, 1] == compareChar && array[2, 2] == compareChar) return true;
                if (array[2, 0] == compareChar && array[1, 1] == compareChar && array[0, 2] == compareChar) return true;
            }

            return false;
        }

    }
}