namespace server.Models
{
    //
    // ======================================
    // LikeRequest Model
    // ======================================
    //
    // Purpose:
    // - Represents a "like" action made by a user on a shared article.
    // - Used when the client (frontend) sends a request to the server
    //   to either like or unlike a specific shared article.
    // - Typically passed to a controller endpoint that handles likes.
    //
    public class LikeRequest
    {
        // The unique ID of the shared article being liked or unliked.
        // This ID is usually from the SharedArticles_FP table.
        public int SharedArticleId { get; set; }

        // The unique ID of the user performing the like/unlike action.
        public int UserId { get; set; }

        // ======================
        // Executes a "Like" action via DB layer
        // ======================
        // Delegates to DBservicesSharedArticles.LikeSharedArticleAsync
        public async Task<bool> LikeAsync()
        {
            var db = new server.DAL.DBservicesSharedArticles();
            return await db.LikeSharedArticleAsync(SharedArticleId, UserId);
        }

        // ======================
        // Executes an "Unlike" action via DB layer
        // ======================
        // Delegates to DBservicesSharedArticles.UnlikeSharedArticleAsync
        public async Task<bool> UnlikeAsync()
        {
            var db = new server.DAL.DBservicesSharedArticles();
            return await db.UnlikeSharedArticleAsync(SharedArticleId, UserId);
        }
    }
}
