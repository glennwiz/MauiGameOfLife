using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace MauiGameOfLife
{
    public partial class MainPage : ContentPage
    {
        const int GridSize = 30;
        const int CellSize = 20; // Adjust the cell size as needed
        Cell[,] cells = new Cell[GridSize, GridSize];

        bool isRunning = false;
        CancellationTokenSource cancellationTokenSource;

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
            InitializeGame(); // Initialize the game and start the simulation
        }

        void CreateGrid()
        {
            // Define rows and columns with fixed sizes
            for (int i = 0; i < GridSize; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });
            }

            // Create cells using Border for borders
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    var border = new Border
                    {
                        Stroke = Colors.Gray,
                        StrokeThickness = 1,
                        BackgroundColor = Colors.White
                    };

                    // Add tap gesture to toggle cell state
                    var tapGesture = new TapGestureRecognizer();
                    int capturedRow = row;
                    int capturedCol = col;
                    tapGesture.Tapped += (s, e) =>
                    {
                        cells[capturedRow, capturedCol].IsAlive = !cells[capturedRow, capturedCol].IsAlive;
                        UpdateCellVisual(cells[capturedRow, capturedCol]);
                    };
                    border.GestureRecognizers.Add(tapGesture);

                    GameGrid.Add(border, col, row);

                    cells[row, col] = new Cell
                    {
                        Row = row,
                        Col = col,
                        IsAlive = false,
                        Border = border
                    };
                }
            }
        }

        void InitializeGame()
        {
            // Stop the simulation if it's running
            StopSimulation();

            // Clear all cells
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    cells[row, col].IsAlive = false;
                    UpdateCellVisual(cells[row, col]);
                }
            }

            // Place a glider in the middle
            int midRow = GridSize / 2;
            int midCol = GridSize / 2;

            SetCellAlive(midRow, midCol);
            SetCellAlive(midRow, midCol + 1);
            SetCellAlive(midRow, midCol + 2);
            SetCellAlive(midRow + 1, midCol + 2);
            SetCellAlive(midRow + 2, midCol + 1);

            // Start the simulation automatically
            StartSimulation();
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
            cell.Border.BackgroundColor = cell.IsAlive ? Colors.Black : Colors.White;
        }

        async void StartSimulation()
        {
            if (isRunning)
                return;

            isRunning = true;
            StartStopButton.Text = "Stop";
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    NextGeneration();
                    await Task.Delay(200, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
            }
            finally
            {
                isRunning = false;
                StartStopButton.Text = "Start";
            }
        }

        void StopSimulation()
        {
            if (!isRunning)
                return;

            cancellationTokenSource.Cancel();
            isRunning = false;
            StartStopButton.Text = "Start";
        }

        void OnStartStopButtonClicked(object sender, EventArgs e)
        {
            if (isRunning)
            {
                StopSimulation();
            }
            else
            {
                StartSimulation();
            }
        }

        void OnResetButtonClicked(object sender, EventArgs e)
        {
            InitializeGame();
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
                        true when aliveNeighbors is 2 or 3 => true,
                        true when aliveNeighbors > 3 => false,
                        false when aliveNeighbors == 3 => true,
                        _ => false
                    };
                }
            }

            // Update cells on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                for (int row = 0; row < GridSize; row++)
                {
                    for (int col = 0; col < GridSize; col++)
                    {
                        cells[row, col].IsAlive = newStates[row, col];
                        UpdateCellVisual(cells[row, col]);
                    }
                }
            });
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
        public Border Border { get; set; }
    }
}
