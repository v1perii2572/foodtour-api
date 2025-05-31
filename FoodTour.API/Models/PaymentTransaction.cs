using System;
using System.Collections.Generic;

namespace FoodTour.API.Models;

public partial class PaymentTransaction
{
    public int Id { get; set; }

    public string OrderId { get; set; } = null!;

    public string RequestId { get; set; } = null!;

    public int Amount { get; set; }

    public int ResultCode { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; }
}
