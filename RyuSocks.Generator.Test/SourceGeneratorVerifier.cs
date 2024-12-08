using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace RyuSocks.Generator.Test
{
    public static class SourceGeneratorVerifier<TSourceGenerator>
        where TSourceGenerator : new()
    {
        // ReSharper disable once StaticMemberInGenericType
        public static readonly List<(string filename, string content)> PostInitGeneratedSources = [];

        private static void AddGeneratedSources(SourceFileCollection collection, IEnumerable<(string filename, string content)> generatedSources)
        {
            foreach ((string filename, string content) in generatedSources)
            {
                collection.Add((typeof(TSourceGenerator), filename, content));
            }
        }

        public static Task VerifyGeneratedSources(string source, params (string filename, string content)[] generatedSources)
        {
            var test = new Test
            {
                TestCode = source
            };

            AddGeneratedSources(test.TestState.GeneratedSources, PostInitGeneratedSources);
            AddGeneratedSources(test.TestState.GeneratedSources, generatedSources);

            return test.RunAsync();
        }


        public class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
        {
            protected override CompilationOptions CreateCompilationOptions()
            {
                var compilationOptions = base.CreateCompilationOptions();
                return compilationOptions.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
            }

            private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
            {
                string[] args = {"/warnaserror:nullable"};
                var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
                var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

                return nullableWarnings;
            }
        }
    }
}
