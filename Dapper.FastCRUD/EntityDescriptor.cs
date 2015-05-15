namespace Dapper.FastCrud
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Globalization;
    using System.Linq;
    using Dapper.FastCrud.Providers;
    using Dapper.FastCrud.Providers.MsSql;

    /// <summary>
    /// Class for Dapper extensions
    /// </summary>
    internal class EntityDescriptor
    {
    }

    internal abstract class EntityDescriptor<TEntity> : EntityDescriptor
    {
        private readonly Dictionary<Type, IOperationDescriptor<TEntity>> _registeredOperations = new Dictionary<Type, IOperationDescriptor<TEntity>>();

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
            this.KeyPropertyDescriptors =
                this.SelectPropertyDescriptors.Where(propInfo => propInfo.Attributes.OfType<KeyAttribute>().Any()).ToArray();
            this.UpdateablePropertyDescriptors = this.SelectPropertyDescriptors.Except(this.KeyPropertyDescriptors).ToArray();

            this.TableDescriptor = TypeDescriptor.GetAttributes(entityType).OfType<TableAttribute>().SingleOrDefault() ??
                            new TableAttribute(entityType.Name);
        }

        public abstract string TableName { get; }

        public PropertyDescriptor[] SelectPropertyDescriptors { get; private set; }
        public PropertyDescriptor[] UpdateablePropertyDescriptors { get; private set; }
        public PropertyDescriptor[] KeyPropertyDescriptors { get; private set; }
        public TableAttribute TableDescriptor { get; private set; }

        protected void RegisterOperation<TOperation>(TOperation operation)
            where TOperation : IOperationDescriptor<TEntity>
        {
            _registeredOperations[typeof (TOperation)] = operation;
        }

        private static readonly List<Type> SimpleSqlTypes = new List<Type>
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

        public TOperation Operation<TOperation>() where TOperation : IOperationDescriptor<TEntity>
        {
            return (TOperation) _registeredOperations[typeof (TOperation)];
        }

        private static bool IsSimpleSqlType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            return type.IsEnum || SimpleSqlTypes.Contains(type);
        }

    }
}

