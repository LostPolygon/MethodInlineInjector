using System;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    public class InjectedMethod : SimpleXmlSerializable {
        public string AssemblyPath { get; private set; }
        public string MethodFullName { get; private set; }
        public MethodInjectionPosition InjectionPosition { get; private set; } = MethodInjectionPosition.InjecteeMethodStart;
        public MethodReturnBehaviour ReturnBehaviour { get; private set; } = MethodReturnBehaviour.ReturnFromSelf;

        private InjectedMethod() {
        }

        public InjectedMethod(
            string assemblyPath,
            string methodFullName,
            MethodInjectionPosition injectionPosition = MethodInjectionPosition.InjecteeMethodStart,
            MethodReturnBehaviour returnBehaviour = MethodReturnBehaviour.ReturnFromSelf
        ) {
            AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
            MethodFullName = methodFullName ?? throw new ArgumentNullException(nameof(methodFullName));
            InjectionPosition = injectionPosition;
            ReturnBehaviour = returnBehaviour;
        }

        public override string ToString() {
            return $"{nameof(AssemblyPath)}: '{AssemblyPath}', " +
                   $"{nameof(MethodFullName)}: '{MethodFullName}', " +
                   $"{nameof(InjectionPosition)}: {InjectionPosition}, " +
                   $"{nameof(ReturnBehaviour)}: {ReturnBehaviour}";
        }

        #region With.Fody

        public InjectionConfiguration WithAssemblyPath(string value) => null;
        public InjectionConfiguration WithMethodFullName(string value) => null;
        public InjectionConfiguration WithInjectionPosition(MethodInjectionPosition value) => null;
        public InjectionConfiguration WithReturnBehaviour(MethodReturnBehaviour value) => null;

        #endregion

        #region Serialization

        protected override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(nameof(InjectedMethod));
            {
                SerializationHelper.ProcessAttributeString(nameof(AssemblyPath), s => AssemblyPath = s, () => AssemblyPath);
                SerializationHelper.ProcessAttributeString(nameof(MethodFullName), s => MethodFullName = s, () => MethodFullName);
                SerializationHelper.ProcessEnumAttribute(nameof(InjectionPosition), s => InjectionPosition = s, () => InjectionPosition);
                SerializationHelper.ProcessEnumAttribute(nameof(ReturnBehaviour), s => ReturnBehaviour = s, () => ReturnBehaviour);
            }
            SerializationHelper.ProcessAdvanceOnRead();
            SerializationHelper.ProcessEndElement();
        }

        #endregion
    }
}