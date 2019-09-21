using System.Collections;
using System.Collections.Generic;

namespace Morpheus.Standard.UnitTests.Serialization
{
    public class CClassWithSortedLists
    {
        [AElementName( "Basic" )]
        public SortedList SimpleSortedList;

        [AElementName( "Sync" )]
        public SortedList SyncSortedList;
    }

    public class CClassWithHashtables
    {
        [AElementName( "Basic" )]
        public Hashtable SimpleHashtable;

        [AElementName( "Sync" )]
        public Hashtable SyncHashtable;
    }

    public class CClassWithStacks
    {
        [AElementName( "Basic" )]
        public Stack SimpleStack;

        [AElementName( "Sync" )]
        public Stack SyncStack;
    }

    public class CClassWithQueues
    {
        [AElementName( "Basic" )]
        public Queue SimpleQueue;

        [AElementName( "Sync" )]
        public Queue SyncQueue;
    }

    public class CClassWithArrayLists
    {
        [AElementName( "Basic" )]
        public ArrayList SimpleArrayList;

        [AElementName( "Sync" )]
        public ArrayList SyncArrayList;

        [AElementName( "Readonly" )]
        public ArrayList ReadOnlyArrayList;
    }

    public class CClassWithIList
    {
        public string Name = "Sweet Momma!";

        public List<CAddress> Addresses = new List<CAddress>();
    }

    public class CClassWithImproperIList
    {
        public string Name = "Salty Daddy!";

        public List<CAddress> Addresses = new List<CAddress>();
    }

    public class CClassWithBothTypesOfCollections
    {
        public Dictionary<string, CAddress> ByName = new Dictionary<string, CAddress>( 17 );

        [ATreatAsInterface( false )]
        public Stack AsStack = new Stack( 17 );
    }
}