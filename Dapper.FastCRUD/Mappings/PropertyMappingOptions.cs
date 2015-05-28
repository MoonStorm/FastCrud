namespace Dapper.FastCrud.Mappings
{
    using System;

    [Flags]
    public enum PropertyMappingOptions
    {
        Regular = 0x00,
        KeyProperty = 0x01,
        DatabaseGeneratedProperty = 0x02,
        ExcludedFromUpdates = 0x04,
    }
}
