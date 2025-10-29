using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Underground_Psychosis.GameEngine;
using System.Collections.Generic;
using System.Linq;

namespace Underground_Psychosis.Levels
{
    public partial class FirstLevel : UserControl
    {
        private GameLoop _game;
        private Player _player;
        private PlayerTwo _playerTwo;
        private LevelEditor _editor;
        private readonly Dictionary<(int x, int y), Tile> _tileEntities = new();

        // UI & goal tracking
        private TextBlock _finishStatus;
        private Border _levelCompleteOverlay;
        private bool _p1OnGoal;
        private bool _p2OnGoal;

        private TextBlock _debugPositions;

        private int[,] levelMap =
        {
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,4,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2 },
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,4,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2 },
            { 2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,4,4,4,4,4,4 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,0,0,0,0,0,0,2,2,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,0,0,0,0,0,0,0,0,2,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,5,5,5,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,5,5,5,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,5,5,5,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6,6,6,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,4,4,4,4,4,4 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,0,2 },
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,2 },
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
        };

        private double _tileWidth;
        private double _tileHeight;
        private bool _tilesLoaded;

        private InventoryControl? _p1Inventory;
        private InventoryControl? _p2Inventory;

        /*
        public FirstLevel()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                _game = new GameLoop(GameCanvas);
                _player = new Player { Position = new System.Windows.Point(100, 850) };
                _game.AddEntity(_player);
                LoadLevel(levelMap);
                _game.Start();
            };
        }
        */

        public FirstLevel()
        {
            InitializeComponent();
             if (HUD == null)
        MessageBox.Show("HUD is null — not created!");
    else
        MessageBox.Show("HUD created successfully!");
            Loaded += OnLoaded;
            Focusable = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus(); // Ensure keyboard input

            _game = new GameLoop(GameCanvas);
            _player = new Player { Position = new Point(100, 400) };
            _playerTwo = new PlayerTwo { Position = new Point(300, 700) };
            _game.AddEntity(_player);
            _game.AddEntity(_playerTwo);

            TryLoadTiles();

            GameCanvas.SizeChanged += (s, ev) =>
            {
                if (!_tilesLoaded) TryLoadTiles();

                // Adjust overlays / UI placed on the canvas when size changes
                UpdateOverlaySize();
                UpdateFinishStatusPosition();
            };

            _editor = new LevelEditor(
                _game,
                GameCanvas,
                levelMap,
                _tileEntities,
                () => _tileWidth,
                () => _tileHeight,
                "FirstLevel");

            GameCanvas.MouseMove += (s, me) => _editor.OnMouseMove(me.GetPosition(GameCanvas));
            GameCanvas.MouseDown += (s, me) => _editor.OnMouseDown(me.GetPosition(GameCanvas), me.ChangedButton);
            GameCanvas.MouseUp += (s, me) => _editor.OnMouseUp(me.ChangedButton);
            KeyDown += FirstLevel_KeyDown;

            RenderOptions.SetBitmapScalingMode(GameCanvas, BitmapScalingMode.NearestNeighbor);

            // create finish UI and subscribe to tick
            _finishStatus = CreateFinishStatus();
            GameCanvas.Children.Add(_finishStatus);
            Canvas.SetZIndex(_finishStatus, 1000);
            _finishStatus.Visibility = Visibility.Collapsed;

            _levelCompleteOverlay = CreateLevelCompleteOverlay();
            GameCanvas.Children.Add(_levelCompleteOverlay);
            Canvas.SetZIndex(_levelCompleteOverlay, 2000);
            _levelCompleteOverlay.Visibility = Visibility.Collapsed;

            _debugPositions = new TextBlock
            {
                Foreground = Brushes.Yellow,
                Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                Padding = new Thickness(6),
                FontSize = 12
            };
            GameCanvas.Children.Add(_debugPositions);
            Canvas.SetZIndex(_debugPositions, 6000);
            Canvas.SetLeft(_debugPositions, 10);
            Canvas.SetTop(_debugPositions, 10);

            _game.Tick += GameTick;

            _game.Start();

            // --- add after starting the game ---
            // create inventories, attach to players and register toggle keys
            _p1Inventory = new InventoryControl { Visibility = Visibility.Collapsed };
            _p2Inventory = new InventoryControl { Visibility = Visibility.Collapsed };

            GameCanvas.Children.Add(_p1Inventory);
            GameCanvas.Children.Add(_p2Inventory);

            // ensure inventories draw above game entities / overlays
            Canvas.SetZIndex(_p1Inventory, 5000);
            Canvas.SetZIndex(_p2Inventory, 5000);

            // force initial measure/arrange so ActualWidth/ActualHeight are valid for Follow's initial placement
            _p1Inventory.UpdateLayout();
            _p2Inventory.UpdateLayout();

            // make inventories follow players (raise verticalOffset so inventory appears higher above player)
            _p1Inventory.Follow(_player, GameCanvas, verticalOffset: 80.0, smoothFactor: 8.0);
            _p2Inventory.Follow(_playerTwo, GameCanvas, verticalOffset: 40.0, smoothFactor: 8.0);

            // register toggle keys on the level window (I for player1, O for player2)
            _p1Inventory.RegisterToggleKey(Window.GetWindow(this) ?? Application.Current.MainWindow, Key.I);
            _p2Inventory.RegisterToggleKey(Window.GetWindow(this) ?? Application.Current.MainWindow, Key.O);

            // optionally seed items for testing (example)
            try
            {
                var potionImg = new BitmapImage();
                potionImg.BeginInit();
                potionImg.UriSource = new System.Uri("pack://application:,,,/images/HealthPotion.png", System.UriKind.Absolute);
                potionImg.EndInit();
                _p1Inventory.AddItem("Health Potion", potionImg);
                _p2Inventory.AddItem("Health Potion", potionImg);
            }
            catch { }

            // --- end additions ---
        }

        private void FirstLevel_KeyDown(object sender, KeyEventArgs e)
        {
            // toggle player 1 inventory with I
            if (e.Key == Key.I && _p1Inventory != null)
            {
                _p1Inventory.Visibility = _p1Inventory.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                e.Handled = true;
                return;
            }

            // toggle player 2 inventory with O
            if (e.Key == Key.O && _p2Inventory != null)
            {
                _p2Inventory.Visibility = _p2Inventory.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                e.Handled = true;
                return;
            }

            
            if (e.Key == Key.F2)
            {
                _editor.Enabled = !_editor.Enabled;
            }
            else
            {
                bool ctrl = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
                _editor.OnKeyDown(e.Key, ctrl);
            }
        }

        private void TryLoadTiles()
        {
            if (GameCanvas.ActualWidth <= 0 || GameCanvas.ActualHeight <= 0)
                return;

            int rows = levelMap.GetLength(0);
            int cols = levelMap.GetLength(1);

            _tileWidth = GameCanvas.ActualWidth / cols;
            _tileHeight = GameCanvas.ActualHeight / rows;

            if (_tileWidth <= 0 || _tileHeight <= 0)
                return;

            LoadLevel(levelMap);
            _tilesLoaded = true;

            
            UpdateOverlaySize();
            UpdateFinishStatusPosition();
        }

        private void LoadLevel(int[,] map)
        {
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);
            bool[,] visited = new bool[rows, cols];

            // First pass: spawn one Crate per connected group of 5s
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (map[y, x] != 5 || visited[y, x]) continue;

                    var group = new List<(int x, int y)>();
                    var q = new Queue<(int x, int y)>();
                    q.Enqueue((x, y));
                    visited[y, x] = true;

                    while (q.Count > 0)
                    {
                        var (cx, cy) = q.Dequeue();
                        group.Add((cx, cy));

                        // 4-dir neighbors
                        var neighbors = new (int nx, int ny)[] { (cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1) };
                        foreach (var (nx, ny) in neighbors)
                        {
                            if (nx >= 0 && nx < cols && ny >= 0 && ny < rows && !visited[ny, nx] && map[ny, nx] == 5)
                            {
                                visited[ny, nx] = true;
                                q.Enqueue((nx, ny));
                            }
                        }
                    }

                    int minX = group.Min(p => p.x);
                    int maxX = group.Max(p => p.x);
                    int minY = group.Min(p => p.y);
                    int maxY = group.Max(p => p.y);

                    var cratePos = new Point(minX * _tileWidth, minY * _tileHeight);
                    var crateWidth = (maxX - minX + 1) * _tileWidth;
                    var crateHeight = (maxY - minY + 1) * _tileHeight;

                    var crate = new Underground_Psychosis.GameEngine.Crate(
                        startPosition: cratePos,
                        width: crateWidth,
                        height: crateHeight,
                        tiles: _tileEntities,
                        tileWidthProvider: () => _tileWidth,
                        tileHeightProvider: () => _tileHeight,
                        p1: _player,
                        p2: _playerTwo
                    );
                    _game.AddEntity(crate);
                }
            }

            // Second pass: load all non-crate tiles (skip code 6 for buttons - create them after tiles exist)
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int code = map[y, x];
                    if (code == 0 || code == 5 || code == 6) continue; // crates already handled, buttons deferred
                    if (_tileEntities.ContainsKey((x, y))) continue;

                    var tile = new Tile(x, y, _tileWidth, _tileHeight, isSolid: true, kind: code);
                    _tileEntities[(x, y)] = tile;
                    _game.AddEntity(tile);
                }
            }

            // Third pass: create Red button tiles (code == 6)
            // Each button will remove wooden-bar tiles (code 4) one tile per press (closest removed first).
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (map[y, x] != 6) continue;

                    // build ordered list of wooden bar coords (code 4) that currently exist
                    var targets = new List<(int x, int y)>();
                    for (int ty = 0; ty < rows; ty++)
                    {
                        for (int tx = 0; tx < cols; tx++)
                        {
                            if (map[ty, tx] == 4 && _tileEntities.ContainsKey((tx, ty)))
                                targets.Add((tx, ty));
                        }
                    }

                    // sort by Manhattan distance from button cell (closest removed first)
                    targets = targets
                        .OrderBy(t => Math.Abs(t.x - x) + Math.Abs(t.y - y))
                        .ToList();

                    var button = new RedButton(
                        cellX: x,
                        cellY: y,
                        width: _tileWidth,
                        height: _tileHeight,
                        game: _game,
                        tiles: _tileEntities,
                        p1: _player,
                        p2: _playerTwo,
                        targets: targets
                    );

                    _tileEntities[(x, y)] = button;
                    _game.AddEntity(button);
                }
            }
        }

        // RedButton: a Tile (visual + solid) that when stood upon removes one wooden-bar (kind 4) tile per press.
        // It removes the closest remaining wooden-bar tile from its internal ordered list each time a player steps on it.
        private class RedButton : Tile
        {
            private readonly GameLoop _game;
            private readonly Dictionary<(int x, int y), Tile> _tiles;
            private readonly Player _p1;
            private readonly PlayerTwo _p2;
            private readonly Queue<(int x, int y)> _targets;
            private readonly Brush _idleFill = Brushes.DarkRed;
            private readonly Brush _pressedFill = Brushes.OrangeRed;

            // interval in seconds between removals while someone stands on the button
            private readonly double _removeInterval = 0.5;
            private double _timeSinceLastRemove;

            public RedButton(int cellX, int cellY, double width, double height,
                             GameLoop game,
                             Dictionary<(int x, int y), Tile> tiles,
                             Player p1,
                             PlayerTwo p2,
                             List<(int x, int y)> targets)
                : base(cellX, cellY, width, height, isSolid: true, kind: 6, fill: Brushes.Red)
            {
                _game = game;
                _tiles = tiles;
                _p1 = p1;
                _p2 = p2;
                // queue copy so we can discard removed / missing tiles as we go
                _targets = new Queue<(int x, int y)>(targets);
                _timeSinceLastRemove = 0;
            }

            private Rect Bounds => new Rect(Position.X, Position.Y, Width, Height);

            public override void Update(double deltaTime)
            {
                bool pressedNow = PlayerIsStandingOn(_p1) || PlayerIsStandingOn(_p2);

                if (pressedNow)
                {
                    // accumulate time while a player stands on the button
                    _timeSinceLastRemove += deltaTime;

                    // remove one bar each interval
                    if (_timeSinceLastRemove >= _removeInterval)
                    {
                        RemoveNextBar();
                        _timeSinceLastRemove = 0;
                    }
                }
                else
                {
                    // reset timer when nobody is standing on the button
                    _timeSinceLastRemove = 0;
                }
            }

            private bool PlayerIsStandingOn(Entity playerEntity)
            {
                if (playerEntity is null) return false;

                // Player classes expose Position, Width, Height
                Point pPos;
                double pW, pH;
                if (playerEntity is Player p)
                {
                    pPos = p.Position;
                    pW = p.Width;
                    pH = p.Height;
                }
                else if (playerEntity is PlayerTwo p2)
                {
                    pPos = p2.Position;
                    pW = p2.Width;
                    pH = p2.Height;
                }
                else return false;

                var pRect = new Rect(pPos.X, pPos.Y, pW, pH);
                var bRect = Bounds;

                if (!pRect.IntersectsWith(bRect)) return false;

                // require that player is above or mostly overlapping vertically to count as "standing"
                double overlapY = Math.Min(pRect.Bottom, bRect.Bottom) - Math.Max(pRect.Top, bRect.Top);
                double overlapX = Math.Min(pRect.Right, bRect.Right) - Math.Max(pRect.Left, bRect.Left);

                if (overlapY <= 0 || overlapX <= 0) return false;

                // check that player's feet are at or below the middle of the button (simple heuristic)
                double playerFeet = pRect.Bottom;
                return playerFeet >= bRect.Top && playerFeet <= bRect.Top + (bRect.Height * 0.75);
            }

            private void RemoveNextBar()
            {
                // pop until we find an existing tile that is still present (and kind 4)
                while (_targets.Count > 0)
                {
                    var coord = _targets.Dequeue();
                    if (_tiles.TryGetValue(coord, out var t))
                    {
                        // Remove from game and dictionary.
                        _game.RemoveEntity(t);
                        _tiles.Remove(coord);
                        return;
                    }
                    
                }

                
            }
        }

        // -- Goal detection & UI helpers --

        private void GameTick()
        {
            UpdateGoalStatus();

            if (_debugPositions != null)
            {
                var p1Pos = _player?.Position ?? new Point(double.NaN, double.NaN);
                var p2Pos = _playerTwo?.Position ?? new Point(double.NaN, double.NaN);
                _debugPositions.Text =
                    $"P1: X={p1Pos.X:0.0}, Y={p1Pos.Y:0.0}\nP2: X={p2Pos.X:0.0}, Y={p2Pos.Y:0.0}";
            }
        }

        private void UpdateGoalStatus()
        {
            // Find green tiles (kind == 1)
            var greenTiles = _tileEntities.Values.Where(t => t.Kind == 1).ToList();

            bool p1Now = _player != null && greenTiles.Any(t => _player.BoundingRect.IntersectsWith(t.BoundingRect));
            bool p2Now = _playerTwo != null && greenTiles.Any(t => _playerTwo.BoundingRect.IntersectsWith(t.BoundingRect));

            // if state changed, update UI
            if (p1Now != _p1OnGoal || p2Now != _p2OnGoal)
            {
                _p1OnGoal = p1Now;
                _p2OnGoal = p2Now;
                RefreshGoalUI();
            }
        }

        private void RefreshGoalUI()
        {
            if (_p1OnGoal && _p2OnGoal)
            {
                // both finished -> show overlay
                _finishStatus.Visibility = Visibility.Collapsed;
                _levelCompleteOverlay.Visibility = Visibility.Visible;
            }
            else if (_p1OnGoal || _p2OnGoal)
            {
                // one finished -> show "1/2 players finished"
                _levelCompleteOverlay.Visibility = Visibility.Collapsed;
                _finishStatus.Text = "1/2 players finished";
                _finishStatus.Visibility = Visibility.Visible;
            }
            else
            {
                // none finished -> hide UI
                _finishStatus.Visibility = Visibility.Collapsed;
                _levelCompleteOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private TextBlock CreateFinishStatus()
        {
            var tb = new TextBlock
            {
                Text = "",
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0)),
                Padding = new Thickness(8),
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };

            // position near top-center
            Canvas.SetLeft(tb, (GameCanvas.ActualWidth / 2) - 80);
            Canvas.SetTop(tb, 10);

            return tb;
        }

        private Border CreateLevelCompleteOverlay()
        {
            var overlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(190, 0, 0, 0)),
                Width = GameCanvas.ActualWidth,
                Height = GameCanvas.ActualHeight,
                Child = new Grid
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Level Complete",
                            Foreground = Brushes.White,
                            FontSize = 48,
                            FontWeight = FontWeights.ExtraBold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        }
                    }
                }
            };
            Canvas.SetLeft(overlay, 0);
            Canvas.SetTop(overlay, 0);

            return overlay;
        }

        private void UpdateOverlaySize()
        {
            if (_levelCompleteOverlay == null) return;
            _levelCompleteOverlay.Width = GameCanvas.ActualWidth;
            _levelCompleteOverlay.Height = GameCanvas.ActualHeight;
        }

        private void UpdateFinishStatusPosition()
        {
            if (_finishStatus == null) return;
            double left = (GameCanvas.ActualWidth / 2) - (_finishStatus.ActualWidth / 2);
            if (double.IsNaN(left) || double.IsInfinity(left))
                left = (GameCanvas.ActualWidth / 2) - 80;
            Canvas.SetLeft(_finishStatus, Math.Max(10, left));
            Canvas.SetTop(_finishStatus, 10);
        }
    }
}