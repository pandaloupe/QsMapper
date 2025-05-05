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
    public void TestReadCustomers()
    {
        var customers = _dao.ExecuteSql("select top 100 Id, SubsidiaryId + 1, * from Schedule.Appointments").AsList();
        var json = JsonConvert.SerializeObject(customers, Formatting.Indented);
        Console.WriteLine(json);
    }
}