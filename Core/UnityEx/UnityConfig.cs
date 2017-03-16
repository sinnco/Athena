using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using System.Reflection;
using Microsoft.Practices.Unity;

namespace ViCore.UnityEx
{
    /// <summary>
    /// 配置文件控制类
    /// </summary>
    public class UnityConfig
    {
        /// <summary>
        /// 检查Unity配置文件是否正确
        /// </summary>
        /// <param name="path">默认配置文件的路径“/config/unity.config”</param>
        public static void Check(string path = @"/config/unity.config")
        {
            UnityContainer ucontainer = new UnityContainer();
            UnityConfigurationSection section = ConfigurationManager.GetSection("unity") as UnityConfigurationSection;
            StringBuilder err = new StringBuilder();
            foreach(var container in section.Containers)
            {
                foreach (var regsiter in container.Registrations)
                {
                    string[] mapto = regsiter.MapToName.Split(',');
                    try
                    {
                        Activator.CreateInstance(mapto[1].Trim(), mapto[0].Trim());
                    }
                    catch (Exception ex)
                    {
                        err.Append(ex.Message + " : " + regsiter.MapToName +"\r\n");
                    }
                }
            }
            foreach (var item in section.Containers)
            {
                try
                {
                    section.Configure(ucontainer, item.Name);
                }
                catch (Exception ex)
                {
                    throw new Exception("Unity配置文件出错，请检查\r\n" + ex.Message +"\r\n" + err.ToString());
                }
            }
        }
    }
}
