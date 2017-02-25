using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LostPolygon.AssemblyMethodInlineInjector {
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
                if (SerializationHelper.ProcessStartElement("InjectedMethods")) {
                    SerializationHelper.ProcessAdvanceOnRead();
                    {
                        this.ProcessCollection(InjectedMethods);
                    }
                    SerializationHelper.ProcessEndElement();
                }

                if (SerializationHelper.ProcessStartElement("InjecteeAssemblies")) {
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

                SerializationHelper.ProcessStartElement("InjecteeAssembly");
                {
                    SerializationHelper.ProcessAttributeString("AssemblyPath", s => AssemblyPath = s, () => AssemblyPath);
                    SerializationHelper.ProcessAdvanceOnRead();

                    SerializationHelper.ProcessStartElement("MemberReferenceWhitelist");
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

                    SerializationHelper.ProcessStartElement("AssemblyWhitelist");
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
                        SerializationHelper.ProcessAttributeString("Filter", s => Filter = s, () => Filter);
                        SerializationHelper.ProcessAttributeString("IsRegex", s => IsRegex = Convert.ToBoolean(s), () => Convert.ToString(IsRegex));
                        SerializationHelper.ProcessFlagsEnumAttributes(kDefaultFilterTypeFlags, l => FilterType = l, () => FilterType);
                    }
                    SerializationHelper.ProcessAdvanceOnRead();
                    SerializationHelper.ProcessEndElement();
                }

                #endregion

                public override string ToString() {
                    return $"Filter: '{Filter}', Is Regex: {IsRegex}, Filter Type: {FilterType}";
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
                        SerializationHelper.ProcessAttributeString("Name", s => Name = s, () => Name);
                        SerializationHelper.ProcessAttributeString("IsStrictCheck", s => IsStrictCheck= Convert.ToBoolean(s), () => Convert.ToString(IsStrictCheck));
                    }
                    SerializationHelper.ProcessAdvanceOnRead();
                    SerializationHelper.ProcessEndElement();
                }

                #endregion

                public override string ToString() {
                    return $"Name: '{Name}', Strict Check: {IsStrictCheck}";
                }
            }

            #endregion
        }

        public class InjectedMethod : SimpleXmlSerializable {
            public string AssemblyPath { get; private set; }
            public string MethodFullName { get; private set; }
            public MethodInjectionPosition InjectionPosition { get; private set; } = MethodInjectionPosition.InjecteeMethodStart;

            public InjectedMethod() {
            }

            public InjectedMethod(string assemblyPath, string methodFullName) {
                AssemblyPath = assemblyPath;
                MethodFullName = methodFullName;
            }

            #region Serialization

            public override void Serialize() {
                base.Serialize();

                SerializationHelper.ProcessStartElement("InjectedMethod");
                {
                    SerializationHelper.ProcessAttributeString("AssemblyPath", s => AssemblyPath = s, () => AssemblyPath);
                    SerializationHelper.ProcessAttributeString("MethodFullName", s => MethodFullName = s, () => MethodFullName);
                    SerializationHelper.ProcessEnumAttribute("InjectionPosition", s => InjectionPosition = s, () => InjectionPosition);
                }
                SerializationHelper.ProcessAdvanceOnRead();
                SerializationHelper.ProcessEndElement();
            }

            #endregion

            public override string ToString() {
                return $"Assembly Path: '{AssemblyPath}', Method Full Name: '{MethodFullName}', Injection Position: {InjectionPosition}";
            }

            public enum MethodInjectionPosition {
                InjecteeMethodStart,
                InjecteeMethodReturn
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
                    SerializationHelper.ProcessAttributeString("Path", s => Path = s, () => Path);
                }
                SerializationHelper.ProcessAdvanceOnRead();
                SerializationHelper.ProcessEndElement();
            }

            #endregion

            public override string ToString() {
                return $"Path: '{Path}'";
            }
        }
    }

}