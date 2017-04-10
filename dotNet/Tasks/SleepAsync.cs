public Task SleepAsync(int millisecondsTimeout)
{
	TaskCompletionSource<bool> tcs = null;
	var t = new Timer(delegate { tcs.TrySetResult(true); }, null, -1, -1);
	tcs = new TaskCompletionSource<bool>(t);
	t.Change(millisecondsTimeout, -1);
	return tcs.Task;
}