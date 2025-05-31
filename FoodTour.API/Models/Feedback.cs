using System;
using System.Collections.Generic;

namespace FoodTour.API.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public Guid? SessionId { get; set; }

    public Guid? UserId { get; set; }

    public string? RelatedPlaceName { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ChatSession? Session { get; set; }

    public virtual User? User { get; set; }
}
