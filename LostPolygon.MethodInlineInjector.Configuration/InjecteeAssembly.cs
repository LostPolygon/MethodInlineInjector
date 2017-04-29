using System;
using System.Collections.ObjectModel;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    public class InjecteeAssembly {
        public string AssemblyPath { get; private set; }
        public ReadOnlyCollection<IIgnoredMemberReference> IgnoredMemberReferences { get; private set; } =
            ReadOnlyCollectionUtility<IIgnoredMemberReference>.Empty;
        public ReadOnlyCollection<IAllowedAssemblyReference> AllowedAssemblyReferences { get; private set; } =
            ReadOnlyCollectionUtility<IAllowedAssemblyReference>.Empty;

        private InjecteeAssembly() {
        }

        public InjecteeAssembly(
            string assemblyPath,
            ReadOnlyCollection<IIgnoredMemberReference> ignoredMemberReferences = null,
            ReadOnlyCollection<IAllowedAssemblyReference> allowedAssemblyReferences = null
        ) {
            AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
            IgnoredMemberReferences = ignoredMemberReferences ?? IgnoredMemberReferences;
            AllowedAssemblyReferences = allowedAssemblyReferences ?? AllowedAssemblyReferences;
        }

        #region With.Fody

        public InjecteeAssembly WithAssemblyPath(string value) => null;
        public InjecteeAssembly WithIgnoredMemberReferences(ReadOnlyCollection<IIgnoredMemberReference> value) => null;
        public InjecteeAssembly WithAllowedAssemblyReferences(ReadOnlyCollection<IAllowedAssemblyReference> value) => null;

        #endregion

        #region Serialization

        [SerializationMethod]
        public static InjecteeAssembly Serialize(InjecteeAssembly instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new InjecteeAssembly();

            serializer.ProcessStartElement(nameof(InjecteeAssembly));
            {
                serializer.ProcessAttributeString(nameof(AssemblyPath), s => instance.AssemblyPath = s, () => instance.AssemblyPath);
                serializer.ProcessEnterChildOnRead();

                serializer.ProcessWithFlags(SimpleXmlSerializerFlags.IsOptional | SimpleXmlSerializerFlags.CollectionUnorderedRequired, () => {
                    serializer.ProcessUnorderedSequence(() => {
                        if (serializer.ProcessStartElement(nameof(IgnoredMemberReferences))) {
                            serializer.ProcessEnterChildOnRead();
                            serializer.ProcessCollectionAsReadOnly(
                                v => instance.IgnoredMemberReferences = v,
                                () => instance.IgnoredMemberReferences,
                                itemSerializer =>
                                    serializer.CreateByKnownInheritors<IIgnoredMemberReference>(
                                        serializer.CurrentXmlElement.Name,
                                        itemSerializer
                                    )
                            );
                        }
                        serializer.ProcessEndElement();

                        if (serializer.ProcessStartElement(nameof(AllowedAssemblyReferences))) {
                            serializer.ProcessEnterChildOnRead();
                            serializer.ProcessCollectionAsReadOnly(
                                v => instance.AllowedAssemblyReferences = v,
                                () => instance.AllowedAssemblyReferences,
                                itemSerializer =>
                                    serializer.CreateByKnownInheritors<IAllowedAssemblyReference>(
                                        serializer.CurrentXmlElement.Name,
                                        itemSerializer
                                    )
                            );
                        }
                        serializer.ProcessEndElement();
                    });
                });
            }
            serializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}