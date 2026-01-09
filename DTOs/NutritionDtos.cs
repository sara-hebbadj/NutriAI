namespace NutriAI.DTOs;

public class Nutrition
{
    public List<Nutrient> nutrients { get; set; }
}

public class Nutrient
{
    public string name { get; set; }
    public double amount { get; set; }
}

public class ExtendedIngredient
{
    public string original { get; set; }
}

public class AnalyzedInstruction
{
    public List<InstructionStep> steps { get; set; }
}

public class InstructionStep
{
    public string step { get; set; }
}
