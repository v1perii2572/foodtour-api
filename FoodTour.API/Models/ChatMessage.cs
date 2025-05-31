using System;
using System.Collections.Generic;

namespace FoodTour.API.Models;

public partial class ChatMessage
{
    public int Id { get; set; }

    public Guid SessionId { get; set; }

    public string? Role { get; set; }

    public string? Message { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual ChatSession Session { get; set; } = null!;
}
