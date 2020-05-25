//
// GSInteractive.cs
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


/***************************About GSInteractive.cs*****************************
 * 
 * This is a derived class of GSDLL which is made abstract so only derived
 * class could access. This class
 * exposes following Ghostscript functions
 *      gsapi_run_string_begin
 *      gsapi_run_string_continue
 *      gsapi_run_string_end
 *      gsapi_run_string
 *      gsapi_run_string_length
 *      gsapi_run_file
 *      
 * It also prepares the environment for video output, and when a bitmap is 
 * ready an event is raised. The event can be passed back to calling method
 * for receiving the bitmap and handling that appropriately.
 * The stdIO calls are not handled here. Calling program must register its
 * event for handling the IO, if not output messages will be lost.
 * 
 * ***************************************************************************/


using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GSWrapper
{
    public class GSInteractive : GSdll
    {

        #region Private variables 

        private IntPtr _pInstance = IntPtr.Zero;
        private GSDisplay _gsDisplay;
        private int _errNo;
        private bool bStringBeginFunction = false;

        #endregion


        /// <summary>
        /// Initialise the environment by loading the dll and preparing the delegates. 
        /// Calls the base function for this. If you use this to Initialise your class
        /// then SetIOHandlers, SetConsole and SetDisplayEventHandler must be called
        /// in that order
        /// </summary>
        public GSInteractive()
        {
        }

        #region Preparation of Environment

        /// <summary>
        /// Simplified instantiation of the class. Message event handler and GSDisplay event handler
        /// can be passed as parameter which are properly handled. Any of this pointer can be null
        /// if there is no need to handle them
        /// </summary>
        /// <param name="stdIn">Callback StdioMessageEventHandler function pointer for StdIo-Input</param>
        /// <param name="stdOut">Callback StdioMessageEventHandler function pointer for StdIo-Output</param>
        /// <param name="stdErr">Callback StdioMessageEventHandler function pointer for StdIo-Error</param>
        /// <param name="dh">Callback DisplayEventHandler function pointer for GSDisplay Event</param>
        /// <returns>True if successful</returns>
        public bool GSInitialise(StdioMessageEventHandler stdIn, StdioMessageEventHandler stdOut, StdioMessageEventHandler stdErr, DisplayEventHandler dh)
        {
            if (InitialiseDLL())
            {
                SetStdIOHandlers(stdIn, stdOut, stdErr);
                SetConsole();
                _gsDisplay.SetDisplayEventHandler(dh);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the console for Interactive display. Uses GSDisplay Class to handle display events
        /// UTF8 is setup for encoding. Callback handlers for StdIO are also set by calling base functions
        /// </summary>
        /// <returns>True if successful</returns>
        protected bool SetConsole()
        {
            _pInstance = IntPtr.Zero;
            IntPtr displayDevice_callback_handle;
            _gsDisplay = new GSDisplay();
            string[] strArray = GetArgsForDisplay();

            // allocate a memory for the display device callback handler
            displayDevice_callback_handle = Marshal.AllocCoTaskMem(_gsDisplay._callback.size);

            // copy display device callback structure content to the pre-allocated block of memory
            Marshal.StructureToPtr(_gsDisplay._callback, displayDevice_callback_handle, true);


            _errNo = gsNewInstance(out _pInstance, IntPtr.Zero);
            if (_errNo != 0)
                return false;
            else
            {
                try
                {
                    _errNo = gsSetArgEncoding(_pInstance, GS_ARG_ENCODING.UTF8);
                    if (_errNo < 0)
                        return false;

                    _errNo = gsSetDisplayCallback(_pInstance, displayDevice_callback_handle);
                    if (_errNo < 0)
                        return false;

                    _errNo = gsInitArgs(_pInstance, strArray.Length, strArray);
                    if (_errNo != 0)
                        return false;

                    _errNo = gsSetStdio(_pInstance, StdIOIn, StdIOOut, StdIOErr);
                    if (_errNo != 0)
                        return false;
                }
                catch (Exception)
                {
                    _pInstance = IntPtr.Zero;
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Sets the Callback DisplayEventHander function pointer passed by calling program
        /// </summary>
        /// <param name="dh">DisplayEventHandler function pointer</param>
        public void SetDisplayEventHandler(DisplayEventHandler dh)
        {
            _gsDisplay.SetDisplayEventHandler(dh);
        }


        /// <summary>
        /// Sets the Callback StdioMessageEventHandler functions of the remote program for handling StdIO messages
        /// </summary>
        /// <param name="stdIn">Callback StdioMessageEventHandler function pointer for StdIo-Input</param>
        /// <param name="stdOut">Callback StdioMessageEventHandler function pointer for StdIo-Ouput</param>
        /// <param name="stdErr">Callback StdioMessageEventHandler function pointer for StdIo-Error</param>
        public void SetIOHandlers(StdioMessageEventHandler stdIn, StdioMessageEventHandler stdOut, StdioMessageEventHandler stdErr)
        {
            SetStdIOHandlers(stdIn, stdOut, stdErr);
        }

        #endregion


        #region Clean up methods

        /// <summary>
        /// Calls CleanUp method. Delete the stored Instance handler and make Display class null
        /// </summary>
        ~GSInteractive()
        {
            CleanUp();
        }


        /// <summary>
        /// Delete the stored Instance handler and make Display class null
        /// </summary>
        private void CleanUp()
        {
            if (_pInstance != IntPtr.Zero)
            {
                gsExit(_pInstance);
                gsDeleteInstance(_pInstance);
            }
            if (_gsDisplay != null)
                _gsDisplay = null;
        }

        #endregion

        #region Helper methods for String operations

        /// <summary>
        /// Helper function for Ghostscript String DLLs.
        /// Method is simplified by calling this with just the string that needs to be executed
        /// Passed string is automatically converted to UTF8 format
        /// Frontend to: gsapi_run_string
        /// </summary>
        /// <param name="str">String that needs to be executed. String is automatically converted to UTF8</param>
        /// <returns>Zero after execution. Use GetLastError to get any erros encountered </returns>
        public int RunString(string str)
        {
            if (_pInstance == IntPtr.Zero)
                return CONSTANTS.KSERROR;
            
            str = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(str));
            gsRunString(_pInstance, str, 0, out _errNo);

            return CONSTANTS.NOERROR;
        }

        /// <summary>
        /// Helper function for Ghostscript String DLLs.
        /// Method is simplified by calling this 
        /// Frontend to: gsapi_run_string_begin
        /// </summary>
        /// <returns>Zero if Successful</returns>
        public int RunStringBegin()
        {
            if (bStringBeginFunction) //Already started? and not closed?
                RunStringEnd();
            
            bStringBeginFunction = true;        
            gsRunStringBegin(_pInstance, 0, out _errNo);
            
            return _errNo;
        }

        /// <summary>
        /// Helper function for Ghostscript String DLLs.
        /// Method is simplified by calling this with just the string that needs to be executed
        /// Passed string is automatically converted to UTF8 format
        /// Frontend to: gsapi_run_string_continue
        /// </summary>
        /// <param name="str">String that needs to be executed. String is automatically converted to UTF8</param>
        /// <returns>Zero after execution. Use GetLastError to get any erros encountered </returns>
        public int RunStringContinue(string str)
        {
            if (str == "" || _pInstance == IntPtr.Zero)
                return CONSTANTS.KSERROR;

            if (!bStringBeginFunction) //If not started we will start it
            {
                _errNo = RunStringBegin();
                if (_errNo != 0)
                    return _errNo;
            }            
            str = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(str));
            gsRunStringContinue(_pInstance, str,(uint)str.Length, 0, out _errNo);
            return CONSTANTS.NOERROR;
        }


        /// <summary>
        /// Helper function for Ghostscript String DLLs.
        /// Method is simplified by calling this with just the string that needs to be executed
        /// Passed string is automatically converted to UTF8 format
        /// Frontend to: gsapi_run_string_with_length
        /// </summary>
        /// <param name="str">String that needs to be executed. String is automatically converted to UTF8</param>
        /// <returns>Zero after execution. Use GetLastError to get any erros encountered </returns>
        public int RunStringLength(string str)
        {
            if (str == "" || _pInstance == IntPtr.Zero)
                return CONSTANTS.KSERROR;

            str = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(str));
            gsRunStringLength(_pInstance, str, (uint)str.Length, 0, out _errNo);
            return CONSTANTS.NOERROR;
        }

        /// <summary>
        /// Helper function for Ghostscript String DLLs.
        /// Method is simplified by calling this to load a file on to the Ghostscript DLL
        /// Passed Filename is automatically converted to UTF8 format
        /// Frontend to: gsapi_run_file
        /// </summary>
        /// <param name="strPath">Filename with full path to be loaded. Path is automatically converted to UTF8</param>
        /// <returns>Zero after execution. Use GetLastError to get any erros encountered </returns>
        public int RunFile(string strPath)
        {
            if (_pInstance == IntPtr.Zero)
                return CONSTANTS.KSERROR;

            strPath = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(strPath));
            gsRunFile(_pInstance, strPath, 0, out _errNo);

            return CONSTANTS.NOERROR;
        }


        /// <summary>
        /// Helper function for Ghostscript String DLLs.
        /// Method is simplified by calling this 
        /// Frontend to: gsapi_run_string_end
        /// </summary>
        /// <returns>Zero if Successful</returns>
        public int RunStringEnd()
        {
            if (!bStringBeginFunction)      //Not started or already ended?
                return CONSTANTS.NOERROR;

            gsRunStringEnd(_pInstance, 0, out int errors);
            bStringBeginFunction = false;
            return errors;
        }

        #endregion


        #region Method to fetch Revision, Error and Bitmap

        /// <summary>
        /// Returns the GSAPI_REVISION_S structure defined by GSDLL. Calling program can analyse 
        /// this for version and release date of the DLL. InitialiseDLL must have been called before
        /// calling this method
        /// </summary>
        /// <returns>GSAPI_REVISION_S structure</returns>
        public GSAPI_REVISION_S GetRevisionInfo()
        {
            return VERSIONINFO;
        }

        /// <summary>
        /// This method will return the last error encountered. Should be called immediately
        /// after running a command as the error gets updated to the latest operation
        /// </summary>
        /// <returns></returns>
        public int GetLastError()
        {
            return _errNo;
        }


        /// <summary>
        /// Returns the last bitmap image that was processed. Only useful if single page 
        /// is processed. Calling application should use DisplayEventHandler to get the 
        /// Bitmap and store it locally
        /// </summary>
        /// <returns></returns>
        public Bitmap GetRecentBitmap()
        {
            if (_gsDisplay != null)
                return _gsDisplay.GetBitmap();
            return null;
        }

        #endregion

    }
}
