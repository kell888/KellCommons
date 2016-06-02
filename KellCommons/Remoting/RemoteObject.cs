using System;
using System.Reflection;

namespace KellCommons.Remoting
{
    public class RemoteObject : MarshalByRefObject
    {
        public object Operation(Delegate dlgt, params object[] args)
        {
            return dlgt.Method.Invoke(dlgt.Target, args);
        }
        public object Operation(Delegate dlgt, object instance, params object[] args)
        {
            return dlgt.Method.Invoke(instance, args);
        }
        public object Operation(MethodInfo method, object instance, params object[] args)
        {
            return method.Invoke(instance, args);
        }
        public double Add(params double[] datas)
        {
            double sum =0;
            foreach (double d in datas)
            {
                sum += d;
            }
            return sum;
        }
    }
}
