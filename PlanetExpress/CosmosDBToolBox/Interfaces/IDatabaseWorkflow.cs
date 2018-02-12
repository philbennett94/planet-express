using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetExpress.Interfaces
{
    interface IDatabaseWorkflow
    {
        void CreateDatabaseWF();
        void DeleteDatabaseWF();
        void ListDatabasesWF();
        void CreateCollectionWF();
        void DeleteCollectionWF();
        void ListCollectionsWF();
        void InsertOneOrManyWF();
    }
}
