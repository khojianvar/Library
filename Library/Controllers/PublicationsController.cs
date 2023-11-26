using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.DAL;
using Library.Models;
using Microsoft.Data.SqlClient;

namespace Library.Controllers
{
    public class PublicationsController : Controller
    {
        private readonly LibraryDbContext _context;

        public PublicationsController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Publications
        public async Task<IActionResult> Index(string sortOrder, string? searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["PublishedDateSort"] = sortOrder == "publishedDate_asc" ? "publishedDate_desc" : "publishedDate_asc";
            ViewData["AuthorSort"] = sortOrder == "author_asc" ? "author_desc" : "author_asc";
            ViewData["BookSort"] = sortOrder == "book_asc" ? "book_desc" : "book_asc";

            var publications = _context.Publications.AsQueryable();

            if (searchString != null)
            {
                publications = publications
                .Where(s => s.Author.FirstName.ToLower().Contains(searchString.ToLower()))
                .AsQueryable();
            }

            publications = sortOrder switch
            {
                "publishedDate_asc" => publications.OrderBy(x => x.PublishedDate),
                "publishedDate_desc" => publications.OrderByDescending(x => x.PublishedDate),
                "author_asc" => publications.OrderBy(x => x.Author.FirstName),
                "author_desc" => publications.OrderByDescending(x => x.Author.FirstName),
                "book_asc" => publications.OrderBy(x => x.Book.Title),
                "book_desc" => publications.OrderByDescending(x => x.Book.Title),
                _ => publications.OrderBy(x => x.Author.FirstName)
            };

            var libraryDbContext = publications.Include(p => p.Author).Include(p => p.Book);

            return View(await libraryDbContext.ToListAsync());
        }

        // GET: Publications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Publications == null)
            {
                return NotFound();
            }

            var publication = await _context.Publications
                .Include(p => p.Author)
                .Include(p => p.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publication == null)
            {
                return NotFound();
            }

            return View(publication);
        }

        // GET: Publications/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName");
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title");
            return View();
        }

        // POST: Publications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PublishedDate,AuthorId,BookId")] Publication publication)
        {
            if (publication != null)
            {
                _context.Publications.Add(publication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName", publication.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", publication.BookId);
            return View(publication);
        }

        // GET: Publications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Publications == null)
            {
                return NotFound();
            }

            var publication = await _context.Publications.FindAsync(id);
            if (publication == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName", publication.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", publication.BookId);
            return View(publication);
        }

        // POST: Publications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PublishedDate,AuthorId,BookId")] Publication publication)
        {
            if (id != publication.Id)
            {
                return NotFound();
            }

            if (publication != null)
            {
                try
                {
                    _context.Update(publication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublicationExists(publication.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "FirstName", publication.AuthorId);
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", publication.BookId);
            return View(publication);
        }

        // GET: Publications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Publications == null)
            {
                return NotFound();
            }

            var publication = await _context.Publications
                .Include(p => p.Author)
                .Include(p => p.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publication == null)
            {
                return NotFound();
            }

            return View(publication);
        }

        // POST: Publications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Publications == null)
            {
                return Problem("Entity set 'LibraryDbContext.Publications'  is null.");
            }
            var publication = await _context.Publications.FindAsync(id);
            if (publication != null)
            {
                _context.Publications.Remove(publication);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PublicationExists(int id)
        {
          return (_context.Publications?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
