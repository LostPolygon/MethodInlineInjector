using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LostPolygon.MethodInlineInjector {
    [XmlRoot("Configuration")]
    public class InjectionConfiguration : SimpleXmlSerializable {
        public List<InjectedMethod> InjectedMethods { get; } = new List<InjectedMethod>();
        public List<InjecteeAssembly> InjecteeAssemblies { get; } = new List<InjecteeAssembly>();

        #region Serialization

        public override void Serialize() {
            base.Serialize();

            // Skip root element when reading
            SerializationHelper.ProcessAdvanceOnRead();

            SerializationHelper.ProcessWhileNotElementEnd(() => {
                if (SerializationHelper.ProcessStartElement(nameof(InjectedMethods))) {
                    SerializationHelper.ProcessAdvanceOnRead();
                    {
                        this.ProcessCollection(InjectedMethods);
                    }
                    SerializationHelper.ProcessEndElement();
                }

                if (SerializationHelper.ProcessStartElement(nameof(InjecteeAssemblies))) {
                    SerializationHelper.ProcessAdvanceOnRead();
                    {
                        this.ProcessCollection(InjecteeAssemblies);
                    }
                    SerializationHelper.ProcessEndElement();
                }
            });
        }

        #endregion

        public class InjecteeAssembly : SimpleXmlSerializable {
            public string AssemblyPath { get; private set; }
            public List<IMemberReferenceWhitelistItem> MemberReferenceWhitelist { get; } = new List<IMemberReferenceWhitelistItem>();
            public List<IAssemblyWhitelistItem> AssemblyWhitelist { get; } = new List<IAssemblyWhitelistItem>();

            public InjecteeAssembly() {
            }

            public InjecteeAssembly(string assemblyPath) {
                AssemblyPath = assemblyPath;
            }

            #region Serialization

            public override void Serialize() {
                base.Serialize();

                SerializationHelper.ProcessStartElement(nameof(InjecteeAssembly));
                {
                    SerializationHelper.ProcessAttributeString(nameof(AssemblyPath), s => AssemblyPath = s, () => AssemblyPath);
                    SerializationHelper.ProcessAdvanceOnRead();

                    SerializationHelper.ProcessStartElement(nameof(MemberReferenceWhitelist));
                    SerializationHelper.ProcessAdvanceOnRead();
                    {
                        this.ProcessCollection(MemberReferenceWhitelist, () => {
                            switch (SerializationHelper.XmlSerializationReader.Name) {
                                case "Filter":
                                    return new MemberReferenceWhitelistFilter();
                                case "Include":
                                    return new MemberReferenceWhitelistFilterInclude();
                                default:
                                    throw new InvalidEnumArgumentException();
                            }
                        });
                    }
                    SerializationHelper.ProcessEndElement();

                    SerializationHelper.ProcessStartElement(nameof(AssemblyWhitelist));
                    SerializationHelper.ProcessAdvanceOnRead();
                    {
                        this.ProcessCollection(AssemblyWhitelist, () => {
                            switch (SerializationHelper.XmlSerializationReader.Name) {
                                case "Assembly":
                                    return new AssemblyWhitelistFilter();
                                case "Include":
                                    return new AssemblyWhitelistFilterInclude();
                                default:
                                    throw new InvalidEnumArgumentException();
                            }
                        });
                    }
                    SerializationHelper.ProcessEndElement();
                }
                SerializationHelper.ProcessEndElement();
            }

            #endregion

            #region MemberReferenceWhitelist

            public interface IMemberReferenceWhitelistItem : ISimpleXmlSerializable {
            }

            public class MemberReferenceWhitelistFilterInclude : FileInclude, IMemberReferenceWhitelistItem {
            }

            public class MemberReferenceWhitelistFilter : SimpleXmlSerializable, IMemberReferenceWhitelistItem {
                private const FilterTypeFlags kDefaultFilterTypeFlags =
                    FilterTypeFlags.SkipTypes |
                    FilterTypeFlags.SkipMethods |
                    FilterTypeFlags.SkipProperties;

                public FilterTypeFlags FilterType { get; private set; } = kDefaultFilterTypeFlags;
                public string Filter { get; private set; }
                public bool IsRegex { get; private set; }

                public MemberReferenceWhitelistFilter() {
                }

                public MemberReferenceWhitelistFilter(FilterTypeFlags filterType, string filter, bool isRegex) {
                    FilterType = filterType;
                    Filter = filter;
                    IsRegex = isRegex;
                }

                #region Serialization

                public override void Serialize() {
                    base.Serialize();

                    SerializationHelper.ProcessStartElement("Filter");
                    {
                        SerializationHelper.ProcessAttributeString(nameof(Filter), s => Filter = s, () => Filter);
                        SerializationHelper.ProcessAttributeString(nameof(IsRegex), s => IsRegex = Convert.ToBoolean(s), () => Convert.ToString(IsRegex));
                        SerializationHelper.ProcessFlagsEnumAttributes(kDefaultFilterTypeFlags, l => FilterType = l, () => FilterType);
                    }
                    SerializationHelper.ProcessAdvanceOnRead();
                    SerializationHelper.ProcessEndElement();
                }

                #endregion

                public override string ToString() {
                    return $"{nameof(Filter)}: '{Filter}', {nameof(IsRegex)}: {IsRegex}, {nameof(FilterType)}: {FilterType}";
                }

                [Flags]
                public enum FilterTypeFlags {
                    SkipTypes = 1 << 0,
                    SkipMethods = 1 << 1,
                    SkipProperties = 1 << 2
                }
            }

            #endregion

            #region AssemblyWhitelist

            public interface IAssemblyWhitelistItem : ISimpleXmlSerializable {
            }

            public class AssemblyWhitelistFilterInclude : FileInclude, IAssemblyWhitelistItem {
            }

            public class AssemblyWhitelistFilter : SimpleXmlSerializable, IAssemblyWhitelistItem {
                public string Name { get; private set; }
                public bool IsStrictCheck { get; private set; }

                public AssemblyWhitelistFilter() {
                }

                public AssemblyWhitelistFilter(string name, bool isStrictCheck) {
                    Name = name;
                    IsStrictCheck = isStrictCheck;
                }

                #region Serialization

                public override void Serialize() {
                    base.Serialize();

                    SerializationHelper.ProcessStartElement("Assembly");
                    {
                        SerializationHelper.ProcessAttributeString(nameof(Name), s => Name = s, () => Name);
                        SerializationHelper.ProcessAttributeString(nameof(IsStrictCheck), s => IsStrictCheck= Convert.ToBoolean(s), () => Convert.ToString(IsStrictCheck));
                    }
                    SerializationHelper.ProcessAdvanceOnRead();
                    SerializationHelper.ProcessEndElement();
                }

                #endregion

                public override string ToString() {
                    return $"{nameof(Name)}: '{Name}', {nameof(IsStrictCheck)}: {IsStrictCheck}";
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

                SerializationHelper.ProcessStartElement("InjectedMethod");
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

                SerializationHelper.ProcessStartElement("Include");
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