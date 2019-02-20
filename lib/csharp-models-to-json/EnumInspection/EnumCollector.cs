using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpModelsToJson.EnumInspection
{
    public class EnumCollector : CSharpSyntaxWalker
    {
        public readonly List<EnumInfo> Enums = new List<EnumInfo>();

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var item = new EnumInfo
            {
                Identifier = node.Identifier.ToString(),
                Items = node.Members.Select(val => new EnumInfoItem
                {
                    Name = val.Identifier.ValueText,
                    Value = int.Parse(val.EqualsValue.Value.ToString())
                })
            };

            Enums.Add(item);
        }
    }
}