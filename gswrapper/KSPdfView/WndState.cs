//
// WndState.cs
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



using System.Windows;


namespace KSPdfView
{ 
    class  WndState
    {
        static double WidthRatio;
        static double HeightRatio;

        /// <summary>
        /// Called when the initial main window is initialised. Tries to Position the window in the same location as the previous instance. 
        /// Screen Resolution is taken into consideration to ensure the window is not placed outside the screen limit.
        /// This Check will be necessary when dealing with multiple monitors. 
        /// </summary>
        public static void SetWindowState()
        {
            //WindowState Normal = 0, Minimized = 1, Maximized = 2
            if (Properties.Settings.Default.WinState == 1)
            {
                Application.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
                return;
            }

            if (Properties.Settings.Default.WinState == 2)
            {
                Application.Current.MainWindow.WindowState = System.Windows.WindowState.Maximized;
                return;
            }

            if (Properties.Settings.Default.ScreenWidth == 0) //Nothing saved. New entry
                return;

            GetScreenRatio();

            if (Properties.Settings.Default.WinWidth != 0)
                Application.Current.MainWindow.Width = Properties.Settings.Default.WinWidth * WidthRatio;

            if (Properties.Settings.Default.WinHeight != 0)
                Application.Current.MainWindow.Height = Properties.Settings.Default.WinHeight * HeightRatio;

            Application.Current.MainWindow.Left = Properties.Settings.Default.WinPosLeft * WidthRatio;
            Application.Current.MainWindow.Top = Properties.Settings.Default.WinPosTop * HeightRatio;

        }

        /// <summary>
        /// This method is called by SetWindowState. When the Window is loaded, this method calculates current Screen settings 
        /// and stored Screen settings ratio to ensure
        /// the screen co-ordinates are maintained the same as the previous saved one. 
        /// </summary>
        private static void GetScreenRatio()
        {
            WidthRatio = Properties.Settings.Default.ScreenWidth != 0 ? SystemParameters.WorkArea.Width / Properties.Settings.Default.ScreenWidth : 1;
            HeightRatio = Properties.Settings.Default.ScreenHeight != 0 ? SystemParameters.WorkArea.Height / Properties.Settings.Default.ScreenHeight : 1;
        }

        /// <summary>
        /// Saves the current state of the window to the Properties Settings. It also stores the Current Screen Resolution 
        /// If the window spans two screen, the settings will not be saved to avoid off screen window 
        /// </summary>
        public static void SaveWindowState()
        {
            if (Application.Current.MainWindow.Left < 0 || Application.Current.MainWindow.Top < 0)
                return;

            Properties.Settings.Default.ScreenHeight = System.Windows.SystemParameters.WorkArea.Height;
            Properties.Settings.Default.ScreenWidth = System.Windows.SystemParameters.WorkArea.Width;
            Properties.Settings.Default.WinState = (int)Application.Current.MainWindow.WindowState; //windowState Normal = 0, Minimized = 1, Maximized = 2

            //Leave the old value As is if Minimised or Maximised.
            if (Properties.Settings.Default.WinState == 0)
            {
                Properties.Settings.Default.WinPosLeft = Application.Current.MainWindow.Left;
                Properties.Settings.Default.WinPosTop = Application.Current.MainWindow.Top;
                Properties.Settings.Default.WinWidth = Application.Current.MainWindow.Width;
                Properties.Settings.Default.WinHeight = Application.Current.MainWindow.Height;

            }
            Properties.Settings.Default.Save();
        }

    }
}
