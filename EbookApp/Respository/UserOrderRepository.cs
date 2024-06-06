using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EbookApp.Respository
{
    public class UserOrderRepository:IUserOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserOrderRepository(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        public async Task ChangeOrderStatus(UpdateOrderStatusModel data)
        {
            var order =await _dbContext.Orders.FindAsync(data.OrderId);
            if (order == null)
            {
                throw new InvalidOperationException($"order with id:{data.OrderId} does not found");

            }
            order.OrderStatusId = data.OrderStatusId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _dbContext.Orders.FindAsync(id);
        }

        public async Task<IEnumerable<OrderStatus>> GetOrderStatuses()
        {
            return await _dbContext.OrderStatuses.ToListAsync();
        }

        public async Task TogglePaymentStatus(int orderId)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"order with id:{orderId} does not found");

            }
            order.IsPaid = !order.IsPaid;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> UserOrders(bool getAll = false)
        {
            var orders = _dbContext.Orders
                        .Include(x => x.OrderStatus)
                        .Include(x => x.OrderDetails)
                        .ThenInclude(x => x.Book)
                        .ThenInclude(x => x.Genre).AsQueryable();
            if(!getAll)
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");
                orders=orders.Where(a =>a.UserId == userId);
                return await orders.ToListAsync();
            }
            return await orders.ToListAsync();

        }

        private string GetUserId()
        {
            var principal = _contextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
