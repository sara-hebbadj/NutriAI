namespace NutriAI.DTOs;

public class ApiRecipeDetailsDto
{
    public int id { get; set; }
    public string title { get; set; }
    public string image { get; set; }

    public int readyInMinutes { get; set; }
    public List<ExtendedIngredient> extendedIngredients { get; set; }
    public List<AnalyzedInstruction> analyzedInstructions { get; set; }
    public Nutrition nutrition { get; set; }
}
