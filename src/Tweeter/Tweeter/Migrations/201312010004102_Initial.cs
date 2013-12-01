namespace Tweeter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        EmailAddress = c.String(),
                        verification = c.String(),
                        UserProfile_UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfile", t => t.UserProfile_UserId)
                .Index(t => t.UserProfile_UserId);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        postContent = c.String(maxLength: 200),
                        creator_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.creator_Id)
                .Index(t => t.creator_Id);
            
            CreateTable(
                "dbo.Hashtags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HashtagPosts",
                c => new
                    {
                        Hashtag_Id = c.Int(nullable: false),
                        Post_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Hashtag_Id, t.Post_Id })
                .ForeignKey("dbo.Hashtags", t => t.Hashtag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.Post_Id, cascadeDelete: true)
                .Index(t => t.Hashtag_Id)
                .Index(t => t.Post_Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.HashtagPosts", new[] { "Post_Id" });
            DropIndex("dbo.HashtagPosts", new[] { "Hashtag_Id" });
            DropIndex("dbo.Posts", new[] { "creator_Id" });
            DropIndex("dbo.Users", new[] { "UserProfile_UserId" });
            DropForeignKey("dbo.HashtagPosts", "Post_Id", "dbo.Posts");
            DropForeignKey("dbo.HashtagPosts", "Hashtag_Id", "dbo.Hashtags");
            DropForeignKey("dbo.Posts", "creator_Id", "dbo.Users");
            DropForeignKey("dbo.Users", "UserProfile_UserId", "dbo.UserProfile");
            DropTable("dbo.HashtagPosts");
            DropTable("dbo.Hashtags");
            DropTable("dbo.Posts");
            DropTable("dbo.UserProfile");
            DropTable("dbo.Users");
        }
    }
}
