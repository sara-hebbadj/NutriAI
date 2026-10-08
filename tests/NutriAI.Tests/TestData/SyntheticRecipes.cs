using NutriAI.Models;

namespace NutriAI.Tests.TestData;

// What a recipe really contains, labelled by hand when the recipe was written.
// These labels are the "ground truth" for the safety checks. They are written independently
// of the keyword lists in RecipeScorer, so the tests can catch words the scorer does not know.
[Flags]
public enum Contains
{
    None = 0,
    Nuts = 1,       // tree nuts and peanuts (the app's "nuts" allergy covers both)
    Dairy = 2,
    Egg = 4,
    Shellfish = 8,  // crustaceans and molluscs (prawns, mussels, scallops, oyster sauce)
    Gluten = 16,    // wheat, barley, rye
    Meat = 32,
    Pork = 64,
    Fish = 128,
}

public sealed record LabelledRecipe(Recipe Recipe, Contains Truth);

// 52 synthetic recipes written for these tests (no Spoonacular data is copied).
// Ingredient lines are written the way recipe sites usually write them
// ("salt and freshly ground black pepper", "300 g large prawns, peeled").
public static class SyntheticRecipes
{
    public static IReadOnlyList<LabelledRecipe> All { get; } = Build();

    // Copies of the catalogue. Tests change recipes (for example clear the ingredient list),
    // so each test gets fresh objects and never edits the shared list.
    public static List<LabelledRecipe> FreshCopy(bool titleOnly = false) =>
        All.Select(x => new LabelledRecipe(Copy(x.Recipe, titleOnly), x.Truth)).ToList();

    public static LabelledRecipe ById(string id) => All.Single(x => x.Recipe.Id == id);

    private static Recipe Copy(Recipe r, bool titleOnly) => new()
    {
        Id = r.Id,
        Title = r.Title,
        MealType = r.MealType,
        Cuisine = r.Cuisine,
        Diet = r.Diet,
        Calories = r.Calories,
        ProteinGrams = r.ProteinGrams,
        CarbsGrams = r.CarbsGrams,
        FatGrams = r.FatGrams,
        CookingTimeMinutes = r.CookingTimeMinutes,
        // titleOnly mimics ApiRecipeService.LoadRecipesAsync, which maps no ingredients.
        Ingredients = titleOnly ? new List<string>() : new List<string>(r.Ingredients),
    };

    private static LabelledRecipe R(
        string id, string title, string meal, string cuisine, string diet,
        int kcal, int protein, int carbs, int fat, int minutes,
        Contains truth, params string[] ingredients)
    {
        var recipe = new Recipe
        {
            Id = id,
            Title = title,
            MealType = meal,
            Cuisine = cuisine,
            Diet = diet,
            Calories = kcal,
            ProteinGrams = protein,
            CarbsGrams = carbs,
            FatGrams = fat,
            CookingTimeMinutes = minutes,
            Ingredients = ingredients.ToList(),
        };
        return new LabelledRecipe(recipe, truth);
    }

    private const Contains N = Contains.None;
    private const Contains Nut = Contains.Nuts, D = Contains.Dairy, E = Contains.Egg, S = Contains.Shellfish;
    private const Contains G = Contains.Gluten, M = Contains.Meat, P = Contains.Pork | Contains.Meat, F = Contains.Fish;

    private static List<LabelledRecipe> Build() => new()
    {
        R("r01", "Overnight Oats with Berries", "breakfast", "other", "vegetarian", 350, 12, 55, 9, 10, D,
            "1 cup gluten-free rolled oats", "1 cup milk", "1/2 cup Greek yogurt", "1 tbsp maple syrup", "1/2 cup mixed berries"),
        R("r02", "Fluffy Buttermilk Pancakes", "breakfast", "american", "vegetarian", 420, 11, 60, 14, 25, G | D | E,
            "2 cups all-purpose flour", "2 tsp baking powder", "1/2 tsp salt", "2 tbsp sugar", "2 cups buttermilk", "2 large eggs", "3 tbsp melted butter"),
        R("r03", "Shakshuka", "breakfast", "middle eastern", "vegetarian", 310, 16, 18, 19, 30, E,
            "2 tbsp olive oil", "1 onion, diced", "1 red bell pepper, diced", "3 garlic cloves, minced", "1 tsp ground cumin",
            "1 tsp paprika", "1 can (400 g) crushed tomatoes", "4 eggs", "salt and freshly ground black pepper", "fresh parsley"),
        R("r04", "Avocado Toast with Poached Egg", "breakfast", "american", "vegetarian", 380, 15, 32, 22, 15, G | E,
            "2 slices sourdough bread", "1 ripe avocado", "2 eggs", "1 tsp lemon juice", "pinch of chili flakes", "salt and pepper"),
        R("r05", "Greek Yogurt Parfait with Almonds", "breakfast", "other", "vegetarian", 330, 18, 38, 12, 5, D | Nut,
            "1 cup Greek yoghurt", "1/3 cup gluten-free granola", "2 tbsp sliced almonds", "1 tbsp maple syrup", "fresh strawberries"),
        R("r06", "Spinach and Feta Omelette", "breakfast", "other", "keto", 290, 20, 4, 22, 12, E | D,
            "3 eggs", "1 cup baby spinach", "30 g feta cheese", "1 tsp butter", "salt and pepper"),
        R("r07", "Banana Peanut Butter Smoothie", "snack", "american", "vegetarian", 340, 12, 45, 14, 5, Nut,
            "1 banana", "2 tbsp peanut butter", "1 cup almond milk", "1 tsp cocoa powder", "a few ice cubes"),
        R("r08", "Hummus and Veggie Wrap", "lunch", "middle eastern", "vegan", 410, 13, 52, 16, 10, G,
            "1 large flour tortilla", "4 tbsp hummus", "1/2 cucumber, sliced", "1 carrot, grated", "handful of mixed greens", "1 tbsp tahini"),
        R("r09", "Chicken Caesar Salad", "lunch", "american", "balanced", 520, 38, 18, 32, 20, M | G | D | E | F,
            "2 chicken breasts", "1 head romaine lettuce", "1/2 cup croutons", "1/4 cup grated parmesan", "3 tbsp Caesar dressing"),
        R("r10", "Red Lentil Soup", "meal", "middle eastern", "vegan", 320, 18, 48, 6, 40, N,
            "1 cup red lentils", "1 onion", "2 carrots", "2 garlic cloves", "1 tsp ground cumin", "4 cups vegetable stock", "1 lemon", "salt and pepper"),
        R("r11", "Spaghetti Bolognese", "dinner", "italian", "balanced", 650, 32, 75, 22, 50, G | M | D,
            "300 g spaghetti", "400 g ground beef", "1 onion", "2 garlic cloves", "1 can crushed tomatoes", "2 tbsp tomato paste", "grated parmesan to serve"),
        R("r12", "Garlic Butter Prawn Linguine", "dinner", "italian", "pescatarian", 610, 30, 68, 22, 25, G | S | D,
            "250 g linguine", "300 g large prawns, peeled", "3 tbsp butter", "4 garlic cloves", "1/4 tsp chili flakes", "fresh parsley", "juice of 1 lemon"),
        R("r13", "Thai Green Curry with Shrimp", "dinner", "asian", "dairy free", 540, 28, 30, 34, 35, S | F,
            "400 ml coconut milk", "2 tbsp green curry paste", "300 g shrimp", "1 tbsp fish sauce", "1 cup green beans", "fresh Thai basil", "jasmine rice"),
        R("r14", "Beef and Broccoli Stir-Fry", "dinner", "asian", "balanced", 520, 36, 28, 26, 25, M | G | S,
            "400 g flank steak, thinly sliced", "3 cups broccoli florets", "3 tbsp soy sauce", "1 tbsp oyster sauce", "1 tbsp cornstarch",
            "2 garlic cloves", "1 tsp grated ginger", "steamed rice"),
        R("r15", "Grilled Salmon with Quinoa", "dinner", "other", "gluten free", 560, 40, 35, 26, 30, F,
            "2 salmon fillets", "1 cup quinoa", "1 lemon", "2 tbsp olive oil", "1 cup cherry tomatoes", "fresh dill", "salt and freshly ground black pepper"),
        R("r16", "Chickpea and Spinach Curry", "dinner", "asian", "vegan", 430, 17, 55, 15, 35, N,
            "2 cans chickpeas", "1 onion", "3 garlic cloves", "1 tbsp curry powder", "1 can (400 ml) coconut milk", "200 g baby spinach", "basmati rice"),
        R("r17", "Margherita Pizza", "dinner", "italian", "vegetarian", 700, 28, 85, 26, 40, G | D,
            "1 pizza dough ball", "1/2 cup tomato sauce", "125 g fresh mozzarella", "fresh basil", "olive oil"),
        R("r18", "Pork Belly Bao Buns", "dinner", "asian", "balanced", 620, 24, 55, 34, 120, P | G,
            "500 g pork belly", "6 steamed bao buns", "2 tbsp hoisin sauce", "1 cucumber", "2 spring onions", "1 tbsp soy sauce"),
        R("r19", "Bacon and Egg Breakfast Muffins", "breakfast", "american", "balanced", 400, 18, 30, 23, 30, E | P | D | G,
            "6 eggs", "4 slices bacon", "1/2 cup shredded cheddar", "1 cup self-raising flour", "1/2 cup milk"),
        R("r20", "Falafel Bowl with Tahini", "lunch", "middle eastern", "vegan", 520, 18, 60, 24, 40, N,
            "1 cup dried chickpeas, soaked", "1 small onion", "fresh parsley", "2 garlic cloves", "1 tsp ground coriander", "1 tsp ground cumin",
            "2 tbsp chickpea flour", "tahini sauce", "cucumber and tomato salad"),
        R("r21", "Pesto Pasta Salad", "lunch", "italian", "vegetarian", 480, 14, 55, 22, 20, G | D | Nut,
            "250 g fusilli pasta", "1/3 cup basil pesto", "1 cup cherry tomatoes", "1/2 cup mozzarella pearls", "handful of rocket"),
        R("r22", "Tuna Nicoise Salad", "lunch", "other", "dairy free", 450, 32, 22, 26, 25, F | E,
            "1 can tuna", "2 eggs", "200 g new potatoes", "100 g green beans", "1/4 cup black olives", "2 tbsp olive oil", "1 tsp Dijon mustard"),
        R("r23", "Shrimp Tacos with Lime Slaw", "dinner", "american", "balanced", 480, 28, 45, 20, 25, S | D,
            "400 g shrimp", "8 small corn tortillas", "2 cups shredded cabbage", "1 lime", "2 tbsp sour cream", "1 tsp chili powder"),
        R("r24", "Seafood Paella", "dinner", "other", "dairy free", 610, 34, 70, 18, 60, S | F,
            "1.5 cups paella rice", "200 g mussels", "200 g king prawns", "150 g squid rings", "pinch of saffron", "4 cups fish stock", "1 red pepper", "1 cup frozen peas"),
        R("r25", "Lamb Kofta with Yogurt Sauce", "dinner", "middle eastern", "balanced", 560, 34, 12, 40, 35, M | D | G,
            "500 g minced lamb", "1 onion, grated", "2 tsp ground cumin", "1 tsp ground cinnamon", "fresh mint", "1 cup plain yogurt", "1 garlic clove", "pita bread to serve"),
        R("r26", "Chicken Shawarma Plate", "dinner", "middle eastern", "balanced", 590, 42, 40, 26, 45, M | D,
            "600 g chicken thighs", "2 tsp shawarma spice", "3 tbsp plain yogurt", "1 lemon", "garlic sauce", "rice", "pickled turnips"),
        R("r27", "Mushroom Risotto", "dinner", "italian", "vegetarian", 520, 13, 68, 18, 45, D,
            "1.5 cups arborio rice", "300 g mushrooms", "1 onion", "4 cups vegetable broth", "1/2 cup grated parmesan", "2 tbsp butter"),
        R("r28", "Vegan Buddha Bowl", "lunch", "other", "vegan", 480, 16, 62, 18, 30, N,
            "1 cup cooked brown rice", "1 sweet potato", "1 can black beans", "1 avocado", "1 cup red cabbage", "2 tbsp tahini", "1 tbsp maple syrup", "1 tbsp lemon juice"),
        R("r29", "Keto Cauliflower Mac and Cheese", "dinner", "american", "keto", 410, 18, 10, 33, 35, D,
            "1 large cauliflower", "1 cup heavy cream", "1.5 cups shredded cheddar", "2 tbsp cream cheese", "1/2 tsp mustard powder"),
        R("r30", "Egg Fried Rice", "lunch", "asian", "balanced", 450, 14, 62, 16, 20, E | G,
            "3 cups cooked rice", "2 eggs", "1 cup frozen peas and carrots", "2 spring onions", "2 tbsp soy sauce", "1 tbsp sesame oil"),
        R("r31", "Chicken Noodle Soup", "meal", "american", "balanced", 380, 28, 35, 12, 45, M | G | E,
            "2 chicken breasts", "150 g egg noodles", "2 carrots", "2 celery stalks", "6 cups chicken broth", "fresh thyme"),
        R("r32", "Ramen with Soft Egg", "dinner", "asian", "balanced", 560, 24, 70, 18, 30, G | E | M,
            "2 portions ramen noodles", "4 cups chicken stock", "2 eggs", "1 tbsp miso paste", "2 spring onions", "100 g bok choy"),
        R("r33", "Almond Butter Energy Bites", "snack", "other", "vegan", 210, 6, 22, 12, 15, Nut,
            "1 cup gluten-free oats", "1/2 cup almond butter", "1/3 cup maple syrup", "1/4 cup dairy-free dark chocolate chips", "2 tbsp chia seeds"),
        R("r34", "Apple Slices with Cashew Dip", "snack", "other", "vegan", 190, 5, 24, 9, 5, Nut,
            "2 apples", "1/4 cup cashews, soaked", "1 tbsp lemon juice", "1 tsp cinnamon"),
        R("r35", "Hard-Boiled Eggs with Seed Seasoning", "snack", "american", "keto", 160, 12, 1, 11, 15, E,
            "4 eggs", "1 tsp sesame and poppy seed seasoning", "pinch of salt"),
        R("r36", "Cheese and Crackers Plate", "snack", "other", "vegetarian", 280, 11, 20, 17, 5, D | G | Nut,
            "60 g aged cheddar", "8 whole-wheat crackers", "1/2 cup grapes", "a few walnuts"),
        R("r37", "Vegan Chocolate Avocado Mousse", "snack", "other", "vegan", 260, 4, 26, 18, 10, N,
            "2 ripe avocados", "1/4 cup cocoa powder", "1/4 cup maple syrup", "1/4 cup soy milk", "1 tsp vanilla extract"),
        R("r38", "Caprese Salad", "lunch", "italian", "gluten free", 320, 16, 8, 25, 10, D,
            "2 large tomatoes", "200 g fresh mozzarella", "fresh basil", "2 tbsp extra virgin olive oil", "balsamic glaze"),
        R("r39", "Turkey Club Sandwich", "lunch", "american", "balanced", 560, 36, 42, 26, 10, G | M | P | E,
            "3 slices white bread", "120 g sliced turkey breast", "2 slices bacon", "2 tbsp mayonnaise", "lettuce leaves", "1 tomato"),
        R("r40", "Black Bean Burrito Bowl", "lunch", "american", "vegan", 520, 20, 78, 14, 25, N,
            "1 cup cooked rice", "1 can black beans", "1 cup corn", "1/2 cup salsa", "1 avocado", "1 tsp ground cumin", "1 lime"),
        R("r41", "Baked Cod with Herb Crust", "dinner", "other", "balanced", 420, 38, 18, 18, 30, F | G | D,
            "2 cod fillets", "1/2 cup panko breadcrumbs", "1 tbsp butter, melted", "fresh parsley", "1 lemon"),
        R("r42", "Stuffed Bell Peppers", "dinner", "american", "balanced", 480, 26, 38, 22, 55, M | D,
            "4 bell peppers", "300 g lean ground turkey", "1 cup cooked rice", "1 can diced tomatoes", "1/2 cup shredded mozzarella", "1 tsp Italian seasoning"),
        R("r43", "Tofu Vegetable Stir-Fry", "dinner", "asian", "vegan", 380, 20, 30, 18, 25, N,
            "400 g firm tofu", "2 cups mixed vegetables", "2 tbsp tamari (gluten-free soy sauce)", "1 tbsp sesame oil", "1 tsp grated ginger", "2 garlic cloves"),
        R("r44", "Lemon Herb Chicken with Roasted Vegetables", "dinner", "other", "gluten free", 480, 42, 24, 22, 50, M,
            "4 chicken thighs", "2 lemons", "3 garlic cloves", "fresh rosemary", "500 g baby potatoes", "2 courgettes", "olive oil"),
        R("r45", "Scallop and Asparagus Pan-Fry", "dinner", "other", "keto", 380, 30, 10, 22, 20, S | D,
            "300 g sea scallops", "1 bunch asparagus", "2 tbsp butter", "1 garlic clove", "lemon zest"),
        R("r46", "Mussels in Tomato Broth", "dinner", "italian", "dairy free", 420, 30, 24, 14, 30, S | F | G,
            "1 kg fresh mussels", "1 can chopped tomatoes", "3 garlic cloves", "1 cup fish stock", "fresh parsley", "crusty bread to serve"),
        R("r47", "Vegetable Lasagna", "dinner", "italian", "vegetarian", 590, 26, 60, 26, 75, G | D,
            "12 lasagna sheets", "500 g ricotta", "2 cups mozzarella", "2 courgettes", "1 jar marinara sauce", "200 g spinach"),
        R("r48", "Sweet Potato and Black Bean Chili", "dinner", "american", "vegan", 450, 17, 70, 10, 50, N,
            "2 sweet potatoes", "2 cans black beans", "1 can crushed tomatoes", "1 onion", "2 tbsp chili powder", "1 tsp ground cumin", "2 cups vegetable broth"),
        R("r49", "Mango Coconut Chia Pudding", "breakfast", "asian", "vegan", 300, 6, 38, 14, 10, N,
            "1/4 cup chia seeds", "1 cup coconut milk", "1 ripe mango", "1 tbsp maple syrup"),
        R("r50", "Smoked Salmon Bagel", "breakfast", "american", "balanced", 420, 24, 48, 14, 5, G | D | F,
            "1 plain bagel", "2 tbsp cream cheese", "60 g smoked salmon", "1 tsp capers", "thinly sliced red onion"),
        R("r51", "Hazelnut Chocolate Crepes", "snack", "other", "vegetarian", 450, 10, 55, 22, 25, G | E | D | Nut,
            "1 cup plain flour", "2 eggs", "1.25 cups milk", "1 tbsp butter", "4 tbsp chocolate hazelnut spread"),
        R("r52", "Quinoa Tabbouleh", "lunch", "middle eastern", "vegan", 280, 9, 38, 11, 20, N,
            "1 cup quinoa", "2 bunches parsley", "fresh mint", "3 tomatoes", "1 cucumber", "3 tbsp olive oil", "juice of 2 lemons"),
    };
}
