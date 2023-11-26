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
using Library.ViewModel;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryDbContext _context;

        public BooksController(LibraryDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSort"] = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewData["DescriptionSort"] = sortOrder == "description_asc" ? "description_desc" : "description_asc";
            ViewData["PriceSort"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
            ViewData["CategorySort"] = sortOrder == "category_asc" ? "category_desc" : "category_asc";

            var books = _context.Books.AsQueryable();

            books = sortOrder switch
            {
                "title_asc" => books.OrderBy(x => x.Title),
                "title_desc" => books.OrderByDescending(x => x.Title),
                "description_asc" => books.OrderBy(x => x.Description),
                "description_desc" => books.OrderByDescending(x => x.Description),
                "price_asc" => books.OrderBy(x => x.Price),
                "price_desc" => books.OrderByDescending(x => x.Price),
                "category_asc" => books.OrderBy(x => x.Category.Name),
                "category_desc" => books.OrderByDescending(x => x.Category.Name),
                _ => books.OrderBy(x => x.Title)
            };
            var libraryDbContext = books.Include(b => b.Category);

            var categories = await _context.Categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString(),
            }).ToListAsync();

            var booksViewModel = new BooksViewModel()
            {
                Books = await libraryDbContext.ToListAsync(),
                Categories = categories
            };

            return View(booksViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string? searchString, string category)
        {
            if(searchString != null && category.Count() == 1)
            {
                int categoryId = int.Parse(category);

                var books = await _context.Books
                    .Include(b => b.Category)
                    .Where(b => b.Title.ToLower().Contains(searchString.ToLower()))
                    .Where(b => b.CategoryId == categoryId)
                    .ToListAsync();

                var categories = await _context.Categories.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToListAsync();

                var bookVM = new BooksViewModel()
                {
                    Books = books,
                    Categories = categories
                };

                return View(bookVM);
            }
            else if (searchString == null && category.Count() == 1)
            {
                int categoryId = int.Parse(category);

                var books = await _context.Books
                    .Include(b => b.Category)
                    .Where(b => b.CategoryId == categoryId)
                    .ToListAsync();

                var categories = await _context.Categories.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToListAsync();

                var bookVM = new BooksViewModel()
                {
                    Books = books,
                    Categories = categories
                };

                return View(bookVM);
            }
            else if (searchString != null && category.Count() > 1)
            {
                var books = await _context.Books
                    .Include(b => b.Category)
                    .Where(b => b.Title.ToLower().Contains(searchString.ToLower()))
                    .ToListAsync();

                var categories = await _context.Categories.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToListAsync();

                var bookVM = new BooksViewModel()
                {
                    Books = books,
                    Categories = categories
                };

                return View(bookVM);
            }
            else
            {
                return View(_context.Books.ToList());
            }
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,CategoryId")] Book book)
        {
            if (book != null)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,CategoryId")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (book != null)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'LibraryDbContext.Books'  is null.");
            }
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
          return (_context.Books?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
