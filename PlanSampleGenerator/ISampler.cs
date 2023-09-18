namespace PlanSampleGenerator
{
    public interface ISampler
    {
        public List<string> Sample(List<string> source, int count);
    }
}
