# Veri Yapıları ile HTML'den DOM Ağacı Oluşturma Projesi

Bu proje, bir HTML stringini parse ederek DOM ağacı yapısı oluşturur.

Backend ve API kısmı .NET 8 framework ile yazılmıştır. 
AJAX kullanarak backendle iletişime geçen bir frontend oluşturulmuştur.

Tüm temel veri yapıları ve arama algoritmaları C#'taki hazır koleksiyonlar (`List<>`, `Dictionary<>` vb.) kullanılmadan sıfırdan yazılmıştır.

## Yapılacaklar
- Hatalı HTML stringleri tespit edilebilecek
- Arama kriterleri daha spesifik olacak
- Benchmark için işlem süresi gösterilecek
- UML diyagramları, zaman karmaşıklıkları ve Big-O notasyonu hesaplanacak
- Sunumlar ve demo videoları hazırlanacak

## Çalıştırma
Projeyi hızlı bir şekilde çalıştırmak için Docker kullanabilirsiniz.

Projenin ana klasöründe bir Docker Compose dosyası bulunmaktadır. Aşağıdaki komutla hem frontend hem de backend projelerini aynı anda çalıştırabilirsiniz.

```bash
  docker compose up -d --build
```

Proje derlendikten sonra [localhost:3000](http://localhost:3000) üzerinden arayüze ulaşabilirsiniz.

## Lisans
Proje MIT lisansı altında lisanslanmıştır.

## Hazırlayanlar
- Ahmet Arda Varak
- Yağızhan Burak Yakar
- Mustafa Eren Çetin
- Yusuf Pehlivan
- Yusuf Kahraman