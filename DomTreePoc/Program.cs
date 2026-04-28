using DomTreePoc.Parsing;

namespace DomTreePoc;

internal static class Program {
    private static void Main() {
        var html = """
        <html id="root">
          <body class="page">
            <main id="content" class="layout">
              <section id="intro" class="card featured">
                <h1 class="title"></h1>
                <p class="text"></p>
              </section>
              <section id="list" class="card">
                <article id="item-1" class="item"></article>
                <article id="item-2" class="item featured"></article>
              </section>
            </main>
          </body>
        </html>
        """;

        var document = HtmlParser.Parse(html);

        Console.WriteLine("DOM agaci:\n");
        document.PrintTree();
    }
}
