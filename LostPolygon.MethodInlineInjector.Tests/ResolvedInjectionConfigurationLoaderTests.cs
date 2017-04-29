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
        [ExpectedException(typeof(MethodInlineInjectorException))]
        public void AttemptInjectUsesThirdPartyLibrary() {
            AttemptInjectUsesThirdPartyLibraryBase(false);
        }

        [Test]
        public void AttemptInjectUsesThirdPartyLibraryWithPathSet() {
            AttemptInjectUsesThirdPartyLibraryBase(true);
        }

        private void AttemptInjectUsesThirdPartyLibraryBase(bool setThirdPartyLibraryPath) {
            InjectedMethod injectedMethod =
                new InjectedMethod(
                    InjectedLibraryPath,
                    $"{typeof(TestInjectedMethods).FullName}.{nameof(TestInjectedMethods.UsesThirdPartyLibrary)}"
                );

            InjectionConfiguration configuration = IntegrationTestsHelper.GetBasicInjectionConfiguration(false, false, injectedMethod);
            configuration =
                configuration
                    .WithInjecteeAssemblies(
                        configuration.InjecteeAssemblies.Select(
                            assembly => {
                                List<IAllowedAssemblyReference> list = assembly.AllowedAssemblyReferences.ToList();
                                list.Add(
                                    new AllowedAssemblyReference(
                                        "Tests.ThirdPartyLibrary",
                                        false,
                                        setThirdPartyLibraryPath ? "Tests.ThirdPartyLibrary.Different.dll" : null));
                                List<IIgnoredMemberReference> list2 = assembly.IgnoredMemberReferences.ToList();
                                list2.Add(new IgnoredMemberReference("some filter"));
                                assembly =
                                    assembly
                                        .WithAllowedAssemblyReferences(list.AsReadOnly())
                                        .WithIgnoredMemberReferences(list2.AsReadOnly());
                                return assembly;
                            })
                            .ToList().AsReadOnly()
                    );

            ExecuteSimpleTest(
                configuration,
                new[] { $"{InjecteeClassName}.{nameof(TestInjectee.SingleStatement)}" },
                false
            );
        }

        [Test]
        public void IgnoreTypeTest() {
            string ignoredTypeFullName = typeof(TestInjectee).FullName;
            ExecuteSimpleIgnoreTypeTest(ignoredTypeFullName);
        }

        [Test]
        public void IgnoreTypeByRegexTest() {
            ResolvedInjectionConfiguration resolvedConfiguration =
                ExecuteIgnoreTest(
                    new IgnoredMemberReference(
                        "Injecte[ed]",
                        IgnoredMemberReferenceFlags.SkipTypes |
                        IgnoredMemberReferenceFlags.IsRegex |
                        IgnoredMemberReferenceFlags.MatchAncestors
                    )
                );

            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(TestInjectee).FullName));
            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(ChildTestInjectee).FullName));
            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(StructTestInjectee).FullName));
        }

        [Test]
        public void IgnoreTypeAndChildTypesTest() {
            ResolvedInjectionConfiguration resolvedConfiguration =
                ExecuteSimpleIgnoreTypeTest(
                    typeof(TestInjectee).FullName,
                    IgnoredMemberReferenceFlags.SkipTypes |
                    IgnoredMemberReferenceFlags.MatchAncestors
                );

            Assert.True(IsTypeSkipped(resolvedConfiguration, typeof(ChildTestInjectee).FullName));
        }

        [Test]
        public void IgnoreStructTypeTest() {
            ExecuteSimpleIgnoreTypeTest(
                typeof(StructTestInjectee).FullName,
                IgnoredMemberReferenceFlags.SkipTypes |
                IgnoredMemberReferenceFlags.MatchAncestors
            );
        }

        [Test]
        public void IgnoreVirtualMethodTest() {
            string methodName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            string methodNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleIgnoreTest(
                methodName,
                IgnoredMemberReferenceFlags.SkipMethods
            );

            Assert.True(IsMethodSkipped(configuration, methodName));
            Assert.False(IsMethodSkipped(configuration, methodNameChild));
        }

        [Test]
        public void IgnoreVirtualMethodOverrideTest() {
            string methodName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            string methodNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatement)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleIgnoreTest(
                    methodName,
                    IgnoredMemberReferenceFlags.SkipMethods |
                    IgnoredMemberReferenceFlags.MatchAncestors
                );

            Assert.True(IsMethodSkipped(configuration, methodName));
            Assert.True(IsMethodSkipped(configuration, methodNameChild));
        }

        [Test]
        public void IgnoreVirtualPropertyTest() {
            string propertyName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            string propertyNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleIgnoreTest(
                    propertyName,
                    IgnoredMemberReferenceFlags.SkipProperties
                );

            Assert.True(IsPropertySkipped(configuration, propertyName));
            Assert.False(IsPropertySkipped(configuration, propertyNameChild));
        }

        [Test]
        public void IgnoreVirtualPropertyOverrideTest() {
            string propertyName = $"{typeof(TestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            string propertyNameChild = $"{typeof(ChildTestInjectee).FullName}.{nameof(TestInjectee.VirtualSingleStatementProperty)}";
            ResolvedInjectionConfiguration configuration =
                ExecuteSimpleIgnoreTest(
                    propertyName,
                    IgnoredMemberReferenceFlags.SkipProperties |
                    IgnoredMemberReferenceFlags.MatchAncestors
                );

            Assert.True(IsPropertySkipped(configuration, propertyName));
            Assert.True(IsPropertySkipped(configuration, propertyNameChild));
        }

        private ResolvedInjectionConfiguration ExecuteSimpleIgnoreTest(
            string ignoredMethodFullName,
            IgnoredMemberReferenceFlags ignoreFlags = IgnoredMemberReferenceFlags.SkipMethods
        ) {
            IIgnoredMemberReference[] ignoredMemberReferences = {
                new IgnoredMemberReference(
                    ignoredMethodFullName,
                    ignoreFlags
                ),
            };

            ResolvedInjectionConfiguration resolvedConfiguration = ExecuteIgnoreTest(ignoredMemberReferences);

            return resolvedConfiguration;
        }

        private ResolvedInjectionConfiguration ExecuteSimpleIgnoreTypeTest(
            string ignoredTypeFullName,
            IgnoredMemberReferenceFlags ignoreFlags =
                IgnoredMemberReferenceFlags.SkipTypes
        ) {
            ResolvedInjectionConfiguration resolvedConfiguration = ExecuteSimpleIgnoreTest(ignoredTypeFullName, ignoreFlags);
            Assert.True(IsTypeSkipped(resolvedConfiguration, ignoredTypeFullName));

            return resolvedConfiguration;
        }

        private ResolvedInjectionConfiguration ExecuteIgnoreTest(
            params IIgnoredMemberReference[] ignoredMemberReferences
        ) {
            InjectionConfiguration configuration = GetInjectionConfiguration(ignoredMemberReferences.ToList());

            // Strip includes
            configuration = StripIncludesFromConfiguration(configuration);

            ResolvedInjectionConfiguration resolvedConfiguration =
                ResolvedInjectionConfigurationLoader.LoadFromInjectionConfiguration(configuration);
            ExecuteSimpleTest(resolvedConfiguration, false);
            return resolvedConfiguration;
        }

        private static InjectionConfiguration StripIncludesFromConfiguration(InjectionConfiguration configuration) {
            configuration =
                configuration.WithInjecteeAssemblies(
                    configuration.InjecteeAssemblies.Select(assembly =>
                            assembly.WithAllowedAssemblyReferences(
                                    assembly.AllowedAssemblyReferences.Where(item => !(item is InjectionConfigurationFileInclude)).ToList().AsReadOnly()
                                )
                                .WithIgnoredMemberReferences(
                                    assembly.IgnoredMemberReferences.Where(item => !(item is InjectionConfigurationFileInclude)).ToList().AsReadOnly()
                                )
                        )
                        .ToList().AsReadOnly()
                );
            return configuration;
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
