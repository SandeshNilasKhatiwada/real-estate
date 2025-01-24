using System;

namespace RealState.Models
{
    public class Bid
    {
        public int Id { get; set; }

        public int PropertyId { get; set; } // Foreign key to Property
        public Property Property { get; set; } // Navigation property

        public string BidderName { get; set; } // Name of the bidder
        public decimal Amount { get; set; } // Bid amount
        public DateTime TimePlaced { get; set; } // Time the bid was placed
    }
}
