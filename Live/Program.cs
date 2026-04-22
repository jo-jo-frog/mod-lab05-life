using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace cli_life
{
    public class GameConfig
    {
        public int Wight { get; set; } = 50;
        public int Height { get; set; } = 50;
        public int CellSize { get; set; } = 1;
        public double LiveDensity { get; set; } = 0.5;
        public int DelayMs { get; set; } = 1000;
    }

    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }

    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        public int CountAlive()
        {
            int count = 0;
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    if (Cells[x, y].IsAlive) count++;
            return count;
        }

        public List<(int x, int y)> GetAliveCells()
        {
            var list = new List<(int, int)>();
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    if (Cells[x, y].IsAlive)
                        list.Add((x, y));
            return list;
        }

        public int CountCombinations()
        {
            var visited = new bool[Columns, Rows];
            int combinations = 0;
            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive && !visited[x, y])
                    {
                        combinations++;
                        var queue = new Queue<(int x, int y)>();
                        queue.Enqueue((x, y));
                        visited[x, y] = true;
                        while (queue.Count > 0)
                        {
                            var (cx, cy) = queue.Dequeue();
                            for (int d = 0; d < 8; d++)
                            {
                                int nx = cx + dx[d];
                                int ny = cy + dy[d];
                                if (nx >= 0 && nx < Columns && ny >= 0 && ny < Rows &&
                                    Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue((nx, ny));
                                }
                            }
                        }
                    }
                }
            }
            return combinations;
        }

        public List<List<(int x, int y)>> GetAllCombinations()
        {
            var visited = new bool[Columns, Rows];
            var combinations = new List<List<(int, int)>>();
            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive && !visited[x, y])
                    {
                        var combo = new List<(int, int)>();
                        var queue = new Queue<(int x, int y)>();
                        queue.Enqueue((x, y));
                        visited[x, y] = true;
                        combo.Add((x, y));
                        while (queue.Count > 0)
                        {
                            var (cx, cy) = queue.Dequeue();
                            for (int d = 0; d < 8; d++)
                            {
                                int nx = cx + dx[d];
                                int ny = cy + dy[d];
                                if (nx >= 0 && nx < Columns && ny >= 0 && ny < Rows &&
                                    Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue((nx, ny));
                                    combo.Add((nx, ny));
                                }
                            }
                        }
                        combinations.Add(combo);
                    }
                }
            }
            return combinations;
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }

        public void SaveGenerationToFile(string path)
        {
            using var writer = new StreamWriter(path);
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    writer.Write(Cells[x, y].IsAlive ? '1' : '0');
                }
                writer.WriteLine();
            }
        }

        public void LoadGenerationFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            for (int y = 0; y < Rows; y++)
            {
                string line = lines[y];
                for (int x = 0; x < Columns && x < line.Length; x++)
                {
                    Cells[x, y].IsAlive = (line[x] == '1');
                }
            }
        }

        public void Clear()
        {
            for (int x = 0; x < Columns; x++) 
            {
                for (int y = 0; y < Rows; y++) 
                {
                    Cells[x, y].IsAlive = false;
                }
            }
        }

        public void LoadPatternAtCenter(string path)
        {
            Clear();
            var lines = File.ReadAllLines(path);
            int patternHeight = lines.Length;
            if (patternHeight == 0) return;
            int patternWidth = lines[0].Length;

            int startX = (Columns - patternWidth) / 2;
            int startY = (Rows - patternHeight) / 2;

            for (int y = 0; y < patternHeight; y++)
            {
                string line = lines[y];
                for (int x = 0; x < patternWidth && x < line.Length; x++)
                {
                    if (line[x] == '1')
                    {
                        int boardX = startX + x;
                        int boardY = startY + y;
                        if (boardX >= 0 && boardX < Columns && boardY >= 0 && boardY < Rows)
                            Cells[boardX, boardY].IsAlive = true;
                    }
                }
            }
        }
    }

    public class Program
    {
        static Board board = null!;
        static GameConfig config = null!;
        static Dictionary<string, HashSet<(int x, int y)>> _shapes = new();

        static Program()
        {
            InitShapes();
        }

        public static void InitShapes()
        {
            // Блок 2x2
            var block = new HashSet<(int, int)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            _shapes["Block"] = block;

            // Улей
            var beehive = new HashSet<(int, int)> { (1, 0), (2, 0), (0, 1), (3, 1), (1, 2), (2, 2) };
            _shapes["Beehive"] = beehive;

            // Лодка
            var boat = new HashSet<(int, int)> { (0, 0), (1, 0), (0, 1), (2, 1), (1, 2) };
            _shapes["Boat"] = boat;

            // Пруд 4x4
            var pool = new HashSet<(int, int)> { (1, 0), (2, 0), (0, 1), (3, 1), (0, 2), (3, 2), (1, 3), (2, 3) };
            _shapes["Pool"] = pool;

            // Корабль 3x3
            var ship = new HashSet<(int, int)> { (0, 0), (1, 0), (2, 0), (0, 1), (2, 1), (2, 2) };
            _shapes["Ship"] = ship;

            // Хлеб
            var loaf = new HashSet<(int, int)> { (1, 0), (2, 0), (0, 1), (3, 1), (1, 2), (3, 2), (2, 3) };
            _shapes["Loaf"] = loaf;
        }

        static HashSet<(int x, int y)> Normalize(List<(int x, int y)> combo)
        {
            int minX = combo.Min(c => c.x);
            int minY = combo.Min(c => c.y);
            var norm = new HashSet<(int, int)>();
            foreach (var (x, y) in combo)
                norm.Add((x - minX, y - minY));
            return norm;
        }

        static string ClassifyCombination(List<(int x, int y)> combo)
        {
            var norm = Normalize(combo);
            foreach (var (name, shape) in _shapes)
            {
                if (norm.SetEquals(shape))
                    return name;
            }
            return "Unknown";
        }

        static private void Reset()
        {
            board = new Board(
                width: config.Wight,
                height: config.Height,
                cellSize: config.CellSize,
                liveDensity: config.LiveDensity);
        }

        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }

        static void RunStabilityExperiments()
        {
            double[] densities = { 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5 };
            int attemptsPerDensity = 20;
            var results = new List<(double density, double avgGenerations)>();

            using var sw = new StreamWriter("Data/data.txt");
            sw.WriteLine("Density\tAvgGenerations");

            foreach (double d in densities)
            {
                Console.WriteLine($"Testing density {d}...");
                var gens = new List<int>();
                for (int i = 0; i < attemptsPerDensity; i++)
                {
                    int stableGen = SimulateUntilStable(d);
                    if (stableGen > 0)
                        gens.Add(stableGen);
                }
                double avg = gens.Count > 0 ? gens.Average() : 0;
                results.Add((d, avg));
                sw.WriteLine($"{d}\t{avg}");
                Console.WriteLine($"  Average: {avg:F1} generations");
            }

            var plt = new ScottPlot.Plot();
            double[] x = results.Select(r => r.density).ToArray();
            double[] y = results.Select(r => r.avgGenerations).ToArray();
            plt.Add.Scatter(x, y);
            plt.Title("Переход в стабильную фазу");
            plt.XLabel("Плотность заполнения");
            plt.YLabel("Среднее число поколений до стабилизации");
            plt.SavePng("Data/plot.png", 800, 600);
        }

        static int SimulateUntilStable(double density, int stableWindow = 5)
        {
            var simBoard = new Board(50, 50, 1, density);
            var history = new Queue<int>();
            int generation = 0;
            int lastStableGen = -1;

            while (true)
            {
                int alive = simBoard.CountAlive();
                history.Enqueue(alive);
                if (history.Count > stableWindow)
                    history.Dequeue();

                if (history.Count == stableWindow && history.Distinct().Count() == 1)
                {
                    lastStableGen = generation - stableWindow + 1;
                    break;
                }

                simBoard.Advance();
                generation++;
                if (generation > 10000)
                {
                    lastStableGen = -1;
                    break;
                }
            }
            return lastStableGen;
        }

        static GameConfig LoadConfig()
        {
            string configPath = "config.json";
            if (!File.Exists(configPath))
            {
                var defaultConfig = new GameConfig();
                string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
                return defaultConfig;
            }
            string jsonText = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<GameConfig>(jsonText) ?? new GameConfig();
        }

        static void LoadPattern(string fileName)
        {
            board.Clear();
            board.LoadPatternAtCenter(fileName);
            Thread.Sleep(500);
        }

        static void Main(string[] args)
        {
            config = LoadConfig();
            Reset();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.S:
                            board.SaveGenerationToFile("save.txt");
                            Console.WriteLine("True");
                            Thread.Sleep(500);
                            break;
                        case ConsoleKey.L:
                            board.LoadGenerationFromFile("save.txt");
                            Console.WriteLine("True");
                            Thread.Sleep(500);
                            break;
                        case ConsoleKey.F1:
                            LoadPattern("Block.txt");
                            Console.WriteLine("структура: устойчивая(блок)");
                            break;
                        case ConsoleKey.F2:
                            LoadPattern("Blinker.txt");
                            Console.WriteLine("структура: периодическая(мигалка)");
                            break;
                        case ConsoleKey.F3:
                            LoadPattern("Glider.txt");
                            Console.WriteLine("структура: двигающаяся");
                            break;
                        case ConsoleKey.F4:
                            LoadPattern("GliderEater.txt");
                            Console.WriteLine("структура: пожиратель");
                            break;
                        case ConsoleKey.F5:
                            LoadPattern("Gun.txt");
                            Console.WriteLine("структура: ружье");
                            break;
                        case ConsoleKey.F6: // в ходе экспериментов выяснилось, что поезд на поле 50*50 не работает(вырождается в блоки), нужно делать большеее поле и наблюдать за поведением
                            LoadPattern("Train.txt");
                            Console.WriteLine("структура: поезд");
                            break;
                        case ConsoleKey.F9:
                            Console.Clear();
                            int alive = board.CountAlive();
                            int combos = board.CountCombinations();
                            Console.WriteLine($"Живых клеток: {alive}");
                            Console.WriteLine($"Комбинаций: {combos}");

                            var allCombos = board.GetAllCombinations();
                            Console.WriteLine("Классификация комбинаций:");
                            foreach (var combo in allCombos)
                            {
                                string type = ClassifyCombination(combo);
                                Console.WriteLine($"  {type} (размер {combo.Count})");
                            }
                            Console.WriteLine("Нажмите любую клавишу...");
                            Console.ReadKey();
                            break;
                        case ConsoleKey.F10:
                            Console.Clear();
                            Console.WriteLine("Запуск экспериментов по стабилизации...");
                            RunStabilityExperiments();
                            Console.WriteLine("График сохранён в Data/plot.png, данные в Data/data.txt");
                            Console.WriteLine("Нажмите любую клавишу для возврата...");
                            Console.ReadKey();
                            break;
                    }
                }
                Console.Clear();
                Render();
                board.Advance();
                Thread.Sleep(config.DelayMs);
            }
        }
        public static Dictionary<string, HashSet<(int x, int y)>> GetShapes() => _shapes;
        public static HashSet<(int x, int y)> NormalizeShape(List<(int x, int y)> combo) => Normalize(combo);
        public static string ClassifyShape(List<(int x, int y)> combo) => ClassifyCombination(combo);
    }
}