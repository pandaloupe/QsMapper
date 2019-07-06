using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Net.Arqsoft.QsMapper.QueryBuilder;
using CommandType = System.Data.CommandType;

namespace Net.Arqsoft.QsMapper.CommandBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for building SQl Commands for stored procedures.
    /// </summary>
    public class DatabaseCommand : ICommand
    {
        private readonly IGenericDao _dao;
        protected readonly ICatalog Catalog;
        private readonly string _commandText;
        private readonly IDictionary<string, object> _parameters;
        private readonly CommandType _commandType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="commandText">Name of SQL procedure or Text Command</param>
        /// <param name="catalog">ICatalog</param>
        /// <param name="dao">IGenericDao providing connection</param>
        /// <param name="commandType"></param>
        public DatabaseCommand(string commandText, ICatalog catalog, IGenericDao dao, CommandType commandType = CommandType.StoredProcedure)
        {
            _commandText = commandText;
            _dao = dao;
            Catalog = catalog;
            _commandType = commandType;
            _parameters = new Dictionary<string, object>();
        }

        public ICommand WithParameter(string name, object value)
        {
            _parameters.Add(name, value);
            return this;
        }

        public virtual IList<T> AsListOf<T>() where T : class, new()
        {
            using (var cmd = GetCommand())
            {
                var mapper = Catalog.GetPropertyMapper<T>();
                using (var reader = cmd.ExecuteReader())
                {
                    var result = mapper.MapAll(reader);
                    reader.Close();
                    return result;
                }
            }
        }

        public virtual IList<T> AsListOf<T>(out int returnValue) where T : class, new()
        {
            using (var cmd = GetCommand())
            {
                cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                var mapper = Catalog.GetPropertyMapper<T>();
                using (var reader = cmd.ExecuteReader())
                {
                    var result = mapper.MapAll(reader);
                    reader.Close();
                    returnValue = (int)cmd.Parameters["@RETURN_VALUE"].Value;
                    return result;
                }
            }
        }
        
        public void AsVoid()
        {
            using (var cmd = GetCommand())
            {
                if (CommandDebugger.DebuggingOn)
                {
                    CommandDebugger.Debug(cmd);
                }

                cmd.ExecuteNonQuery();
            }
        }

        public int AsFunction()
        {
            using (var cmd = GetCommand())
            {
                cmd.Parameters.Add("@RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                if (CommandDebugger.DebuggingOn)
                {
                    CommandDebugger.Debug(cmd);
                }

                cmd.ExecuteNonQuery();

                return (int)cmd.Parameters["@RETURN_VALUE"].Value;
            }
        }

        /// <summary>
        /// Prepare command and set parameters
        /// </summary>
        /// <returns></returns>
        protected SqlCommand GetCommand()
        {
            var cmd = _dao.GetSqlCommand(_commandText);
            cmd.CommandType = _commandType;
            foreach (var key in _parameters.Keys)
            {
                cmd.Parameters.AddWithValue(key, _parameters[key] ?? DBNull.Value);
            }

            return cmd;
        }
    }
}
