using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace fakebook.Models;

[PrimaryKey(nameof(FollowerId), nameof(FollowedId))]
public class Follow
{
    [Required]
    public int FollowerId { get; set; }
    [Required]
    public int FollowedId { get; set; }
    [Required]
    public bool Pending { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Follower { get; set; }
    public User Followed { get; set; }
}
