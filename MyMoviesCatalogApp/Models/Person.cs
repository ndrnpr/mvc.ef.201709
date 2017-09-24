using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyMoviesCatalogApp.Models
{
    public class Person : MovieCatalogEntity
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        [NotMapped]
        public virtual string FullName { get { return Name; } internal set { Name = value; } }

        [NotMapped]
        public override string Name
        {
            get
            {
                if (MiddleName != null)
                    return string.Format("{0} {1} {2}", FirstName, MiddleName, LastName);
                else
                    return string.Format("{0} {1}", FirstName, LastName);
            }
            set
            {
                if (value != null)
                {
                    var parts = value.Split(' ');
                    FirstName = parts.First();
                    if (parts.Count() > 1)
                    {
                        LastName = parts.Last();
                        if (parts.Length > 2)
                            MiddleName = string.Join(" ", parts.Where((v, i) => i > 0 && i < parts.Length - 1));
                    }
                }
            }
        }
    }
}