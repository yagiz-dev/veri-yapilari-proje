using System;
using System.Collections.Generic;

namespace Program
{
    class Program
{
    static void Main()
    {
            static void Main()
            {
                // 1. Kök düğümü (Root) oluştur: <html>
                HtmlNode rootNode = new HtmlNode("html");

                // 2. <head> ve <body> düğümlerini oluştur
                HtmlNode headNode = new HtmlNode("head");
                HtmlNode bodyNode = new HtmlNode("body");

                // Bunları kök düğüme (html) çocuk olarak ekle
                rootNode.AddChild(headNode);
                rootNode.AddChild(bodyNode);

                // 3. <body> içine bir <div> ekle
                HtmlNode divNode = new HtmlNode("div");
                divNode.Attributes.Add("id", "main-container"); // Özellik ekleme
                divNode.Attributes.Add("class", "flex");
                bodyNode.AddChild(divNode);

                // 4. <div> içine <h1> ve <p> ekle (N-ary: İstediğin kadar çocuk eklenebilir)
                HtmlNode h1Node = new HtmlNode("h1");
                h1Node.InnerText = "DOM Projesine Hoş Geldiniz";

                HtmlNode pNode = new HtmlNode("p");
                pNode.InnerText = "Bu paragraf div içindedir.";

                divNode.AddChild(h1Node);
                divNode.AddChild(pNode);

                // Ağacı konsola yazdırarak test et
                rootNode.PrintNode();
            }
            rootNode.PrintNode();
    }
}
    // N-ary Tree'nin her bir düğümünü temsil eden sınıf
    public class HtmlNode
    {
        public string Tag { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        // DÜZELTME 1: Parent null olabilir (kök düğümün ebeveyni yoktur). 
        // Bu yüzden HtmlNode? şeklinde tanımlıyoruz.
        public HtmlNode? Parent { get; set; }

        public List<HtmlNode> Children { get; set; }
        public string InnerText { get; set; }

        // DÜZELTME 2: Parametredeki parent değerinin null olabileceğini belirtmek için
        // HtmlNode? parent = null şeklinde güncelledik.
        public HtmlNode(string tag, HtmlNode? parent = null)
        {
            Tag = tag;
            Parent = parent;
            Children = new List<HtmlNode>();
            Attributes = new Dictionary<string, string>();
            InnerText = string.Empty;
        }

        public void AddChild(HtmlNode child)
        {
            child.Parent = this; 
            Children.Add(child); 
        }

        public void PrintNode(int depth = 0)
        {
            string indent = new string('-', depth * 2);
            Console.WriteLine($"{indent} <{Tag}> (Çocuk Sayısı: {Children.Count})");
            if (!string.IsNullOrWhiteSpace(InnerText))
            {
               Console.Write($" => İçerik: \"{InnerText}\"");
            }
            foreach (var child in Children)
            {
                child.PrintNode(depth + 1); 
            }
        }
    }
}