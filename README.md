# QsMapper
Conventional .Net SQL entity mapping framework.

QsMapper provides a Linq-like fluent syntax for database operations on (MS)SQL Server databases.

    var result = dao.Query<Customer>()  
       .Where(x => x.Field("FirstName").IsEqualTo("John"))  
       .And(x => x.Field("LastName").IsLike("Do%"))  
       .OrderBy("LastName")  
       .ThenBy("FrstName")  
       .ToList();

A basic dao implementation for MSSQL databases is provided in this project. 
Implementations for other relational database management systems may be developed based on the framework's interface definitions.

# Conventions

## Table and field names

By default QsMapper makes use of sql database schemes.

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

The corresponding Class should reside in a folder/namespace named **Contacts** and the class name itself woule be **Customer**.

Property names are mapped by the convention of identical names (case sensitive).

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

# Setup

The contained IGenericDao implementation may be constructed using a connection string like so.

    var dao = new GenericDao("Data Source=.\SQLEXPRESS; Initial Catalog=QsSamples;Integrated Security=True");

Please refer to Docs/GenericDao.md for more information.

# Basic operations

## Creating and updating objects

    //create
    var customer = new Customer
    {
       Salutation = "Mr",
       FirstName = "John",
       LastName = "Doe"
    };
    
    dao.Save(customer);
    
    // Id will be updated during save so it can be requested immediately after
    // assuming the Id column is declared as identity
    var generatedId = customer.Id;
    
    // update
    customer.Birthday = new DateTime(1985, 10, 3);
    
    dao.Save(customer);
    
## Retrieving single objects by id

    var customer = dao.Get<Customer>(1);
    
## Deleting objects

    // by id
    dao.Delete<Customer>(1);
	
    // by object
    dao.Delete(customer);


## Querying objects

    // retrieve all records
    var allCustomers = dao.Query<Customer>().ToList();

    // query data using conditions
    var customers = dao.Query<Customer>()
       .Where(x => x.Field("Salutation").IsEqualTo("Mr")
       .OrderBy("Name")
       .ToList();
       
Please refer to Docs/QueryBuilder.md for more information.

# Mapping declarations

Data relations and behaviour of the framework may be defined in a custom catalog definition.

    public class SampleCatalog : Catalog
    {
       public SampleCatalog() 
       {
          RegisterMap<Quote>()
	     .QueryWithView("Sales.QuotesQuery")
	     .WithMany<QuoteItem>();
       }
    }
    
Please refer to Docs/Catalog.md for more information.
