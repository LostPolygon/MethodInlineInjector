using System;
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

        protected override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(nameof(InjecteeAssembly));
            {
                SerializationHelper.ProcessAttributeString(nameof(AssemblyPath), s => AssemblyPath = s, () => AssemblyPath);
                SerializationHelper.ProcessAdvanceOnRead();

                SerializationHelper.ProcessStartElement(nameof(MemberReferenceBlacklist));
                SerializationHelper.ProcessAdvanceOnRead();
                {
                    this.ProcessCollectionAsReadOnly(
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
                    this.ProcessCollectionAsReadOnly(
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