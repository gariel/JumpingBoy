using System;
using System.Collections.Generic;
using System.Linq;
using LittleCoins;
using LittleCoins.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JumpingBoy
{
    public class Game : Layer
    {
        public event Action Restart;

        private float floor;
        private Random random;
        private bool isGameOver;

        private AnimatedSprite player;
        private ScrollBackground ground;

        private bool isJumping;
        private bool jumpKeyPressed = true;
        private float remainingJump;
        private float force;

        private ObstacleType[] obstacleTypes;
        private AnimatedSprite[] obstacles;
        private int obstacleIndex = 0;
        private float distanceNext = 1f;

        private double elapsed;
        private AssetGroup<Texture2D> textGroup;
        private List<FunnySprite> numbers;

        private FunnySprite gameOver;
        private Sprite replay;

        public override void Build()
        {
            random = new Random();
            floor = Rect.Height * 0.9f;

            player = new AnimatedSprite(
                new TextureKeysProvider(
                    Assets.Image.Group("player"),
                    "1", "2", "3", "4", "3", "2"))
            {
                Scale = new Vector2(0.5f),
                Left = Rect.Width * 0.1f,
                Bottom = floor,
            };
            Add(player);

            ground = new ScrollBackground
            {
                Top = floor,
                Left = Rect.Left,
                Speed = new Vector2(500, 0),
                Texture = Assets.Image.Load("ground", "ground")
            };
            Add(ground);

            textGroup = Assets.Image.Group("text");
            var scoreLabel = new FunnySprite
            {
                Texture = textGroup.Load("score"),
                Left = 100,
                Top = 30,
            };
            Add(scoreLabel);
            numbers = new List<FunnySprite>();
            for (var i = 0; i < 5; i++)
            {
                var n = new FunnySprite
                {
                    Left = scoreLabel.Right + 20 + i * 50,
                    Top = 50 - i * 10,
                    Speed = 30 + i * 10,
                };
                Add(n);
                numbers.Add(n);
            }

            var obsAssets = Assets.Image.Group("obstacles");
            obstacles = new AnimatedSprite[10];
            for (var i = 0; i < obstacles.Length; i++)
            {
                var a = new AnimatedSprite(null) {Right = Rect.Left};
                Add(a);
                obstacles[i] = a;
            }
            obstacleTypes = new []
            {
                new ObstacleType(
                    frequency: 1f,
                    framesPerSecond: 0,
                    fromGround: 0,
                    provider: new TextureKeysProvider(obsAssets, "rock")),
                new ObstacleType(
                    frequency: 0.1f,
                    framesPerSecond: 2,
                    fromGround: 100,
                    provider: new TextureKeysProvider(obsAssets.Group("bird"), "1", "2")),
            };

            gameOver = new FunnySprite
            {
                Texture = textGroup.Load("gameover"),
                CenteredOrigin = true,
                Left = Rect.Center.X,
                Top = Rect.Center.Y,
                Speed = 22,
                Visible = false,
            };
            Add(gameOver);
            replay = new Sprite
            {
                Texture = textGroup.Load("replay"),
                CenteredOrigin = true,
                Left = Rect.Center.X,
                Bottom = floor,
                Visible = false,
            };
            Add(replay);
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (isGameOver)
            {
                var state = Keyboard.GetState();
                var space = state.IsKeyDown(Keys.Space);
                if (isJumping)
                {
                    isJumping = space;
                }
                else if (space)
                {
                    Restart?.Invoke();
                }
            }
            else
            {
                Jump(delta);
                Roll(delta);
                Score(delta);
                isGameOver = ValidateGameOver();
            }
        }

        private void Jump(float delta)
        {
            var state = Keyboard.GetState();
            var jumpKey = state.IsKeyDown(Keys.Space);
            var jumpKeyJustPressed = jumpKey && !jumpKeyPressed;
            jumpKeyPressed = jumpKey;

            if (jumpKeyJustPressed && player.Bottom >= floor)
            {
                remainingJump = 100f;
                force = 300f;
                isJumping = true;
            }
            else if (!jumpKey)
            {
                isJumping = false;
                remainingJump = 0;
            }

            if (isJumping && remainingJump > 0f)
            {
                var use = 500 * delta;
                remainingJump -= use;
                player.Top -= use;
            }
            else
            {
                if (player.Bottom < floor)
                {
                    force -= 1000 * delta;
                    player.Top -= force * delta;
                }

                if (player.Bottom > floor)
                {
                    player.Bottom = floor;
                }
            }
        }

        private void Roll(float delta)
        {
            var current = obstacles[obstacleIndex];
            var movement = (float)(120 + elapsed * 5);

            if (distanceNext > 0)
            {
                distanceNext -= movement * delta;
                if (distanceNext <= 0)
                {
                    ObstacleType t = null;
                    var ok = false;
                    while (!ok)
                    {
                        t = obstacleTypes[random.Next(obstacleTypes.Length)];
                        ok = random.Next(10) < t.Frequency * 10;
                    }

                    current.TextureProvider = t.Provider;
                    current.FramesPerSecond = t.FramesPerSecond;
                    current.Bottom = floor - t.FromGround;
                    current.Paused = false;
                    current.Left = Rect.Right;
                    obstacleIndex = (obstacleIndex + 1) % obstacles.Length;
                }
            }

            if (current.Right <= Rect.Left && distanceNext < 0)
            {
                distanceNext = random.Next(250, 400);
                current.Paused = true;
            }


            foreach (var t in obstacles)
            {
                if (t.Right > Rect.Left)
                {
                    t.Left -= delta * movement;
                }
            }

            ground.Speed = new Vector2(movement, 0);
            player.FramesPerSecond = 6 + (int)(elapsed / 20);
        }

        private void Score(float delta)
        {
            elapsed += delta;
            if (elapsed > 49999)
            {
                return;
            }

            var score = ((int) (elapsed * 2)).ToString();
            for (var i = 0; i < score.Length; i++)
            {
                var n = numbers[i];
                n.Texture = textGroup.Load(score[i].ToString());
                n.Modulate = Color.Lerp(Color.DarkTurquoise, Color.DarkBlue, (float)0.2 * (score.Length - i - 1));
            }
        }

        private bool ValidateGameOver()
        {
            Func<Rectangle, Rectangle> shrink = rect => new Rectangle(
                (rect.Location.ToVector2() * 1.1f).ToPoint(),
                (rect.Size.ToVector2() * 0.9f).ToPoint());

            if (!obstacles.Any(o => shrink(o.Rect).Intersects(player.Rect)))
            {
                return false;
            }

            foreach (var a in Things.OfType<IPausable>())
            {
                a.Paused = true;
            }

            gameOver.Paused = false;
            gameOver.Visible = true;
            replay.Visible = true;

            return true;
        }
    }
}
