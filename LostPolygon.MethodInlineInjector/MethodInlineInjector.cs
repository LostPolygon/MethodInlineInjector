using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace LostPolygon.MethodInlineInjector {
    public class MethodInlineInjector {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(nameof(MethodInlineInjector));

        private readonly ResolvedInjectionConfiguration _resolvedInjectionConfiguration;

        public event Action<(ResolvedInjectedMethod injectedMethod, MethodDefinition injecteeMethod)> BeforeMethodInjected;

        public MethodInlineInjector(ResolvedInjectionConfiguration resolvedInjectionConfiguration) {
            _resolvedInjectionConfiguration = resolvedInjectionConfiguration;
        }

        public void Inject() {
            foreach (ResolvedInjecteeAssembly injecteeAssembly in _resolvedInjectionConfiguration.InjecteeAssemblies) {
                var assemblyMethodsGroupings =
                    _resolvedInjectionConfiguration
                    .InjectedMethods
                    .GroupBy(method => method.MethodDefinition.Module.Assembly);

                foreach (var assemblyMethodsGrouping in assemblyMethodsGroupings) {
                    InjectToAssembly(injecteeAssembly, assemblyMethodsGrouping.ToArray(), assemblyMethodsGrouping.Key);
                }
            }
        }

        private void InjectToAssembly(
            ResolvedInjecteeAssembly resolvedInjecteeAssembly,
            IReadOnlyList<ResolvedInjectedMethod> injectedMethods,
            AssemblyDefinition injectedMethodsAssembly
        ) {
            Collection<AssemblyNameReference> injecteeAssemblyNameReferences =
                resolvedInjecteeAssembly.AssemblyDefinition.MainModule.AssemblyReferences;

            IEnumerable<TypeReference> injectedTypeReferences =
                injectedMethodsAssembly.MainModule.GetTypeReferences();

            foreach (TypeReference injectedTypeReference in injectedTypeReferences) {
                AssemblyNameReference injectedAssemblyNameReference = injectedTypeReference.Scope as AssemblyNameReference;

                // TODO: add strict name check mode
                AssemblyNameReference matchingAssemblyNameReference =
                    GetMatchingAssemblyNameReference(injectedAssemblyNameReference, injecteeAssemblyNameReferences, false);

                if (matchingAssemblyNameReference == null) {
                    bool IsAssemblyReferenceWhitelisted(
                        AssemblyNameReference assemblyNameReference,
                        AssemblyNameReference whitelistedAssemblyNameReference,
                        bool isStrictCheck) {
                        if (isStrictCheck) {
                            return assemblyNameReference.FullName == whitelistedAssemblyNameReference.FullName;
                        } else {
                            return assemblyNameReference.Name == whitelistedAssemblyNameReference.Name;
                        }
                    }

                    bool isWhitelisted =
                        resolvedInjecteeAssembly
                            .AssemblyReferenceWhiteList
                            .Any(tuple =>
                                IsAssemblyReferenceWhitelisted(
                                    injectedAssemblyNameReference,
                                    tuple.AssemblyNameReference,
                                    tuple.StrictNameCheck
                                ));

                    if (!isWhitelisted)
                        throw new MethodInlineInjectorException(
                            $"Assembly '{injectedAssemblyNameReference}' is not whitelisted " +
                            $"and cannot be added as a reference"
                        );

                    Log.Info(
                        $"Injectee assembly '{resolvedInjecteeAssembly.AssemblyDefinition} ' " +
                        $"has no match for assembly reference '{injectedTypeReference.Scope}', " +
                        $"the reference will be added"
                    );
                } else {
                    injectedTypeReference.Scope = matchingAssemblyNameReference;
                }
            }

            foreach (MethodDefinition injecteeMethod in resolvedInjecteeAssembly.InjecteeMethods) {
                foreach (ResolvedInjectedMethod injectedMethod in injectedMethods) {
                    Log.DebugFormat(
                        "Injecting method '{0}' to method {1} at {2}",
                        injectedMethod.MethodDefinition.GetFullSimpleName(),
                        injecteeMethod.GetFullSimpleName(),
                        injectedMethod.InjectionPosition
                    );

                    BeforeMethodInjected?.Invoke((injectedMethod, injecteeMethod));
                    MethodDefinition importedInjectedMethod =
                        CloneAndImportMethod(
                            injectedMethod.MethodDefinition,
                            resolvedInjecteeAssembly.AssemblyDefinition
                        );
                    InjectMethod(
                        importedInjectedMethod,
                        injecteeMethod,
                        injectedMethod.InjectionPosition
                    );
                }
            }
        }

        private static AssemblyNameReference GetMatchingAssemblyNameReference(
            AssemblyNameReference injectedAssemblyNameReference,
            IEnumerable<AssemblyNameReference> injecteeAssemblyNameReferences,
            bool isStrictCheck
        ) {
            foreach (AssemblyNameReference injecteeAssemblyNameReference in injecteeAssemblyNameReferences) {
                if (isStrictCheck) {
                    if (injectedAssemblyNameReference.FullName == injecteeAssemblyNameReference.FullName)
                        return injecteeAssemblyNameReference;
                } else {
                    if (injectedAssemblyNameReference.Name == injecteeAssemblyNameReference.Name)
                        return injecteeAssemblyNameReference;
                }
            }

            return null;
        }

        private static void InjectMethod(
            MethodDefinition injectedMethod,
            MethodDefinition injecteeMethod,
            MethodInjectionPosition injectionPosition
            ) {
            // Unroll short form instructions so they can be auto-fixed by Cecil
            // automatically when new instructions are inserted
            injectedMethod.Body.SimplifyMacros();
            injecteeMethod.Body.SimplifyMacros();

            injecteeMethod.Body.InitLocals |= injectedMethod.Body.InitLocals;

            ILProcessor injecteeIlProcessor = injecteeMethod.Body.GetILProcessor();
            if (injectionPosition == MethodInjectionPosition.InjecteeMethodStart) {
                // Inject variables to the beginning of the variable list
                injecteeMethod.Body.Variables.InsertRangeToStart(injectedMethod.Body.Variables);

                // First instruction of the injectee method. Instruction of the injected methods are inserted before it
                Instruction injecteeFirstInstruction = injecteeMethod.Body.Instructions[0];
                Instruction injectedLastInstruction = injectedMethod.Body.Instructions.Last();

                // Insert injected method to the beginning
                for (int i = 0; i < injectedMethod.Body.Instructions.Count; i++) {
                    Instruction injectedInstruction = injectedMethod.Body.Instructions[i];
                    injecteeIlProcessor.InsertBefore(injecteeFirstInstruction, injectedInstruction);
                }

                // Clone exception handlers
                injecteeMethod.Body.ExceptionHandlers.AddRange(injectedMethod.Body.ExceptionHandlers);

                // Replace Ret from the end of the injected method with Nop,
                // so the execution could go to injectee code after the injected method end
                injectedLastInstruction = injecteeIlProcessor.ReplaceAndFixReferences(injectedLastInstruction, Instruction.Create(OpCodes.Nop));
            } else if (injectionPosition == MethodInjectionPosition.InjecteeMethodReturn) {
                // Inject variables to the end of the variable list
                injecteeMethod.Body.Variables.AddRange(injectedMethod.Body.Variables);

                // Ret instruction at the end of the injectee method must be replace with Nop,
                // so the execution could go to the injected code after the injecteed method end
                Instruction injecteeLastInstruction = injecteeMethod.Body.Instructions.Last();

                // Append injected method to the end
                for (int i = injectedMethod.Body.Instructions.Count - 1; i >= 0 ; i--) {
                    Instruction injectedInstruction = injectedMethod.Body.Instructions[i];
                    injecteeIlProcessor.InsertAfter(injecteeLastInstruction, injectedInstruction);
                }

                // Clone exception handlers
                injecteeMethod.Body.ExceptionHandlers.AddRange(injectedMethod.Body.ExceptionHandlers);

                // Replace Ret from the end of the injectee method with Nop,
                // so the execution could go to injected code after the injectee method end
                injecteeLastInstruction = injecteeIlProcessor.ReplaceAndFixReferences(injecteeLastInstruction, Instruction.Create(OpCodes.Nop));
            }

            injecteeMethod.Body.OptimizeMacros();
        }

        private static MethodDefinition CloneAndImportMethod(MethodDefinition sourceMethod, AssemblyDefinition targetAssembly) {
            TypeReference importedReturnTypeReference = targetAssembly.MainModule.Import(sourceMethod.ReturnType);
            MethodDefinition clonedMethod = new MethodDefinition(sourceMethod.Name, sourceMethod.Attributes, importedReturnTypeReference) {
                DeclaringType = targetAssembly.MainModule.Types[0]
            };
            MethodDefinitionCloner methodDefinitionCloner = new MethodDefinitionClonerValidated(sourceMethod, clonedMethod, targetAssembly.MainModule);
            methodDefinitionCloner.Clone();

            return clonedMethod;
        }

        private class MethodDefinitionClonerValidated : MethodDefinitionCloner {
            public MethodDefinitionClonerValidated(
                MethodDefinition sourceMethod,
                MethodDefinition targetMethod,
                ModuleDefinition targetModule)
                : base(sourceMethod, targetMethod, targetModule) {
            }

            protected override TypeReference ImportTypeReference(TypeReference operand) {
                ValidateTypeReference(operand);
                return base.ImportTypeReference(operand);
            }

            protected override MethodReference ImportMethodReference(MethodReference operand) {
                ValidateTypeReference(operand.DeclaringType);
                ValidateTypeReference(operand.ReturnType);
                foreach (ParameterDefinition parameterDefinition in operand.Parameters) {
                    ValidateTypeReference(parameterDefinition.ParameterType);
                }

                foreach (GenericParameter genericParameter in operand.GenericParameters) {
                    ValidateTypeReference(genericParameter.DeclaringType);
                }

                return base.ImportMethodReference(operand);
            }

            protected override FieldReference ImportFieldReference(FieldReference operand) {
                ValidateTypeReference(operand.DeclaringType);
                ValidateTypeReference(operand.FieldType);

                return base.ImportFieldReference(operand);
            }

            private void ValidateTypeReference(TypeReference type) {
                if (type.Scope == SourceMethod.Module)
                    throw new MethodInlineInjectorException(
                        $"Type '{type.FullName}' was attempted to be imported from injected assembly " +
                        $"'{SourceMethod.Module.Assembly}' to the injectee assembly '{TargetMethod.Module.Assembly}'");
            }
        }
    }
}