using System.Collections.Generic;

namespace CSharpModelsToJson.EnumInspection
{
    public class EnumInfo
    {
        public string Identifier { get; set; }

        public IEnumerable<EnumInfoItem> Items { get; set; }
    }
}