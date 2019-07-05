# Query initialization

A new query instance for a specific model class may be created like so.

```csharp
dao.Query<ClassName>() 
```

# Basic example

```csharp
var dao = new GenericDao();
// Customer class must reside in a namespace ending with '.Contacts'
// so the query builder assumes the table name to be 'Contacts.Customers'
var result = dao.Query<Customer>()
 	.From("Contacts.Customers") // this is obsolete regarding to the convention mentioned above
 	.Where(x => x.Field("LastName").IsEqualTo("Wolff"))  // cf. to operator list below
 	.And(x => x.Field("City").IsEqualTo("Hamm")
 	.OrderBy("LastName")
 	.ToList();
```

# Conditions

## Logical operators

The condition block is initiated using the **Where()** method and may be extended using subsequent **And()** or **Or()** condition methods.

```csharp
query = query.Where(condition1).And(condition2);

query = query.Where(condition1).Or(condition2);
```

Operator precedence is following common rules (as in C# or T-SQL) so **And** has precedence over **Or**.

'Brackets' may be set using nested expressions:

```csharp
query = query.Where(condition1.Or(condition2))
   .And(condition3);
```

## Conditions

Conditions are declared as **QueryParameter** objects which are created using lambda expressions inside Where, And or Or methods.

```csharp
var query = query.Where(x => x.Field("Fieldname").ConditionMethod()); 
var query = query.Where(x => x.Field("Fieldname").ConditionMethod())
 	.And(x => x.Field("Fieldname2").ConditionMethod());
```

:warning: Nested conditions require a change of the lambda variable name.

```csharp
query = query.Where(x => x.Field("Fieldname").ConditionMethod()
 	.Or(y => y.Field("Fieldname2").ConditionOperator()));
```

## Comparison operators

The above mentioned **ConditionMethods** may be one of the following operator methods.

QsMapper |	SQL
---------|----
IsEqualTo(object value)	| = value
IsGreaterThan(object value)	| > value
IsGreaterOrEqual(object value) |	>= value
IsLessThan(object value) |	< value
IsLessOrEqual(object value) |	<= value
IsLike(string value) |	like 'value'
Contains(string value) |	like '%value%'
IsNull() |	is null
IsTrue() |	isnull(field, 0) = 1
IsFalse() |	isnull(field, 0) = 0
IsBetween(object value1).And(object value2) |	between value1 and value2
IsIn(params object[] values) |	in (values[0], values[1], …)

All operators may be inverted using the **Not** keyword.

```csharp
var result = dao.Query<Customer>()
 	.Where(x => x.Field("LastName").Not.IsEqualTo("Doe"))  
 	.ToList();
 ```
 
# Sorting

One or more order statements may be added to the query.

```csharp
query = query.OrderBy("Fieldname")[.Ascending];
query = query.OrderBy("Fieldname").Descending;
query = query.OrderBy("Fieldname").Ascending
 	.ThenBy("Fieldname2").Descending
 	.ThenBy(…);
```

# Result methods

The query is built, concluded and excuted using one of these methods.

Method | Description
--------------------
IList<T> ToList() | Returns a typed list using the type parameter used in the **Query<T>()** method.	
IList<T> ToExtendedList() |	Loops through 1:n and m:n relations as defined in the catalog (cf. Catalog.md) and fills the respectice collections on each returned object.
IList<T1> ToListOf<T1>() |	Returns a list of type T1 (T1 must derive from T).
T FirstOrDefault() |	Returns the first record as type T (or null, if no results where found).
int Count()	| Returns the number of records found by the query (using count(*) in the compiled sql expression).





