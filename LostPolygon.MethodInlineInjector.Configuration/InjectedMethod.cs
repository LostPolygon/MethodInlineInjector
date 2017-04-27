using System;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    public class InjectedMethod {
        public string AssemblyPath { get; private set; }
        public string MethodFullName { get; private set; }
        public MethodInjectionPosition InjectionPosition { get; private set; } = MethodInjectionPosition.InjecteeMethodStart;

        private InjectedMethod() {
        }

        public InjectedMethod(
            string assemblyPath,
            string methodFullName,
            MethodInjectionPosition injectionPosition = MethodInjectionPosition.InjecteeMethodStart
        ) {
            AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
            MethodFullName = methodFullName ?? throw new ArgumentNullException(nameof(methodFullName));
            InjectionPosition = injectionPosition;
        }

        public override string ToString() {
            return $"{nameof(AssemblyPath)}: '{AssemblyPath}', " +
                   $"{nameof(MethodFullName)}: '{MethodFullName}', " +
                   $"{nameof(InjectionPosition)}: {InjectionPosition}";
        }

        #region With.Fody

        public InjectionConfiguration WithAssemblyPath(string value) => null;
        public InjectionConfiguration WithMethodFullName(string value) => null;
        public InjectionConfiguration WithInjectionPosition(MethodInjectionPosition value) => null;

        #endregion

        #region Serialization

        [SerializationMethod]
        public static InjectedMethod Serialize(InjectedMethod instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new InjectedMethod();

            serializer.ProcessStartElement(nameof(InjectedMethod));
            {
                serializer.ProcessAttributeString(nameof(AssemblyPath), s => instance.AssemblyPath = s, () => instance.AssemblyPath);
                serializer.ProcessAttributeString(nameof(MethodFullName), s => instance.MethodFullName = s, () => instance.MethodFullName);
                serializer.ProcessWithFlags(
                    SimpleXmlSerializerFlags.IsOptional,
                    () => {
                    serializer.ProcessEnumAttribute(nameof(InjectionPosition), s => instance.InjectionPosition = s, () => instance.InjectionPosition);
                });
            }
            serializer.ProcessEnterChildOnRead();
            serializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}