namespace ExtraFireworks;

public class ItemTiers
{
    public string Value { get; }

    private ItemTiers(string value)
    {
        Value = value; 
    }

    public override string ToString()
    {
        return Value;
    }
    
    public static ItemTiers White => new ItemTiers("Tier1");
    public static ItemTiers Green => new ItemTiers("Tier2");
    public static ItemTiers Red => new ItemTiers("Tier3");
    public static ItemTiers Lunar => new ItemTiers("Lunar");
    public static ItemTiers Yellow => new ItemTiers("Boss");
}