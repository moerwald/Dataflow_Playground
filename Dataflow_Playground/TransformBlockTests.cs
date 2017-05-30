using System;
using NUnit.Framework;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using System.Threading.Tasks;

namespace Dataflow_Playground
{
    [TestFixture]
    public class TransformBlockTests
    {
       

        [Test]
        public void Transformblock_Sync()
        {
            TraceHelper.TraceWithTreadId("Starting test");

            var multiplyBlock = new TransformBlock<int, int>(val =>
            {
                TraceHelper.TraceWithTreadId("In transformation block");
                return val * 2;
            });

            var checkOutcome = new ActionBlock<int>(val =>
            {
                TraceHelper.TraceWithTreadId("In action block");
                val.Should().Be(4);
            });

            multiplyBlock.LinkTo(checkOutcome, new DataflowLinkOptions { PropagateCompletion = true });

            multiplyBlock.Post(2);

            multiplyBlock.Complete();
            if (checkOutcome.Completion.Wait(TimeSpan.FromMilliseconds(5000)) == false)
            {
                Assert.IsTrue(false, $"Wait operation for {nameof(checkOutcome)} to tong");
            }
        }

        [Test]
        public async Task TransformBlock_Async()
        {
            TraceHelper.TraceWithTreadId("Starting test");

            var multiplyBlock = new TransformBlock<int, int>(val =>
            {
                TraceHelper.TraceWithTreadId("In transformation block");
                return val * 2;
            });

            var checkOutcome = new ActionBlock<int>(val =>
            {
                TraceHelper.TraceWithTreadId("In action block");
                Assert.IsTrue(val >= 0 && val <= 20);
            });

            multiplyBlock.LinkTo(checkOutcome, new DataflowLinkOptions { PropagateCompletion = true });

            for (int i = 0; i < 10; i++)
            {
                await multiplyBlock.SendAsync(i);
            }

            multiplyBlock.Complete();

            await checkOutcome.Completion;
        }
    }
}
