# Proje Analiz Raporu (Big-O)

Bu dokümanda, projemizde yazdığımız arama algoritmalarının zaman (ne kadar sürdüğü) ve uzay (ne kadar RAM harcadığı) karmaşıklıkları incelenmiştir.

### 1. ID ile Arama (Hash Table)
DOM üzerindeki bir elemanı `id` değerine göre bulma işlemidir.
- **Zaman Karmaşıklığı:** O(1)
  - Hash Table kullandığımız için elemanın yerini dizin (index) hesabı ile doğrudan, tek işlemde buluruz.
- **Uzay Karmaşıklığı:** O(M)
  - Sadece `id` niteliğine sahip olan (M tane) elemanları Hash Table'a kaydettiğimiz için O(M) kadar hafıza kullanırız.

### 2. Derinlik Öncelikli Arama (DFS)
Ağaç üzerinde dal dal (aşağı doğru) inerek yapılan arama işlemidir. (Örn: Class veya Tag ararken)
- **Zaman Karmaşıklığı:** O(N)
  - En kötü ihtimalle ağaçtaki bütün düğümleri (N tane) tek tek kontrol etmemiz gerekir.
- **Uzay Karmaşıklığı:** O(D)
  - Rekürsif (kendi kendini çağıran) bir fonksiyon olduğu için ağacın maksimum derinliği (D) kadar hafıza kullanır.

### 3. Sığlık Öncelikli Arama (BFS)
Ağaç üzerinde katman katman (yatay olarak) yapılan arama işlemidir. Kuyruk (Queue) yapısı ile çalışır.
- **Zaman Karmaşıklığı:** O(N)
  - DFS gibi, en kötü ihtimalle bütün düğümleri (N tane) kontrol etmemiz gerekir.
- **Uzay Karmaşıklığı:** O(W)
  - O anki katmandaki elemanları sıraya (kuyruğa) aldığı için, ağacın en geniş katmanındaki eleman sayısı (W) kadar hafıza kullanır.

---

### Diğer İşlemler (Fonksiyonlar)

- **Ağaç Derinliği Hesaplama (CalculateDepth):**
  - **Zaman:** O(N) (Bütün elemanlara bakar)
  - **Uzay:** O(D) (Ağacın derinliği kadar hafıza harcar)

- **Düğüm Sayma (CountNodes):**
  - **Zaman:** O(N) (Bütün elemanları tek tek sayar)
  - **Uzay:** O(D) (Ağacın derinliği kadar hafıza harcar)

- **Kardeş Düğümleri Bulma (GetSiblings):**
  - **Zaman:** O(K) (Sadece aynı ebeveyne sahip çocukları - K tane - kontrol eder)
  - **Uzay:** O(K) (Bulduğu kardeşleri yeni bir listeye ekler)

### Neden Hem DFS hem BFS Yaptık?
- **DFS:** Hedefimiz ağacın alt kısımlarındaysa daha az RAM (hafıza) harcayarak hedefe ulaşır.
- **BFS:** Hedefimiz ağacın üst/kök kısımlarına yakınsa daha çabuk bulur ama geniş ağaçlarda kuyruk yüzünden daha çok RAM harcar.
