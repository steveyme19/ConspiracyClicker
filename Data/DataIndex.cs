namespace ConspiracyClicker.Data;

/// <summary>
/// Builds id -> item lookups for the static data tables.
///
/// These tables are read thousands of times per second from the game loop and the UI
/// refresh (every purchased upgrade is resolved by id on every EPS/click recalculation),
/// so the linear FirstOrDefault scans they replace dominated the frame budget.
///
/// First entry wins on a duplicate id, matching the FirstOrDefault behaviour it replaces.
/// </summary>
internal static class DataIndex
{
    public static Dictionary<string, T> Build<T>(IEnumerable<T> items, Func<T, string> idSelector)
    {
        var map = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            string id = idSelector(item);
            if (!map.ContainsKey(id)) map[id] = item;
        }
        return map;
    }
}
