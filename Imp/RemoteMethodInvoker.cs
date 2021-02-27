using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DouglasDwyer.Imp
{
    public class RemoteMethodInvoker
    {
        public MethodInfo Method { get; }
        protected Func<TaskScheduler, object, object[], Type[], Task<object>> GetResult { get; }

        public RemoteMethodInvoker(MethodInfo method)
        {
            Method = method;
            if (method.ReturnType == typeof(Task))
            {
                GetResult = async (scheduler, x, y, gens) => {
                    MethodInfo toInvoke;
                    if(gens is null)
                    {
                        toInvoke = Method;
                    }
                    else
                    {
                        toInvoke = Method.MakeGenericMethod(gens);
                    }
                    await Task.Factory.StartNew(() => (Task)toInvoke.Invoke(x, y), CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();
                    return null;
                };
            }
            else if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                GetResult = async (scheduler, x, y, gens) => {
                    MethodInfo toInvoke;
                    if (gens is null)
                    {
                        toInvoke = Method;
                    }
                    else
                    {
                        toInvoke = Method.MakeGenericMethod(gens);
                    }
                    PropertyInfo resultInfo = toInvoke.ReturnType.GetProperty(nameof(Task<object>.Result));
                    return resultInfo.GetValue(await Task.Factory.StartNew(() => toInvoke.Invoke(x, y), CancellationToken.None, TaskCreationOptions.None, scheduler));
                };
            }
            else
            {
                GetResult = (scheduler, x, y, gens) =>
                {
                    MethodInfo toInvoke;
                    if (gens is null)
                    {
                        toInvoke = Method;
                    }
                    else
                    {
                        toInvoke = Method.MakeGenericMethod(gens);
                    }
                    return Task.Factory.StartNew(() => toInvoke.Invoke(x, y), CancellationToken.None, TaskCreationOptions.None, scheduler);
                };
            }
        }

        public virtual Task<object> Invoke(IImpClient client, ImpClient caller, object target, object[] args, Type[] genericArguments)
        {
            return GetResult(caller.RemoteTaskScheduler, target, args, genericArguments);
        }
    }
}
