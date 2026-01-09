namespace NutriAI.DTOs;

public class ApiSearchRecipeDto
{
    public int id { get; set; }
    public string title { get; set; }
    public string image { get; set; }

    public int readyInMinutes { get; set; }
    public Nutrition nutrition { get; set; }
}
