using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ULCWebAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ULCWebAPI.Security;

namespace ULCWebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class APIDatabaseContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        public DbSet<Artifact> Artifacts { get; set; }

        /// <summary>
        /// Versioning
        /// </summary>
        public DbSet<ArtifactBackup> ArtifactsBackup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<Lecture> Lectures { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<Package> Packages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<ArtifactStorageItem> ArtifactStorage { get; set; }

        /// <summary>
        /// Versioning
        /// </summary>
        public DbSet<ArtifactBackupStorageItem> ArtifactStorageBackup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<LectureStorageItem> LectureStorage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<LoginToken> Tokens { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<ApplicationUser> Users { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<UserLecture> UserLectures { get; set; }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./packages.db");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1:n binding Lecture : Package
            modelBuilder.Entity<Lecture>().HasMany(l => l.Contents);

            // 1:n binding Package : Package
            modelBuilder.Entity<Package>().HasMany(p => p.Dependencies);

            // 1:n binding Artifact : ArtifactStorageItem
            modelBuilder.Entity<Artifact>().HasMany(a => a.StorageItems).WithOne(asi => asi.ArtifactRef).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ArtifactStorageItem>().HasOne(asi => asi.ArtifactRef).WithMany(a => a.StorageItems).OnDelete(DeleteBehavior.SetNull);

            // 1:n binding Artifact : ArtifactBackup
            modelBuilder.Entity<Artifact>().HasMany(a => a.Backups).WithOne(abi => abi.ArtifactRef).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ArtifactBackup>().HasOne(abi => abi.ArtifactRef).WithMany(a => a.Backups).OnDelete(DeleteBehavior.SetNull);

            // 1:n binding ArtifactBackup : ArtifactBackupStorageItem
            modelBuilder.Entity<ArtifactBackup>().HasMany(ab => ab.StorageItems).WithOne(absi => absi.ArtifactBackupRef).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ArtifactBackupStorageItem>().HasOne(absi => absi.ArtifactBackupRef).WithMany(ab => ab.StorageItems).OnDelete(DeleteBehavior.SetNull);

            // 1:n binding Lecture : LectureStorageItem
            modelBuilder.Entity<Lecture>().HasMany(a => a.StorageItems).WithOne(asi => asi.LectureRef).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LectureStorageItem>().HasOne(asi => asi.LectureRef).WithMany(a => a.StorageItems).OnDelete(DeleteBehavior.Cascade);

            // 1:1 binding LoginToken : User
            modelBuilder.Entity<LoginToken>().HasOne(lt => lt.User);

            // -- START BINDING --
            // m:n binding User : Lecture
            modelBuilder.Entity<UserLecture>().HasKey(ul => new { ul.LectureID, ul.UserID });

            modelBuilder.Entity<UserLecture>()
                .HasOne(sc => sc.User)
                .WithMany(s => s.UserLectures)
                .HasForeignKey(sc => sc.UserID);


            modelBuilder.Entity<UserLecture>()
                .HasOne(sc => sc.Lecture)
                .WithMany(s => s.UserLectures)
                .HasForeignKey(sc => sc.LectureID);
            // -- END BINDING --

            // Get all foreign keys and change the delete behaviour to SetNull
            var fks = modelBuilder.Entity<Package>().Metadata.GetForeignKeys();

            foreach (var fk in fks)
            {
                if (fk.PrincipalEntityType.ClrType.Name == "Lecture")
                    fk.DeleteBehavior = DeleteBehavior.SetNull;
            }

            // Get all foreign keys and change the delete behaviour to Cascade
            // If an ApplicationUser were deleted from the db, the corresponding token will be deleted.
            fks = modelBuilder.Entity<LoginToken>().Metadata.GetForeignKeys();

            foreach(var fk in fks)
            {
                if (fk.PrincipalEntityType.ClrType.Name == "ApplicationUser")
                    fk.DeleteBehavior = DeleteBehavior.Cascade;
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
