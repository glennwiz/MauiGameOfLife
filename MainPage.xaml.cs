using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Threading;
using MauiGameOfLife.Interfaces;

namespace MauiGameOfLife
{
    public partial class MainPage : ContentPage
    {
        const int GridSize = 30;
        const int CellSize = 20;
        private readonly IScreenshotService _screenShotService;
        Cell[,] cells = new Cell[GridSize, GridSize];

        bool isRunning = false;
        CancellationTokenSource cancellationTokenSource;

        public MainPage()
        {
#if ANDROID
            _screenShotService = new MauiGameOfLife.Platforms.Android.Services.ScreenshotService();
#endif
            InitializeComponent();
            CreateGrid();
            InitializeGame();
        }

        void CreateGrid()
        {
            for (int i = 0; i < GridSize; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });
            }

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
            StopSimulation();
            ClearGrid();
            SpawnPattern("Glider", GridSize / 2, GridSize / 2);           
            StartSimulation();
        }

        void ClearGrid()
        {
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    cells[row, col].IsAlive = false;
                    UpdateCellVisual(cells[row, col]);
                }
            }
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

        void OnSpawnPatternClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            string patternName = button.Text;
            StopSimulation();

            ClearGrid();
            SpawnPattern(patternName, GridSize / 2, GridSize / 2);
            StartSimulation();
        }

        void SpawnPattern(string patternName, int startRow, int startCol)
        {
            switch (patternName)
            {
                case "Glider":
                    SetCellAlive(startRow, startCol);
                    SetCellAlive(startRow, startCol + 1);
                    SetCellAlive(startRow, startCol + 2);
                    SetCellAlive(startRow + 1, startCol + 2);
                    SetCellAlive(startRow + 2, startCol + 1);
                    break;

                case "Blinker":
                    SetCellAlive(startRow, startCol - 1);
                    SetCellAlive(startRow, startCol);
                    SetCellAlive(startRow, startCol + 1);
                    break;

                case "Toad":
                    SetCellAlive(startRow, startCol + 1);
                    SetCellAlive(startRow, startCol + 2);
                    SetCellAlive(startRow, startCol + 3);
                    SetCellAlive(startRow + 1, startCol);
                    SetCellAlive(startRow + 1, startCol + 1);
                    SetCellAlive(startRow + 1, startCol + 2);
                    break;

                case "Beacon":
                    SetCellAlive(startRow, startCol);
                    SetCellAlive(startRow, startCol + 1);
                    SetCellAlive(startRow + 1, startCol);
                    SetCellAlive(startRow + 1, startCol + 1);
                    SetCellAlive(startRow + 2, startCol + 2);
                    SetCellAlive(startRow + 2, startCol + 3);
                    SetCellAlive(startRow + 3, startCol + 2);
                    SetCellAlive(startRow + 3, startCol + 3);
                    break;

                case "Pulsar":
                    int r = startRow - 6;
                    int c = startCol - 6;
                    int[][] offsets = new int[][]
                    {
                        new int[]{2,0},new int[]{3,0},new int[]{4,0},new int[]{8,0},new int[]{9,0},new int[]{10,0},
                        new int[]{0,2},new int[]{5,2},new int[]{7,2},new int[]{12,2},
                        new int[]{0,3},new int[]{5,3},new int[]{7,3},new int[]{12,3},
                        new int[]{0,4},new int[]{5,4},new int[]{7,4},new int[]{12,4},
                        new int[]{2,5},new int[]{3,5},new int[]{4,5},new int[]{8,5},new int[]{9,5},new int[]{10,5},
                        new int[]{2,7},new int[]{3,7},new int[]{4,7},new int[]{8,7},new int[]{9,7},new int[]{10,7},
                        new int[]{0,8},new int[]{5,8},new int[]{7,8},new int[]{12,8},
                        new int[]{0,9},new int[]{5,9},new int[]{7,9},new int[]{12,9},
                        new int[]{0,10},new int[]{5,10},new int[]{7,10},new int[]{12,10},
                        new int[]{2,12},new int[]{3,12},new int[]{4,12},new int[]{8,12},new int[]{9,12},new int[]{10,12}
                    };
                    foreach (var offset in offsets)
                    {
                        SetCellAlive(r + offset[1], c + offset[0]);
                    }
                    break;

                default:
                    break;
            }
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

        async void OnScreenshotButtonClicked(object sender, EventArgs e)
        {           
            //var screenshotService = DependencyService.Get<IScreenshotService>();
            //if (screenshotService == null)
            //{
            //    await DisplayAlert("Error", "Screenshot service not available.", "OK");
            //    return;
            //}
            System.Diagnostics.Debug.WriteLine("Screenshot button clicked");
            // Capture the screenshot
            string filePath = await _screenShotService.CaptureScreenshotAsync();

            if (!string.IsNullOrEmpty(filePath))
            {
                await DisplayAlert("Screenshot Captured", $"Saved to {filePath}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to capture screenshot.", "OK");
            }
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
