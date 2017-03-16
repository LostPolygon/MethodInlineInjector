using System.Collections.ObjectModel;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    public class InjecteeAssembly : SimpleXmlSerializable {
        public string AssemblyPath { get; private set; }
        public ReadOnlyCollection<IMemberReferenceBlacklistItem> MemberReferenceBlacklist { get; private set; } =
            ReadOnlyCollectionUtility<IMemberReferenceBlacklistItem>.Empty;
        public ReadOnlyCollection<IAssemblyReferenceWhitelistItem> AssemblyReferenceWhitelist { get; private set; } =
            ReadOnlyCollectionUtility<IAssemblyReferenceWhitelistItem>.Empty;

        private InjecteeAssembly() {
        }

        public InjecteeAssembly(
            string assemblyPath,
            ReadOnlyCollection<IMemberReferenceBlacklistItem> memberReferenceBlacklist,
            ReadOnlyCollection<IAssemblyReferenceWhitelistItem> assemblyReference
        ) {
            AssemblyPath = assemblyPath;
            MemberReferenceBlacklist = memberReferenceBlacklist ?? MemberReferenceBlacklist;
            AssemblyReferenceWhitelist = assemblyReference ?? AssemblyReferenceWhitelist;
        }

        #region Serialization

        public override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(nameof(InjecteeAssembly));
            {
                SerializationHelper.ProcessAttributeString(nameof(AssemblyPath), s => AssemblyPath = s, () => AssemblyPath);
                SerializationHelper.ProcessAdvanceOnRead();

                SerializationHelper.ProcessStartElement(nameof(MemberReferenceBlacklist));
                SerializationHelper.ProcessAdvanceOnRead();
                {
                    this.ProcessCollectionAsReadonly(
                        v => MemberReferenceBlacklist = v,
                        () => MemberReferenceBlacklist,
                        () =>
                            SimpleXmlSerializationHelper.CreateByKnownInheritors<IMemberReferenceBlacklistItem>(
                                SerializationHelper.XmlSerializationReader.Name
                            )
                    );
                }
                SerializationHelper.ProcessEndElement();

                SerializationHelper.ProcessStartElement(nameof(AssemblyReferenceWhitelist));
                SerializationHelper.ProcessAdvanceOnRead();
                {
                    this.ProcessCollectionAsReadonly(
                        v => AssemblyReferenceWhitelist = v,
                        () => AssemblyReferenceWhitelist,
                        () =>
                            SimpleXmlSerializationHelper.CreateByKnownInheritors<IAssemblyReferenceWhitelistItem>(
                                SerializationHelper.XmlSerializationReader.Name
                            )
                    );
                }
                SerializationHelper.ProcessEndElement();
            }
            SerializationHelper.ProcessEndElement();
        }

        #endregion
    }
}