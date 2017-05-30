using System;

namespace Dataflow_Playground
{
    internal static class TraceHelper
    {
        internal static void TraceWithTreadId(string traceMessage)
        {
            Console.WriteLine($"[ThreadID: {System.Threading.Thread.CurrentThread.ManagedThreadId}]: {traceMessage}");
        }
    }
}
