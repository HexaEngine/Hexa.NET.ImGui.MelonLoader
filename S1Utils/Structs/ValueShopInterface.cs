namespace S1Utils.Structs
{
    using Il2CppScheduleOne.UI.Shop;
    using S1Utils.Extensions;

    public struct ValueShopInterface
    {
        public string ShopName;
        public List<ValueShopListing> Listings;
        public Dictionary<string, ValueShopListing> IDToListing;
        public ShopInterface ShopInterface;

        public ValueShopInterface(ShopInterface shopInterface)
        {
            ShopName = shopInterface.ShopName;
            Listings = [.. shopInterface.Listings.Select(x => new ValueShopListing(shopInterface, x))];
            IDToListing = Listings.ToDictionary(x => x.ID);
            ShopInterface = shopInterface;
        }
    }
}