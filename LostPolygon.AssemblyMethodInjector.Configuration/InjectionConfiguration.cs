using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LostPolygon.AssemblyMethodInjector {
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
                        ProcessCollection(InjectedMethods);
                    }
                    SerializationHelper.ProcessEndElement();
                }

                if (SerializationHelper.ProcessStartElement("InjecteeAssemblies")) {
                    SerializationHelper.ProcessAdvanceOnRead();
                    {
                        ProcessCollection(InjecteeAssemblies);
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
                        ProcessCollection(MemberReferenceWhitelist, () => {
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
                        ProcessCollection(AssemblyWhitelist, () => {
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

                public FilterTypeFlags FilterFlags { get; private set; } = kDefaultFilterTypeFlags;
                public string Filter { get; private set; }
                public bool IsRegex { get; private set; }

                public MemberReferenceWhitelistFilter() {
                }

                public MemberReferenceWhitelistFilter(FilterTypeFlags filterFlags, string filter, bool isRegex) {
                    FilterFlags = filterFlags;
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
                        FilterTypeFlags[] filterTypeFlagsValues = (FilterTypeFlags[]) Enum.GetValues(typeof(FilterTypeFlags));
                        string[] filterTypeFlagsNames = Enum.GetNames(typeof(FilterTypeFlags));
                        
                        if (SerializationHelper.IsXmlSerializationReading) {
                            FilterTypeFlags resultFlags = kDefaultFilterTypeFlags;
                            for (int i = 0; i < filterTypeFlagsNames.Length; i++) {
                                string filterTypeFlagsName = filterTypeFlagsNames[i];
                                FilterTypeFlags filterTypeFlagsValue = filterTypeFlagsValues[i];

                                bool currentFlagValue = false;
                                if (SerializationHelper.ProcessAttributeString(filterTypeFlagsName, s => currentFlagValue = Convert.ToBoolean(s), null)) {
                                    if (currentFlagValue) {
                                        resultFlags |= filterTypeFlagsValue;
                                    } else {
                                        resultFlags &= ~filterTypeFlagsValue;
                                    }
                                }
                            }
                            FilterFlags = resultFlags;
                        } else {
                            for (int i = 0; i < filterTypeFlagsNames.Length; i++) {
                                string filterTypeFlagsName = filterTypeFlagsNames[i];
                                FilterTypeFlags filterTypeFlagsValue = filterTypeFlagsValues[i];

                                FilterTypeFlags currentFlag = FilterFlags & filterTypeFlagsValue;
                                if (currentFlag != (kDefaultFilterTypeFlags & filterTypeFlagsValue)) {
                                    SerializationHelper.ProcessAttributeString(filterTypeFlagsName, null, () => Convert.ToString(currentFlag != 0));
                                }
                            }
                        }
                    }
                    SerializationHelper.ProcessAdvanceOnRead();
                    SerializationHelper.ProcessEndElement();
                }

                #endregion

                public override string ToString() {
                    return $"Filter: '{Filter}', Is Regex: {IsRegex}, Filter Type: {FilterFlags}";
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
                }
                SerializationHelper.ProcessAdvanceOnRead();
                SerializationHelper.ProcessEndElement();
            }

            #endregion

            public enum MethodInjectionPosition {
                AtMethodStart,
                
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