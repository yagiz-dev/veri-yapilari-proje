namespace DomEngine.Core.DataStructures;

/// <summary>
/// Çift yönlü bağlı liste ve diğer veri yapıları için temel düğüm (Node).
/// </summary>
/// <typeparam name="T">Düğümün tutacağı verinin tipi.</typeparam>
public class ArdaNode<T>
{
    public T Data { get; set; }
    public ArdaNode<T>? Next { get; set; }
    public ArdaNode<T>? Prev { get; set; }

    public ArdaNode(T data)
    {
        Data = data;
        Next = null;
        Prev = null;
    }
}
