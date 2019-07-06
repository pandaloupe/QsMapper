namespace Net.Arqsoft.QsMapper.Exceptions {
    using System;
    
    /// <summary>
    /// Indicates that data has been changed during load and save.
    /// </summary>
    public class DataHasChangedException : Exception {
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public DataHasChangedException():base() {}

        /// <summary>
        /// Constructor with message.
        /// </summary>
        /// <param name="message"></param>
        public DataHasChangedException(string message):base(message) {}
    }
}
