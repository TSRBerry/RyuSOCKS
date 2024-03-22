// Copyright (C) RyuSOCKS
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2,
// as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
    public class SocksClassGenerator : IIncrementalGenerator
    {
        private const string Namespace = "RyuSocks";
        private const string SocksClassAttributeName = "SocksClassAttribute";
        private const string GlobalNamespacePrefix = "global::";
        private const string ClientSuffix = "Client";
        private const string ServerSuffix = "Server";
        private const string SessionSuffix = "Session";

        private static readonly HashSet<string> _netCoreServerClassNames = [$"Tcp{ClientSuffix}", $"Tcp{ServerSuffix}", $"Tcp{SessionSuffix}"];
        private static readonly Dictionary<string, string> _serverClientConstructors = new()
        {
            {"System.Net.IPAddress address, int port", "address, port"},
            {"string address, int port", "address, port"},
            {"System.Net.DnsEndPoint endpoint", "endpoint"},
            {"System.Net.IPEndPoint endpoint", "endpoint"},
        };
        private static readonly Dictionary<string, string> _sessionConstructors = new()
        {
            {"NetCoreServer.TcpServer server", "server"},
        };

        private const string AttributeText = @"
using System;

namespace %NAMESPACE%
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [System.Diagnostics.Conditional(""RyuSocks_ConstructorGenerator_DEBUG"")]
    public sealed class %ATTRIBUTE_NAME% : Attribute
    {
        /// <summary>
        /// Marks this class as a SOCKS class.
        /// </summary>
        public %ATTRIBUTE_NAME%()
        {
        }
    }
}
";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{SocksClassAttributeName}.g.cs",
                    AttributeText
                        .Replace("%NAMESPACE%", Namespace)
                        .Replace("%ATTRIBUTE_NAME%", SocksClassAttributeName)
                )
            );

            // Create an incremental value provider using the generated attribute
            var socksClassProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{Namespace}.{SocksClassAttributeName}",
                IsSocksClass,
                TransformSocksClass
            );

            // Extract session classes
            var sessionClassProvider = socksClassProvider.Where(socksClass => socksClass.ClassType == ClassType.Session).Collect();
            // Extract server classes
            var serverClassProvider = socksClassProvider.Where(socksClass => socksClass.ClassType == ClassType.Server).Collect();
            // Combine server and session classes
            var sessionServerProvider = serverClassProvider.Combine(sessionClassProvider);
            // Generate the CreateSession methods
            context.RegisterSourceOutput(sessionServerProvider, ProduceCreateSessionMethod);

            // Generate the constructors
            context.RegisterSourceOutput(socksClassProvider.Collect(), ProduceConstructors);
        }

        /// <summary>
        /// Check whether the provided <paramref name="syntaxNode"/> extends a NetCoreServer class.
        /// </summary>
        /// <param name="syntaxNode">The syntax node to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>True</c> if <paramref name="syntaxNode"/> extends a NetCoreServer class.</returns>
        private static bool IsSocksClass(SyntaxNode syntaxNode, CancellationToken cancellationToken) =>
            syntaxNode is ClassDeclarationSyntax { BaseList.Types.Count: > 0 } classDeclaration
            && classDeclaration.BaseList.Types.Any(type => _netCoreServerClassNames.Contains(type.Type.ToString()));

        private static SocksClassModel TransformSocksClass(GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken)
        {
            var classSymbol = context.TargetSymbol;
            var classNode = context.TargetNode as ClassDeclarationSyntax;

            var baseClassName = classNode!.BaseList!.Types.Select(t => t.Type.ToString()).First(_netCoreServerClassNames.Contains);

            ClassType classType;

            if (baseClassName.EndsWith(ClientSuffix))
            {
                classType = ClassType.Client;
            }
            else if (baseClassName.EndsWith(ServerSuffix))
            {
                classType = ClassType.Server;
            }
            else if (baseClassName.EndsWith(SessionSuffix))
            {
                classType = ClassType.Session;
            }
            else
            {
                throw new InvalidOperationException($"No matching suffix for base class: {baseClassName}");
            }

            return new SocksClassModel(
                classSymbol.Name,
                classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                baseClassName,
                classType
            );
        }

        private static void ProduceCreateSessionMethod(SourceProductionContext context, (ImmutableArray<SocksClassModel> ServerClasses, ImmutableArray<SocksClassModel> SessionClasses) socksTuple)
        {
            // Create an array of tuples from the two arrays
            var socksTuples = socksTuple.ServerClasses.Zip(socksTuple.SessionClasses,
                (server, session) => new Tuple<SocksClassModel, SocksClassModel>(server, session));

            foreach ((SocksClassModel server, SocksClassModel session) in socksTuples)
            {
                string classNamespace = server.FullClassName.Extract(GlobalNamespacePrefix.Length, -(server.ClassName.Length + 1));

                // Begin building the generated source
                CodeBuilder source = new();

                source.EnterScope($"namespace {classNamespace}");
                source.EnterScope($"public partial class {server.ClassName} : NetCoreServer.{server.BaseClassName}");

                source.EnterScope("protected override NetCoreServer.TcpSession CreateSession()");
                source.AppendLine($"return new {session.FullClassName}(this);");
                source.LeaveScope();

                // Finish building the generated source
                source.LeaveScope();
                source.LeaveScope();

                // Add the source
                context.AddSource($"{server.FullClassName.Substring(GlobalNamespacePrefix.Length)}.CreateSession.g.cs", source.ToString());
            }
        }

        private static void AddConstructors(CodeBuilder source, string className, Dictionary<string, string> constructors, bool leaveScope = true)
        {
            foreach (var entry in constructors)
            {
                string args = entry.Key;
                string baseParams = entry.Value;

                if (string.IsNullOrEmpty(baseParams))
                {
                    if (leaveScope)
                    {
                        source.AppendLine($"public {className}({args}) {{ }}");
                    }
                    else
                    {
                        source.EnterScope($"public {className}({args})");
                    }
                }
                else
                {
                    if (leaveScope)
                    {
                        source.AppendLine($"public {className}({args}) : base({baseParams}) {{ }}");
                    }
                    else
                    {
                        source.EnterScope($"public {className}({args}) : base({baseParams})");
                    }
                }
            }
        }

        private static void ProduceConstructors(SourceProductionContext context, ImmutableArray<SocksClassModel> socksClassModels)
        {
            foreach (var socksClass in socksClassModels)
            {
                string classNamespace = socksClass.FullClassName.Extract(GlobalNamespacePrefix.Length, -(socksClass.ClassName.Length + 1));

                // Begin building the generated source
                CodeBuilder source = new();

                source.EnterScope($"namespace {classNamespace}");
                source.EnterScope($"public partial class {socksClass.ClassName} : NetCoreServer.{socksClass.BaseClassName}");

                switch (socksClass.ClassType)
                {
                    // Add constructors for the client/server classes
                    case ClassType.Client:
                    case ClassType.Server:
                        AddConstructors(source, socksClass.ClassName, _serverClientConstructors);
                        break;
                    // Add constructors for the session class
                    case ClassType.Session:
                        AddConstructors(source, socksClass.ClassName, _sessionConstructors);
                        break;
                }

                // Finish building the generated source
                source.LeaveScope();
                source.LeaveScope();

                // Add the source
                context.AddSource($"{socksClass.FullClassName.Substring(GlobalNamespacePrefix.Length)}.Constructors.g.cs", source.ToString());
            }
        }

        private enum ClassType
        {
            Client,
            Server,
            Session,
        }

        private record struct SocksClassModel(string ClassName, string FullClassName, string BaseClassName, ClassType ClassType);
    }
}
