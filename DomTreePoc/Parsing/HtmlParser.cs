using DomTreePoc.Collections;
using DomTreePoc.Models;

namespace DomTreePoc.Parsing;

internal static class HtmlParser
{
    public static DomDocument Parse(string html)
    {
        var document = new DomDocument();
        var stack = new NodeStack();

        for (var i = 0; i < html.Length; i++) {
            if (html[i] != '<') {
                continue;
            }

            var tagEnd = html.IndexOf('>', i + 1);
            var tagContent = html[(i + 1)..tagEnd].Trim();
            i = tagEnd;

            if (tagContent.Length == 0) {
                continue;
            }

            if (tagContent[0] == '/') {
                stack.Pop();
                continue;
            }

            var selfClosing = tagContent.EndsWith('/');
            if (selfClosing) {
                tagContent = tagContent.Substring(0, tagContent.Length - 1).TrimEnd();
            }

            var node = CreateNode(tagContent);
            var parent = stack.IsEmpty ? document.Root : stack.Peek();
            parent.AddChild(node);

            if (!selfClosing) {
                stack.Push(node);
            }
        }

        return document;
    }

    private static DomNode CreateNode(string tagContent) {
        var tagName = ReadTagName(tagContent);

        return new DomNode(tagName.ToLowerInvariant());
    }

    private static string ReadTagName(string tagContent) {
        var end = 0;
        while (end < tagContent.Length && !char.IsWhiteSpace(tagContent[end])) {
            end++;
        }

        return tagContent[..end];
    }
}
