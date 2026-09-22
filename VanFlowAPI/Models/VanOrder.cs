namespace VanFlowAPI.Models;

public class VanOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public DateTime RequestedDeliveryDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Queued;
    public string ErpStatus { get; set; } = "Awaiting sync";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncedAtUtc { get; set; }
}

public enum OrderStatus { Queued, InProduction, Ready, Delivered }
