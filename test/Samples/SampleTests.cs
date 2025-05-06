using System;
using Net.Arqsoft.QsMapper;
using Newtonsoft.Json;
using NUnit.Framework;
using Samples.Model.Contacts;
using Samples.Model.Messaging;

namespace Samples;

[TestFixture]
public class SampleTests
{
    private readonly IGenericDao _dao = new GenericDao
    (
        @"Data Source=.\MSSQL2019; Initial Catalog=SpaManager;Integrated Security=True;TrustServerCertificate=True;",
        new SampleCatalog()
    );

    [Test]
    public void TestCreateCustomer()
    {
        var customer = new Customer
        {
            Salutation = "Mr",
            FirstName = "John",
            LastName = "Doe"
        };

        _dao.Save(customer);

        Assert.Greater(customer.Id, 0);
    }

    [Test]
    public void TestUpdateCustomer()
    {
        var customer = _dao.Get<Customer>(1);
        Assert.IsNotNull(customer);
        customer.Birthday = new DateTime(1985, 10, 3);
        _dao.Save(customer);
    }

    [Test]
    public void TestFindCustomer()
    {
        var customer = _dao.Query<Customer>()
            .Where(x => x.Field("LastName").IsEqualTo("Doe"))
            .And(x => x.Field("FirstName").IsEqualTo("John"))
            .FirstOrDefault();

        Assert.IsNotNull(customer);
    }

    [Test]
    public void TestSaveAttachment()
    {
        var attachment = new Attachment()
        {
            Filename = "test.pdf",
            MimeType = "application/pdf",
            Data = "---",
            MessageId = 76
        };

        _dao.Save(attachment);
    }

    [Test]
    public void TestReadAppointments()
    {
        var customers = _dao.ExecuteSql("""
                                        select top 100 a.Id, 
                                            a.SubsidiaryId as 'Subsidiary.Id',
                                            s.CompanyName as 'Subsidiary.Name',
                                            a.CustomerId as 'Customer.Id',
                                            cy.Id as 'Subsidiary.Country.Id',
                                            cy.Name as 'Subsidiary.Country.Name',
                                            c.Name as 'Customer.Name',
                                            a.Date,
                                            a.StartTime,
                                            a.Duration
                                        from Schedule.Appointments a
                                            left join Contacts.Customers c on c.Id = a.CustomerId
                                            left join System.Subsidiaries s on s.Id = a.SubsidiaryId
                                            left join System.Countries cy on cy.Id = s.CountryId
                                        order by Id
                                        """).AsList();
        var json = JsonConvert.SerializeObject(customers, Formatting.Indented);
        Console.WriteLine(json);
    }
}