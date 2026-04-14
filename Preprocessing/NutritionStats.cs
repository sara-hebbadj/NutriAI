// Authorship note:
// This file was written by the author as part of NutriAI's preprocessing pipeline.
// Microsoft documentation was used as reference for standard C# class and field syntax.
// The decision to keep min/max nutrition statistics in one small helper class
// for feature normalization was made by the author.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Preprocessing
{
    public class NutritionStats
    {
        // These fields store the minimum and maximum values
        // for each nutrition attribute across the current recipe set.
        // They are later used for min-max normalization.
        public float MinCalories, MaxCalories;
        public float MinProtein, MaxProtein;
        public float MinCarbs, MaxCarbs;
        public float MinFat, MaxFat;
        public float MinTime, MaxTime;
    }

}