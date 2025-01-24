using System;
using System.Collections.Generic;

namespace RealState.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Price { get; set; }
        public string VideoUrl { get; set; }

        public DateTime? BiddingStartTime { get; set; }
        public DateTime? BiddingEndTime { get; set; }

        // Initialize Bids to an empty list
        public List<Bid> Bids { get; set; } = new List<Bid>();
    }
}
