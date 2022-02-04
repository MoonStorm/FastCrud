namespace Dapper.FastCrud.Mappings.Registrations
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Type of relationship between two entities.
    /// </summary>
    internal enum EntityRelationshipType
    {
        /// <summary>
        /// A child-to-parent relationship.
        /// </summary>
        ChildToParent,

        /// <summary>
        /// A parent-to-children relationship.
        /// </summary>
        ParentToChildren
    }
}
