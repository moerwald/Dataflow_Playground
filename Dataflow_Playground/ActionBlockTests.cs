using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using System.Diagnostics;

namespace Dataflow_Playground
{
    [TestFixture]
    public class ActionBlockTests
    {

        [Test]
        public void ActionBlock()
        {
            void TraceWithTreadId(string traceMessage)
            {
                Console.WriteLine($"[ThreadID: {System.Threading.Thread.CurrentThread.ManagedThreadId}]: {traceMessage}");
            }

            TraceWithTreadId("Starting test");

            var transformationBlock = new TransformBlock<int, int>(val =>
            {
                TraceWithTreadId("In transformation block");
                return val * 2;
            });

            var checkOutcome = new ActionBlock<int>(val =>
            {
                TraceWithTreadId("Checking test result");
                val.Should().Be(4);
            });

            transformationBlock.LinkTo(checkOutcome);

            transformationBlock.Post(2);
            TraceWithTreadId("After posting");

            transformationBlock.Complete();
            checkOutcome.Complete();
            if (checkOutcome.Completion.Wait(TimeSpan.FromMilliseconds(5000)) == false)
            {
                Assert.IsTrue(false, $"Wait operation for {nameof(checkOutcome)} to tong");
            }

        }
    }
}
