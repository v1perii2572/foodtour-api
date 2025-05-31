using System;
using System.Collections.Generic;

namespace FoodTour.API.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Role { get; set; } = "Free";

    public DateTime? SubscriptionDate { get; set; }

    public string? Name { get; set; }

    public string? DislikedFoods { get; set; }

    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<SavedRoute> SavedRoutes { get; set; } = new List<SavedRoute>();
}
