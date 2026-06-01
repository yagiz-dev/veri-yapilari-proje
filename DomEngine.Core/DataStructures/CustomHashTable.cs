using System;

namespace DomEngine.Core.DataStructures;

public class HashNode<K, V>
{
    public K Key { get; set; }
    public V Value { get; set; }
    public HashNode<K, V>? Next { get; set; }

    public HashNode(K key, V value)
    {
        Key = key;
        Value = value;
        Next = null;
    }
}

/// <summary>
/// id bazlı aramalarda ve attributeları tutmada O(1) hız sunan karma tablo.
/// Zincirleme (Chaining) yöntemi kullanılarak çakışmalar (collisions) çözülmüştür.
/// </summary>
/// <typeparam name="K">Anahtar tipi.</typeparam>
/// <typeparam name="V">Değer tipi.</typeparam>
public class CustomHashTable<K, V>
{
    private int capacity = 16;
    private HashNode<K, V>?[] _buckets;
    private int _count;

    public CustomHashTable()
    {
        _buckets = new HashNode<K, V>[capacity];
    }

    private int GetCustomHash(K key)
    {
        if (key == null) return 0;
        
        string strKey = key.ToString() ?? "";
        int hash = 0;
        
        foreach (char c in strKey)
        {
            // Aynı harflere sahip farklı kelimeler (örn: Ali ve ila) aynı sonucu 
            // vermesin diye önceki toplamı 17 gibi asal bir sayıyla çarpıp harfin değerini ekliyoruz.
            hash = (hash * 17) + c;
        }
        
        return hash;
    }

    private int GetBucketIndex(K key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        
        int hash = GetCustomHash(key);
        
        return Math.Abs(hash % _buckets.Length);
    }

    private HashNode<K, V>[] ReHash()
    {
        var oldBuckets = _buckets;
        capacity *= 2;
        _buckets = new HashNode<K, V>[capacity];
        _count = 0;

        foreach (var head in oldBuckets)
        {
            var current = head;
            while (current != null)
            {
                Add(current.Key, current.Value);
                current = current.Next;
            }
        }
        return _buckets;
    }

    public void Add(K key, V value)
    {
        if(((_buckets.Length/4))*3 <_count) { _buckets = ReHash(); }
        
        int index = GetBucketIndex(key);
        var head = _buckets[index];
        var current = head;

        // Key zaten var mı kontrol et
        while (current != null)
        {
            if (current.Key!.Equals(key))
            {
                throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
            }
            current = current.Next;
        }

        // Yeni node oluştur ve başa ekle (daha hızlı)
        var newNode = new HashNode<K, V>(key, value);
        newNode.Next = head;
        _buckets[index] = newNode;
        _count++;
    }

    public bool TryGetValue(K key, out V value)
    {
        int index = GetBucketIndex(key);
        var current = _buckets[index];

        while (current != null)
        {
            if (current.Key!.Equals(key))
            {
                value = current.Value;
                return true;
            }
            current = current.Next;
        }

        value = default!;
        return false;
    }

    public bool ContainsKey(K key)
    {
        return TryGetValue(key, out _);
    }

    //alttaki "indexer" yapısı.
    //sınıftan nesne oluşturulduğunda o nesneyi bir diziymiş gibi kullanmamızı sağlar.
    //Örn: HashTable myHashTable = new HashTable();
    //myHashTable["key"] = "value";  //bu kullanımı sağlar

    public V this[K key]
    {
        get
        {
            if (TryGetValue(key, out V value))
            {
                return value;
            }
            throw new System.Collections.Generic.KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
        }
        set
        {
            int index = GetBucketIndex(key);
            var current = _buckets[index];

            while (current != null)
            {
                if (current.Key!.Equals(key))
                {
                    current.Value = value;
                    return;
                }
                current = current.Next;
            }

            // Key yoksa yeni ekle
            Add(key, value);
        }
    }

    public CustomList<System.Collections.Generic.KeyValuePair<K, V>> GetAllPairs()
    {
        var list = new CustomList<System.Collections.Generic.KeyValuePair<K, V>>();
        foreach (var head in _buckets)
        {
            var current = head;
            while (current != null)
            {
                list.Add(new System.Collections.Generic.KeyValuePair<K, V>(current.Key, current.Value));
                current = current.Next;
            }
        }
        return list;
    }

    public void Remove(K key)
    {
        int index = GetBucketIndex(key);
        var head = _buckets[index];
        HashNode<K, V>? prev = null;
        var current = head;

        while (current != null)
        {
            if (current.Key!.Equals(key))
            {
                if (prev == null)
                {
                    _buckets[index] = current.Next;
                }
                else
                {
                    prev.Next = current.Next;
                }
                _count--;
                return;
            }
            prev = current;
            current = current.Next;
        }
    }
}
