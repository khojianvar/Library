using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    [Table(nameof(Publication))]
    public class Publication
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PublishedDate { get; set; }

        public int AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; }

        public int BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; }
    }
}
