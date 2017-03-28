using System;
using System.Xml.Serialization;
using LostPolygon.MethodInlineInjector.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Filter")]
    public class MemberReferenceBlacklistFilter : SimpleXmlSerializable, IMemberReferenceBlacklistItem {
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

        protected override void Serialize() {
            base.Serialize();

            Serializer.ProcessStartElement(Serializer.GetXmlRootName(GetType()));
            {
                Serializer.ProcessAttributeString(nameof(Filter), s => Filter = s, () => Filter);
                Serializer.ProcessOptional(() => {
                    Serializer.ProcessFlagsEnumAttributes(kDefaultFilterOptions, s => FilterFlags = s, () => FilterFlags);
                });
            }
            Serializer.ProcessAdvanceOnRead();
            Serializer.ProcessEndElement();
        }

        #endregion
    }
}