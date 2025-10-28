using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace Underground_Psychosis.GameEngine
{
    public class GameLoop
    {
        private readonly Canvas _canvas;
        private readonly List<Entity> _entities = new();
        private readonly List<Entity> _pendingAdds = new();
        private readonly List<Entity> _pendingRemoves = new();
        private DateTime _lastUpdate;

        public GameLoop(Canvas canvas)
        {
            _canvas = canvas;
            _lastUpdate = DateTime.Now;
        }

        // Deferred add: queue entity to be added at a safe point
        public void AddEntity(Entity entity)
        {
            if (entity == null) return;
            if (!_pendingAdds.Contains(entity) && !_entities.Contains(entity))
                _pendingAdds.Add(entity);
        }

        // Deferred remove: queue entity to be removed at a safe point
        public void RemoveEntity(Entity entity)
        {
            if (entity == null) return;
            if (!_pendingRemoves.Contains(entity))
                _pendingRemoves.Add(entity);
        }

        public void Start()
        {
            CompositionTarget.Rendering += GameLoopTick;
        }
        public void Stop()
        {
            CompositionTarget.Rendering -= GameLoopTick;
        }

        public event Action? Tick;

        private void GameLoopTick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var deltaTime = (now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            // Use a stable snapshot for Update so updates can't invalidate the enumerator
            var updateSnapshot = _entities.ToList();
            foreach (var entity in updateSnapshot)
                entity.Update(deltaTime);

            ResolvePlayerTileCollisions();

            // Apply pending changes after Update and collision resolution so Draw sees current state
            ApplyPendingChanges();

            // Draw from a snapshot to avoid modifications during draw
            var drawSnapshot = _entities.ToList();
            foreach (var entity in drawSnapshot)
                entity.Draw(_canvas);

            // Apply any changes that occurred during Draw (unlikely, but safe)
            ApplyPendingChanges();

            Tick?.Invoke();
        }

        private void ApplyPendingChanges()
        {
            if (_pendingRemoves.Count > 0)
            {
                foreach (var e in _pendingRemoves)
                {
                    _entities.Remove(e);
                    if (e.Sprite != null && _canvas.Children.Contains(e.Sprite))
                        _canvas.Children.Remove(e.Sprite);
                }
                _pendingRemoves.Clear();
            }

            if (_pendingAdds.Count > 0)
            {
                _entities.AddRange(_pendingAdds);
                _pendingAdds.Clear();
            }
        }

        private void ResolvePlayerTileCollisions()
        {
            var tiles = _entities.OfType<Tile>().Where(t => t.IsSolid).ToList();
            foreach (var player in _entities.OfType<Player>())
            {
                player.ResolveCollisions(tiles);
            }
            foreach (var player in _entities.OfType<PlayerTwo>())
            {
                player.ResolveCollisions(tiles);
            }
        }
    }
}
