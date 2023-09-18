namespace PlanSampleGenerator
{
    public class RandomSampler : ISampler
    {
        public int Seed { get; set; } = -1;
        public RandomSampler(int seed)
        {
            Seed = seed;
        }

        public List<string> Sample(List<string> source, int count)
        {
            List<string> subset = new List<string>();
            var rnd = GetRandomizer(Seed);

            while (subset.Count < count)
            {
                var target = rnd.Next(0, source.Count);
                if (!subset.Contains(source[target]))
                    subset.Add(source[target]);
            }

            return subset;
        }

        private Random GetRandomizer(int seed)
        {
            if (seed == -1)
                return new Random();
            else
                return new Random(seed);
        }
    }
}
