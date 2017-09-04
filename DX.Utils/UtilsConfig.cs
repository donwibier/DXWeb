using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DX.Utils
{
    public sealed class UtilsConfig
    {
        static UtilsConfigSection _Config;
        
        #region Singleton Initialization

        private readonly static object l = new object();
        static UtilsConfig()
        {
            lock (l)
            {
                Trace.Write("UtilsConfig singelton constructor called in l");
                try
                {
                    _Config = ConfigurationManager.GetSection("DXWeb/Utils") as UtilsConfigSection;
                }
                catch { }
                finally
                {
                    if (_Config == null)
                    {
                        Trace.Write("UtilsConfig could not find the DXWeb/Utils config section, initializing default one.");
                        _Config = new UtilsConfigSection();
                    }
                }

            }
        }
        #endregion
        public static UtilsConfigSection Current { get { return _Config; } }

        #region Embedded config class
        public sealed class UtilsConfigSection : ConfigurationSection
        {
            [ConfigurationProperty("LoggerType", DefaultValue = null, IsDefaultCollection = false, IsRequired = false)]
            public Type LoggerType
            {
                get { return (Type)base["LoggerType"]; }
                set { base["LoggerType"] = value; }
            }
        }
        #endregion
    }
}
