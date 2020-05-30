using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using System.Collections.Immutable;
using System;

namespace Dataflow_Playground
{
    [TestFixture]
    public class BufferBlockTests
    {
        [Test]
        [Description("Posts and receives data in a synchronous way.")]
        public void BufferBlock_PostAndReceive()
        {
            List<int> results = new List<int>();

            var bufferBlock = new BufferBlock<int>();
            for (int i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // Receive the messages back from the block.
            while (bufferBlock.TryReceive(out int value))
            {
                results.Add(value);
            }

            results.Should().Contain(Enumerable.Range(0, 3));
        }


        [Test]
        public async Task BufferBlock_PostSynchronousAndReceiveAsync()
        {
            IImmutableList<int> exceptedEntries = ImmutableList.Create(Enumerable.Range(0, 3).ToArray());

            var cts = new System.Threading.CancellationTokenSource();

            var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions { CancellationToken = cts.Token });

            var t = Task.Run(() =>
            {
                // Post synchronosly from another task
                for (int i = 0; i < 3; i++)
                {
                    bufferBlock.Post(i);
                }
                bufferBlock.Complete();
            },
            cts.Token);

            do
            {
                var entry = await bufferBlock.ReceiveAsync().ConfigureAwait(false); // Resume execution from default task scheduler
                TraceHelper.TraceWithTreadId($"Receiving data {entry}");
                // Check if the polled value is included in the expected list
                exceptedEntries.Should().Contain(entry);
            }
            while (await bufferBlock.OutputAvailableAsync());

            // wait for completion
            if (t.Wait(System.TimeSpan.FromMilliseconds(500)) == false)
            {
                cts.Cancel();
            }
        }

        #region BoundedCapacity

        [Test]
        [Description("Check if Bufferblock declines messages added via Post(), if BoundedCapacity is set to 1 ")]
        public async Task BufferBlock_Limit()
        {
            var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 1 });
            var actionBlock = new ActionBlock<int>(
                async val =>
                {
                    await Task.Delay(2000);
                    TraceHelper.TraceWithTreadId($"Received value {val}. DateTime: {System.DateTime.UtcNow}");
                },
                new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

            bufferBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            const int MessageToProduce = 10;
            int numberOfDroppedMessages = 0;
            foreach (var i in Enumerable.Range(0, MessageToProduce))
            {
                if (bufferBlock.Post(i) == false)
                {
                    numberOfDroppedMessages++;
                    TraceHelper.TraceWithTreadId($"Could not add {i} to buffer block.");
                }
            }

            Assert.AreEqual(9, numberOfDroppedMessages);

            bufferBlock.Complete();
            await actionBlock.Completion;
        }

        [Test]
        [Description("Check if Bufferblock throttle incoming messages if message are posted via SendAsync() (BoundedCapacity is set to 1)")]
        public async Task BufferBlock_Limit_SendAsync()
        {
            var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 1 /* Throttle incoming messages */ });
            var actionBlock = new ActionBlock<int>(
                async val =>
                {
                    TraceHelper.TraceWithTreadId($"Received value {val}. DateTime: {System.DateTime.UtcNow}");
                    await Task.Delay(100);
                },
                new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

            bufferBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            const int MessageToProduce = 10;
            int numberOfDroppedMessages = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (var i in Enumerable.Range(0, MessageToProduce))
            {
                if (await bufferBlock.SendAsync(i) == false)
                {
                    numberOfDroppedMessages++; // Should not get increased -> Bufferblock throttles incoming messages
                    TraceHelper.TraceWithTreadId($"Could not add {i} to buffer block.");
                }
                else
                {
                    TraceHelper.TraceWithTreadId($"+++ ADDED {i} to buffer block Time = {DateTime.UtcNow}");
                }
            }

            sw.Stop();

            TraceHelper.TraceWithTreadId($"Adding messages took {sw.Elapsed}");

            Assert.AreEqual(0, numberOfDroppedMessages);

            bufferBlock.Complete();
            await actionBlock.Completion;
        }

        #endregion
    }
}
