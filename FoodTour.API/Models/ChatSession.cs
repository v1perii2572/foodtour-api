using System;
using System.Collections.Generic;

namespace FoodTour.API.Models;

public partial class ChatSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime? StartedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<SavedRoute> SavedRoutes { get; set; } = new List<SavedRoute>();

    public virtual User User { get; set; } = null!;
}
