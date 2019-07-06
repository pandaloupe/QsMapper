using System;

namespace Net.Arqsoft.QsMapper
{
    public class MapperSettings
    {
        private MapperSettings() { }

        private static readonly Lazy<MapperSettings> Instance = new Lazy<MapperSettings>(() => new MapperSettings());

        private bool _debuggingOn;
        private bool _useProvisioning;

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
    }
}
