using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Models;

public class UserPreferences
{
    public string Name { get; set; } = string.Empty;

    public int? WeightKg { get; set; }
    public int? HeightCm { get; set; }
    public string Goal { get; set; } = string.Empty;          // lose weight, gain muscle

    public List<string> DietaryPreferences { get; set; } = new(); // vegan, halal, etc.

    public List<string> Allergies { get; set; } = new();

    public int? DailyCalorieTarget { get; set; }

    public string PreferredCuisine { get; set; } = string.Empty;
}

