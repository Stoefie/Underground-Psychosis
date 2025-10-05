using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underground_Psychosis.GameEngine
{
    public class GameLoop
    {
        private readonly Canvas _canvas;
        private readonly List<Entity> _entities = new();
        private DateTime _lastUpdate;
        
        public GameLoop (Canvas canvas)
        {
            _canvas = canvas;
            _lastUpdate = DateTime.Now;
        }

        public void AddEntity(Entity entity) => _entities.Add(entity);

        public void RemoveEntity(Entity entity)
        {
            _entities.Remove(entity);
            if (entity.Sprite != null && _canvas.Children.Contains(entity.Sprite))
                _canvas.Children.Remove(entity.Sprite);
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

            foreach(var entity in _entities)
                entity.Update(deltaTime);

            ResolvePlayerTileCollisions();

            foreach(var entity in _entities)
                entity.Draw(_canvas);

            Tick?.Invoke();
        }

        private void ResolvePlayerTileCollisions()
        {
            var tiles = _entities.OfType<Tile>().Where(t => t.IsSolid).ToList();
            foreach (var player in _entities.OfType<Player>())
            {
                player.ResolveCollisions(tiles);
            }
        }
    }
}
