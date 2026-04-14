// Authorship note:
// This file was written by the author to store user-recipe interaction history for NutriAI's
// local recommendation logic.
// External help was used in two limited areas:
// 1) sqlite-net usage, especially the PrimaryKey attribute for local SQLite mapping
// 2) standard C# syntax for nullable DateTime fields and simple properties
// Documentation and tutorial-style examples were used to understand how model classes map to
// SQLite tables, but the interaction fields themselves and their meaning in NutriAI were decided
// by the author.
// The design choice to track views, saves, cooks, timestamps, and last used meal context
// is the author's own work.

using SQLite;


namespace NutriAI.Models;

public class UserRecipeInteraction
{
    // RecipeId is used as the unique key for one interaction record per recipe.
    [PrimaryKey]
    public string RecipeId { get; set; } = string.Empty;

    public int ViewCount { get; set; }
    public int SaveCount { get; set; }
    public int CookCount { get; set; }

    // These stay null until the user actually performs the relevant action.
    public DateTime? LastViewedAt { get; set; }
    public DateTime? LastCookedAt { get; set; }

    // Stores the most recent meal context linked to recipe use.
    // Example values: breakfast, lunch, dinner, snack.
    public string? LastUsedMealType { get; set; }
}