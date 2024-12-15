using Microsoft.ML.Data;

namespace ContentBasedRecommender.Models
{
    public class ClothingItem
    {
        [LoadColumn(0)]
        public string ItemId { get; set; }

        [LoadColumn(1)]
        public string Name { get; set; }

        [LoadColumn(2)]
        public float[] Features { get; set; } // вектор из 1000 элементов
    }
} 