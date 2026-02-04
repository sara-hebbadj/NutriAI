using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Services.Recommendation;

public static class TimeDecay
{
    // halfLifeDays: 7 means signal halves every 7 days
    public static double Compute(DateTime? lastEventUtc, double halfLifeDays = 7)
    {
        if (lastEventUtc == null)
            return 0.3; // neutral-low for unknown recency

        var daysAgo = (DateTime.UtcNow - lastEventUtc.Value).TotalDays;

        // exp decay using half-life
        return Math.Exp(-Math.Log(2) * daysAgo / halfLifeDays);
    }
}

