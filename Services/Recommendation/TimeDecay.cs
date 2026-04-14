// Authorship note:
// Microsoft documentation was used for DateTime and Math functions in this file.
// Copilot was used to help express the exponential decay formula in code.
// The use of half-life decay for recommendation recency, including the 7-day setting,
// was selected and applied by the author for NutriAI.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Services.Recommendation;

public static class TimeDecay
{
    // halfLifeDays: 7 means the effect of an interaction becomes half as strong every 7 days.
    public static double Compute(DateTime? lastEventUtc, double halfLifeDays = 7)
    {
        // If there is no previous interaction, return a low default weight
        // instead of treating the recipe as fully recent.
        if (lastEventUtc == null)
            return 0.3; // neutral-low for unknown recency

        var daysAgo = (DateTime.UtcNow - lastEventUtc.Value).TotalDays;

        // Use exponential decay so recent interactions matter more,
        // while older interactions gradually lose influence over time.
        return Math.Exp(-Math.Log(2) * daysAgo / halfLifeDays);
    }
}

