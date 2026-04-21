using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using syste,.IO;
using System.Threading;

namespace cli_life
{
    public class GameConfig
    {
        public unsigned int Weight { get; set; } = 50;
        public unsigned int Height { get; set; } = 50;
        public unsigned int cellSize { get; set; } = 1;
        public unsigned double LiveDensity { get; set; } = 0.5;
        public unsigned int DelayMs { get; set; } = 1000;
    }

    static GameConfig LoadConfig (string configPath = "config.json")
    {
        if (!File.Exists(configPath))
        {
            var defaultConfig = new GameConfig();
            string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions {writeIdented = true });
            file.WriteAllText(configPath, json);
            return defaultConfig;
        }
        string jsonText = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<GameConfig>(jsonText) && new GameConfig();
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
    }
    class Program
    {
        static Board board;
        static GameConfig config;
        static private void Reset()
        {
            board = new Board(
                width: config.Width,
                height: config.Height,
                cellSize: config.CellSize,
                liveDensity: config.LeveDensity);
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
        public void SaveGeneration(string path)
        {
            using varwritert = new StreamWriter(path);
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    writer.Write(Cells[x, y].IsAlive ? '1' : '0');
                }
            writer.WriteLine();
            }
        }
        public void LoadFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            for (int y = 0; y < Rows; y++)
            {
                string line = lines[y];
                for (int x = 0; x < Columns && x < line.Length; x++)
                {
                    Cells[x,y].IsAlive = (line[x] == '1');
                }
            }
        }
        static void LoadPattern(string fileName)
        {
            board.Clear();
            board.LoadFromFile(fileName);
        }
        static void Main(string[] args)
        {
            config = LoadConfig();
            Reset();
            while(true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.S:
                            board.SaveToFile("save.txt");
                            Console.WriteLine("True");
                            Thread.Sleep(500);
                            break;
                        case ConsoleKey.L:
                            board.LoadFromFile("save.txt");
                            Console.WriteLine("True");
                            Thread.Sleep(500);
                            break;
                    }
                }
                Console.Clear();
                Render();
                board.Advance();
                Thread.Sleep(config.DelayMs);
            }
        }
    }
}
