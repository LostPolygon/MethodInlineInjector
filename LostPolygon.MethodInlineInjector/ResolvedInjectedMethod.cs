using System;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectedMethod {
        public MethodDefinition MethodDefinition { get; }
        public MethodInjectionPosition InjectionPosition { get; }

        public ResolvedInjectedMethod(
            MethodDefinition methodDefinition,
            MethodInjectionPosition injectionPosition = MethodInjectionPosition.InjecteeMethodStart
        ) {
            MethodDefinition = methodDefinition ?? throw new ArgumentNullException(nameof(methodDefinition));
            InjectionPosition = injectionPosition;
        }
    }
}