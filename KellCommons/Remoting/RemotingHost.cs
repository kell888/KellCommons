using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Ipc;

namespace KellCommons.Remoting
{
    public class RemotingHost
    {
        public static void CreateAndRegisterChannel(Type type)
        {
            IChannel channel = new TcpChannel(int.Parse(System.Configuration.ConfigurationManager.AppSettings["ChannelPort"]));
            string channelType = System.Configuration.ConfigurationManager.AppSettings["ChannelType"];
            if (channelType.ToLower() == "http")
                channel = new HttpChannel(int.Parse(System.Configuration.ConfigurationManager.AppSettings["ChannelPort"]));
            else if (channelType.ToLower() == "ipc")
                channel = new IpcChannel(System.Configuration.ConfigurationManager.AppSettings["IpcChannelPortName"]);
            bool ensureSecurity = false;
            string security = System.Configuration.ConfigurationManager.AppSettings["EnsureSecurity"];
            if (security == "1")
                ensureSecurity = true;
            ChannelServices.RegisterChannel(channel, ensureSecurity);
            string registerMode = System.Configuration.ConfigurationManager.AppSettings["WellKnownObjectMode"];
            WellKnownObjectMode wkom = WellKnownObjectMode.Singleton;
            if (registerMode.ToLower() == "singlecall")
                wkom = WellKnownObjectMode.SingleCall;
            string objectClass = System.Configuration.ConfigurationManager.AppSettings["ObjectClass"];
            string objectUri = "RemoteObject";
            if (!string.IsNullOrEmpty(objectClass))
                objectUri = objectClass;
            RemotingConfiguration.RegisterWellKnownServiceType(type, objectUri, wkom);
        }
    }
}
