namespace server.Models
{
    // ==========================
    // Source Model
    // ==========================
    // Purpose:
    // - Represents the "source" of a news article.
    // - Used when consuming data from external news APIs (e.g., NewsAPI),
    //   where each article contains information about the source it came from.
    //
    // Usage:
    // - Part of an Article object to indicate where the article was published.
    // - May be optional (null) if the source is not provided by the API.
    public class Source
    {
        // The unique identifier of the source.
        // Example: "bbc-news", "cnn"
        // Nullable because not all APIs return a source ID.
        public string? Id { get; set; }

        // The human-readable name of the source.
        // Example: "BBC News", "CNN"
        public string? Name { get; set; }

        // --------------------------
        // Constructors
        // --------------------------

        // Default constructor — required for serialization/deserialization.
        public Source() { }

        // Parameterized constructor for quickly creating a Source object.
        public Source(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
