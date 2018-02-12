using Microsoft.Azure.CosmosDB.Table;

namespace PlanetExpress
{
    class TestEntity : TableEntity
    {
        string firstName;
        string title;
        int pageCount;
        public TestEntity(string id, string authorFirstName, string authorLastName, string title, int pageCount) {
            this.PartitionKey = id;
            this.RowKey       = authorLastName;
            this.firstName    = authorFirstName;
            this.title        = title;
            this.pageCount    = pageCount;
        }
    }
}
