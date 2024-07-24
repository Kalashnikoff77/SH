﻿namespace DataContext.Entities
{
    public class AccountsRelationsEntity : EntityBase
    {
        public DateTime CreateDate { get; set; }

        public int SenderId { get; set; }

        public int RecipientId { get; set; }

        public short Type { get; set; }

        public bool IsConfirmed { get; set; }
    }
}
