using System.Xml.Serialization;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Filter")]
    public class MemberReferenceBlacklistFilter : SimpleXmlSerializable, IMemberReferenceBlacklistItem {
        private const MemberReferenceFilterFlags kDefaultFilterOptions =
            MemberReferenceFilterFlags.SkipTypes |
            MemberReferenceFilterFlags.SkipMethods |
            MemberReferenceFilterFlags.SkipProperties;

        public string Filter { get; private set; }
        public MemberReferenceFilterFlags FilterOptions { get; private set; } = kDefaultFilterOptions;
        public bool IsRegex => (FilterOptions & MemberReferenceFilterFlags.IsRegex) != 0;
        public bool MatchAncestors => (FilterOptions & MemberReferenceFilterFlags.MatchAncestors) != 0;

        private MemberReferenceBlacklistFilter() {
        }

        public MemberReferenceBlacklistFilter(string filter, MemberReferenceFilterFlags filterOptions) {
            Filter = filter;
            FilterOptions = filterOptions;
        }

        #region Serialization

        public override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
            {
                SerializationHelper.ProcessAttributeString(nameof(Filter), s => Filter = s, () => Filter);
                SerializationHelper.ProcessFlagsEnumAttributes(kDefaultFilterOptions, s => FilterOptions = s, () => FilterOptions);
            }
            SerializationHelper.ProcessAdvanceOnRead();
            SerializationHelper.ProcessEndElement();
        }

        #endregion

        public override string ToString() {
            return $"{nameof(Filter)}: '{Filter}', {nameof(FilterOptions)}: {FilterOptions}";
        }

    }
}