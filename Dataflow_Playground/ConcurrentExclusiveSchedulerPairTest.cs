using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading.Tasks.Dataflow;
using System.Threading;

namespace Dataflow_Playground
{

    [TestFixture]
    public class ConcurrentExclusiveSchedulerPairTest
    {
        [Test]
        public async Task CalcAvg_OnNonSynchronized_Collection()
        {
            // Non thread-safe collection
            var intCollection = new List<int>();

            // Scheduler synchronizes reader and writer task
            var taskSchedulerPair = new ConcurrentExclusiveSchedulerPair();
            var cts = new CancellationTokenSource();

            // Create reader task
            var readerTask = Task.Factory.StartNew(async () =>
                {
                    while (true) // Run as long not canceled from outside
                    {
                        if (intCollection.Any())
                        {
                            TraceHelper.TraceWithTreadId($"--------------------- Average: {intCollection.Average()}");
                        }
                        await Task.Delay(100); // Sim. some I/O
                    }
                },
                cts.Token, // Used to break while loop
                TaskCreationOptions.None,
                taskSchedulerPair.ConcurrentScheduler);
           
            var writerAction = new ActionBlock<int>(
                msg =>
                {
                   intCollection.Add(msg);
                   TraceHelper.TraceWithTreadId($"Added {msg} to list");
                },
                new ExecutionDataflowBlockOptions
                {
                    TaskScheduler = taskSchedulerPair.ExclusiveScheduler
                }
            );

            BroadcastBlock<int> broadcaster = new BroadcastBlock<int>(null);
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            broadcaster.LinkTo(writerAction, linkOptions);

            foreach (var x in Enumerable.Range(0, 500))
            {
                await broadcaster.SendAsync(x);
                await Task.Delay(42);
            }

            broadcaster.Complete();
            cts.Cancel();
            Task.WaitAll(new[] { writerAction.Completion, taskSchedulerPair.Completion, readerTask }, TimeSpan.FromMilliseconds(5000));

            
        }
    }
}
