# Veri Yapıları ile HTML'den DOM Ağacı Oluşturma Projesi

Bu proje, bir HTML stringini parse ederek DOM ağacı yapısı oluşturur. Mevcut aşamada projenin backend'inin temeli atılmış olup ileride bir frontend kısmı ile desteklenecektir.

Proje .NET 10 framework ile yazılmıştır. İleride Docker ile çalışan bir sistem kuracağız, ilk aşamada test etmek için .NET 10 kurmanız gerekiyor.

## Eklenecek özellikler
- Mevcuttaki C# backend HTTP API yapısına dönüştürülecek
- Arama algoritmaları yazılacak
- Basit bir frontend ile string alınıp parse edilip API'a gönderilerek DOM ağacı oluşturulacak
- Hatalı HTML stringleri tespit edilebilecek
- Docker Compose ile container yapısı kurulacak
- UML diyagramları, zaman karmaşıklıkları ve Big-O notasyonu hesaplanacak
- Sunumlar ve demo videoları hazırlanacak

## Şu anki kapsam
- Main'e hardcode edilen HTML stringini basit bir şekilde parse etme
- HTML tag'leri basit bir şekilde parse edilir, attribute'lar veya ID'ler henüz parse edilmez
- Konsola çıktı olarak verme (HTTP API ile değiştirilecek)

## Çalıştırma
Projeyi çalıştırmak için Main fonksiyonundaki html değişkenini değiştirip şu komutu kullanabilirsiniz:

```bash
dotnet run --project DomTreePoc
```
Örnek çıktı:

```text
DOM agaci:

#document
  html
    body
      main
        section
          h1
          p
        section
          article
          article
```

## Lisans
Proje MIT lisansı altında lisanslanmıştır.

## Hazırlayanlar
- Ahmet Arda Varak
- Yağızhan Burak Yakar
- Mustafa Eren Çetin
- Yusuf Pehlivan
- Yusuf Kahraman