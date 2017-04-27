using System.Collections.Generic;
using System.Linq;
using devtm.Cecil.Extensions;
using Mono.Cecil;
using Mono.Cecil.Rocks;
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

        [Test]
        public void BlacklistVirtualMethodTest() {
            string methodName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            string methodNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleBlacklistTest(
                methodName,
                MemberReferenceBlacklistFilterFlags.SkipMethods
            );

            Assert.True(IsMethodSkipped(configuration, methodName));
            Assert.False(IsMethodSkipped(configuration, methodNameChild));
        }

        [Test]
        public void BlacklistVirtualMethodOverrideTest() {
            string methodName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            string methodNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleBlacklistTest(
                    methodName,
                    MemberReferenceBlacklistFilterFlags.SkipMethods |
                    MemberReferenceBlacklistFilterFlags.MatchAncestors
                );

            Assert.True(IsMethodSkipped(configuration, methodName));
            Assert.True(IsMethodSkipped(configuration, methodNameChild));
        }

        [Test]
        public void BlacklistVirtualPropertyTest() {
            string propertyName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            string propertyNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleBlacklistTest(
                    propertyName,
                    MemberReferenceBlacklistFilterFlags.SkipProperties
                );

            Assert.True(IsPropertySkipped(configuration, propertyName));
            Assert.False(IsPropertySkipped(configuration, propertyNameChild));
        }

        [Test]
        public void BlacklistVirtualPropertyOverrideTest() {
            string propertyName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            string propertyNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleBlacklistTest(
                    propertyName,
                    MemberReferenceBlacklistFilterFlags.SkipProperties |
                    MemberReferenceBlacklistFilterFlags.MatchAncestors
                );

            Assert.True(IsPropertySkipped(configuration, propertyName));
            Assert.True(IsPropertySkipped(configuration, propertyNameChild));
        }

        private ResolvedInjectionConfiguration ExecuteSimpleBlacklistTest(
            string blacklistedMethodFullName,
            MemberReferenceBlacklistFilterFlags blacklistFilterFlags =
                MemberReferenceBlacklistFilterFlags.SkipMethods
        ) {
            IMemberReferenceBlacklistItem[] memberReferenceBlacklist = {
                new MemberReferenceBlacklistFilter(
                    blacklistedMethodFullName,
                    blacklistFilterFlags
                ),
            };

            ResolvedInjectionConfiguration resolvedConfiguration = ExecuteBlacklistTest(memberReferenceBlacklist);

            return resolvedConfiguration;
        }

        private ResolvedInjectionConfiguration ExecuteSimpleBlacklistTypeTest(
            string blacklistedTypeFullName,
            MemberReferenceBlacklistFilterFlags blacklistFilterFlags =
                MemberReferenceBlacklistFilterFlags.SkipTypes
        ) {
            ResolvedInjectionConfiguration resolvedConfiguration = ExecuteSimpleBlacklistTest(blacklistedTypeFullName, blacklistFilterFlags);
            Assert.True(IsTypeSkipped(resolvedConfiguration, blacklistedTypeFullName));

            return resolvedConfiguration;
        }

        private ResolvedInjectionConfiguration ExecuteBlacklistTest(
            params IMemberReferenceBlacklistItem[] memberReferenceBlacklist
        ) {
            InjectionConfiguration configuration = GetInjectionConfiguration(memberReferenceBlacklist.ToList());

            // Strip includes
            configuration =
                configuration.WithInjecteeAssemblies(
                    configuration.InjecteeAssemblies.Select(assembly =>
                        assembly.WithAssemblyReferenceWhitelist(
                            assembly.AssemblyReferenceWhitelist.Where(item => !(item is InjectionConfigurationFileInclude)).ToList().AsReadOnly()
                        )
                        .WithMemberReferenceBlacklist(
                            assembly.MemberReferenceBlacklist.Where(item => !(item is InjectionConfigurationFileInclude)).ToList().AsReadOnly()
                        )
                    )
                    .ToList().AsReadOnly()
                );

            ResolvedInjectionConfiguration resolvedConfiguration =
                ResolvedInjectionConfigurationLoader.LoadFromInjectionConfiguration(configuration);
            ExecuteSimpleTest(resolvedConfiguration, false);
            return resolvedConfiguration;
        }

        private static bool IsPropertySkipped(ResolvedInjectionConfiguration resolvedConfiguration, string skippedPropertyFullName) {
            IEnumerable<PropertyDefinition> allProperties =
                resolvedConfiguration
                .InjecteeAssemblies
                .SelectMany(assembly => assembly.AssemblyDefinition.MainModule.GetAllTypes().SelectMany(type => type.Properties));

            PropertyDefinition skippedProperty =
                allProperties
                .First(property => property.GetFullSimpleName() == skippedPropertyFullName);

            if (skippedProperty.GetMethod != null && !IsMethodSkipped(resolvedConfiguration, skippedProperty.GetMethod.GetFullSimpleName()))
                return false;

            if (skippedProperty.SetMethod != null && !IsMethodSkipped(resolvedConfiguration, skippedProperty.SetMethod.GetFullSimpleName()))
                return false;

            return true;
        }

        private static bool IsMethodSkipped(ResolvedInjectionConfiguration resolvedConfiguration, string skippedMethodFullName) {
            return resolvedConfiguration
                .InjecteeAssemblies
                .SelectMany(assembly => assembly.InjecteeMethods)
                .All(method => method.GetFullSimpleName() != skippedMethodFullName);
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
