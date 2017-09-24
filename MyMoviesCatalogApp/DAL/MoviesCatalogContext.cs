using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MyMoviesCatalogApp.Models;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace MyMoviesCatalogApp.DAL
{
    public class MoviesCatalogContext : IdentityDbContext<ApplicationUser> 
    {
        public MoviesCatalogContext() : base("DefaultConnection")
        {
        }

        public static MoviesCatalogContext Create()
        {
            return new MoviesCatalogContext();
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<GenreGroup> GenreGroups { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Writer> Writers { get; set; }
        public DbSet<Actor> Actors { get; set; }

        public T GetOrAdd<T>(T entity) where T : MovieCatalogEntity
        {
            Func<T, bool> lookup_func;
            DbSet<T> lookup_source = null;
            T lookup_value = null;

            if (typeof(T).Name == "Person")
            {
                lookup_func = (o => Equals((o as Person).FirstName, (entity as Person).FirstName)
                        && Equals((o as Person).LastName, (entity as Person).LastName)
                        && Equals((o as Person).MiddleName, (entity as Person).MiddleName));
                lookup_source = Persons as DbSet<T>;                
            }
            else
            {
                lookup_func = (o => o.Name.Equals(entity.Name));
                switch (typeof(T).Name)
                {
                    case "Actor":
                        lookup_source = Actors as DbSet<T>;
                        break;
                    case "Genre":
                        lookup_source = Genres as DbSet<T>;
                        break;
                    case "GenreGroup":
                        lookup_source = GenreGroups as DbSet<T>;
                        break;
                    case "Writer":
                        lookup_source = Writers as DbSet<T>;
                        break;
                }
            }
            if (lookup_source != null)
            {
                lookup_value = lookup_source.Where(lookup_func).FirstOrDefault();
                if (lookup_value == null)
                { 
                    lookup_value = lookup_source.Local.Where(lookup_func).FirstOrDefault();
                    if (lookup_value == null)
                        lookup_source.Add(entity);
                }
            }
            if (lookup_value == null)
                lookup_value = entity;

            return lookup_value;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<Movie>().HasMany(m => m.Genres).WithMany(m => m.Movies);
            modelBuilder.Entity<Movie>().HasOptional(m => m.Director);
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is MovieCatalogEntity trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.CreatedUserID = null;
                            Entry(trackable).Property(x => x.CreatedAt).IsModified = false;
                            Entry(trackable).Property(x => x.CreatedUserID).IsModified = false;
                            trackable.LastUpdatedAt = now;
                            break;

                        case EntityState.Added:
                            trackable.LastUpdatedAt = null;
                            Entry(trackable).Property(x => x.LastUpdatedAt).IsModified = false;
                            Entry(trackable).Property(x => x.LastUpdatedUserID).IsModified = false;
                            trackable.CreatedAt = now;
                            break;
                    }
                }
            }
        }
    }
}