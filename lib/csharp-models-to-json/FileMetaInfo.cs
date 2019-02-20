using System.Collections.Generic;
using CSharpModelsToJson.EnumInspection;
using CSharpModelsToJson.ModelInspection;

namespace CSharpModelsToJson
{
    public class FileMetaInfo
    {
        public string FileName { get; set; }
        
        public IEnumerable<ModelInfo> Models { get; set; }
        
        public IEnumerable<EnumInfo> Enums { get; set; }
    }
}