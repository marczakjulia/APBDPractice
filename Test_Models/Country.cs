namespace Test_Models;

public class Country
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<Currency> Currencies { get; set; }
}