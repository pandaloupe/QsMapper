# QsMapper
Conventional .Net SQL entity mapping framework.

QsMapper is an easy to use zero configuration mapping framework using simple naming conventions to map sql based data into .Net objects and vice versa.

The framework provides a Linq-like fluent syntax for database operations on (MS)SQL Server databases.

```csharp
var result = dao.Query<Customer>()  
   .Where(x => x.Field("FirstName").IsEqualTo("John"))  
   .And(x => x.Field("LastName").IsLike("Do%"))  
   .OrderBy("LastName")  
   .ThenBy("FirstName")  
   .ToList();
```

A basic dao implementation for MSSQL databases is provided with the project. 

Implementations for other relational database management systems may be developed based on the framework's interface definitions.

# Conventions

## Table and field names

By default QsMapper makes use of sql database schemes.

```tsql
create database QsSamples
go 

use QsSamples
go

create schema Contacts
go
    
create table Contacts.Customers
(
   Id int not null identity(1, 1),
   Salutation nvarchar(20),
   FirstName nvarchar(50),
   LastName nvarchar(50),
   Name as trim(FirstName + ' ' + LastName),
   Birthday datetime,
   constraint pk_contacts_customers primary key (Id)
)
go
```

The corresponding .Net Class should reside in a folder/namespace named **Contacts** and the class name itself would be **Customer**.

As you might have noticed the table name is the plural form of the class name. For conventions regarding exceptions please refer to [doc/Conventions.md](doc/Conventions.md).

Property names are mapped by the convention of identical names (case sensitive).

```csharp
using Net.Arqsoft.QsMapper.Model; 
    
namespace Net.Arqsoft.QsMapper.Examples.Model.Contacts
{
   public class Customer : IntegerBasedEntity
   {
      // Id and Name are already declared in IntegerBasedEntity class
      public string Salutation { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public DateTime? Birthday { get; set; } // may be null, so ? is recommended but optional
   }
}
```

Please refer to [doc/Conventions.md](doc/Conventions.md) for more information on the framework's naming conventions.


# Setup

The contained IGenericDao implementation may be constructed using a connection string like so.

```csharp
var dao = new GenericDao(@"Data Source=.\SQLEXPRESS; Initial Catalog=QsSamples;Integrated Security=True");
```

Please refer to **GenericDao.md** (TODO) for more information.

# Basic operations

## Creating and updating objects

```csharp
//create
var customer = new Customer
{
   Salutation = "Mr",
   FirstName = "John",
   LastName = "Doe"
};
   
dao.Save(customer);
    
// Id will be updated during save so it can be requested immediately after
// (assuming the Id column is declared as identity)
var generatedId = customer.Id;
   
// update
customer.Birthday = new DateTime(1985, 10, 3);
    
dao.Save(customer);
```
    
## Retrieving single objects by id

```csharp
var customer = dao.Get<Customer>(1);
```

## Deleting objects

```csharp
// by id
dao.Delete<Customer>(1);
	
// by object
dao.Delete(customer);
```

## Querying objects

```csharp
// retrieve all records
var allCustomers = dao.Query<Customer>().ToList();

// query data using conditions
var customers = dao.Query<Customer>()
   .Where(x => x.Field("Salutation").IsEqualTo("Mr")
   .OrderBy("Name")
   .ToList();
```

Please refer to [doc/QueryBuilder.md](doc/QueryBuilder.md) for more information.

## Calling stored procedures

```csharp
// without return value
dao.Execute("Booking.CreateSchedule")
  .WithParameter("Year", 2020)
  .AsVoid();
      
// with return value
dao.Execute("Sales.CreateJournal")
  .WithParameter("UserName", CurrentUser.Name)
  .WithParameter("Time", DateTime.Now)
  .AsFunction();

// with data output
dao.Execute("Contacts.RetrieveCustomersByCityCode")
    .WithParameter("FirstCityCode", "20000")
    .WithParameter("LastCityCode", "29999")
    .AsListOf<Customer>();
```

Please refer to **CommandBuilder.md** (TODO) for more information.

# Mapping declarations

Data relations and behaviour of the framework may be defined in a custom catalog definition.

```csharp
public class SampleCatalog : Catalog
{
   public SampleCatalog() 
   {
      RegisterMap<Quote>()
         .QueryWithView("Sales.QuotesQuery")
	 .WithMany<QuoteItem>();
   }
}
```

Please refer to **Catalog.md** (TODO) for more information.
