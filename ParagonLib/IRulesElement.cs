using System;

namespace ParagonLib
{
    public interface IRulesElement
    {
        string Name { get; }

        string Type { get; }

        string Source { get; }

        string System { get; }

        string InternalId { get; }

        string[] Category { get; }

    }
}

