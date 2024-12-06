namespace Conster.Core;

public class WorkerClientMessageRequest
{
    public List<string> IDs { get; set; } = new();
    public string Name { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Server { get; set; } = null!;
}