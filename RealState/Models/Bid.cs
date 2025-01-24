using System;

namespace RealState.Models
{
    public class Bid
    {
        public int Id { get; set; }

        public int PropertyId { get; set; } // Foreign key to Property
        public Property Property { get; set; } // Navigation property

        public string BidderId { get; set; } // ID of the buyer who placed the bid
        public string BidderName { get; set; } // Name of the bidder
        public decimal Amount { get; set; } // Bid amount
        public DateTime TimePlaced { get; set; } // Time the bid was placed

        public bool IsActive { get; set; } = true; // Bid status (default: active)

        public bool IsWinningBid { get; set; } = false; // Whether this bid is the highest and winning bid
    }
}
