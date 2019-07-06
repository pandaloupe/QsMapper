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
