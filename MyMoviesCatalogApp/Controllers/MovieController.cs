﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MyMoviesCatalogApp.DAL;
using MyMoviesCatalogApp.Models;
using PagedList;

namespace MyMoviesCatalogApp.Controllers
{
    public class MovieController : Controller
    {
        private MoviesCatalogContext db = new MoviesCatalogContext();


        // GET: MovieInfo
        //public ActionResult Index()
        //{
        //    var moves = db.Movies.Include(m => m.Director).Include(m => m.CreatedUser).Include(m => m.LastUpdatedUser);
        //    return View(moves.ToList());
        //}

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
            ViewBag.CurrentController = "Movie";

            var movies = from s in db.Movies
                         select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s =>
                    s.OriginalName.Contains(searchString)
                    || s.Description.Contains(searchString)
                    || s.Actors.Any(a => (a.Person != null && (a.Person.FirstName + (a.Person.MiddleName != null ? " " + a.Person.MiddleName : "") + " " + a.Person.LastName).Contains(searchString)))
                    || s.Writers.Any(w => (w.Person != null && (w.Person.FirstName + (w.Person.MiddleName != null ? " " + w.Person.MiddleName : "")+ " " + w.Person.LastName).Contains(searchString)))
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

            int pageSize = 9;
            int pageNumber = (page ?? 1);
            return View(movies.ToPagedList(pageNumber, pageSize));
        }

        // GET: MovieInfo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movieInfo = db.Movies.Where(o => o.ID == id).Include(m => m.CreatedUser).Include(m => m.LastUpdatedUser).FirstOrDefault();
            if (movieInfo == null)
            {
                return HttpNotFound();
            }
            db.Entry(movieInfo).Reload();           

            ViewBag.GenresList = new MultiSelectList(db.Genres.OrderBy(g => g.GroupID).ThenBy(g => g.Name).ToList(), "ID", "Name", "Group.Name", selectedValues: movieInfo.Genres.Select(s => s.ID.ToString()).ToArray());
            ViewBag.ActorsList = new MultiSelectList(db.GetActorCandidates(movieInfo), "ID", "FullName", movieInfo.Actors.Select(a => a.PersonID.ToString()).ToArray());
            ViewBag.WritersList = new MultiSelectList(db.GetWriterCandidates(movieInfo), "ID", "FullName", movieInfo.Writers.Select(a => a.PersonID.ToString()).ToArray());
            //ViewBag.ActorsList = new MultiSelectList(movieInfo.Actors.Select(a => new { Person = a.Person, IsActor = db.Actors.Where(ap => ap.PersonID == a.PersonID).Count() }).Union(db.Persons.Select(p => new { Person = p, IsActor = db.Actors.Where(a => a.PersonID == p.ID).Count() })).OrderByDescending(p => p.IsActor).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName", movieInfo.Actors.Select(a => a.PersonID.ToString()).ToArray());
            //ViewBag.WritersList = new MultiSelectList(movieInfo.Writers.Select(w => new { Person = w.Person, IsWriter = db.Writers.Where(wp => wp.PersonID == w.PersonID).Count() }).Union(db.Persons.Select(p => new { Person = p, IsWriter = db.Writers.Where(w => w.PersonID == p.ID).Count() })).OrderByDescending(p => p.IsWriter).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName", selectedValues: movieInfo.Writers.Select(w => w.PersonID.ToString()).ToArray());
            if (movieInfo.Director != null)
                ViewBag.PersonsList = new SelectList(new HashSet<Person> { movieInfo.Director }.Union(db.Persons.OrderBy(o => o.LastName)), "ID", "FullName", movieInfo.DirectorID);
            else
                ViewBag.PersonsList = new SelectList(db.Persons.OrderBy(o => o.LastName), "ID", "FullName");
            return View(movieInfo);
        }

        [Authorize()]
        // GET: MovieInfo/Create
        public ActionResult Create()
        {            
            ViewBag.GenresList = new MultiSelectList(db.Genres.OrderBy(g => g.GroupID).ThenBy(g => g.Name).ToList(), "ID", "Name", "Group.Name");
            ViewBag.ActorsList = new MultiSelectList(db.GetActorCandidates(), "ID", "FullName");
            ViewBag.WritersList = new MultiSelectList(db.GetWriterCandidates(), "ID", "FullName");
            //ViewBag.ActorsList = new MultiSelectList(db.Persons.Select(p => new { Person = p, IsActor = db.Actors.Where(w => w.PersonID == p.ID).Count() }).OrderByDescending(p => p.IsActor).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName");
            //ViewBag.WritersList = new MultiSelectList(db.Persons.Select(p => new { Person = p, IsWriter = db.Writers.Where(w => w.PersonID == p.ID).Count() }).OrderByDescending(p => p.IsWriter).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName");
            ViewBag.PersonsList = new SelectList(db.Persons.Select(p => new { Person = p, IsDirector = db.Movies.Where(w => w.DirectorID == p.ID).Count() }).OrderByDescending(p => p.IsDirector).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName");
            return View();
        }

        // POST: MovieInfo/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize()]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OriginalName,Year,Description,DirectorID,_FileData,_GenreIDs,_WriterIDs,_ActorIDs")] Movie movieInfo)
        {
            var supportedContentTypes = new string[] { "image/gif", "image/jpeg", "image/pjpeg", "image/png" };

            if (movieInfo._FileData != null
                   && (movieInfo._FileData.ContentLength < 1 || movieInfo._FileData.ContentLength > 1024 * 100
                        || !supportedContentTypes.Contains(movieInfo._FileData.ContentType)))
                ModelState.AddModelError("ImageUpload", "Photo format is unsupported or greater than allowed size of 100 Kb.");

            if (ModelState.IsValid)
            {
                if (movieInfo._WriterIDs != null)
                    movieInfo.Writers = movieInfo._WriterIDs.Select(writerID => db.GetOrAdd(new Writer { PersonID = writerID, Movie = movieInfo })).ToList();
                if (movieInfo._ActorIDs != null)
                    movieInfo.Actors = movieInfo._ActorIDs.Select(actorID => db.GetOrAdd(new Actor { PersonID = actorID, Movie = movieInfo })).ToList();
                if (movieInfo._GenreIDs != null)
                {
                    foreach (var genre in movieInfo._GenreIDs.Select(genreID => db.GetOrAdd(new Genre { ID = genreID })))
                        movieInfo.Genres.Add(genre);
                }

                db.Entry(movieInfo).State = EntityState.Added;

                if (movieInfo._FileData != null)
                {
                    movieInfo.Photo = new byte[movieInfo._FileData.ContentLength];
                    movieInfo.PhotoContentType = movieInfo._FileData.ContentType;
                    movieInfo._FileData.InputStream.Read(movieInfo.Photo, 0, movieInfo._FileData.ContentLength);
                    var base64 = Convert.ToBase64String(movieInfo.Photo);
                }
                else
                    db.Entry(movieInfo).Property(x => x.Photo).IsModified = false;

                movieInfo.CreatedUserID = User.Identity.GetUserId();
                db.Movies.Add(movieInfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DirectorID = new SelectList(db.Persons, "ID", "FullName", movieInfo.DirectorID);
            return View(movieInfo);
        }

        [Authorize()]
        // GET: MovieInfo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movieInfo = db.Movies.Where(o => o.ID == id).Include(m => m.CreatedUser).Include(m => m.LastUpdatedUser).FirstOrDefault();
            if (movieInfo == null)
            {
                return HttpNotFound();
            }
            db.Entry(movieInfo).Reload();
            ViewBag.GenresList = new MultiSelectList(db.Genres.OrderBy(g => g.GroupID).ThenBy(g => g.Name).ToList(), "ID", "Name", "Group.Name", selectedValues: movieInfo.Genres.Select(s => s.ID.ToString()).ToArray());
            ViewBag.ActorsList = new MultiSelectList(db.GetActorCandidates(movieInfo), "ID", "FullName", movieInfo.Actors.Select(a => a.PersonID.ToString()).ToArray());
            ViewBag.WritersList = new MultiSelectList(db.GetWriterCandidates(movieInfo), "ID", "FullName", movieInfo.Writers.Select(a => a.PersonID.ToString()).ToArray());
            //ViewBag.ActorsList = new MultiSelectList(movieInfo.Actors.Select(a => new { Person = a.Person, IsActor = db.Actors.Where(ap => ap.PersonID == a.PersonID).Count() }).Union(db.Persons.Select(p => new { Person = p, IsActor = db.Actors.Where(a => a.PersonID == p.ID).Count() })).OrderByDescending(p => p.IsActor).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName", movieInfo.Actors.Select(a => a.PersonID.ToString()).ToArray());
            //ViewBag.WritersList = new MultiSelectList(movieInfo.Writers.Select(w => new { Person = w.Person, IsWriter = db.Writers.Where(wp => wp.PersonID == w.PersonID).Count() }).Union(db.Persons.Select(p => new { Person = p, IsWriter = db.Writers.Where(w => w.PersonID == p.ID).Count() })).OrderByDescending(p => p.IsWriter).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName", selectedValues: movieInfo.Writers.Select(w => w.PersonID.ToString()).ToArray());
            if (movieInfo.Director != null)
                ViewBag.PersonsList = new SelectList(new HashSet<Person> { movieInfo.Director }.Union(db.Persons.OrderBy(o => o.LastName)), "ID", "FullName", movieInfo.DirectorID);
            else
                ViewBag.PersonsList = new SelectList(db.Persons.OrderBy(o => o.LastName), "ID", "FullName");

            return View(movieInfo);
        }

        [Authorize()]
        // POST: MovieInfo/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,OriginalName,Year,Description,DirectorID,_FileData,_GenreIDs,_ActorIDs,_WriterIDs")] Movie movieInfo)
        {
            var supportedContentTypes = new string[] { "image/gif", "image/jpeg", "image/pjpeg", "image/png" };

            if (movieInfo._FileData != null
                   && (movieInfo._FileData.ContentLength < 1 || movieInfo._FileData.ContentLength > 1024 * 100
                    || !supportedContentTypes.Contains(movieInfo._FileData.ContentType)))
                ModelState.AddModelError("ImageUpload", "Photo format is unsupported or greater than allowed size of 100 Kb.");

            if (ModelState.IsValid)
            {

                var entry = db.Entry(movieInfo);
                entry.State = EntityState.Modified;

                if (movieInfo._WriterIDs != null)
                    movieInfo.Writers = movieInfo._WriterIDs.Select(writerID => db.GetOrAdd(new Writer { PersonID = writerID, MovieID = movieInfo.ID })).ToList();
                if (movieInfo._ActorIDs != null)
                    movieInfo.Actors = movieInfo._ActorIDs.Select(actorID => db.GetOrAdd(new Actor { PersonID = actorID, MovieID = movieInfo.ID })).ToList();
                if (movieInfo._GenreIDs != null)
                {
                    entry.Collection(g => g.Genres).Load();

                    movieInfo.Genres.Clear();

                    foreach (var genre in movieInfo._GenreIDs.Select(genreID => db.GetOrAdd(new Genre { ID = genreID })))
                        movieInfo.Genres.Add(genre);
                    //movieInfo.Genres = movieInfo._GenreIDs.Select(genreID => db.GetOrAdd(new Genre { ID = genreID })).ToList();
                }
                

                if (movieInfo._FileData != null)
                {
                    movieInfo.Photo = new byte[movieInfo._FileData.ContentLength];
                    movieInfo.PhotoContentType = movieInfo._FileData.ContentType;
                    movieInfo._FileData.InputStream.Read(movieInfo.Photo, 0, movieInfo._FileData.ContentLength);
                    var base64 = Convert.ToBase64String(movieInfo.Photo);
                }
                else
                    db.Entry(movieInfo).Property(x => x.Photo).IsModified = false;
                
                movieInfo.LastUpdatedUserID = User.Identity.GetUserId();
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DirectorID = new SelectList(db.Persons, "ID", "FullName", movieInfo.DirectorID);
            return View(movieInfo);
        }

        [Authorize()]
        // GET: MovieInfo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movie movieInfo = db.Movies.Where(o => o.ID == id).Include(m => m.CreatedUser).Include(m => m.LastUpdatedUser).FirstOrDefault();
            if (movieInfo == null)
            {
                return HttpNotFound();
            }
            db.Entry(movieInfo).Reload();
            ViewBag.GenresList = new MultiSelectList(db.Genres.OrderBy(g => g.GroupID).ThenBy(g => g.Name).ToList(), "ID", "Name", "Group.Name", selectedValues: movieInfo.Genres.Select(s => s.ID.ToString()).ToArray());
            ViewBag.ActorsList = new MultiSelectList(db.GetActorCandidates(movieInfo), "ID", "FullName", movieInfo.Actors.Select(a => a.PersonID.ToString()).ToArray());
            ViewBag.WritersList = new MultiSelectList(db.GetWriterCandidates(movieInfo), "ID", "FullName", movieInfo.Writers.Select(a => a.PersonID.ToString()).ToArray());
            //ViewBag.ActorsList = new MultiSelectList(movieInfo.Actors.Select(a => new { Person = a.Person, IsActor = db.Actors.Where(ap => ap.PersonID == a.PersonID).Count() }).Union(db.Persons.Select(p => new { Person = p, IsActor = db.Actors.Where(a => a.PersonID == p.ID).Count() })).OrderByDescending(p => p.IsActor).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName", movieInfo.Actors.Select(a => a.PersonID.ToString()).ToArray());
            //ViewBag.WritersList = new MultiSelectList(movieInfo.Writers.Select(w => new { Person = w.Person, IsWriter = db.Writers.Where(wp => wp.PersonID == w.PersonID).Count() }).Union(db.Persons.Select(p => new { Person = p, IsWriter = db.Writers.Where(w => w.PersonID == p.ID).Count() })).OrderByDescending(p => p.IsWriter).ThenBy(p => p.Person.LastName).ToList(), "Person.ID", "Person.FullName", selectedValues: movieInfo.Writers.Select(w => w.PersonID.ToString()).ToArray());
            if (movieInfo.Director != null)
                ViewBag.PersonsList = new SelectList(new HashSet<Person> { movieInfo.Director }.Union(db.Persons.OrderBy(o => o.LastName)), "ID", "FullName", movieInfo.DirectorID);
            else
                ViewBag.PersonsList = new SelectList(db.Persons.OrderBy(o => o.LastName), "ID", "FullName");
            return View(movieInfo);
        }

        [Authorize()]
        // POST: MovieInfo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movie movieInfo = db.Movies.Find(id);
            db.Movies.Remove(movieInfo);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
