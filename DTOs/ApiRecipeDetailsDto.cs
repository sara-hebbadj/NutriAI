// Authorship note:
// External help used in this file was specific to:
// 1) Postman Spoonacular API documentation was used to identify the field names
//    and nested recipe-detail sections such as ingredients, instructions, and nutrition
// 2) Microsoft Learn, used to confirm standard C# property syntax and JSON deserialization structure
// 3) Minor boilerplate assistance from Copilot only for repeated DTO property declarations

namespace NutriAI.DTOs;

public class ApiRecipeDetailsDto
{
    // Property names follow the API response field names so the JSON can be deserialized directly.
    public int id { get; set; }

    public string title { get; set; } = string.Empty;

    public string image { get; set; } = string.Empty;

    public int readyInMinutes { get; set; }

    // Nested DTO types are used to capture ingredients, instructions, and nutrition data.
    public List<ExtendedIngredient> extendedIngredients { get; set; } = new();

    public List<AnalyzedInstruction> analyzedInstructions { get; set; } = new();

    public Nutrition nutrition { get; set; } = new();
}