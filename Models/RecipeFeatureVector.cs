using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Models
{
        public class RecipeFeatureVector
        {
            public string RecipeId { get; set; }

            // Normalized numeric features
            public float Calories { get; set; }
            public float Protein { get; set; }
            public float Carbs { get; set; }
            public float Fat { get; set; }
            public float CookingTime { get; set; }

            // Encoded categories
            public int MealType { get; set; }
            public int Diet { get; set; }
            public int Cuisine { get; set; }
        }

    }

