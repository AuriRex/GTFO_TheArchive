using System.Collections.Generic;

namespace TheArchive.Models.Vanity;

public class LocalVanityAcquiredLayerDrops
{
    public HashSet<string> ClaimedDrops { get; set; } = new HashSet<string>();

    public bool HasBeenClaimed(string key)
    {
        return ClaimedDrops.Contains(key);
    }

    public void Claim(string key)
    {
        ClaimedDrops.Add(key);
    }
}