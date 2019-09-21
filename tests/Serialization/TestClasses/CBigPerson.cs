using System;

namespace Morpheus.Standard.UnitTests.Serialization
{
    public class CBigPerson
    {
        public enum ERole
        {
            Parent, Child, Unknown
        }

        public string Name = "Homer Simpson";
        public int Age = 40;
        public double Height = 5.87;
        public bool IsParent = true;
        public string[] KidsNames = new string[] { "Bart", "Lisa", "Maggie" };
        public int[] KidsAges = new int[] { 10, 7, 2 };

        public CFriend[] Friends;
        public byte[] Numbers;

        public ERole Role = ERole.Unknown;


        public static CBigPerson[] People;

        public static void GenerateData( int _personCount )
        {
            var rng = new Random( 12345 );

            People = new CBigPerson[_personCount];

            for (var i = 0; i < _personCount; i++)
            {
                var p = new CBigPerson
                {
                    Name = "Person " + i,
                    Age = rng.Next( 10, 100 ),
                    Height = rng.NextDouble(),
                    IsParent = rng.Next( 2 ) == 0
                };

                var kidCount = rng.Next( 0, 4 ) + rng.Next( 0, 4 );
                p.KidsAges = new int[kidCount];
                p.KidsNames = new string[kidCount];

                for (var j = 0; j < kidCount; j++)
                {
                    p.KidsNames[j] = "Kidname " + j;
                    p.KidsAges[j] = rng.Next( 3, 18 );
                }

                p.Numbers = new byte[rng.Next( 1, 50 )];
                rng.NextBytes( p.Numbers );

                People[i] = p;
            }

            for (var i = 0; i < _personCount; i++)
            {
                var numFriends = rng.Next( 1, 12 ) + rng.Next( 1, 12 );
                People[i].Friends = new CFriend[numFriends];

                for (var j = 0; j < numFriends; j++)
                {
                    var f = new CFriend
                    {
                        IsBestFriend = rng.Next( 2 ) == 0,
                        Rating = rng.Next( 1000000 ),
                        FriendPerson = People[rng.Next( _personCount )]
                    };

                    People[i].Friends[j] = f;
                }
            }
        }
    }

    public class CFriend
    {
        public bool IsBestFriend;
        public int Rating;
        public CBigPerson FriendPerson;
    }
}