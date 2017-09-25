using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyMoviesCatalogApp.Models
{
    public class Genre : MovieCatalogEntity
    {
        //public string Name { get; set; }
        [ForeignKey("Group")]
        public int GroupID { get; set; }

        public virtual ICollection<Movie> Movies { get; set; }
        public virtual GenreGroup Group { get; set; }

        public Genre()
        {
            Movies = new HashSet<Movie>();
        }

    }

    public class GenreGroup : MovieCatalogEntity
    {
        //public string Name { get; set; }

        public virtual ICollection<Genre> Genres { get; set; }

        public GenreGroup()
        {
            Genres = new HashSet<Genre>();
        }

    }
}