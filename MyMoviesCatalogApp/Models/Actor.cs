using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyMoviesCatalogApp.Models
{    
    public class Actor : MovieCatalogEntity
    {
        [DisplayName("Actor")]
        [ForeignKey("Person")]
        public int PersonID { get; set; }
        [DisplayName("Movie")]
        [ForeignKey("Movie")]
        public int MovieID { get; set; }
                
        public virtual Person Person { get; set; }
        public virtual Movie Movie { get; set; }

        [NotMapped]
        public virtual string FullName { get { return Person.FullName; } }
        [NotMapped]
        public override string Name { get { return Person.FullName; } }
    }
}