using System.Collections.ObjectModel;
using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Configuration")]
    public class InjectionConfiguration {
        public ReadOnlyCollection<InjecteeAssembly> InjecteeAssemblies { get; private set; } =
            ReadOnlyCollectionUtility<InjecteeAssembly>.Empty;

        public ReadOnlyCollection<InjectedMethod> InjectedMethods { get; private set; } =
            ReadOnlyCollectionUtility<InjectedMethod>.Empty;

        private InjectionConfiguration() {
        }

        public InjectionConfiguration(
            ReadOnlyCollection<InjecteeAssembly> injecteeAssemblies,
            ReadOnlyCollection<InjectedMethod> injectedMethods
        ) {
            InjecteeAssemblies = injecteeAssemblies ?? InjecteeAssemblies;
            InjectedMethods = injectedMethods ?? InjectedMethods;
        }

        #region With.Fody

        public InjectionConfiguration WithInjecteeAssemblies(ReadOnlyCollection<InjecteeAssembly> value) => null;
        public InjectionConfiguration WithInjectedMethods(ReadOnlyCollection<InjectedMethod> value) => null;

        #endregion

        #region Serialization

        [SerializationMethod]
        public static InjectionConfiguration Serialize(InjectionConfiguration instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new InjectionConfiguration();

            serializer.ProcessStartElement(serializer.GetXmlRootName(instance.GetType()));
            serializer.ProcessAdvanceOnRead();
            {
                serializer.ProcessWhileNotElementEnd(() => {
                    if (serializer.ProcessStartElement(nameof(InjecteeAssemblies))) {
                        serializer.ProcessAdvanceOnRead();
                        {
                            serializer.ProcessCollectionAsReadOnly(v => instance.InjecteeAssemblies = v, () => instance.InjecteeAssemblies);
                        }
                        serializer.ProcessEndElement();
                    }

                    if (serializer.ProcessStartElement(nameof(InjectedMethods))) {
                        serializer.ProcessAdvanceOnRead();
                        {
                            serializer.ProcessCollectionAsReadOnly(v => instance.InjectedMethods = v, () => instance.InjectedMethods);
                        }
                        serializer.ProcessEndElement();
                    }
                });
            }
            serializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}
