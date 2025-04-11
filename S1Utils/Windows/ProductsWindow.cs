namespace S1Utils.Windows
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.Utilities.Text;
    using Il2CppInterop.Runtime;
    using Il2CppInterop.Runtime.Runtime;
    using Il2CppScheduleOne.ItemFramework;
    using Il2CppScheduleOne.NPCs;
    using Il2CppScheduleOne.PlayerScripts;
    using Il2CppScheduleOne.Product;
    using Il2CppScheduleOne.UI.Shop;
    using Il2CppSystem.Collections.Generic;
    using S1Utils.Extensions;
    using S1Utils.Structs;
    using System;
    using Unity.Collections;

    public class ProductsWindow : ImWindow
    {
        private string searchString = string.Empty;
        private ProductDefinition? selectedProduct;
        private static System.Collections.Generic.List<ValueShopInterface> shops = [];
        private float split = 200;

        public ProductsWindow()
        {
            shops = [.. ShopInterface.AllShops.Select(x => new ValueShopInterface(x))];
        }

        public override string Name { get; } = "Products";

        public override unsafe void DrawContent()
        {
            byte* buffer = stackalloc byte[1024];
            StrBuilder builder = new(buffer, 1024);

            ImGui.InputTextWithHint("##Search"u8, "Search ..."u8, ref searchString, 1024);
            var avail = ImGui.GetContentRegionAvail();
            ImGui.BeginChild("##Products"u8, new System.Numerics.Vector2(split, avail.Y));

            bool search = !string.IsNullOrEmpty(searchString);
            ImGui.SeparatorText("Favourited"u8);
            DrawProducts(ref builder, ProductManager.FavouritedProducts, search, 0);
            ImGui.SeparatorText("All"u8);
            DrawProducts(ref builder, ProductManager.DiscoveredProducts, search, 1);
            ImGui.EndChild();

            ImGuiSplitter.VerticalSplitter("##Splitter"u8, ref split, 0, avail.X);

            ImGui.BeginChild("##Selected"u8);
            if (selectedProduct != null)
            {
                Display();
            }
            ImGui.EndChild();
        }

        private unsafe void DrawProducts(ref StrBuilder builder, List<ProductDefinition> products, bool search, int idMajor)
        {
            for (int i = 0; i < products.Count; i++)
            {
                ProductDefinition product = products[i];

                var name = product.GetName();

                if (search && !name.AsReadOnlySpan().Contains(searchString, StringComparison.CurrentCultureIgnoreCase))
                {
                    name.Release();
                    continue;
                }

                builder.Reset();
                builder.Append(name);
                builder.Append("##"u8);
                product.AppendID(ref builder);
                builder.Append(idMajor);
                builder.End();
                name.Release();

                if (ImGui.Selectable(builder, selectedProduct == product))
                {
                    visited.Clear();
                    recipes.Clear();
                    Traverse(product);
                    selectedProduct = product;
                }
            }
        }

        private static readonly System.Collections.Generic.HashSet<string> visited = [];
        private static readonly System.Collections.Generic.List<ValueRecipe> recipes = [];

        private static float Traverse(ProductDefinition product)
        {
            if (!visited.Add(product.ID))
            {
                return 0;
            }

            float price = 0;
            foreach (var recipe in product.Recipes)
            {
                ValueRecipe valueRecipe = new()
                {
                    Name = product.Name,
                    Ingredients = new ValueIngredient[recipe.Ingredients.Count],
                    Recipe = recipe,
                    Product = product
                };

                ProductDefinition? next = null;
                int i = 0;
                foreach (var ingredient in recipe.Ingredients)
                {
                    ValueIngredient valueIngredient = new(ingredient);
                    if (ingredient.Item is ProductDefinition productDefinition)
                    {
                        next = productDefinition;
                    }

                    if (ingredient.Item is ItemDefinition itemDefinition)
                    {
                        var result = FindItemInShop(itemDefinition.ID);
                        if (result.HasValue)
                        {
                            valueIngredient.Price = result.Value.Price;
                        }
                    }

                    valueRecipe.Cost += valueIngredient.Price;
                    valueRecipe.Ingredients[i] = valueIngredient;
                    i++;
                }
                int index = recipes.Count;
                recipes.Add(valueRecipe);
                if (next != null)
                {
                    valueRecipe.Cost += Traverse(next);
                    recipes[index] = valueRecipe;
                }

                price = Math.Min(price, valueRecipe.Cost);
            }

            return price;
        }

        private static ValueShopListing? FindItemInShop(string id)
        {
            foreach (var shop in shops)
            {
                if (shop.IDToListing.TryGetValue(id, out var listing))
                {
                    return listing;
                }
            }
            return default;
        }

        private static unsafe void Display()
        {
            byte* buffer = stackalloc byte[1024];
            StrBuilder builder = new(buffer, 1024);
            for (int i = 0; i < recipes.Count; i++)
            {
                ValueRecipe recipe = recipes[i];
                builder.Reset();
                builder.Append(recipe.Name);
                builder.Append(", $ "u8);
                builder.Append(recipe.Product.Price);
                builder.Append(", cost $ "u8);
                builder.Append(recipe.Cost);
                builder.End();
                if (!ImGui.TreeNodeEx(builder, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    break;
                }
                ImGui.SameLine();

                ImGui.Separator();
                foreach (var ingredient in recipe.Ingredients)
                {
                    ImGui.TreeNodeEx(ingredient.Name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.Bullet);
                }
                ImGui.TreePop();
            }
        }
    }
}