using NutriAI.Models;

// Authorship note:
// External help used in this file was specific to:
// 1) Postman Spoonacular API documentation was used to confirm the top-level
//    search response structure and results collection
// 2) Microsoft Learn, used to confirm standard C# property syntax for simple DTO wrapper classes
// 3) Minor boilerplate assistance from Copilot only where repeated DTO syntax was typed faster


namespace NutriAI.DTOs;

public class ApiSearchResponse
{
    // Holds the list of recipe items returned by the search request.
    public List<ApiSearchRecipeDto> results { get; set; } = new();

    // This property appears to have been added for internal handling.
    // If not used anywhere in the project, it may be removed to keep the DTO clean.
    public object? Content { get; internal set; }
}