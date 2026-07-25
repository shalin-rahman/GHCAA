using System;
using System.Collections.Generic;

namespace GHCAA.Domain.Models
{
    public class EventBudget
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        public decimal EstimatedTotal { get; set; }
        public decimal ActualTotal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Detailed items
        public ICollection<EventExpense> Expenses { get; set; } = new List<EventExpense>();

        // Navigation
        public AlumniEvent? Event { get; set; }
    }

    public class EventExpense
    {
        public int Id { get; set; }
        public int EventBudgetId { get; set; }

        public string Category { get; set; } = null!; // e.g., Venue, Catering
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTime SpentAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public EventBudget? Budget { get; set; }
    }
}
