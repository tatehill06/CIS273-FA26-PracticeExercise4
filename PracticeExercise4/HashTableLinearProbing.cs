using System.Data;

namespace PracticeExercise4;

public class HashTableLinearProbing<K, V> : IHashTable<K, V>
{
    private HashEntry<K, V>[] hashEntries;

    private int initialCapacity = 13;

    public readonly double MAX_LOAD_FACTOR = 0.6;

    public HashTableLinearProbing()
    {
        // init all hash entries 
        hashEntries = new HashEntry<K, V>[initialCapacity];

        for(int i=0; i < hashEntries.Length; i++)
        {
            hashEntries[i] = new HashEntry<K, V>();
        }
    }

    private int count;
    public int Count => count;

    public double LoadFactor => (double)count / hashEntries.Length;

    public bool Add(K key, V value)
    {
        // reject null keys
        if(key is null)
        {
            throw new NoNullAllowedException();
        }
        // if key is already present, update its value
        if( Update(key, value))
        {
            return true;
        }

        // resize if needed
        if( LoadFactor > MAX_LOAD_FACTOR)
        {
            Resize();
        }

        // get the hash of the key
        int hash = Hash(key);
        
        // find the array index for this hash
        int index = hash % hashEntries.Length;

        // if there's a collision at that index, find the next available index
        // (an EmptyAfterRemoval slot can be reused, since Update already
        // confirmed the key isn't further along the probe sequence)
        while( hashEntries[index].State == HashEntryState.Full )
        {
            // go to next index
            index = (index + 1) % hashEntries.Length;
        }

        // save key/value pair at correct index
        hashEntries[index] = new HashEntry<K, V>(key, value);
        count++;

        return false;
    }

    // If key is present, update its value and return true; otherwise return false.
    private bool Update(K key, V value)
    {
        // find starting index
        int index = FindIndex(key);
        if(index == -1)
        {
            return false;
        }

        hashEntries[index].Value = value;
        return true;

        // keep probing past Full and EmptyAfterRemoval slots;
        // stop at EmptySinceStart, since the key can't be beyond a never-used slot

    }

    public bool ContainsKey(K key)
    {
        int index = FindIndex(key);

        return index != -1;
    }

    public bool ContainsValue(V value)
    {
        foreach(var entry in hashEntries)
        {
            if(entry.State == HashEntryState.Full && Equals(entry.Value, value))
            {
                return true;
            }
            
        }
        return false;
    }

    public V Get(K key)
    {
        int index = FindIndex(key);

        if (index == -1)
        {
            throw new KeyNotFoundException();
        }

        return hashEntries[index].Value;
    }

    public List<K> GetKeys()
    {
        var keys = new List<K>();

        foreach(var entry in hashEntries)
        {
            if(entry.State == HashEntryState.Full)
            {
                keys.Add(entry.Key);
            }
        }
        return keys;
    }

    public List<V> GetValues()
    {
        var values = new List<V>();

        foreach(var entry in hashEntries)
        {
            if(entry.State == HashEntryState.Full)
            {
                values.Add(entry.Value);
            }
            
        }
        return values;
    }

    public bool Remove(K key)
    {
        // get array index for key
        int index = FindIndex(key);

        // check if -1
        if( index == -1)
        {
            return false;
        }

        // get that entry and get rid of it
        hashEntries[index].Clear();
        count--;
        return true;
    }

    private int FindIndex(K key)
    {
        if( key is null)
        {
            return -1;
        }

        int startingIndex = Hash(key) % hashEntries.Length;
        int index = startingIndex;

        while(hashEntries[index].State != HashEntryState.EmptySinceStart)
        {
            if( hashEntries[index].State == HashEntryState.Full)
            {
                if (hashEntries[index].Key.Equals(key))
                {
                    return index;
                }
            }
            index = (index + 1) % hashEntries.Length;

            if(index == startingIndex)
            {
                return -1;
            }
        }
        return -1;
    }

    private int Hash(K key)
    {
        // clear the sign bit (Math.Abs throws on int.MinValue)
        return key.GetHashCode() & 0x7FFFFFFF;
    }

    private void Resize()
    {
        int newSize = NextPrime(2 * hashEntries.Length);
        var newBuckets = new HashEntry<K, V>[newSize];
        var oldBuckets = hashEntries;

        // initialize the new buckets array 
        for (int i = 0; i < newBuckets.Length; i++)
        {
            newBuckets[i] = new HashEntry<K, V>();
        }

        hashEntries = newBuckets;
        count = 0;

        // re-add all the old buckets into the new array
        foreach (var bucket in oldBuckets)
        {
            if (bucket.State == HashEntryState.Full)
            {
                Add(bucket.Key, bucket.Value);
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
