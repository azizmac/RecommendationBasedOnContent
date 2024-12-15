namespace ContentBasedRecommender.Models
{
    public class SimilarityMetrics
    {
        public float CosineSimilarity { get; set; }
        public float EuclideanDistance { get; set; }
        public float Confidence { get; set; }
    }
} 