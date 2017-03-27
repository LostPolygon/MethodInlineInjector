using System;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    public abstract class InjectionConfigurationFileInclude : SimpleXmlSerializable {
        public string Path { get; private set; }

        protected InjectionConfigurationFileInclude() {
        }

        protected InjectionConfigurationFileInclude(string path) {
            Path = String.IsNullOrEmpty(path) ? throw new ArgumentNullException(nameof(path)) : path;
        }

        public override string ToString() {
            return $"{nameof(Path)}: '{Path}'";
        }

        #region Serialization

        protected override void Serialize() {
            base.Serialize();

            Serializer.ProcessStartElement(SimpleXmlSerializer.GetXmlRootName(GetType()));
            {
                Serializer.ProcessAttributeString(nameof(Path), s => Path = s, () => Path);
            }
            Serializer.ProcessAdvanceOnRead();
            Serializer.ProcessEndElement();
        }

        #endregion
    }
}