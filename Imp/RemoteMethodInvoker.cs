using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Allows for customizable method invocation on behalf of a remote <see cref="IImpClient"/>.
    /// </summary>
    public class RemoteMethodInvoker
    {
        /// <summary>
        /// The method that this invoker will invoke.
        /// </summary>
        public MethodInfo Method { get; }
        protected Func<TaskScheduler, object, object[], Type[], Task<object>> GetResult { get; }

        /// <summary>
        /// Creates a new invoker for the specified method. The invoker will examine the method's signature to determine whether the method should execute synchronously or asynchronously.
        /// </summary>
        /// <param name="method">The method that this invoker should call.</param>
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

        /// <summary>
        /// Asynchronously invokes the given method using the <paramref name="caller"/>'s task scheduler, returning a <see cref="Task"/> that represents the state of the operation.
        /// </summary>
        /// <param name="client">The remote client that is calling this method.</param>
        /// <param name="caller">The host client of the remote client.</param>
        /// <param name="target">The object to invoke the method on.</param>
        /// <param name="args">The arguments to provide to the method.</param>
        /// <param name="genericArguments">The generic type arguments to utilize in the method's signature, or null if the method is not generic.</param>
        /// <returns>A <see cref="Task"/> that represents the current state of the method.</returns>
        public virtual Task<object> Invoke(IImpClient client, ImpClient caller, object target, object[] args, Type[] genericArguments)
        {
            return GetResult(caller.RemoteTaskScheduler, target, args, genericArguments);
        }
    }
}
