namespace ContentBasedRecommender.Models
{
    public enum InteractionType
    {
        Like = 1,
        Dislike = -1
    }

    public class UserInteraction
    {
        public string UserId { get; set; }
        public string ItemId { get; set; }
        public InteractionType Type { get; set; }
    }
} 