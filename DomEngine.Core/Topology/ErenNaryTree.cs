using DomEngine.Core.DataStructures;

namespace DomEngine.Core.Topology;

/// <summary>
/// Sınırsız alt çocuk düğüm destekleyen asıl ağaç yapısı.
/// Tüm HTML belgesini (Document) temsil eder.
/// </summary>
public class ErenNaryTree
{
    public ErenDomNode Root { get; private set; }
    
    // O(1) hızında id ile erişim sağlamak için oluşturulan Hash Table
    private ArdaHashTable<string, ErenDomNode> _elementsById;

    public ErenNaryTree(string rootTagName = "html")
    {
        Root = new ErenDomNode(rootTagName);
        _elementsById = new ArdaHashTable<string, ErenDomNode>();
    }

    /// <summary>
    /// Ağaca yeni bir düğüm eklendiğinde, eğer id'si varsa Hash Table'a kaydeder.
    /// Böylece GetElementById O(1) sürede çalışır.
    /// </summary>
    public void RegisterNode(ErenDomNode node)
    {
        string id = node.Id;
        if (!string.IsNullOrEmpty(id))
        {
            // Eğer aynı id'den birden fazla varsa HTML standartlarına aykırıdır, 
            // ancak okul projesi basitliğinde sadece ilkini tutabilir veya üzerine yazabiliriz.
            _elementsById[id] = node;
        }
    }

    /// <summary>
    /// O(1) karmaşıklığında, ağaçta arama yapmadan doğrudan Hash Table üzerinden düğümü bulur.
    /// </summary>
    public ErenDomNode? GetElementById(string id)
    {
        if (_elementsById.TryGetValue(id, out ErenDomNode node))
        {
            return node;
        }
        return null;
    }

    /// <summary>
    /// Kök düğüme veya belirtilen ebeveyn düğüme yeni bir çocuk ekler.
    /// </summary>
    public void AddNode(ErenDomNode parent, ErenDomNode child)
    {
        if (parent == null)
        {
            throw new System.ArgumentNullException(nameof(parent));
        }

        parent.AddChild(child);
    }
}
