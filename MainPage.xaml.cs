using Microsoft.Maui.Controls;
using System;

namespace MauiGameOfLife
{
    public partial class MainPage : ContentPage
    {
        const int GridSize = 30;
        Cell[,] cells = new Cell[GridSize, GridSize];

        public MainPage()
        {
            InitializeComponent(); // Ensure this method is called in the constructor
            CreateGrid();
            InitializeGame();
        }

        void CreateGrid()
        {
            // Define rows and columns
            for (int i = 0; i < GridSize; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            // Create cells
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    var box = new BoxView
                    {
                        Color = Colors.White,
                        Margin = new Thickness(0.5) // Simulate border thickness
                    };

                    GameGrid.Add(box, col, row);

                    cells[row, col] = new Cell
                    {
                        Row = row,
                        Col = col,
                        IsAlive = false,
                        BoxView = box
                    };
                }
            }
        }

        void InitializeGame()
        {
            // Place a glider in the middle
            int midRow = GridSize / 2;
            int midCol = GridSize / 2;

            SetCellAlive(midRow, midCol);
            SetCellAlive(midRow, midCol + 1);
            SetCellAlive(midRow, midCol + 2);
            SetCellAlive(midRow + 1, midCol + 2);
            SetCellAlive(midRow + 2, midCol + 1);

            // Start the game loop
            Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
            {
                NextGeneration();
                return true; // Return true to keep the timer running
            });
        }

        void SetCellAlive(int row, int col)
        {
            if (row >= 0 && row < GridSize && col >= 0 && col < GridSize)
            {
                cells[row, col].IsAlive = true;
                UpdateCellVisual(cells[row, col]);
            }
        }

        void UpdateCellVisual(Cell cell)
        {
            cell.BoxView.Color = cell.IsAlive ? Colors.Black : Colors.White;
        }

        void NextGeneration()
        {
            var newStates = new bool[GridSize, GridSize];

            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    int aliveNeighbors = CountAliveNeighbors(row, col);
                    bool isAlive = cells[row, col].IsAlive;

                    newStates[row, col] = isAlive switch
                    {
                        true when aliveNeighbors < 2 => false,
                        true when aliveNeighbors == 2 || aliveNeighbors == 3 => true,
                        true when aliveNeighbors > 3 => false,
                        false when aliveNeighbors == 3 => true,
                        _ => isAlive
                    };
                }
            }

            // Update cells
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    cells[row, col].IsAlive = newStates[row, col];
                    UpdateCellVisual(cells[row, col]);
                }
            }
        }

        int CountAliveNeighbors(int row, int col)
        {
            int count = 0;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (y == 0 && x == 0)
                        continue;

                    int newRow = (row + y + GridSize) % GridSize; // Wrap around
                    int newCol = (col + x + GridSize) % GridSize; // Wrap around

                    if (cells[newRow, newCol].IsAlive)
                        count++;
                }
            }

            return count;
        }
    }

    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public bool IsAlive { get; set; }
        public BoxView BoxView { get; set; }
    }
}


public class Cell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public bool IsAlive { get; set; }
    public BoxView BoxView { get; set; }
}


