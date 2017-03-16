using System.Collections.ObjectModel;
using System.Xml.Serialization;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Configuration")]
    public class InjectionConfiguration : SimpleXmlSerializable {
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

        protected override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
            SerializationHelper.ProcessAdvanceOnRead();
            {
                SerializationHelper.ProcessWhileNotElementEnd(() => {
                    if (SerializationHelper.ProcessStartElement(nameof(InjecteeAssemblies))) {
                        SerializationHelper.ProcessAdvanceOnRead();
                        {
                            this.ProcessCollectionAsReadOnly(v => InjecteeAssemblies = v, () => InjecteeAssemblies);
                        }
                        SerializationHelper.ProcessEndElement();
                    }

                    if (SerializationHelper.ProcessStartElement(nameof(InjectedMethods))) {
                        SerializationHelper.ProcessAdvanceOnRead();
                        {
                            this.ProcessCollectionAsReadOnly(v => InjectedMethods = v, () => InjectedMethods);
                        }
                        SerializationHelper.ProcessEndElement();
                    }
                });
            }
            SerializationHelper.ProcessEndElement();
        }

        #endregion
    }
}
