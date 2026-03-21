using System;
using System.Collections.Generic;

namespace GHCAA.Application.DTOs
{
    public class EventTaskDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? AssignedMemberId { get; set; }
        public string? AssignedMemberName { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class EventBudgetDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public decimal EstimatedTotal { get; set; }
        public decimal ActualTotal { get; set; }
        public List<EventExpenseDto> Expenses { get; set; } = new();
    }

    public class EventExpenseDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTime SpentAt { get; set; }
    }

    public class CreateEventTaskDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? AssignedMemberId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateEventBudgetDto
    {
        public int EventId { get; set; }
        public decimal EstimatedTotal { get; set; }
    }

    public class AddEventExpenseDto
    {
        public int EventId { get; set; }
        public string Category { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTime SpentAt { get; set; }
    }
}
