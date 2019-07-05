using System;
using Net.Arqsoft.QsMapper.QueryBuilder;

namespace Net.Arqsoft.QsMapper.Model
{
    /// <summary>
    /// Definition for 1:n and n:m relations.
    /// </summary>
    public class ChildCollection
    {
        /// <summary>
        /// Constructor setting default command type as TableOrView.
        /// </summary>
        public ChildCollection()
        {
            GetCommandType = CommandType.TableOrView;
        }

        /// <summary>
        /// Class of child elements.
        /// </summary>
        public Type ChildType { get; set; }

        /// <summary>
        /// Property name of parent holding the child elements.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Table storing the children.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Name of stored procedure when commant type is set to Function or StoredProcedure. 
        /// </summary>
        public string GetCommandName { get; set; }

        /// <summary>
        /// Type of getter.
        /// </summary>
        public CommandType GetCommandType { get; set; }

        /// <summary>
        /// Table field for parent id.
        /// </summary>
        public string MasterFieldName { get; set; }

        /// <summary>
        /// Table field for child id.
        /// </summary>
        public string ChildFieldName { get; set; }

        /// <summary>
        /// Property name for parent in child class.
        /// </summary>
        public string MasterPropertyName { get; set; }

        /// <summary>
        /// Property name for child collection in parent class.
        /// </summary>
        public string ChildPropertyName { get; set; }
    }
}
