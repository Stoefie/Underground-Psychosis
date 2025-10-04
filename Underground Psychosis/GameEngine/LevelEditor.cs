using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Underground_Psychosis.GameEngine
{
    public class LevelEditor
    {
        private readonly Canvas _canvas;
        private readonly int[,] _map;
        private readonly Dictionary<(int x, int y), Tile> _tiles;
        private readonly GameLoop _game;
        private readonly Func<double> _tileWidth;
        private readonly Func<double> _tileHeight;

        private readonly Rectangle _cursor;
        private readonly TextBlock _status;
        private bool _enabled;
        private int _currentKind = 1;

        //Drag painting state
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
                _status.Text = value ? $"EDITOR ON  (1/2 select kind, LMB place, RMB remove, Ctrl+S save, Ctrl+L load)  Kind={_currentKind}" : "";
            }
        }

        public LevelEditor(GameLoop game, Canvas canvas, int[,] map, Dictionary<(int x, int y), Tile> tiles, Func<double> tileWidthProvider, Func<double> tileHeightProvider)
        {
            _game = game;
            _canvas = canvas;
            _map = map;
            _tiles = tiles;
            _tileWidth = tileWidthProvider;
            _tileHeight = tileHeightProvider;

            _cursor = new Rectangle
            {
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(40, 255, 255, 0)),
                IsHitTestVisible = false,
                Visibility = Visibility.Hidden
            };
            _canvas.Children.Add(_cursor);

            _status = new TextBlock
            {
                Foreground = Brushes.Yellow,
                Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
                FontSize = 14,
                Text = "",
                IsHitTestVisible = false
            };
            Canvas.SetLeft(_status, 4);
            Canvas.SetTop(_status, 4);
            _canvas.Children.Add(_status);
        }

        public void OnMouseMove(Point p)
        {
            if (_tileWidth() <= 0 || _tileHeight() <= 0) return;
            var (cx, cy) = WorldToCell(p);
            if (!InBounds(cx,cy)) return;

            if (Enabled)
            { 
                _cursor.Width = _tileWidth();
                _cursor.Height = _tileHeight();
                Canvas.SetLeft(_cursor, cx * _tileWidth());
                Canvas.SetTop(_cursor, cy * _tileHeight());
            }

            if (Enabled && _isPainting)
            {
                if (_lastPaintCell != (cx, cy))
                {
                    if (_paintMode == PaintMode.Place)
                        PlaceTile (cx, cy, _currentKind, allowSameReplace: false);
                    else if (_paintMode == PaintMode.Erase)
                        RemoveTile(cx, cy);

                    _lastPaintCell = (cx, cy);
                }
            }

            /*
            if (!Enabled) return;
            double w = _tileWidth();
            double h = _tileHeight();
            if (w <= 0 || h <= 0) return;

            var (cx, cy) = WorldToCell(p);
            if (cx < 0) return;
            if (!InBounds(cx, cy)) return;
            
            if (Enabled && _isPainting)
                _cursor.Width = _tileWidth;
                _cursor.Height = _tileHeight;
                Canvas.SetLeft(_cursor, cx * _tileWidth());
                Canvas.SetTop(_cursor, cy * _tileHeight());

            /*
            if (_tileWidth() <= 0 || _tileHeight() <= 0) return;
            var (cx, cy) = WorldToCell(p);
            if (!InBounds(cx,cy)) return;
            Canvas.SetLeft(_cursor, cx * _tileWidth());
            Canvas.SetTop(_cursor, cy * _tileHeight());
            _cursor.Width = _tileWidth();
            _cursor.Height = _tileHeight(); */
        }

        public void OnMouseDown(Point p, MouseButton button)
        {
            if (!Enabled) return;
            var (cx, cy) = WorldToCell(p);
            if (!InBounds(cx,cy)) return;

            _lastPaintCell = (-9999, -9999); // Reset drag cache

            if (button == MouseButton.Left)
            {
                _paintMode = PaintMode.Place;
                _isPainting = true;
                PlaceTile(cx, cy, _currentKind, allowSameReplace: false);
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
            if ((button == MouseButton.Left && _paintMode == PaintMode.Place) || (button == MouseButton.Right && _paintMode == PaintMode.Erase))
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


            if (ctrl && key == Key.S) Save("level_edit.txt");
            else if (ctrl && key == Key.L) Load("level_edit.txt");
        }

        private void RefreshStatus()
        {
            if (_enabled)
                _status.Text = $"EDITOR ON  (1/2 select kind, LMB place, RMB remove, Ctrl+S save, Ctrl+L load)  Kind={_currentKind}";
        }

        private (int x, int y) WorldToCell(Point p)
        {
            double w = _tileWidth();
            double h = _tileHeight();
            if (w <= 0 || h <= 0) return (-1, -1);
            return ((int)(p.X / w), (int)(p.Y / h));
        }

        private bool InBounds(int x, int y)
            => (y >= 0 && y < _map.GetLength(0) && x >= 0 && x < _map.GetLength(1));

        private void PlaceTile(int x, int y, int kind, bool allowSameReplace)
        {
            if (!allowSameReplace && _map[y, x] == kind) return;

            RemoveTile(x, y); // remove previous entity if any
            _map[y, x] = kind;
            
            if (kind != 0)
            {
                var tile = new Tile(x, y, _tileWidth(), _tileHeight(), isSolid: true, kind: kind);
                _tiles[(x, y)] = tile;
                _game.AddEntity(tile);
            }
        }
        
        private void RemoveTile(int x, int y)
        {
            if (_tiles.Remove((x,y), out var tile))
            {
                _game.RemoveEntity(tile);
            }
            _map[y, x] = 0;
        }

        public void Save(string path)
        {
            using var sw = new StreamWriter(path);
            int rows = _map.GetLength(0);
            int cols = _map.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    sw.Write(_map[r, c]);
                    if (c < cols - 1) sw.Write(',');
                }
                sw.WriteLine();
            }
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) return;

            // Clear all existing tile entities
            foreach (var kv in _tiles.Values)
            {
                _game.RemoveEntity(kv);
            }
            _tiles.Clear();

            var lines = File.ReadAllLines(path);
            int rows = Math.Min(lines.Length, _map.GetLength(0));
            int cols = _map.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                var parts = lines[r].Split(',');
                for (int c = 0; c < Math.Min(parts.Length, cols); c++)
                {
                    if (int.TryParse(parts[c], out int v))
                    {
                        _map[r, c] = v;
                        if (v != 0)
                        {
                            var tile = new Tile(c, r, _tileWidth(), _tileHeight(), true, v);
                            _tiles[(c, r)] = tile;
                            _game.AddEntity(tile);
                        }
                    }
                }
            }
        }
        }
    }
