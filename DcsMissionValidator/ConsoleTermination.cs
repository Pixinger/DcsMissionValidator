using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DcsMissionValidator
{
    class ConsoleTermination
    {
        #region  P/Invoke: bool SetConsoleCtrlHandler(CtrlTypeEventHandler handler, bool add);
        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        private delegate bool HandlerRoutineDelegate(CtrlTypes ctrlType);

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutineDelegate handler, bool add);
        #endregion

        private volatile bool _IsRunning = true;
        private HandlerRoutineDelegate _NativeHandler;

        public ConsoleTermination()
        {
            _NativeHandler += new HandlerRoutineDelegate(OnNativeHandler);
            SetConsoleCtrlHandler(_NativeHandler, true);
        }

        private bool OnNativeHandler(CtrlTypes controlType)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            if (controlType == CtrlTypes.CTRL_C_EVENT)
            {
                //allow main to run off
                _IsRunning = false;
                return true;
            }

            // If we get here, we have not handled this callback. Let the callaer know about it.
            return false;
        }


        public static void WaitForControlC()
        {
            var instance = new ConsoleTermination();

            while (instance._IsRunning)
            {
                System.Threading.Thread.Sleep(200);
            }
        }
    }
}
