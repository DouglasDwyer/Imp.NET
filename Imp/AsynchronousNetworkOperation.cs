using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DouglasDwyer.Imp
{
    public abstract class AsynchronousNetworkOperation
    {
        public ushort OperationID { get; private set; }
        public Action<AsynchronousNetworkOperation> OnCompletion { get; set; }

        public AsynchronousNetworkOperation(ushort operationID, Action<AsynchronousNetworkOperation> onCompletion)
        {
            OperationID = operationID;
            OnCompletion = onCompletion;
        }

        public abstract void SetResult(object result, TaskScheduler scheduler);
        public abstract void SetException(Exception result, TaskScheduler scheduler);
    }

    public class AsynchronousNetworkOperation<T> : AsynchronousNetworkOperation
    {
        public Task<T> Operation => OperationSource.Task;
        private TaskCompletionSource<T> OperationSource { get; set; }

        public AsynchronousNetworkOperation(ushort operationID, Action<AsynchronousNetworkOperation> onCompletion) : base(operationID, onCompletion)
        {
            OperationSource = new TaskCompletionSource<T>();
        }

        public override void SetResult(object result, TaskScheduler scheduler)
        {
            Task.Factory.StartNew(() => OperationSource.SetResult((T)result), CancellationToken.None, TaskCreationOptions.None, scheduler);
            OnCompletion(this);
        }

        public override void SetException(Exception result, TaskScheduler scheduler)
        {
            Task.Factory.StartNew(() => OperationSource.SetException(result), CancellationToken.None, TaskCreationOptions.None, scheduler);
            OnCompletion(this);
        }
    }
}
