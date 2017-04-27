using System;
using System.Xml.Serialization;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Filter")]
    public class MemberReferenceBlacklistFilter : IMemberReferenceBlacklistItem {
        private const MemberReferenceBlacklistFilterFlags kDefaultFilterOptions =
            MemberReferenceBlacklistFilterFlags.SkipTypes |
            MemberReferenceBlacklistFilterFlags.SkipMethods |
            MemberReferenceBlacklistFilterFlags.SkipProperties;

        public string Filter { get; private set; }
        public MemberReferenceBlacklistFilterFlags FilterFlags { get; private set; } = kDefaultFilterOptions;
        public bool IsRegex => (FilterFlags & MemberReferenceBlacklistFilterFlags.IsRegex) != 0;
        public bool MatchAncestors => (FilterFlags & MemberReferenceBlacklistFilterFlags.MatchAncestors) != 0;

        private MemberReferenceBlacklistFilter() {
        }

        public MemberReferenceBlacklistFilter(string filter, MemberReferenceBlacklistFilterFlags filterFlags = kDefaultFilterOptions) {
            Filter = String.IsNullOrEmpty(filter) ? throw new ArgumentNullException(nameof(filter)) : filter;
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
        public static MemberReferenceBlacklistFilter Serialize(MemberReferenceBlacklistFilter instance, SimpleXmlSerializerBase sSerializer) {
            instance = instance ?? new MemberReferenceBlacklistFilter();

            sSerializer.ProcessStartElement(sSerializer.GetXmlRootName(instance.GetType()));
            {
                sSerializer.ProcessAttributeString(nameof(Filter), s => instance.Filter = s, () => instance.Filter);
                sSerializer.ProcessWithFlags(
                    SimpleXmlSerializerFlags.IsOptional,
                    () => {
                    sSerializer.ProcessFlagsEnumAttributes(kDefaultFilterOptions, s => instance.FilterFlags = s, () => instance.FilterFlags);
                });
            }
            sSerializer.ProcessEnterChildOnRead();
            sSerializer.ProcessEndElement();

            return instance;
        }

        #endregion
    }
}