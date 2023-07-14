using FastEndpoints;

namespace FEPOC.Contracts;

public class SayHelloCommand : ICommand
{
    public string From { get; set; }
}