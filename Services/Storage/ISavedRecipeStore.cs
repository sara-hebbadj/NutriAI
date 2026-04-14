// Authorship note:
// This interface was written by the author to support loading and saving saved recipes in NutriAI.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Storage;

// This interface defines the basic storage operations
// for loading and saving the user's saved recipes.
public interface ISavedRecipeStore
{
    Task<IReadOnlyList<Recipe>> LoadAsync();
    Task SaveAsync(IEnumerable<Recipe> recipes);
}

