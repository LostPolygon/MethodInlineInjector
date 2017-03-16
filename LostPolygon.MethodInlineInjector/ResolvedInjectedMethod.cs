using System;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class ResolvedInjectedMethod {
        public InjectedMethod SourceInjectedMethod { get; }
        public MethodDefinition MethodDefinition { get; }

        public ResolvedInjectedMethod(InjectedMethod sourceInjectedMethod, MethodDefinition methodDefinition) {
            SourceInjectedMethod = sourceInjectedMethod ?? throw new ArgumentNullException(nameof(sourceInjectedMethod));
            MethodDefinition = methodDefinition ?? throw new ArgumentNullException(nameof(methodDefinition));
        }
    }
}