# QsMapper
Conventional .Net SQL entity mapping framework.

QsMapper provides a Linq-like fluent syntax for database operations on MSSQL Server databases.

    var dao = new GenericDao();
    var result = dao.Query()  
       .Where(x => x.Field("FirstName").IsEqualTo("John"))  
       .And(x => x.Field("LastName").IsLike("Do%"))  
       .OrderBy("LastName")  
       .ThenBy("FrstName")  
       .ToList();

Providers for other databases may be developed based on the framework's interface definitions.
