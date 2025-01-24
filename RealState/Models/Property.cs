using System;
using System.ComponentModel.DataAnnotations;

namespace RealState.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        public string VideoUrl { get; set; }

        [Required(ErrorMessage = "Bidding start time is required.")]
        public DateTime? BiddingStartTime { get; set; }

        [Required(ErrorMessage = "Bidding end time is required.")]
        public DateTime? BiddingEndTime { get; set; }
    }
}
