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

# Conventions

## Table names

By default QsMapper makes use of sql database schemes.

    create database QsSamples
	go 

	use QsSamples
	go
	
    create schema Contacts
    go
    
    create table Contacts.Customers (
       Id int not null identity(1, 1),
       Salutation nvarchar(20),
       FirstName nvarchar(50),
       LastName nvarchar(50),
       Name as trim(FirstName + ' ' + LastName),
       Birthday datetime,
       constraint pk_contacts_customers primary key (Id)
    )
    go

The corresponding Class should reside in a folder/namespace named 'Contacts' and the class name itself woule be 'Customer'.

    using Net.Arqsoft.QsMapper.Model; 
    
    namespace Net.Arqsoft.QsMapper.Examples.Model.Contacts
    {
       public class Customer : IntegerBasedEntity // Id and Name are already declared in IntegerBasedEntity class
       {
          public string Salutation { get; set; }
          public string FirstName { get; set; }
          public string LastName { get; set; }
          public DateTime? Birthday { get; set; } // may be null, so ? is recommended but optional
       }
    }
