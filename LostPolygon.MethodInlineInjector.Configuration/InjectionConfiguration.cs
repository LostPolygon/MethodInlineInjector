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

            Serializer.ProcessStartElement(Serializer.GetXmlRootName(GetType()));
            Serializer.ProcessAdvanceOnRead();
            {
                if (Serializer.ProcessStartElement(nameof(InjecteeAssemblies))) {
                    Serializer.ProcessAdvanceOnRead();
                    {
                        Serializer.ProcessCollectionAsReadOnly(v => InjecteeAssemblies = v, () => InjecteeAssemblies);
                    }
                    Serializer.ProcessEndElement();
                }

                if (Serializer.ProcessStartElement(nameof(InjectedMethods))) {
                    Serializer.ProcessAdvanceOnRead();
                    {
                        Serializer.ProcessCollectionAsReadOnly(v => InjectedMethods = v, () => InjectedMethods);
                    }
                    Serializer.ProcessEndElement();
                }
            }
            Serializer.ProcessEndElement();
        }

        #endregion
    }
}
