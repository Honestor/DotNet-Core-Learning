using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IChangeToken.Application
{
    class Program
    {

        /// <summary>
        /// CancellationChangeToken测试
        /// </summary>
        /// <param name="args"></param>
        //static void Main(string[] args)
        //{
        //    CancellationTokenSource tokenSource = new CancellationTokenSource();
        //    CancellationChangeToken changeToken = new CancellationChangeToken(tokenSource.Token);
        //    var m = new Machine(tokenSource);
        //    var w = new Worker(changeToken);
        //    var mm = new Manager(changeToken);
        //    Console.ReadKey();
        //}

        /// <summary>
        /// Token发生改变,执行注册的回调函数
        /// </summary>
        /// <param name="args"></param>
        //static void Main(string[] args)
        //{
        //    var token = new TestChangeToken();
        //    ChangeToken.OnChange(() => token, () => Console.WriteLine($"管理员人您好,机器发生故障"));
        //    Console.WriteLine("开始发送故障通知");
        //    Task.Delay(2000).Wait();
        //    Console.WriteLine("故障通知发送成功");
        //    token.Changed();
        //    Console.ReadKey();
        //}

        static void Main(string[] args)
        {
            var token = new TestChangeToken();
            object state = new object();
            object callbackState = null;
            ChangeToken.OnChange(() => token, s => callbackState = s, state);
            token.Changed();
            Console.ReadKey();
        }
    }

    public class Machine
    {
        public Machine(CancellationTokenSource tokenSource)
        {
            Task.Run(() =>
            {
                Console.WriteLine("开始发送故障通知");
                Task.Delay(2000).Wait();
                Console.WriteLine("故障通知发送成功");
                tokenSource.Cancel();
            });
        }
    }

    public class Worker
    {
        public Worker(CancellationChangeToken cancellationChangeToken)
        {
            cancellationChangeToken.RegisterChangeCallback(token =>
            {
                Console.WriteLine($"工作人员您好,机器发生故障");
            }, null);
        }
    }

    public class Manager
    {
        public Manager(CancellationChangeToken cancellationChangeToken)
        {
            cancellationChangeToken.RegisterChangeCallback(token =>
            {
                Console.WriteLine($"管理员人您好,机器发生故障");
            }, null);
        }
    }

    public class TestChangeToken : IChangeToken
    {
        private Action _callback;

        public bool ActiveChangeCallbacks { get; set; }
        public bool HasChanged { get; set; }

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            _callback = () => callback(state);
            return null;
        }

        public void Changed()
        {
            HasChanged = true;
            _callback();
        }
    }
}
