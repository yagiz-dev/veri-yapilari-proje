using DomEngine.Core.DataStructures;

namespace DomEngine.Core.Topology;

/// <summary>
/// DOM Ağacındaki her bir öğeyi (element) temsil eden düğüm sınıfı.
/// </summary>
public class ErenDomNode
{
    public string TagName { get; set; }
    public string InnerText { get; set; }
    public ErenDomNode? Parent { get; set; }
    
    // Kendi yazdığımız liste sınıfı kullanılıyor
    public CustomList<ErenDomNode> Children { get; private set; }
    
    // Nitelikleri (id, class, src vb.) tutan HashTable
    public CustomHashTable<string, string> Attributes { get; private set; }

    public ErenDomNode(string tagName)
    {
        TagName = tagName;
        InnerText = string.Empty;
        Children = new CustomList<ErenDomNode>();
        Attributes = new CustomHashTable<string, string>();
    }

    /// <summary>
    /// Bu düğüme alt düğüm (child) ekler.
    /// </summary>
    public void AddChild(ErenDomNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    // Kısa erişim özellikleri (Opsiyonel ama işi kolaylaştırır)
    public string Id
    {
        get => Attributes.TryGetValue("id", out string id) ? id : string.Empty;
        set => Attributes["id"] = value;
    }

    public string ClassName
    {
        get => Attributes.TryGetValue("class", out string cls) ? cls : string.Empty;
        set => Attributes["class"] = value;
    }
}
