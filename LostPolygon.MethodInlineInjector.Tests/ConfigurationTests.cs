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
        public void NonGeneratedInjectionConfigurationSerializationTest() {
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
"
.Replace("\r\n", Environment.NewLine);
            InjectionConfiguration configurationDeserialized =
                SimpleXmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(configurationSerialized);
            string configurationSerializedAgain = SimpleXmlSerializationUtility.XmlSerializeToString(configurationDeserialized);
            configurationSerializedAgain = configurationSerializedAgain.Replace("\r\n", Environment.NewLine);

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
            List<IIgnoredMemberReference> ignoredMemberReference = null,
            List<IAllowedAssemblyReference> allowedAssemblyReferences = null
            ) {
            IgnoredMemberReference skippedMember =
                new IgnoredMemberReference(
                "ClassInheritedFromThirdPartyLibraryClass",
                IgnoredMemberReferenceFlags.SkipTypes |
                IgnoredMemberReferenceFlags.MatchAncestors
            );

            if (ignoredMemberReference == null) {
                ignoredMemberReference = new List<IIgnoredMemberReference> {
                    skippedMember,
                    new IgnoredMemberReference(
                        "SomeFilterString",
                        IgnoredMemberReferenceFlags.SkipProperties |
                        IgnoredMemberReferenceFlags.SkipMethods |
                        IgnoredMemberReferenceFlags.IsRegex
                    ),
                    new IgnoredMemberReference(
                        "SomeOtherFilterString",
                        IgnoredMemberReferenceFlags.SkipTypes |
                        IgnoredMemberReferenceFlags.MatchAncestors
                    ),
                    new IgnoredMemberReferenceInclude("SomeIgnoredMemberReferencesFilterInclude.xml")
                };
            } else {
                ignoredMemberReference.Insert(0, skippedMember);
            }

            if (allowedAssemblyReferences == null) {
                allowedAssemblyReferences = new List<IAllowedAssemblyReference> {
                    new AllowedAssemblyReference("mscorlib", true),
                    new AllowedAssemblyReference("System", false),
                    new AllowedAssemblyReference("Tests.ThirdPartyLibrary", false),
                    new AllowedAssemblyReferenceInclude("SomeInclude.xml")
                };
            }

            InjectionConfiguration configuration = new InjectionConfiguration(
                new List<InjecteeAssembly> {
                    new InjecteeAssembly(
                        InjecteeLibraryName,
                        new ReadOnlyCollection<IIgnoredMemberReference>(ignoredMemberReference),
                        new ReadOnlyCollection<IAllowedAssemblyReference>(allowedAssemblyReferences))
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
