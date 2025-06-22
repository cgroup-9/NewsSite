namespace server.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Tag() { }

        public Tag(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
