using FastEndpoints;

namespace FEPOC.Contracts;

public sealed class SomethingHappened : IEvent
{
    public int Id { get; set; }
    public string Description { get; set; }
}
