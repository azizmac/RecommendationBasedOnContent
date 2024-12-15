namespace ContentBasedRecommender.Models
{
    public class User
    {
        public string UserId { get; set; }
        public float[] PreferenceVector { get; set; } // вектор из 1000 элементов
    }
} 