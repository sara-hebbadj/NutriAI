using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Models;

public class RecipeFilter
{
    public string MealType { get; set; } = string.Empty;
    public string Diet { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public int? MaxCalories { get; set; }
}
