using Microsoft.AspNetCore.Mvc;
using DomEngine.Core.Parser;
using DomEngine.Core.Topology;
using DomEngine.Core.Algorithms;
using DomEngine.Core.DataStructures;
using System.Collections.Generic;
using System.Diagnostics;

namespace DomEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParserController : ControllerBase
{
    // Frontend'den gelecek istek gövdesi
    public class ParseRequest
    {
        public string HtmlContent { get; set; } = string.Empty;
    }

    // Arama isteği gövdesi
    public class SearchRequest
    {
        public string HtmlContent { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;     // ID için: "header" | DFS/BFS için: class="container"
        public string SearchType { get; set; } = "id";        // "id", "dfs" veya "bfs"
    }

    [HttpPost("parse")]
    public IActionResult ParseHtml([FromBody] ParseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HtmlContent))
        {
            return BadRequest("HTML içeriği boş olamaz.");
        }

        // Stopwatch ile süre ölçümü
        var watch = Stopwatch.StartNew();

        ErenNaryTree tree;
        try
        {
            var parser = new HtmlParser();
            tree = parser.Parse(request.HtmlContent);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }

        watch.Stop();

        // Kendi yazdığımız yapıları JSON olarak sorunsuz gönderebilmek için DTO'ya çeviriyoruz
        var dto = MapToDto(tree.Root);

        return Ok(new
        {
            tree = dto,
            totalNodes = YusufPehDomSearch.CountNodes(tree.Root),
            treeDepth = YusufPehDomSearch.CalculateDepth(tree.Root),
            elapsedMs = watch.Elapsed.TotalMilliseconds
        });
    }

    [HttpPost("search")]
    public IActionResult Search([FromBody] SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HtmlContent) || string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Geçersiz arama isteği.");
        }

        ErenNaryTree tree;
        try
        {
            var parser = new HtmlParser();
            tree = parser.Parse(request.HtmlContent);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }

        // Stopwatch ile süre ölçümü
        var watch = Stopwatch.StartNew();

        ArdaList<ErenDomNode> searchResults = new ArdaList<ErenDomNode>();

        if (request.SearchType == "id")
        {
            // Hash Table ile O(1) arama — kullanıcı sadece ID değerini yazar (örn: "header")
            var node = tree.GetElementById(request.Query);
            if (node != null)
                searchResults.Add(node);
        }
        else
        {
            // DFS veya BFS ile O(N) arama — kullanıcı key="value" formatında yazar
            // Örn: class="container" → searchKey = "class", searchValue = "container"
            var parts = request.Query.Split('=', 2); // En fazla 2 parçaya böl
            if (parts.Length != 2)
            {
                return BadRequest("Sorgu formatı hatalı. Örn: class=\"container\" veya tag=\"div\"");
            }

            string searchKey = parts[0].Trim().ToLower();
            string searchValue = parts[1].Trim().Trim('"').Trim('\''); // Tırnakları temizle

            if (request.SearchType == "dfs")
            {
                searchResults = YusufPehDomSearch.SearchDFS(tree, searchKey, searchValue);
            }
            else // bfs
            {
                searchResults = YusufPehDomSearch.SearchBFS(tree, searchKey, searchValue);
            }
        }

        watch.Stop();

        var dtos = new List<object>();
        foreach (var node in searchResults)
        {
            dtos.Add(MapToDto(node));
        }

        return Ok(new
        {
            results = dtos,
            count = dtos.Count,
            elapsedMs = watch.Elapsed.TotalMilliseconds
        });
    }

    // CustomTree -> JSON dostu DTO çevirici
    private object MapToDto(ErenDomNode node)
    {
        if (node == null) return null;

        var attributes = new Dictionary<string, string>();
        
        var allPairs = node.Attributes.GetAllPairs();
        foreach (var pair in allPairs)
        {
            attributes.TryAdd(pair.Key, pair.Value);
        }

        var children = new List<object>();
        foreach (var child in node.Children)
        {
            children.Add(MapToDto(child));
        }

        return new
        {
            TagName = node.TagName,
            Id = node.Id,
            ClassName = node.ClassName,
            InnerText = node.InnerText,
            Attributes = attributes,
            Children = children
        };
    }
}

