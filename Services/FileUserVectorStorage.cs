using ContentBasedRecommender.Models;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContentBasedRecommender.Services
{
    public class FileUserVectorStorage : IUserVectorStorage
    {
        private readonly string _storageDirectory;

        public FileUserVectorStorage(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
            Directory.CreateDirectory(_storageDirectory);
        }

        public async Task SaveUserVectorAsync(User user)
        {
            var filePath = Path.Combine(_storageDirectory, $"{user.UserId}.json");
            var json = JsonSerializer.Serialize(user);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<User> GetUserVectorAsync(string userId)
        {
            var filePath = Path.Combine(_storageDirectory, $"{userId}.json");
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<User>(json);
        }
    }
} 