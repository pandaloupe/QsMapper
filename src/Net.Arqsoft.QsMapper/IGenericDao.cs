using System.Security.Cryptography.X509Certificates;

namespace Net.Arqsoft.QsMapper {
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using CommandBuilder;
    using QueryBuilder;

    /// <summary>
    /// A Generic interface for object queries.
    /// </summary>
    public interface IGenericDao {
        /// <summary>
        /// Get current date from database server
        /// </summary>
        /// <returns>Date + Time from database</returns>
        DateTime GetCurrentDate();
        
        /// <summary>
        /// Defines the current Catalog. 
        /// </summary>
        ICatalog Catalog { get; set; }

        /// <summary>
        /// Return a single object from Database
        /// including 1:n and n:m relations defined in Catalog
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(object id) where T : class, new();

        /// <summary>
        /// Return a single object from Database using a composite key
        /// including 1:n and n:m relations defined in Catalog
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="compositeId"></param>
        /// <returns></returns>
        T Get<T>(params object[] compositeId) where T : class, new();
            
        /// <summary>
        /// Return the connection currently used
        /// </summary>
        /// <returns></returns>
        SqlConnection GetSqlConnection();

        /// <summary>
        /// Create a new SQL Command.
        /// </summary>
        /// <returns></returns>
        SqlCommand GetSqlCommand();
        
        /// <summary>
        /// Create a new SQL Command for a certain sql proc/function.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        SqlCommand GetSqlCommand(string procedureName);
        
        /// <summary>
        /// Apply current values from Database to object
        /// including all defined relations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void Restore<T>(object item) where T : class, new();

        /// <summary>
        /// Apply current values from Database to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="withRelations">restore 1:n and n:m relations as well</param>
        void Restore<T>(object item, bool withRelations) where T : class, new();

        /// <summary>
        /// Save object including all fields and relations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void Save<T>(T item) where T : class, new();
        
        /// <summary>
        /// Save data from anonymous type
        /// only properties supplied will be updated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void Save<T>(object item) where T : class, new();
        
        /// <summary>
        /// Delete object from Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The object to delete</param>
        void Delete<T>(T item) where T : class, new();
        
        /// <summary>
        /// Delete object from Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">Key values identifying object</param>
        void Delete<T>(params object[] keys) where T : class, new();

        /// <summary>
        /// Delete object from Database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">Values identifying object(s)</param>
        void Delete<T>(IDictionary<string, object> parameters) where T : class, new();

        /// <summary>
        /// invokes a Query Chain Expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQuery<T> Query<T>() where T : class, new();

        /// <summary>
        /// invokes a Query Chain Expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableOrView">Name of SQL Table or View to be used instead of configured value</param>
        /// <returns></returns>
        IQuery<T> Query<T>(string tableOrView) where T : class, new();

        /// <summary>
        /// invokes a Query Chain Expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition">Condition to be used as where clause</param>
        /// <returns></returns>
        IQuery<T> Query<T>(Func<QueryParameter, QueryParameter> condition) where T : class, new();

        /// <summary>
        /// Open the connection
        /// </summary>
        SqlConnection OpenConnection();

        /// <summary>
        /// CloseConnection
        /// </summary>
        void CloseConnection();
        
        /// <summary>
        /// Begin a Database Transaction
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commit the current Database Transaction
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rollback the current Database Transaction
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Start a execution chain for a sql server stored procedure
        /// </summary>
        /// <param name="procedureName">Name of the procedure including Schema</param>
        /// <returns>ICommand object</returns>
        ICommand Execute(string procedureName);

        /// <summary>
        /// Start a execution chain for a sql server stored procedure
        /// </summary>
        /// <param name="queryText">Plain SQL query text</param>
        /// <returns>ICommand object</returns>
        ICommand ExecuteSql(string queryText);
    }
}
