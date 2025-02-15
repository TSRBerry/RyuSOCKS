using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace RyuSocks.Generator.Test
{
    [ExcludeFromCodeCoverage]
    public static class SourceGeneratorVerifier<TSourceGenerator>
        where TSourceGenerator : new()
    {
        public static Task VerifyGeneratedSources(string source, (string filename, string content)[] generatedSources, DiagnosticResult[] expectedDiagnostics = null)
        {
            var test = new Test
            {
                TestCode = source
            };

            foreach ((string filename, string content) in generatedSources)
            {
                test.TestState.GeneratedSources.Add((typeof(TSourceGenerator), filename, content));
            }

            if (expectedDiagnostics != null)
            {
                test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
            }

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
                string[] args = ["/warnaserror:nullable"];
                var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
                return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
            }
        }
    }
}
