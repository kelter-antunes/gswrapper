//
// GSdll.cs
// This is a part of the source files of GSWrapper.dll 
// which is a c# wrapper for Ghostscript library
//
// Author: Vijaya Vasudevan (vdevan@gmail.com, https://kamban.com.au) 
// Copyright (c) 2020 by Kamban Software, Australia. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files and use the 
// Software without restriction. Sources full or part and compiled resources
// can be used without limitation. Permission is granted to copy, modify, 
// merge, publish, distribute and/or sell copies of the Software, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software. 
// Though no mandatory any changes done to the sources supplied herein
// if modified, a copy of the same can be provided to author for including
// in future upgrades.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


/***************************About GSDll.cs************************************
 * 
 * Core class that interacts with the DLL. All delegates are defined here.
 * The class itself is declared as abstract. Two derived class GSInterface and
 * GSInteractive will interface with this class and exposes the functions to
 * remote application. The class provides some helper function for derived
 * class. 
 * 
 * ***************************************************************************/


using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace GSWrapper
{
    

    public abstract class GSdll :IDisposable
    {
        //To ensure single instance only. Lock the instance handle obtained during function call 
        //and then release and delete the instance        

        /*****************************************************************
         * Kernel functions that are used to load DLL modules into memory.
         * These will work when compiled for 32bit or with x64
         * Using int.Size, App can be assumed as compiled for 64 bit or 32 bit and 
         * then appropriate dll module can be loaded. 
         ******************************************************************/

        #region Basic Functions for Loading DLL
        /// <summary>
        /// Alternate method for loading gsdll64.dll which works when compiled under x64
        /// This function loads the specified DLL into memory
        /// </summary>
        /// <param name="dllToLoad">Name of the DLL to load - in this case gsdll64.dll</param>
        /// <returns>Pointer to the loaded module</returns>
        [DllImport("kernel32.dll")]
        protected static extern IntPtr LoadLibrary(string dllToLoad);


        /// <summary>
        /// Get the procedure address of the functions we need. 
        /// </summary>
        /// <param name="hModule">Pointer of the loaded module</param>
        /// <param name="procedureName">Name of the Procedure whose address is required</param>
        /// <returns>Pointer to the address of the procedure</returns>
        [DllImport("kernel32.dll")]
        protected static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);


        /// <summary>
        /// Frees the Pointer holding the procedure address. Basic Method called for clean-up
        /// </summary>
        /// <param name="hModule">Pointer that holds the address</param>
        /// <returns>'true' if pointer is freed else 'false</returns>
        [DllImport("kernel32.dll")]
        protected static extern bool FreeLibrary(IntPtr hModule);

        #endregion


        /************************************************
         * These are the list of functions that are exported by Ghostscript
         * Version of gsdll64.dll is 9.52. All the functions are defined in
         * API.HTM.
         ************************************************/

        #region DLL functions Exported by gssdllxx.dll

        /// <summary>
        /// Create a new instance of Ghostscript. This instance is passed to many of the gsapi functions. 
        /// The caller_handle will be provided to callback functions.Unless Ghostscript has been compiled 
        /// with the GS_THREADSAFE define, only one instance at a time is supported.
        /// </summary>
        /// <param name="pinstance">IntPtr for holding the Instance Handle. Used as 'out' parameter</param>
        /// <param name="caller_handle">Address of the procedure to get Instance Handle - gsapi_new_instance</param>
        /// <returns>Instance handle</returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_new_instance(out IntPtr pinstance, IntPtr caller_handle);


        /// <summary>
        /// This function returns the revision numbers and strings of the Ghostscript interpreter library; 
        /// you should call it before any other interpreter library functions to make sure that the correct 
        /// version of the Ghostscript interpreter has been loaded.
        /// </summary>
        /// <param name="pr">Structure of gsapi_revision_s for GhostScript Version</param>
        /// <param name="len">Length of the stucture. </param>
        /// <returns>returns the size of gsVersion if successful</returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_revision(ref GSAPI_REVISION_S pr, Int32 len);


        /// <summary>
        /// Destroy an instance of Ghostscript. Before you call this, Ghostscript must have finished. 
        /// If Ghostscript has been initialised, you must call gsapi_exit before gsapi_delete_instance
        /// </summary>
        /// <param name="instance">Obtained Instance Handle</param>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void gsapi_delete_instance(IntPtr instance);


        /// <summary>
        /// Set the callback functions for stdio The stdin callback function should return the number 
        /// of characters read, 0 for EOF, or -1 for error. The stdout and stderr callback functions 
        /// should return the number of characters written.
        /// <param name="instance">Obtained Instance Handle</param>
        /// <param name="stdin_fn">Callback Function for stdin</param>
        /// <param name="stdout_fn">Callback Function for stdout</param>
        /// <param name="stderr_fn">Callback Function for stderr</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_set_stdio(IntPtr pInstance, StdioMessageEventHandler gsdll_stdin, StdioMessageEventHandler gsdll_stdout, StdioMessageEventHandler gsdll_stderr);

        /// <summary>
        /// Set the callback function for polling. This function will only be called if the 
        /// Ghostscript interpreter was compiled with CHECK_INTERRUPTS as described in gpcheck.h.
        /// </summary>
        /// <param name="instance">Obtained Instance Handle</param>
        /// <param name="poll_fn">Callback Function for Polling</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_set_poll(IntPtr instance, PollCallBackFunction poll_fn);


        /// <summary>
        /// Set the display device callback structure.
        /// If the display device is used, this must be called
        /// after gsapi_new_instance() and before gsapi_init_with_args().
        /// See gdevdisp.h for more details.
        /// </summary>
        /// <param name="instance">Obtained Instance Handle</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_set_display_callback(IntPtr instance, IntPtr callback);


        /// <summary>
        /// Set the encoding used for the args. By default we assume
        /// 'local' encoding. For windows this equates to whatever the current
        /// codepage is. For linux this is utf8.
        /// Use of this API (gsapi) with 'local' encodings (and hence without calling
        /// this function) is now deprecated!
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_set_arg_encoding(IntPtr instance, GS_ARG_ENCODING encoding);



        /// <summary>
        /// Initialise the interpreter. Initialise the interpreter. 
        /// This calls gs_main_init_with_args() in imainarg.c. 
        /// See document for return codes. 
        /// The arguments are the same as the "C" main function: 
        /// argv[0] is ignored and the user supplied arguments are argv[1] to argv[argc-1].
        /// This calls gs_main_init_with_args() in imainarg.c
        /// </summary>
        /// <param name="instance">Obtained Instance Handle</param>
        /// <param name="argc">Length of parameters - string array length</param>
        /// <param name="argv">Actual parameters as string array</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        public delegate int gsapi_init_with_args(IntPtr instance, Int32 argc, string[] argv);


        // The gsapi_run_* functions are like gs_main_run_* except
        // that the error_object is omitted.
        // If these functions return <= -100, either quit or a fatal
        // error has occured.  You then call gsapi_exit() next.
        // The only exception is gsapi_run_string_continue()
        // which will return e_NeedInput if all is well.

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_run_string_begin(IntPtr instance, Int32 user_errors, out Int32 pexit_code);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        public delegate int gsapi_run_string_continue(IntPtr instance, String str, UInt32 length, Int32 user_errors, out Int32 pexit_code);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_run_string_end(IntPtr instance, Int32 user_errors, out Int32 pexit_code);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        protected delegate int gsapi_run_string_with_length(IntPtr instance, String str, UInt32 length, Int32 user_errors, out Int32 pexit_code);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        public delegate int gsapi_run_string(IntPtr instance, String str, Int32 user_errors, out Int32 pexit_code);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        public delegate int gsapi_run_file(IntPtr instance, String file_name, Int32 user_errors, out Int32 pexit_code);

        /// <summary>
        /// Exit the interpreter.
        /// This must be called on shutdown if gsapi_init_with_args()
        /// has been called, and just before gsapi_delete_instance().
        /// </summary>
        /// <param name="instance">Obtained Instance Handle</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_exit(IntPtr instance);


        //Not tested. 
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int gsapi_add_fs(IntPtr instance, GSAPI_FS_T fs_t, IntPtr secret);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void gsapi_remove_fs(IntPtr instance, GSAPI_FS_T fs_t, IntPtr secret);


        #endregion



        #region delegates for Ghostscript functions

        protected gsapi_revision gsRevision;
        protected gsapi_new_instance gsNewInstance;
        protected gsapi_delete_instance gsDeleteInstance;
        protected gsapi_set_stdio gsSetStdio;
        protected gsapi_set_poll gsSetPoll;
        protected gsapi_set_display_callback gsSetDisplayCallback;
        protected gsapi_set_arg_encoding gsSetArgEncoding;
        protected gsapi_run_string_begin gsRunStringBegin;
        protected gsapi_run_string_continue gsRunStringContinue;
        protected gsapi_run_string_end gsRunStringEnd;
        protected gsapi_run_string_with_length gsRunStringLength;
        protected gsapi_run_string gsRunString;
        protected gsapi_init_with_args gsInitArgs;
        protected gsapi_run_file gsRunFile;
        protected gsapi_exit gsExit;
        protected gsapi_add_fs gsAddfs;
        protected gsapi_remove_fs gsRemovefs;

        #endregion


        #region Call back Message handler declaration for StdIO & Event Handler

        public StdioMessageEventHandler _stdIn = null;
        public StdioMessageEventHandler _stdOut = null;
        public StdioMessageEventHandler _stdErr = null;


        /// <summary>
        /// Set the private handler variables to remote StdioMessageEventHandler methods. 
        /// If set the local method will pass the values to the remote handler
        /// for further processing 
        /// </summary>
        /// <param name="stdIn">Remote Input handler for StdioMessageEventHandler method</param>
        /// <param name="stdOut">Remote Output handler for StdioMessageEventHandler method</param>
        /// <param name="stdErr">Remote Error handler for StdioMessageEventHandler method</param>
        protected void SetStdIOHandlers(StdioMessageEventHandler stdIn, StdioMessageEventHandler stdOut, StdioMessageEventHandler stdErr)
        {
            _stdIn = stdIn;
            _stdOut = stdOut;
            _stdErr = stdErr;
        }

        #endregion

        public string GsMessage{ get; set; }
        public string GsErrMessage { get; set; }
        private IntPtr _loadDll;

        protected StdioMessageEventHandler StdIOIn;
        protected StdioMessageEventHandler StdIOOut;
        protected StdioMessageEventHandler StdIOErr;

        readonly string[] GS_DEFAULT =
        {
            "-ks",           //Not used. Marked for identity ks: Kamban Software
            "-dNOPAUSE",
            "-dNOPROMPT",
            "-dQUIET",
            "-dMaxBitmap=1g", 
			                
            // Configure the output anti-aliasing, resolution, etc
            "-dAlignToPixels=0",
            "-dTextAlphaBits=4",
            "-dGraphicsAlphaBits=4"
        };


        public static GSAPI_REVISION_S VERSIONINFO; 


        #region OnStdIoCallback functions


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="count"></param>
        protected int OnStdIoInput(IntPtr handle, IntPtr pointer, int count)
        {
            _stdIn?.Invoke(handle, pointer, count);
            return count;
        }

        /// <summary>
        /// Output messages are stored in GsMessage buffer. Normally
        /// not used here
        /// /// </summary>
        /// <param name="handle">Instance handle</param>
        /// <param name="pointer">Pointer to the message</param>
        /// <param name="count">Total length of the string</param>
        /// <returns>Total length of the string</returns>
        protected int OnStdIoOutput(IntPtr handle, IntPtr pointer, int count)
        {
            lock (GsMessage)
            {
                string output = Marshal.PtrToStringAnsi(pointer, count);
                GsMessage += output;

            }
            _stdOut?.Invoke(handle, pointer, count);

            return count;
        }


        /// <summary>
        /// Error messages are stored in GsErrMessage buffer. Normally
        /// not used here
        /// </summary>
        /// <param name="handle">Instance handle</param>
        /// <param name="pointer">Pointer to the message</param>
        /// <param name="count">Total length of the string</param>
        /// <returns>Total length of the string</returns>
        protected int OnStdIoError(IntPtr handle, IntPtr pointer, int count)
        {
            lock (GsErrMessage)
            {
                string message = Marshal.PtrToStringAnsi(pointer);
                GsErrMessage += message;
            }

            _stdErr?.Invoke(handle, pointer, count);

            return count;
        }

        #endregion


        #region Initialisation methods

        /// <summary>
        /// Load appropriate DLL. Check Environment for 32 bit or 64 bit and load appropriate module. 
        /// Get all the API function address from the DLL and store that in the appropriate
        /// Delegate function global variable. Must be called first thing before attempting any
        /// other function calls.
        /// </summary>
        /// <returns>true if the module is loaded. Else false.</returns>
        protected bool InitialiseDLL()
        {
            if (!SetDllLib())
                return false;

            StdIOIn = new StdioMessageEventHandler(OnStdIoInput);
            StdIOOut = new StdioMessageEventHandler(OnStdIoOutput);
            StdIOErr = new StdioMessageEventHandler(OnStdIoError);

            GsMessage = "";
            GsErrMessage = "";

            gsNewInstance = GetDelegateFunction<gsapi_new_instance>("gsapi_new_instance");
            gsDeleteInstance = GetDelegateFunction<gsapi_delete_instance>("gsapi_delete_instance");
            gsSetStdio = GetDelegateFunction<gsapi_set_stdio>("gsapi_set_stdio");
            gsSetPoll = GetDelegateFunction<gsapi_set_poll>("gsapi_set_poll");
            gsSetDisplayCallback = GetDelegateFunction<gsapi_set_display_callback>("gsapi_set_display_callback");
            gsSetArgEncoding = GetDelegateFunction<gsapi_set_arg_encoding>("gsapi_set_arg_encoding");
            gsRunStringBegin = GetDelegateFunction<gsapi_run_string_begin>("gsapi_run_string_begin");
            gsRunStringContinue = GetDelegateFunction<gsapi_run_string_continue>("gsapi_run_string_continue");
            gsRunStringEnd = GetDelegateFunction<gsapi_run_string_end>("gsapi_run_string_end");
            gsRunStringLength = GetDelegateFunction<gsapi_run_string_with_length>("gsapi_run_string_with_length");
            gsRunString = GetDelegateFunction<gsapi_run_string>("gsapi_run_string");
            gsInitArgs = GetDelegateFunction<gsapi_init_with_args>("gsapi_init_with_args");
            gsRunFile = GetDelegateFunction<gsapi_run_file>("gsapi_run_file");
            gsExit = GetDelegateFunction<gsapi_exit>("gsapi_exit");
            gsAddfs = GetDelegateFunction<gsapi_add_fs>("gsapi_add_fs");
            gsRemovefs = GetDelegateFunction<gsapi_remove_fs>("gsapi_remove_fs");
            return true;
        }


        /// <summary>
        /// Returns the function pointer which can be used as method to be called
        /// </summary>
        /// <typeparam name="T">Type of the Function delegated</typeparam>
        /// <param name="procName">Name of the Function</param>
        /// <returns>Instance of the Function from the DLL</returns>
        private T GetDelegateFunction<T>(string procName)
        {
            IntPtr procAddress = GetProcAddress(_loadDll, procName);
            return Marshal.GetDelegateForFunctionPointer<T>(procAddress);
        }


        /// <summary>
        /// Check the Environment for 32bit or 64 bit and then Check the current 
        /// app path and also its subdirectories to load the approrpriate Ghostscript DLL.
        /// To ensure in case Ghostscript package is installed, it will check the programfiles path as well
        /// </summary>
        /// <returns>True if successful</returns>
        private bool SetDllLib()
        {
            string dll = CONSTANTS.DLL32;
            string path = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            if (IntPtr.Size > 4 )
            {
                dll = CONSTANTS.DLL64;
                path = Environment.GetEnvironmentVariable("ProgramFiles");
            }

            string[] strfile = Directory.GetFiles(Directory.GetCurrentDirectory(), dll, SearchOption.AllDirectories);
            if (strfile.Length > 0)
            {
                foreach (string f in strfile)
                {
                    if (GetVersionInfo(f))
                        return true;
                }
            }

            //Now search the program paths, in case Ghostscript is installed.
            return CheckProgramPath(path, dll);

        }

        /// <summary>
        /// Checks the program paths for the dll. Also checks the versions of the installation 
        /// to ensure older versions are not accidentally picked up
        /// </summary>
        /// <param name="programPath">c:\Program Files or c: \Program Files(x86)</param>
        /// <param name="dll">gsdll32 or gsdll64</param>
        /// <returns>True if right version found</returns>
        private bool CheckProgramPath(string programPath, string dll)
        {

            //Get subdirectories -Check the top subdirectory. If no access then move on
            string[] path = Directory.GetDirectories(programPath, "*", SearchOption.TopDirectoryOnly);
            int pathcount;
            for (pathcount = 0; path.Length > pathcount; pathcount++)
            {
                try
                {
                    string []strfile = Directory.GetFiles(path[pathcount], dll, SearchOption.AllDirectories);

                    if (strfile.Length > 0)
                    {
                        foreach (string f in strfile)
                        {
                            if (GetVersionInfo(f))
                                return true;
                        }
                    }

                }

                catch (Exception e)
                {
                    Debug.WriteLine("Unauthorized Access: " + e.Message);
                }

            }
            return false;
        }

        /// <summary>
        /// Loads the library and checks the right version. If right version found,
        /// Sets the Library & Version (Revision) information in the VERSIONINFO structure
        /// Sets the default switches to be used
        /// If version is < 9.5 then unloads the library
        /// </summary>
        /// <param name="dllPath">The path containing the GSDLLXX</param>
        /// <returns>if successful, true</returns>
        protected bool GetVersionInfo(string dllPath)
        {
            _loadDll = LoadLibrary(dllPath);
            if (_loadDll == IntPtr.Zero)
                return false;

            //Store the address of the various functions in respective delegate.
            gsRevision = GetDelegateFunction<gsapi_revision>("gsapi_revision");
            if (gsRevision == null)
                return false;
            
            gsRevision(ref VERSIONINFO, Marshal.SizeOf(VERSIONINFO));

            if (VERSIONINFO.revision >= CONSTANTS.GSVERSION)
                return true;

            FreeLibrary(_loadDll);
            _loadDll = IntPtr.Zero;

            return false;
        }

        #endregion


        #region Default parameters in addition to Default Switches - GS_DEFAULT


        /// <summary>
        /// Default Parameters Required for API calls
        /// </summary>
        /// <returns>Returns the array list of default switches</returns>
        protected ArrayList GetDefaultParams()
        {
            ArrayList argsArray = new ArrayList();
            foreach (string s in GS_DEFAULT)
                argsArray.Add(s);
            argsArray.Add("-dSAFER");
            argsArray.Add("-dGridFitTT=2");
            argsArray.Add("-dBATCH");
            argsArray.Add("-sDEVICE=jpeg");
            return argsArray;
        }

        /// <summary>
        /// Default parameters required for Display settings
        /// </summary>
        /// <returns>Array of string converted to UTF8</returns>
        protected string[] GetArgsForDisplay()
        {
            ArrayList argsArray = new ArrayList();
            foreach (string s in GS_DEFAULT)
                argsArray.Add(s);

            argsArray.Add("-sDEVICE=display");

            if (Environment.Is64BitProcess)
                argsArray.Add("-sDisplayHandle=0");
            else
                argsArray.Add("-dDisplayHandle=0");

            argsArray.Add("-dDisplayFormat=" +
                        ((int)DISPLAY_FORMAT_COLOR.DISPLAY_COLORS_RGB |
                        (int)DISPLAY_FORMAT_ALPHA.DISPLAY_ALPHA_NONE |
                        (int)DISPLAY_FORMAT_DEPTH.DISPLAY_DEPTH_8 |
                        (int)DISPLAY_FORMAT_ENDIAN.DISPLAY_LITTLEENDIAN |
                        (int)DISPLAY_FORMAT_FIRSTROW.DISPLAY_BOTTOMFIRST).ToString());

            argsArray.Add("-dDOINTERPOLATE");
            argsArray.Add("-dGridFitTT=0");
            argsArray.Add("-dNOSAFER");
            string[] strArray = (string[])argsArray.ToArray(typeof(string));

            for (int i = 0; i < strArray.Length; i++)
                strArray[i] = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(strArray[i]));

            return strArray;
        }

        #endregion


        #region Clean up methods

        ~GSdll()
        {
            Dispose();
        }


        /// <summary>
        /// Releases all resources. 
        /// </summary>
        public void Dispose()
        {
            FreeLibrary(_loadDll);
            _loadDll = IntPtr.Zero;
        }

        /// <summary>
        /// Exit and release the Instance Handle obtained for calling API functions
        /// </summary>
        /// <param name="pInt"></param>
        public void Cleanup(IntPtr pInt)
        {
            if (pInt != IntPtr.Zero)
            {
                gsExit(pInt);
                gsDeleteInstance(pInt);
            }
        }

        #endregion

    }




}
