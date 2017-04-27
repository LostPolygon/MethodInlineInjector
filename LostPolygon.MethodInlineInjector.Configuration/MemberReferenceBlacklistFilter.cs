using System;
using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Filter")]
    public class MemberReferenceBlacklistFilter : IMemberReferenceBlacklistItem {
        private const MemberReferenceBlacklistFilterFlags kDefaultFilterOptions =
            MemberReferenceBlacklistFilterFlags.SkipTypes |
            MemberReferenceBlacklistFilterFlags.SkipMethods |
            MemberReferenceBlacklistFilterFlags.SkipProperties |
            MemberReferenceBlacklistFilterFlags.MatchAncestors;

        public string Filter { get; private set; }
        public MemberReferenceBlacklistFilterFlags FilterFlags { get; private set; } = kDefaultFilterOptions;
        public bool IsRegex => (FilterFlags & MemberReferenceBlacklistFilterFlags.IsRegex) != 0;
        public bool MatchAncestors => (FilterFlags & MemberReferenceBlacklistFilterFlags.MatchAncestors) != 0;

        private MemberReferenceBlacklistFilter() {
        }

        public MemberReferenceBlacklistFilter(string filter, MemberReferenceBlacklistFilterFlags filterFlags = kDefaultFilterOptions) {
            Filter = !String.IsNullOrEmpty(filter) ? filter : throw new ArgumentNullException(nameof(filter));
            FilterFlags = filterFlags;
        }

        public override string ToString() {
            return $"{nameof(Filter)}: '{Filter}', {nameof(FilterFlags)}: {FilterFlags}";
        }

        #region With.Fody

        public MemberReferenceBlacklistFilter WithFilter(string value) => null;
        public MemberReferenceBlacklistFilter WithFilterFlags(MemberReferenceBlacklistFilterFlags value) => null;

        #endregion

        #region Serialization

        [SerializationMethod]
        public static MemberReferenceBlacklistFilter Serialize(MemberReferenceBlacklistFilter instance, SimpleXmlSerializerBase serializer) {
            instance = instance ?? new MemberReferenceBlacklistFilter();

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