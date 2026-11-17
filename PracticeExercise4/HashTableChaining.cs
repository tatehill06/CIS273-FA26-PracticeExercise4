using System.Data;

namespace PracticeExercise4;

public class HashTableChaining<K, V> : IHashTable<K, V>
{
    List<HashEntry<K, V>>[] entryListArray;

    private int initialCapacity = 13;

    public readonly double MAX_LOAD_FACTOR = 0.6;

    public HashTableChaining()
    {
        // init all array and entry lists
        entryListArray = new List<HashEntry<K, V>>[initialCapacity];
        for(int i = 0; i < entryListArray.Length; i++)
        {
            entryListArray[i] = new List<HashEntry<K, V>>();
        }
    }

    private int count;
    public int Count => count;

    public double LoadFactor => (double)count / entryListArray.Length;

    public bool Add(K key, V value)
    {
        if(key is null)
        {
            throw new NoNullAllowedException();
        }
        // resize if needed
        if(LoadFactor > MAX_LOAD_FACTOR)
        {
            Resize();
        }

        // get the hash of the key
        int hash = Hash(key);

        // find the correct array index / element
        int index = hash % entryListArray.Length;

        // check if key is already present
        foreach(var entry in entryListArray[index])
        {
            if(entry.Key.Equals(key))
            {
                //update existing value
                entry.Value = value;
                return true;
            }
        }

        // add the new key/value pair to the correct list
        entryListArray[index].Add(new HashEntry<K, V>(key, value));
        count++;

        return false;
    }

    public bool ContainsKey(K key)
    {
        if(key == null)
        {
            return false;
        }
        int index = Hash(key) % entryListArray.Length;
        foreach(var entry in entryListArray[index])
        {
            if (entry.Key.Equals(key))
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsValue(V value)
    {
        if(value == null)
        {
            return false;
        }

        foreach (var list in entryListArray)
        {
            foreach (var entry in list)
            {
                if (Equals(entry.Value, value))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public V Get(K key)
    {
        // hash the key
        int hash = Hash(key);
        
        // find the correct index of the list
        int index = hash % entryListArray.Length;

        // find the entry with the given key (in the correct list)
        foreach(var entry in entryListArray[index])
        {
            if(entry.Key.Equals(key))
            {
                return entry.Value;
            }
        }
        throw new KeyNotFoundException();
    }

    public List<K> GetKeys()
    {
        var keys = new List<K>();

        for(int i = 0; i < entryListArray.Length; i++)
        {
            foreach(var entry in entryListArray[i])
            {
                keys.Add(entry.Key);
            }
        }
        return keys;
    }

    public List<V> GetValues()
    {
        var values = new List<V>();

        for(int i = 0; i < entryListArray.Length; i++)
        {
            foreach(var entry in entryListArray[i])
            {
                values.Add(entry.Value);
            }
        }
        return values;
    }

    public bool Remove(K key)
    {
        int hash = Hash(key);
        
        int index = hash % entryListArray.Length;

        foreach(var entry in entryListArray[index])
        {
            if(entry.Key.Equals(key))
            {
                entryListArray[index].Remove(entry);
                count--;
                return true;
            }
        }
        return false;
    }


    private int Hash(K key)
    {
        // clear the sign bit (Math.Abs throws on int.MinValue)
        return key.GetHashCode() & 0x7FFFFFFF;
    }

    private void Resize()
    {
        int newSize = NextPrime(2 * entryListArray.Length);
        var newEntryListArray = new List<HashEntry<K, V>>[newSize];
        var oldEntryListArray = entryListArray;

        // initialize the new entry list array 
        for (int i = 0; i < newEntryListArray.Length; i++)
        {
            newEntryListArray[i] = new List<HashEntry<K, V>>();
        }

        entryListArray = newEntryListArray;
        count = 0;

        // re-add all the old buckets into the new array
        foreach (var list in oldEntryListArray)
        {
            foreach (var entry in list)
            {
                Add(entry.Key, entry.Value);
            }
        }

    }
    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n < 4) return true;           // 2 and 3
        if (n % 2 == 0) return false;

        for (int d = 3; (long)d * d <= n; d += 2)
        {
            if (n % d == 0) return false;
        }
        return true;
    }

    private static int NextPrime(int n)
    {
        if (n <= 2) return 2;
        int candidate = (n % 2 == 0) ? n + 1 : n;   // only odd numbers can be prime past 2
        while (!IsPrime(candidate))
        {
            candidate += 2;
        }
        return candidate;
    }
}


