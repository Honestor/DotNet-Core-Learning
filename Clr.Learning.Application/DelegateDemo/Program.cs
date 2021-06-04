using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clr.Learning.Application.DelegateDemo
{
    public class Program
    {
        public static void Start()
        {
            new Machine().Run();
        }
    }

    public class Machine
    {
        public delegate void WarnHandler(object obj, MachineWarnEventArgs args);

        public  void Run()
        {
            for (var i =0; i < 100; i++)
            {
                if (i == 66)
                {
                    new WarnHandler(Machine_Warn).Invoke(this, new MachineWarnEventArgs($"温度达到了{i}度"));
                }
            }
        }

        public static void Machine_Warn(object obj, MachineWarnEventArgs args)
        {
            Console.WriteLine($"警报,{args._param}");
        }

        public class MachineWarnEventArgs : EventArgs
        {
            public string _param;

            public MachineWarnEventArgs(string param)
            {
                _param = param;
            }
        }
    }

    public class MachineWarn
    { 
        
    }
}
