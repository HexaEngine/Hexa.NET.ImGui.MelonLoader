namespace S1Utils.Structs
{
    using Il2CppScheduleOne.UI.Shop;

    public struct ValueShopListing
    {
        public string ID;
        public string Name;
        public ShopInterface ShopInterface;
        public ShopListing ShopListing;

        public readonly float Price => ShopListing.Price;

        public ValueShopListing(ShopInterface shopInterface, ShopListing shopListing)
        {
            ID = shopListing.Item.ID;
            Name = shopListing.Item.Name;
            ShopInterface = shopInterface;
            ShopListing = shopListing;
        }
    }
}