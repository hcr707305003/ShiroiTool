using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ShiroiTool
{
    internal class DllLoadHelper
    {

        public static void LoadResourceDll()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //获取dll名称
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            //dll名称处理
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            //获取命名空间
            string Namespace = Assembly.GetEntryAssembly().GetTypes()[0].Namespace;
            //加载dll
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            //返回程序集
            return System.Reflection.Assembly.Load(bytes);
        }
    }
}
