using System;
using DomEngine.Core.DataStructures;
using DomEngine.Core.Topology;

namespace DomEngine.Core.Algorithms;

/// <summary>
/// DOM Ağacı üzerinde arama, gezinti (Traversal) ve analiz algoritmaları barındırır.
/// </summary>
public static class YusufPehDomSearch
{
    /// <summary>
    /// Kullanıcının girdiği sorguya göre bir düğümün eşleşip eşleşmediğini kontrol eder.
    /// Desteklenen formatlar:
    ///   id="header"          → Düğümün id'si "header" mı?
    ///   class="container"    → Düğümün class'ları arasında "container" var mı?
    ///   tag="div"            → Düğümün etiket ismi "div" mi?
    ///   href="#home"          → Düğümün href attribute'u "#home" mu?
    ///   (ve diğer tüm attribute'lar aynı şekilde çalışır)
    /// </summary>
    private static bool IsMatch(ErenDomNode node, string searchKey, string searchValue)
    {
        // "tag" özel durum: etiket ismi attribute'larda değil, TagName'de tutulur
        if (searchKey == "tag")
        {
            return node.TagName.ToLower() == searchValue.ToLower();
        }

        // Diğer her şey (id, class, href, src vb.): HashTable'dan bak
        if (node.Attributes.TryGetValue(searchKey, out string value))
        {
            return value.ToLower().Contains(searchValue.ToLower());
        }

        return false;
    }

    /// <summary>
    /// Genişlik Öncelikli Arama (BFS - Breadth First Search) kullanarak arama yapar.
    /// CustomQueue kullanarak seviye seviye tüm düğümleri ziyaret eder.
    /// Zaman Karmaşıklığı: O(N) — N: toplam düğüm sayısı
    /// Uzay Karmaşıklığı: O(W) — W: ağacın en geniş seviyesindeki düğüm sayısı
    /// </summary>
    public static CustomList<ErenDomNode> SearchBFS(ErenNaryTree tree, string searchKey, string searchValue)
    {
        var result = new CustomList<ErenDomNode>();
        if (tree.Root == null || string.IsNullOrWhiteSpace(searchValue)) return result;

        var queue = new CustomQueue<ErenDomNode>();
        queue.Enqueue(tree.Root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (IsMatch(current, searchKey, searchValue))
            {
                result.Add(current);
            }

            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }

        return result;
    }

    /// <summary>
    /// Derinlik Öncelikli Arama (DFS - Depth First Search) kullanarak arama yapar.
    /// CustomStack kullanarak bir dalı sonuna kadar takip eder, sonra geri döner.
    /// Zaman Karmaşıklığı: O(N) — N: toplam düğüm sayısı
    /// Uzay Karmaşıklığı: O(D) — D: ağacın derinliği
    /// </summary>
    public static CustomList<ErenDomNode> SearchDFS(ErenNaryTree tree, string searchKey, string searchValue)
    {
        var result = new CustomList<ErenDomNode>();
        if (tree.Root == null || string.IsNullOrWhiteSpace(searchValue)) return result;

        var stack = new CustomStack<ErenDomNode>();
        stack.Push(tree.Root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (IsMatch(current, searchKey, searchValue))
            {
                result.Add(current);
            }

            // Stack'e eklerken sağdan sola eklersek, çıkarırken soldan sağa işlenmiş olur
            foreach (var child in current.Children)
            {
                stack.Push(child);
            }
        }

        return result;
    }

    // ===================================================================
    // Rekürsif Ağaç Analiz Algoritmaları (Faz 2 Gereksinimleri)
    // ===================================================================

    /// <summary>
    /// Ağacın (veya bir alt ağacın) derinliğini rekürsif olarak hesaplar.
    /// Yaprak düğümün derinliği 0'dır. Her seviye yukarı çıkıldığında 1 eklenir.
    /// 
    /// Zaman Karmaşıklığı: O(N) — her düğüm bir kez ziyaret edilir
    /// Uzay Karmaşıklığı: O(D) — rekürsiyon yığını derinliği kadar yer kaplar
    /// </summary>
    public static int CalculateDepth(ErenDomNode node)
    {
        if (node == null || node.Children.Count == 0)
            return 0; // Yaprak düğüm veya null — derinlik 0

        int maxChildDepth = 0;
        foreach (var child in node.Children)
        {
            int childDepth = CalculateDepth(child);
            if (childDepth > maxChildDepth)
                maxChildDepth = childDepth;
        }

        return maxChildDepth + 1; // En derin çocuğun derinliği + 1 (kendisi)
    }

    /// <summary>
    /// Bir düğümün altındaki (alt ağaç) toplam düğüm sayısını rekürsif olarak hesaplar.
    /// Kendisini de sayar (1 + çocukların altındakiler).
    /// 
    /// Zaman Karmaşıklığı: O(N) — her düğüm bir kez ziyaret edilir
    /// Uzay Karmaşıklığı: O(D) — rekürsiyon yığını
    /// </summary>
    public static int CountNodes(ErenDomNode node)
    {
        if (node == null)
            return 0;

        int count = 1; // Kendisi
        foreach (var child in node.Children)
        {
            count += CountNodes(child); // + tüm alt ağaçtaki düğümler
        }

        return count;
    }
}
