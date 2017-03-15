using System.Collections.Generic;
using System.Collections.ObjectModel;
using LostPolygon.MethodInlineInjector.Serialization;
using NUnit.Framework;
using TestInjectedLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    [TestFixture]
    public class ConfigurationTests : IntegrationTestMainBase {
        [Test]
        public void InjectionConfigurationSerializationTest() {
            InjectionConfiguration configuration = GetInjectionConfiguration();

            string configurationSerialized = SimpleXmlSerializationUtility.XmlSerializeToString(configuration);
            InjectionConfiguration configurationDeserialized =
                SimpleXmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(configurationSerialized);
            string configurationSerializedAgain = SimpleXmlSerializationUtility.XmlSerializeToString(configurationDeserialized);

            Assert.AreEqual(configurationSerialized, configurationSerializedAgain);
        }

        protected InjectionConfiguration GetInjectionConfiguration(
            List<InjectionConfiguration.InjecteeAssembly.IMemberReferenceBlacklistItem> memberReferenceBlacklist = null,
            List<InjectionConfiguration.InjecteeAssembly.IAssemblyReferenceWhitelistItem> assemblyReferenceWhitelist = null
            ) {
            if (memberReferenceBlacklist == null) {
                memberReferenceBlacklist = new List<InjectionConfiguration.InjecteeAssembly.IMemberReferenceBlacklistItem> {
                    new InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter(
                        "SomeFilterString",
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipProperties |
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipMethods |
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.IsRegex
                    ),
                    new InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter(
                        "SomeOtherFilterString",
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipTypes |
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.MatchAncestors
                    )
                };
            }

            if (assemblyReferenceWhitelist == null) {
                assemblyReferenceWhitelist = new List<InjectionConfiguration.InjecteeAssembly.IAssemblyReferenceWhitelistItem> {
                    new InjectionConfiguration.InjecteeAssembly.AssemblyReferenceWhitelistFilter("mscorlib", true),
                    new InjectionConfiguration.InjecteeAssembly.AssemblyReferenceWhitelistFilter("System", false),
                };
            }

            InjectionConfiguration configuration = new InjectionConfiguration(
                new List<InjectionConfiguration.InjecteeAssembly> {
                    new InjectionConfiguration.InjecteeAssembly(
                        InjecteeLibraryName,
                        new ReadOnlyCollection<InjectionConfiguration.InjecteeAssembly.IMemberReferenceBlacklistItem>(memberReferenceBlacklist),
                        new ReadOnlyCollection<InjectionConfiguration.InjecteeAssembly.IAssemblyReferenceWhitelistItem>(assemblyReferenceWhitelist))
                }.AsReadOnly(),
                new ReadOnlyCollection<InjectionConfiguration.InjectedMethod>(new List<InjectionConfiguration.InjectedMethod> {
                    new InjectionConfiguration.InjectedMethod(
                        InjectedLibraryName,
                        $"{typeof(TestInjectedMethods).FullName}.{nameof(TestInjectedMethods.SingleStatement)}",
                        InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodStart,
                        InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromSelf)
                })
            );
            return configuration;
        }
    }
}
