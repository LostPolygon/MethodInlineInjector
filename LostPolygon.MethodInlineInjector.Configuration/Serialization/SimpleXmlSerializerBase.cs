using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Contracts;

namespace LostPolygon.MethodInlineInjector.Serialization {
    public abstract class SimpleXmlSerializerBase {
        public XmlDocument Document { get; }
        public XmlElement CurrentXmlElement { get; protected set; }
        public bool IsDeserializing { get; }

        protected SimpleXmlSerializerBase(
            bool isDeserealizing,
            XmlDocument xmlDocument,
            XmlElement currentXmlElement) {
            IsDeserializing  = isDeserealizing;
            Document = xmlDocument ?? throw new ArgumentNullException(nameof(xmlDocument));
            CurrentXmlElement = currentXmlElement;
        }

        protected abstract SimpleXmlSerializerBase CloneSerializer(object serializedObject);

        public abstract bool ProcessAttributeString(string name, Action<string> readAction, Func<string> writeFunc);
        public abstract bool ProcessStartElement(string name, string prefix = null, string namespaceUri = null);
        public abstract void ProcessEndElement();
        public abstract void ProcessAdvanceOnRead();

        public abstract void ProcessCollection<T>(
            ICollection<T> collection,
            Func<SimpleXmlSerializerBase, T> createItemFunc = null)
            where T : class;

        public abstract void ProcessCollectionAsReadOnly<T>(
            Action<ReadOnlyCollection<T>> collectionSetAction,
            Func<ReadOnlyCollection<T>> collectionGetFunc,
            Func<SimpleXmlSerializerBase, T> createItemFunc = null)
            where T : class;

        public abstract bool ProcessEnumAttribute<T>(string name, Action<T> readAction, Func<T> writeFunc)
            where T : struct, IConvertible;

        public abstract void ProcessFlagsEnumAttributes<T>(T defaultValue, Action<T> readAction, Func<T> writeFunc)
            where T : struct, IConvertible;

        public abstract void ProcessWhileNotElementEnd(Action action);

        public virtual void ProcessWithFlags(SimpleXmlSerializerFlags flags, Action action) {
            action();
        }

        public virtual T CreateByXmlRootName<T>(string name, params Type[] types)
            where T : class {
            foreach (Type type in types) {
                if (GetXmlRootName(type) == name) {
                    T value = (T) CloneSerializerAndInvokeSerializationMethod(type, this);
                    return value;
                }
            }

            throw new NotSupportedException($"Unknown element name {name}");
        }

        public virtual T CreateByKnownInheritors<T>(string name, SimpleXmlSerializerBase serializer = null)
            where T : class {
            IEnumerable<Type> knownInheritors =
                Attribute
                    .GetCustomAttributes(typeof(T), typeof(KnownInheritorsAttribute))
                    .Cast<KnownInheritorsAttribute>()
                    .SelectMany(attribute => attribute.InheritorTypes)
                    .Distinct();

            bool isEmpty = true;
            foreach (Type type in knownInheritors) {
                isEmpty = false;
                if (GetXmlRootName(type) == name) {
                    serializer = serializer ?? CloneSerializer(this);
                    T value = (T) InvokeSerializationMethod(type, null, serializer);
                    return value;
                }
            }

            if (isEmpty)
                throw new InvalidOperationException($"Type {typeof(T)} has no {nameof(KnownInheritorsAttribute)} attached");

            throw new NotSupportedException($"Unknown element name {name}");
        }

        [DebuggerStepThrough]
        public virtual string GetXmlRootName(Type type) {
            XmlRootAttribute xmlRootAttribute = (XmlRootAttribute) Attribute.GetCustomAttribute(type, typeof(XmlRootAttribute));
            if (xmlRootAttribute == null)
                throw new Exception($"No {nameof(XmlRootAttribute)} defined on {type.Name}");

            return xmlRootAttribute.ElementName;
        }

        public string GetXmlRootName<T>() {
            return GetXmlRootName(typeof(T));
        }

        protected T CloneSerializerAndInvokeSerializationMethod<T>() where T : class {
            return CloneSerializerAndInvokeSerializationMethod<T>(this);
        }

        protected object CloneSerializerAndInvokeSerializationMethod(Type serializedObjectType, SimpleXmlSerializerBase serializer)  {
            SimpleXmlSerializerBase clonedSerializer = CloneSerializer(serializer);
            return InvokeSerializationMethod(serializedObjectType, null, clonedSerializer);
        }

        protected T CloneSerializerAndInvokeSerializationMethod<T>(SimpleXmlSerializerBase serializer) where T : class {
            SimpleXmlSerializerBase clonedSerializer = CloneSerializer(serializer);
            return InvokeSerializationMethod<T>(null, clonedSerializer);
        }

        protected T CloneSerializerAndInvokeSerializationMethod<T>(T serializedObject) {
            return CloneSerializerAndInvokeSerializationMethod(serializedObject, this);
        }

        protected T CloneSerializerAndInvokeSerializationMethod<T>(T serializedObject, SimpleXmlSerializerBase serializer) {
            SimpleXmlSerializerBase clonedSerializer = CloneSerializer(serializer);
            return InvokeSerializationMethod<T>(serializedObject, clonedSerializer);
        }

        public static T InvokeSerializationMethod<T>(T serializedObject, SimpleXmlSerializerBase serializer) {
            return (T) InvokeSerializationMethod(typeof(T), serializedObject, serializer);
        }

        public static object InvokeSerializationMethod(Type serializedObjectType, object serializedObject, SimpleXmlSerializerBase serializer) {
            MethodInfo serializationMethod = SerializationMethodProvider.GetSerializationMethod(serializedObjectType, serializedObject);

            return serializationMethod.Invoke(null, new[] { serializedObject, serializer });
        }

        private static class SerializationMethodProvider {
            private static readonly Dictionary<Type, MethodInfo> _typeToSerializationMethodMap = new Dictionary<Type, MethodInfo>();

            public static MethodInfo GetSerializationMethod(Type serializedObjectType, object serializedObject) {
                serializedObjectType = serializedObject?.GetType() ?? serializedObjectType;
                MethodInfo serializationMethod;
                if (!_typeToSerializationMethodMap.TryGetValue(serializedObjectType, out serializationMethod)) {
                    serializationMethod =
                        serializedObjectType
                            .GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(method => Attribute.IsDefined(method, typeof(SerializationMethodAttribute)));

                    ValidateSerializationMethod(serializedObjectType, serializationMethod);

                    _typeToSerializationMethodMap.Add(serializedObjectType, serializationMethod);
                }

                return serializationMethod;
            }

            private static void ValidateSerializationMethod(Type serializedObjectType, MethodInfo serializationMethod) {
                if (serializationMethod == null)
                    throw new InvalidOperationException(
                        $"Type '{serializedObjectType.FullName}' must have " +
                        $"a public static method marked with [{nameof(SerializationMethodAttribute)}]"
                    );

                ParameterInfo[] parameterInfos = serializationMethod.GetParameters();
                if (serializationMethod.ReturnType != serializedObjectType ||
                    serializationMethod.IsGenericMethod ||
                    parameterInfos.Length != 2 ||
                    parameterInfos[0].ParameterType != serializedObjectType ||
                    parameterInfos[1].ParameterType != typeof(SimpleXmlSerializerBase))
                    throw new InvalidOperationException(
                        $"'{serializationMethod.DeclaringType.FullName}.{serializationMethod.Name}' method signature must be " +
                        $"'{serializedObjectType.FullName} {serializationMethod.Name}(" +
                        $"{serializedObjectType.FullName} instance, {nameof(SimpleXmlSerializerBase)} serializer)"
                        );
            }
        }
    }
}