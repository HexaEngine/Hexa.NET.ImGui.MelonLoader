namespace S1Utils.Structs
{
    using Il2CppScheduleOne.Product;
    using Il2CppScheduleOne.StationFramework;

    public struct ValueRecipe
    {
        public string Name;
        public ValueIngredient[] Ingredients;
        public float Cost;
        public StationRecipe Recipe;
        public ProductDefinition Product;
    }
}