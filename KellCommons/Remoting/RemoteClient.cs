using System;
using System.Reflection;

namespace KellCommons.Remoting
{
    public class RemoteClient// : MarshalByRefObject
    {
        public static string ServiceURL
        {
            get
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["ServiceURL"];
                if (!url.EndsWith("/"))
                    url += "/";
                return url;
            }
        }
        public static object CallRemoteOperation(Delegate dlgt, params object[] args)
        {
            try
            {
                RemoteObject proxyObject = (RemoteObject)Activator.GetObject(typeof(RemoteObject), ServiceURL + "RemoteObject");
                object obj = proxyObject.Operation(dlgt, args);
                return obj;
            }
            catch (Exception e)
            {
                throw new Exception("调用远程对象时出错：" + Environment.NewLine + e.Message);
            }
        }
        public static object CallRemoteOperation(Delegate dlgt, object instance, params object[] args)
        {
            try
            {
                RemoteObject proxyObject = (RemoteObject)Activator.GetObject(typeof(RemoteObject), ServiceURL + "RemoteObject");
                object obj = proxyObject.Operation(dlgt, instance, args);
                return obj;
            }
            catch (Exception e)
            {
                throw new Exception("调用远程对象时出错：" + Environment.NewLine + e.Message);
            }
        }
        public static object CallRemoteOperation(MethodInfo method, object instance, params object[] args)
        {
            try
            {
                RemoteObject proxyObject = (RemoteObject)Activator.GetObject(typeof(RemoteObject), ServiceURL + "RemoteObject");
                object obj = proxyObject.Operation(method, instance, args);
                return obj;
            }
            catch (Exception e)
            {
                throw new Exception("调用远程对象时出错：" + Environment.NewLine + e.Message);
            }
        }
        public static double Add(params double[] args)
        {
            try
            {
                RemoteObject proxyObject = (RemoteObject)Activator.GetObject(typeof(RemoteObject), ServiceURL + "RemoteObject");
                double obj = proxyObject.Add(args);
                return obj;
            }
            catch (Exception e)
            {
                throw new Exception("调用远程对象时出错：" + Environment.NewLine + e.Message);
            }
        }

        public static object GetRemoteObjectByActivator(Type type, string typeName)
        {
            return Activator.GetObject(type, ServiceURL + typeName);
        }

        public static object GetRemoteObjectByConstructor(Type type)
        {
            return type.GetConstructor(new Type[0] { }).Invoke(new object[0] { });
        }
    }
}
