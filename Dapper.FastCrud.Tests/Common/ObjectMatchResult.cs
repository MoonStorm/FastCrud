namespace Dapper.FastCrud.Tests.Common
{
    using System.Collections.Generic;

    internal class ObjectMatchResult
    {
        private List<ObjectMatchMismatch> _mismatches = new List<ObjectMatchMismatch>();

        public ObjectMatchResult(object expected, object actual)
        {
            this.ExpectedObject = expected;
            this.ActualObject = actual;
        }

        public void RegisterMismatch(ObjectMatchMismatchType mismatchType, 
                                     string mismatchLocation,
                                     object? expectedValue, 
                                     object? actualValue, object? extraInfo = null)
        {
            _mismatches.Add(new ObjectMatchMismatch()
                {
                    MismatchType = mismatchType,
                    MismatchLocation =  mismatchLocation,
                    ExtraInfo =  extraInfo,
                    ExpectedValue = expectedValue,
                    ActualValue = actualValue
                });
        }

        public object ExpectedObject { get; }
        public object ActualObject { get; }

        public ObjectMatchMismatch[] Mismatches => _mismatches.ToArray();

        public bool IsMatch => this.Mismatches.Length == 0;
    }

    public class ObjectMatchMismatch
    {
        public ObjectMatchMismatchType MismatchType;
        public string MismatchLocation;
        public object? ExtraInfo;
        public object? ExpectedValue;
        public object? ActualValue;

        public override string ToString()
        {
            return $"{this.MismatchType} @{this.MismatchLocation}: expected {this.ExpectedValue}, actual {this.ActualValue}), extra info {this.ExtraInfo} ";
        }
    }

    public enum ObjectMatchMismatchType
    {
        Type,
        PropertyValue,
        CollectionLength,
        CollectionElement,
        CollectionElementOrder
    }
}
