using System.IO;
using System.Threading.Tasks;
using Xunit;
using Verify = RyuSocks.Generator.Test.SourceGeneratorVerifier<RyuSocks.Generator.PacketGenerator>;

namespace RyuSocks.Generator.Test
{
    public class PacketGeneratorTests
    {
        private const string DataDirectory = "data/PacketGenerator/";

        public PacketGeneratorTests()
        {
            Verify.PostInitGeneratedSources.Add(GetGeneratedSourceFromFile("PacketFieldAttribute.g.cs"));
            Verify.PostInitGeneratedSources.Add(GetGeneratedSourceFromFile("Packet.g.cs"));
        }

        private static string GetSourceFromFile(string path) => File.ReadAllText(DataDirectory + path);
        private static (string filename, string content) GetGeneratedSourceFromFile(string path) =>
            (Path.GetFileName(path), File.ReadAllText(DataDirectory + path));

        [Theory]
        [InlineData("SimpleProps", "TestPacket.cs",
            "TestPacket.Byte1.g.cs", "TestPacket.SByte1.g.cs",
            "TestPacket.UShort1.g.cs", "TestPacket.Short1.g.cs",
            "TestPacket.UInt1.g.cs", "TestPacket.Int1.g.cs",
            "TestPacket.ULong1.g.cs", "TestPacket.Long1.g.cs")]
        public async Task GeneratedSources_AsExpected(string directory, string sourcePath, params string[] expectedGeneratedSourcePath)
        {
            directory += "/";
            (string, string)[] generatedSources = new (string, string)[expectedGeneratedSourcePath.Length];

            for (int i = 0; i < expectedGeneratedSourcePath.Length; i++)
            {
                generatedSources[i] = GetGeneratedSourceFromFile(directory + expectedGeneratedSourcePath[i]);
            }

            await Verify.VerifyGeneratedSources(
                GetSourceFromFile(directory + sourcePath),
                generatedSources
            );
        }
    }
}
