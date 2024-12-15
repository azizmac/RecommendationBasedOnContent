using ContentBasedRecommender.Models;
using System.Threading.Tasks;

namespace ContentBasedRecommender.Services
{
    public interface IUserVectorStorage
    {
        Task SaveUserVectorAsync(User user);
        Task<User> GetUserVectorAsync(string userId);
    }
} 