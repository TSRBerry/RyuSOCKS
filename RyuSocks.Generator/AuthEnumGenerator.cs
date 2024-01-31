using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RyuSocks.Generator
{
    // Based on: https://github.com/dotnet/roslyn-sdk/blob/8c5ae6fcf79914d0671b15c562db22b014c9c00e/samples/CSharp/SourceGenerators/SourceGeneratorSamples/AutoNotifyGenerator.cs
    [Generator]
    public class AuthEnumGenerator : ISourceGenerator
    {
        private const string Namespace = "RyuSocks.Auth";
        private const string AuthMethodImplAttributeName = "AuthMethodImplAttribute";
        private const string ProxyAuthInterfaceName = "IProxyAuth";

        private const string AttributeText = @"
using System;

namespace %NAMESPACE%
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [System.Diagnostics.Conditional(""RyuSocks_AuthEnumGenerator_DEBUG"")]
    public sealed class %ATTRIBUTE_NAME% : Attribute
    {
        private byte methodId;

        /// <summary>
        /// Mark this class as an authentication method. It must extend <see cref=""IProxyAuth""/>.
        /// </summary>
        /// <param name=""methodId"">The value between 0x00 and 0xFF used to identify this authentication method.</param>
        public %ATTRIBUTE_NAME%([System.ComponentModel.DataAnnotations.DeniedValues(0xFF)] byte methodId)
        {
            this.methodId = methodId;
        }

        /// <summary>
        /// The name for this authentication method used in the AuthMethod enum.
        /// </summary>
        public string MethodName { get; set; }

        public byte MethodId => methodId;
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterForPostInitialization((i) => i.AddSource(
                    "AuthMethodImplAttribute.g.cs",
                    AttributeText
                            .Replace("%NAMESPACE%", Namespace)
                            .Replace("%ATTRIBUTE_NAME%", AuthMethodImplAttributeName)
                )
            );

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new AuthMethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Retrieve the populated receiver
            if (context.SyntaxContextReceiver is not AuthMethodSyntaxReceiver receiver)
            {
                return;
            }

            // Get the added attribute
            INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName($"{Namespace}.{AuthMethodImplAttributeName}");

            // Begin building the generated source
            CodeBuilder source = new();
            CodeBuilder sourceExtensions = new();
            source.EnterScope($"namespace {Namespace}");
            source.EnterScope("public enum AuthMethod : byte");
            sourceExtensions.AppendLine($"using {Namespace};");
            sourceExtensions.AppendLine("using System;");
            sourceExtensions.AppendLine();
            sourceExtensions.EnterScope($"namespace {Namespace}.Extensions");
            sourceExtensions.EnterScope("public static class AuthMethodExtensions");
            sourceExtensions.EnterScope($"public static {ProxyAuthInterfaceName} GetAuth(this AuthMethod authMethod) => authMethod switch");

            bool firstMember = true;
            byte lastMemberValue = 0;

            receiver.Classes.Sort(CompareMethodId);

            // Sort the classes by MethodId, and generate the source
            foreach (INamedTypeSymbol classSymbol in receiver.Classes)
            {
                var attributeData = GetAttributeData(classSymbol, attributeSymbol);

                if (firstMember)
                {
                    firstMember = false;

                    if (attributeData.Item1 == 0)
                    {
                        source.AppendLine($"{attributeData.Item2},");
                    }
                    else
                    {
                        source.AppendLine($"{attributeData.Item2} = 0x{attributeData.Item1:X2},");
                    }

                    sourceExtensions.AppendLine($"AuthMethod.{attributeData.Item2} => new {classSymbol.ToDisplayString()}(),");
                    lastMemberValue = attributeData.Item1;

                    continue;
                }

                // Add a comment for IANA assigned methods
                if (lastMemberValue <= 0x02 && attributeData.Item1 >= 0x03)
                {
                    source.AppendLine();
                    source.AppendLine("// 0x03 - 0x7F: IANA assigned (https://www.iana.org/assignments/socks-methods/socks-methods.xhtml)");
                }

                // Add a comment for private methods
                if (lastMemberValue <= 0x7F && attributeData.Item1 >= 0x80)
                {
                    source.AppendLine();
                    source.AppendLine("// 0x80 - 0xFE: Reserved for private methods");
                }

                // Add comments for unassigned values in between
                if (lastMemberValue < attributeData.Item1 - 1)
                {
                    int firstUnassigned = lastMemberValue + 1;
                    int lastUnassigned = attributeData.Item1 - 1;

                    if (firstUnassigned == lastUnassigned)
                    {
                        source.AppendLine($"// 0x{firstUnassigned:X2}: Unassigned");
                    }
                    else
                    {
                        source.AppendLine($"// 0x{firstUnassigned:X2} - 0x{lastUnassigned:X2}: Unassigned");
                    }
                }

                if (attributeData.Item1 == lastMemberValue + 1)
                {
                    source.AppendLine($"{attributeData.Item2},");
                }
                else
                {
                    source.AppendLine($"{attributeData.Item2} = 0x{attributeData.Item1:X2},");
                }

                sourceExtensions.AppendLine($"AuthMethod.{attributeData.Item2} => new {classSymbol.ToDisplayString()}(),");
                lastMemberValue = attributeData.Item1;
            }

            // Finish building the generated source
            source.AppendLine();
            source.AppendLine("NoAcceptableMethods = 0xFF,");
            source.LeaveScope();
            source.LeaveScope();
            sourceExtensions.AppendLine("_ => throw new ArgumentException($\"Invalid authentication method provided: {authMethod}\", nameof(authMethod)),");
            sourceExtensions.LeaveScope(";");
            sourceExtensions.LeaveScope();
            sourceExtensions.LeaveScope();

            // Add the source
            context.AddSource("AuthMethod.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
            context.AddSource("AuthMethodExtensions.g.cs", SourceText.From(sourceExtensions
                .ToString(), Encoding.UTF8));

            return;

            int CompareMethodId(INamedTypeSymbol a, INamedTypeSymbol b)
            {
                return ((byte)a.GetAttributes()
                    .Single(ad => ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                    .ConstructorArguments[0].Value!).CompareTo((byte)b.GetAttributes()
                    .Single(ad => ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                    .ConstructorArguments[0].Value!);
            }
        }

        private static Tuple<byte, string> GetAttributeData(INamedTypeSymbol classSymbol, INamedTypeSymbol attributeSymbol)
        {
            AttributeData attributeData = classSymbol.GetAttributes().Single(ad =>
                ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));

            if (attributeData.ConstructorArguments.Length == 0)
            {
                throw new ArgumentException("Attribute constructor doesn't have arguments.", nameof(attributeData));
            }

            byte methodId = (byte)attributeData.ConstructorArguments[0].Value!;
            TypedConstant memberName = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "MethodName").Value;

            if (!memberName.IsNull)
            {
                return new Tuple<byte, string>(methodId, (string)memberName.Value);
            }

            return new Tuple<byte, string>(methodId, classSymbol.Name);
        }

        class AuthMethodSyntaxReceiver : ISyntaxContextReceiver
        {
            public List<INamedTypeSymbol> Classes { get; } = [];

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // Any class with at least one base type and at least one attribute is a candidate for enum member generation
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Count > 0
                    && classDeclarationSyntax.BaseList?.Types.Count > 0)
                {
                    // Get the symbol being declared by the class, and keep it if it implements IProxyAuth and is annotated
                    INamedTypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                    if (classSymbol!.AllInterfaces.Any(nts => nts.ToDisplayString() == $"{Namespace}.{ProxyAuthInterfaceName}")
                        && classSymbol.GetAttributes().Any(ad => ad.AttributeClass!.ToDisplayString() == $"{Namespace}.{AuthMethodImplAttributeName}"))
                    {
                        Classes.Add(classSymbol);
                    }
                }
            }
        }
    }
}
