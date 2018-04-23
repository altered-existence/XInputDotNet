using System;
using System.Threading;
using XInputDotNetPure;

namespace XInputDemo
{
    class Program
    {
        static GamePadManager manager;
        static void Main(string[] args)
        {
            manager = new GamePadManager();
            while (true)
            {
                manager.Pole();

                if (manager.GetButtonUp(PlayerIndex.One, ButtonCode.A))
                {
                    Console.WriteLine("Up");
                }

                if (manager.GetButton(PlayerIndex.One, ButtonCode.A))
                {
                    Console.Write("AAA");
                }

                if (manager.GetButtonDown(PlayerIndex.One, ButtonCode.A))
                {
                    Console.WriteLine("Down");
                }

                Thread.Sleep(16);
            }
        }
    }
}
