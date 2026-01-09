using NutriAI.Models;
using System.Collections.ObjectModel;

namespace NutriAI.Services;
public static class ServiceLocator
{
    public static IRecipeService RecipeService { get; } =
    new ApiRecipeService();


}
