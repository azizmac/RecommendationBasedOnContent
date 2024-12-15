using ContentBasedRecommender.Models;
using ContentBasedRecommender.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentBasedRecommender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var storage = new FileUserVectorStorage("UserVectors");
            var recommendationService = new RecommendationService(storage);

            var items = CreateSampleItems();
            string userId = "user1";

            // Обрабатываем несколько взаимодействий
            await recommendationService.ProcessInteractionAsync(userId, items[0], InteractionType.Like);
            await recommendationService.ProcessInteractionAsync(userId, items[1], InteractionType.Dislike);

            // Получаем детальный анализ сходства
            var metrics = await recommendationService.CalculateDetailedSimilarity(userId, items[2]);
            Console.WriteLine($"\nАнализ сходства для товара {items[2].Name}:");
            Console.WriteLine($"Косинусное сходство: {metrics.CosineSimilarity:F2}");
            Console.WriteLine($"Евклидово расстояние: {metrics.EuclideanDistance:F2}");
            Console.WriteLine($"Уверенность: {metrics.Confidence:F2}");

            // Получаем анализ предпочтений пользователя
            var preferences = await recommendationService.GetUserPreferencesAnalysis(userId);
            Console.WriteLine("\nАнализ предпочтений пользователя:");
            foreach (var pref in preferences)
            {
                Console.WriteLine($"{pref.Key}: {pref.Value:F2}");
            }

            // Получаем разнообразные рекомендации
            var diverseRecommendations = await recommendationService.GetDiverseRecommendationsAsync(userId, items);
            Console.WriteLine("\nРазнообразные рекомендации:");
            foreach (var item in diverseRecommendations)
            {
                Console.WriteLine($"- {item.Name}");
            }
        }

        private static List<ClothingItem> CreateSampleItems()
        {
            return new List<ClothingItem>
            {
                new ClothingItem
                {
                    ItemId = "item1",
                    Name = "Красная футболка",
                    Features = new float[1000].Select((x, i) => i < 500 ? 1f : 0f).ToArray()
                },
                new ClothingItem
                {
                    ItemId = "item2",
                    Name = "Синие джинсы",
                    Features = new float[1000].Select((x, i) => i >= 500 ? 1f : 0f).ToArray()
                },
                new ClothingItem
                {
                    ItemId = "item3",
                    Name = "Черная куртка",
                    Features = new float[1000].Select((x, i) => i % 2 == 0 ? 1f : 0f).ToArray()
                }
            };
        }
    }
} 