using System.Data.SqlClient;
using log4net;

namespace Net.Arqsoft.QsMapper.QueryBuilder;

/// <summary>
/// Establishes SQL logging for any command.
/// Internally used by BaseQuery.ToList().
/// </summary>
public class CommandRunner
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(CommandRunner));

    /// <summary>
    /// Determines if SQL Commands should be logged to Console.
    /// </summary>
    public static bool DebuggingOn
    {
        get => MapperSettings.DebuggingOn;

        set => MapperSettings.DebuggingOn = value;
    }

    public static void Run(SqlCommand cmd, Action<SqlCommand> action)
    {
        if (DebuggingOn)
        {
            Debug(cmd);
            var startTime = DateTime.Now;
            action(cmd);
            Log.Debug($"{cmd.CommandText} executed in {(DateTime.Now - startTime).TotalMilliseconds:#,##0}ms");
            return;
        }

        action(cmd);
    }

    public static T Run<T>(SqlCommand cmd, Func<SqlCommand, T> func)
    {
        // ReSharper disable once InvertIf
        if (DebuggingOn)
        {
            Debug(cmd);
            var startTime = DateTime.Now;
            var result = func(cmd);
            Log.Debug($"{cmd.CommandText} executed in {(DateTime.Now - startTime).TotalMilliseconds:#,##0}ms");
            return result;
        }

        return func(cmd);
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
            var quotes = param.SqlValue == DBNull.Value ? "" : "'";

            if (param.Size > 0)
            {
                Console.WriteLine($"declare @{param.ParameterName} {param.SqlDbType}({param.Size}) = {quotes}{param.SqlValue}{quotes}");
            }
            else
            {
                Console.WriteLine($"declare @{param.ParameterName} {param.SqlDbType} = {quotes}{param.SqlValue}{quotes}");
            }
        }

        Console.WriteLine(cmd.CommandText);
    }
}