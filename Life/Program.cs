using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace cli_life
{
    public class Configuration
    {
        public int width { get; set; }
        public int height { get; set; }
        public int cellSize { get; set; }
        public double density { get; set; }
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

        public int Columns { get { return Cells.GetLength(1); } }
        public int Rows { get { return Cells.GetLength(0); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity)
        {
            CellSize = cellSize;

            Cells = new Cell[height / cellSize, width / cellSize];
            for (int x = 0; x < Rows; x++)
                for (int y = 0; y < Columns; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            FileConfiguration(width, height, cellSize, liveDensity);
            //Randomize(liveDensity);
        }

        public int aliveCells()
        {
            int result = 0;
            foreach (var cell in Cells)
            {
                if (cell.IsAlive) result++;
            }
            return result;
        }

        public void GenStartCondition(int width, int height, int cellSize, double liveDensity)
        {
            Cell[,] Cells = new Cell[height / cellSize, width / cellSize];
            for (int x = 0; x < Rows; x++)
                for (int y = 0; y < Columns; y++)
                    Cells[x, y] = new Cell();
            Random rand = new Random();
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
            char[][] config = new char[Rows][];
            for (int i = 0; i < Rows; i++) config[i] = new char[Columns];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    var cell = Cells[i, j];
                    if (cell.IsAlive)
                    {
                        config[i][j] = '*';
                    }
                    else
                    {
                        config[i][j] = ' ';
                    }
                }
            }
            File.Create("startCondition.txt").Close();
            using (StreamWriter writer = new StreamWriter("startCondition.txt", true))
            {
                for (int i = 0; i < config.Length; i++)
                {
                    string str = new string(config[i]);
                    writer.WriteLine(str);
                }
            }
        }

        public void FileConfiguration(int width, int height, int cellSize, double liveDensity)
        {
            GenStartCondition(width, height, cellSize, liveDensity);
            string[] data = File.ReadAllLines("startCondition.txt");
            char[][] config = new char[Rows][];
            for (int i = 0; i < data.Length; ++i)
            {
                config[i] = new char[Columns];
                for (int j = 0; j < data[i].Length; ++j)
                {
                    config[i][j] = data[i][j];
                }
            }
            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Columns; ++j)
                {
                    if (config[i][j] == '*')
                    {
                        Cells[i, j].IsAlive = true;
                    }
                }
            }
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
            for (int x = 0; x < Rows; x++)
            {
                for (int y = 0; y < Columns; y++)
                {
                    int xL = (x > 0) ? x - 1 : Rows - 1;
                    int xR = (x < Rows - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Columns - 1;
                    int yB = (y < Columns - 1) ? y + 1 : 0;

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

        public int Boxes()
        {
            int result = 0;
            for (int i = 1; i < Rows - 1; i++)
            {
                for (int j = 0; j < Columns - 2; j++)
                {
                    if (Cells[i, j].IsAlive && Cells[i - 1, j + 1].IsAlive && Cells[i + 1, j + 1].IsAlive && 
                        Cells[i, j + 2].IsAlive && !Cells[i, j + 1].IsAlive && !Cells[i - 1, j].IsAlive &&
                        !Cells[i + 1, j].IsAlive && !Cells[i - 1, j + 2].IsAlive && !Cells[i + 1, j + 2].IsAlive)
                    {
                        result++;
                    }
                }
            }
            return result;
        }

        public int Hives()
        {
            int result = 0;
            for (int i = 1; i < Rows - 1; i++)
            {
                for (int j = 0; j < Columns - 3; j++)
                {
                    if (Cells[i, j].IsAlive && Cells[i - 1, j + 1].IsAlive && Cells[i - 1, j + 2].IsAlive && Cells[i, j + 3].IsAlive && 
                        Cells[i + 1, j + 1].IsAlive && Cells[i + 1, j + 2].IsAlive && !Cells[i, j + 1].IsAlive && !Cells[i, j + 2].IsAlive && 
                        !Cells[i - 1, j].IsAlive && !Cells[i + 1, j].IsAlive && !Cells[i - 1, j + 3].IsAlive && !Cells[i + 1, j + 3].IsAlive)
                    {
                        result++;
                    }
                }
            }
            return result;
        }

        public int Blocks()
        {
            int result = 0;
            for (int i = 1; i < Rows - 2; i++)
            {
                for (int j = 1; j < Columns - 2; j++)
                {
                    if (Cells[i, j].IsAlive && Cells[i, j + 1].IsAlive && Cells[i + 1, j].IsAlive && Cells[i + 1, j + 1].IsAlive)
                    {
                        if (!Cells[i - 1, j - 1].IsAlive && !Cells[i, j - 1].IsAlive && !Cells[i + 1, j - 1].IsAlive && !Cells[i + 2, j - 1].IsAlive && 
                            !Cells[i - 1, j + 2].IsAlive && !Cells[i, j + 2].IsAlive && !Cells[i + 1, j + 2].IsAlive && !Cells[i + 2, j + 2].IsAlive && 
                            !Cells[i - 1, j].IsAlive && !Cells[i + 2, j].IsAlive && !Cells[i - 1, j + 2].IsAlive && !Cells[i + 2, j + 2].IsAlive)
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }

    }
    class Program
    {
        static Board board;
        static private void Reset()
        {
            /*string json = File.ReadAllText(@"./Settings.json");
            Configuration data = JsonSerializer.Deserialize<Configuration>(json);
            int width = data.width;
            int height = data.height;
            int cellSize = data.cellSize;
            double density = data.density;
            */
            int width = 50;
            int height = 20;
            int cellSize = 1;
            double density = 0.5;
            board = new Board(width, height, cellSize, density);
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[row, col];
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

        public static void ToFile()
        {
            char[][] config = new char[board.Rows][];
            for (int i = 0; i < board.Rows; i++) config[i] = new char[board.Columns];
            for (int i = 0; i < board.Rows; i++)
            {
                for (int j = 0; j < board.Columns; j++)
                {
                    var cell = board.Cells[i, j];
                    if (cell.IsAlive)
                    {
                        config[i][j] = '*';
                    }
                    else
                    {
                        config[i][j] = ' ';
                    }
                }
            }
            File.Create("condition.txt").Close();
            using (StreamWriter writer = new StreamWriter("condition.txt", true))
            {
                for (int i = 0; i < config.Length; i++)
                {
                    string str = new string(config[i]);
                    writer.WriteLine(str);
                }
            }
        }


        static void Main(string[] args)
        {
            Reset();
            int iter = 0;
            int[] aliveCells = new int[7];
            while (true)
            {
                iter++;
                Console.Clear();
                Render();
                board.Advance();
                aliveCells[iter % 7] = board.aliveCells();
                ToFile();
                if (aliveCells[0] == aliveCells[1] && aliveCells[1] == aliveCells[2] && aliveCells[2] == aliveCells[3] && aliveCells[3] == aliveCells[4] &&
                    aliveCells[4] == aliveCells[5] && aliveCells[5] == aliveCells[6])
                {
                    Console.WriteLine("Стабильность на " + iter + " шаге");
                    Console.WriteLine("Живых клеток: " + aliveCells[0]);
                    Console.WriteLine("Мёртвых клеток: " + (board.Columns* board.Rows - aliveCells[0]));
                    break;
                }
                Thread.Sleep(500);
            }
            Console.WriteLine("Ящиков: " + board.Boxes());
            Console.WriteLine("Ульев: " + board.Hives());
            Console.WriteLine("Блоков: " + board.Blocks());
            Console.WriteLine("Всего симметричных фигур: " + (board.Blocks() + board.Hives() + board.Boxes()));
        }
    }
}