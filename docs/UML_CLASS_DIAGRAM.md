# DomEngine — UML Sınıf Diyagramı (Class Diagram)

> Bu diyagram, UML 2.0 standartlarına uygun olarak hazırlanmıştır. Tüm sınıflar, alanlar (attributes), metotlar (operations), erişim belirteçleri (visibility), generic tipler ve sınıflar arası ilişkiler (association, composition, dependency, realization) akademik notasyona sadık kalınarak gösterilmiştir.

---

## Sınıf Diyagramı

```mermaid
classDiagram
    direction TB

    %% ══════════════════════════════════════════════
    %% KATMAN 1 — DataStructures (Veri Yapıları)
    %% ══════════════════════════════════════════════

    class `Node~T~` {
        +T Data
        +Node~T~? Next
        +Node~T~? Prev
        +Node(T data)
    }

    class `HashNode~K,V~` {
        +K Key
        +V Value
        +HashNode~K,V~? Next
        +HashNode(K key, V value)
    }

    class `CustomList~T~` {
        -Node~T~? _head
        -Node~T~? _tail
        -int _count
        +int Count
        +T this[int index]
        +Add(T item) void
        +Remove(T item) bool
        +Clear() void
        +GetEnumerator() IEnumerator~T~
        -GetNodeAt(int index) Node~T~
    }

    class `CustomStack~T~` {
        -Node~T~? _top
        -int _count
        +int Count
        +Push(T item) void
        +Pop() T
        +Peek() T
    }

    class `CustomQueue~T~` {
        -Node~T~? _head
        -Node~T~? _tail
        -int _count
        +int Count
        +Enqueue(T item) void
        +Dequeue() T
        +Peek() T
    }

    class `CustomHashTable~K,V~` {
        -int capacity
        -HashNode~K,V~?[] _buckets
        -int _count
        +V this[K key]
        +Add(K key, V value) void
        +Remove(K key) void
        +TryGetValue(K key, out V value) bool
        +ContainsKey(K key) bool
        +GetAllPairs() CustomList~KeyValuePair~K,V~~
        -GetCustomHash(K key) int
        -GetBucketIndex(K key) int
        -ReHash() HashNode~K,V~[]
    }

    class `IEnumerable~T~` {
        <<interface>>
        +GetEnumerator() IEnumerator~T~
    }

    %% ══════════════════════════════════════════════
    %% KATMAN 2 — Topology (Ağaç Topolojisi)
    %% ══════════════════════════════════════════════

    class DomNode {
        +string TagName
        +string InnerText
        +DomNode? Parent
        +CustomList~DomNode~ Children
        +CustomHashTable~string，string~ Attributes
        +string Id
        +string ClassName
        +DomNode(string tagName)
        +AddChild(DomNode child) void
    }

    class NaryTree {
        +DomNode Root
        -CustomHashTable~string，DomNode~ _elementsById
        +NaryTree(string rootTagName)
        +RegisterNode(DomNode node) void
        +GetElementById(string id) DomNode?
        +AddNode(DomNode parent, DomNode child) void
        +RemoveNode(DomNode nodeToRemove) bool
    }

    %% ══════════════════════════════════════════════
    %% KATMAN 3 — Parser
    %% ══════════════════════════════════════════════

    class HtmlParser {
        -string[] _selfClosingTags
        +Parse(string html) NaryTree
        -FindTagEnd(string html, int tagStart) int
        -ThrowHtmlError(string msg, string html, int index) void
        -GetLineNumber(string html, int index) int
        -IsSelfClosingTag(string tagName) bool
        -ParseTagAttributes(string tagContent, DomNode node, string html, int start) void
        -IsValidTagName(string tagName) bool
        -ContainsWhitespace(string value) bool
        -IsValidAttributeName(string name) bool
    }

    %% ══════════════════════════════════════════════
    %% KATMAN 4 — Algorithms
    %% ══════════════════════════════════════════════

    class DomSearch {
        <<static>>
        -IsMatch(DomNode node, string key, string value)$ bool
        +SearchBFS(NaryTree tree, string key, string value)$ CustomList~DomNode~
        +SearchDFS(NaryTree tree, string key, string value)$ CustomList~DomNode~
        +CalculateDepth(DomNode node)$ int
        +GetSiblings(DomNode node)$ CustomList~DomNode~
        +CountNodes(DomNode node)$ int
    }

    %% ══════════════════════════════════════════════
    %% KATMAN 5 — API (Presentation Layer)
    %% ══════════════════════════════════════════════

    class ParserController {
        +ParseHtml(ParseRequest request) IActionResult
        +Search(SearchRequest request) IActionResult
        -MapToDto(DomNode node) object
    }

    class ParseRequest {
        +string HtmlContent
    }

    class SearchRequest {
        +string HtmlContent
        +string Query
        +string SearchType
    }

    class ControllerBase {
        <<abstract>>
    }

    %% ══════════════════════════════════════════════
    %%                 İLİŞKİLER
    %% ══════════════════════════════════════════════

    %% — Composition (Güçlü sahiplik: parça, bütün olmadan yaşayamaz) —
    `CustomList~T~` *-- `Node~T~` : içerir (head, tail)
    `CustomStack~T~` *-- `Node~T~` : içerir (top)
    `CustomQueue~T~` *-- `Node~T~` : içerir (head, tail)
    `CustomHashTable~K,V~` *-- `HashNode~K,V~` : içerir (buckets[])
    NaryTree *-- DomNode : kök düğümü sahiplenir (Root)

    %% — Composition (DomNode kendi çocuklarını sahiplenir) —
    DomNode *-- DomNode : Children

    %% — Association (DomNode nitelikleri ve çocukları bu yapılarla tutar) —
    DomNode --> `CustomList~DomNode~` : Children
    DomNode --> `CustomHashTable~string，string~` : Attributes
    NaryTree --> `CustomHashTable~string，DomNode~` : _elementsById

    %% — Realization (Interface implementasyonu) —
    `CustomList~T~` ..|> `IEnumerable~T~` : implements

    %% — Dependency (Kullanım bağımlılıkları) —
    HtmlParser ..> NaryTree : oluşturur «create»
    HtmlParser ..> DomNode : oluşturur «create»
    HtmlParser ..> `CustomStack~T~` : kullanır «use»

    DomSearch ..> NaryTree : arama yapar «use»
    DomSearch ..> DomNode : analiz eder «use»
    DomSearch ..> `CustomQueue~T~` : BFS için «use»
    DomSearch ..> `CustomStack~T~` : DFS için «use»
    DomSearch ..> `CustomList~T~` : sonuç döner «use»

    ParserController ..> HtmlParser : kullanır «use»
    ParserController ..> NaryTree : kullanır «use»
    ParserController ..> DomSearch : çağırır «use»

    %% — Inheritance (Kalıtım) —
    ParserController --|> ControllerBase : extends

    %% — Nesting (İç sınıflar) —
    ParserController *-- ParseRequest : inner class
    ParserController *-- SearchRequest : inner class
```

---

## Notasyon Kılavuzu (Legend)

| Sembol | Anlamı | UML Terimi |
|--------|--------|------------|
| `+` | Public erişim | Public visibility |
| `-` | Private erişim | Private visibility |
| `$` (metot sonrası) | Static metot | Static operation |
| `<<static>>` | Statik sınıf stereotipi | Stereotype |
| `<<interface>>` | Arayüz stereotipi | Stereotype |
| `<<abstract>>` | Soyut sınıf stereotipi | Stereotype |
| `~T~` | Generic tip parametresi | Template parameter |
| `◆ (*--)` | **Composition** — Parça, bütün olmadan yaşayamaz | Filled diamond |
| `→ (-->)` | **Association** — Kalıcı referans ilişkisi | Directed association |
| `⇢ (..>)` | **Dependency** — Geçici kullanım bağımlılığı | Dashed arrow |
| `⇢▷ (..\|>)` | **Realization** — Interface implementasyonu | Dashed triangle |
| `→▷ (--\|>)` | **Generalization** — Kalıtım (Inheritance) | Solid triangle |

---

## Paket (Package) Organizasyonu

```mermaid
graph TB
    subgraph "DomEngine.Api — Sunum Katmanı"
        PC["ParserController"]
    end

    subgraph "DomEngine.Core — İş Mantığı Katmanı"
        subgraph "Algorithms"
            DS["DomSearch «static»"]
        end
        subgraph "Parser"
            HP["HtmlParser"]
        end
        subgraph "Topology"
            DN["DomNode"]
            NT["NaryTree"]
        end
        subgraph "DataStructures"
            CL["CustomList‹T›"]
            CS["CustomStack‹T›"]
            CQ["CustomQueue‹T›"]
            CHT["CustomHashTable‹K,V›"]
            ND["Node‹T›"]
            HN["HashNode‹K,V›"]
        end
    end

    PC --> DS
    PC --> HP
    HP --> NT
    HP --> DN
    HP --> CS
    DS --> NT
    DS --> CQ
    DS --> CS
    DS --> CL
    DN --> CL
    DN --> CHT
    NT --> CHT
    CL --> ND
    CS --> ND
    CQ --> ND
    CHT --> HN
```

---

## Multiplicity (Çokluk) Tablosu

| İlişki Türü | Kaynak Sınıf | Hedef Sınıf | Çokluk | Açıklama |
|-------------|-------------|------------|--------|----------|
| Composition | `CustomList<T>` | `Node<T>` | 1 → 0..* | Liste sıfır veya daha fazla düğüm içerir |
| Composition | `CustomStack<T>` | `Node<T>` | 1 → 0..* | Stack sıfır veya daha fazla düğüm içerir |
| Composition | `CustomQueue<T>` | `Node<T>` | 1 → 0..* | Queue sıfır veya daha fazla düğüm içerir |
| Composition | `CustomHashTable<K,V>` | `HashNode<K,V>` | 1 → 0..* | Hash table sıfır veya daha fazla hash düğümü içerir |
| Composition | `NaryTree` | `DomNode` | 1 → 1 | Her ağacın tam olarak bir kök düğümü vardır |
| Composition | `DomNode` | `DomNode` | 1 → 0..* | Bir düğüm sıfır veya daha fazla çocuğa sahip olabilir |
| Association | `DomNode` | `DomNode` | 0..* → 0..1 | Her çocuğun en fazla bir ebeveyni vardır (Parent) |
| Association | `DomNode` | `CustomHashTable<string,string>` | 1 → 1 | Her düğümün bir attribute tablosu vardır |
| Association | `NaryTree` | `CustomHashTable<string,DomNode>` | 1 → 1 | Ağacın bir ID indeks tablosu vardır |
| Realization | `CustomList<T>` | `IEnumerable<T>` | — | Interface implementasyonu |
| Generalization | `ParserController` | `ControllerBase` | — | Kalıtım (Inheritance) |

---

## Veri Yapısı Karmaşıklık Özeti (Big-O)

| Veri Yapısı | Ekleme | Silme | Arama | Erişim (Index) | Kullanım Amacı |
|-------------|--------|-------|-------|----------------|----------------|
| `CustomList<T>` (Doubly Linked List) | O(1) sona | O(n) | O(n) | O(n) | DomNode çocuk listesi, sonuç listesi |
| `CustomStack<T>` (LIFO) | O(1) | O(1) | — | — | HTML parser tag eşleşme, DFS traversal |
| `CustomQueue<T>` (FIFO) | O(1) | O(1) | — | — | BFS traversal |
| `CustomHashTable<K,V>` (Chaining) | O(1) ort. | O(1) ort. | O(1) ort. | — | Attribute saklama, ID ile O(1) erişim |
| `Node<T>` (Doubly Linked) | — | — | — | — | CustomList, CustomStack, CustomQueue temel yapı taşı |
| `HashNode<K,V>` (Singly Linked) | — | — | — | — | CustomHashTable zincirleme (chaining) düğümü |

---

## Algoritma Karmaşıklık Özeti

| Algoritma | Zaman | Uzay | Kullandığı Yapı |
|-----------|-------|------|-----------------|
| BFS (SearchBFS) | O(N) | O(W) — W: en geniş seviye | `CustomQueue<T>` |
| DFS (SearchDFS) | O(N) | O(D) — D: ağaç derinliği | `CustomStack<T>` |
| GetElementById | O(1) | O(1) | `CustomHashTable<K,V>` |
| CalculateDepth | O(N) | O(D) — rekürsiyon yığını | Rekürsiyon |
| CountNodes | O(N) | O(D) — rekürsiyon yığını | Rekürsiyon |
| GetSiblings | O(K) — K: kardeş sayısı | O(K) | `CustomList<T>` |
