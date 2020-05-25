//
// GSInterface.cs
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


/***************************About GSInterface.cs************************************
 *
 * Basic interface to all Ghostscript functions. Provides easy access to all functions.
 * Also has two helper function. OutputJpgFile With very minimal input each page of PDF 
 * file can be converted to JPG file.
 * ExecuteCommand is the easier interface to gs_init_args, which takes care of all the
 * background requirements for running this function and then cleans the environment
 * after running the file.
 * 
 * ***************************************************************************/




using System;
using System.Collections;


namespace GSWrapper
{
    public class GSInterface : GSdll
    {
        public int GSVersion { get; } = 0;
        public int GSDate { get; } = 0;

        private static readonly object objLock = new object();

        public StatusEventsHandler FeedbackStatus;
        private string _strArgs;

        /// <summary>
        /// constructor that initialises the DLL. Check GSVersion to ensure 
        /// the constructor initialised successfully
        /// </summary>
        public GSInterface()
        {
            if (InitialiseDLL())
            {
                GSVersion = VERSIONINFO.revision;
                GSDate = VERSIONINFO.revisiondate;
            }
        }


        #region Support function provided by GSInterface for quick access

        /// <summary>
        /// Reads the supplied PDF file and output a jpeg pdf File 
        /// page nos, dpi and image size can be prescribed. If left to default
        /// first page will be output with dpi of 96. If firstpage is zero
        /// then lastPage is ignored and all the pages are output to individual file
        /// The file name will be automatically numbered with Pagenumbers
        /// </summary>
        /// <param name="pdfFileName">pdfFile for processing</param>
        /// <param name="imgFileName">image file output name. Page numbered will be automatically appended</param>
        /// <param name="firstPage">First page number to start processing</param>
        /// <param name="lastPage">Last page number for processing</param>
        /// <param name="pageWidth"> Page Width for the output  - default=0 will use A4 and next parameter ignored</param>
        /// <param name="pageHeight">Page Height for the output image - default=0 which is actual</param>
        /// <param name="dpix">dots per inch x- default 96</param>
        /// <param name="dpiy">dots per inch x- default 96</param>
        /// <returns>Not used</returns>
        public int OutputJpgFile(string pdfFileName, string imgFileName = "", 
                int firstPage = -1, int lastPage = -1, int pageWidth = 0, int pageHeight = 0, int dpix = 96, int dpiy = 96)
        {
            ArrayList argList = GetDefaultParams();

            //Change \ to / Unix standard for filename
            //pdfFileName = pdfFileName.Replace('\\', '/');
            //Pages to Output - Default all Pages 
            if (firstPage > 0)
            {
                argList.Add(string.Format("dFirstPage={0}", firstPage));
                argList.Add(string.Format("dLastPage={0}", lastPage > firstPage ? lastPage : firstPage));
            }
            else
            {
                argList.Add("-dFirstPage=1");
            }

            //Page size default is A4
            if (pageWidth > 0)
            {
                argList.Add(string.Format("-dDEVICEWIDTHPOINTS={0}", pageWidth));
                if (pageHeight > 0)
                    argList.Add(string.Format("-dDEVICEHEIGHTPOINTS={0}", pageHeight));
                else
                    argList.Add(string.Format("-dDEVICEHEIGHTPOINTS={0}", pageWidth));

            }
            else
                argList.Add(string.Format("-sPAPERSIZE={0}", GS_PAGESIZES.a4));

            //Page Switch. Same page size for the whole document and fit images per page
            argList.Add("-dFIXEDMEDIA");
            argList.Add("-dPDFFitPage");

            //Resolution - default used is 96 DPI
            argList.Add(string.Format("-r{0}x{1}", dpix,dpiy)); //-rXRESxYRES

            // Output Files
            if (imgFileName == "")
                imgFileName = pdfFileName;

            argList.Add(String.Format("-sOutputFile={0}%02d.jpg", imgFileName.Substring(0, imgFileName.LastIndexOf('.'))));


            //Input File
            argList.Add(pdfFileName);

            _strArgs = argList[0].ToString();
            for (int i = 1; i < argList.Count; i++)
                _strArgs += ", " + argList[i];

            for (int i = 0; i < argList.Count; i++)
                argList[i] = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(argList[i].ToString()));

            return ExecuteCommand((string[])argList.ToArray(typeof(string)));
        }


        /// <summary>
        /// ExecuteCommand is the easier interface to gs_init_args, which takes care of all the
        /// background requirements for running this function and then cleans the environment
        /// after running the file. If you do not want to handle the stdIO then you can always
        /// call GetLastMessage and GetLastErrorMessage to get the messages.
        /// </summary>
        /// <param name="args">Args that need to be passed</param>
        /// <param name="stdIn">If you want to handle Input locally set this. Generally leave this null for this call</param>
        /// <param name="stdOut">If you want to handle the messages then set this</param>
        /// <param name="stdErr">If you want to handle the error messages then set this</param>
        /// <returns></returns>
        public int ExecuteCommand(string[] args, StdioMessageEventHandler stdIn=null, StdioMessageEventHandler stdOut=null, StdioMessageEventHandler stdErr=null)
        {

            // Get a pointer to an instance of the Ghostscript API and run the API with the current arguments
            
            IntPtr pInstance = IntPtr.Zero;
            int result;
            GsMessage = "";
            GsErrMessage = "";
            lock (objLock)
            {
                result = gsNewInstance(out pInstance, IntPtr.Zero);
                StatusUpdate(string.Format("Sent Command: 'gsNewInstance(out pInstance, IntPtr.Zero)'\n"), string.Format("pInstance returned: 0x{0:X16}\n",pInstance.ToInt64()));
                if (result != 0)
                    return result;

                try
                {
                    SetStdIOHandlers(stdIn, stdOut, stdErr);
                    result = gsSetStdio(pInstance, StdIOIn, StdIOOut, StdIOErr);

                    StatusUpdate(string.Format("Sent Command: 'gsSetStdio(0x{0:X16}, new StdioMessageEventHandler(OnStdIoInput), new StdioMessageEventHandler(OnStdIoOutput), new StdioMessageEventHandler(OnStdIoError))'\n", pInstance.ToInt64()));
                    result = gsSetArgEncoding(pInstance, GS_ARG_ENCODING.UTF8);
                    StatusUpdate(string.Format("Sent Command: 'gsSetArgEncoding(0x{0:X16}, GS_ARG_ENCODING.UTF8)'\n", pInstance.ToInt64()));
 
                    result = gsInitArgs(pInstance, args.Length, args);

                    StatusUpdate(string.Format("Sent Command: 'gsInitArgs(0x{0:X16}, {1}, {2})'\n",pInstance.ToInt64(),args.Length, _strArgs));
                }
                finally
                {
                    Cleanup(pInstance);
                    StatusUpdate(string.Format("Sent Command: 'gsExit(0x{0:X16})'\n",pInstance.ToInt64()));                    
                    StatusUpdate(string.Format("Sent Command: 'gsDeleteInstance(0x{0:X16})'\n", pInstance.ToInt64()));
                }
            }

            return 0;
        }

        #endregion

        #region Ghostscript function Interface

        #region Initialisation functions

        /// <summary>
        /// Returns the GSAPI_REVISION_S structure defined by GSDLL. Calling program can analyse 
        /// this for version and release date of the DLL. InitialiseDLL must have been called before
        /// calling this method. GSInterface class constructor automatically initialises the DLL.
        /// </summary>
        /// <returns>GSAPI_REVISION_S structure</returns>
        public GSAPI_REVISION_S GSRevision()
        {
            return VERSIONINFO;
        }


        /// <summary>
        /// Create a new instance of Ghostscript. This instance is passed to most other gsapi functions. The caller_handle will be provided 
        /// to callback functions. Unless Ghostscript has been compiled with the GS_THREADSAFE define, only one instance at a time is supported.
        /// Historically, Ghostscript has only supported a single instance; any attempt to create more than one at a time would result in 
        /// gsapi_new_instance returning an error.Experimental work has been done to lift this restriction; if Ghostscript is compiled with the 
        /// GS_THREADSAFE define then multiple concurrent instances are permitted.
        /// </summary>
        /// <param name="pInstance">Ghostscript instance handle that will be returned. Make sure the pointer is initialised to zero</param>
        /// <returns>zero if successful</returns>
        public int GSNewInstance(out IntPtr pInstance)
        {
            return gsNewInstance(out pInstance, IntPtr.Zero);
        }

        /// <summary>
        /// Call this method before Calling GSDeleteInstance to ensure all the process are complete. 
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <returns>zero if successful</returns>
        public int GSExit(IntPtr pInstance)
        {
            return gsExit(pInstance);
        }

        /// <summary>
        /// Destroy an instance of Ghostscript. Before you call this, Ghostscript must have finished. If Ghostscript has been initialised, 
        /// you must call GSExit before GSDeleteInstance. Free library needs to be
        /// called to free the library after this call. This is automatically handled by the class Destructor
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        public void GSDeleteInstance(IntPtr pInstance)
        {
             gsDeleteInstance(pInstance);
        }


        /// <summary>
        /// Set the callback functions for stdio The stdin callback function should return the number of characters read, 0 for EOF, 
        /// or -1 for error. The stdout and stderr callback functions should return the number of characters written. See KSPdfView for
        /// implementation of this
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="stdIn">StdioMessageEventHandler pointer for Input</param>
        /// <param name="stdOut">StdioMessageEventHandler pointer for Output Message</param>
        /// <param name="stdErr">StdioMessageEventHandler pointer for Error Output</param>
        /// <returns>returns zero if successful</returns>
        public int GSSetStdio(IntPtr pInstance, StdioMessageEventHandler stdIn, StdioMessageEventHandler stdOut, StdioMessageEventHandler stdErr)
        {
            SetStdIOHandlers(stdIn, stdOut, stdErr);
            return gsSetStdio(pInstance, StdIOIn, StdIOOut, StdIOErr);
        }


        /// <summary>
        /// Set the callback function for polling. This function will only be called if the Ghostscript interpreter was compiled with CHECK_INTERRUPTS 
        /// as described in gpcheck.h. The polling function should return zero if all is well, and return negative if it wants ghostscript to abort.This 
        /// is often used for checking for a user cancel.This can also be used for handling window events or cooperative multitasking. The polling function 
        /// is called very frequently during interpretation and rendering so it must be fast. If the function is slow, then using a counter to return 0 
        /// immediately some number of times can be used to reduce the performance impact.
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="poll">PollCallBack event handler pointer</param>
        /// <returns>Return zero if all is well, and return negative you want ghostscript to abort.</returns>
        public int GSSetPoll(IntPtr pInstance, PollCallBackFunction poll)
        {
            return gsSetPoll(pInstance, poll);
        }


        /// <summary>
        /// Set the encoding used for the interpretation of all subsequent args supplied via the gsapi interface on this instance. By default we 
        /// expect args to be in encoding 0 (the 'local' encoding for this OS). On Windows this means "the currently selected codepage". On Linux 
        /// this typically means utf8. This means that omitting to call this function will leave Ghostscript running exactly as it always has. 
        /// Please note that use of the 'local' encoding is now deprecated and should be avoided in new code. This must be called after 
        /// GSNewInstance and before GSInitArgs.
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="enc">Encoding type required as defined in Enum GS_ARG_ENCODING</param>
        /// <returns>zero if successful</returns>
        public int GSSetEncoding(IntPtr pInstance, GS_ARG_ENCODING enc)
        {
            return gsSetArgEncoding(pInstance, enc);
        }


        /// <summary>
        /// Set the callback structure for the display device. If the display device is used, this must be called after GSNewInstance and before 
        /// GSInitArgs. See gdevdsp.h for more details.
        /// GSInteractive class handles complete suite of stdIO and the Display callback. Users are encouraged to use GSInteractive  class. 
        /// Check KSPdfView for implementation of this function. This method here is given here for completeness. 
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="displayDeviceCallback">A pointer to DisplayCallback Structure</param>
        /// <returns>Zero if successful</returns>
        public int GSSetDisplayCallback(IntPtr pInstance, IntPtr displayDeviceCallback)
        {
            return gsSetDisplayCallback(pInstance, displayDeviceCallback);
        }

        #endregion

        #region Ghostscript Run functions

        /// <summary>
        /// This function will send a string to the Ghostscript for processing. Generally the gsapi_run_* functions are like gs_main_run_* 
        /// except that the error_object is omitted. If this functions return <= -100, either quit or a fatal error has occured. 
        /// You must call GSExit next. The only exception is GSRunStringContinue which will return e_NeedInput( = -106) if all is well. 
        /// The user_errors argument is normally set to zero to indicate that errors should be handled through the normal mechanisms within the 
        /// interpreted code. If set to a negative value, the functions will return an error code directly to the caller, bypassing the interpreted language.
        /// GSInteractive class handles complete suite of stdIO and the Display callback. Users are encouraged to use GSInteractive  class. 
        /// Check KSPdfView for implementation of this function. This method here is given here for completeness. 
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="runString">The string to run in Ghostscript</param>
        /// <param name="userError">User Error generally set to zero</param>
        /// <param name="exitError">exitError will be used to return the exit code for the interpreter in case of a quit or fatal error</param>
        /// <returns>zero if successful</returns>
        public int GSRunString(IntPtr pInstance, string runString, int userError, out int exitError)
        {
            return gsRunString(pInstance, runString, userError, out exitError);
        }

        /// <summary>
        /// This function is same as GSRunString except there is an additional parameter - length to be supplied for lengthy string operation. 
        /// This sends string (script) to Ghostscript. If this functions return <= -100, it is fatal and GSExit needs to be called. 
        /// The user_errors argument is normally set to zero to indicate that errors should be handled through the normal mechanisms within the 
        /// interpreted code. If set to a negative value, the functions will return an error code directly to the caller, bypassing the interpreted language.
        /// GSInteractive class handles complete suite of stdIO and the Display callback. Users are encouraged to use GSInteractive  class. 
        /// Check KSPdfView for implementation of this function. This method here is given here for completeness. 
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="runString">The string to run in Ghostscript</param>
        /// <param name="length">Length of the string that is run</param>
        /// <param name="userError">User Error generally set to zero</param>
        /// <param name="exitError">exitError will be used to return the exit code for the interpreter in case of a quit or fatal error</param>
        /// <returns>zero if successful</returns>
        public int GSRunStringLength(IntPtr pInstance, string runString, uint length, int userError, out int exitError)
        {
            return gsRunStringLength(pInstance, runString, length, userError, out exitError);
        }


        /// <summary>
        /// Begin a session to start running a series of string command. If this functions return <= -100, either quit or a fatal error has occured. 
        /// You must call GSExit next. After successful session call GSRunStringEnd to close the session 
        /// The user_errors argument is normally set to zero to indicate that errors should be handled through the normal mechanisms within the 
        /// interpreted code. If set to a negative value, the functions will return an error code directly to the caller, bypassing the interpreted language.
        /// GSInteractive class handles complete suite of stdIO and the Display callback. Users are encouraged to use GSInteractive  class. 
        /// Check KSPdfView for implementation of this function. This method here is given here for completeness. 
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="userError">User Error generally set to zero</param>
        /// <param name="exitError">exitError will be used to return the exit code for the interpreter in case of a quit or fatal error</param>
        /// <returns>zero if successful</returns>
        public int GSRunStringBegin(IntPtr pInstance,  int userError, out int exitError)
        {
            return gsRunStringBegin(pInstance, userError, out exitError);
        }

        /// <summary>
        /// After GSRunStringBegin, Call this function to send string (script) to Ghostscript. If this functions return 
        /// <= -100, it is fatal and GSExit needs to be called. However if this returns  e_NeedInput( = -106) then all is well. 
        /// The user_errors argument is normally set to zero to indicate that errors should be handled through the normal mechanisms within the 
        /// interpreted code. If set to a negative value, the functions will return an error code directly to the caller, bypassing the interpreted language.
        /// GSInteractive class handles complete suite of stdIO and the Display callback. Users are encouraged to use GSInteractive  class. 
        /// Check KSPdfView for implementation of this function. This method here is given here for completeness. 
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="runString">The string to run in Ghostscript</param>
        /// <param name="length">Length of the string that is run</param>
        /// <param name="userError">User Error generally set to zero</param>
        /// <param name="exitError">exitError will be used to return the exit code for the interpreter in case of a quit or fatal error</param>
        /// <returns>zero if successful</returns>
        public int GSRunStringContinue(IntPtr pInstance, string runString, uint length, int userError, out int exitError)
        {
            return gsRunStringContinue(pInstance, runString, length, userError, out exitError);
        }


        /// <summary>
        /// When you Begin a session calling GSRunStringBegin, call this method to close the session 
        /// The user_errors argument is normally set to zero to indicate that errors should be handled through the normal mechanisms within the 
        /// interpreted code. If set to a negative value, the functions will return an error code directly to the caller, bypassing the interpreted language.
        /// GSInteractive class handles complete suite of stdIO and the Display callback. Users are encouraged to use GSInteractive  class. 
        /// Check KSPdfView for implementation of this function. This method here is given here for completeness.         
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="userError">User Error generally set to zero</param>
        /// <param name="exitError">exitError will be used to return the exit code for the interpreter in case of a quit or fatal error</param>
        /// <returns>Zero if successful</returns>
        public int GSRunStringEnd(IntPtr pInstance, int userError, out int exitError)
        {
            return gsRunStringEnd(pInstance, userError, out exitError);
        }


        /// <summary>
        /// This function will load the specified file into Ghostscript dll for further processing.
        /// If this functions return <= -100, either quit or a fatal error has occured. You must call GSExit next. 
        /// The user_errors argument is normally set to zero to indicate that errors should be handled through the normal mechanisms within the 
        /// interpreted code. If set to a negative value, the functions will return an error code directly to the caller, bypassing the interpreted language.
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="fileName">The file name with full path that needs to be loaded to Ghostscript</param>
        /// <param name="userError">User Error generally set to zero</param>
        /// <param name="exitError">exitError will be used to return the exit code for the interpreter in case of a quit or fatal error</param>
        /// <returns>zero if successful</returns>
        public int GSRunFile(IntPtr pInstance, string fileName, int userError, out int exitError)
        {
            return gsRunFile(pInstance, fileName, userError, out exitError);
        }

        #endregion

        #region Main function and File server functions

        /// <summary>
        /// Initialise the interpreter. This calls gs_main_init_with_args() in imainarg.c. The arguments are the same as the "C" main function: 
        /// argv[0] is ignored and the user supplied arguments are argv[1] to argv[argc-1].
        /// Make sure GSNewInstance & GSSetEncoding is called before this call. After this, call GSExit and GSDeleteInstance to dispose the handle.
        /// Alternatively Execute command can be called, which will handle all the calls.
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="length">Length of the arguement that is passed</param>
        /// <param name="args">String array consisting of all arguements of Ghostscript processing</param>
        /// <returns>zero if successful</returns>
        public int GSInitArgs(IntPtr pInstance, int length, string[]args)
        {
            return GSInitArgs(pInstance, length, args);
        }


        /// <summary>
        /// Adds a new 'Filing System' to the interpreter. This enables callers to implement their own filing systems. The system starts with 
        /// just the conventional 'file' handlers installed, to allow access to the local filing system. Whenever files are to be opened from the 
        /// interpreter, the file paths are offered around each registered filing system in turn (from most recently registered to oldest), until 
        /// either an error is given, or the file is opened successfully.
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="gsFS">File Structure GSAPI_FS_T which has all the handles for all callback functions</param>
        /// <param name="secret">the password, if the file system is encrypted</param>
        /// <returns>Zero if successful</returns>
        public int GSAddFs(IntPtr pInstance, GSAPI_FS_T gsFS, IntPtr secret)
        {
            return gsAddfs(pInstance, gsFS, secret);
        }


        /// <summary>
        /// Removes the 'Filing System' from the interpreter that was previously added through GSAddFs. n
        /// </summary>
        /// <param name="pInstance">Ghostscript Instance handle obtained by calling GSNewInstance</param>
        /// <param name="gsFS">File Structure GSAPI_FS_T which has all the handles for all callback functions</param>
        /// <param name="secret">the password, if the file system is encrypted</param>
        public void GSRemoveFS(IntPtr pInstance, GSAPI_FS_T gsFS, IntPtr secret)
        {
            gsRemovefs(pInstance, gsFS, secret);
        }

        #endregion

        #endregion


        #region Status Updates - Provides feedback of the instructions passed to DLL

        /// <summary>
        /// At the moment this just sends the commandline arguements of all
        /// the calls send to the dll for informative purpose. Left for future 
        /// enhancement
        /// </summary>
        /// <param name="obj">Parameter not used</param>
        /// <param name="e">Two strings in StatusEvents that is passed</param>
        public virtual void  OnStatusUpdate(object obj, StatusEvents e)
        {
            FeedbackStatus?.Invoke(this, e);
        }



        /// <summary>
        /// Prepares the StatusEvents with the information before calling OnStatusUpdate
        /// </summary>
        /// <param name="status">Message that need to be passed to calling program</param>
        /// <param name="addMessage">Additional Message that need to be passed to calling program</param>
        private void StatusUpdate(string status, string addMessage="")
        {
            StatusEvents e = new StatusEvents
            {
                statusData = status,
                additionalInfo = addMessage
            };
            OnStatusUpdate(this, e);
        }

        #endregion

        #region Helper functions for messages


        /// <summary>
        /// Gets the GsMessage from base GSDll. Can be used if you do not want to handle STDIOs
        /// </summary>
        /// <returns>Last Message stored in GsMessage</returns>
        public string GetLastMessage()
        {
            return GsMessage;
        }

        /// <summary>
        /// Gets the GsErrMessage from base GSDll. Can be used if you do not want to handle STDIOs
        /// </summary>
        /// <returns>>Last Message stored in GsErrMessage</returns>
        public string GetLastErrMessage()
        {
            return GsErrMessage;
        }


        /// <summary>
        /// If you want to handle the StdIO calls then set the calls handle here
        /// </summary>
        /// <param name="stdIn">Handle for the Event for StdIn </param>
        /// <param name="stdOut">Handle for the Event for StdOut </param>
        /// <param name="stdErr">Handle for the Event for StdErr </param>
        public void SetStdHandlers(StdioMessageEventHandler stdIn, StdioMessageEventHandler stdOut, StdioMessageEventHandler stdErr)
        {
            SetStdIOHandlers(stdIn, stdOut, stdErr);
        }

        #endregion
    }
}
