using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DNA.Threading
{
	public class TaskScheduler
	{
		private class ScheduledTask : Task
		{
			private ParameterizedThreadStart _paramCallback;

			private ThreadStart _callback;

			private object _state;

			public ScheduledTask(ParameterizedThreadStart callback, object state)
			{
				_paramCallback = callback;
				_state = state;
			}

			public ScheduledTask(ThreadStart callback)
			{
				_callback = callback;
			}

			public bool DoWork()
			{
				base.Status = TaskStatus.InProcess;
				if (Debugger.IsAttached)
				{
					if (_paramCallback == null)
					{
						_callback();
					}
					else
					{
						_paramCallback(_state);
					}
				}
				else
				{
					try
					{
						if (_paramCallback == null)
						{
							_callback();
						}
						else
						{
							_paramCallback(_state);
						}
					}
					catch (Exception exception)
					{
						base.Exception = exception;
						base.Status = TaskStatus.Failed;
						return false;
					}
				}
				base.Status = TaskStatus.Compelete;
				return true;
			}
		}

		public class ExceptionEventArgs : EventArgs
		{
			public Exception InnerException;

			public ExceptionEventArgs(Exception e)
			{
				InnerException = e;
			}
		}

		private Queue<ScheduledTask> _taskQueue = new Queue<ScheduledTask>();

		private Thread _queueWorkerThread;

		public int[] ProcessorThreads = new int[3] { 3, 4, 5 };

		private bool _runThread = true;

		private AutoResetEvent _event = new AutoResetEvent(false);

		public bool ThreadRunning
		{
			get
			{
				if (_queueWorkerThread != null)
				{
					return _runThread;
				}
				return false;
			}
		}

		public event EventHandler<ExceptionEventArgs> ThreadException;

		public void Exit()
		{
			if (ThreadRunning)
			{
				Thread queueWorkerThread = _queueWorkerThread;
				_runThread = false;
				_event.Set();
				if (Thread.CurrentThread != queueWorkerThread)
				{
					queueWorkerThread.Join();
				}
			}
		}

		private void ExecutionThread(object state)
		{
			Thread.CurrentThread.SetProcessorAffinity(ProcessorThreads[0]);
			ScheduledTask scheduledTask = (ScheduledTask)state;
			if (!scheduledTask.DoWork() && this.ThreadException != null)
			{
				this.ThreadException(this, new ExceptionEventArgs(scheduledTask.Exception));
			}
		}

		private void StartWorkerQueue()
		{
			if (_queueWorkerThread == null)
			{
				_queueWorkerThread = new Thread(QueueWorker);
				_queueWorkerThread.Name = "TaskSchedulerQueueWorker";
				_queueWorkerThread.IsBackground = true;
				_queueWorkerThread.Start();
			}
			_event.Set();
		}

		private void QueueWorker()
		{
			while (_runThread)
			{
				_event.WaitOne();
				while (_taskQueue.Count > 0)
				{
					ScheduledTask state;
					lock (_taskQueue)
					{
						state = _taskQueue.Dequeue();
					}
					ExecutionThread(state);
				}
			}
			_queueWorkerThread = null;
			_runThread = true;
		}

		public Task QueueUserWorkItem(ThreadStart callBack)
		{
			lock (_taskQueue)
			{
				ScheduledTask scheduledTask = new ScheduledTask(callBack);
				_taskQueue.Enqueue(scheduledTask);
				StartWorkerQueue();
				return scheduledTask;
			}
		}

		public Task QueueUserWorkItem(ParameterizedThreadStart callBack, object state)
		{
			lock (_taskQueue)
			{
				ScheduledTask scheduledTask = new ScheduledTask(callBack, state);
				_taskQueue.Enqueue(scheduledTask);
				StartWorkerQueue();
				return scheduledTask;
			}
		}

		public Task DoUserWorkItem(ThreadStart callBack)
		{
			ScheduledTask scheduledTask = new ScheduledTask(callBack);
			ThreadPool.QueueUserWorkItem(ExecutionThread, scheduledTask);
			return scheduledTask;
		}

		public Task DoUserWorkItem(ParameterizedThreadStart callBack, object state)
		{
			ScheduledTask scheduledTask = new ScheduledTask(callBack, state);
			ThreadPool.QueueUserWorkItem(ExecutionThread, scheduledTask);
			return scheduledTask;
		}
	}
}
