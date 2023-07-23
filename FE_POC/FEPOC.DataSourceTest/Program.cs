// See https://aka.ms/new-console-template for more information


var a1 = new Area
{
    Id = 0,
    Descr = "aaaa",
    IdInsediamento = 0
};
var a2 = new Area
{
    Id = 0,
    Descr = "aaaa",
    IdInsediamento = null
};
Console.WriteLine($"A1==A2 {a1.GetHashCode()==a2.GetHashCode()}");




Console.ReadLine();


public record Area
{
    public const string DdType = "AREE";
    public int Id { get; set; }   
    public string Descr { get; set; } = null!; 
    public int? IdInsediamento { get; set; }
}

public record Insediamento
{
    public const string DdType = "INSEDIAMENTI";
    
    public int Id { get; set; }

    public string? Codice { get; set; }

    public string Descr { get; set; } = null!;

    // public int? Idpolicyaccesso { get; set; }

    public int? Idportineria { get; set; }

    // public int? Idcontrollovarchiconfig { get; set; }
}