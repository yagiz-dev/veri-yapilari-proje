using System.Text;
using DomEngine.Core.DataStructures;
using DomEngine.Core.Topology;

namespace DomEngine.Core.Parser;

/// <summary>
/// Gelen HTML metnini tarayıp (tokenization) parçalara ayıran ve
/// CustomStack kullanarak NaryTree (DOM Ağacı) oluşturan motor.
/// </summary>
public class HtmlParser
{
    // Kendi kendine kapanan (self-closing) etiketler. Bunlar stack'e atılmaz.
    private readonly string[] _selfClosingTags = { "img", "br", "hr", "input", "meta", "link" };

    public NaryTree Parse(string html)
    {
        var tree = new NaryTree("document");
        var stack = new CustomStack<DomNode>();
        stack.Push(tree.Root);

        if (string.IsNullOrWhiteSpace(html))
            return tree;

        int i = 0;
        StringBuilder textBuffer = new StringBuilder();

        while (i < html.Length)
        {
            if (html[i] == '<')
            {
                // 1. Önceki birikmiş metni (Inner Text) mevcut düğüme kaydet
                if (textBuffer.Length > 0 && stack.Count > 0)
                {
                    string text = textBuffer.ToString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        // Metnin arasına boşluk bırakarak ekleyelim
                        stack.Peek().InnerText += text.Trim() + " "; 
                    }
                    textBuffer.Clear();
                }

                // 2. Yorum satırı kontrolü (<!-- ... -->)
                if (i + 3 < html.Length && html[i + 1] == '!' && html[i + 2] == '-' && html[i + 3] == '-')
                {
                    int commentEnd = html.IndexOf("-->", i);
                    if (commentEnd != -1)
                        i = commentEnd + 3;
                    else
                        i = html.Length;
                    continue;
                }

                // 3. Kapanış etiketi mi? (</tag>)
                if (i + 1 < html.Length && html[i + 1] == '/')
                {
                    int closeEnd = html.IndexOf('>', i);
                    if (closeEnd != -1)
                    {
                        string tagName = html.Substring(i + 2, closeEnd - i - 2).Trim().ToLower();
                        
                        // Kök düğümü (document) hiçbir zaman stack'ten çıkarmamalıyız
                        if (stack.Count > 1) 
                        {
                            var top = stack.Pop();
                            // Hatalı HTML'e karşı (kapanmayan tag vs.) okul projesi seviyesinde
                            // katı bir kontrol yapmadan pop ediyoruz. LIFO mantığı burada tam uyuyor.
                        }
                        i = closeEnd + 1;
                        continue;
                    }
                }

                // 4. Açılış etiketi (<tag attr="value">)
                int tagEnd = -1;
                bool inQuotes = false;
                char currentQuote = '\0';
                for (int j = i + 1; j < html.Length; j++)
                {
                    if (inQuotes && html[j] == '\\' && j + 1 < html.Length)
                    {
                        j++; // Escape (\) karakterini gördüysek sonraki karakteri (örn: ") atla
                        continue;
                    }

                    if (!inQuotes && (html[j] == '"' || html[j] == '\''))
                    {
                        inQuotes = true;
                        currentQuote = html[j];
                    }
                    else if (inQuotes && html[j] == currentQuote)
                    {
                        inQuotes = false;
                    }
                    else if (!inQuotes && html[j] == '>')
                    {
                        tagEnd = j;
                        break;
                    }
                }

                if (tagEnd != -1)
                {
                    string tagContent = html.Substring(i + 1, tagEnd - i - 1);
                    
                    // /> ile biten tag'ler için temizlik
                    bool isSelfClosingImplicit = tagContent.EndsWith("/");
                    if (isSelfClosingImplicit)
                    {
                        tagContent = tagContent.TrimEnd('/');
                    }

                    // Yeni DomNode oluştur
                    DomNode newNode = new DomNode("");
                    
                    // Sınıf içindeki metotla TagName ve Nitelikleri (Attributes) ayrıştır
                    ParseTagAttributes(tagContent, newNode);
                    
                    // ID'si varsa O(1) arama için NaryTree'nin Hash Table'ına kaydet
                    tree.RegisterNode(newNode);
                    
                    // Ebeveynine bağla
                    if (stack.Count > 0)
                    {
                        stack.Peek().AddChild(newNode);
                    }

                    // Kendi kendini kapatan tag değilse (örn: <div>) stack'e ekle (Çocukları olabilir)
                    bool isSelfClosing = isSelfClosingImplicit || IsSelfClosingTag(newNode.TagName);
                    if (!isSelfClosing)
                    {
                        stack.Push(newNode);
                    }

                    i = tagEnd + 1;
                    continue;
                }
            }
            else
            {
                // Normal metin (Inner Text) karakterleri
                textBuffer.Append(html[i]);
                i++;
            }
        }

        // 5. Belgenin en sonunda kalan metin varsa son düğüme ekle
        if (textBuffer.Length > 0 && stack.Count > 0)
        {
            string text = textBuffer.ToString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                stack.Peek().InnerText += text.Trim();
            }
        }

        return tree;
    }

    private bool IsSelfClosingTag(string tagName)
    {
        for (int i = 0; i < _selfClosingTags.Length; i++)
        {
            if (_selfClosingTags[i] == tagName) return true;
        }
        return false;
    }

    /// <summary>
    /// Karakter karakter (tokenization) tarama işlemiyle nitelikleri (Attributes) ayrıştırır.
    /// Örn: div id="main" class="box"
    /// </summary>
    private void ParseTagAttributes(string tagContent, DomNode node)
    {
        tagContent = tagContent.Trim();
        int firstSpace = tagContent.IndexOf(' ');

        if (firstSpace == -1)
        {
            // Hiç attribute yoksa direkt tag ismidir
            node.TagName = tagContent.ToLower();
            return;
        }

        node.TagName = tagContent.Substring(0, firstSpace).ToLower();
        string attrString = tagContent.Substring(firstSpace + 1).Trim();

        // Attribute ayrıştırma (Karakter tabanlı basit State Machine)
        int index = 0;
        while (index < attrString.Length)
        {
            // Boşlukları geç
            while (index < attrString.Length && char.IsWhiteSpace(attrString[index])) index++;
            if (index >= attrString.Length) break;

            // Key'i oku (örn: id, class)
            int keyStart = index;
            while (index < attrString.Length && attrString[index] != '=' && !char.IsWhiteSpace(attrString[index]))
            {
                index++;
            }
            string key = attrString.Substring(keyStart, index - keyStart).ToLower();

            // Value'yu oku (örn: "main")
            string value = "";
            if (index < attrString.Length && attrString[index] == '=')
            {
                index++; // '=' işaretini atla
                
                // Tırnak (quote) kontrolü (" veya ')
                if (index < attrString.Length && (attrString[index] == '"' || attrString[index] == '\''))
                {
                    char quote = attrString[index];
                    index++; // açılış tırnağını atla
                    int valStart = index;
                    while (index < attrString.Length)
                    {
                        if (attrString[index] == '\\' && index + 1 < attrString.Length)
                        {
                            index += 2; // Escape karakterini ve sonrasındaki tırnağı atla
                            continue;
                        }
                        if (attrString[index] == quote)
                        {
                            break;
                        }
                        index++;
                    }
                    value = attrString.Substring(valStart, index - valStart);
                    index++; // kapanış tırnağını atla
                }
                else
                {
                    // Tırnak yoksa boşluğa kadar oku
                    int valStart = index;
                    while (index < attrString.Length && !char.IsWhiteSpace(attrString[index]))
                    {
                        index++;
                    }
                    value = attrString.Substring(valStart, index - valStart);
                }
            }

            if (!string.IsNullOrEmpty(key))
            {
                node.Attributes.Add(key, value); // Özel HashTable'ımıza ekliyoruz O(1)
            }
        }
    }
}
