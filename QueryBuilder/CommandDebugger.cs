namespace Net.Arqsoft.QsMapper.QueryBuilder
{
    using System;
    using System.Data.SqlClient;

    /// <summary>
    /// Establishes SQL logging for any command.
    /// Internally used by BaseQuery.ToList().
    /// </summary>
    public class CommandDebugger
    {
        /// <summary>
        /// Determines if SQL Commands should be logged to Console.
        /// </summary>
        public static bool DebuggingOn
        {
            get => MapperSettings.DebuggingOn;

            set => MapperSettings.DebuggingOn = value;
        }

        /// <summary>
        /// Log Command and it's parameters to Console.
        /// </summary>
        /// <param name="cmd">SqlCommand</param>
        public static void Debug(SqlCommand cmd)
        {
            if (!DebuggingOn)
            {
                return;
            }

            foreach (SqlParameter param in cmd.Parameters)
            {
                Console.WriteLine(
                    "declare @{0} {1} = {2}{3}{2}",
                    param.ParameterName,
                    param.SqlDbType,
                    param.SqlValue == DBNull.Value ? "" : "'",
                    param.SqlValue
                );
            }

            Console.WriteLine(cmd.CommandText);
        }
    }
}
