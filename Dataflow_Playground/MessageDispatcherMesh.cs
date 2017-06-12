using System;
using System.Collections.Generic;
using System.Threading;

using NUnit.Framework;
using System.Threading.Tasks.Dataflow;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Dataflow_Playground
{
    [TestFixture]
    public class MessageDispatcherMesh
    {

#if Uncomment_Until_Code_Is_Ready
        static class MessageTypes
        {
            internal static class WriteToConsole
            {
                internal static readonly string MessageType = "WriteToConsole";

                internal static readonly string Prefix = "Prefix";
            }
        }

        class Message
        {
            public Message(string messageType, IReadOnlyDictionary<string,string> parameter)
            {
                this.MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
                this.Paramters = parameter ?? throw new ArgumentNullException(nameof(parameter));
            }

            public Message ShallowCopy()
            {
                return (Message)this.MemberwiseClone();
            }

            public IReadOnlyDictionary<string, string> Paramters;

            public string MessageType { get; private set; }
        }

        [SetUp]
        public void Setup()
        {
            InitMessageToFuncMapper();

            // Local Helpers
            void InitMessageToFuncMapper()
            {
                var builder = ImmutableDictionary.CreateBuilder<string, Func<Message, Task<IReadOnlyCollection<Message>>>>();
                builder.Add(MessageTypes.WriteToConsole.MessageType, async inMsg =>
                {
                    await Task.Delay(100);

                    Console.WriteLine($"[ {inMsg.Paramters[MessageTypes.WriteToConsole.Prefix]} {MessageTypes.WriteToConsole.MessageType}]: Received message {inMsg}");
                    return new List<Message>();
                });

              
                this.messageToFuncMapper = builder.ToImmutable();
            }
        }

        private CancellationTokenSource cts = new CancellationTokenSource();

        private ImmutableDictionary<string, Func<Message, Task<IReadOnlyCollection<Message>>>> messageToFuncMapper;



        [Test]
        public void xxxx()
        {
            var incomingMessageBufferBlock1 = new BufferBlock<Message>(new DataflowBlockOptions { BoundedCapacity = 5, CancellationToken = this.cts.Token });
            var incomingMessageBufferBlock2 = new BufferBlock<Message>(new DataflowBlockOptions { BoundedCapacity = 5, CancellationToken = this.cts.Token });


            var workers = new List<TransformBlock<Message, IReadOnlyCollection<Message>>>();
            SetupMesh();


            var wokerTasks = workers.Select(w => w.Completion).ToArray();

            foreach (var i in Enumerable.Range(0,4))
            {
                Task.Run(() =>
               {

               });
            }




            Task.WaitAll(wokerTasks);



            // Local helpers
            void SetupMesh()
            {
                CreateWorkers();

                var propagateCompletion = new DataflowLinkOptions { PropagateCompletion = true };

                workers.ForEach(worker =>
               {
                   incomingMessageBufferBlock1.LinkTo(worker, propagateCompletion);
                   incomingMessageBufferBlock2.LinkTo(worker, propagateCompletion);
               });
            }

            void CreateWorkers()
            {
                foreach (var i in Enumerable.Range(0, 2))
                {
                    workers.Add(new TransformBlock<Message, IReadOnlyCollection<Message>>(inMsg =>
                    {
                        if (this.messageToFuncMapper.ContainsKey(inMsg.MessageType))
                        {
                            return this.messageToFuncMapper[inMsg.MessageType].Invoke(inMsg);
                        }

                        return null;

                    }
                        , new ExecutionDataflowBlockOptions { BoundedCapacity = 1 }));
                }
            }
        }

#endif
    }

}

