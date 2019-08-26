using System;
using LittleCoins.Things;

namespace JumpingBoy
{
    public class FunnySprite : Sprite, IPausable
    {
        private float down = 0f;
        private float left = 0f;

        private int direction = 0;

        public float Distance { get; set; } = 10f;
        public float Speed { get; set; } = 50f;
        public int Delay { get; set; } = 0;
        public bool Paused { get; set; }

        public override void Update(float delta)
        {
            base.Update(delta);

            if (Paused)
            {
                return;
            }

            if (Delay > 0)
            {
                var d = (int)(delta * 1000);
                Delay = Math.Max(0, Delay - d);
                return;
            }

            var move = Speed * delta;
            if (direction == 0)
            {
                var to = left + move;
                if (to > Distance)
                {
                    direction = 1;
                    Left += Distance - left;
                    left = Distance;
                }
                else
                {
                    Left += move;
                    left += move;
                }
            }
            else if (direction == 1)
            {
                var to = down + move;
                if (to > Distance)
                {
                    direction = 2;
                    Top += Distance - down;
                    down = Distance;
                }
                else
                {
                    Top += move;
                    down += move;
                }
            }

            else if (direction == 2)
            {
                var to = left - move;
                if (to < 0)
                {
                    direction = 3;
                    Left -= left;
                }
                else
                {
                    Left -= move;
                    left -= move;
                }
            }

            else if (direction == 3)
            {
                var to = down - move;
                if (to < 0)
                {
                    direction = 0;
                    Top -= down;
                }
                else
                {
                    Top -= move;
                    down -= move;
                }
            }
        }
    }
}
