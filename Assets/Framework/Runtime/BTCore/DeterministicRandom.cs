namespace XSkillSystem
{
    /// <summary>
    /// 确定性随机实现（32位LCG）。
    /// </summary>
    public struct DeterministicRandom : IBTRandom
    {
        private readonly System.Random _rng;
        uint _state;

        public DeterministicRandom(int seed)
        {
            _state = (uint)seed | 1u;
            _rng = new System.Random(seed);
        }

        uint Step() => _state = _state * 1103515245u + 12345u;
        public int Next() => (int)(Step() >> 1);
        public int Next(int maxExclusive) => (int)(NextDouble() * maxExclusive);
        public int Next(int minInclusive, int maxExclusive) => minInclusive + Next(maxExclusive - minInclusive);
        public double NextDouble() => (Step() & 0xFFFFFF) / (double)0x1000000; // [0,1)

        public float NextFloat01() => (float)_rng.NextDouble();
        public int NextIntInclusive(int min, int max) => _rng.Next(min, max + 1);
    }
}