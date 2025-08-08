using server.Models;


namespace server.Models
{
    // Represents a request to delete a specific saved article for a given user.
    // Used by the API to identify which article to remove from the database.
    public class ArticleDeleteRequest
    {
        public int UserId { get; set; }        // The ID of the user requesting the deletion.
        public string ArticleUrl { get; set; } // The unique URL of the article to be deleted.
    }
}



