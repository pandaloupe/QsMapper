using Net.Arqsoft.QsMapper;
using Samples.Model.Contacts;

namespace Samples
{
    public class SampleCatalog : Catalog
    {
        public SampleCatalog()
        {
            RegisterMap<Customer>()
                .ReadOnly("Name");
        }

    }
}
