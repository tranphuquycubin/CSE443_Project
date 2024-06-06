

using Microsoft.EntityFrameworkCore;

namespace EbookApp.Respository
{
    public class HomeRespository:IHomeRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public HomeRespository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }
        public async Task<IEnumerable<Genre>> Genres()
        {
            return await _dbContext.Genres.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0)
        {
            sTerm =sTerm.ToLower(); 
            IEnumerable<Book> booksQuery =await (from book in _dbContext.Books
                              join genre in _dbContext.Genres
                              on book.GenreId equals genre.Id
                              join stock in _dbContext.Stocks
                              on book.Id equals stock.BookId
                              into book_stocks
                              from bookWithStock in book_stocks.DefaultIfEmpty()
                              where String.IsNullOrWhiteSpace(sTerm) ||(book!=null && book.BookName.ToLower().StartsWith(sTerm))
                              select new Book
                              {
                                  Id = book.Id,
                                  Image = book.Image,
                                  AuthorName = book.AuthorName,
                                  BookName = book.BookName,
                                  GenreId = book.GenreId,
                                  Price = book.Price,
                                  GenreName = genre.GenreName,
                                  Quantity=bookWithStock==null ? 0 : bookWithStock.Quantity
                              }
                        ).ToListAsync();
            if(genreId>0)
            {
                booksQuery = booksQuery.Where(a => a.GenreId == genreId).ToList();
            }
            return booksQuery;

            

        }
    }
}
