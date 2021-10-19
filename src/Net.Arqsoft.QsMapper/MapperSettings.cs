using System;

namespace Net.Arqsoft.QsMapper
{
    public class MapperSettings
    {
        private MapperSettings() { }

        private static readonly Lazy<MapperSettings> Instance = new Lazy<MapperSettings>(() => new MapperSettings());

        private bool _debuggingOn;
        private bool _useProvisioning;
        private bool _logUnmappedProperties;

        /// <summary>
        /// Triggers debug output of all executed SQL commands
        /// </summary>
        public static bool DebuggingOn
        {
            get => Instance.Value._debuggingOn;
            set => Instance.Value._debuggingOn = value;
        }

        /// <summary>
        /// Defines if items should be cached internally
        /// </summary>
        public static bool UseProvisioning
        {
            get => Instance.Value._useProvisioning;
            set => Instance.Value._useProvisioning = value;
        }

        /// <summary>
        /// Defines if properties that cannot be resolved should be logged.
        /// </summary>
        public static bool LogUnmappedProperties
        {
            get => Instance.Value._logUnmappedProperties;
            set => Instance.Value._logUnmappedProperties = value;
        }

    }
}
