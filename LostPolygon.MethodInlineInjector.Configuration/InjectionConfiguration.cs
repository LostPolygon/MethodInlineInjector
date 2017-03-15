using System;
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

        public InjectionConfiguration() {
        }

        public InjectionConfiguration(ReadOnlyCollection<InjecteeAssembly> injecteeAssemblies, ReadOnlyCollection<InjectedMethod> injectedMethods) {
            InjecteeAssemblies = injecteeAssemblies ?? InjecteeAssemblies;
            InjectedMethods = injectedMethods ?? InjectedMethods;
        }

        #region Serialization

        public override void Serialize() {
            base.Serialize();

            SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
            SerializationHelper.ProcessAdvanceOnRead();
            {
                SerializationHelper.ProcessWhileNotElementEnd(() => {
                    if (SerializationHelper.ProcessStartElement(nameof(InjecteeAssemblies))) {
                        SerializationHelper.ProcessAdvanceOnRead();
                        {
                            this.ProcessCollectionAsReadonly(v => InjecteeAssemblies = v, () => InjecteeAssemblies);
                        }
                        SerializationHelper.ProcessEndElement();
                    }

                    if (SerializationHelper.ProcessStartElement(nameof(InjectedMethods))) {
                        SerializationHelper.ProcessAdvanceOnRead();
                        {
                            this.ProcessCollectionAsReadonly(v => InjectedMethods = v, () => InjectedMethods);
                        }
                        SerializationHelper.ProcessEndElement();
                    }
                });
            }
            SerializationHelper.ProcessEndElement();
        }

        #endregion

        public class InjecteeAssembly : SimpleXmlSerializable {
            public string AssemblyPath { get; private set; }
            public ReadOnlyCollection<IMemberReferenceBlacklistItem> MemberReferenceBlacklist { get; private set; } =
                ReadOnlyCollectionUtility<IMemberReferenceBlacklistItem>.Empty;
            public ReadOnlyCollection<IAssemblyReferenceWhitelistItem> AssemblyReferenceWhitelist { get; private set; } =
                ReadOnlyCollectionUtility<IAssemblyReferenceWhitelistItem>.Empty;

            public InjecteeAssembly() {
            }

            public InjecteeAssembly(
                string assemblyPath,
                ReadOnlyCollection<IMemberReferenceBlacklistItem> memberReferenceBlacklist,
                ReadOnlyCollection<IAssemblyReferenceWhitelistItem> assemblyReference) {
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
                                SimpleXmlSerializationHelper.CreateByXmlRootName<IMemberReferenceBlacklistItem>(
                                    SerializationHelper.XmlSerializationReader.Name,
                                    typeof(MemberReferenceBlacklistFilter),
                                    typeof(MemberReferenceBlacklistFilterInclude)
                        ));
                    }
                    SerializationHelper.ProcessEndElement();

                    SerializationHelper.ProcessStartElement(nameof(AssemblyReferenceWhitelist));
                    SerializationHelper.ProcessAdvanceOnRead();
                    {
                        this.ProcessCollectionAsReadonly(
                            v => AssemblyReferenceWhitelist = v,
                            () => AssemblyReferenceWhitelist,
                            () =>
                                SimpleXmlSerializationHelper.CreateByXmlRootName<IAssemblyReferenceWhitelistItem>(
                                    SerializationHelper.XmlSerializationReader.Name,
                                    typeof(AssemblyReferenceWhitelistFilter),
                                    typeof(AssemblyReferenceWhitelistFilterInclude)
                        ));
                    }
                    SerializationHelper.ProcessEndElement();
                }
                SerializationHelper.ProcessEndElement();
            }


            #endregion

            #region MemberReferenceBlacklist

            public interface IMemberReferenceBlacklistItem : ISimpleXmlSerializable {
            }

            [XmlRoot("Include")]
            public class MemberReferenceBlacklistFilterInclude : FileInclude, IMemberReferenceBlacklistItem {
                public MemberReferenceBlacklistFilterInclude() {
                }

                public MemberReferenceBlacklistFilterInclude(string path) : base(path) {
                }
            }

            [XmlRoot("Filter")]
            public class MemberReferenceBlacklistFilter : SimpleXmlSerializable, IMemberReferenceBlacklistItem {
                private const FilterFlags kDefaultFilterOptions =
                    FilterFlags.SkipTypes |
                    FilterFlags.SkipMethods |
                    FilterFlags.SkipProperties;

                public string Filter { get; private set; }
                public FilterFlags FilterOptions { get; private set; } = kDefaultFilterOptions;
                public bool IsRegex => (FilterOptions & FilterFlags.IsRegex) != 0;
                public bool MatchAncestors => (FilterOptions & FilterFlags.MatchAncestors) != 0;

                public MemberReferenceBlacklistFilter() {
                }

                public MemberReferenceBlacklistFilter(string filter, FilterFlags filterOptions) {
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

                [Flags]
                public enum FilterFlags {
                    SkipTypes = 1 << 0,
                    SkipMethods = 1 << 1,
                    SkipProperties = 1 << 2,
                    IsRegex = 1 << 5,
                    MatchAncestors = 1 << 6,
                }
            }

            #endregion

            #region AssemblyReferenceWhitelist

            public interface IAssemblyReferenceWhitelistItem : ISimpleXmlSerializable {
            }

            [XmlRoot("Include")]
            public class AssemblyReferenceWhitelistFilterInclude : FileInclude, IAssemblyReferenceWhitelistItem {
                public AssemblyReferenceWhitelistFilterInclude() {
                }

                public AssemblyReferenceWhitelistFilterInclude(string path) : base(path) {
                }
            }

            [XmlRoot("Assembly")]
            public class AssemblyReferenceWhitelistFilter : SimpleXmlSerializable, IAssemblyReferenceWhitelistItem {
                public string Name { get; private set; }
                public bool IsStrictNameCheck { get; private set; }

                public AssemblyReferenceWhitelistFilter() {
                }

                public AssemblyReferenceWhitelistFilter(string name, bool isStrictNameCheck) {
                    Name = name;
                    IsStrictNameCheck = isStrictNameCheck;
                }

                #region Serialization

                public override void Serialize() {
                    base.Serialize();

                    SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
                    {
                        SerializationHelper.ProcessAttributeString(nameof(Name), s => Name = s, () => Name);
                        SerializationHelper.ProcessAttributeString(nameof(IsStrictNameCheck), s => IsStrictNameCheck = Convert.ToBoolean(s), () => Convert.ToString(IsStrictNameCheck));
                    }
                    SerializationHelper.ProcessAdvanceOnRead();
                    SerializationHelper.ProcessEndElement();
                }

                #endregion

                public override string ToString() {
                    return $"{nameof(Name)}: '{Name}', {nameof(IsStrictNameCheck)}: {IsStrictNameCheck}";
                }
            }

            #endregion
        }

        public class InjectedMethod : SimpleXmlSerializable {
            public string AssemblyPath { get; private set; }
            public string MethodFullName { get; private set; }
            public MethodInjectionPosition InjectionPosition { get; private set; } = MethodInjectionPosition.InjecteeMethodStart;
            public MethodReturnBehaviour ReturnBehaviour { get; private set; } = MethodReturnBehaviour.ReturnFromSelf;

            public InjectedMethod() {
            }

            public InjectedMethod(
                string assemblyPath,
                string methodFullName,
                MethodInjectionPosition injectionPosition = MethodInjectionPosition.InjecteeMethodStart,
                MethodReturnBehaviour returnBehaviour = MethodReturnBehaviour.ReturnFromSelf
                ) {
                AssemblyPath = assemblyPath;
                MethodFullName = methodFullName;
                InjectionPosition = injectionPosition;
                ReturnBehaviour = returnBehaviour;
            }

            #region Serialization

            public override void Serialize() {
                base.Serialize();

                SerializationHelper.ProcessStartElement(nameof(InjectedMethod));
                {
                    SerializationHelper.ProcessAttributeString(nameof(AssemblyPath), s => AssemblyPath = s, () => AssemblyPath);
                    SerializationHelper.ProcessAttributeString(nameof(MethodFullName), s => MethodFullName = s, () => MethodFullName);
                    SerializationHelper.ProcessEnumAttribute(nameof(InjectionPosition), s => InjectionPosition = s, () => InjectionPosition);
                    SerializationHelper.ProcessEnumAttribute(nameof(ReturnBehaviour), s => ReturnBehaviour = s, () => ReturnBehaviour);
                }
                SerializationHelper.ProcessAdvanceOnRead();
                SerializationHelper.ProcessEndElement();
            }

            #endregion

            public override string ToString() {
                return $"{nameof(AssemblyPath)}: '{AssemblyPath}', " +
                       $"{nameof(MethodFullName)}: '{MethodFullName}', " +
                       $"{nameof(InjectionPosition)}: {InjectionPosition}, " +
                       $"{nameof(ReturnBehaviour)}: {ReturnBehaviour}";
            }

            public enum MethodInjectionPosition {
                InjecteeMethodStart,
                InjecteeMethodReturn
            }

            public enum MethodReturnBehaviour {
                ReturnFromSelf,
                ReturnFromInjectee
            }
        }

        public abstract class FileInclude : SimpleXmlSerializable {
            public string Path { get; private set; }

            public FileInclude() {
            }

            public FileInclude(string path) {
                Path = path;
            }

            #region Serialization

            public override void Serialize() {
                base.Serialize();

                SerializationHelper.ProcessStartElement(SimpleXmlSerializationHelper.GetXmlRootName(GetType()));
                {
                    SerializationHelper.ProcessAttributeString(nameof(Path), s => Path = s, () => Path);
                }
                SerializationHelper.ProcessAdvanceOnRead();
                SerializationHelper.ProcessEndElement();
            }

            #endregion

            public override string ToString() {
                return $"{nameof(Path)}: '{Path}'";
            }
        }
    }

}