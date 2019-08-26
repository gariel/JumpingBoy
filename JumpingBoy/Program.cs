using System;
using LittleCoins;
using Microsoft.Xna.Framework;

namespace JumpingBoy
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new LittleGame
            {
                Resolution = new Point(700, 400),
                Aspect = Aspect.Keep,
            })
            {
                game.Run(new GameScene());
            }
        }
    }
}
