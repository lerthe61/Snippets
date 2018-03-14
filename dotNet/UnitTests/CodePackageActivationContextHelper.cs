// able to mock configuration for Service Fabric service
public static class CodePackageActivationContextHelper
    {
        public static ConfigurationPackage CreatePackage(IEnumerable<KeyValuePair<string, string>> configurationParameters)
        {
            var package = CreateUninitialized<ConfigurationPackage>();
            var settings = CreateUninitialized<ConfigurationSettings>();
            var sections = new KeyedCollectionImpl<string, ConfigurationSection>(s => s.Name);
            var section = CreateUninitialized<ConfigurationSection>();
            var parameters = new KeyedCollectionImpl<string, ConfigurationProperty>(p => p.Name);
            sections.Add(section);

            foreach (var keyValuePair in configurationParameters)
            {
                var parameter = CreateUninitialized<ConfigurationProperty>();
                typeof(ConfigurationProperty)
                    .GetProperty("Name")
                    .SetValue(parameter, keyValuePair.Key);
                typeof(ConfigurationProperty)
                    .GetProperty("Value")
                    .SetValue(parameter, keyValuePair.Value);
                parameters.Add(parameter);
            }

            typeof(ConfigurationPackage)
                .GetProperty("Settings")
                .SetValue(package, settings);
            typeof(ConfigurationSettings)
                .GetProperty("Sections")
                .SetValue(settings, sections);
            typeof(ConfigurationSection)
                .GetProperty("Parameters")
                .SetValue(section, parameters);
            typeof(ConfigurationSection)
                .GetProperty("Name")
                .SetValue(section, "Configuration");

            return package;
        }

        private static T CreateUninitialized<T>()
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

        private class KeyedCollectionImpl<TKey, TValue> : KeyedCollection<TKey, TValue>
        {
            private readonly Func<TValue, TKey> _getKey;

            public KeyedCollectionImpl(Func<TValue, TKey> getKey)
            {
                _getKey = getKey;
            }

            protected override TKey GetKeyForItem(TValue item)
            {
                return _getKey(item);
            }
        }
    }