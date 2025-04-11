namespace S1Utils.Structs
{
    using Il2CppScheduleOne.StationFramework;

    public struct ValueIngredient
    {
        public string Name;
        public StationRecipe.IngredientQuantity Ingredient;
        public float Price;

        public ValueIngredient(StationRecipe.IngredientQuantity ingredient)
        {
            Name = ingredient.Item.Name;
            Ingredient = ingredient;
        }
    }
}