namespace Dapper.FastCrud.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal sealed class ObjectMatchResult:IDisposable
    {
        private List<ObjectMatchMismatch> _mismatches = new List<ObjectMatchMismatch>();

        public ObjectMatchResult(object expected, object actual)
        {
            this.ExpectedObject = expected;
            this.ActualObject = actual;
        }

        public void RegisterMismatch(ObjectMatchMismatchType mismatchType, 
                                     int level,
                                     string mismatchLocation,
                                     object? expectedValue, 
                                     object? actualValue,
                                     params ObjectMatchResult[] deeperReason)
        {
            this.IsMatch = false;
            _mismatches.Add(new ObjectMatchMismatch()
                {
                    Level = level,
                    MismatchType = mismatchType,
                    MismatchLocation =  mismatchLocation,
                    DeeperReasons =  deeperReason,
                    ExpectedValue = expectedValue,
                    ActualValue = actualValue
                });
        }

        public void RegisterInterestedParty()
        {
            this.InterestedParties++;
        }

        public int InterestedParties { get; private set; }
        public object ExpectedObject { get; }
        public object ActualObject { get; }
        public ObjectMatchMismatch[] Mismatches => _mismatches.ToArray();
        public bool? IsMatch { get; set; }
        public int MatchingScore { get; set; } // 10 for each prop simply matched, 1 for matched collections or complex objects

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.InterestedParties--;
            if (this.InterestedParties == 0 && !this.IsMatch.HasValue)
            {
                this.IsMatch = _mismatches.Count == 0;
            }
        }
    }

    internal class ObjectMatchMismatch
    {
        public int Level;
        public ObjectMatchMismatchType MismatchType;
        public string MismatchLocation;
        public ObjectMatchResult[] DeeperReasons;
        public object ExpectedValue;
        public object ActualValue;

        public override string ToString()
        {
            return $"{this.MismatchType} @{this.MismatchLocation}";
        }
    }

    public enum ObjectMatchMismatchType
    {
        Type,
        PropertyValue,
        CollectionLength,
        CollectionElementNotFound,
        CollectionElementOrder
    }
}
