using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace KellCommons.Services
{
    //事件通知服务用于解决多个应用程序之间的事件发布与预定的问题。在.NET平台上，跨应用程序的事件发布/预定通常以Remoting作为底层的通信基础，在此基础之上，事件通知服务使用中介者模式来简化跨应用程序的事件通知问题。
    //事件服务器EventServer和事件客户端EventClient。EventServer作为中介者，并作为一个独立的系统，通常可以将其作为windows服务运行。
    public interface IEventNotification
    {
        void SubscribeEvent(string eventName, EventProcessHandler handler);//预定事件
        void UnSubscribeEvent(string eventName, EventProcessHandler handler);//取消预定
        void RaiseEvent(string eventName, object eventContent); //发布事件
    }

    public delegate void EventProcessHandler(string eventName, object eventContent);    //注意，IEventNotification接口中的每个方法的第一个参数是事件名，事件名唯一标志了每个事件，它相当于一个主键。EventClient与包含它的应用程序之间的交互通过本地事件预定/发布来完成，而与EventServer之间的交互则通过remoting完成。其实现如下： 

    public class EventClient : MarshalByRefObject, IEventNotification
    {
        private IEventNotification eventServer = null;
        private Hashtable htableSubscribed = new Hashtable(); //eventName -- Delegate(是一个链表)

        public EventClient(string eventServerUri)
        {
            TcpChannel theChannel = new TcpChannel(0);
            ChannelServices.RegisterChannel(theChannel, false);

            this.eventServer = (IEventNotification)Activator.GetObject(typeof(IEventNotification), eventServerUri);
        }

        public override object InitializeLifetimeService()
        {
            //Remoting对象 无限生存期
            return null;
        }

        #region IEventNotification 成员
        //handler是本地委托
        public void SubscribeEvent(string eventName, EventProcessHandler handler)
        {
            lock (this)
            {
                Delegate handlerList = (Delegate)this.htableSubscribed[eventName];
                if (handlerList == null)
                {
                    this.htableSubscribed.Add(eventName, handler);
                    this.eventServer.SubscribeEvent(eventName, new EventProcessHandler(this.OnRemoteEventHappen));
                    return;
                }

                handlerList = Delegate.Combine(handlerList, handler);
                this.htableSubscribed[eventName] = handlerList;
            }
        }

        public void UnSubscribeEvent(string eventName, EventProcessHandler handler)
        {
            lock (this)
            {
                Delegate handlerList = (Delegate)this.htableSubscribed[eventName];

                if (handlerList != null)
                {
                    handlerList = Delegate.Remove(handlerList, handler);
                    this.htableSubscribed[eventName] = handlerList;
                }
            }
        }

        public void RaiseEvent(string eventName, object eventContent)
        {
            this.eventServer.RaiseEvent(eventName, eventContent);
        }
        #endregion

        #region OnRemoteEventHappen
        /// <summary>
        /// 当EventServer上有事件触发时，EventServer会转换为客户端，而EventClient变成远程对象，
        /// 该方法会被远程调用。所以必须为public
        /// </summary>        
        public void OnRemoteEventHappen(string eventName, object eventContent)
        {
            lock (this)
            {
                Delegate handlerList = (Delegate)this.htableSubscribed[eventName];
                if (handlerList == null)
                {
                    return;
                }

                object[] args = { eventName, eventContent };
                foreach (Delegate dg in handlerList.GetInvocationList())
                {
                    try
                    {
                        dg.DynamicInvoke(args);
                    }
                    catch (Exception ee)
                    {
                        ee = ee;
                    }
                }
            }
        }
        #endregion
    }

    public class EventServer : MarshalByRefObject, IEventNotification
    {
        //htableSubscribed内部每项的Delegate链表中每一个委托都是透明代理
        private Hashtable htableSubscribed = new Hashtable(); //eventName -- Delegate(是一个链表)
        public EventServer()
        {
        }

        public override object InitializeLifetimeService()
        {
            //Remoting对象 无限生存期
            return null;
        }


        #region IEventNotification 成员
        //handler是一个透明代理，指向EventClient.OnRemoteEventHappen委托
        public void SubscribeEvent(string eventName, EventProcessHandler handler)
        {
            lock (this)
            {
                Delegate handlerList = (Delegate)this.htableSubscribed[eventName];

                if (handlerList == null)
                {
                    this.htableSubscribed.Add(eventName, handler);
                    return;
                }

                handlerList = Delegate.Combine(handlerList, handler);
                this.htableSubscribed[eventName] = handlerList;
            }
        }

        public void UnSubscribeEvent(string eventName, EventProcessHandler handler)
        {
            lock (this)
            {
                Delegate handlerList = (Delegate)this.htableSubscribed[eventName];

                if (handlerList != null)
                {
                    handlerList = Delegate.Remove(handlerList, handler);
                    this.htableSubscribed[eventName] = handlerList;
                }
            }
        }

        public void RaiseEvent(string eventName, object eventContent)
        {
            lock (this)
            {
                Delegate handlerList = (Delegate)this.htableSubscribed[eventName];
                if (handlerList == null)
                {
                    return;
                }

                object[] args = { eventName, eventContent };
                IEnumerator enumerator = handlerList.GetInvocationList().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Delegate handler = (Delegate)enumerator.Current;
                    try
                    {
                        handler.DynamicInvoke(args);
                    }
                    catch (Exception ee) //也可重试
                    {
                        ee = ee;
                        handlerList = Delegate.Remove(handlerList, handler);
                        this.htableSubscribed[eventName] = handlerList;
                    }
                }
            }
        }

        #endregion
    }
}