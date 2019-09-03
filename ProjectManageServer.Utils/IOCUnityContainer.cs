using Microsoft.Practices.Unity.Configuration;
using System;
using System.Configuration;
using System.IO;
using Unity;

namespace ProjectManageServer.Utils
{
    public class IOCUnityContainer
    {
        private static IUnityContainer container;

        private static UnityConfigurationSection section;

        static IOCUnityContainer()
        {
            ExeConfigurationFileMap exeConfiguration = new ExeConfigurationFileMap();

            exeConfiguration.ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Unity.Config");

            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(exeConfiguration, ConfigurationUserLevel.None);

            section = (UnityConfigurationSection)configuration.GetSection(UnityConfigurationSection.SectionName);

            container = new UnityContainer();
        }

        public static T ResolveContainer<T>(string containerName)
        {
            section.Configure(container, containerName);

            return container.Resolve<T>();
        } 
    }
}
