using server.DAL;

public class SharedArticleRequest
{
    public int UserId { get; set; }
    public string ArticleUrl { get; set; }  
    public string Comment { get; set; }

    public SharedArticleRequest() { }

    public SharedArticleRequest(int userId, string articleUrl, string comment)
    {
        UserId = userId;
        ArticleUrl = articleUrl;
        Comment = comment;
    }

    public int Share()
    {
        DBservicesSharedArticles db = new DBservicesSharedArticles();
        return db.ShareArticle(this);
    }

}
