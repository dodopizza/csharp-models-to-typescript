using System.Linq;
using CSharpModelsToJson.ModelInspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CSharpModelsToJson.Tests
{
    [TestFixture]
    public class ModelCollectorTest
    {
        [Test]
        public void BasicInheritance_ReturnsInheritedClass()
        {
            const string baseClasses = "B, C, D";

            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : B, C, D
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.IsNotNull(modelCollector.Models);
            Assert.AreEqual(modelCollector.Models.First().BaseClasses, baseClasses);
        }

        [Test]
        public void InterfaceImport_ReturnsSyntaxClassFromInterface()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public interface IPhoneNumber {
                    string Label { get; set; }
                    string Number { get; set; }
                    int MyProperty { get; set; }
                }

                public interface IPoint
                {
                   // Property signatures:
                   int x
                   {
                      get;
                      set;
                   }

                   int y
                   {
                      get;
                      set;
                   }
                }


                public class X {
                    public IPhoneNumber test { get; set; }
                    public IPoint test2 { get; set; }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.Visit(root);

            Assert.IsNotNull(modelCollector.Models);
            Assert.AreEqual(modelCollector.Models.Count, 3);
            Assert.AreEqual(modelCollector.Models.First().Properties.Count(), 3);
        }


        [Test]
        public void TypedInheritance_ReturnsInheritance()
        {
            const string baseClasses = "IController<Controller>";

            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    public void AMember()
                    {
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.IsNotNull(modelCollector.Models);
            Assert.AreEqual(modelCollector.Models.First().BaseClasses, baseClasses);
        }

        [Test]
        public void AccessibilityRespected_ReturnsPublicOnly()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A : IController<Controller>
                {
                    public void AMember()
                    {
                        const A_Constant = 0;

                        private string B { get; set }

                        static string C { get; set }

                        public string Included { get; set }
                    }
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            modelCollector.VisitClassDeclaration(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First());

            Assert.IsNotNull(modelCollector.Models);
            Assert.IsNotNull(modelCollector.Models.First().Properties);
            Assert.AreEqual(modelCollector.Models.First().Properties.Count(), 1);
        }

        [Test]
        public void ObsoleteFields_ShouldBeMarkedObsolete()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
                public class A
                {
                    public int NonObsoleteProperty { get; set; }

                    [Obsolete]
                    public int ObsoleteProperty { get; set; }

                    [Obsolete]
                    public int ObsoleteField;
                }"
            );

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var modelCollector = new ModelCollector();
            modelCollector.Visit(root);
            var modelProperties = modelCollector.Models.Single().Properties.ToArray();
            var modelFields = modelCollector.Models.Single().Fields.ToArray();

            Assert.That(modelProperties[0].Identifier, Is.EqualTo("NonObsoleteProperty"));
            Assert.That(modelProperties[0].IsObsolete, Is.False);

            Assert.That(modelProperties[1].Identifier, Is.EqualTo("ObsoleteProperty"));
            Assert.That(modelProperties[1].IsObsolete, Is.True);

            Assert.That(modelFields[0].Identifier, Is.EqualTo("ObsoleteField"));
            Assert.That(modelFields[0].IsObsolete, Is.True);
        }
    }
}