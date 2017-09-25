using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyMoviesCatalogApp.Models
{
    public class Movie : MovieCatalogEntity
    {
        [DisplayName("Original movie name")]
        public string OriginalName { get; set; }
        [DisplayName("Movie release year")]
        public int Year { get; set; }
        [DisplayName("Director")]
        [ForeignKey("Director")]
        public int? DirectorID { get; set; }
        [DisplayName("IMDb Rating")]
        public int Rating { get; set; }

        public virtual Person Director { get; set; }
        public virtual ICollection<Writer> Writers { get; set; }
        public virtual ICollection<Actor> Actors { get; set; }
        public virtual ICollection<Genre> Genres { get; set; }

        [NotMapped]
        public int[] _GenreIDs { get; set; }
        [NotMapped]
        public int[] _ActorIDs { get; set; }
        [NotMapped]
        public int[] _WriterIDs { get; set; }
        [NotMapped]
        public HttpPostedFileBase _FileData { get; set; }

        public Movie()
        {
            Genres = new HashSet<Genre>();
            Actors = new HashSet<Actor>();
            Writers = new HashSet<Writer>();
        }
    }
}