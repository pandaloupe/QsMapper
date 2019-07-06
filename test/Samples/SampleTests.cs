using System;
using Net.Arqsoft.QsMapper;
using NUnit.Framework;
using Samples.Model.Contacts;

namespace Samples
{
    [TestFixture]
    public class SampleTests
    {
        private readonly IGenericDao _dao;

        public SampleTests()
        {
            _dao = new GenericDao(@"Data Source=.\SQLEXPRESS; Initial Catalog=QsSamples;Integrated Security=True");
        }

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
    }
}
