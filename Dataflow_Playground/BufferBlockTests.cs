using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using System.Collections.Immutable;

namespace Dataflow_Playground
{
    [TestFixture]
    public class BufferBlockTests
    {
        [Test]
        public void BufferBlock_PostAndReceive()
        {
            List<int> results = new List<int>();

            var bufferBlock = new BufferBlock<int>();
            for (int i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // Receive the messages back from the block.
            int value;
            while (bufferBlock.TryReceive(out value))
            {
                results.Add(value);
            }

            results.Should().Contain(Enumerable.Range(0, 3));
        }

        [Test]
        public async Task BufferBlock_Limit()
        {
            var bufferBlock = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 1 });
            var actionBlock = new ActionBlock<int>(
                val => TraceHelper.TraceWithTreadId($"Received value {val}. DateTime: {System.DateTime.UtcNow}"),
                new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

            bufferBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            foreach (var i in Enumerable.Range(0, 100))
            {
                await bufferBlock.SendAsync(i);
            }

            bufferBlock.Complete();
            await actionBlock.Completion;
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
                var entry = await bufferBlock.ReceiveAsync().ConfigureAwait(false);
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
    }
}
