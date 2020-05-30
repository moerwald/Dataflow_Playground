using NUnit.Framework;
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
        [Description("Simplest way to use action block -> post messages inside, complete posting and wait until all messages are computed.")]
        public void ActionRunsInOneTask()
        {
            ActionBlock<int> ab = new ActionBlock<int>(i =>
            {
                // Handle in messages
                TraceHelper.TraceWithTreadId($"{i}");
            });

            foreach (var x in Enumerable.Range(1, 10))
            {
                // Add in messages to the block
                ab.Post(x);
            }

            // Wait till the ActionBlock is ready
            ab.Complete();
            ab.Completion.Wait();
        }

        [Test]
        [Description("Action block uses multiple task to compute incoming messages.")]
        public void ActionRunWithMultipleTasks()
        {
            ActionBlock<int> ab = new ActionBlock<int>(async i =>  // Receive and compute message in an async way
            {
                await Task.Delay(50); // Simulate "long" running action
                TraceHelper.TraceWithTreadId($"{i}");

            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10});

            foreach (var x in Enumerable.Range(1, 100))
            {
                ab.Post(x);
            }

            // Wait till the ActionBlock is ready
            ab.Complete();
            ab.Completion.Wait();
        }


        [Test]
        [Description("Build a load balancer. Bufferblock takes incoming messages, and forwards them to a free action block. The action blocks are non-greedy -> means they don't store ")]
        public void UseMultipleActionBlocks()
        {
            const int EntriesToGenerate = 1000;
            const int ActionBlockToGenerate = 10;

            // Add blocks to list
            List<ActionBlock<int>> abList = new List<ActionBlock<int>>();
            foreach (var x in Enumerable.Range(0, ActionBlockToGenerate))
            {
                var j = x; // Avoid closure uups
                abList.Add(new ActionBlock<int>(
                    async i =>
                    {
                        await Task.Delay(10);
                        TraceHelper.TraceWithTreadId($"ActionBlock[{j}] Number is {i}");
                    },
                    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 /* Non greedy block*/, NameFormat = $"ActionBlock[{j}]", }));
            }

            // In-buffer
            var bb = new BufferBlock<int>(
                new DataflowBlockOptions
                {
                    BoundedCapacity = EntriesToGenerate,
                    NameFormat = "BufferBlock"
                });

            // Link action blocks to buffer
            foreach (var ab in abList)
            {
                bb.LinkTo(ab, new DataflowLinkOptions { PropagateCompletion = true });
            }

            // Generate messages
            foreach (var i in Enumerable.Range(0, EntriesToGenerate))
            {
                if (bb.Post(i) == false) { Assert.IsFalse(true, $"Could not post all messages to buffer block. Lost message {i}"); }
            }

            bb.Complete(); // Signal the mesh that we are done

            Task.WaitAll(GetTasksFromActionBlocks()); // Wait for action blocks to get finished

            // Local Helpers
            Task[] GetTasksFromActionBlocks() => abList.Select(ab => ab.Completion).ToArray();
        }
    }
}




