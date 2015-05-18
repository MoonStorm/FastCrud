namespace Dapper.FastCrud
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Dapper.FastCrud.Providers;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class EntityDescriptor
    {
    }

    internal abstract class EntityDescriptor<TEntity> : EntityDescriptor
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected EntityDescriptor()
        {
            Type entityType = typeof (TEntity);
            this.SelectPropertyDescriptors =
                TypeDescriptor.GetProperties(entityType)
                    .Cast<PropertyDescriptor>()
                    .Where(
                        p =>
                            IsSimpleSqlType(p.PropertyType)
                            && p.Attributes.OfType<EditableAttribute>().All(editableAttr => editableAttr.AllowEdit))
                    .ToArray();
            this.KeyPropertyDescriptors = this.SelectPropertyDescriptors.Where(propInfo => propInfo.Attributes.OfType<KeyAttribute>().Any()).ToArray();
            this.TableDescriptor = TypeDescriptor.GetAttributes(entityType)
                .OfType<TableAttribute>().SingleOrDefault() ?? new TableAttribute(entityType.Name);
            this.DatabaseGeneratedPropertyDescriptors = this.SelectPropertyDescriptors
                .Where(propInfo => propInfo.Attributes.OfType<DatabaseGeneratedAttribute>()
                .Any(dbGenerated => dbGenerated.DatabaseGeneratedOption==DatabaseGeneratedOption.Computed || dbGenerated.DatabaseGeneratedOption==DatabaseGeneratedOption.Identity))
                .ToArray();

            // everything can be updateable, with the exception of the primary keys
            this.UpdatePropertyDescriptors = this.SelectPropertyDescriptors.Except(this.KeyPropertyDescriptors).ToArray();

            // we consider properties that go into an insert only the ones that are not auto-generated 
            this.InsertPropertyDescriptors = this.SelectPropertyDescriptors.Except(this.DatabaseGeneratedPropertyDescriptors).ToArray();
        }

        public abstract string TableName { get; }

        public PropertyDescriptor[] DatabaseGeneratedPropertyDescriptors { get; private set; }
        public PropertyDescriptor[] SelectPropertyDescriptors { get; private set; }
        public PropertyDescriptor[] InsertPropertyDescriptors { get; private set; }
        public PropertyDescriptor[] UpdatePropertyDescriptors { get; private set; }
        public PropertyDescriptor[] KeyPropertyDescriptors { get; private set; }
        public TableAttribute TableDescriptor { get; private set; }

        public ISingleDeleteEntityOperationDescriptor<TEntity> SingleDeleteOperation { get; set; }
        public ISingleSelectEntityOperationDescriptor<TEntity> SingleSelectOperation { get; set; }
        public ISingleInsertEntityOperationDescriptor<TEntity> SingleInsertOperation { get; set; }
        public ISingleUpdateEntityOperationDescriptor<TEntity> SingleUpdateOperation { get; set; }
        public IBatchSelectEntityOperationDescriptor<TEntity> BatchSelectOperation { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DynamicParameters CreateParameters<T>(PropertyDescriptor[] propDescriptors, T source)
        {
            var parameters = new DynamicParameters();
            foreach (var propDescriptor in propDescriptors)
            {
                parameters.Add("@"+propDescriptor.Name, propDescriptor.GetValue(source));
            }

            return parameters;
        }

        private static readonly Type[] SimpleSqlTypes = new[]
        {
            typeof (byte),
            typeof (sbyte),
            typeof (short),
            typeof (ushort),
            typeof (int),
            typeof (uint),
            typeof (long),
            typeof (ulong),
            typeof (float),
            typeof (double),
            typeof (decimal),
            typeof (bool),
            typeof (string),
            typeof (char),
            typeof (Guid),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (byte[])
        };

        private static bool IsSimpleSqlType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            return type.IsEnum || SimpleSqlTypes.Contains(type);
        }

    }
}

