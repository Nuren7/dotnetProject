using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanFlowAPI.Data;
using VanFlowAPI.Models;

namespace VanFlowAPI.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummary>>> GetOrders(CancellationToken cancellationToken)
    {
        var orders = await dbContext.VanOrders.AsNoTracking()
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new OrderSummary(order.Id, order.CustomerName, order.VehicleModel, order.BuildNumber, order.RequestedDeliveryDate, order.Status.ToString(), order.ErpStatus, order.CreatedAtUtc, order.LastSyncedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult<OrderSummary>> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.VehicleModel) || string.IsNullOrWhiteSpace(request.BuildNumber))
            return BadRequest("Customer, vehicle model, and build number are required.");
        if (request.RequestedDeliveryDate.Date < DateTime.UtcNow.Date)
            return BadRequest("Delivery date must be today or later.");

        var order = new VanOrder { CustomerName = request.CustomerName.Trim(), VehicleModel = request.VehicleModel.Trim(), BuildNumber = request.BuildNumber.Trim().ToUpperInvariant(), RequestedDeliveryDate = request.RequestedDeliveryDate.ToUniversalTime() };
        dbContext.VanOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, ToSummary(order));
    }

    private static OrderSummary ToSummary(VanOrder order) => new(order.Id, order.CustomerName, order.VehicleModel, order.BuildNumber, order.RequestedDeliveryDate, order.Status.ToString(), order.ErpStatus, order.CreatedAtUtc, order.LastSyncedAtUtc);
}

public record CreateOrderRequest(string CustomerName, string VehicleModel, string BuildNumber, DateTime RequestedDeliveryDate);
public record OrderSummary(int Id, string CustomerName, string VehicleModel, string BuildNumber, DateTime RequestedDeliveryDate, string Status, string ErpStatus, DateTime CreatedAtUtc, DateTime? LastSyncedAtUtc);
