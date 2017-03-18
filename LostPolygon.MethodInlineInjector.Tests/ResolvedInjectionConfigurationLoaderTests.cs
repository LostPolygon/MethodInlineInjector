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
                new InjectedMethod(
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
                new InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(InvalidInjectedMethods.FieldDependent)}"
                ),
                $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}"
            );
        }

        [Test]
        public void AttemptInjectFieldDependentValid() {
            ExecuteSimpleTest(
                new InjectedMethod(
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
                new InjectedMethod(
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
                new InjectedMethod(
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
                new InjectedMethod(
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
                new InjectedMethod(
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
                new InjectedMethod(
                    InjectedLibraryPath,
                    $"{InjectedClassName}.{nameof(TestInjectedMethods.SimpleReturn)}",
                    MethodInjectionPosition.InjecteeMethodStart
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
                    new MemberReferenceBlacklistFilter(
                        "Injecte[ed]",
                        MemberReferenceBlacklistFilterFlags.SkipTypes |
                        MemberReferenceBlacklistFilterFlags.IsRegex |
                        MemberReferenceBlacklistFilterFlags.MatchAncestors
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
                    MemberReferenceBlacklistFilterFlags.SkipTypes |
                    MemberReferenceBlacklistFilterFlags.MatchAncestors
                );

            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(ChildTestInjectee).FullName));
        }

        [Test]
        public void BlacklistStructTypeTest() {
            ExecuteSimpleBlacklistTypeTest(
                typeof(StructTestInjectee).FullName,
                MemberReferenceBlacklistFilterFlags.SkipTypes |
                MemberReferenceBlacklistFilterFlags.MatchAncestors
            );
        }

        private ResolvedInjectionConfiguration ExecuteSimpleBlacklistTypeTest(
            string blacklistedTypeFullName,
            MemberReferenceBlacklistFilterFlags blacklistFilterFlags =
                MemberReferenceBlacklistFilterFlags.SkipTypes
        ) {
            IMemberReferenceBlacklistItem[] memberReferenceBlacklist = {
                new MemberReferenceBlacklistFilter(
                    blacklistedTypeFullName,
                    blacklistFilterFlags
                ),
            };

            ResolvedInjectionConfiguration resolvedConfiguration = ExecuteBlacklistTest(memberReferenceBlacklist);
            Assert.True(IsTypeSkipped(resolvedConfiguration, blacklistedTypeFullName));

            return resolvedConfiguration;
        }

        private ResolvedInjectionConfiguration ExecuteBlacklistTest(
            params IMemberReferenceBlacklistItem[] memberReferenceBlacklist
        ) {
            InjectionConfiguration configuration = GetInjectionConfiguration(memberReferenceBlacklist.ToList());
            ResolvedInjectionConfiguration resolvedConfiguration =
                ResolvedInjectionConfigurationLoader.LoadFromInjectionConfiguration(configuration);
            ExecuteSimpleTest(resolvedConfiguration, false);
            return resolvedConfiguration;
        }

        private static bool IsTypeSkipped(ResolvedInjectionConfiguration resolvedConfiguration, string skippedTypeFullName) {
            return resolvedConfiguration
                       .InjecteeAssemblies
                       .SelectMany(assembly => assembly.InjecteeMethods)
                       .All(method => method.DeclaringType.FullName != skippedTypeFullName);
        }

        #region Setup

        public override string InjectedClassName => typeof(InvalidInjectedMethods).FullName;

        #endregion
    }
}
