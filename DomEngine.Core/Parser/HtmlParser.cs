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
    private readonly string[] _selfClosingTags = { "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr" };

    public NaryTree Parse(string html)
    {
        var tree = new NaryTree("document");
        var stack = new CustomStack<DomNode>();
        var tagStartIndexes = new CustomStack<int>();
        stack.Push(tree.Root);
        tagStartIndexes.Push(0);

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
                    if (commentEnd == -1)
                    {
                        ThrowHtmlError("HTML yorumu kapatılmamış.", html, i);
                    }

                    i = commentEnd + 3;
                    continue;
                }

                // <!DOCTYPE html> gibi bildirimleri DOM dugumu olarak eklemiyoruz.
                if (i + 1 < html.Length && html[i + 1] == '!')
                {
                    int declarationEnd = FindTagEnd(html, i);
                    if (declarationEnd == -1)
                    {
                        ThrowHtmlError("HTML bildirimi kapatılmamış.", html, i);
                    }

                    string declaration = html.Substring(i + 2, declarationEnd - i - 2).Trim();
                    if (string.IsNullOrWhiteSpace(declaration))
                    {
                        ThrowHtmlError("HTML bildirimi boş olamaz.", html, i);
                    }

                    i = declarationEnd + 1;
                    continue;
                }

                // 3. Kapanış etiketi mi? (</tag>)
                if (i + 1 < html.Length && html[i + 1] == '/')
                {
                    int closeEnd = html.IndexOf('>', i);
                    if (closeEnd == -1)
                    {
                        ThrowHtmlError("Kapanış etiketi '>' ile kapatılmamış.", html, i);
                    }

                    string rawTagName = html.Substring(i + 2, closeEnd - i - 2);
                    string tagName = rawTagName.Trim().ToLower();

                    if (rawTagName.Length == 0 || char.IsWhiteSpace(rawTagName[0]) || ContainsWhitespace(tagName) || !IsValidTagName(tagName))
                    {
                        ThrowHtmlError($"Geçersiz kapanış etiketi sözdizimi: </{tagName}>.", html, i);
                    }

                    if (stack.Count <= 1)
                    {
                        ThrowHtmlError($"Beklenmeyen kapanış etiketi: </{tagName}>.", html, i);
                    }

                    var top = stack.Peek();
                    if (top.TagName != tagName)
                    {
                        ThrowHtmlError($"Kapanış etiketi eşleşmiyor: </{tagName}>. Beklenen: </{top.TagName}>.", html, i);
                    }

                    stack.Pop();
                    tagStartIndexes.Pop();
                    i = closeEnd + 1;
                    continue;
                }

                // 4. Açılış etiketi (<tag attr="value">)
                int tagEnd = FindTagEnd(html, i);

                if (tagEnd == -1)
                {
                    ThrowHtmlError("Açılış etiketi '>' ile kapatılmamış.", html, i);
                }

                string rawTagContent = html.Substring(i + 1, tagEnd - i - 1);
                int tagContentStart = i + 1;
                while (tagContentStart < tagEnd && char.IsWhiteSpace(html[tagContentStart]))
                {
                    tagContentStart++;
                }

                string tagContent = rawTagContent.Trim();

                if (string.IsNullOrWhiteSpace(tagContent))
                {
                    ThrowHtmlError("Açılış etiketi boş olamaz.", html, i);
                }

                // /> ile biten tag'ler için temizlik
                bool isSelfClosingImplicit = tagContent.EndsWith("/");
                if (isSelfClosingImplicit)
                {
                    tagContent = tagContent.Substring(0, tagContent.Length - 1).TrimEnd();
                }

                if (string.IsNullOrWhiteSpace(tagContent))
                {
                    ThrowHtmlError("Açılış etiketi boş olamaz.", html, i);
                }

                // Yeni DomNode oluştur
                DomNode newNode = new DomNode("");
                
                // Sınıf içindeki metotla TagName ve Nitelikleri (Attributes) ayrıştır
                ParseTagAttributes(tagContent, newNode, html, tagContentStart);
                
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
                    tagStartIndexes.Push(i);
                }

                i = tagEnd + 1;
                continue;
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

        if (stack.Count > 1)
        {
            ThrowHtmlError($"Kapatılmamış etiket: <{stack.Peek().TagName}>.", html, tagStartIndexes.Peek());
        }

        return tree;
    }

    private int FindTagEnd(string html, int tagStart)
    {
        bool inQuotes = false;
        char currentQuote = '\0';

        for (int j = tagStart + 1; j < html.Length; j++)
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
            else if (!inQuotes && html[j] == '<')
            {
                ThrowHtmlError("Etiket kapanmadan önce beklenmeyen '<' karakteri bulundu.", html, j);
            }
            else if (!inQuotes && html[j] == '>')
            {
                return j;
            }
        }

        return -1;
    }

    private void ThrowHtmlError(string message, string html, int index)
    {
        throw new FormatException($"{message} Satır: {GetLineNumber(html, index)}.");
    }

    private int GetLineNumber(string html, int index)
    {
        int line = 1;
        int safeIndex = Math.Clamp(index, 0, Math.Max(html.Length - 1, 0));

        for (int i = 0; i < safeIndex; i++)
        {
            if (html[i] == '\n')
            {
                line++;
            }
        }

        return line;
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
    private void ParseTagAttributes(string tagContent, DomNode node, string html, int tagContentStart)
    {
        tagContent = tagContent.Trim();

        int index = 0;
        int tagNameStart = index;
        while (index < tagContent.Length && !char.IsWhiteSpace(tagContent[index]))
        {
            index++;
        }

        string tagName = tagContent.Substring(tagNameStart, index - tagNameStart).ToLower();
        if (!IsValidTagName(tagName))
        {
            ThrowHtmlError($"Geçersiz açılış etiketi sözdizimi: <{tagName}>.", html, tagContentStart + tagNameStart);
        }

        node.TagName = tagName;

        // Attribute ayrıştırma (Karakter tabanlı basit State Machine)
        while (index < tagContent.Length)
        {
            // Boşlukları geç
            while (index < tagContent.Length && char.IsWhiteSpace(tagContent[index])) index++;
            if (index >= tagContent.Length) break;

            // Key'i oku (örn: id, class)
            int keyStart = index;
            while (index < tagContent.Length && tagContent[index] != '=' && !char.IsWhiteSpace(tagContent[index]))
            {
                index++;
            }

            string key = tagContent.Substring(keyStart, index - keyStart).ToLower();
            if (!IsValidAttributeName(key))
            {
                ThrowHtmlError($"<{node.TagName}> etiketi içinde geçersiz attribute sözdizimi: {key}.", html, tagContentStart + keyStart);
            }

            while (index < tagContent.Length && char.IsWhiteSpace(tagContent[index])) index++;

            // Value'yu oku (örn: "main")
            string value = "";
            if (index < tagContent.Length && tagContent[index] == '=')
            {
                index++; // '=' işaretini atla
                while (index < tagContent.Length && char.IsWhiteSpace(tagContent[index])) index++;
                if (index >= tagContent.Length)
                {
                    ThrowHtmlError($"<{node.TagName}> etiketi içindeki '{key}' attribute değeri eksik.", html, tagContentStart + keyStart);
                }
                
                // Tırnak (quote) kontrolü (" veya ')
                if (tagContent[index] == '"' || tagContent[index] == '\'')
                {
                    char quote = tagContent[index];
                    index++; // açılış tırnağını atla
                    int valStart = index;
                    while (index < tagContent.Length)
                    {
                        if (tagContent[index] == '\\' && index + 1 < tagContent.Length)
                        {
                            index += 2; // Escape karakterini ve sonrasındaki tırnağı atla
                            continue;
                        }
                        if (tagContent[index] == quote)
                        {
                            break;
                        }
                        index++;
                    }

                    if (index >= tagContent.Length || tagContent[index] != quote)
                    {
                        ThrowHtmlError($"<{node.TagName}> etiketi içindeki '{key}' attribute değeri kapatılmamış.", html, tagContentStart + valStart - 1);
                    }

                    value = tagContent.Substring(valStart, index - valStart);
                    index++; // kapanış tırnağını atla
                }
                else
                {
                    // Tırnak yoksa boşluğa kadar oku
                    int valStart = index;
                    while (index < tagContent.Length && !char.IsWhiteSpace(tagContent[index]))
                    {
                        if (tagContent[index] == '"' || tagContent[index] == '\'' || tagContent[index] == '<' || tagContent[index] == '>' || tagContent[index] == '=')
                        {
                            ThrowHtmlError($"<{node.TagName}> etiketi içindeki '{key}' attribute için geçersiz tırnaksız değer.", html, tagContentStart + index);
                        }

                        index++;
                    }

                    if (valStart == index)
                    {
                        ThrowHtmlError($"<{node.TagName}> etiketi içindeki '{key}' attribute değeri eksik.", html, tagContentStart + keyStart);
                    }

                    value = tagContent.Substring(valStart, index - valStart);
                }
            }

            if (node.Attributes.ContainsKey(key))
            {
                ThrowHtmlError($"<{node.TagName}> etiketi içinde tekrarlı attribute: {key}.", html, tagContentStart + keyStart);
            }

            node.Attributes.Add(key, value); // Özel HashTable'ımıza ekliyoruz O(1)
        }
    }

    private bool IsValidTagName(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName) || !char.IsLetter(tagName[0]))
        {
            return false;
        }

        for (int i = 1; i < tagName.Length; i++)
        {
            char current = tagName[i];
            if (!char.IsLetterOrDigit(current) && current != '-' && current != '_' && current != ':')
            {
                return false;
            }
        }

        return true;
    }

    private bool ContainsWhitespace(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsWhiteSpace(value[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidAttributeName(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            return false;
        }

        for (int i = 0; i < attributeName.Length; i++)
        {
            char current = attributeName[i];
            if (char.IsWhiteSpace(current) || current == '=' || current == '"' || current == '\'' || current == '<' || current == '>' || current == '/')
            {
                return false;
            }
        }

        return true;
    }
}
