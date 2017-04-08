using System;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    public abstract class InjectionConfigurationFileInclude {
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

        [SerializationMethod]
        public static InjectionConfigurationFileInclude Serialize(InjectionConfigurationFileInclude instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? throw new ArgumentNullException(nameof(instance));

            serializer.ProcessStartElement(serializer.GetXmlRootName(instance.GetType()));
            {
                serializer.ProcessAttributeString(nameof(Path), s => instance.Path = s, () => instance.Path);
            }
            serializer.ProcessAdvanceOnRead();
            serializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}