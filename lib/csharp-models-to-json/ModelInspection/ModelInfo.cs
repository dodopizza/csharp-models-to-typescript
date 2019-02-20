using System.Collections.Generic;

namespace CSharpModelsToJson.ModelInspection
{
    public class ModelInfo
    {
        public string ModelName { get; set; }

        public IEnumerable<FieldInfo> Fields { get; set; }

        public IEnumerable<PropertyInfo> Properties { get; set; }

        public string BaseClasses { get; set; }
    }
}