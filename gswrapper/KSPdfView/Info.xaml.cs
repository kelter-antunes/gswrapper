//
// Info.xaml.cs
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

/***************************About Info.xaml.cs************************************
 * 
 * Simple dialog window to present the Application information
 * and license information 
 * 
 * ***************************************************************************/

using System.Windows;
using GSWrapper;

namespace KSPdfView
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// Displays License Information
    /// </summary>
    public partial class Info : Window
    {
        public Info()
        {
            InitializeComponent();
            tbkLicense.Text = CONSTANTS.LICENSEINFO;
        }


        /// <summary>
        /// Close the dialog box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
