using System;
using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Filter")]
    public class IgnoredMemberReference : IIgnoredMemberReference {
        private const IgnoredMemberReferenceFlags kDefaultFilterOptions =
            IgnoredMemberReferenceFlags.SkipTypes |
            IgnoredMemberReferenceFlags.SkipMethods |
            IgnoredMemberReferenceFlags.SkipProperties |
            IgnoredMemberReferenceFlags.MatchAncestors;

        public string Filter { get; private set; }
        public IgnoredMemberReferenceFlags FilterFlags { get; private set; } = kDefaultFilterOptions;
        public bool IsRegex => (FilterFlags & IgnoredMemberReferenceFlags.IsRegex) != 0;
        public bool MatchAncestors => (FilterFlags & IgnoredMemberReferenceFlags.MatchAncestors) != 0;

        private IgnoredMemberReference() {
        }

        public IgnoredMemberReference(string filter, IgnoredMemberReferenceFlags filterFlags = kDefaultFilterOptions) {
            Filter = !String.IsNullOrEmpty(filter) ? filter : throw new ArgumentNullException(nameof(filter));
            FilterFlags = filterFlags;
        }

        public override string ToString() {
            return $"{nameof(Filter)}: '{Filter}', {nameof(FilterFlags)}: {FilterFlags}";
        }

        #region With.Fody

        public IgnoredMemberReference WithFilter(string value) => null;
        public IgnoredMemberReference WithFilterFlags(IgnoredMemberReferenceFlags value) => null;

        #endregion

        #region Serialization

        [SerializationMethod]
        public static IgnoredMemberReference Serialize(IgnoredMemberReference instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new IgnoredMemberReference();

            serializer.ProcessStartElement(serializer.GetXmlRootName(instance.GetType()));
            {
                serializer.ProcessAttributeString(nameof(Filter), s => instance.Filter = s, () => instance.Filter);
                serializer.ProcessWithFlags(
                    SimpleXmlSerializerFlags.IsOptional,
                    () => {
                    serializer.ProcessFlagsEnumAttributes(kDefaultFilterOptions, s => instance.FilterFlags = s, () => instance.FilterFlags);
                });
            }
            serializer.ProcessEnterChildOnRead();
            serializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}