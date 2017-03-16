using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    public abstract class InjectionConfigurationFileInclude : SimpleXmlSerializable {
        public string Path { get; private set; }

        protected InjectionConfigurationFileInclude() {
        }

        protected InjectionConfigurationFileInclude(string path) {
            Path = path;
        }

        #region Serialization

        public override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
            {
                SerializationHelper.ProcessAttributeString(nameof(Path), s => Path = s, () => Path);
            }
            SerializationHelper.ProcessAdvanceOnRead();
            SerializationHelper.ProcessEndElement();
        }

        #endregion

        public override string ToString() {
            return $"{nameof(Path)}: '{Path}'";
        }
    }
}