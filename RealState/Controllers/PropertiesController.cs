using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealState.Data;
using RealState.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealState.Controllers
{
    [Authorize]
    public class PropertiesController : Controller
    {
        private readonly RealStateContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PropertiesController(RealStateContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List all properties
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Show only properties created by the current seller
                var properties = await _context.Properties
                    .Where(p => p.CreatedBy == user.Id)
                    .ToListAsync();

                return View(properties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Index action: {ex.Message}");
                return View("Error");
            }
        }

        // Show details of a property
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    return NotFound();
                }

                var property = await _context.Properties
                    .Include(p => p.Bids) // Include related bids
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (property == null)
                {
                    return NotFound();
                }

                return View(property);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Details action: {ex.Message}");
                return View("Error");
            }
        }

        // Show create form
        [Authorize(Roles = "Seller")]
        public IActionResult Create()
        {
            return View(new Property());
        }

        // Handle create form submission
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                Console.WriteLine($"Logged-in User ID: {user.Id}");

                // Set the CreatedBy field manually
                property.CreatedBy = user.Id;

                // Remove CreatedBy from ModelState to prevent validation issues
                ModelState.Remove(nameof(Property.CreatedBy));

                if (ModelState.IsValid)
                {
                    _context.Properties.Add(property);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                // Log ModelState errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }

                return View(property);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Create action: {ex.Message}");
                return View("Error");
            }
        }

        // Show edit form
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

                if (property == null)
                {
                    TempData["Error"] = "You can only edit properties you own.";
                    return RedirectToAction(nameof(Index));
                }

                return View(property);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Edit action: {ex.Message}");
                return View("Error");
            }
        }

        // Handle edit form submission
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Property property)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (id != property.Id)
                {
                    return NotFound();
                }

                // Get the existing property to update only editable fields
                var existingProperty = await _context.Properties
                    .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

                if (existingProperty == null)
                {
                    TempData["Error"] = "You can only edit properties you own.";
                    return RedirectToAction(nameof(Index));
                }

                // Remove CreatedBy from ModelState to prevent validation issues
                ModelState.Remove(nameof(Property.CreatedBy));

                if (ModelState.IsValid)
                {
                    // Update editable fields only
                    existingProperty.Name = property.Name;
                    existingProperty.Address = property.Address;
                    existingProperty.Price = property.Price;
                    existingProperty.VideoUrl = property.VideoUrl;
                    existingProperty.BiddingStartTime = property.BiddingStartTime;
                    existingProperty.BiddingEndTime = property.BiddingEndTime;

                    _context.Update(existingProperty);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                // Log ModelState errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }

                return View(property);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Edit action: {ex.Message}");
                return View("Error");
            }
        }

        // Show delete confirmation
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

                if (property == null)
                {
                    TempData["Error"] = "You can only delete properties you own.";
                    return RedirectToAction(nameof(Index));
                }

                return View(property);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Delete action: {ex.Message}");
                return View("Error");
            }
        }

        // Handle delete confirmation
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == user.Id);

                if (property == null)
                {
                    TempData["Error"] = "You can only delete properties you own.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteConfirmed action: {ex.Message}");
                return View("Error");
            }
        }
    }
}
