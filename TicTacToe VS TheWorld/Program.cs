using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TicTacToeCs
{
    enum Mode { Single, Multi }
    enum Difficulty { Easy, Medium, Hard }

    class Player
    {
        public string Name { get; set; } = "";
        public char Symbol { get; set; } = 'X';
        public ConsoleColor Color { get; set; } = ConsoleColor.White;
    }

    class DifficultyStats
    {
        public int Games, Wins, Losses, Draws;
        public double Ratio => Losses == 0 ? (Wins > 0 ? Wins : 0.0) : (double)Wins / Losses;
    }

    static class Program
    {
        // ---------- State ----------
        static char[,] board = new char[3, 3];
        static Mode mode = Mode.Single;
        static Difficulty difficulty = Difficulty.Hard;

        static Player P1 = new Player { Name = "You", Symbol = 'X', Color = ConsoleColor.Cyan };
        static Player P2 = new Player { Name = "AI", Symbol = 'O', Color = ConsoleColor.Yellow };

        static readonly Dictionary<Difficulty, DifficultyStats> stats = new Dictionary<Difficulty, DifficultyStats>
        {
            { Difficulty.Easy,   new DifficultyStats() },
            { Difficulty.Medium, new DifficultyStats() },
            { Difficulty.Hard,   new DifficultyStats() },
        };

        static readonly ConsoleColor[] AllowedColors =
        {
            ConsoleColor.Red, ConsoleColor.Green, ConsoleColor.Yellow,
            ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.Cyan, ConsoleColor.White
        };

        static readonly Random rng = new Random();

        // ---------- Entry ----------
        static void Main()
        {
            Console.Title = "Tic Tac Toe (C#)";
            SetupMode();

            do
            {
                ResetBoard();
                if (mode == Mode.Single) stats[difficulty].Games++;

                RunOneGame();

            } while (AskYesNo("\nPlay again? (y/n): ", 'y', 'n', onYes: () =>
            {
                if (AskYesNo("Change mode or difficulty? (y/n): ", 'y', 'n'))
                    SetupMode();
            }));
            Console.WriteLine("\nThanks for playing!");
        }
        // Returns true if the given symbol has a 3-in-a-row
        static bool CheckWin(char sym)
        {
            // rows
            for (int r = 0; r < 3; r++)
                if (board[r, 0] == sym && board[r, 1] == sym && board[r, 2] == sym)
                    return true;

            // cols
            for (int c = 0; c < 3; c++)
                if (board[0, c] == sym && board[1, c] == sym && board[2, c] == sym)
                    return true;

            // diagonals
            if (board[0, 0] == sym && board[1, 1] == sym && board[2, 2] == sym) return true;
            if (board[0, 2] == sym && board[1, 1] == sym && board[2, 0] == sym) return true;

            return false;
        }

        // ---------- Game Loop ----------
        static void RunOneGame()
        {
            bool gameOver = false;

            while (!gameOver)
            {
                PrintBoard();
                PlayerMove(P1);
                PrintBoard(); // refresh after P1

                if (CheckWin(P1.Symbol))
                {
                    Console.WriteLine($"🎉 {ColorName(P1)} WINS!");
                    if (mode == Mode.Single) stats[difficulty].Wins++;
                    gameOver = true;
                    break;
                }
                if (!IsMovesLeft()) break;

                if (mode == Mode.Multi)
                {
                    PlayerMove(P2);
                }
                else
                {
                    AiMove();
                }
                PrintBoard(); // refresh after P2/AI

                if (CheckWin(P2.Symbol))
                {
                    Console.WriteLine($"💀 {ColorName(P2)} WINS!");
                    if (mode == Mode.Single) stats[difficulty].Losses++;
                    gameOver = true;
                    break;
                }
                if (!IsMovesLeft())
                {
                    Console.WriteLine("🤝 It's a DRAW!");
                    if (mode == Mode.Single) stats[difficulty].Draws++;
                    break;
                }
            }

            if (mode == Mode.Single)
                PrintScoreboard();
        }

        // ---------- Setup ----------
        static void SetupMode()
        {
            Console.Clear();
            Console.WriteLine("🎮 Welcome to Tic Tac Toe (C#)\n");
            Console.WriteLine("Select mode: [1] Single Player   [2] Multiplayer");
            int choice = ReadIntInRange("> ", 1, 2);
            mode = (choice == 2) ? Mode.Multi : Mode.Single;

            if (mode == Mode.Multi)
            {
                // Names
                Console.Write("Enter Player 1 name: ");
                P1.Name = ReadNonEmpty();
                Console.Write("Enter Player 2 name: ");
                P2.Name = ReadNonEmpty();

                // Symbols
                P1.Symbol = 'X';
                P2.Symbol = 'O';

                // Colors
                Console.WriteLine("\nAvailable colors: " + string.Join(", ", AllowedColors.Select(c => c.ToString().ToLower())));
                P1.Color = ChooseColor($"{P1.Name}, choose your color: ");
                do
                {
                    P2.Color = ChooseColor($"{P2.Name}, choose your color (not {P1.Color.ToString().ToLower()}): ");
                } while (P2.Color == P1.Color);
            }
            else
            {
                // single player
                P1.Name = "You";
                P2.Name = "AI";
                P1.Symbol = 'X';
                P2.Symbol = 'O';

                Console.WriteLine("\nAvailable colors: " + string.Join(", ", AllowedColors.Select(c => c.ToString().ToLower())));
                P1.Color = ChooseColor("Choose your color: ");

                // AI gets a different random color
                var aiChoices = AllowedColors.Where(c => c != P1.Color).ToArray();
                P2.Color = aiChoices[rng.Next(aiChoices.Length)];

                Console.Write("\nSelect difficulty (1=easy, 2=medium, 3=hard): ");
                int d = ReadIntInRange("", 1, 3);
                difficulty = (Difficulty)(d - 1);
            }
        }

        // ---------- Board / Render ----------
        static void ResetBoard()
        {
            char c = '1';
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    board[i, j] = c++;
        }

        static void PrintBoard()
        {
            Console.Clear();
            Console.WriteLine("\n     TIC TAC TOE");
            if (mode == Mode.Single)
            {
                Console.Write("  Player: "); WriteColored(P1.Symbol, P1.Color);
                Console.Write("  |  AI: "); WriteColored(P2.Symbol, P2.Color);
                Console.WriteLine($"\n  Difficulty: {difficulty.ToString().ToLower()}");
            }
            else
            {
                Console.Write($"  {P1.Name} ("); WriteColored(P1.Symbol, P1.Color); Console.Write(") vs ");
                Console.Write($"{P2.Name} ("); WriteColored(P2.Symbol, P2.Color); Console.WriteLine(")");
            }
            Console.WriteLine();

            for (int i = 0; i < 3; i++)
            {
                Console.Write("     ");
                for (int j = 0; j < 3; j++)
                {
                    WriteCell(board[i, j]);
                    if (j < 2) Console.Write(" | ");
                }
                if (i < 2) Console.WriteLine("\n    -----------");
            }
            Console.WriteLine("\n");
        }

        static void WriteCell(char c)
        {
            if (c == P1.Symbol) WriteColored(c, P1.Color);
            else if (c == P2.Symbol) WriteColored(c, P2.Color);
            else Console.Write(c);
        }

        static void WriteColored(char c, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(c);
            Console.ForegroundColor = prev;
        }

        static string ColorName(Player p) => $"{p.Name} ({p.Symbol})";

        // ---------- Input ----------
        static void PlayerMove(Player p)
        {
            while (true)
            {
                Console.Write($"{p.Name} ("); WriteColored(p.Symbol, p.Color); Console.Write(") enter move (1-9): ");
                int move = ReadIntInRange("", 1, 9);
                int r = (move - 1) / 3, c = (move - 1) % 3;
                if (board[r, c] != 'X' && board[r, c] != 'O')
                {
                    board[r, c] = p.Symbol;
                    break;
                }
                Console.WriteLine("⚠️  Cell already taken. Try again.");
            }
        }

        static bool AskYesNo(string prompt, char yes, char no, Action onYes = null)
        {
            Console.Write(prompt);
            while (true)
            {
                var key = Console.ReadKey(intercept: true).KeyChar;
                if (char.ToLowerInvariant(key) == yes)
                {
                    Console.WriteLine("y");
                    onYes?.Invoke();
                    return true;
                }
                if (char.ToLowerInvariant(key) == no)
                {
                    Console.WriteLine("n");
                    return false;
                }
            }
        }

        static int ReadIntInRange(string prompt, int min, int max)
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(prompt)) Console.Write(prompt);
                string s = Console.ReadLine();
                if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
                Console.WriteLine($"❌ Enter a number between {min} and {max}.");
            }
        }

        static string ReadNonEmpty()
        {
            while (true)
            {
                string s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.Write("Please enter something: ");
            }
        }

        static ConsoleColor ChooseColor(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim().ToLower();
                foreach (var c in AllowedColors)
                {
                    if (c.ToString().ToLower() == input) return c;
                }
                Console.WriteLine("❌ Invalid color. Try one of: " + string.Join(", ", AllowedColors.Select(c => c.ToString().ToLower())));
            }
        }

        // ---------- AI ----------
        static void AiMove()
        {
            Console.WriteLine("🤖 AI is thinking...");
            Thread.Sleep(700);

            (int r, int c) move =
                difficulty == Difficulty.Easy ? RandomMove()
              : (difficulty == Difficulty.Medium && rng.Next(2) == 0) ? RandomMove()
              : BestMove();

            board[move.r, move.c] = P2.Symbol;
        }

        static (int r, int c) RandomMove()
        {
            var empty = new List<(int r, int c)>();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] != 'X' && board[i, j] != 'O') empty.Add((i, j));
            return empty[rng.Next(empty.Count)];
        }

        static (int r, int c) BestMove()
        {
            int bestVal = int.MinValue;
            (int r, int c) best = (-1, -1);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] != 'X' && board[i, j] != 'O')
                    {
                        char backup = board[i, j];
                        board[i, j] = P2.Symbol;
                        int val = Minimax(isMax: false);
                        board[i, j] = backup;

                        if (val > bestVal) { bestVal = val; best = (i, j); }
                    }
            return best;
        }

        static int Minimax(bool isMax)
        {
            int score = Evaluate();
            if (score == 10 || score == -10) return score;
            if (!IsMovesLeft()) return 0;

            int best = isMax ? int.MinValue : int.MaxValue;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] != 'X' && board[i, j] != 'O')
                    {
                        char backup = board[i, j];
                        board[i, j] = isMax ? P2.Symbol : P1.Symbol;
                        int val = Minimax(!isMax);
                        board[i, j] = backup;

                        if (isMax) best = Math.Max(best, val);
                        else best = Math.Min(best, val);
                    }
            return best;
        }

        static int Evaluate()
        {
            // rows
            for (int r = 0; r < 3; r++)
                if (board[r, 0] == board[r, 1] && board[r, 1] == board[r, 2])
                    return board[r, 0] == P2.Symbol ? 10 : (board[r, 0] == P1.Symbol ? -10 : 0);

            // cols
            for (int c = 0; c < 3; c++)
                if (board[0, c] == board[1, c] && board[1, c] == board[2, c])
                    return board[0, c] == P2.Symbol ? 10 : (board[0, c] == P1.Symbol ? -10 : 0);

            // diagonals
            if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
                return board[0, 0] == P2.Symbol ? 10 : (board[0, 0] == P1.Symbol ? -10 : 0);
            if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
                return board[0, 2] == P2.Symbol ? 10 : (board[0, 2] == P1.Symbol ? -10 : 0);

            return 0;
        }

        static bool IsMovesLeft()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] != 'X' && board[i, j] != 'O') return true;
            return false;
        }

        // ---------- Scoreboard ----------
        static void PrintScoreboard()
        {
            Console.WriteLine("\n🏆 SCOREBOARD (single-player totals)");
            Console.WriteLine("{0,-10}{1,8}{2,8}{3,10}{4,8}{5,12}",
                "Difficulty", "Games", "Wins", "Losses", "Draws", "W/L Ratio");
            Console.WriteLine(new string('-', 56));

            foreach (var kv in stats)
            {
                var name = kv.Key.ToString().ToLower();
                var s = kv.Value;
                Console.WriteLine("{0,-10}{1,8}{2,8}{3,10}{4,8}{5,12:F2}",
                    name, s.Games, s.Wins, s.Losses, s.Draws, s.Ratio);
            }
            Console.WriteLine();
        }
    }
}
