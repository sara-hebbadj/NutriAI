// Authorship note:
// External help used in this file was specific to:
// 1) Postman Spoonacular API documentation was used to identify the nested JSON
//    structure for nutrition, nutrients, ingredients, and instruction steps
// 2) Microsoft Learn, used to confirm standard C# property syntax and list/object representation
//    for deserialization
// 3) Minor boilerplate assistance from Copilot only for repeated small DTO class declarations

namespace NutriAI.DTOs;

public class Nutrition
{
    // Holds the nutrient entries returned by the API.
    public List<Nutrient> nutrients { get; set; } = new();
}

public class Nutrient
{
    public string name { get; set; } = string.Empty;

    public double amount { get; set; }
}

public class ExtendedIngredient
{
    // Stores the ingredient text in the form returned by the API.
    public string original { get; set; } = string.Empty;
}

public class AnalyzedInstruction
{
    // Each instruction block may contain multiple ordered steps.
    public List<InstructionStep> steps { get; set; } = new();
}

public class InstructionStep
{
    public string step { get; set; } = string.Empty;
}