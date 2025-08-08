using server.DAL;

//
// ===========================
// SharedArticleIndex Model
// ===========================
//
// Purpose:
// - Represents a shared article record as it is displayed in the UI.
// - Holds all the metadata for an article shared by a user.
// - Used when retrieving and listing shared articles from the database.
//
public class SharedArticleIndex
{
    // --------------------------
    // Properties
    // --------------------------

    // Unique ID for the shared article record in the database.
    public int SharedId { get; set; }

    // ID of the user who shared the article.
    public int UserId { get; set; }

    // Name of the user who shared the article (for display purposes).
    public string UserName { get; set; }

    // The unique URL of the original article.
    public string ArticleUrl { get; set; }

    // Title of the article.
    public string Title { get; set; }

    // URL to an image related to the article (thumbnail/cover).
    public string UrlToImage { get; set; }

    // Author of the article.
    public string Author { get; set; }

    // Comment provided by the user when sharing the article.
    public string Comment { get; set; }

    // Date and time when the article was shared.
    public DateTime DateShared { get; set; }

    // Number of likes this shared article has received.
    public int LikesCount { get; set; }

    // Indicates whether the current logged-in user has already liked this article.
    public bool AlreadyLiked { get; set; }

    // --------------------------
    // Constructors
    // --------------------------

    // Default constructor — required for serialization/deserialization.
    public SharedArticleIndex() { }

    // Parameterized constructor — allows quick initialization of main properties.
    public SharedArticleIndex(
        int sharedId,
        int userId,
        string userName,
        string articleUrl,
        string title,
        string urlToImage,
        string author,
        string comment,
        DateTime dateShared)
    {
        SharedId = sharedId;
        UserId = userId;
        UserName = userName;
        ArticleUrl = articleUrl;
        Title = title;
        UrlToImage = urlToImage;
        Author = author;
        Comment = comment;
        DateShared = dateShared;
    }

    // --------------------------
    // Methods
    // --------------------------

    // Retrieves a paginated list of shared articles from the database.
    //
    // Parameters:
    // - hiddenUserIds: Optional list of user IDs whose shared articles should be excluded.
    // - page: The current page number for pagination.
    // - pageSize: The number of records per page.
    // - currentUserId: ID of the current user (used to determine if AlreadyLiked is true).
    //
    // Returns:
    // - A list of SharedArticleIndex objects populated from the database.
    public static List<SharedArticleIndex> GetAllSharedArticles(
        string? hiddenUserIds,
        int page,
        int pageSize,
        int? currentUserId)
    {
        DBservicesSharedArticles db = new();
        return db.GetSharedArticles(hiddenUserIds, page, pageSize, currentUserId);
    }
}
