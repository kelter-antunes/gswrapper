using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace GSWrapper
{
    public abstract class StdIO
    {

        internal StdioMessageEventHandler _stdioIn;
        internal StdioMessageEventHandler _stdioOut;
        internal StdioMessageEventHandler _stdioErr;
        

        #region private Callback function for Set_stdio

        /// <summary>
        /// Constructor for StdIO. Attaches the function for event handlers
        /// </summary>
        public StdIO ()
        {
            //_stdioIn = new StdioMessageEventHandler(StdInCallbackMessageEvent);
            _stdioIn = null; //Not implemented
            _stdioOut = new StdioMessageEventHandler(StdOutCallbackMessageEvent);
            _stdioErr = new StdioMessageEventHandler(StdErrCallbackMessageEvent);
        }


        /// <summary>
        /// This is not used Provided for future compatibility
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="pointer"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int StdInCallbackMessageEvent(IntPtr handle, IntPtr pointer, int count)
        {
            return count;
        }

        private int StdOutCallbackMessageEvent(IntPtr handle, IntPtr pointer, int count)
        {

            string message = Marshal.PtrToStringAnsi(pointer, count);
            this.StdOut(message);
            return count;
        }

        private int StdErrCallbackMessageEvent(IntPtr handle, IntPtr pointer, int count)
        {
            string message = Marshal.PtrToStringAnsi(pointer);
            this.StdError(message);
            return count;
        }

        #endregion


        /// <summary>
        /// Abstract standard input method.
        /// </summary>
        /// <param name="input">Input data.</param>
        /// <param name="count">Expected size of the input data.</param>
        public abstract void StdIn(out string input, int count);

        /// <summary>
        /// Abstract standard output method.
        /// </summary>
        /// <param name="output">Output data.</param>
        public abstract void StdOut(string output);

        /// <summary>
        /// Abstract standard error method.
        /// </summary>
        /// <param name="error">Error data.</param>
        public abstract void StdError(string error);
    }








    
    
    
    
    
    internal class StdIOHandler : StdIO
    {
        private StdInEventHandler _input;
        private StdOutEventHandler _output;
        private StdErrEventHandler _error;

        public StdIOHandler(StdInEventHandler input, StdOutEventHandler output, StdErrEventHandler error)
        {
            _input = input;
            _output = output;
            _error = error;
        }

        public override void StdIn(out string input, int count)
        {
            _input(out input, count);
        }

        public override void StdOut(string output)
        {
            _output(output);
        }

        public override void StdError(string error)
        {
            _error(error);
        }
    }
}
