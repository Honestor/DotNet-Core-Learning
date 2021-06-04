using System;
using System.Threading;

namespace Timer.Application
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Test(null);
            Console.ReadKey();
        }

        /// <summary>
        /// 基于Threading的Timer-最底层的Timer 结论:Timer并不会等待前一个回调执行结束,线程会被唤醒,调用ThreadPool添加task到线程池队列中
        /// </summary>
        static void Threading_Timer_CanNotWait()
        {
            var index = 1;
            var timer = new System.Threading.Timer(state => {
                Console.WriteLine($"第{index}个任务开始执行");
                var temp = index;
                ++index;
                Thread.Sleep(2000);
                Console.WriteLine($"第{temp}个任务开始执行完毕");
            }, 0, 0, 1000);
        }


        private static System.Threading.Timer _timer;
        /// <summary>
        /// 基于Threading的Timer-最底层的Timer 结论:Timer等待前一个回调执行结束,线程会被唤醒,调用ThreadPool添加task到线程池队列中
        /// </summary>
        static void Threading_Timer_Wait()
        {
            var index = 1;
            _timer = new System.Threading.Timer(state => {
                Console.WriteLine($"第{index}个任务开始执行");
                var temp = index;
                ++index;
                Thread.Sleep(2000);
                Console.WriteLine($"第{temp}个任务开始执行完毕");
                _timer.Change(1000, Timeout.Infinite);
            }, 0, 0, Timeout.Infinite);
        }

        static void Test(object state)
        {
            CheckTime();
            Console.WriteLine(1);
        }

        static void CheckTime()
        {
            DateTime now = DateTime.Now;
            DateTime oneOClock = DateTime.Today.AddHours(8);
            if (now > oneOClock)
            {
                oneOClock = oneOClock.AddDays(1.0);
            }

            int msUntilFour = (int)((oneOClock - now).TotalMilliseconds);
            var t = new System.Threading.Timer(Test);

            t.Change(msUntilFour, Timeout.Infinite);
        }
    }
}
