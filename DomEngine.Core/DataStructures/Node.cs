namespace DomEngine.Core.DataStructures;

/// <summary>
/// Çift yönlü bağlı liste ve diğer veri yapıları için temel düğüm (Node).
/// </summary>
/// <typeparam name="T">Düğümün tutacağı verinin tipi.</typeparam>
public class Node<T>
{
    public T Data { get; set; }
    public Node<T>? Next { get; set; }
    public Node<T>? Prev { get; set; }

    public Node(T data)
    {
        Data = data;
        Next = null;
        Prev = null;
    }
}
