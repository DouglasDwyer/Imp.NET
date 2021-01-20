using System;
using System.Collections.Generic;
using System.Text;
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

        public abstract void SetResult(object result);
        public abstract void SetException(Exception result);
    }

    public class AsynchronousNetworkOperation<T> : AsynchronousNetworkOperation
    {
        public Task<T> Operation => OperationSource.Task;
        private TaskCompletionSource<T> OperationSource { get; set; }

        public AsynchronousNetworkOperation(ushort operationID, Action<AsynchronousNetworkOperation> onCompletion) : base(operationID, onCompletion)
        {
            OperationSource = new TaskCompletionSource<T>();
        }

        public override void SetResult(object result)
        {
            OperationSource.SetResult((T)result);
            OnCompletion(this);
        }

        public override void SetException(Exception result)
        {
            OperationSource.SetException(result);
            OnCompletion(this);
        }
    }
}
