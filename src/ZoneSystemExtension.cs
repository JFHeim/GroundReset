namespace GroundReset;

public static class ZoneSystemExtension
{
    extension(ZDOMan zdoMan)
    {
        public Task<List<ZDO>> GetWorldObjectsAsync(params Func<ZDO, bool>[] customFilters)
        {
            var zdos = new List<ZDO>(zdoMan.m_objectsByID.Values);
            return Task.Run(() =>
            {
                if (customFilters.Length == 0) return zdos;

                zdos = zdos.Where(x => customFilters.All(filter => filter?.Invoke(x) ?? true)).ToList();
                return zdos;
            });
        }

        public Task<List<ZDO>> GetWorldObjectsAsync() => zdoMan.GetWorldObjectsAsync([]);

        public Task<List<ZDO>> GetWorldObjectsAsync(string prefabName, params Func<ZDO, bool>[] customFilters)
        {
            int prefabHash = prefabName.GetStableHashCode();
            Func<ZDO, bool> prefabFilter = zdo => zdo.GetPrefab() == prefabHash;

            return zdoMan.GetWorldObjectsAsync(customFilters.AddItem(prefabFilter).ToArray());
        }
    }
}