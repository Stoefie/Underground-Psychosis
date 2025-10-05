using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Underground_Psychosis.Levels;

namespace Underground_Psychosis.GameEngine
{
    public class LevelEditor
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<(int x, int y), Tile> _tiles;
        private readonly GameLoop _game;
        private readonly Func<double> _tileWidth;
        private readonly Func<double> _tileHeight;

        // Two possible backing map types (only one is used)
        private readonly int[,]? _rectMap;
        private readonly List<int[]>? _jaggedMap;
        private readonly bool _dynamic;

        private readonly Rectangle _cursor;
        private readonly TextBlock _status;
        private bool _enabled;
        private int _currentKind = 1;

        private readonly string _levelName;
        private readonly string _levelDir;
        private readonly string _levelFile;

        private bool _isPainting;
        private PaintMode _paintMode = PaintMode.None;
        private (int x, int y) _lastPaintCell = (-9999, -9999);

        private enum PaintMode { None, Place, Erase }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                _cursor.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                RefreshStatus();
            }
        }

        // Constructor for static map (int[,])
        public LevelEditor(
            GameLoop game,
            Canvas canvas,
            int[,] map,
            Dictionary<(int x, int y), Tile> tiles,
            Func<double> tileWidthProvider,
            Func<double> tileHeightProvider,
            string levelName)
        {
            _game = game;
            _canvas = canvas;
            _rectMap = map;
            _tiles = tiles;
            _tileWidth = tileWidthProvider;
            _tileHeight = tileHeightProvider;
            _dynamic = false;

            _levelName = levelName;
            _levelDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels", _levelName);
            Directory.CreateDirectory(_levelDir);
            _levelFile = System.IO.Path.Combine(_levelDir, "level_edit.txt");


            _cursor = CreateCursor();
            _canvas.Children.Add(_cursor);
            _status = CreateStatusBlock();
            _canvas.Children.Add(_status);
        }

        // Constructor for dynamic map (List<int[]>)
        public LevelEditor(
            GameLoop game,
            Canvas canvas,
            List<int[]> map,
            Dictionary<(int x, int y), Tile> tiles,
            Func<double> tileWidthProvider,
            Func<double> tileHeightProvider,
            string levelName)
        {
            _game = game;
            _canvas = canvas;
            _jaggedMap = map;
            _tiles = tiles;
            _tileWidth = tileWidthProvider;
            _tileHeight = tileHeightProvider;
            _dynamic = true;

            _levelName = levelName;
            _levelDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels", _levelName);
            Directory.CreateDirectory(_levelDir);
            _levelFile = System.IO.Path.Combine(_levelDir, "level_edit.txt");

            _cursor = CreateCursor();
            _canvas.Children.Add(_cursor);
            _status = CreateStatusBlock();
            _canvas.Children.Add(_status);
        }

        private Rectangle CreateCursor() => new Rectangle
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(40, 255, 255, 0)),
            IsHitTestVisible = false,
            Visibility = Visibility.Hidden
        };

        private TextBlock CreateStatusBlock()
        {
            var tb = new TextBlock
            {
                Foreground = Brushes.Yellow,
                Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
                FontSize = 14,
                Text = "",
                IsHitTestVisible = false
            };
            Canvas.SetLeft(tb, 4);
            Canvas.SetTop(tb, 4);
            return tb;
        }

        public void OnMouseMove(Point p)
        {
            if (_tileWidth() <= 0 || _tileHeight() <= 0) return;
            var (cx, cy) = WorldToCell(p);
            if (!InBounds(cx, cy)) return;

            if (Enabled)
            {
                _cursor.Width = _tileWidth();
                _cursor.Height = _tileHeight();
                Canvas.SetLeft(_cursor, cx * _tileWidth());
                Canvas.SetTop(_cursor, cy * _tileHeight());
                _cursor.Stroke = _currentKind == 9 ? Brushes.Orange : Brushes.Yellow;
            }

            if (Enabled && _isPainting && _lastPaintCell != (cx, cy))
            {
                if (_paintMode == PaintMode.Place)
                    PlaceTile(cx, cy, _currentKind, false);
                else if (_paintMode == PaintMode.Erase)
                    RemoveTile(cx, cy);

                _lastPaintCell = (cx, cy);
            }
        }

        public void OnMouseDown(Point p, MouseButton button)
        {
            if (!Enabled) return;
            var (cx, cy) = WorldToCell(p);
            if (!InBounds(cx, cy)) return;

            _lastPaintCell = (-9999, -9999);

            if (button == MouseButton.Left)
            {
                _paintMode = PaintMode.Place;
                _isPainting = true;
                PlaceTile(cx, cy, _currentKind, false);
            }
            else if (button == MouseButton.Right)
            {
                _paintMode = PaintMode.Erase;
                _isPainting = true;
                RemoveTile(cx, cy);
            }
        }

        public void OnMouseUp(MouseButton button)
        {
            if (!_isPainting) return;
            if ((button == MouseButton.Left && _paintMode == PaintMode.Place) ||
                (button == MouseButton.Right && _paintMode == PaintMode.Erase))
            {
                _isPainting = false;
                _paintMode = PaintMode.None;
                _lastPaintCell = (-9999, -9999);
            }
        }

        public void OnKeyDown(Key key, bool ctrl)
        {
            if (!Enabled && key != Key.F2) return;

            if (key == Key.D1) { _currentKind = 1; RefreshStatus(); }
            else if (key == Key.D2) { _currentKind = 2; RefreshStatus(); }
            else if (key == Key.D3) { _currentKind = 3; RefreshStatus(); }
            else if (key == Key.D4) { _currentKind = 4; RefreshStatus(); }
            else if (key == Key.D9) { _currentKind = 9; RefreshStatus(); } // border marker

            if (key == Key.Escape)
            {
                Window parentWindow = Window.GetWindow(_canvas);
                if (parentWindow is MainWindow mainWindow)
                {
                mainWindow.MainContent.Content = new MainMenu();
                }
            }
            if (ctrl && key == Key.S) Save();
            else if (ctrl && key == Key.L) Load();
        }

        private void RefreshStatus()
        {
            if (_enabled)
                _status.Text = $"EDITOR ON (1-4/9 kinds, LMB place, RMB remove, Ctrl+S save, Ctrl+L load)  Kind={_currentKind}";
        }

        private (int x, int y) WorldToCell(Point p)
        {
            double w = _tileWidth();
            double h = _tileHeight();
            if (w <= 0 || h <= 0) return (-1, -1);
            return ((int)(p.X / w), (int)(p.Y / h));
        }

        private int RowCount => _dynamic ? (_jaggedMap?.Count ?? 0) : (_rectMap?.GetLength(0) ?? 0);
        private int ColCount => _dynamic ? (_jaggedMap is { Count: > 0 } ? _jaggedMap[0].Length : 0) : (_rectMap?.GetLength(1) ?? 0);

        private bool InBounds(int x, int y) => y >= 0 && y < RowCount && x >= 0 && x < ColCount;

        private int GetValue(int x, int y)
            => _dynamic ? _jaggedMap![y][x] : _rectMap![y, x];

        private void SetValue(int x, int y, int v)
        {
            if (_dynamic) _jaggedMap![y][x] = v;
            else _rectMap![y, x] = v;
        }

        private void PlaceTile(int x, int y, int kind, bool allowSameReplace)
        {
            if (!InBounds(x, y)) return;
            if (!allowSameReplace && GetValue(x, y) == kind) return;

            RemoveTile(x, y);
            SetValue(x, y, kind);

            if (kind != 0)
            {
                var tile = new Tile(x, y, _tileWidth(), _tileHeight(), isSolid: true, kind: kind);
                _tiles[(x, y)] = tile;
                _game.AddEntity(tile);
            }
        }

        private void RemoveTile(int x, int y)
        {
            if (!InBounds(x, y)) return;

            if (_tiles.Remove((x, y), out var tile))
                _game.RemoveEntity(tile);

            SetValue(x, y, 0);
        }

        public void Save()
        {
            using var sw = new StreamWriter(_levelFile);
            int rows = RowCount;
            int cols = ColCount;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    sw.Write(GetValue(c, r));
                    if (c < cols - 1) sw.Write(',');
                }
                sw.WriteLine();
            }
        }

        public void Load()
        {
            if (!File.Exists(_levelFile)) return;

            foreach (var t in _tiles.Values)
                _game.RemoveEntity(t);
            _tiles.Clear();

            var lines = File.ReadAllLines(_levelFile);
            if (lines.Length == 0) return;

            int fileRowCount = lines.Length;
            int fileMaxCols = 0;
            var parsed = new List<int[]>(fileRowCount);

            foreach (var line in lines)
            {
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                fileMaxCols = Math.Max(fileMaxCols, parts.Length);
                var rowVals = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    rowVals[i] = int.TryParse(parts[i], out var v) ? v : 0;
                parsed.Add(rowVals);
            }

            // Expand dynamic map width if needed
            if (_dynamic)
            {
                // Add missing rows
                if (fileRowCount > RowCount)
                {
                    for (int r = RowCount; r < fileRowCount; r++)
                        _jaggedMap!.Add(new int[ColCount > 0 ? ColCount : fileMaxCols]);
                }

                // Expand width if needed
                if (fileMaxCols > ColCount)
                {
                    for (int r = 0; r < _jaggedMap!.Count; r++)
                    {
                        var rowArr = _jaggedMap[r];
                        Array.Resize(ref rowArr, fileMaxCols);
                        _jaggedMap[r] = rowArr;
                    }
                }
            }
            else
            {
                // Static map: clamp to existing bounds, ignore overflow
                fileRowCount = Math.Min(fileRowCount, RowCount);
                fileMaxCols = Math.Min(fileMaxCols, ColCount);
            }

            int rowsToApply = _dynamic ? fileRowCount : Math.Min(fileRowCount, RowCount);
            for (int r = 0; r < rowsToApply; r++)
            {
                int colsAvailable = _dynamic ? parsed[r].Length : Math.Min(parsed[r].Length, ColCount);
                for (int c = 0; c < colsAvailable; c++)
                {
                    int v = parsed[r][c];
                    SetValue(c, r, v);
                    if (v != 0)
                    {
                        var tile = new Tile(c, r, _tileWidth(), _tileHeight(), true, v);
                        _tiles[(c, r)] = tile;
                        _game.AddEntity(tile);
                    }
                }
                // Remaining new cells (if any after width expansion) stay 0
            }
        }
    }
}
