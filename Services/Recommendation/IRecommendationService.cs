using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Recommendation;

public interface IRecommendationService
{
    Task<IReadOnlyList<Recipe>> RankAsync(IEnumerable<Recipe> candidates);
}

