using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using ProjectAPI.Models;

namespace ProjectAPI.Models
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
        public DbSet<ArtifactStorageItem> Storage { get; set; }

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
            modelBuilder.Entity<Lecture>().HasMany(l => l.Contents);
            modelBuilder.Entity<Package>().HasMany(p => p.Dependencies);
            modelBuilder.Entity<Artifact>().HasMany(a => a.StorageItems).WithOne(asi => asi.ArtifactRef).OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);
            modelBuilder.Entity<ArtifactStorageItem>().HasOne(asi => asi.ArtifactRef).WithMany(a => a.StorageItems);

            modelBuilder.Entity<Lecture>().Property(l => l.Version).ForSqliteHasDefaultValue(0);
            modelBuilder.Entity<Package>().Property(p => p.Version).ForSqliteHasDefaultValue(0);
            modelBuilder.Entity<Artifact>().Property(a => a.Version).ForSqliteHasDefaultValue(0);
            modelBuilder.Entity<ArtifactStorageItem>().Property(asi => asi.Version).ForSqliteHasDefaultValue(0);

            base.OnModelCreating(modelBuilder);
        }
    }
}
