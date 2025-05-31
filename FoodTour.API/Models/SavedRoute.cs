using System;
using System.Collections.Generic;

namespace FoodTour.API.Models;

public partial class SavedRoute
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? SavedAt { get; set; }

    public virtual ICollection<SavedRoutePlace> SavedRoutePlaces { get; set; } = new List<SavedRoutePlace>();

    public virtual ChatSession Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
