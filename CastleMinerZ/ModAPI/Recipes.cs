using System.Collections.Generic;
using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Recipes
    {
        public static void Add(InventoryItemIDs result, int resultCount, params InventoryItemIDs[] ingredients)
        {
            InventoryItem resultItem = InventoryItem.CreateItem(result, resultCount);
            InventoryItem[] ingredientItems = new InventoryItem[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                ingredientItems[i] = InventoryItem.CreateItem(ingredients[i], 1);
            }
            Receipe.CookBook.Add(new Receipe(resultItem, ingredientItems));
        }

        public static void Add(string resultId, int resultCount, params InventoryItemIDs[] ingredients)
        {
            InventoryItem resultItem = InventoryItem.CreateItem(new ModItemId(resultId), resultCount);
            InventoryItem[] ingredientItems = new InventoryItem[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                ingredientItems[i] = InventoryItem.CreateItem(ingredients[i], 1);
            }
            Receipe.CookBook.Add(new Receipe(resultItem, ingredientItems));
        }

        public static void Add(string resultId, int resultCount, params string[] ingredientIds)
        {
            InventoryItem resultItem = InventoryItem.CreateItem(new ModItemId(resultId), resultCount);
            InventoryItem[] ingredientItems = new InventoryItem[ingredientIds.Length];
            for (int i = 0; i < ingredientIds.Length; i++)
            {
                ingredientItems[i] = InventoryItem.CreateItem(new ModItemId(ingredientIds[i]), 1);
            }
            Receipe.CookBook.Add(new Receipe(resultItem, ingredientItems));
        }

        public static void Remove(InventoryItemIDs itemId)
        {
            List<Receipe> toRemove = new List<Receipe>();
            for (int i = 0; i < Receipe.CookBook.Count; i++)
            {
                if (Receipe.CookBook[i].Result.ItemClass.ID == itemId)
                {
                    toRemove.Add(Receipe.CookBook[i]);
                }
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                Receipe.CookBook.Remove(toRemove[i]);
            }
        }

        public static void Remove(string itemId)
        {
            List<Receipe> toRemove = new List<Receipe>();
            for (int i = 0; i < Receipe.CookBook.Count; i++)
            {
                if (Receipe.CookBook[i].Result.ItemClass.ModItemId == itemId)
                {
                    toRemove.Add(Receipe.CookBook[i]);
                }
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                Receipe.CookBook.Remove(toRemove[i]);
            }
        }

        public static void Modify(InventoryItemIDs itemId, int newResultCount, params InventoryItemIDs[] newIngredients)
        {
            Remove(itemId);
            Add(itemId, newResultCount, newIngredients);
        }

        public static void Modify(string itemId, int newResultCount, params InventoryItemIDs[] newIngredients)
        {
            Remove(itemId);
            Add(itemId, newResultCount, newIngredients);
        }

        public static void Clear()
        {
            Receipe.CookBook.Clear();
        }
    }
}
