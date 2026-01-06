namespace Agile_Actors_Assignment.Models
{
    public class AggregatedStatisticsPerAPI
    {
        public int totalRequestCount { get; set; }
        public float averageResponseTimeMs { get; set; } = 0;
        public Dictionary<string, int> groupedByPerformanceDictionary { get; set; } = new(); // fast, average, slow
    }
}
