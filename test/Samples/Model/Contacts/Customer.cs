using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Net.Arqsoft.QsMapper.Model;

namespace Samples.Model.Contacts
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
