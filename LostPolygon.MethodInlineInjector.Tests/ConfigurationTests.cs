using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LostPolygon.Common.SimpleXmlSerialization;
using NUnit.Framework;
using TestInjectedLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class ConfigurationTests : IntegrationTestMainBase {
        [Test]
        public void InjectionConfigurationSerializationTest() {
            InjectionConfiguration configuration = GetInjectionConfiguration();

            string configurationSerialized = SimpleXmlSerializationUtility.XmlSerializeToString(configuration);
            Console.WriteLine(configurationSerialized);
            Console.WriteLine();
            InjectionConfiguration configurationDeserialized =
                SimpleXmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(configurationSerialized);
            string configurationSerializedAgain = SimpleXmlSerializationUtility.XmlSerializeToString(configurationDeserialized);

            Console.WriteLine(configurationSerializedAgain);
            Assert.AreEqual(configurationSerialized, configurationSerializedAgain);
        }

        [Test]
        public void InjectionConfigurationSerializationTest2() {
            string configurationSerialized =
@"<Configuration>
    <InjecteeAssemblies>
        <InjecteeAssembly AssemblyPath=""lib1.dll"">
        </InjecteeAssembly>
        <InjecteeAssembly AssemblyPath=""lib2.dll"">
        </InjecteeAssembly>
    </InjecteeAssemblies>
    <InjectedMethods>
        <InjectedMethod AssemblyPath=""TestInjectedLibrary.dll"" MethodFullName=""TestInjectedLibrary.TestInjectedMethods.Complex"" InjectionPosition=""InjecteeMethodStart"" />
    </InjectedMethods>
</Configuration>
";
            InjectionConfiguration configurationDeserialized =
                SimpleXmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(configurationSerialized);
            string configurationSerializedAgain = SimpleXmlSerializationUtility.XmlSerializeToString(configurationDeserialized);

            Console.WriteLine(configurationSerializedAgain);
            Assert.AreEqual(configurationSerialized.Trim(), configurationSerializedAgain.Trim());
        }


        [Test]
        public void GenerateSchema() {
            InjectionConfiguration configuration = GetInjectionConfiguration();

            string configurationSerialized = SimpleXmlSerializationUtility.GenerateXmlSchemaString(configuration);
            Console.WriteLine(configurationSerialized);
        }

        protected InjectionConfiguration GetInjectionConfiguration(
            List<IMemberReferenceBlacklistItem> memberReferenceBlacklist = null,
            List<IAssemblyReferenceWhitelistItem> assemblyReferenceWhitelist = null
            ) {
            if (memberReferenceBlacklist == null) {
                memberReferenceBlacklist = new List<IMemberReferenceBlacklistItem> {
                    new MemberReferenceBlacklistFilter(
                        "SomeFilterString",
                        MemberReferenceBlacklistFilterFlags.SkipProperties |
                        MemberReferenceBlacklistFilterFlags.SkipMethods |
                        MemberReferenceBlacklistFilterFlags.IsRegex
                    ),
                    new MemberReferenceBlacklistFilter(
                        "SomeOtherFilterString",
                        MemberReferenceBlacklistFilterFlags.SkipTypes |
                        MemberReferenceBlacklistFilterFlags.MatchAncestors
                    ),
                    new MemberReferenceBlacklistFilterInclude("SomeMemberReferenceBlacklistFilterInclude.xml")
                };
            }

            if (assemblyReferenceWhitelist == null) {
                assemblyReferenceWhitelist = new List<IAssemblyReferenceWhitelistItem> {
                    new AssemblyReferenceWhitelistFilter("mscorlib", true),
                    new AssemblyReferenceWhitelistFilter("System", false),
                    new AssemblyReferenceWhitelistFilterInclude("SomeInclude.xml")
                };
            }

            InjectionConfiguration configuration = new InjectionConfiguration(
                new List<InjecteeAssembly> {
                    new InjecteeAssembly(
                        InjecteeLibraryName,
                        new ReadOnlyCollection<IMemberReferenceBlacklistItem>(memberReferenceBlacklist),
                        new ReadOnlyCollection<IAssemblyReferenceWhitelistItem>(assemblyReferenceWhitelist))
                }.AsReadOnly(),
                new ReadOnlyCollection<InjectedMethod>(new List<InjectedMethod> {
                    new InjectedMethod(
                        InjectedLibraryName,
                        $"{typeof(TestInjectedMethods).FullName}.{nameof(TestInjectedMethods.SingleStatement)}",
                        MethodInjectionPosition.InjecteeMethodStart
                    )
                })
            );

            return configuration;
        }
    }
}
