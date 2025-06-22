namespace server.Models
{
    public class Source
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        public Source() { }

        public Source(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
