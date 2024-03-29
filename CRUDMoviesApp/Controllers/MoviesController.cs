using CRUDMoviesApp.Models;
using CRUDMoviesApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace CRUDMoviesApp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private List<string> _allowedExtensions = new List<string> { ".jpg",".png" };
        private long _MaxAllowedPosterSize = 1048576;


        public MoviesController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(a => a.Rate).ToListAsync();
            return View(movies);
        }
        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync()
            };

            return View("MovieForm",viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var files = Request.Form.Files;

                if (!files.Any())
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Please select movie poster!");
                    return View("MovieForm",model);
                }

                var poster = files.FirstOrDefault();

                if (!_allowedExtensions.Contains(Path.GetExtension(poster!.FileName).ToLower()))
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Please select .jpg or .png img");
                    return View("MovieForm",model);

                }
                if(poster.Length > _MaxAllowedPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB");
                    return View("MovieForm",model);
                }

                using var dataStream = new MemoryStream();

                await poster.CopyToAsync(dataStream);

                var movies = new Movie
                {
                    Title = model.Title,
                    GenreId = model.GenreId,
                    Year = model.Year,
                    Rate = model.Rate,
                    Storyline = model.Storyline,
                    Poster = dataStream.ToArray()
                };
                _context.Movies.Add(movies);
                _context.SaveChanges();
                _toastNotification.AddSuccessToastMessage("Movie Created Successfully");
                return RedirectToAction(nameof(Index));
            }
            return View("MovieForm",model);
            
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            var viewModel = new MovieFormViewModel
            {
                Id= movie.Id,
                Title = movie.Title,
                Rate= movie.Rate,
                Year = movie.Year,
                GenreId = movie.GenreId,
                Poster = movie.Poster,
                Storyline = movie.Storyline,
                Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync()
            };

            return View("MovieForm", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("MovieForm", model);
            }

            var movie = await _context.Movies.FindAsync(model.Id);

            if (movie == null)
                return NotFound();

            movie.Title = model.Title;
            movie.GenreId = model.GenreId;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.Storyline = model.Storyline;

            var files = Request.Form.Files;

            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var dataStream = new MemoryStream();
                await poster!.CopyToAsync(dataStream);
                model.Poster = dataStream.ToArray();
                if (!_allowedExtensions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Please select .jpg or .png img");
                    return View("MovieForm", model);
                }
                if (poster.Length > _MaxAllowedPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB");
                    return View("MovieForm", model);
                }
                movie.Poster = dataStream.ToArray();
            }
            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Movie Updated Successfully");
            return RedirectToAction(nameof(Index));

        }
        
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null) 
                return BadRequest();

            var movie = await _context.Movies.Include(c => c.Genre).SingleOrDefaultAsync( m => m.Id == id);

            if(movie == null) 
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }
    }
}
