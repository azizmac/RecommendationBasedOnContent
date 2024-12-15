using ContentBasedRecommender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentBasedRecommender.Services
{
    public class RecommendationService
    {
        private const float LEARNING_RATE = 0.1f;
        private const float CONFIDENCE_THRESHOLD = 0.7f;
        private readonly IUserVectorStorage _userVectorStorage;

        public RecommendationService(IUserVectorStorage userVectorStorage)
        {
            _userVectorStorage = userVectorStorage;
        }

        public void UpdateUserPreferences(User user, ClothingItem item, InteractionType interaction)
        {
            if (user.PreferenceVector.Length != item.Features.Length)
                throw new ArgumentException("Размерности векторов не совпадают");

            float direction = interaction == InteractionType.Like ? 1 : -1;

            for (int i = 0; i < user.PreferenceVector.Length; i++)
            {
                float adjustment = direction * LEARNING_RATE * item.Features[i];
                user.PreferenceVector[i] += adjustment;
            }

            NormalizeVector(user.PreferenceVector);
        }

        public float CalculateSimilarity(float[] vector1, float[] vector2)
        {
            // Косинусное сходство между векторами
            float dotProduct = vector1.Zip(vector2, (a, b) => a * b).Sum();
            float magnitude1 = (float)Math.Sqrt(vector1.Sum(x => x * x));
            float magnitude2 = (float)Math.Sqrt(vector2.Sum(x => x * x));

            return dotProduct / (magnitude1 * magnitude2);
        }

        private void NormalizeVector(float[] vector)
        {
            float magnitude = (float)Math.Sqrt(vector.Sum(x => x * x));
            if (magnitude > 0)
            {
                for (int i = 0; i < vector.Length; i++)
                {
                    vector[i] /= magnitude;
                }
            }
        }

        public async Task<List<ClothingItem>> GetTopRecommendationsAsync(
            string userId, 
            IEnumerable<ClothingItem> items, 
            int topN = 5)
        {
            var user = await _userVectorStorage.GetUserVectorAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            return items
                .Select(item => new 
                { 
                    Item = item, 
                    Similarity = CalculateSimilarity(user.PreferenceVector, item.Features)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(topN)
                .Select(x => x.Item)
                .ToList();
        }

        public async Task ProcessInteractionAsync(
            string userId, 
            ClothingItem item, 
            InteractionType interaction)
        {
            var user = await _userVectorStorage.GetUserVectorAsync(userId);
            if (user == null)
            {
                user = new User
                {
                    UserId = userId,
                    PreferenceVector = new float[item.Features.Length].Select(x => 1f / item.Features.Length).ToArray()
                };
            }

            UpdateUserPreferences(user, item, interaction);
            await _userVectorStorage.SaveUserVectorAsync(user);
        }

        public async Task<SimilarityMetrics> CalculateDetailedSimilarity(string userId, ClothingItem item)
        {
            var user = await _userVectorStorage.GetUserVectorAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            return new SimilarityMetrics
            {
                CosineSimilarity = CalculateSimilarity(user.PreferenceVector, item.Features),
                EuclideanDistance = CalculateEuclideanDistance(user.PreferenceVector, item.Features),
                Confidence = CalculateConfidence(user.PreferenceVector, item.Features)
            };
        }

        private float CalculateEuclideanDistance(float[] vector1, float[] vector2)
        {
            return (float)Math.Sqrt(vector1.Zip(vector2, (a, b) => (a - b) * (a - b)).Sum());
        }

        private float CalculateConfidence(float[] userVector, float[] itemVector)
        {
            float similarity = CalculateSimilarity(userVector, itemVector);
            float distance = CalculateEuclideanDistance(userVector, itemVector);
            
            // Нормализуем расстояние в диапазон [0,1]
            float normalizedDistance = 1 / (1 + distance);
            
            // Комбинируем метрики для получения уверенности
            return (similarity + normalizedDistance) / 2;
        }

        public async Task<Dictionary<string, float>> GetUserPreferencesAnalysis(string userId)
        {
            var user = await _userVectorStorage.GetUserVectorAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            // Анализируем вектор предпочтений и возвращаем основные характеристики
            return new Dictionary<string, float>
            {
                { "AveragePreference", user.PreferenceVector.Average() },
                { "PreferenceVariance", CalculateVariance(user.PreferenceVector) },
                { "PreferenceStrength", CalculateVectorMagnitude(user.PreferenceVector) }
            };
        }

        private float CalculateVariance(float[] vector)
        {
            float mean = vector.Average();
            return vector.Select(x => (x - mean) * (x - mean)).Average();
        }

        private float CalculateVectorMagnitude(float[] vector)
        {
            return (float)Math.Sqrt(vector.Sum(x => x * x));
        }

        public async Task<List<ClothingItem>> GetDiverseRecommendationsAsync(
            string userId,
            IEnumerable<ClothingItem> items,
            int topN = 5,
            float diversityWeight = 0.3f)
        {
            var user = await _userVectorStorage.GetUserVectorAsync(userId);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            var recommendations = new List<ClothingItem>();
            var remainingItems = items.ToList();

            while (recommendations.Count < topN && remainingItems.Any())
            {
                var nextItem = GetNextDiverseItem(user, remainingItems, recommendations, diversityWeight);
                recommendations.Add(nextItem);
                remainingItems.Remove(nextItem);
            }

            return recommendations;
        }

        private ClothingItem GetNextDiverseItem(
            User user,
            List<ClothingItem> candidates,
            List<ClothingItem> selectedItems,
            float diversityWeight)
        {
            return candidates
                .Select(item => new
                {
                    Item = item,
                    Score = CalculateDiversityScore(user, item, selectedItems, diversityWeight)
                })
                .OrderByDescending(x => x.Score)
                .First()
                .Item;
        }

        private float CalculateDiversityScore(
            User user,
            ClothingItem candidate,
            List<ClothingItem> selectedItems,
            float diversityWeight)
        {
            float similarityScore = CalculateSimilarity(user.PreferenceVector, candidate.Features);
            
            if (!selectedItems.Any())
                return similarityScore;

            float diversityScore = selectedItems
                .Select(item => 1 - CalculateSimilarity(item.Features, candidate.Features))
                .Average();

            return (1 - diversityWeight) * similarityScore + diversityWeight * diversityScore;
        }
    }
} 