using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace UnitTests.Helpers
{
    // the default app.config is used.
    /// <summary>
    ///     Allow to override default configuration file access via ConfigurationService for piece of code
    ///     Main purpose: unit tests
    ///     Do not use it in production code
    /// </summary>
    /// <example>
    ///     // the default app.config is used.
    ///     using(AppConfig.Change(tempFileName))
    ///     {
    ///     // the app.config in tempFileName is used
    ///     }
    /// </example>
    public abstract class AppConfig : IDisposable
    {
        public abstract void Dispose();

        public static AppConfig Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        private class ChangeAppConfig : AppConfig
        {
            private readonly string oldConfig =
                AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();

            private bool disposedValue;

            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                ResetConfigMechanism();
            }

            public override void Dispose()
            {
                if (!disposedValue)
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                    ResetConfigMechanism();


                    disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
                typeof(ConfigurationManager)
                    .GetField("s_initState", BindingFlags.NonPublic |
                                             BindingFlags.Static)
                    .SetValue(null, 0);

                typeof(ConfigurationManager)
                    .GetField("s_configSystem", BindingFlags.NonPublic |
                                                BindingFlags.Static)
                    .SetValue(null, null);

                typeof(ConfigurationManager)
                    .Assembly.GetTypes()
                    .Where(x => x.FullName ==
                                "System.Configuration.ClientConfigPaths")
                    .First()
                    .GetField("s_current", BindingFlags.NonPublic |
                                           BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}