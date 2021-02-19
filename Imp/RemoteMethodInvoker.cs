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
        protected Func<TaskScheduler, object, object[], Task<object>> GetResult { get; }

        public RemoteMethodInvoker(MethodInfo method)
        {
            Method = method;
            if (method.ReturnType == typeof(Task))
            {
                GetResult = async (scheduler, x, y) => { await Task.Factory.StartNew(() => (Task)Method.Invoke(x, y), CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap(); return null; };
            }
            else if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                PropertyInfo resultInfo = method.ReturnType.GetProperty(nameof(Task<object>.Result));
                GetResult = async (scheduler, x, y) => resultInfo.GetValue(await Task.Factory.StartNew(() => Method.Invoke(x, y), CancellationToken.None, TaskCreationOptions.None, scheduler));
            }
            else
            {
                GetResult = (scheduler, x, y) => Task.Factory.StartNew(() => Method.Invoke(x, y), CancellationToken.None, TaskCreationOptions.None, scheduler);
            }
        }

        public virtual Task<object> Invoke(IImpClient client, ImpClient caller, object target, object[] args)
        {
            return GetResult(caller.RemoteTaskScheduler, target, args);
        }
    }
}
