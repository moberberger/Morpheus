using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.Standard.UnitTests.Serialization
{
    public enum ETestStatus
    {
        NONE = 0,

        EXTERNAL_SERIALIZER = 1,
        EXTERNAL_DESERIALIZER,
        EXTERNAL_SERIALIZER_INCOMPLETE,
        EXTERNAL_DESERIALIZER_INCOMPLETE,

        IMPLICIT_SERIALIZER,
        IMPLICIT_DESERIALIZER,
        IMPLICIT_SERIALIZER_INCOMPLETE,
        IMPLICIT_DESERIALIZER_INCOMPLETE,
        IMPLICIT_SERIALIZER_VOID,
        IMPLICIT_DESERIALIZER_VOID,
    }

}
