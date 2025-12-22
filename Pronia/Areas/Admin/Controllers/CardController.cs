using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;
using Pronia.Models;

namespace Pronia.Areas.Admin.Controllers;

[Area("Admin")]
public class CardController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var cards = await context.Cards.ToListAsync();
        return View(cards);
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Create(string description, string title, string icon)
    {
        Card card = new()
        {
            Description = description,
            Title = title,
            Icon = icon
        };

        await context.AddAsync(card);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var card = await context.Cards.FindAsync(id);
        if (card is null)
            return NotFound();

        context.Cards.Remove(card);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {

        var feature = await context.Cards.FindAsync(id);
        return View(feature);
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, Card card)
    {
        if (!ModelState.IsValid)
            return View(card);

        var updatedCard = await context.Cards.FindAsync(id);
        if (updatedCard is null) return NotFound();

        updatedCard.Title = card.Title;
        updatedCard.Description = card.Description;
        updatedCard.Icon = card.Icon;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
