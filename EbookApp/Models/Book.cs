using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EbookApp.Models
{
    [Table("Book")]
    public class Book
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string? BookName { get; set; }
        [Required]
        [MaxLength(50)]
        public string? AuthorName { get; set; }
        public long Price { get; set; }
        public String? Image {  get; set; }
        [Required]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public List<OrderDetail>OrderDetails { get; set; }
        public List<CartDetail> CartDetails { get; set; }
        public Stock Stock { get; set; }
        [NotMapped]
        public String GenreName { get; set; }
        [NotMapped]
        public int Quantity { get; set; }

    }
}
