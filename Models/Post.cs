namespace fakebook.Models;

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Body { get; set; }
    public int? ParentId {  get; set; }
    public PostStatus Status { get; set; }
    public DateTime CreatedAt {  get; set; }
    public DateTime? DeletedAt { get; set; }
}

public enum PostStatus { PUBLISED, DRAFT, DELETED }