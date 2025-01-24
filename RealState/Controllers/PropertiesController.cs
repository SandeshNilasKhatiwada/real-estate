using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealState.Data;
using RealState.Models;
using System;
using System.Linq;

namespace RealState.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly RealStateContext _context;

        public PropertiesController(RealStateContext context)
        {
            _context = context;
        }

        // List all properties
        public IActionResult Index()
        {
            var properties = _context.Properties.ToList();
            return View(properties);
        }

        // Show details of a property
        public IActionResult Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var property = _context.Properties
                .Include(p => p.Bids) // Include related bids
                .FirstOrDefault(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }


        // Show create form
        public IActionResult Create()
        {
            // Pass an initialized Property object to the view
            var property = new Property();
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Property property)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
            {
                if (property.BiddingStartTime == null || property.BiddingEndTime == null)
                {
                    ModelState.AddModelError("", "Bidding start and end times are required.");
                    return View(property);
                }

                if (property.BiddingEndTime <= property.BiddingStartTime)
                {
                    ModelState.AddModelError("", "Bidding end time must be after the start time.");
                    return View(property);
                }

                try
                {
                    _context.Properties.Add(property);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating property: {ex.Message}");
                    ViewBag.ErrorMessage = "An error occurred while creating the property.";
                    return View("Error");
                }
            }

            return View(property);
        }

        // Show edit form
        public IActionResult Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var property = _context.Properties.Find(id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // Handle edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Property property)
        {
            if (id != property.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (property.BiddingStartTime == null || property.BiddingEndTime == null)
                {
                    ModelState.AddModelError("", "Bidding start and end times are required.");
                    return View(property);
                }

                if (property.BiddingEndTime <= property.BiddingStartTime)
                {
                    ModelState.AddModelError("", "Bidding end time must be after the start time.");
                    return View(property);
                }

                try
                {
                    _context.Update(property);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error editing property: {ex.Message}");
                    ViewBag.ErrorMessage = "An error occurred while editing the property.";
                    return View("Error");
                }
            }

            return View(property);
        }

        // Show delete confirmation
        public IActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var property = _context.Properties.FirstOrDefault(p => p.Id == id);
            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // Handle delete confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var property = _context.Properties.Find(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceBid(int propertyId, string bidderName, decimal amount)
        {
            var property = _context.Properties.FirstOrDefault(p => p.Id == propertyId);
            if (property == null)
            {
                return NotFound();
            }

            // Validate bidding time
            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is not open for this property.";
                return RedirectToAction("Details", new { id = propertyId });
            }

            // Save the bid
            var bid = new Bid
            {
                PropertyId = propertyId,
                BidderName = bidderName,
                Amount = amount,
                TimePlaced = DateTime.Now
            };

            _context.Bids.Add(bid);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = propertyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBid(int bidId, decimal newAmount)
        {
            var bid = _context.Bids.Include(b => b.Property).FirstOrDefault(b => b.Id == bidId);
            if (bid == null || !bid.IsActive)
            {
                return NotFound();
            }

            var property = bid.Property;

            // Validate bidding time
            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is closed for this property.";
                return RedirectToAction("Details", new { id = property.Id });
            }

            // Update the bid
            bid.Amount = newAmount;
            bid.TimePlaced = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = property.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelBid(int bidId)
        {
            var bid = _context.Bids.Include(b => b.Property).FirstOrDefault(b => b.Id == bidId);
            if (bid == null || !bid.IsActive)
            {
                return NotFound();
            }

            var property = bid.Property;

            // Validate bidding time
            if (property.BiddingStartTime > DateTime.Now || property.BiddingEndTime < DateTime.Now)
            {
                TempData["Error"] = "Bidding is closed for this property.";
                return RedirectToAction("Details", new { id = property.Id });
            }

            // Cancel the bid
            bid.IsActive = false;
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = property.Id });
        }

    }
}
