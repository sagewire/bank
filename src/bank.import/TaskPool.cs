using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace bank.import
{
    public class TaskPool<T>
    {
        private ConcurrentQueue<T> _jobQueue = new ConcurrentQueue<T>();
        //private Timer _queueTimer = new Timer();
        //private Timer _workerTimer = new Timer();
        private object _refillSync = new object();
        private object _startTaskSync = new object();
        private bool _isRefilling = false;
        private int _inProgress;
        private int _minimumQueueSize = 0;
        private bool _continue = true;
        //private DateTime? _lastWorkerStart = null;
        //private bool _isRampingUp = false;
        private AutoResetEvent _WaitHandle = new AutoResetEvent(false);

        public delegate int RefillQueueHandler(int queueCount);
        public delegate void NextTaskHandler(T task);
        public delegate void QueueEmptyHandler();

        public int MaxWorkers { get; set; }


        public event RefillQueueHandler RefillQueue;
        public event NextTaskHandler NextTask;
        public event QueueEmptyHandler QueueEmpty;

        public void Start(bool async)
        {
            fillQueue();

            if (!async)
            {
                _WaitHandle.WaitOne();
            }
        }

        public void Start()
        {
            Start(true);
        }

        public void Pause()
        {

        }

        public int MinimumQueueSize
        {
            get
            {
                return _minimumQueueSize == 0 ? MaxWorkers * 3 : _minimumQueueSize;
            }
            set
            {
                _minimumQueueSize = value;
            }
        }


        public int InProgress
        {
            get
            {
                return _inProgress;
            }
            private set
            {
                _inProgress = value;
            }
        }

        public int QueueLength
        {
            get
            {
                return _jobQueue.Count;
            }
        }

        private void checkQueue()
        {
            try
            {
                if (_jobQueue.Count <= MinimumQueueSize)
                {
                    fillQueue();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private void fillQueue()
        {
            Task.Factory.StartNew(() => OnRefillQueue());
        }

        private void OnRefillQueue()
        {
            if (RefillQueue == null) return;

            //if we are already refilling, we don't want to do it again
            if (_isRefilling || QueueLength > MinimumQueueSize) return;

            lock (_refillSync)
            {
                if (QueueLength > MinimumQueueSize) return;

                _isRefilling = true;

                try
                {
                    var added = RefillQueue(_jobQueue.Count);

                    if (added == 0)
                    {
                        _continue = false;
                        if (QueueLength == 0)
                        {
                            onQueueEmpty();
                        }
                    }

                    //if (added == 0) Console.WriteLine("Refill failed");

                    //if (added == 0 && QueueLength == 0)
                    //{   //there's nothing to do here
                    //    onQueueEmpty();
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    _isRefilling = false;
                }


            }
        }

        //void startWorker()
        //{
        //    try
        //    {
        //        if (InProgress < MaxWorkers)
        //        {
        //            //Console.WriteLine("inprogress {0} max workers {1}", InProgress, MaxWorkers);
        //            startTaskAsync();
        //        }
        //        else
        //        {
        //            //Console.WriteLine("no more workers left to start. inprogress {0} max workers {1}", InProgress, MaxWorkers);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        public void Enqueue(List<T> tasks)
        {
            foreach (var task in tasks)
            {
                Enqueue(task);
            }
        }

        //private void checkRampUpSpeed()
        //{
        //    if (_lastWorkerStart.HasValue)
        //    {
        //        var elapsed = DateTime.Now - _lastWorkerStart.Value;

        //        var diff = (int)Math.Round(RampUpDelay - elapsed.TotalMilliseconds);

        //        if (diff > 0)
        //        {
        //            _isRampingUp = true;
        //            Console.WriteLine("Ramping up too fast. Slowing down for {0}ms", diff);
        //            Thread.Sleep(diff);
        //            _isRampingUp = false;
        //        }
        //    }

        //    _lastWorkerStart = DateTime.Now;
        //}

        public void Enqueue(T task)
        {
            //Console.WriteLine("Enqueuing {0}", task);
            _continue = true;
            _jobQueue.Enqueue(task);
            //Console.WriteLine("{2}\t\tTask {0} enqueued. Total in queue: {1}", task, _jobQueue.Count, System.Threading.Thread.CurrentThread.ManagedThreadId);
            startTaskAsync();
            //startWorker();
        }

        private void startTaskAsync()
        {
            if (workersMaxedOut)
            {
                //Console.WriteLine("Workers maxed out");
                return;
            }

            if (!_continue && QueueLength == 0)
            {
                //Console.WriteLine("Told not to continue. stopping");
                onQueueEmpty();
                return;
            }

            Task.Factory.StartNew(() => StartTask());
        }

        private bool workersMaxedOut
        {
            get
            {
                var maxed = InProgress >= MaxWorkers;
                return maxed;
            }
        }

        private void onQueueEmpty()
        {
            //Console.WriteLine("onQueueEmpty called {0}", QueueLength);
            if (QueueEmpty != null)
            {
                QueueEmpty();

                _WaitHandle.Set();
            }
            //_continue = false;
        }

        private void StartTask()
        {
            lock (_startTaskSync)
            {
                if (workersMaxedOut)
                {
                    return;
                }
                InProgress++;
            }

            checkQueue();

            try
            {

                var task = getNextTask();

                if (task != null)
                {
                    NextTask(task);
                }
                else
                {
                    //Console.WriteLine("Next task is null {0}", _jobQueue.Count);
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (InProgress > 0) InProgress--;

                startTaskAsync();
            }

        }

        private T getNextTask()
        {

            T nextTask;
            int maxRetryAttempts = 5;

            while (maxRetryAttempts > 0)
            {
                if (_jobQueue.Count == 0)
                {
                    return default(T);
                }

                if (_jobQueue.TryDequeue(out nextTask))
                {
                    return nextTask;
                }
                else
                {
                    maxRetryAttempts--;
                    Thread.Sleep(100);
                }
            }

            throw new Exception(string.Format("Cannot get task from queue {0}", _jobQueue.Count));
        }
    }
}
