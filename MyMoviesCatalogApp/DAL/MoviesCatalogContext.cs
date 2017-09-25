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
            Func<T, bool> lookup_func = null;
            DbSet<T> lookup_source = null;
            T lookup_value = null;

            if (typeof(T).Name == "Person")
            {
                if ((entity as Person).FirstName != null)
                    lookup_func = (o => Equals((o as Person).FirstName, (entity as Person).FirstName)
                            && Equals((o as Person).LastName, (entity as Person).LastName)
                            && Equals((o as Person).MiddleName, (entity as Person).MiddleName));
                else
                    lookup_func = (o => Equals(o.ID, entity.ID));
                lookup_source = Persons as DbSet<T>;                
            }
            else
            {
                if ((entity.Name != null))
                    lookup_func = (o => Equals(o.Name, entity.Name));
                else
                    lookup_func = (o => Equals(o.ID, entity.ID));
                switch (typeof(T).Name)
                {
                    case "Actor":
                        if ((entity as Actor).PersonID != 0 && (entity as Actor).MovieID != 0) // not an Actor object for a movie object just created
                            lookup_func = (w => Equals((w as Actor).PersonID, (entity as Actor).PersonID) && Equals((w as Actor).MovieID, (entity as Actor).MovieID));
                        else
                            goto SkipLookup;
                        lookup_source = Actors as DbSet<T>;
                        break;
                    case "Genre":
                        lookup_source = Genres as DbSet<T>;
                        break;
                    case "GenreGroup":
                        lookup_source = GenreGroups as DbSet<T>;
                        break;
                    case "Writer":
                        if ((entity as Writer).PersonID != 0 && (entity as Writer).MovieID != 0) // not a Writer object for a movie object just created
                            lookup_func = (w => Equals((w as Writer).PersonID, (entity as Writer).PersonID) && Equals((w as Writer).MovieID, (entity as Writer).MovieID));
                        else
                            goto SkipLookup;
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
        SkipLookup:
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