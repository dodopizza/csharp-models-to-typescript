using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpModelsToJson.EnumInspection;
using CSharpModelsToJson.ModelInspection;
using Ganss.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace CSharpModelsToJson
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(args[0]));

            var includes = config.Include ?? Enumerable.Empty<string>();
            var excludes = config.Exclude ?? Enumerable.Empty<string>();

            var files = GetFileNames(includes, excludes).Select(ParseFile);
            var json = JsonConvert.SerializeObject(files);
            
            Console.WriteLine(json);
        }

        private static IEnumerable<string> GetFileNames(IEnumerable<string> includes, IEnumerable<string> excludes)
        {
            var fileNames = new List<string>();

            foreach (var path in ExpandGlobPatterns(includes))
            {
                fileNames.Add(path);
            }

            foreach (var path in ExpandGlobPatterns(excludes))
            {
                fileNames.Remove(path);
            }

            return fileNames;
        }

        private static IEnumerable<string> ExpandGlobPatterns(IEnumerable<string> globPatterns)
        {
            var glob = new Glob();
            var fileNames = new List<string>();

            foreach (var pattern in globPatterns)
            {
                var paths = glob.Expand(pattern);

                fileNames.AddRange(paths.Select(path => path.FullName));
            }

            return fileNames;
        }

        private static FileMetaInfo ParseFile(string path)
        {
            var source = File.ReadAllText(path);
            var tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var modelCollector = new ModelCollector();
            var enumCollector = new EnumCollector();

            modelCollector.Visit(root);
            enumCollector.Visit(root);

            return new FileMetaInfo
            {
                FileName = Path.GetFullPath(path),
                Models = modelCollector.Models,
                Enums = enumCollector.Enums
            };
        }
    }
}