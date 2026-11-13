
public class NBPData
{
    public string table { get; set; }
    public string currency { get; set; }
    public string code { get; set; }
    public NBPRate[] rates { get; set; }
}

public class NBPRate
{
    public string no { get; set; }
    public DateOnly effectiveDate { get; set; }
    public decimal mid { get; set; }
}
