using server.DAL;

//
// ==========================
// SharedArticleRequest Model
// ==========================
//
// Purpose:
// - Represents the data needed when a user shares an article.
// - Acts as a Data Transfer Object (DTO) for the "Share Article" feature.
// - Encapsulates the properties and the database call for sharing an article.
//
public class SharedArticleRequest
{
    // --------------------------
    // Properties
    // --------------------------

    // The ID of the user who is sharing the article.
    public int UserId { get; set; }

    // The unique URL of the article being shared.
    // Used to identify the article in the database and link it to its metadata.
    public string ArticleUrl { get; set; }

    // An optional comment provided by the user when sharing.
    // This is stored together with the share record.
    public string Comment { get; set; }

    // --------------------------
    // Constructors
    // --------------------------

    // Default constructor — required for serialization/deserialization.
    public SharedArticleRequest() { }

    // Parameterized constructor — quickly initialize all properties.
    public SharedArticleRequest(int userId, string articleUrl, string comment)
    {
        UserId = userId;
        ArticleUrl = articleUrl;
        Comment = comment;
    }

    // --------------------------
    // Methods
    // --------------------------

    // Shares the article by calling the corresponding DB service method.
    //
    // Process:
    // 1. Creates an instance of DBservicesSharedArticles (Data Access Layer).
    // 2. Calls ShareArticle() and passes the current object as the data to insert.
    // 3. The DAL executes the stored procedure SP_ShareArticle_FP.
    // 4. Returns the integer result (could be a success code, inserted ID, or error code).
    public int Share()
    {
        DBservicesSharedArticles db = new DBservicesSharedArticles();
        return db.ShareArticle(this);
    }
}
