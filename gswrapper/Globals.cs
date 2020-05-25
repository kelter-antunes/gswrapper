//
// Global.cs
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


/***************************About Global.cs************************************
 * 
 * Global variables declared by Ghostscript are declared here. All globals 
 * are declared in the main namespace GSWrapper. Most of the variables are 
 * provided for reference. May or may not have been used in this application.
 * Used a separate file for clarity. No class is declared here. 
 * 
 * ***************************************************************************/


using System;


namespace GSWrapper
{
    /// <summary>
    /// All the constants used are defined here
    /// </summary>
    public static class CONSTANTS
    {
        public const char SPACE = ' ';
        public const char NEWLINE = (char)0x0a;
        public const string TOTALPAGES = "Total Pages:";
        public const string PROCESSPERCENTAGE = "Processed Percent: ";
        public const string CANCELLEDPROCESS = "Processing Cancelled by User";
        public const string PROGRESSPROCESS = "Processing in Progress";
        public const string PROCESSCOMPLETE = "Process Completed!";
        public const int KSERROR = -1000;
        public const int NOERROR = 0;
        public const string CLEARSTACK = " clear cleardictstack ";
        public const string DLL32 = "gsdll32.dll";
        public const string DLL64 = "gsdll64.dll";
        public const string DELEGATEERROR = "Delegate for {0} exported function not found";
        public const int GSVERSION = 950; 

        public const string LICENSEINFO = 
                    "Author: Vijaya Vasudevan establishes his right as owner of this software and grants Permission free of charge, " + 
                    "to any person obtaining  a copy of this software and associated documentation files and use the Software without restriction.\n " +
                    "\nThis software uses Ghostscript binaries provided under GNU. License details can be found at:\n" +  
                    "https://www.ghostscript.com/license.html \n" + 
                    "\nThis software uses GSWrapper - a c# wrapper for Ghostscript developed by Kamban Software and provided under GNU. " + 
                    "A copy of the license, source of the DLL and source of this application can be found at:\n" +  
                    "https://kamban.visualstudio.com/_git/gswrapper \n" +
                    "\nTHE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE " + 
                    "WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.\n" + 
                    "\nIN NO EVENT SHALL THE AUTHOR OR Kamban Software BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION " + 
                    "OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION  WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. \n" +
                    "\nOur Website: \n" +  
                    "https://kamban.com.au";
  
    }

    #region Delegates for Event handlers

    /// <summary>
    /// StdIO call back handler used by gsapi_set_stdio. Function delegate signature is provided
    /// in Ghostscript API documentation. Use the delegate signature to create method and then
    /// attach it to gsapi_set_stdio function parameters.
    /// Three public variables are declared by GSdll.cs with the StdIO delegate signature. Any application
    /// can declare their own call back functions with delegate signature and then attach this to
    /// the declared variable in GSdll.cs. See the supplied KSPdfView application for implementing
    /// </summary>
    /// <param name="handle">Instance handle provided by ghostscript to calling method</param>
    /// <param name="pointer">Pointer to a string that is passed to calling method</param>
    /// <param name="count">Length of the string</param>
    /// <returns>zero if successfully handles are attached</returns>
    public delegate int StdioMessageEventHandler(IntPtr handle, IntPtr pointer, int count);

    /// <summary>
    /// Public delegate for DisplayEventHandler. This is raised by GSDisplay when the bitmap buffer is full
    /// to offload the image.
    /// </summary>
    /// <param name="obj">object raising the event</param>
    /// <param name="e">DisplayEvent variables. Here Bitmap is the only variable that is used</param>
    public delegate void DisplayEventHandler(object obj, DisplayEvent e); 

    /// <summary>
    /// Public delegate used by GSInterface to raise an event to provide 
    /// feedback about API commands being used for a method declared.
    /// </summary>
    /// <param name="obj">object raising the event</param>
    /// <param name="e">StatusEvents variables. Two string variables are declared to provide feedback</param>
    public delegate void StatusEventsHandler(object obj, StatusEvents e);

    /// <summary>
    /// Callback function for gsapi_set_poll function.
    /// </summary>
    /// <param name="handle">Handle for Poll call back function</param>
    /// <returns></returns>
    public delegate int PollCallBackFunction(IntPtr handle);

    #endregion

    #region delegates for FileSystem Structure Not tested. Passed As-Is

    /*****************Notes on File System Functions*******************
     * 
     * If the filename (always given in utf-8 format) is recognised as being one that the filing system handles (perhaps by the prefix used), 
     * then it should open the file, fill in the gp_file pointer and return 0.
     * If the filename is not-recognised as being one that the filing system handles, then returning 0 will cause the filename to be 
     * offered to other registered filing systems.
     * If an error is returned (perhaps gs_error_invalidfileaccess), then no other filing system will be allowed to try to open the file. 
     * This provides a mechanism whereby a caller to gsapi can completely control access to all files accessed via gp_fopen at runtime.
     * 
     * Note, that while most file access within ghostscript will be redirected via these functions, stdio will not; see the existing mechanisms 
     * within Ghostscript for intercepting/replacing this.
     * 
     * Please check https://www.ghostscript.com/doc/9.52/API.htm#add_fs for additional info
     * 
     * *****************************************************************/


    /// <summary>
    /// The OpenFileFS function pointer will be called when something (most often a call to gp_fopen) attempts to open a file.
    /// </summary>
    /// <param name="gs_memory"></param>
    /// <param name="secret"></param>
    /// <param name="fname"></param>
    /// <param name="mode"></param>
    /// <param name="fileptr"></param>
    /// <returns></returns>
    public delegate int OpenFileFS(IntPtr gs_memory, IntPtr secret, IntPtr fname, IntPtr mode, IntPtr fileptr);


    /// <summary>
    /// The OpenPipeFS function pointer will be called when something (most often a call to gp_popen) attempts to open a pipe. 
    /// rfname points to a 4K buffer in which the actual name of the opened pipe should be returned.
    /// </summary>
    /// <param name="gs_memory"></param>
    /// <param name="secret"></param>
    /// <param name="fname"></param>
    /// <param name="rfname"></param>
    /// <param name="mode"></param>
    /// <param name="fileptr"></param>
    /// <returns></returns>
    public delegate int OpenPipeFS(IntPtr gs_memory, IntPtr secret, IntPtr fname, IntPtr rfname, IntPtr mode, IntPtr fileptr);


    /// <summary>
    /// The OpenScratchFS function pointer will be called when something (most often a call to gp_open_scratch_file or 
    /// gp_open_scratch_file_rm) attempts to open a temporary file. rfname points to a 4K buffer in which the actual name of the 
    /// opened pipe should be returned. If rm is true, then the file should be set to delete itself when all handles to it are closed.
    /// </summary>
    /// <param name="gs_memory"></param>
    /// <param name="secret"></param>
    /// <param name="fname"></param>
    /// <param name="rfname"></param>
    /// <param name="mode"></param>
    /// <param name="rm"></param>
    /// <param name="fileptr"></param>
    /// <returns></returns>
    public delegate int OpenScratchFS(IntPtr gs_memory, IntPtr secret, IntPtr fname, IntPtr rfname, IntPtr mode, int rm, IntPtr fileptr);

    /// <summary>
    /// The OpenPrinterFS function pointer will be called when something (most often a call to gp_open_printer) attempts to open a stream 
    /// to a printer. If binary is true, then the stream should be opened as binary; most streams will be binary by default - 
    /// this has historical meaning on OS/2.
    /// </summary>
    /// <param name="gs_memory"></param>
    /// <param name="secret"></param>
    /// <param name="fname"></param>
    /// <param name="binary"></param>
    /// <param name="fileptr"></param>
    /// <returns></returns>
    public delegate int OpenPrinterFS(IntPtr gs_memory, IntPtr secret, IntPtr fname, int binary, IntPtr fileptr);


    #endregion

    #region structure of File System. Not tested. Passed As-Is

    /// <summary>
    /// File System Structure. Initialise with proper Callback functions. Used by gsapi_add_fs
    /// Each 'filing system' within gs is a structure of function pointers; each function pointer 
    /// gives a handler from taking a different named resource (a file, a pipe, a printer, 
    /// a scratch file etc) and attempts to open it.
    /// Note OpenFileFS signature is used by FSOpenFile and FSOpenHandle, as they both have similar parameters
    /// 
    /// The open_handle function pointer will be called when something (most often a call via the postscript %handle% IO device) 
    /// attempts to open a Windows handle. This entry point will never be called on non-Windows builds.
    /// </summary>
    public struct GSAPI_FS_T
    {
        public OpenFileFS       FSOpenFile;
        public OpenPipeFS       FSOpenPipe;
        public OpenScratchFS    FSOpenScractch;
        public OpenPrinterFS    FSOpenPrinter;
        public OpenFileFS       FSOpenHandle;
    }


    #endregion

    #region enums & Structs defined in Ghostscript. Given for reference   

    /// <summary>
    /// As defined by Ghostscript Revision structure
    /// </summary>
    public struct GSAPI_REVISION_S
    {
        public IntPtr product;
        public IntPtr copyright;
        public int revision;
        public int revisiondate;
    }


    /// <summary>
    /// Encoding. for Windows UTF8 is used
    /// </summary>
    public enum GS_ARG_ENCODING : int
    {
        LOCAL = 0,
        UTF8 = 1,
        UTF16LE = 2
    }


    /// <summary>
    /// Page sizes as defined by Ghostscript
    /// </summary>
    public enum GS_PAGESIZES
    {
        UNDEFINED,
        ledger, legal, letter, lettersmall,
        archE, archD, archC, archB, archA,
        a0, a1, a2, a3, a4, a4small, a5, a6, a7, a8, a9, a10,
        isob0, isob1, isob2, isob3, isob4, isob5, isob6,
        c0, c1, c2, c3, c4, c5, c6,
        jisb0, jisb1, jisb2, jisb3, jisb4, jisb5, jisb6,
        b0, b1, b2, b3, b4, b5,
        flsa, flse, halfletter
    }


    /// <summary>
    /// Devices as defined by Ghostscript
    /// </summary>
    public enum GS_DEVICES
    {
        UNDEFINED,
        png16m, pnggray, png256, png16, pngmono, pngalpha,
        jpeg, jpeggray,
        tiffgray, tiff12nc, tiff24nc, tiff32nc, tiffsep, tiffcrle, tiffg3, tiffg32d, tiffg4, tifflzw, tiffpack,
        faxg3, faxg32d, faxg4,
        bmpmono, bmpgray, bmpsep1, bmpsep8, bmp16, bmp256, bmp16m, bmp32b,
        pcxmono, pcxgray, pcx16, pcx256, pcx24b, pcxcmyk, psdcmyk, psdrgb,
        pdfwrite, pswrite, epswrite,
        pxlmono, pxlcolor
    }


    /// <summary>
    /// GhostScript error code enumeration. these are taken from the GhostScript error.h file.
    /// In general a return value of 0 or above is considered successful operation as of 
    /// ghostscript version 9.52. 
    /// </summary>
    public enum ReturnCode : int
    {
        // Postscript level 1 errors
        e_unknownerror = -1,
        e_dictfull = -2,
        e_dictstackoverflow = -3,
        e_dictstackunderflow = -4,
        e_execstackoverflow = -5,
        e_interrupt = -6,
        e_invalidaccess = -7,
        e_invalidexit = -8,
        e_invalidfileaccess = -9,
        e_invalidfont = -10,
        e_invalidrestore = -11,
        e_ioerror = -12,
        e_limitcheck = -13,
        e_nocurrentpoint = -14,
        e_rangecheck = -15,
        e_stackoverflow = -16,
        e_setackunderflow = -17,
        e_syntaxerror = -18,
        e_timeout = -19,
        e_typecheck = -20,
        e_undefined = -21,
        e_undefinedfilename = -22,
        e_undefinedresult = -23,
        e_unmatchedmark = -24,
        e_VMerror = -25,

        // Additional level 2 and DPS errors
        e_configurationerror = -26,
        e_invalidcontext = -27,
        e_undefinedresource = -28,
        e_unregistered = -29,
        
        // Pseudo-errors used by ghostscript internally
        e_invalidid = -30,                                  // invalidid is for the NeXT DPS extension.
        e_fatal = -100,                                     // Internal code for a fatal error. gs_interpret also returns this for a .quit with a positive exit code.
        e_Quit = -101,                                      // Internal code for the .quit operator. The real quit code is an integer on the operand stack. gs_interpret returns this only for a .quit with a zero exit code.
        e_InterpreterExit = -102,                           // Internal code for a normal exit from the interpreter. Do not use outside of interp.c.
        e_RemapColor = -103,                                // Internal code that indicates that a procedure has been stored in the remap_proc of the graphics state, and should be called before retrying the current token.  This is used for color remapping involving a call back into the interpreter -- inelegant, but effective.
        e_ExecStackUnderflow = -104,                        // Internal code to indicate we have underflowed the top block of the e-stack.
        e_VMreclaim = -105,                                 // Internal code for the vmreclaim operator with a positive operand. We need to handle this as an error because otherwise the interpreter won't reload enough of its state when the operator returns.
        e_NeedInput = -106,                                 // Internal code for requesting more input from run_string.
        e_NeedStdin = -107,                                 // Internal code for stdin callout.
        e_NeedStdout = -108,                                // Internal code for stdout callout.
        e_NeedStderr = -109,                                // Internal code for stderr callout.
        e_Info = -110,                                      // Internal code for a normal exit when usage info is displayed. This allows Window versions of Ghostscript to pause until the message can be read.
    }

    #endregion

    #region enum as defined in gdevdsp.h for display. Given for reference

    /// <summary>
    /// The display format is set by a combination of the following bitfields
    /// Define the color space alternatives 
    /// </summary>
    public enum DISPLAY_FORMAT_COLOR : long
    {
        DISPLAY_COLORS_NATIVE = (1 << 0),
        DISPLAY_COLORS_GRAY = (1 << 1),
        DISPLAY_COLORS_RGB = (1 << 2),
        DISPLAY_COLORS_CMYK = (1 << 3),
        DISPLAY_COLORS_SEPARATION = (1 << 19),
    }

    /// <summary>
    /// Define whether alpha information, or an extra unused bytes is included 
    /// DISPLAY_ALPHA_FIRST and DISPLAY_ALPHA_LAST are not implemented 
    /// </summary>
    public enum DISPLAY_FORMAT_ALPHA : long
    {
        DISPLAY_ALPHA_NONE = (0 << 4),
        DISPLAY_ALPHA_FIRST = (1 << 4),
        DISPLAY_ALPHA_LAST = (1 << 5),
        DISPLAY_UNUSED_FIRST = (1 << 6),	/* e.g. Mac xRGB */
    DISPLAY_UNUSED_LAST = (1 << 7)	/* e.g. Windows BGRx */
    }

    /// <summary>
    /// Define the depth per component for DISPLAY_COLORS_GRAY,
    /// DISPLAY_COLORS_RGB and DISPLAY_COLORS_CMYK,
    /// or the depth per pixel for DISPLAY_COLORS_NATIVE
    /// DISPLAY_DEPTH_2 and DISPLAY_DEPTH_12 have not been tested.
    /// </summary>
    public enum DISPLAY_FORMAT_DEPTH : long
    {
        DISPLAY_DEPTH_1 = (1 << 8),
        DISPLAY_DEPTH_2 = (1 << 9),
        DISPLAY_DEPTH_4 = (1 << 10),
        DISPLAY_DEPTH_8 = (1 << 11),
        DISPLAY_DEPTH_12 = (1 << 12),
        DISPLAY_DEPTH_16 = (1 << 13)
        /* unused (1<<14) */
        /* unused (1<<15) */
    }

    /// <summary>
    /// Define whether Red/Cyan should come first,
    /// or whether Blue/Black should come first
    /// </summary>
    public enum DISPLAY_FORMAT_ENDIAN
    {
        DISPLAY_BIGENDIAN = (0 << 16),	/* Red/Cyan first */
        DISPLAY_LITTLEENDIAN = (1 << 16)	/* Blue/Black first */
    }

    /// <summary>
    /// Define whether the raster starts at the top or bottom of the bitmap
    /// </summary>
    public enum DISPLAY_FORMAT_FIRSTROW
    {
        DISPLAY_TOPFIRST = (0 << 17),	/* Unix, Mac */
        DISPLAY_BOTTOMFIRST = (1 << 17)	/* Windows */
    }


    /// <summary>
    /// Define whether packing RGB in 16-bits should use 555
    /// or 565 (extra bit for green)
    /// </summary>
    public enum DISPLAY_FORMAT_555
    {
        DISPLAY_NATIVE_555 = (0 << 18),
        DISPLAY_NATIVE_565 = (1 << 18)
    }


    /// <summary>
    /// Define the row alignment, which must be equal to or greater than
    /// the size of a pointer.
    /// The default (DISPLAY_ROW_ALIGN_DEFAULT) is the size of a pointer,
    /// 4 bytes (DISPLAY_ROW_ALIGN_4) on 32-bit systems or 8 bytes
    /// (DISPLAY_ROW_ALIGN_8) on 64-bit systems.
    /// </summary>
    public enum DISPLAY_FORMAT_ROW_ALIGN
    {
        DISPLAY_ROW_ALIGN_DEFAULT = (0 << 20),
        /* DISPLAY_ROW_ALIGN_1 = (1<<20), */
        /* not currently possible */
        /* DISPLAY_ROW_ALIGN_2 = (2<<20), */
        /* not currently possible */
        DISPLAY_ROW_ALIGN_4 = (3 << 20),
        DISPLAY_ROW_ALIGN_8 = (4 << 20),
        DISPLAY_ROW_ALIGN_16 = (5 << 20),
        DISPLAY_ROW_ALIGN_32 = (6 << 20),
        DISPLAY_ROW_ALIGN_64 = (7 << 20)
    }



    #endregion

}
