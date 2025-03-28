using Microsoft.CodeAnalysis.Testing;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Verify = RyuSocks.Generator.Test.SourceGeneratorVerifier<RyuSocks.Generator.PacketGenerator>;

namespace RyuSocks.Generator.Test
{
    [ExcludeFromCodeCoverage]
    public class PacketGeneratorTests
    {
        private const string DataDirectory = "data/PacketGenerator/";
        private const int DefaultSourcesAmount = 3;

        private static string GetSourceFromFile(string path) => File.ReadAllText(DataDirectory + path);
        private static (string filename, string content) GetGeneratedSourceFromFile(string path) =>
            (Path.GetFileName(path), File.ReadAllText(DataDirectory + path));

        private static (string, string)[] GetDefaultGeneratedSources(int otherExpectedSourcesLength)
        {

            (string, string)[] generatedSources = new (string, string)[DefaultSourcesAmount + otherExpectedSourcesLength];

            generatedSources[0] = GetGeneratedSourceFromFile("StringEncoding.g.cs");
            generatedSources[1] = GetGeneratedSourceFromFile("PacketFieldAttribute.g.cs");
            generatedSources[2] = GetGeneratedSourceFromFile("Packet.g.cs");

            return generatedSources;
        }

        [Theory]
        [InlineData("SimpleProps", "TestPacket.cs",
            "TestPacket.Byte1.g.cs", "TestPacket.SByte1.g.cs",
            "TestPacket.UShort1.g.cs", "TestPacket.Short1.g.cs",
            "TestPacket.UInt1.g.cs", "TestPacket.Int1.g.cs",
            "TestPacket.ULong1.g.cs", "TestPacket.Long1.g.cs",
            "TestPacket.String1.g.cs", "TestPacket.String2.g.cs", "TestPacket.String3.g.cs")]
        [InlineData("SimplePropsOffsetMember", "TestPacket.cs",
            "TestPacket.Byte1.g.cs", "TestPacket.SByte1.g.cs",
            "TestPacket.UShort1.g.cs", "TestPacket.Short1.g.cs",
            "TestPacket.UInt1.g.cs", "TestPacket.Int1.g.cs",
            "TestPacket.ULong1.g.cs", "TestPacket.Long1.g.cs",
            "TestPacket.String1.g.cs", "TestPacket.String2.g.cs", "TestPacket.String3.g.cs")]
        [InlineData("Namespaces", "TestPacket.cs",
            "TestSpace.TestPacket.TestField.g.cs", "TestSpace.TestPacket1.FirstField.g.cs",
            "AnotherSpace.TestPacket.TestField.g.cs", "AnotherSpace.TestPacket2.SecondField.g.cs")]
        [InlineData("StringEncodings", "TestPacket.cs",
            "LittleEndian.ASCIIPacket.String1.g.cs", "LittleEndian.ASCIIPacket.String2.g.cs", "LittleEndian.ASCIIPacket.String3.g.cs", "LittleEndian.ASCIIPacket.String4.g.cs",
            "LittleEndian.UnicodePacket.String1.g.cs", "LittleEndian.UnicodePacket.String2.g.cs", "LittleEndian.UnicodePacket.String3.g.cs", "LittleEndian.UnicodePacket.String4.g.cs",
            "LittleEndian.UTF7Packet.String1.g.cs", "LittleEndian.UTF7Packet.String2.g.cs", "LittleEndian.UTF7Packet.String3.g.cs", "LittleEndian.UTF7Packet.String4.g.cs",
            "LittleEndian.UTF8Packet.String1.g.cs", "LittleEndian.UTF8Packet.String2.g.cs", "LittleEndian.UTF8Packet.String3.g.cs", "LittleEndian.UTF8Packet.String4.g.cs",
            "LittleEndian.UTF32Packet.String1.g.cs", "LittleEndian.UTF32Packet.String2.g.cs", "LittleEndian.UTF32Packet.String3.g.cs", "LittleEndian.UTF32Packet.String4.g.cs",
            "BigEndian.ASCIIPacket.String1.g.cs", "BigEndian.ASCIIPacket.String2.g.cs", "BigEndian.ASCIIPacket.String3.g.cs", "BigEndian.ASCIIPacket.String4.g.cs",
            "BigEndian.UnicodePacket.String1.g.cs", "BigEndian.UnicodePacket.String2.g.cs", "BigEndian.UnicodePacket.String3.g.cs", "BigEndian.UnicodePacket.String4.g.cs",
            "BigEndian.UTF7Packet.String1.g.cs", "BigEndian.UTF7Packet.String2.g.cs", "BigEndian.UTF7Packet.String3.g.cs", "BigEndian.UTF7Packet.String4.g.cs",
            "BigEndian.UTF8Packet.String1.g.cs", "BigEndian.UTF8Packet.String2.g.cs", "BigEndian.UTF8Packet.String3.g.cs", "BigEndian.UTF8Packet.String4.g.cs",
            "BigEndian.UTF32Packet.String1.g.cs", "BigEndian.UTF32Packet.String2.g.cs", "BigEndian.UTF32Packet.String3.g.cs", "BigEndian.UTF32Packet.String4.g.cs")]
        public async Task GeneratedSources_AsExpected(string directory, string sourcePath, params string[] expectedGeneratedSourcePath)
        {
            (string, string)[] generatedSources = GetDefaultGeneratedSources(expectedGeneratedSourcePath.Length);
            directory += "/";

            for (int i = 0; i < expectedGeneratedSourcePath.Length; i++)
            {
                generatedSources[DefaultSourcesAmount + i] = GetGeneratedSourceFromFile(directory + expectedGeneratedSourcePath[i]);
            }

            await Verify.VerifyGeneratedSources(
                GetSourceFromFile(directory + sourcePath),
                generatedSources
            );
        }

        [Fact]
        public async Task Generator_Fails_BadConstructorArgs()
        {
            const string Directory = "Exceptions/";
            (string, string)[] generatedSources = GetDefaultGeneratedSources(0);
            DiagnosticResult[] expectedDiagnostics = [
                // Generator failed to generate source
                DiagnosticResult.CompilerWarning("CS8785"),
                // Argument 1: Cannot convert from float to int
                DiagnosticResult.CompilerError("CS1503").WithSpan(7, 18, 7, 22).WithArguments("1", "float", "int"),
                // Partial property must have an implementation part
                DiagnosticResult.CompilerError("CS9248").WithSpan(8, 25, 8, 30).WithArguments("TestPacket.Byte1"),
            ];

            await Verify.VerifyGeneratedSources(
                GetSourceFromFile(Directory + "BadConstructorArgs.cs"),
                generatedSources,
                expectedDiagnostics
            );
        }

        [Fact]
        public async Task GeneratedSources_WorksWithGeneratedTypes()
        {
            const int ExpectedGeneratedSourcesLength = 11;
            const string Directory = "GeneratedTypes/";
            (string, string)[] generatedSources = GetDefaultGeneratedSources(ExpectedGeneratedSourcesLength);
            DiagnosticResult[] expectedDiagnostics = [
                // Type not found
                DiagnosticResult.CompilerError("CS0246").WithSpan(9, 20, 9, 37).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte1.g.cs", 9, 20, 9, 37).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte1.g.cs", 13, 21, 13, 38).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(12, 22, 12, 40).WithArguments("GeneratedEnumSByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.SByte1.g.cs", 9, 22, 9, 40).WithArguments("GeneratedEnumSByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.SByte1.g.cs", 13, 21, 13, 39).WithArguments("GeneratedEnumSByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(15, 23, 15, 42).WithArguments("GeneratedEnumUShort"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.UShort1.g.cs", 9, 23, 9, 42).WithArguments("GeneratedEnumUShort"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.UShort1.g.cs", 13, 21, 13, 40).WithArguments("GeneratedEnumUShort"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(18, 21, 18, 39).WithArguments("GeneratedEnumShort"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Short1.g.cs", 9, 21, 9, 39).WithArguments("GeneratedEnumShort"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Short1.g.cs", 13, 21, 13, 39).WithArguments("GeneratedEnumShort"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(21, 31, 21, 48).WithArguments("GeneratedEnumUInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.UInt1.g.cs", 9, 31, 9, 48).WithArguments("GeneratedEnumUInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.UInt1.g.cs", 13, 21, 13, 38).WithArguments("GeneratedEnumUInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(24, 13, 24, 29).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Int1.g.cs", 9, 13, 9, 29).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Int1.g.cs", 13, 21, 13, 37).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(27, 13, 27, 31).WithArguments("GeneratedEnumULong"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.ULong1.g.cs", 9, 13, 9, 31).WithArguments("GeneratedEnumULong"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.ULong1.g.cs", 13, 21, 13, 39).WithArguments("GeneratedEnumULong"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(30, 20, 30, 37).WithArguments("GeneratedEnumLong"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Long1.g.cs", 9, 20, 9, 37).WithArguments("GeneratedEnumLong"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Long1.g.cs", 13, 21, 13, 38).WithArguments("GeneratedEnumLong"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(33, 20, 33, 37).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte2.g.cs", 9, 20, 9, 37).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte2.g.cs", 13, 21, 13, 38).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(36, 13, 36, 29).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Int2.g.cs", 9, 13, 9, 29).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Int2.g.cs", 13, 13, 13, 29).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Int2.g.cs", 13, 45, 13, 61).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Int2.g.cs", 17, 30, 17, 46).WithArguments("GeneratedEnumInt"),
                DiagnosticResult.CompilerError("CS0246").WithSpan(39, 23, 39, 40).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte3.g.cs", 9, 23, 9, 40).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte3.g.cs", 13, 13, 13, 30).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte3.g.cs", 13, 46, 13, 63).WithArguments("GeneratedEnumByte"),
                DiagnosticResult.CompilerError("CS0246").WithSpan("RyuSocks.Generator/RyuSocks.Generator.PacketGenerator/TestPacket.Byte3.g.cs", 17, 30, 17, 47).WithArguments("GeneratedEnumByte"),
            ];

            generatedSources[DefaultSourcesAmount] = GetGeneratedSourceFromFile(Directory + "TestPacket.Byte1.g.cs");
            generatedSources[DefaultSourcesAmount + 1] = GetGeneratedSourceFromFile(Directory + "TestPacket.SByte1.g.cs");
            generatedSources[DefaultSourcesAmount + 2] = GetGeneratedSourceFromFile(Directory + "TestPacket.UShort1.g.cs");
            generatedSources[DefaultSourcesAmount + 3] = GetGeneratedSourceFromFile(Directory + "TestPacket.Short1.g.cs");
            generatedSources[DefaultSourcesAmount + 4] = GetGeneratedSourceFromFile(Directory + "TestPacket.UInt1.g.cs");
            generatedSources[DefaultSourcesAmount + 5] = GetGeneratedSourceFromFile(Directory + "TestPacket.Int1.g.cs");
            generatedSources[DefaultSourcesAmount + 6] = GetGeneratedSourceFromFile(Directory + "TestPacket.ULong1.g.cs");
            generatedSources[DefaultSourcesAmount + 7] = GetGeneratedSourceFromFile(Directory + "TestPacket.Long1.g.cs");
            generatedSources[DefaultSourcesAmount + 8] = GetGeneratedSourceFromFile(Directory + "TestPacket.Byte2.g.cs");
            generatedSources[DefaultSourcesAmount + 9] = GetGeneratedSourceFromFile(Directory + "TestPacket.Int2.g.cs");
            generatedSources[DefaultSourcesAmount + 10] = GetGeneratedSourceFromFile(Directory + "TestPacket.Byte3.g.cs");

            await Verify.VerifyGeneratedSources(
                GetSourceFromFile(Directory + "TestPacket.cs"),
                generatedSources,
                expectedDiagnostics
            );
        }
    }
}
