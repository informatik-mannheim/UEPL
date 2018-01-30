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
        public DbSet<LoginToken> Tokens { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<ApplicationUser> Users { get; set; }

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
            modelBuilder.Entity<Artifact>().HasMany(a => a.StorageItems).WithOne(asi => asi.ArtifactRef).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ArtifactStorageItem>().HasOne(asi => asi.ArtifactRef).WithMany(a => a.StorageItems).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LoginToken>().HasOne(lt => lt.User);


            var fks = modelBuilder.Entity<Package>().Metadata.GetForeignKeys();

            foreach (var fk in fks)
            {
                if (fk.PrincipalEntityType.ClrType.Name == "Lecture")
                    fk.DeleteBehavior = DeleteBehavior.SetNull;
            }

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
