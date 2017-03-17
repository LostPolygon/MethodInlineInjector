using System;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectedMethod {
        public MethodDefinition MethodDefinition { get; }
        public MethodInjectionPosition InjectionPosition { get; }
        public MethodReturnBehaviour ReturnBehaviour { get; }

        public ResolvedInjectedMethod(
            MethodDefinition methodDefinition, 
            MethodInjectionPosition injectionPosition = MethodInjectionPosition.InjecteeMethodStart,
            MethodReturnBehaviour returnBehaviour = MethodReturnBehaviour.ReturnFromSelf
        ) {
            MethodDefinition = methodDefinition ?? throw new ArgumentNullException(nameof(methodDefinition));
            InjectionPosition = injectionPosition;
            ReturnBehaviour = returnBehaviour;
        }
    }
}