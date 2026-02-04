using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace NutriAI.Models;

public class UserRecipeInteraction
{
    [PrimaryKey]
    public string RecipeId { get; set; } = string.Empty;

    public int ViewCount { get; set; }
    public int SaveCount { get; set; }
    public int CookCount { get; set; }

    public DateTime? LastViewedAt { get; set; }
    public DateTime? LastCookedAt { get; set; }

    // breakfast/lunch/dinner/snack/unknown
    public string? LastUsedMealType { get; set; }
}


