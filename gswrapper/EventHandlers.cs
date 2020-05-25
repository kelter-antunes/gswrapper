//
// EventHandlers.cs
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

/***************************About EventHandlers.cs************************************
 * 
 * Two Events are defined in GSWrapper. 
 * StatusEvents is used by GSInterface
 * as a way to document the process of commands. At this stage it does not 
 * serve major purpose.
 * 
 * Display Event is used by GSInteractive which raises an event when the bitmap buffer
 * is full. This is used by calling program to save the bitmap image and present it
 * in the application.  
 * 
 * ***************************************************************************/


using System;
using System.Drawing;

namespace GSWrapper
{

    /// <summary>
    /// Event handler to provide feedback to calling application. GSInterface uses
    /// StatusEvents to provide the Ghostscript API functions that are executed 
    /// with the full parameters by the interface.
    /// There is no major functionality at this time for this event. Left for future use
    /// Note: CallbackFunction Notes.txt has notes on raising and handling Events
    /// </summary>
    public class StatusEvents : EventArgs
    {
        public string statusData; //Not used. Left for future versions
        public string additionalInfo;
    }

    /// <summary>
    /// This is raised by GSDisplay when a bitmap is ready in its buffer. Of the two 
    /// variable that is declared, only Bitmap is used. PageNo is actually indicates no. of copies
    /// </summary>
    public class DisplayEvent :EventArgs
    {
        public Bitmap bmp;
        public int pageNo;
    }


}
