using System;
using System.Collections.Generic;

namespace FoodTour.API.Models;

public partial class SavedRoutePlace
{
    public int Id { get; set; }

    public Guid RouteId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public double? Lat { get; set; }

    public double? Lng { get; set; }

    public int? Sequence { get; set; }

    public string? TimeSlot { get; set; }

    public string? Role { get; set; }

    public string? Note { get; set; }

    public virtual SavedRoute Route { get; set; } = null!;
}
