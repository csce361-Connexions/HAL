﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Tweeter.Models
{
    public class EntityContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public EntityContext()
            : base("DefaultConnection")
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Post>().
                HasMany(p => p.likers).WithMany(u => u.likes).Map(
                m =>
                {
                    m.MapLeftKey("likes_PostId");
                    m.MapRightKey("likes_UserId");
                    m.ToTable("Likes");
                });
            modelBuilder.Entity<User>().
                HasMany(u => u.followers).WithMany(u => u.following).Map(
                m =>
                {
                    m.MapLeftKey("followerId");
                    m.MapRightKey("followingId");
                    m.ToTable("Follow");
                });
            modelBuilder.Entity<Hashtag>().
                HasMany(h => h.watchers).WithMany(u => u.watching).Map(
                m =>
                {
                    m.MapLeftKey("watcherId");
                    m.MapRightKey("watchingId");
                    m.ToTable("Watch");
                });
        }
    }
}