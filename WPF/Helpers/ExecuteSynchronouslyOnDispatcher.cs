/// <summary>
/// Executes passed task (probably result of async method call) on a Dispatcher as
/// synchronous one, but without Wait() and risk of deadlock, using new dispatcher frame
/// that is finished as a continuation of passed task.
///
/// You should still await the task returned back if you want correct exception propagation.
///
/// Note that you can execute this method only if you're in a WPF dispatcher's context,
/// otherwise it will throw an exception.
/// </summary>
public static Task ExecuteSynchronouslyOnDispatcher(this Task task)
{
    if (!(SynchronizationContext.Current is DispatcherSynchronizationContext))
        throw new InvalidOperationException("This method should be called only from UI thread");
    
    var dispatcherFrame = new DispatcherFrame();
    task.ContinueWith(_ => { dispatcherFrame.Continue = false; });
    Dispatcher.PushFrame(dispatcherFrame);
    return task;
}