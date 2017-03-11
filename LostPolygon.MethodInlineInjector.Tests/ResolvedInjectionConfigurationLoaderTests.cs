using NUnit.Framework;
using TestInjectedLibrary;
using TestInjecteeLibrary;

namespace LostPolygon.MethodInlineInjector.Tests {
    public class ResolvedInjectionConfigurationLoaderTests : IntegrationTestMainBase {
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

        #region Setup

        public override string InjectedClassName => typeof(TestInjectedLibrary.InvalidInjectedMethods).FullName;

        #endregion
    }
}
