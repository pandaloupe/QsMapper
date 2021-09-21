namespace Net.Arqsoft.QsMapper {
    using Net.Arqsoft.QsMapper.Exceptions;

    class NameResolver {
        public static void ResolveTableName<T>(TableMap<T> map) where T: class, new() {
            //get class including last part of namespace
            var fullName = typeof(T).FullName;
            if (fullName==null) {
                throw new InvalidMapTypeException("Anonymous Types cannot be mapped, stupid!");
            }
            var nameParts = fullName.Split('.');
            var schema = nameParts[nameParts.Length - 2];
            var typeName = nameParts[nameParts.Length - 1];
            map.Table(schema, GetPluralName(typeName));
        }

        public static string GetPluralName(string typeName) {
            var typeRoot = typeName;
            var pluralExtension = "s";
            switch (typeName.Substring(typeName.Length - 1, 1)) {
                case "s":
                    pluralExtension = "es";
                    break;
                case "y":
                    typeRoot = typeName.Substring(0, typeName.Length - 1);
                    pluralExtension = "ies";
                    break;
            }

            switch (typeName.Substring(typeName.Length - 2, 2))
            {
                case "sh":
                case "ch":
                    pluralExtension = "es";
                    break;
                case "ay":
                    pluralExtension = "ys";
                    break;
            }

            if (typeName.EndsWith("Person")) {
                pluralExtension = "People";
                typeRoot = typeName.Substring(0, typeName.LastIndexOf("Person"));
            }

            else if (typeName.EndsWith("Schema")) {
                pluralExtension = "Schemes";
                typeRoot = typeName.Substring(0, typeName.LastIndexOf("Schema"));
            }

            else if (typeName.EndsWith("Child")) {
                pluralExtension = "Children";
                typeRoot = typeName.Substring(0, typeName.LastIndexOf("Schema"));
            }

            return typeRoot + pluralExtension;
        }
    }
}
