using LittleCoins.Things;

namespace JumpingBoy
{
    class ObstacleType
    {
        public ObstacleType(float frequency, int framesPerSecond, int fromGround, ITexturesProvider provider)
        {
            Frequency = frequency;
            FramesPerSecond = framesPerSecond;
            FromGround = fromGround;
            Provider = provider;
        }

        public float Frequency { get; set; }
        public int FramesPerSecond { get; set; }
        public ITexturesProvider Provider { get; set; }
        public int FromGround { get; set; }
    }
}
