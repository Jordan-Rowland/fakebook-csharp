using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fakebook.Models;

public class Post
{
    [Key]
    [Required]
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    [MaxLength(400)]
    public string Body { get; set; }
    [ForeignKey("Id")]
    public int? ParentId {  get; set; }
    [Required]
    public PostStatus Status { get; set; }
    [Required]
    public DateTime CreatedAt {  get; set; }
    [Required]
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; }
    public Post Parent { get; set; }
    public ICollection<Post> Replies { get; set; }
}

public enum PostStatus { PUBLISED, DRAFT, DELETED }