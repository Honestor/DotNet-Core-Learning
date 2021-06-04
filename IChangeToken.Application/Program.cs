using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace IChangeToken.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            //var machine = new Machine();
            //machine._warnEvent += Test;
            //machine.Run();
            var t = DateTime.Today;
            Console.ReadKey();
        }

        static WarnResultChild Test(object obj, object args)
        {
            Console.WriteLine($"警告!警告!温度达到了{((Machine)obj).Temperature}度,来自机器传递的参数实例,_parameter属性值为:{((Machine.WarnEventArgs)args)._parameter}");
            return new WarnResultChild();
        }
    }

    public class Machine
    {
        public delegate WarnResult WarnHandler(Machine obj, WarnEventArgs args);
        public event WarnHandler _warnEvent;

        public int Temperature { get; set; }

        public void Run()
        {
            for (var i = 0; i < 99; i++)
            {
                Temperature = i;

                if (i == 66)
                {
                    _warnEvent(this, new WarnEventArgs("达到临界温度66度"));
                }
            }
        }

        /// <summary>
        /// 机器警告事件参数模型
        /// </summary>
        public class WarnEventArgs : EventArgs
        {
            public string _parameter;
            public WarnEventArgs(string str)
            {
                _parameter = str;
            }
        }
    }

    public class WarnResult
    { 
        
    }

    public class WarnResultChild: WarnResult
    { 
        
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

    public interface IChangeToken
    {

    }

    public static class ChangeToken
    {
        public static void OnChange<TState>(Func<IChangeToken> producer, Action<TState> consumer, TState state)
        {

        }
    }
}
