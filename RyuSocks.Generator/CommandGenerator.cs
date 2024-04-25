using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace RyuSocks.Generator
{
    [Generator]
    public class CommandGenerator : IIncrementalGenerator
    {
        private const string Namespace = "RyuSocks.Commands";
        private const string ProxyCommandImplAttributeName = "ProxyCommandImplAttribute";
        private const string ClientCommandClassName = "ClientCommand";
        private const string ServerCommandClassName = "ServerCommand";
        private const string ProxyCommandEnumName = "ProxyCommand";
        private const string ProxyCommandExtensionsClassName = "ProxyCommandExtensions";

        private const string AttributeText = @"
using System;

namespace %NAMESPACE%
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [System.Diagnostics.Conditional(""RyuSocks_CommandGenerator_DEBUG"")]
    public sealed class %ATTRIBUTE_NAME% : Attribute
    {
        private byte commandId;

        /// <summary>
        /// Marks this class as a proxy command. It must forward the base class constructor
        /// and extend either <see cref=""%CLIENT_CLASS_NAME%""/> or <see cref=""%SERVER_CLASS_NAME%""/>.
        /// </summary>
        /// <param name=""commandId"">The value between 0x01 and 0xFF used to identify this proxy command.</param>
        public %ATTRIBUTE_NAME%([System.ComponentModel.DataAnnotations.DeniedValues(0x0)] byte commandId)
        {
            this.commandId = commandId;
        }

        /// <summary>
        /// The name for this command used in the %ENUM_NAME% enum.
        /// </summary>
        public string CommandName { get; set; }

        public byte CommandId => commandId;
    }
}
";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{ProxyCommandImplAttributeName}.g.cs",
                    AttributeText
                            .Replace("%NAMESPACE%", Namespace)
                            .Replace("%ATTRIBUTE_NAME%", ProxyCommandImplAttributeName)
                            .Replace("%CLIENT_CLASS_NAME%", ClientCommandClassName)
                            .Replace("%SERVER_CLASS_NAME%", ServerCommandClassName)
                            .Replace("%ENUM_NAME%", ProxyCommandEnumName)
                )
            );

            // Create an incremental value provider using the generated attribute
            var proxyCommandProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{Namespace}.{ProxyCommandImplAttributeName}",
                IsProxyCommandClass,
                TransformProxyCommandClass
            );

            // Extract command ids and member names
            var commandIdProvider = proxyCommandProvider.Select((model, _) => new Tuple<byte, string>(model.Id, model.MemberName)).Collect();
            // Generate the proxy command enum
            context.RegisterSourceOutput(commandIdProvider, ProduceCommandEnum);

            // Generate the proxy command extensions
            context.RegisterSourceOutput(proxyCommandProvider.Collect(), ProduceCommandExtensions);
        }

        /// <summary>
        /// Checks whether the provided <paramref name="syntaxNode"/> extends the Client-/ServerCommand class.
        /// </summary>
        /// <param name="syntaxNode">The syntax node to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>True</c> if <paramref name="syntaxNode"/> extends the Client-/ServerCommand class.</returns>
        private static bool IsProxyCommandClass(SyntaxNode syntaxNode, CancellationToken cancellationToken) =>
            syntaxNode is ClassDeclarationSyntax { BaseList.Types.Count: > 0 } classDeclaration
            && classDeclaration.BaseList.Types.Any(type =>
                type.Type.ToString() == ClientCommandClassName
                || type.Type.ToString() == ServerCommandClassName);

        private static ProxyCommandModel TransformProxyCommandClass(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var classSymbol = context.TargetSymbol;
            var attributeData = context.Attributes[0];
            var classNode = context.TargetNode as ClassDeclarationSyntax;

            bool isClientCommand = classNode!.BaseList!.Types.Any(t => t.Type.ToString() == ClientCommandClassName);

            if (attributeData.ConstructorArguments.Length == 0)
            {
                throw new InvalidOperationException($"Attribute constructor doesn't have arguments: {attributeData.AttributeClass?.Name}");
            }

            byte commandId = (byte)attributeData.ConstructorArguments[0].Value!;
            TypedConstant memberName = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "CommandName").Value;

            if (!memberName.IsNull)
            {
                return new ProxyCommandModel(
                    commandId,
                    (string)memberName.Value,
                    isClientCommand,
                    classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                );
            }

            return new ProxyCommandModel(
                commandId,
                classSymbol.Name.Replace("Command", ""),
                isClientCommand,
                classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );
        }

        private void ProduceCommandEnum(SourceProductionContext context, ImmutableArray<Tuple<byte, string>> commands)
        {
            // Begin building the generated source
            CodeBuilder source = new();
            source.EnterScope($"namespace {Namespace}");
            source.EnterScope($"public enum {ProxyCommandEnumName} : byte");

            bool firstMember = true;
            byte lastMemberValue = 0;

            // Group the commands by ID and generate the source
            foreach (var command in commands.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1)).GroupBy(tuple => tuple.Item1, tuple => tuple.Item2))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (command.Distinct().Count() > 1)
                {
                    continue;
                }

                var commandName = command.First();

                if (firstMember)
                {
                    firstMember = false;

                    if (command.Key == 0)
                    {
                        source.AppendLine($"{commandName},");
                    }
                    else
                    {
                        source.AppendLine($"{commandName} = 0x{command.Key:X2},");
                    }

                    lastMemberValue = command.Key;

                    continue;
                }

                // Add a comment for non-default commands
                if (lastMemberValue <= 0x03 && command.Key > 0x03)
                {
                    source.AppendLine();
                    source.AppendLine("// 0x04 - 0xFF: Non-default commands");
                }

                // Add comments for unassigned values in between
                if (lastMemberValue < command.Key - 1)
                {
                    int firstUnassigned = lastMemberValue + 1;
                    int lastUnassigned = command.Key - 1;

                    if (firstUnassigned == lastUnassigned)
                    {
                        source.AppendLine($"// 0x{firstUnassigned:X2}: Unassigned");
                    }
                    else
                    {
                        source.AppendLine($"// 0x{firstUnassigned:X2} - 0x{lastUnassigned:X2}: Unassigned");
                    }
                }

                if (command.Key == lastMemberValue + 1)
                {
                    source.AppendLine($"{commandName},");
                }
                else
                {
                    source.AppendLine($"{commandName} = 0x{command.Key:X2},");
                }

                lastMemberValue = command.Key;
            }

            // Finish building the generated source
            source.LeaveScope();
            source.LeaveScope();

            // Add the source
            context.AddSource($"{ProxyCommandEnumName}.g.cs", source.ToString());
        }

        private static void ProduceCommandExtensions(SourceProductionContext context, ImmutableArray<ProxyCommandModel> commands)
        {
            // Begin building the generated source
            CodeBuilder sourceExtensions = new();
            sourceExtensions.AppendLine($"using {Namespace};");
            sourceExtensions.AppendLine("using System;");
            sourceExtensions.AppendLine("using System.Net;");
            sourceExtensions.AppendLine();
            sourceExtensions.EnterScope($"namespace {Namespace}.Extensions");
            sourceExtensions.EnterScope($"public static class {ProxyCommandExtensionsClassName}");

            // Group commands by type and ID
            Dictionary<string, Dictionary<byte, ProxyCommandModel>> groupedCommands = new()
            {
                {ClientCommandClassName, new Dictionary<byte, ProxyCommandModel>()},
                {ServerCommandClassName, new Dictionary<byte, ProxyCommandModel>()},
            };

            foreach (var entry in commands.Sort(CompareModel).GroupBy(model => model.Id))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (entry.Select(model => model.MemberName).Distinct().Count() != 1 || entry.Select(model => model.IsClientCommand).Distinct().Count() != 2)
                {
                    // TODO: Report diagnostics using an analyzer
                    continue;
                }

                foreach (var model in entry)
                {
                    if (model.IsClientCommand)
                    {
                        groupedCommands[ClientCommandClassName][entry.Key] = model;
                    }
                    else
                    {
                        groupedCommands[ServerCommandClassName][entry.Key] = model;
                    }
                }
            }

            // Iterate through all commands by type
            foreach (var entry in groupedCommands)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var commandType = entry.Key;
                var socksClassName = commandType == ClientCommandClassName ? "SocksClient" : "SocksSession";
                var typeParams = commandType == ClientCommandClassName
                    ? $"{socksClassName}, RyuSocks.Types.ProxyEndpoint, {commandType}"
                    : $"{socksClassName}, IPEndPoint, RyuSocks.Types.ProxyEndpoint, {commandType}";
                var constructorArgs = commandType == ClientCommandClassName
                    ? "(parent, endpoint)"
                    : "(parent, endpoint, proxyEndpoint)";

                // Generate the source
                sourceExtensions.EnterScope($"public static Func<{typeParams}> Get{commandType}(this {ProxyCommandEnumName} command) => command switch");

                foreach (var command in entry.Value.Values.ToImmutableSortedSet(Comparer<ProxyCommandModel>.Create(CompareModel)))
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    sourceExtensions.AppendLine($"{ProxyCommandEnumName}.{command.MemberName} => {constructorArgs} => new {command.ClassName}{constructorArgs},");
                }

                sourceExtensions.AppendLine("_ => throw new ArgumentException($\"Invalid proxy command provided: {command}\", nameof(command)),");
                sourceExtensions.LeaveScope(";");
                sourceExtensions.AppendLine();
            }

            // Finish building the generated source
            sourceExtensions.LeaveScope();
            sourceExtensions.LeaveScope();

            // Add the source
            context.AddSource($"{ProxyCommandExtensionsClassName}.g.cs", sourceExtensions.ToString());

            return;

            static int CompareModel(ProxyCommandModel m1, ProxyCommandModel m2) => m1.Id.CompareTo(m2.Id);
        }

        private record struct ProxyCommandModel(byte Id, string MemberName, bool IsClientCommand, string ClassName);
    }
}
