// Authorship note:
// This interface was designed by the author as the main recommendation operation used in NutriAI.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Recommendation;

// This interface defines the main recommendation operation used by the app.
// It keeps the rest of the system independent from one specific ranking implementation.
public interface IRecommendationService
{
    Task<IReadOnlyList<Recipe>> RankAsync(
        IEnumerable<Recipe> candidates,
        UserPreferences preferences);
}


