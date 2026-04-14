// Authorship note:
// External help used in this file was specific to:
// 1) Postman Spoonacular API documentation was used to identify the JSON
//    fields returned for recipe search items
// 2) Microsoft Learn, used to confirm standard C# property syntax and JSON deserialization structure
// 3) Minor boilerplate assistance from Copilot only for repeated DTO property declarations


namespace NutriAI.DTOs;

public class ApiSearchRecipeDto
{
    // Names are kept aligned with the API response for straightforward deserialization.
    public int id { get; set; }

    public string title { get; set; } = string.Empty;

    public string image { get; set; } = string.Empty;

    public int readyInMinutes { get; set; }

    public Nutrition nutrition { get; set; } = new();

    public List<string> dishTypes { get; set; } = new();

    public List<string> diets { get; set; } = new();

    public List<string> cuisines { get; set; } = new();
}