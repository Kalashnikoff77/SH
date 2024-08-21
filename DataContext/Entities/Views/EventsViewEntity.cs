﻿using System.ComponentModel.DataAnnotations;

namespace DataContext.Entities.Views
{
    public class EventsViewEntity : EventsEntity
    {
        [Required]
        public DateTime NearestDate { get; set; }

        [Required]
        public string Admin { get; set; } = null!;

        [Required]
        public string Country { get; set; } = null!;

        [Required]
        public string Avatar { get; set; } = null!;

        public string? Photos { get; set; }

        [Required]
        public string? RegisteredAccounts { get; set; }

        [Required]
        public string? Schedule { get; set; }

        [Required]
        public int? NumOfDiscussions { get; set; }

        // Для AccountsEventsView
        public bool IsRegistered { get; set; }
    }
}
