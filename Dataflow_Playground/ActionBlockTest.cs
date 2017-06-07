using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Dataflow_Playground
{
    [TestFixture]
    public class ActionBlockTest
    {

        [Test]
        public void ActionRunsInOneTask()
        {
            ActionBlock<int> ab = new ActionBlock<int>(i =>
            {
                 TraceHelper.TraceWithTreadId($"{i}");
             });

            foreach (var x in Enumerable.Range(1, 10))
            {
                ab.Post(x);
            }

            // Wait till the ActionBlock is ready
            ab.Complete();
            ab.Completion.Wait();
        }

        [Test]
        public void ActionRunWithMultipleTasks()
        {
            ActionBlock<int> ab = new ActionBlock<int>(async i =>
            {
                await Task.Delay(500);
                TraceHelper.TraceWithTreadId($"{i}");

            },new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10, MaxMessagesPerTask = 1  });

            foreach (var x in Enumerable.Range(1, 100))
            {
                ab.Post(x);
            }

            // Wait till the ActionBlock is ready
            ab.Complete();
            ab.Completion.Wait();
        }


        [Test]
        public void UseMultipleActionBlocks()
        {

            Func<int, Task> actionPerformedByAllBlocks = async i =>
            {
                await Task.Delay(10);
                TraceHelper.TraceWithTreadId($" Number is {i}");
            };

            List<ActionBlock<int>> abList = new List<ActionBlock<int>>();


            foreach (var x in Enumerable.Range(0, 10))
            {
                abList.Add(new ActionBlock<int>(
                    actionPerformedByAllBlocks,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10, BoundedCapacity = 1 /* Non greedy block*/, NameFormat = $"ActionBlock[{x}]" }));
            }

            var bb = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 10000, NameFormat = "BufferBlock" });

            foreach (var ab in abList)
            {
                bb.LinkTo(ab, new DataflowLinkOptions { PropagateCompletion = true });
            }

            foreach (var x in Enumerable.Range(0, 1000))
            {
                bb.Post(x);
            }

            bb.Complete(); // Signal the mesh that we are done

            Task.WaitAll(GetTasksFromActionBlocks()); // Wait for action blocks to get finished


            // Helpers
            Task[] GetTasksFromActionBlocks()
            {
                return abList.Select(ab => ab.Completion).ToArray();
            }
        }

      
    }
}
