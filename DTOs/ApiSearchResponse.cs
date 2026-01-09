using NutriAI.Models;

namespace NutriAI.DTOs;

public class ApiSearchResponse
{
    public List<ApiSearchRecipeDto> results { get; set; }
}
