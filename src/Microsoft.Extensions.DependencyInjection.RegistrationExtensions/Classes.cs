using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.RegistrationExtensions
{
    public static class Classes
    {
        public static ThisAssemblyReference FromAssembly(object o)
        {
            return new ThisAssemblyReference(o.GetType());
        }

        public class ThisAssemblyReference
        {
            private readonly Type _t;

            public ThisAssemblyReference(Type t)
            {
                _t = t;
            }

            public Services<T> BaseOn<T>()
            {
                if (!typeof(T).GetTypeInfo().IsInterface && !typeof(T).GetTypeInfo().IsClass)
                {
                    throw new ArgumentException("T must be a interface");
                }

                return new Services<T>(_t);
            }

            public Services BaseOn(Type baseType)
            {
                if (!baseType.GetTypeInfo().IsInterface && !baseType.GetTypeInfo().IsClass)
                {
                    throw new ArgumentException("T must be a interface");
                }

                return new Services(typeInAssembly: _t, baseType: baseType);
            }
        }

        public class Services
        {
            private readonly ServiceReferenceCreator _serviceReferenceCreator;

            public Services(Type typeInAssembly, Type baseType)
            {
                _serviceReferenceCreator = new ServiceReferenceCreator(typeInAssembly, baseType);
            }

            public List<ServiceReference> WithDefaultInterface()
            {
                return _serviceReferenceCreator.WithDefaultInterface();
            }

            public List<ServiceReference> WithSelf()
            {
                return _serviceReferenceCreator.WithSelf();
            }

            public List<ServiceReference> WithDefaultInterfaceAndSelf()
            {
                return _serviceReferenceCreator.WithDefaultInterfaceAndSelf();
            }
        }

        public class ServiceReferenceCreator
        {
            private readonly Type _typeInAssembly;
            private readonly Type _baseType;

            public ServiceReferenceCreator(Type typeInAssembly, Type baseType)
            {
                _typeInAssembly = typeInAssembly;
                _baseType = baseType;
            }

            public List<ServiceReference> WithDefaultInterface()
            {
                var serviceWithImpl = _typeInAssembly.Assembly
                    .ExportedTypes
                    .Where(IsSubClass)
                    .Where(t => t.IsClass)
                    .Where(t => !t.IsAbstract)
                    .Select(t => new ServiceReference() { Implementation = t, Service = GetDefaultInterface(t) })
                    .ToList();

                return serviceWithImpl;
            }

            public List<ServiceReference> WithBaseClass()
            {
                var serviceWithImpl = _typeInAssembly.Assembly
                    .ExportedTypes
                    .Where(IsSubClass)
                    .Where(t => t.IsClass)
                    .Where(t => !t.IsAbstract)
                    .Select(t => new ServiceReference() { Implementation = t, Service = GetBaseClass(t) })
                    .ToList();

                return serviceWithImpl;
            }

            public List<ServiceReference> WithSelf()
            {
                var serviceWithImpl = _typeInAssembly.Assembly
                    .ExportedTypes
                    .Where(IsSubClass)
                    .Where(t => t.IsClass)
                    .Where(t => !t.IsAbstract)
                    .Select(t => new ServiceReference() { Implementation = t, Service = t })
                    .ToList();

                return serviceWithImpl;
            }

            public List<ServiceReference> WithDefaultInterfaceAndSelf()
            {
                var list1 = WithDefaultInterface();
                var list2 = WithSelf();

                list1.AddRange(list2);
                return list1;
            }

            public List<ServiceReference> WithBaseClassAndSelf()
            {
                var list1 = WithBaseClass();
                var list2 = WithSelf();

                list1.AddRange(list2);
                return list1;
            }

            private Type GetDefaultInterface(Type c)
            {
                var i = c.GetInterfaces().First();

                return i;
            }

            private Type GetBaseClass(Type c)
            {
                var b = c.BaseType;

                return b;
            }

            private bool IsSubClass(Type t)
            {
                if (_baseType.IsGenericTypeDefinition)
                {
                    return t.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition())
                        .Any(d => d == _baseType);
                }

                return _baseType.IsAssignableFrom(t);
            }
        }

        public class Services<T>
        {
            private readonly ServiceReferenceCreator _serviceReferenceCreator;

            public Services(Type t)
            {
                _serviceReferenceCreator = new ServiceReferenceCreator(typeInAssembly: t, baseType: typeof(T));
            }

            public List<ServiceReference> WithDefaultInterface()
            {
                return _serviceReferenceCreator.WithDefaultInterface();
            }

            public List<ServiceReference> WithSelf()
            {
                return _serviceReferenceCreator.WithSelf();
            }

            public List<ServiceReference> WithDefaultInterfaceAndSelf()
            {
                return _serviceReferenceCreator.WithDefaultInterfaceAndSelf();
            }

            public List<ServiceReference> WithBaseClass()
            {
                return _serviceReferenceCreator.WithBaseClass();
            }

            public List<ServiceReference> WithBaseClassAndSelf()
            {
                return _serviceReferenceCreator.WithBaseClassAndSelf();
            }
        }
    }
}