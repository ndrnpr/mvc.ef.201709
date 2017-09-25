using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyMoviesCatalogApp.Models
{
    public class Writer : MovieCatalogEntity
    {
        [DisplayName("Writer")]
        [ForeignKey("Person")]
        public int PersonID { get; set; }
        [DisplayName("Movie")]
        [ForeignKey("Movie")]
        public int MovieID { get; set; }

        public virtual Person Person { get; set; }
        public virtual Movie Movie { get; set; }

        [NotMapped]
        public virtual string FullName
        {
            get
            {
                return Name;
            }
        }
        [NotMapped]
        public override string Name {
            get
            {
                string result;
                try
                {
                    result = (Person != null) ? Person.FullName : base.Name;
                }
                catch
                {
                    result = base.Name;
                }
                return result;
            }
        }
    }
}