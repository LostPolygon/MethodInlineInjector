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
            AssemblyPath = assemblyPath;
            MethodFullName = methodFullName;
            InjectionPosition = injectionPosition;
            ReturnBehaviour = returnBehaviour;
        }

        public override string ToString() {
            return $"{nameof(AssemblyPath)}: '{AssemblyPath}', " +
                   $"{nameof(MethodFullName)}: '{MethodFullName}', " +
                   $"{nameof(InjectionPosition)}: {InjectionPosition}, " +
                   $"{nameof(ReturnBehaviour)}: {ReturnBehaviour}";
        }

        #region Serialization

        public override void Serialize() {
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