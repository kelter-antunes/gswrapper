//
// GSDisplay.cs
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



/***************************About GSDisplay.cs************************************
 * 
 * This is used for setting callback fuctions for display. The details of 
 * delegates, constants and structure for display callback are provided in
 * Ghostscript API files: gdevdsp.h 
 * When a page is processed, the Ghostscript DLL returns an image in the callback
 * function 'Size'. Then the callback function 'Page' is used to signal completed 
 * processing of the image. By implementing these two functions, the image of the 
 * PDF page being processed can be obtained. 
 * When the image (Bitmap) is ready a Display Event is raised with two event parameters:
 * Bitmap & Pages. Bitmap is the System.Drawing.Bitmap while Pages is actually
 * copies. 
 * Calling program can handlle the Display Event to obtain the bitmap and the copies
 * 
 * ***************************************************************************/


using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;


namespace GSWrapper
{
    internal class GSDisplay
    {
        //MoveMemory from Kernel32.dll - used by CopyImageToBitmap
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void MoveMemory(IntPtr destination, IntPtr source, uint length);

        #region Delegate for display callback functions. Note (CallingConvention.Cdecl)

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_open_callback(IntPtr handle, IntPtr device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_preclose_callback(IntPtr handle, IntPtr device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_close_callback(IntPtr handle, IntPtr device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_presize_callback(IntPtr handle, IntPtr device, Int32 width, Int32 height, Int32 raster, UInt32 format);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_size_callback(IntPtr handle, IntPtr device, Int32 width, Int32 height, Int32 raster, UInt32 format, IntPtr pimage);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_sync_callback(IntPtr handle, IntPtr device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_page_callback(IntPtr handle, IntPtr device, Int32 copies, Int32 flush);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_update_callback(IntPtr handle, IntPtr device, Int32 x, Int32 y, Int32 w, Int32 h);

        //Following callbacks are not used
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void display_memalloc_callback(IntPtr handle, IntPtr device, UInt32 size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int display_memfree_callback(IntPtr handle, IntPtr device, IntPtr mem);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate int display_separation_callback(IntPtr handle, IntPtr device, Int32 component, String component_name, UInt16 c, UInt16 m, UInt16 y, UInt16 k);

        #endregion

        #region constants as defined in gdevdsp.h for display
        public class Displayconst
        {
            public const int DISPLAY_VERSION_MAJOR = 2;
            public const int DISPLAY_VERSION_MINOR = 0;
            public const long DISPLAY_COLORS_MASK = 0x8000fL;
            public const long DISPLAY_ALPHA_MASK = 0x00f0L;
            public const long DISPLAY_DEPTH_MASK = 0xff00L;
            public const long DISPLAY_ENDIAN_MASK = 0x00010000L;
            public const long DISPLAY_FIRSTROW_MASK = 0x00020000L;
            public const long DISPLAY_555_MASK = 0x00040000L;
            public const long DISPLAY_ROW_ALIGN_MASK = 0x00700000L;
        }
        #endregion

        #region Structure for Display callback function as defined in gdevdsp.h

        /// <summary>
        /// GSDisplay device callback structure.
        /// 
        /// Note that for Windows, the display callback functions are
        /// cdecl, not stdcall.  This differs from those in iapi.h.
        /// Hence in delegate [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
        /// must be denoted 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct DisplayCallback
        {
            /// <summary>
            /// Size of this structure
            /// Used for checking if we have been handed a valid structure
            /// </summary>
            public int size;

            /// <summary>
            /// Major version of this structure
            /// The major version number will change if this structure changes.
            /// </summary>
            public int version_major;

            /// <summary>
            /// Minor version of this structure 
            /// The minor version number will change if new features are added
            /// without changes to this structure.  For example, a new color
            /// format.
            /// </summary>
            public int version_minor;

            /// <summary>
            /// New device has been opened 
            /// This is the first event from this device.
            /// </summary>
            public display_open_callback DisplayOpen;

            /// <summary>
            /// Device is about to be closed. 
            /// Device will not be closed until this function returns. 
            /// </summary>
            public display_preclose_callback DisplayPreclose;

            /// <summary>
            /// Device has been closed. 
            /// This is the last event from this device. 
            /// </summary>
            public display_close_callback DisplayClose;


            /// <summary>
            /// Device is about to be resized. 
            /// Resize will only occur if this function returns 0. 
            /// raster is byte count of a row.
            /// </summary>
            public display_presize_callback DisplayPresize;

            /// <summary>
            /// Device has been resized. 
            /// New pointer to raster returned in pimage
            /// </summary>
            public display_size_callback DisplaySize;

            /// <summary>
            /// flushpage
            /// </summary>
            public display_sync_callback DisplaySync;

            /// <summary>
            /// showpage 
            /// If you want to pause on showpage, then don't return immediately
            /// </summary>
            public display_page_callback DisplayPage;

            /// <summary>
            /// Notify the caller whenever a portion of the raster is updated.
            /// This can be used for cooperative multitasking or for
            /// progressive update of the display.
            /// This function pointer may be set to NULL if not required.
            /// </summary>
            public display_update_callback DisplayUpdate;

            /// <summary>
            /// Allocate memory for bitmap 
            /// This is provided in case you need to create memory in a special
            /// way, e.g. shared.  If this is NULL, the Ghostscript memory device
            /// allocates the bitmap. This will only called to allocate the
            /// image buffer. The first row will be placed at the address
            /// returned by display_memalloc.
            /// </summary>
            public display_memalloc_callback DisplayMemalloc;

            /// <summary>
            /// Free memory for bitmap 
            /// If this is NULL, the Ghostscript memory device will free the bitmap
            /// </summary>
            public display_memfree_callback DisplayMemfree;

            /// <summary>
            /// Added in V2 
            /// When using separation color space (DISPLAY_COLORS_SEPARATION),
            /// give a mapping for one separation component.
            /// This is called for each new component found.
            /// It may be called multiple times for each component.
            /// It may be called at any time between display_size
            /// and display_close.
            /// The client uses this to map from the separations to CMYK
            /// and hence to RGB for display.
            /// GS must only use this callback if version_major >= 2.
            /// The unsigned short c,m,y,k values are 65535 = 1.0.
            /// This function pointer may be set to NULL if not required.
            /// </summary>
            public display_separation_callback display_separation;
        }

        #endregion

        #region Private variables for Display

        internal DisplayCallback _callback;
        private IntPtr _srcImage;
        private Rectangle _rect;
        private int _raster;
        private Bitmap _bitmap;

        #endregion

        public event DisplayEventHandler DisplayCallbackEventHandler;


        /// <summary>
        /// Class initialised with Callback routines. 
        /// </summary>
        public GSDisplay()
        {
            _callback = new DisplayCallback();
            _callback.size = Marshal.SizeOf(_callback);

            _callback.version_minor = Displayconst.DISPLAY_VERSION_MINOR;
            _callback.version_major = Displayconst.DISPLAY_VERSION_MAJOR;

            _callback.DisplayOpen = new display_open_callback(Display_Open);
            _callback.DisplayPreclose = new display_preclose_callback(Display_Preclose);
            _callback.DisplayClose = new display_close_callback(Display_Close);
            _callback.DisplayPresize = new display_presize_callback(Display_Presize);
            _callback.DisplaySize = new display_size_callback(Display_Size);
            _callback.DisplaySync = new display_sync_callback(Display_Sync);
            _callback.DisplayPage = new display_page_callback(Display_Page);
            _callback.DisplayUpdate = null;// new display_update_callback(Display_Update); - Not used

        }

        #region Callback functions. Only Size & Page are handled here

        /// <summary>
        /// Not handled
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private int Display_Open(IntPtr handle, IntPtr device)
        {
            return 0;
        }


        /// <summary>
        /// Not handled
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private int Display_Preclose(IntPtr handle, IntPtr device)
        {
            return 0;
        }


        /// <summary>
        /// On close and dispose the image object
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private int Display_Close(IntPtr handle, IntPtr device)
        {
            return 0;
        }


        /// <summary>
        /// Not handled
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="device"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="raster"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private int Display_Presize(IntPtr handle, IntPtr device, int width, int height, int raster, uint format)
        {
            return 0;
        }


        /// <summary>
        /// This is where the actual image is obtained. This is called twice, once during creation and once after completion. 
        /// Saving this information can be used to convert to Bitmap in display_page, which is the final callback for
        /// each page.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="device"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="raster"></param>
        /// <param name="format"></param>
        /// <param name="pimage"></param>
        /// <returns></returns>
        private int Display_Size(IntPtr handle, IntPtr device, int width, int height, int raster, uint format, IntPtr pimage)
        {
            _srcImage = pimage;
            _raster = raster;
            _rect = new Rectangle(0, 0, width, height);

            return 0;
        }


        /// <summary>
        /// Not handled
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private int Display_Sync(IntPtr handle, IntPtr device)
        {
            return 0;
        }


        /// <summary>
        /// Main callback method to obtain the Bitmap image from the source which is stored when 'Size' Callback is called
        /// The image is UNIX complaint which is inversed and flipped for windows. Hence the bitmap is copied to a temp
        /// buffer and then flipped by calling FlipImageVertical. When the bitmap is ready Signals the DisplayEvent for 
        /// handling the bitmap
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="device"></param>
        /// <param name="copies"></param>
        /// <param name="flush"></param>
        /// <returns></returns>
        private int Display_Page(IntPtr handle, IntPtr device, int copies, int flush)
        {

            _bitmap = new Bitmap(_rect.Width, _rect.Height, PixelFormat.Format24bppRgb);
            if (_bitmap != null)
            {

                BitmapData bmpdata = _bitmap.LockBits(_rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                IntPtr tempBmp = Marshal.AllocHGlobal(_raster * _rect.Height);
                CopyImageToBitmap(_srcImage, tempBmp, _rect, _raster);
                FlipImageVertical(tempBmp, bmpdata.Scan0, bmpdata.Height, bmpdata.Stride);
                Marshal.FreeHGlobal(tempBmp);
                _bitmap.UnlockBits(bmpdata);
            }

            RaiseBitmapAvailableEvent(copies);
            return 0;
        }

        #endregion

        #region Methods used for Rasing Display Events

        /// <summary>
        /// DisplayEvent is raised when a bitmap is ready. A copy of the bitmap and
        /// no. of copies of the page are passed as event variables. A handler can
        /// pick up these variables
        /// </summary>
        /// <param name="page">No. of Copies.</param>
        private void RaiseBitmapAvailableEvent(int page)
        {
            DisplayEvent de = new DisplayEvent
            {
                bmp = _bitmap.Clone() as Bitmap,
                pageNo = page
            };
            OnBitmapReadyHandler(this, de);
        }


        /// <summary>
        /// if handler for GSDisplay event is implemented, then that event is called.
        /// </summary>
        /// <param name="obj">Sender</param>
        /// <param name="e">DisplayEvent variable</param>
        public virtual void OnBitmapReadyHandler(object obj, DisplayEvent e) //Same delegate signature as DisplayEventHandler delegate
        {

            DisplayCallbackEventHandler?.Invoke(this, e);
           
            /* Simplified code is provided above
            DisplayEventHandler handler = DisplayCallbackEventHandler; //Attach calling program handle here
            if (handler != null)
            {
                handler(this, e);
            }
            */
        }

        /// <summary>
        /// Sets the remote Event pointer that handles the raised display event. When an event is raised
        /// it is passed to this pointer.
        /// </summary>
        /// <param name="dh"></param>
        public void SetDisplayEventHandler(DisplayEventHandler dh)
        {
            DisplayCallbackEventHandler = dh;
        }

        #endregion

        #region Helper functions

        /// <summary>
        /// Takes source image flips and inverts that vertically then passes it to destination
        /// </summary>
        /// <param name="tempBmp">Source bitmap pointer</param>
        /// <param name="destBmp">Destination bitmap</param>
        /// <param name="height">Bitmap height</param>
        /// <param name="raster">Raster</param>
        private void FlipImageVertical(IntPtr tempBmp, IntPtr destBmp, int height, int raster)
        {
            int size = height * raster;
            var buffer = new byte[size];
            Marshal.Copy(tempBmp, buffer, 0, size);
            byte[] row = new byte[raster];

            int top = 0;
            int bottom = height - 1;
            int posTop;
            int posBottom;

            while (top <= bottom)
            {
                posTop = top * raster;
                posBottom = bottom * raster;

                Array.Copy(buffer, posTop, row, 0, raster);
                Array.Copy(buffer, posBottom, buffer, posTop, raster);
                Array.Copy(row, 0, buffer, posBottom, raster);

                top++;
                bottom--;
            }

            Marshal.Copy(buffer, 0, destBmp, size);
        }


        /// <summary>
        /// Copy the image to Bitmap by allocating 3 pixels per pixel of the image
        /// </summary>
        /// <param name="srcImage">Source Bitmap</param>
        /// <param name="destImage">Destination Bitmap</param>
        /// <param name="rect">Rectangle of the image</param>
        /// <param name="raster">Raster</param>
        private void CopyImageToBitmap(IntPtr srcImage, IntPtr destImage, Rectangle rect, int raster)
        {
            int bytesPerPixel = 3;
            int destRaster = (((rect.Width * bytesPerPixel) + 3) & ~3);
            int srcTop = 0;
            int destTop = 0;
            int srcBottom = rect.Height - 1;
            int posSrcTop;
            int posDestTop;

            while (srcTop <= srcBottom)
            {
                posSrcTop = (srcTop * (raster)); //+ (0 * bytesPerPixel);
                posDestTop = (destTop * (destRaster));
                MoveMemory(new IntPtr((long)destImage + posDestTop), new IntPtr((long)srcImage + posSrcTop), (uint)(rect.Width * bytesPerPixel));
                srcTop++;
                destTop++;
            }

        }

        /// <summary>
        /// A simple helper function to get the stored Bitmap. This must be called immediately
        /// after a page process. Only useful for single page processing as the Bitmap gets 
        /// rewritten for every page.
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmap()
        {
            return _bitmap.Clone() as Bitmap;
        }

        #endregion
    }
}
