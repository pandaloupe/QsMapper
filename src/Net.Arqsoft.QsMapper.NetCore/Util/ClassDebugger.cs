using System.Collections;
using System.Globalization;
using System.Text;

namespace Net.Arqsoft.QsMapper.Util;

public class ClassDebugger
{
    public static string GetObjectInfo(object o, int maxLevel = 0, int currentLevel = 1, IList mappedObjects = null)
    {
        if (o == null)
        {
            return "null";
        }

        var indent = new string(' ', (currentLevel - 1) * 3);
        indent += "   ";

        var parentObjects = mappedObjects == null
            ? new ArrayList()
            : new ArrayList(mappedObjects);
            
        if (parentObjects.Contains(o))
        {
            return $"{indent}null // circular reference to {o.GetType().Name}";
        }

        parentObjects.Add(o);

        var sb = new StringBuilder();

        var propInfo = new List<string>();

        sb.AppendLine($"{indent}new {o.GetType().Name}\n{indent}{{");

        foreach (var prop in o.GetType().GetProperties())
        {
            if (!prop.CanWrite)
            {
                continue;
            }

            var value = prop.GetValue(o, null);

            if (value == null)
            {
                propInfo.Add($"{indent}{prop.Name} = null");
            }
            else
            {
                var type = value.GetType();
                if (value is string)
                {
                    propInfo.Add($"{indent}{prop.Name} = \"{value}\"");
                }
                else if (value is Guid)
                {
                    propInfo.Add($"{indent}{prop.Name} = new Guid(\"{value}\")");
                }
                else if (value is DateTime)
                {
                    propInfo.Add($"{indent}{prop.Name} = new DateTime({value:yyyy, M, d, H, m, s})");
                }
                else if (type.IsPrimitive || value is decimal)
                {
                    propInfo.Add(string.Format(CultureInfo.GetCultureInfo("en-US"), "{0}{1} = {2}", indent, prop.Name, value));
                }
                else if (type.IsEnum)
                {
                    propInfo.Add($"{indent}{prop.Name} = {type.Name}.{value}");
                }
                else if (type.GetInterface(typeof(IEnumerable).FullName) != null)
                {
                    var data = (IEnumerable)value;
                    var members = new List<string>();

                    foreach (var item in data)
                    {
                        if (maxLevel > 0)
                        {
                            maxLevel++;
                        }

                        members.Add(GetObjectInfo(item, maxLevel, currentLevel + 2, parentObjects));
                    }

                    propInfo.Add($"{indent}{prop.Name} = new {type.Name}\n{indent}{{\n{string.Join(",\n", members)}\n{indent}}}");
                }
                else if (maxLevel == 0 || currentLevel < maxLevel)
                {
                    if (maxLevel > 0)
                    {
                        maxLevel++;
                    }

                    propInfo.Add($"{indent}{prop.Name} = {GetObjectInfo(value, maxLevel, currentLevel + 1, parentObjects).TrimStart()}");
                }
            }
        }

        sb.Append(string.Join(",\n", propInfo));

        indent = indent.Substring(0, indent.Length - 3);

        sb.Append($"\n{indent}}}");

        if (currentLevel == 1)
        {
            sb.AppendLine(";");
        }

        return sb.ToString();
    }

    public static void LogCollection(IEnumerable collection)
    {
        LogCollection(Console.Out, collection);
    }

    public static void LogCollection(TextWriter writer, IEnumerable collection)
    {
        if (collection == null)
        {
            writer.WriteLine("no data");
            return;
        }

        foreach (var item in collection)
        {
            writer.WriteLine(GetObjectInfo(item));
        }
    }

    public static void LogItem(object item)
    {
        LogItem(Console.Out, item);
    }

    public static void LogItem(TextWriter writer, object item)
    {
        if (item == null)
        {
            writer.WriteLine("no data");
            return;
        }

        writer.WriteLine(GetObjectInfo(item));
    }

}