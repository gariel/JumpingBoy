using LittleCoins.Things;
using Microsoft.Xna.Framework;

namespace JumpingBoy
{
    public class GameScene : Scene
    {
        public override Color BackColor => Color.Bisque;

        public override void Build()
        {
            var g = new Game();
            g.Restart += Restart;
            AddLayer(g);
        }

        private void Restart()
        {
            ClearLayers();
            Built = false;
        }
    }
}
