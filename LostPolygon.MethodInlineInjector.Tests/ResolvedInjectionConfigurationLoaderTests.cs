using System.Linq;
using NUnit.Framework;
using TestInjectedLibrary;
using TestInjecteeLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    public class ResolvedInjectionConfigurationLoaderTests : ConfigurationTests {
        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptInjectNonStatic() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.NonStatic)}"
                ),
                null
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptInjectFieldDependent() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.FieldDependent)}"
                ),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        public void AttemptInjectFieldDependentValid() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.FieldDependentValid)}"
                ),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}",
                false
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptInjectTypeDependent() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.TypeDependent)}"
                ),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptInjectWithParameters() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.WithParameters)}"
                ),
                null
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptInjectWithGenericParameters() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.WithGenericParameters)}"
                ),
                null
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptInjectWithReturnValue() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.WithReturnValue)}"
                ),
                null
            );
        }

        [Test]
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptIncompatibleInjectedMethodOptions() {
            ExecuteSimpleTest(
                new InjectionConfiguration.InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}",
                    InjectionConfiguration.InjectedMethod.MethodInjectionPosition.InjecteeMethodStart,
                    InjectionConfiguration.InjectedMethod.MethodReturnBehaviour.ReturnFromInjectee
                ),
                null
            );
        }

        [Test]
        public void BlacklistTypeTest() {
            string blacklistedTypeFullName = typeof(TestInjectee).FullName;
            ExecuteSimpleBlacklistTypeTest(blacklistedTypeFullName);
        }

        [Test]
        public void BlacklistTypeByRegexTest() {
            ResolvedInjectionConfiguration resolvedConfiguration =
                ExecuteBlacklistTest(
                    new InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter(
                        "Injecte[ed]",
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipTypes |
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.IsRegex |
                        InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.MatchAncestors
                    )
                );

            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(TestInjectee).FullName));
            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(ChildTestInjectee).FullName));
            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(StructTestInjectee).FullName));
        }

        [Test]
        public void BlacklistTypeAndChildTypesTest() {
            ResolvedInjectionConfiguration resolvedConfiguration =
                ExecuteSimpleBlacklistTypeTest(
                    typeof(TestInjectee).FullName,
                    InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipTypes |
                    InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.MatchAncestors
                );

            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(ChildTestInjectee).FullName));
        }

        [Test]
        public void BlacklistStructTypeTest() {
            ExecuteSimpleBlacklistTypeTest(
                typeof(StructTestInjectee).FullName,
                InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipTypes |
                InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.MatchAncestors
            );
        }

        private ResolvedInjectionConfiguration ExecuteSimpleBlacklistTypeTest(
            string blacklistedTypeFullName,
            InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags filterFlags =
                InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter.FilterFlags.SkipTypes
        ) {
            var memberReferenceBlacklist = new InjectionConfiguration.InjecteeAssembly.IMemberReferenceBlacklistItem[] {
                new InjectionConfiguration.InjecteeAssembly.MemberReferenceBlacklistFilter(
                    blacklistedTypeFullName,
                    filterFlags
                ),
            };

            ResolvedInjectionConfiguration resolvedConfiguration = ExecuteBlacklistTest(memberReferenceBlacklist);
            Assert.True(IsTypeSkipped(resolvedConfiguration, blacklistedTypeFullName));

            return resolvedConfiguration;
        }

        private ResolvedInjectionConfiguration ExecuteBlacklistTest(
            params InjectionConfiguration.InjecteeAssembly.IMemberReferenceBlacklistItem[] memberReferenceBlacklist) {
            InjectionConfiguration configuration = GetInjectionConfiguration(memberReferenceBlacklist: memberReferenceBlacklist.ToList());
            ResolvedInjectionConfiguration resolvedConfiguration =
                ResolvedInjectionConfigurationLoader.LoadFromInjectionConfiguration(configuration);
            ExecuteSimpleTest(resolvedConfiguration, false);
            return resolvedConfiguration;
        }

        private static bool IsTypeSkipped(ResolvedInjectionConfiguration resolvedConfiguration, string skippedTypeFullName) {
            return resolvedConfiguration
                       .InjecteeAssemblies
                       .SelectMany(assembly => assembly.InjecteeMethodsDefinitions)
                       .FirstOrDefault(method => method.DeclaringType.FullName == skippedTypeFullName) == null;
        }

        #region Setup

        public override string InjectedClassName => typeof(TestInjectedLibrary.InvalidInjectedMethods).FullName;

        #endregion
    }
}
