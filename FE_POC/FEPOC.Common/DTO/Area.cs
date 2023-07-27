namespace FEPOC.Common.DTO;

public record Area
{
    public const string DdType = "AREE";
    public int Id { get; set; }   
    public string Descr { get; set; } = null!; 
    public int IdInsediamento { get; set; }
}