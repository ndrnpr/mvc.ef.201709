using MyMoviesCatalogApp.DAL;
using MyMoviesCatalogApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace MyMoviesCatalogApp.Controllers
{    
    public class HomeController : Controller
    {
        private MoviesCatalogContext db = new MoviesCatalogContext();

        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            //ViewBag.DirectorSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentController = "Home";

            var movies = from s in db.Movies select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s =>
                    s.OriginalName.Contains(searchString)
                    || s.Description.Contains(searchString)
                    || s.Actors.Any(a => (a.Person != null && (a.Person.FirstName + (a.Person.MiddleName != null ? " " + a.Person.MiddleName : "") + " " + a.Person.LastName).Contains(searchString)))
                    || s.Writers.Any(w => (w.Person != null && (w.Person.FirstName + (w.Person.MiddleName != null ? " " + w.Person.MiddleName : "") + " " + w.Person.LastName).Contains(searchString)))
                    || (s.Director != null && (s.Director.FirstName + (s.Director.MiddleName != null ? " " + s.Director.MiddleName : "") + " " + s.Director.LastName).Contains(searchString))
                    || s.Genres.Any(g => g.Name.Contains(searchString)));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    movies = movies.OrderByDescending(s => s.OriginalName).ThenBy(s => s.ID);
                    break;
                case "Year":
                    movies = movies.OrderBy(s => s.Year).ThenBy(s => s.ID);
                    break;
                case "year_desc":
                    movies = movies.OrderByDescending(s => s.Year).ThenBy(s => s.ID);
                    break;
                default:   
                    movies = movies.OrderByDescending(s => s.Rating).ThenBy(s => s.ID);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(movies.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}