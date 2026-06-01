using Microsoft.AspNetCore.Mvc;
using DomEngine.Core.Parser;
using DomEngine.Core.Topology;
using DomEngine.Core.Algorithms;
using DomEngine.Core.DataStructures;
using System.Collections.Generic;

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
        public string Query { get; set; } = string.Empty; // id="main" veya class="container" veya tag="div" gibi
        public string SearchType { get; set; } = "id"; // id, class, tag
    }

    [HttpPost("parse")]
    public IActionResult ParseHtml([FromBody] ParseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HtmlContent))
        {
            return BadRequest("HTML içeriği boş olamaz.");
        }

        NaryTree tree;
        try
        {
            var parser = new HtmlParser();
            tree = parser.Parse(request.HtmlContent);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }

        // Kendi yazdığımız yapıları JSON olarak sorunsuz gönderebilmek için DTO'ya çeviriyoruz
        var dto = MapToDto(tree.Root);
        return Ok(dto);
    }

    [HttpPost("search")]
    public IActionResult Search([FromBody] SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HtmlContent) || string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Geçersiz arama isteği.");
        }

        NaryTree tree;
        try
        {
            var parser = new HtmlParser();
            tree = parser.Parse(request.HtmlContent);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }

        CustomList<DomNode> searchResults = new CustomList<DomNode>();

        if (request.SearchType == "id")
        {
            // O(1) arama
            var node = tree.GetElementById(request.Query);
            if (node != null)
                searchResults.Add(node);
        }
        else if (request.SearchType == "dfs")
        {
            searchResults = DomSearch.SearchDFS(tree, request.Query);
        }
        else if (request.SearchType == "bfs")
        {
            searchResults = DomSearch.SearchBFS(tree, request.Query);
        }

        var dtos = new List<object>();
        foreach (var node in searchResults)
        {
            dtos.Add(MapToDto(node));
        }

        return Ok(dtos);
    }

    // CustomTree -> JSON dostu DTO çevirici
    private object MapToDto(DomNode node)
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
