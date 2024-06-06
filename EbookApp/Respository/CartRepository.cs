using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EbookApp.Respository
{
    public class CartRepository:ICartReposttory
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public CartRepository(ApplicationDbContext dbContext,UserManager<IdentityUser>userManager, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }
        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged in");
                var cart = await GetCart(userId);
                if (cart == null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _dbContext.ShoppingCarts.Add(cart);
                }
                _dbContext.SaveChanges();
                //cart detail section
                var cartItem =_dbContext.CartDetails.FirstOrDefault(a=>a.ShoppingCartId==cart.Id&&a.BookId==bookId);
                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _dbContext.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };
                    _dbContext.CartDetails.Add(cartItem);

                }
                _dbContext.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
            }
            var cartItemCount=await GetCartItemCount(userId);
            return cartItemCount;
            
            

        }
        public async Task<int> RemoveItem(int bookId)
        {
            //using var transaction = _dbContext.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {

                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged in");
                var cart = await GetCart(userId);
                if (cart == null)
                {
                    throw new InvalidOperationException("Invalid cart");
                }
                //cart detail section
                var cartItem = _dbContext.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem == null)
                    throw new InvalidOperationException("No items in cart");

                else if (cartItem.Quantity==1)
                {
                    _dbContext.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;

                }
                _dbContext.SaveChanges();
                //transaction.Commit();
                
            }
            catch (Exception ex)
            {
               
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;


        }
        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new InvalidOperationException("Invalid user ID");
            var shoppingCart=await _dbContext.ShoppingCarts
                                 .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Book)
                                  .ThenInclude(a => a.Stock)
                                .Include(a =>a.CartDetails)
                                .ThenInclude(a => a.Book)
                                .ThenInclude(a=>a.Genre)
                                .Where(a=>a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
                                

                                
        }
        public async Task<int>GetCartItemCount(String userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId=GetUserId();
            }
            var data = await (from cart in _dbContext.ShoppingCarts
                              join cartDetail in _dbContext.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId==userId
                              select new { cartDetail.Id }).ToListAsync();
            return data.Count;
        }
        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged in");
                var cart = await GetCart(userId);
                if (cart == null)
                    throw new InvalidOperationException("Invalid cart");
                var cartDetail = _dbContext.CartDetails.
                    Where(a => a.ShoppingCartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new InvalidOperationException("Cart is empty");
                var pendingRecord = _dbContext.OrderStatuses.FirstOrDefault
                    (s => s.StatusName == "Pending");
                if (pendingRecord == null)
                    throw new InvalidOperationException("Order status does not have pending status");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name=model.Name,
                    Email=model.Email,
                    MobileNumber=model.MobileNumber,
                    PaymentMethod=model.PaymentMethod,
                    Address=model.Address,
                    IsPaid=false,
                    OrderStatusId = pendingRecord.Id
                };
                _dbContext.Orders.Add(order);
                _dbContext.SaveChanges();
                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _dbContext.OrderDetails.Add(orderDetail);
                    var stock = await _dbContext.Stocks.FirstOrDefaultAsync(a => a.BookId == item.BookId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null");
                    }

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} items(s) are available in the stock");
                    }
                    stock.Quantity -= item.Quantity;
                }
                //_dbContext.SaveChanges();
                _dbContext.CartDetails.RemoveRange(cartDetail);
                _dbContext.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task <ShoppingCart> GetCart(string userId)
        {
            var cart =await _dbContext.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }
        private String GetUserId()
        {
            var principal = _contextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
