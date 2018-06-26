using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Vision
{
    public class LabelCount
    {
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }

        [JsonProperty(PropertyName = "timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        public LabelCount()
        {
            //Enforces document order when transported to cosmosDB
            this.Id = null;
        }

        public override bool Equals(object obj)
        {
            var count = obj as LabelCount;
            return count != null &&
                   Label == count.Label &&
                   Count == count.Count;
        }

        public override int GetHashCode()
        {
            var hashCode = 476689593;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            hashCode = hashCode * -1521134295 + Count.GetHashCode();
            return hashCode;
        }

    }
}
