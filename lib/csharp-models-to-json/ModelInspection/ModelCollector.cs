using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpModelsToJson.ModelInspection
{
    public class ModelCollector : CSharpSyntaxWalker
    {
        public readonly List<ModelInfo> Models = new List<ModelInfo>();

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var model = GetModel(node);

            Models.Add(model);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var model = GetModel(node);

            Models.Add(model);
        }

        private static ModelInfo GetModel(TypeDeclarationSyntax node)
        {
            return new ModelInfo
            {
                ModelName = node.Identifier.ToString(),
                Fields = node.Members.OfType<FieldDeclarationSyntax>()
                    .Where(field => IsAccessible(field.Modifiers))
                    .Select(ConvertField),
                Properties = node.Members.OfType<PropertyDeclarationSyntax>()
                    .Where(property => IsAccessible(property) && IsAccessible(property.Modifiers))
                    .Select(ConvertProperty),
                BaseClasses = node.BaseList?.Types.ToString()
            };
        }

        private static bool IsAccessible(SyntaxTokenList modifiers)
        {
            return modifiers.All(modifier =>
                modifier.ToString() != "const" &&
                modifier.ToString() != "static" &&
                modifier.ToString() != "private"
            );
        }

        private static bool IsAccessible(BasePropertyDeclarationSyntax property)
        {
            return property.AccessorList.Accessors.Count == 2;
        }

        private static FieldInfo ConvertField(FieldDeclarationSyntax field)
        {
            return new FieldInfo
            {
                Identifier = field.Declaration.Variables.First().GetText().ToString(),
                Type = field.Declaration.Type.ToString(),
            };
        }

        private static PropertyInfo ConvertProperty(PropertyDeclarationSyntax property)
        {
            return new PropertyInfo
            {
                Identifier = property.Identifier.ToString(),
                Type = property.Type.ToString(),
            };
        }
    }
}