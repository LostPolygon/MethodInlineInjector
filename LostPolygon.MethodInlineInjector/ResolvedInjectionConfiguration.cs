using System;
using System.Collections.Generic;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectionConfiguration {
        public IReadOnlyList<ResolvedInjectedMethod> InjectedMethods { get; }
        public IReadOnlyList<ResolvedInjecteeAssembly> InjecteeAssemblies { get; }

        public ResolvedInjectionConfiguration(
            IReadOnlyList<ResolvedInjectedMethod> injectedMethods,
            IReadOnlyList<ResolvedInjecteeAssembly> injecteeAssemblies) {
            InjectedMethods = injectedMethods ?? throw new ArgumentNullException(nameof(injectedMethods));
            InjecteeAssemblies = injecteeAssemblies ?? throw new ArgumentNullException(nameof(injecteeAssemblies));
        }

        #region With.Fody

        public ResolvedInjectionConfiguration WithInjectedMethods(IReadOnlyList<ResolvedInjectedMethod> value) => null;
        public ResolvedInjectionConfiguration WithInjecteeAssemblies(IReadOnlyList<ResolvedInjecteeAssembly> value) => null;

        #endregion
    }
}