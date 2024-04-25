/*
 * Copyright (C) RyuSOCKS
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2,
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace RyuSocks.Generator
{
    [Generator]
    public class AuthMethodGenerator : IIncrementalGenerator
    {
        private const string Namespace = "RyuSocks.Auth";
        private const string AuthMethodImplAttributeName = "AuthMethodImplAttribute";
        private const string ProxyAuthInterfaceName = "IProxyAuth";
        private const string AuthMethodEnumName = "AuthMethod";
        private const string AuthMethodExtensionsClassName = "AuthMethodExtensions";

        private const string AttributeText = @"
using System;

namespace %NAMESPACE%
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [System.Diagnostics.Conditional(""RyuSocks_AuthMethodGenerator_DEBUG"")]
    public sealed class %ATTRIBUTE_NAME% : Attribute
    {
        private byte methodId;

        /// <summary>
        /// Marks this class as an authentication method. It must have a parameterless constructor and extend <see cref=""%INTERFACE_NAME%""/>.
        /// </summary>
        /// <param name=""methodId"">The value between 0x00 and 0xFF used to identify this authentication method.</param>
        public %ATTRIBUTE_NAME%([System.ComponentModel.DataAnnotations.DeniedValues(0xFF)] byte methodId)
        {
            this.methodId = methodId;
        }

        /// <summary>
        /// The name for this authentication method used in the %ENUM_NAME% enum.
        /// </summary>
        public string MethodName { get; set; }

        public byte MethodId => methodId;
    }
}
";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{AuthMethodImplAttributeName}.g.cs",
                    AttributeText
                            .Replace("%NAMESPACE%", Namespace)
                            .Replace("%ATTRIBUTE_NAME%", AuthMethodImplAttributeName)
                            .Replace("%INTERFACE_NAME%", ProxyAuthInterfaceName)
                            .Replace("%ENUM_NAME%", AuthMethodEnumName)
                )
            );

            // Create an incremental value provider using the generated attribute
            var authMethodProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{Namespace}.{AuthMethodImplAttributeName}",
                IsAuthMethodClass,
                TransformAuthMethodClass
            );

            context.RegisterSourceOutput(authMethodProvider.Collect(), ProduceSourceCode);
        }

        /// <summary>
        /// Check whether the provided <paramref name="syntaxNode"/> implements the proxy auth interface.
        /// </summary>
        /// <param name="syntaxNode">The syntax node to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>True</c> if <paramref name="syntaxNode"/> implements the proxy auth interface.</returns>
        private static bool IsAuthMethodClass(SyntaxNode syntaxNode, CancellationToken cancellationToken) =>
            syntaxNode is ClassDeclarationSyntax { BaseList.Types.Count: > 0 } classDeclaration
                   && classDeclaration.BaseList.Types.Any(type => type.Type.ToString() == ProxyAuthInterfaceName);

        private static AuthMethodModel TransformAuthMethodClass(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var classSymbol = context.TargetSymbol;
            var attributeData = context.Attributes[0];

            if (attributeData.ConstructorArguments.Length == 0)
            {
                throw new InvalidOperationException($"Attribute constructor doesn't have arguments: {attributeData.AttributeClass?.Name}");
            }

            byte methodId = (byte)attributeData.ConstructorArguments[0].Value!;
            TypedConstant memberName = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "MethodName").Value;

            if (!memberName.IsNull)
            {
                return new AuthMethodModel(
                    methodId,
                    (string)memberName.Value,
                    classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                );
            }

            return new AuthMethodModel(
                methodId,
                classSymbol.Name,
                classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );
        }

        private static void ProduceSourceCode(SourceProductionContext context, ImmutableArray<AuthMethodModel> authMethods)
        {
            // Begin building the generated source
            CodeBuilder source = new();
            CodeBuilder sourceExtensions = new();
            BlockBuilder implToEnumBlock = new();
            source.EnterScope($"namespace {Namespace}");
            source.EnterScope($"public enum {AuthMethodEnumName} : byte");
            sourceExtensions.AppendLine($"using {Namespace};");
            sourceExtensions.AppendLine("using System;");
            sourceExtensions.AppendLine();
            sourceExtensions.EnterScope($"namespace {Namespace}.Extensions");
            sourceExtensions.EnterScope($"public static class {AuthMethodExtensionsClassName}");
            sourceExtensions.EnterScope($"public static {ProxyAuthInterfaceName} GetAuth(this {AuthMethodEnumName} authMethod) => authMethod switch");
            implToEnumBlock.EnterScope($"public static {AuthMethodEnumName} GetAuth(this {ProxyAuthInterfaceName} authImpl) => authImpl switch");

            bool firstMember = true;
            byte lastMemberValue = 0;

            // TODO: Filter duplicates and report diagnostics using an analyzer

            // Sort the models by ID and generate the source
            foreach (var authMethod in authMethods.Sort((model, methodModel) => model.Id.CompareTo(methodModel.Id)))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (firstMember)
                {
                    firstMember = false;

                    if (authMethod.Id == 0)
                    {
                        source.AppendLine($"{authMethod.MemberName},");
                    }
                    else
                    {
                        source.AppendLine($"{authMethod.MemberName} = 0x{authMethod.Id:X2},");
                    }

                    sourceExtensions.AppendLine($"{AuthMethodEnumName}.{authMethod.MemberName} => new {authMethod.ClassName}(),");
                    implToEnumBlock.AppendLine($"{authMethod.ClassName} => {AuthMethodEnumName}.{authMethod.MemberName},");
                    lastMemberValue = authMethod.Id;

                    continue;
                }

                // Add a comment for IANA assigned methods
                if (lastMemberValue <= 0x02 && authMethod.Id >= 0x03)
                {
                    source.AppendLine();
                    source.AppendLine("// 0x03 - 0x7F: IANA assigned (https://www.iana.org/assignments/socks-methods/socks-methods.xhtml)");
                }

                // Add a comment for private methods
                if (lastMemberValue <= 0x7F && authMethod.Id >= 0x80)
                {
                    source.AppendLine();
                    source.AppendLine("// 0x80 - 0xFE: Reserved for private methods");
                }

                // Add comments for unassigned values in between
                if (lastMemberValue < authMethod.Id - 1)
                {
                    int firstUnassigned = lastMemberValue + 1;
                    int lastUnassigned = authMethod.Id - 1;

                    if (firstUnassigned == lastUnassigned)
                    {
                        source.AppendLine($"// 0x{firstUnassigned:X2}: Unassigned");
                    }
                    else
                    {
                        source.AppendLine($"// 0x{firstUnassigned:X2} - 0x{lastUnassigned:X2}: Unassigned");
                    }
                }

                if (authMethod.Id == lastMemberValue + 1)
                {
                    source.AppendLine($"{authMethod.MemberName},");
                }
                else
                {
                    source.AppendLine($"{authMethod.MemberName} = 0x{authMethod.Id:X2},");
                }

                sourceExtensions.AppendLine($"{AuthMethodEnumName}.{authMethod.MemberName} => new {authMethod.ClassName}(),");
                implToEnumBlock.AppendLine($"{authMethod.ClassName} => {AuthMethodEnumName}.{authMethod.MemberName},");
                lastMemberValue = authMethod.Id;
            }

            // Add a comment for private methods if missing
            if (lastMemberValue <= 0x7F)
            {
                source.AppendLine();
                source.AppendLine("// 0x80 - 0xFE: Reserved for private methods");
            }

            // Finish building the generated source
            source.AppendLine();
            source.AppendLine("NoAcceptableMethods = 0xFF,");
            source.LeaveScope();
            source.LeaveScope();
            sourceExtensions.AppendLine("_ => throw new ArgumentException($\"Invalid authentication method provided: {authMethod}\", nameof(authMethod)),");
            sourceExtensions.LeaveScope(";");
            implToEnumBlock.AppendLine("_ => throw new ArgumentException($\"Unknown authentication implementation provided: {authImpl}\", nameof(authImpl)),");
            implToEnumBlock.LeaveScope(";");
            sourceExtensions.AppendBlock(implToEnumBlock.GetLines());
            sourceExtensions.LeaveScope();
            sourceExtensions.LeaveScope();

            // Add the source
            context.AddSource($"{AuthMethodEnumName}.g.cs", source.ToString());
            context.AddSource($"{AuthMethodExtensionsClassName}.g.cs", sourceExtensions.ToString());
        }

        private record struct AuthMethodModel(byte Id, string MemberName, string ClassName);
    }
}
