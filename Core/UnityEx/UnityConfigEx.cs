using System;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace ViCore.UnityEx
{
    public class UnityConfigEx
    {
        static object _lockobj = new object();
        private static IUnityContainer _container;
        private static IUnityContainer Container
        {
            get
            {
                if (_container == null)
                {
                    _container = new UnityContainer();
                    UnityConfigurationSection section = ConfigurationManager.GetSection("unity") as UnityConfigurationSection;
                    foreach (var item in section.Containers)
                    {
                        section.Configure(_container, item.Name);
                    }
                }
                return _container;
            }
        }

        public static T GetService<T>(string name = null)
        {
            lock (_lockobj)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return Container.Resolve<T>();
                }
                return Container.Resolve<T>(name);
            }
        }

        public static object Resolve(Type type, string name = null, params ResolverOverride[] overrides)
        {
            lock (_lockobj)
            {
                return Container.Resolve(type, name, overrides);
            }
        }
    }
}
