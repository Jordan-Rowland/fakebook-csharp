﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace fakebook.Models;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User, ApplicationRole, int>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // https://medium.com/@dmitry.pavlov/tree-structure-in-ef-core-how-to-configure-a-self-referencing-table-and-use-it-53effad60bf
        modelBuilder.Entity<Post>()
            .HasOne(x => x.Parent)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Post>()
            .HasOne(x => x.User)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }

    public DbSet<Post> Posts => Set<Post>();
    //public DbSet<User> Users => Set<User>();  // This seems to be covered by Identity
    public DbSet<Follow> Follows => Set<Follow>();
}

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole(string roleName) : base(roleName) { }
    public ApplicationRole() : base() { }
}
