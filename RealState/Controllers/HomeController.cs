using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealState.Data;

public class HomeController : Controller
{
    private readonly RealStateContext _context;

    public HomeController(RealStateContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Fetch all properties
        var properties = await _context.Properties.ToListAsync();

        // Pass the properties to the view
        return View(properties);
    }
}
