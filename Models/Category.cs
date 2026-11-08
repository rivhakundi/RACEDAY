public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Open Men", "U18"
    public int EventId { get; set; }
    public Event? Event { get; set; }
}