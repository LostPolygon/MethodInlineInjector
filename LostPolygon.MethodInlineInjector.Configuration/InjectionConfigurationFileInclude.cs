using System;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    public abstract class InjectionConfigurationFileInclude : SimpleXmlSerializable {
        public string Path { get; private set; }

        protected InjectionConfigurationFileInclude() {
        }

        protected InjectionConfigurationFileInclude(string path) {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override string ToString() {
            return $"{nameof(Path)}: '{Path}'";
        }

        #region Serialization

        protected override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
            {
                SerializationHelper.ProcessAttributeString(nameof(Path), s => Path = s, () => Path);
            }
            SerializationHelper.ProcessAdvanceOnRead();
            SerializationHelper.ProcessEndElement();
        }

        #endregion
    }
}