# QsMapper Conventions

QsMapper is designed to map almost everything based on simple naming conventions. 

If the database structure and model classes follow these conventions there is no need to configure the mapping at all.

:warning: QsMapper naming conventions are case sensitive.

# Table mapping

## Namespaces

Since database models tend to grow, I recommend dividing the tables into entity groups using database schemas.

The naming convention for entity groups is: 

**Schema names are identical to the last class namespace particle.**

In T-SQL this is straightforward:

```tsql
create schema Contacts
create schema Sales
create schema Billing
```

To represent these schemes in C# (or .Net in general) you divide your model namespace using subfolders in your project:

- Model
-- Contacts
-- Sales
-- Billing

The mapper evaluates the entity class name and assumes the last namespace part before the class name being the schema.

Net.Arqsoft.QsMapper.Samples.Model.**Contacts**.Customer

If you do not want to use schemes and stay with the default 'dbo' schema there would be only one subfolder named 'dbo' in your Model folder.

Nevertheless table names may be configured individually using a custom Catalog class. Please refer to **Catalog.md** for more information.

## Table/Class names

As table names in sql are supposed to be in plural and class names in .Net in singular the mapping convention for table names is:

**Table names are the plural form of the class name.**

So the class *Net.Arqsoft.QsMapper.Samples.Model.Contacts.Customer* maps to the table *Contacts.Customers*.

The default plural is built by simply adding a 's' to the class name. 

Exceptions known to the framework are:

- '*s' => '*ses'
- '*sh' => '*shes'
- '*ch' => '*ches'
- '*y' => '*ies'
- '*Person' => '*People'
- '*Schema' => '*Schemes'
- '*Child' => '*Children'

Exceptions may be extended by changing the *GetPluralName()*-Method in the *NameResolver*-Class.

# Field mapping

## Field/Property Names

For property/field names the convention is simply:

**Database field names are identical to class property names.**

QsMapper uses reflection to check the class's property names against the database field names. The mapping direction is always from datab

## Data Types

QsMapper assumes that the sql types and .Net types are convertible and tries to set the value directly via *Property.Set()* method.

If the value retrieved from the databse is *DBNull.Value* the mapping is skipped, so the class property will keeps it's default value.

## Object mapping (n:1 relation)

If a child class references a parent class the parent property name must be identical to the table field name appended with 'Id'.

```csharp
public class Customer
{
	...
	public Employee AccountManager { get; set;}
	...
}
```

```tsql
create table Contacts.Customers
(
	...
	AccountManagerId int,
	...
)
```

So the convention for this is:

**Parent property names are mapped to table fields of the same name appended with 'Id'.**

In this constellation a customer returned by **Dao.Get<Customer>(n);** would have AccountManager set to null if AccountManagerId is null in the database. Else AccountManager will by an object of type *Employee* with it's Id set to the value of *AccountManagerId'.

Other properties of the parent object may be set and mapped using database views or queries having fieldnames containing a dot.

```tsql
select c.*, e.Name as 'AccountManager.Name'
from Contacts.Customers c
left join Sales.Employees e on e.Id = c.AccountManagerId
```

The mapper now would set the AccountManager.Name property according to the value returnend from the database.








