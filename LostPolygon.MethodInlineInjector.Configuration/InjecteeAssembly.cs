using System;
using System.Collections.ObjectModel;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    public class InjecteeAssembly {
        public string AssemblyPath { get; private set; }
        public ReadOnlyCollection<IMemberReferenceBlacklistItem> MemberReferenceBlacklist { get; private set; } =
            ReadOnlyCollectionUtility<IMemberReferenceBlacklistItem>.Empty;
        public ReadOnlyCollection<IAssemblyReferenceWhitelistItem> AssemblyReferenceWhitelist { get; private set; } =
            ReadOnlyCollectionUtility<IAssemblyReferenceWhitelistItem>.Empty;

        private InjecteeAssembly() {
        }

        public InjecteeAssembly(
            string assemblyPath,
            ReadOnlyCollection<IMemberReferenceBlacklistItem> memberReferenceBlacklist = null,
            ReadOnlyCollection<IAssemblyReferenceWhitelistItem> assemblyReferenceWhitelist = null
        ) {
            AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
            MemberReferenceBlacklist = memberReferenceBlacklist ?? MemberReferenceBlacklist;
            AssemblyReferenceWhitelist = assemblyReferenceWhitelist ?? AssemblyReferenceWhitelist;
        }

        #region With.Fody

        public InjecteeAssembly WithAssemblyPath(string value) => null;
        public InjecteeAssembly WithMemberReferenceBlacklist(ReadOnlyCollection<IMemberReferenceBlacklistItem> value) => null;
        public InjecteeAssembly WithAssemblyReferenceWhitelist(ReadOnlyCollection<IAssemblyReferenceWhitelistItem> value) => null;

        #endregion

        #region Serialization

        [SerializationMethod]
        public static InjecteeAssembly Serialize(InjecteeAssembly instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new InjecteeAssembly();

            serializer.ProcessStartElement(nameof(InjecteeAssembly));
            {
                serializer.ProcessAttributeString(nameof(AssemblyPath), s => instance.AssemblyPath = s, () => instance.AssemblyPath);
                serializer.ProcessAdvanceOnRead();

                serializer.ProcessWithFlags(
                    SimpleXmlSerializerFlags.IsOptional, 
                    () => {
                    serializer.ProcessStartElement(nameof(MemberReferenceBlacklist));
                    serializer.ProcessAdvanceOnRead();
                    {
                        serializer.ProcessCollectionAsReadOnly(
                            v => instance.MemberReferenceBlacklist = v,
                            () => instance.MemberReferenceBlacklist,
                            itemSerializer =>
                                serializer.CreateByKnownInheritors<IMemberReferenceBlacklistItem>(
                                    serializer.CurrentXmlElement.Name,
                                    itemSerializer
                                )
                        );
                    }
                    serializer.ProcessEndElement();

                    serializer.ProcessStartElement(nameof(AssemblyReferenceWhitelist));
                    serializer.ProcessAdvanceOnRead();
                    {
                        serializer.ProcessCollectionAsReadOnly(
                            v => instance.AssemblyReferenceWhitelist = v,
                            () => instance.AssemblyReferenceWhitelist,
                            itemSerializer =>
                                serializer.CreateByKnownInheritors<IAssemblyReferenceWhitelistItem>(
                                    serializer.CurrentXmlElement.Name,
                                    itemSerializer
                                )
                        );
                    }
                    serializer.ProcessEndElement();
                });
            }
            serializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}