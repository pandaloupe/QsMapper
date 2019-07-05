using System.Collections.Generic;

namespace Net.Arqsoft.QsMapper.CommandBuilder
{
    /// <summary>
    /// Wrapper interface for sql stored procedures.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Add a command parameter with name and value
        /// </summary>
        /// <param name="name">Name of sql proc parameter (excluding the @-sign)</param>
        /// <param name="value">Value to pass as paramter</param>
        /// <returns>self (fluent syntax)</returns>
        ICommand WithParameter(string name, object value);

        /// <summary>
        /// return generic list using the proc's result set
        /// </summary>
        /// <typeparam name="T">newable class</typeparam>
        /// <returns></returns>
        IList<T> AsListOf<T>() where T : class, new();

        /// <summary>
        /// return generic list using the proc's result set
        /// </summary>
        /// <typeparam name="T">newable class</typeparam>
        /// <param name="returnValue">the proc's return value</param>
        /// <returns></returns>
        IList<T> AsListOf<T>(out int returnValue) where T : class, new();

        /// <summary>
        /// Simply execute the stored procedure
        /// </summary>
        void AsVoid();

        /// <summary>
        /// Execute the stored procedure and return it's return value
        /// </summary>
        int AsFunction();
    }
}
