// Assets/Framework/Runtime/Utils/EventBusUtil.cs
using System;
using System.Reflection;
using UnityEngine;

namespace XSkillSystem
{
    static class EventBusUtil
    {
        // 尝试订阅 EventBus 上的事件，优先寻找： Subscribe<T>(Action<T>) 返回 IDisposable / 返回 int
        // 返回 IDisposable（若返回 int，会封装一个 IDisposable 去调用 Unsubscribe(handle)）
        public static IDisposable SubscribeTo<T>(object busObj, Action<T> cb, out int outHandle)
        {
            outHandle = -1;
            if (busObj == null) return null;

            var busType = busObj.GetType();

            // 1) Find Subscribe<T>(Action<T>) that returns IDisposable
            var methods = busType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var m in methods)
            {
                if (m.Name != "Subscribe") continue;
                var ps = m.GetParameters();
                if (ps.Length != 1) continue;
                var pType = ps[0].ParameterType;
                if (!pType.IsGenericType) continue;
                if (!pType.GetGenericTypeDefinition().Equals(typeof(Action<>))) continue;
                // check generic arg
                var paramArg = pType.GetGenericArguments()[0];
                if (paramArg != typeof(T)) continue;

                // found a Subscribe method; try invoke generic
                try
                {
                    var gen = m;
                    var ret = gen.Invoke(busObj, new object[] { cb });
                    if (ret is IDisposable d) return d;
                    if (ret is int handle)
                    {
                        outHandle = handle;
                        return new UnsubscriberByHandle(busObj, handle);
                    }
                }
                catch { /* ignore */ }
            }

            // 2) fallback: look for Subscribe<T>(Action<T>) returning void (no handle) -> cannot unsubscribe, so just call and return null-disposable
            try
            {
                // var mi = busType.GetMethod("Subscribe", BindingFlags.Instance | BindingFlags.Public);
                // Type[] paramTypes = new Type[] { typeof(Action<>) };
                // MethodInfo mi = busType.GetMethod("Subscribe", BindingFlags.Instance | BindingFlags.Public, null, paramTypes, null);
                // Type[] paramTypes = new Type[] { typeof(Action<>), typeof(Predicate<>) };
                // MethodInfo mi = busType.GetMethod("Subscribe", BindingFlags.Instance | BindingFlags.Public, null, paramTypes, null);
                
                
                MethodInfo[] allMethods = busType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                MethodInfo mi = null;

                foreach (var method in allMethods)
                {
                    if (method.Name == "Subscribe" && method.IsGenericMethod)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length == 1 && 
                            parameters[0].ParameterType.IsGenericType && 
                            parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Action<>))
                        {
                            mi = method;  // 不带filter的
                            break;
                        }
                        // 对于带filter的：if (parameters.Length == 2 && parameters[0]... == typeof(Action<>) && parameters[1]... == typeof(Predicate<>))
                    }
                }
                
                if (mi != null)
                {
                    // Use generic method MakeGenericMethod
                    var gm = mi.MakeGenericMethod(typeof(T));
                    gm.Invoke(busObj, new object[] { cb });
                    return null; // cannot unsubscribe
                }
            }
            catch(Exception e)
            {
                /* ignore */ 
                Debug.LogWarning($"Can't subscribe to {busType}, error: {e.Message}");
            }

            return null;
        }

        sealed class UnsubscriberByHandle : IDisposable
        {
            readonly object _bus;
            readonly int _handle;
            readonly MethodInfo _miUnsub;

            public UnsubscriberByHandle(object bus, int handle)
            {
                _bus = bus;
                _handle = handle;
                // find Unsubscribe(int)
                _miUnsub = _bus.GetType().GetMethod("Unsubscribe", new Type[] { typeof(int) });
            }

            public void Dispose()
            {
                try
                {
                    _miUnsub?.Invoke(_bus, new object[] { _handle });
                }
                catch { }
            }
        }
    }
}
