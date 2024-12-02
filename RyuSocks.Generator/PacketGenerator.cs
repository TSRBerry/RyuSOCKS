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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RyuSocks.Generator.Builder;
using RyuSocks.Generator.Packet;
using System;
using System.Linq;
using System.Threading;

namespace RyuSocks.Generator
{
    // TODO: Write an analyzer for PacketFieldAttribute
    //         - make sure (Offset and OffsetMember) and (Length and LengthMember) aren't used at the same time
    //         - check whether the attribute is used on a property within a class/subclass that extends Packet
    // FIXME: Get accessibility modifiers of property accessors.
    // FIXME: Pay attention to the endianness, don't assume little endian.

    [Generator]
    public class PacketGenerator : IIncrementalGenerator
    {
        private const string Namespace = "RyuSocks.Packets";
        private const string AbstractClassName = "Packet";
        private const string PacketFieldAttributeName = "PacketFieldAttribute";
        private const string PacketBytesFieldName = "Bytes";

#region SourceText
        private const string PacketFieldAttributeText = @"
using System;

namespace %NAMESPACE%
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    [System.Diagnostics.Conditional(""RyuSocks_PacketGenerator_DEBUG"")]
    public sealed class %ATTRIBUTE_NAME% : Attribute
    {
        private int offset = -1;
        private string offsetMember = string.Empty;

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref=""%ABSTRACT_CLASS_NAME%""/>.
        /// </summary>
        /// <param name=""offset"">The offset of this field in the packet.</param>
        public %ATTRIBUTE_NAME%([System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)] int offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// Marks this property as a field of the packet.
        /// The containing class needs to extend <see cref=""%ABSTRACT_CLASS_NAME%""/>.
        /// </summary>
        /// <param name=""offsetMember"">The name of the member which specifies the offset of this field in the packet.</param>
        public %ATTRIBUTE_NAME%(string offsetMember)
        {
            this.offsetMember = offsetMember;
        }

        /// <summary>
        /// The length of this field.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public int Length { get; set; } = 0;

        /// <summary>
        /// The name of the member which specifies the length of this field.
        /// Only required if it can't be determined from the property type.
        /// </summary>
        public string LengthMember { get; set; } = string.Empty;

        /// <summary>
        /// Whether this field is big endian.
        /// </summary>
        public bool IsBigEndian { get; set; } = false;

        public int Offset => offset;
        public string OffsetMember => offsetMember;
    }
}
";

        private const string AbstractPacketClassText = @"
using System;

namespace %NAMESPACE%
{
    public abstract partial class %CLASS_NAME%
    {
        /// <summary>
        /// The contents of the packet.
        /// </summary>
        public byte[] %BYTES_FIELD_NAME% { get; protected set; }

        /// <inheritdoc cref=""%BYTES_FIELD_NAME%""/>
        public Span<byte> AsSpan() => %BYTES_FIELD_NAME%;

        protected %CLASS_NAME%() { }
        
        protected %CLASS_NAME%(byte[] bytes)
        {
            %BYTES_FIELD_NAME% = bytes;
        }
    }
}
";
#endregion

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{PacketFieldAttributeName}.g.cs",
                    PacketFieldAttributeText
                        .Replace("%NAMESPACE%", Namespace)
                        .Replace("%ATTRIBUTE_NAME%", PacketFieldAttributeName)
                        .Replace("%ABSTRACT_CLASS_NAME%", AbstractClassName)
                )
            );
            // Register the abstract packet class source
            context.RegisterPostInitializationOutput(static postContext => postContext.AddSource(
                    $"{AbstractClassName}.g.cs",
                    AbstractPacketClassText
                        .Replace("%NAMESPACE%", Namespace)
                        .Replace("%CLASS_NAME%", AbstractClassName)
                        .Replace("%BYTES_FIELD_NAME%", PacketBytesFieldName)
                )
            );

            // Create an incremental value provider using the generated attribute
            var packetFieldProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{Namespace}.{PacketFieldAttributeName}",
                IsValidPacketFieldProperty,
                TransformPacketFieldProperty
            );

            // TODO: Figure out how to group by a field/property, so code can be generated for all classes in parallel,
            //       while every class gets only one generated file.
            //       Not sure if that's actually desirable.
            //       Looking for something similar to LinQ GroupBy
            context.RegisterSourceOutput(packetFieldProvider, ProduceSourceCode);
        }

        /// <summary>
        /// Check whether <paramref name="syntaxNode"/> is a valid packet field property.
        /// </summary>
        /// <param name="syntaxNode">The syntax node to check.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>True</c> if <paramref name="syntaxNode"/> is a valid packet field property.</returns>
        private static bool IsValidPacketFieldProperty(SyntaxNode syntaxNode, CancellationToken cancellationToken) =>
            syntaxNode is PropertyDeclarationSyntax { Parent: ClassDeclarationSyntax { BaseList.Types.Count: > 0 } classDeclaration } propertyDeclaration
            && propertyDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
            && classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));

        private static PacketFieldModel TransformPacketFieldProperty(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var propertySymbol = (context.TargetSymbol as IPropertySymbol)!;
            var attributeData = context.Attributes[0];
            var classSymbol = context.TargetSymbol.ContainingSymbol;

            if (attributeData.ConstructorArguments.Length == 0)
            {
                throw new InvalidOperationException($"Attribute constructor doesn't have arguments: {attributeData.AttributeClass?.Name}");
            }

            // Parse attribute data

            int offset = -1;
            string offsetMember = string.Empty;

            switch (attributeData.ConstructorArguments[0].Type!.Name)
            {
                case "String":
                    offsetMember = (string)attributeData.ConstructorArguments[0].Value!;
                    break;
                case "Int32":
                    offset = (int)attributeData.ConstructorArguments[0].Value!;
                    break;
                default:
                    throw new InvalidOperationException($"Attribute constructor argument type is not valid: {attributeData.ConstructorArguments[0].Type!.Name}");
            }

            TypedConstant lengthArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "Length").Value;
            TypedConstant lengthMemberArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "LengthMember").Value;
            TypedConstant isBigEndianArg = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "IsBigEndian").Value;
            int length = !lengthArg.IsNull ? (int)lengthArg.Value! : -1;
            string lengthMember = !lengthMemberArg.IsNull ? (string)lengthMemberArg.Value! : string.Empty;
            bool isBigEndian = !isBigEndianArg.IsNull && (bool)isBigEndianArg.Value!;

            // Get information about the property type

            ITypeSymbol actualTypeSymbol = propertySymbol.Type;
            bool isArray = propertySymbol.Type.TypeKind == TypeKind.Array;
            bool isEnum = propertySymbol.Type.TypeKind == TypeKind.Enum;
            bool isStruct = IsActualStruct(propertySymbol.Type);
            if (!Enum.TryParse(actualTypeSymbol.Name, out ActualType actualType))
            {
                actualType = ActualType.NamedType;
            }

            if (isArray)
            {
                actualTypeSymbol = (propertySymbol.Type as IArrayTypeSymbol)!.ElementType;
                isEnum = actualTypeSymbol.TypeKind == TypeKind.Enum;
                isStruct = IsActualStruct(actualTypeSymbol);

                if (!isEnum && !isStruct && actualTypeSymbol.TypeKind != TypeKind.Class)
                {
                    actualType = (ActualType)Enum.Parse(typeof(ActualType), actualTypeSymbol.Name, true);
                }
            }

            if (isEnum)
            {
                actualType = (ActualType)Enum.Parse(typeof(ActualType), (actualTypeSymbol as INamedTypeSymbol)!.EnumUnderlyingType!.Name, true);
            }

            // Get imports
            // NOTE: This is only necessary because property types could be generated by other source generators,
            //       which aren't available at this point.

            // TODO: Add something like "AssumeEnum" to PacketFieldAttribute
            //       which could be used when propertySymbol.Type.TypeKind == TypeKind.Error

            CompilationUnitSyntax rootNode = context.TargetNode.SyntaxTree.GetCompilationUnitRoot();
            string[] imports = rootNode.Usings.ToString().Split('\n');
            for (int i = 0; i < imports.Length; i++)
            {
                imports[i] = imports[i].Trim();
            }

            // Create model from data
            return new PacketFieldModel(
                offset,
                offsetMember,
                length,
                lengthMember,
                isBigEndian,
                new FieldTypeModel(
                    propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    isArray,
                    isEnum,
                    isStruct,
                    actualType
                ),
                propertySymbol.Name,
                propertySymbol.DeclaredAccessibility,
                classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                imports
            );
        }

        private static void ProduceSourceCode(SourceProductionContext context, PacketFieldModel packetField)
        {
            string namespaceName = string.Empty;
            string className = packetField.ClassName;

            if (className.Contains("."))
            {
                namespaceName = className.Substring(0, className.LastIndexOf(".", StringComparison.Ordinal));
                className = className.Substring(className.LastIndexOf(".", StringComparison.Ordinal) + 1);
            }

            if (namespaceName.StartsWith("global::"))
            {
                namespaceName = namespaceName.Substring(8);
            }

            // Begin building the generated source
            CodeBuilder source = new();
            string[] requiredImports = ["using System;", "using System.Text;"];

            // Add imports

            foreach (var importString in packetField.Imports)
            {
                source.AppendLine(importString);
            }

            foreach (var requiredImportString in requiredImports)
            {
                if (!packetField.Imports.Contains(requiredImportString))
                {
                    source.AppendLine(requiredImportString);
                }
            }

            source.AppendLine();

            // Add namespace if necessary

            if (namespaceName.Length > 0)
            {
                source.EnterScope($"namespace {namespaceName}");
            }

            // Add class and property

            source.EnterScope($"partial class {className}");
            source.EnterScope($"{packetField.PropertyAccessModifier.ToModifierString()} partial {packetField.FieldType.Name} {packetField.PropertyName}");

            // Add code for getter
            source.EnterScope("get");
            AccessorGenerator.Generate(PacketBytesFieldName, source, packetField, true);
            source.LeaveScope();

            // Add code for setter
            source.EnterScope("set");
            AccessorGenerator.Generate(PacketBytesFieldName, source, packetField, false);
            source.LeaveScope();

            // Leave property and class scope
            source.LeaveScope();
            source.LeaveScope();

            // Leave namespace scope if necessary
            if (namespaceName.Length > 0)
            {
                source.LeaveScope();
            }

            // Add the generated source
            string fullyQualifiedName = namespaceName.Length > 0 ? $"{namespaceName}.{className}" : className;
            context.AddSource($"{fullyQualifiedName}.{packetField.PropertyName}.g.cs", source.ToString());
        }

        private static bool IsActualStruct(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            return typeSymbol.SpecialType is < SpecialType.System_Boolean or > SpecialType.System_String;
        }
    }
}
