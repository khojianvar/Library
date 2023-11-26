using Bogus;
using Library.DAL;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Extensions
{
    public static class DatabaseSeeder
    {
        private static Faker _faker = new Faker();

        public static void SeedDatabase(this IServiceCollection _, IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<LibraryDbContext>>();
            using var context = new LibraryDbContext(options);

            CreateCategories(context);
            CreateAuthors(context);
            CreateBooks(context);
            CreatePublications(context);
        }

        private static void CreateCategories(LibraryDbContext context)
        {
            if (context.Categories.Any()) return;

            List<Category> categories = new();

            for (int i = 0; i < 15; i++)
            {
                categories.Add(new Category
                {
                    Name = _faker.Name.JobArea()
                });
            }

            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        private static void CreateAuthors(LibraryDbContext context)
        {
            if (context.Authors.Any()) return;
            List<Author> authors = new List<Author>();

            for (int i = 0; i < 10; i++)
            {
                authors.Add(new Author()
                {
                    FirstName = _faker.Name.FirstName(),
                    LastName = _faker.Name.LastName(),
                    Age = _faker.Random.Int(20, 40),
                    Email = _faker.Person.Email,
                    PhoneNumber = _faker.Phone.PhoneNumber("+998-(##) ###-##-##")
                }) ;
            }

            context.Authors.AddRange(authors);
            context.SaveChanges();
        }

        private static void CreateBooks(LibraryDbContext context)
        {
            if (context.Books.Any()) return;

            var categories = context.Categories.ToList();

            List<Book> books = new List<Book>();

            foreach (var category in categories)
            {
                int booksCount = new Random().Next(1, 5);

                for (int i = 0; i < booksCount; i++)
                {
                    books.Add(new Book()
                    {
                        CategoryId = category.Id,
                        Title = _faker.Lorem.Word(),
                        Description = _faker.Lorem.Sentence(),
                        Price = _faker.Random.Decimal(1000, 10000),
                    });
                }
            }

            context.Books.AddRange(books);
            context.SaveChanges();
        }

        private static void CreatePublications(LibraryDbContext context)
        {
            if (context.Publications.Any()) return;

            var books = context.Books.ToList();
            var authors = context.Authors.ToList();
            List<Publication> publications = new List<Publication>();

            foreach (var book in books)
            {
                var randomAuthor = _faker.PickRandom(authors);

                publications.Add(new Publication()
                {
                    AuthorId = randomAuthor.Id,
                    BookId = book.Id,
                    PublishedDate = _faker.Date.Between(DateTime.Now.AddYears(-18), DateTime.Now),
                });
            }

            context.Publications.AddRange(publications);
            context.SaveChanges();
        }
    }
}
