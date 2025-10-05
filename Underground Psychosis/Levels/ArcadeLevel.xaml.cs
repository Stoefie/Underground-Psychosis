using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Underground_Psychosis.GameEngine;

namespace Underground_Psychosis.Levels
{
    public partial class ArcadeLevel : UserControl
    {
        private GameLoop _game;
        private Player _player;
        private LevelEditor _editor;
        private readonly Dictionary<(int x, int y), Tile> _tileEntities = new();
        private const string ArcadeLevelFile = "Levels/ArcadeLevel/level_edit.txt";

        private List<int[]> levelMap = new();
        private const int BorderTileCode = 9;
        private const int InitialWidth = 100;
        private const int LevelHeight = 36;
        private const int ColumnBatch = 10;

        private double _tileWidth = 32; 
        private double _tileHeight = 32;

        private double _cameraX, _cameraY;
        private const double CameraSmoothness = 0.15;

        public ArcadeLevel()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Focusable = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();

            _game = new GameLoop(GameCanvas);
            _player = new Player { Position = new Point(100, 400) };
            _game.AddEntity(_player);

            InitializeLevel(InitialWidth);
            BuildCanvasSize();

            _editor = new LevelEditor(
                _game,
                GameCanvas,
                levelMap,
                _tileEntities,
                () => _tileWidth,
                () => _tileHeight,
                "ArcadeLevel");

            _editor.Load();

            BuildCanvasSize();

            GameCanvas.MouseMove += (s, me) => _editor.OnMouseMove(me.GetPosition(GameCanvas));
            GameCanvas.MouseDown += GameCanvas_MouseDown;
            GameCanvas.MouseUp += (s, me) => _editor.OnMouseUp(me.ChangedButton);
            KeyDown += ArcadeLevel_KeyDown;

            RenderOptions.SetBitmapScalingMode(GameCanvas, BitmapScalingMode.NearestNeighbor);

            _cameraX = 0;
            _cameraY = 0;

            _game.Tick += () =>
            {
                GenerateColumnsIfNeeded();
                CenterViewOnPlayer();
            };
            _game.Start();
        }
        private bool TryAutoLoadDynamic()
        {
            try
            {
                var absolute = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ArcadeLevelFile);
                if (!System.IO.File.Exists(absolute))
                    return false;

                var loaded = LevelFileLoader.LoadJagged(absolute);
                levelMap.Clear();
                levelMap.AddRange(loaded);
                return true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ArcadeLevel autoload failed: {ex.Message}");
                return false;
            }
        }

        // Ctrl + Left Mouse Button: teleport player to mouse (tile-aligned if Shift held)
        private void GameCanvas_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(GameCanvas);
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (ctrl && e.ChangedButton == MouseButton.Left)
            {
                if (_player == null) return;

                // Optional snap to tile grid when Shift is held
                bool snap = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                double px = pos.X - _player.Width / 2;
                double py = pos.Y - _player.Height / 2;

                if (snap && _tileWidth > 0 && _tileHeight > 0)
                {
                    px = System.Math.Round((pos.X / _tileWidth)) * _tileWidth - _player.Width / 2;
                    py = System.Math.Round((pos.Y / _tileHeight)) * _tileHeight - _player.Height / 2;
                }

                _player.Position = new Point(px, py);
                _player.BoundingRect = new Rect(_player.Position.X, _player.Position.Y, _player.Width, _player.Height);

                e.Handled = true; // prevent editor tile placement
                return;
            }

            // Normal editor behavior
            _editor.OnMouseDown(pos, e.ChangedButton);
        }

        private void ArcadeLevel_KeyDown(object sender, KeyEventArgs e)
        {
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

        private void InitializeLevel(int initialWidth)
        {
            levelMap.Clear();
            for (int y = 0; y < LevelHeight; y++)
            {
                levelMap.Add(new int[initialWidth]);
            }
        }

        private void BuildCanvasSize()
        {
            if (levelMap.Count == 0) return;
            GameCanvas.Width = levelMap[0].Length * _tileWidth;
            GameCanvas.Height = levelMap.Count * _tileHeight;
        }
        private void GenerateColumnsIfNeeded()
        {
            if (levelMap.Count == 0 || _tileWidth <= 0) return;

            int playerCol = (int)(_player.Position.X / _tileWidth);
            int currentWidth = levelMap[0].Length;

            if (playerCol > currentWidth - 5 && !IsBorderPlaced())
            {
                for (int r = 0; r < levelMap.Count; r++)
                {
                    var row = levelMap[r];
                    int oldLen = row.Length;
                    System.Array.Resize(ref row, oldLen + ColumnBatch);
                    for (int c = oldLen; c < row.Length; c++)
                        row[c] = 0;
                    levelMap[r] = row; // write back resized row
                }
                BuildCanvasSize();
            }
        }

        private bool IsBorderPlaced()
        {
            int lastCol = levelMap[0].Length - 1;
            foreach (var row in levelMap)
            {
                if (row[lastCol] == BorderTileCode)
                    return true;
            }
            return false;
        }

        public void CenterViewOnPlayer()
        {
            double viewportWidth = ArcadeLevelScrollViewer.ViewportWidth;
            double viewportHeight = ArcadeLevelScrollViewer.ViewportHeight;
            if (viewportWidth <= 0 || viewportHeight <= 0) return;

            double targetX = _player.Position.X + _player.Width / 2 - viewportWidth / 2;
            double targetY = _player.Position.Y + _player.Height / 2 - viewportHeight / 2;

            targetX = System.Math.Max(0, System.Math.Min(targetX, GameCanvas.Width - viewportWidth));
            targetY = System.Math.Max(0, System.Math.Min(targetY, GameCanvas.Height - viewportHeight));

            _cameraX += (targetX - _cameraX) * CameraSmoothness;
            _cameraY += (targetY - _cameraY) * CameraSmoothness;

            ArcadeLevelScrollViewer.ScrollToHorizontalOffset(_cameraX);
            ArcadeLevelScrollViewer.ScrollToVerticalOffset(_cameraY);
        }

        private void LoadLevel(List<int[]> map)
        {
            int rows = map.Count;
            int cols = map[0].Length;

            for (int y = 0; y < rows; y++)
            {
                var row = map[y];
                for (int x = 0; x < cols; x++)
                {
                    int code = row[x];
                    if (code == 0) continue;
                    if (_tileEntities.ContainsKey((x, y))) continue;

                    var tile = new Tile(x, y, _tileWidth, _tileHeight, isSolid: true, kind: code);
                    _tileEntities[(x, y)] = tile;
                    _game.AddEntity(tile);
                }
            }
        }
    }
}