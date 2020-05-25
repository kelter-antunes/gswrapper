//
// MainWindow.xaml.cs
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


/***************************About MainWindow.xaml.cs************************************
 * 
 * Main Application. This application demonstrates the functionalities of GSWrapper DLL - 
 * a c# wrapper for Ghostscript Application. The application exploits two main functinalities:
 * A display Event handler and Message event handler. By sucessfully implementing the 
 * callback procedures, PDF files can be easily viewed and Postscript commands can be run. 
 * This application demonstrates and uses following Ghostscript functions:
 *      gsapi_run_string_begin
 *      gsapi_run_string_continue
 *      gsapi_run_string_end
 *      gsapi_run_string
 * The wrapper for these calls are provided in GSInteractive class part of GSWrapper Namespace  
 * 
 * ***************************************************************************/


using KSPdfView.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GSWrapper;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.IO;


namespace KSPdfView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region StdIO & Display Handlers

        readonly StdioMessageEventHandler _stdIn = null;
        readonly StdioMessageEventHandler _stdOut = null;
        readonly StdioMessageEventHandler _stdErr = null;

        readonly GSInteractive _gsInteractive;
        readonly DisplayEventHandler _displayHandle;

        #endregion

        #region Variables used by KSPdfView

        const int MINIMUMWIDTH = 300;

        private bool bCancel;
        private bool bDevelop;
        private bool bpsFileOpen;

        GSAPI_REVISION_S _gsVersion;
        readonly private Dictionary<int, System.Drawing.Bitmap> _pageBitmapList = new Dictionary<int, System.Drawing.Bitmap>();

        private string _message = "";
        private int _totalPages;
        private int _pageCount;

        #endregion



        /// <summary>
        /// Main Window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _stdIn = new StdioMessageEventHandler(OnStdIoInput);
            _stdOut = new StdioMessageEventHandler(OnStdIoOutput);
            _stdErr = new StdioMessageEventHandler(OnStdIoError);
            _displayHandle = new DisplayEventHandler(GsInteractive_displayEvent);
            _gsInteractive = new GSInteractive();
            if (!_gsInteractive.GSInitialise(_stdIn, _stdOut, _stdErr, _displayHandle))
            {
                MessageBox.Show("Ghostscript DLL not found or not of the right version. Make sure latest version of Ghostscript is installed.\nExiting the Application");
                this.Close();
                return;
            }
            _gsVersion = _gsInteractive.GetRevisionInfo();
            tbkStatus.Text = string.Format("Ghostscript Version Found: {0} Dated: {1}", _gsVersion.revision, _gsVersion.revisiondate);

            SetDevelopPanel(Settings.Default.DevelopPanel);
        }


        #region Main GUI Routed events Handlers

        /// <summary>
        /// When the window is initialised, just before the layout is getting updated
        /// use the stored value for position and size. this will ensure persistence
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void KSMain_Initialized(object sender, EventArgs e)
        {
            WndState.SetWindowState();
        }


        /// <summary>
        /// Reverse of Main Window initialised. When exiting save the position
        /// and size of the window.
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void KSMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bCancel = true;

            SaveDevelopPanel();
            WndState.SaveWindowState();

        }

        /// <summary>
        /// Called when File Open button is clicked. Opens dialog for 
        /// opening PDF documents. Stores the selected folder for persistence
        /// </summary>
        /// <param name="sender">Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnFileOpen_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "PDF files (PDF Files|*.pdf",

                InitialDirectory = Settings.Default.WorkingFolder,
                RestoreDirectory = true,
                Title = "Open PDF File"
            };

            if (ofd.ShowDialog() == true)
            {
                Settings.Default.WorkingFolder = ofd.FileName.Substring(0, ofd.FileName.LastIndexOf('\\'));
                Settings.Default.Save();
                OpenPDFDocument(ofd.FileName);
                return;
            }
        }


        /// <summary>
        /// Cancels lengthy operaion
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            bCancel = true;
        }


        /// <summary>
        /// Called when Exit Button is clicked. Exits the program 
        /// by calling Close(), which is handled by RoutdEventArgs Closing()
        /// </summary>
        /// <param name="sender">Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        /// <summary>
        /// Slider Control to zoom the image. Zoom out is handled by pressing '-' button
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (slImage.Value > slImage.Minimum)
                slImage.Value -= slImage.SmallChange;
        }


        /// <summary>
        /// Handles zooming of image based on value obtained from zoom slider.
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void SlImage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (imgPDF != null)
            {
                double dblImageRatio = slImage.Value / 100;
                cvImage.Width = imgPDF.ActualWidth * dblImageRatio;
                cvImage.Height = imgPDF.ActualHeight * dblImageRatio;
            }
        }


        /// <summary>
        /// Slider Control to zoom the image. Zoom in is handled by pressing '+' button
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (slImage.Value < slImage.Maximum)
                slImage.Value += slImage.SmallChange;
        }


        /// <summary>
        /// Slider Control to navigate PDF pages. Previous page navigation is handled by pressing 'Prev' button
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (slPage.Value > slPage.Minimum)
                slPage.Value -= slPage.SmallChange;
        }


        /// <summary>
        /// Slider Control to Navigate the PDF Pages. Proper page is displayed based
        /// on value obtained from Page Slider
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void SlPage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (hdrPDFPages == null)
                return;

            BitmapSource bp = GetBitmapImage((int)slPage.Value);
            if (bp == null)
            {
                if (e.OldValue <= 0 && _pageBitmapList.Count == 0)
                    hdrPDFPages.Header = "PDF Pages";
                else
                    slPage.Value = e.OldValue;
                e.Handled = true;
            }
            else
            {
                SetImage(bp);
            }
        }


        /// <summary>
        /// Slider Control to navigate PDF pages. Next page navigation is handled by pressing 'Next' button
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (slPage.Value < slPage.Maximum)
                slPage.Value += slPage.SmallChange;
        }


        /// <summary>
        ///  Handles during loading of the image. zooming of image is based on value obtained from zoom slider.
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void ImgPDF_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double dblImageRatio = slImage.Value / 100;

            cvImage.Width = imgPDF.ActualWidth * dblImageRatio;
            cvImage.Height = imgPDF.ActualHeight * dblImageRatio;
        }


        /// <summary>
        /// Prepares the development environment or cancels the environment
        /// The grids are adjusted so the developer panel is hidden when not
        /// needed. Resets all file values and operations
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnDevelop_Click(object sender, RoutedEventArgs e)
        {
            if (btnDevelop.IsChecked == false)
            {
                Settings.Default.ColTextWidth = colText.Width;
                colText.MinWidth = 0;
                colText.Width = new GridLength(0);
                gsLeftImage.Visibility = Visibility.Collapsed;
                btnFileOpen.IsEnabled = true;
                bDevelop = false;
            }
            else
            {              
                _gsInteractive.RunStringBegin();
                _gsInteractive.RunStringContinue(CONSTANTS.CLEARSTACK);
                bCancel = true; //cancel any pending operation
                tbkStatus.Text = "Debug Window Setup Completed";
                colText.Width = Settings.Default.ColTextWidth;
                colText.MinWidth = MINIMUMWIDTH;
                gsLeftImage.Visibility = Visibility.Visible;
                btnFileOpen.IsEnabled = false;
                bDevelop = true;
            }
            slPage.Minimum = 1;
            slPage.Maximum = 1;
            slPage.Value = 1;
            cvImage.Children.Clear();
            _pageCount = 0;
            _totalPages = 0;
            _message = "";
            
            hdrPDFPages.Header = string.Format("Pages: {0} of {1}", slPage.Value, slPage.Maximum);
        }


        /// <summary>
        /// Displays Information about the application and License
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void BtnLicense_Click(object sender, RoutedEventArgs e)
        {
            
            Info In = new Info
            {
                Owner = this
            };
            In.ShowDialog();
            
        }

        /// <summary>
        /// Handles when Output text are changed. Scroll to bottom so the latest 
        /// output is visible
        /// </summary>
        /// <param name="sender">>Object sender - Not used</param>
        /// <param name="e">Routed Events - not used</param>
        private void TbOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbOutput.ScrollToEnd();
        }



        #endregion


        #region Private Methods

        /// <summary>
        /// This is simple hack to refresh the UI while batch process is called. 
        /// Picked the code from https://stackoverflow.com/questions/4502037/where-is-the-application-doevents-in-wpf
        /// This hack works better than using batch process Async and then adding await Task(100) to wait for 100ms. 
        /// While the MessageBox waits for user to decide cancel or continue, the Async method pushes another file causing message box to appear twice.
        /// This hack solves the problem as the process runs synchronously.
        /// </summary>
        private static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(
                    delegate (object f)
                    {
                        ((DispatcherFrame)f).Continue = false;
                        return null;
                    }), frame);
            Dispatcher.PushFrame(frame);
        }




        /// <summary>
        /// Called when a PDF file is opened. This method calls OpenDocument
        /// to get the page details and calls further processing in the 
        /// background if the pages are more than 1
        /// </summary>
        /// <param name="fileName">Name of the file that is opened</param>
        private void OpenPDFDocument(string fileName)
        {
            int pages = OpenDocument(fileName);

            if (pages <= 0)
                MessageBox.Show(string.Format("Unable to open the PDF document: {0}", fileName.Substring(fileName.LastIndexOf('\\'))));
            else
            {
                slPage.Value = 1;
                slPage.Maximum = pages;
                SetImage(GetBitmapImage(1));
                pbrProgressStatus.Maximum = pages;
                pbrProgressStatus.Value = 1;
                _totalPages = pages;
                if (_totalPages > 1)
                    CallBackgroundProcessing();
            }
        }


        /// <summary>
        /// Using the bitmapsource sets the Image in the canvas
        /// </summary>
        /// <param name="bitmapSource">Bitmap source of the image</param>
        private void SetImage(BitmapSource bitmapSource)
        {
            cvImage.Children.Clear();
            imgPDF.Source = bitmapSource;
            cvImage.Children.Add(imgPDF);
            hdrPDFPages.Header = string.Format("Pages: {0} of {1}", slPage.Value, slPage.Maximum);
        }


        /// <summary>
        /// Updates the status of the operations. Since the Stdio call back are handled here
        /// The callback procedures will call this method. The status bar is updated with 
        /// the current process of the app
        /// </summary>
        /// <param name="status">String Value that is passed by calling program</param>
        /// <param name="value">Int Value passed by calling program</param>
        private void UpdateProgress(string status, int value)
        {
            //We need to handle two process. Process complete and Process Cancelled. 
            if (status == CONSTANTS.CANCELLEDPROCESS)
            {
                tbkStatus.Text = string.Format(string.Format("{0}. Total pages processed: {1} ", CONSTANTS.CANCELLEDPROCESS, value));
                _pageCount = value;
            }
            else
            {
                pbrProgressStatus.Value = value;
                btnCancel.IsEnabled = true;

                //This automatically handles process complete.
                if (value >= _totalPages)
                {
                    tbkStatus.Text = "Processed all the pages of PDF file";
                    btnCancel.IsEnabled = false;
                }
                else
                    tbkStatus.Text = string.Format("Processing PDF file in background. Completed % : {0:0.##}", (value / (double)_totalPages) * 100);
            }
        }


        /// <summary>
        /// Develop Panel is set or reset. Note: The output display size depends on
        /// previous window settings. Need to send string for correct size before proceeding
        /// </summary>
        private void SetDevelopPanel(bool bPanelSet)
        {
            if (bPanelSet)
            {
                _gsInteractive.RunStringBegin();
                bCancel = true; //Cancel Any pending process
                tbkStatus.Text = "Debug Window Ready";
                colText.Width = Settings.Default.ColTextWidth;
                rowInput.Height = Settings.Default.RowInputHeight;
                rowErrOut.Height = Settings.Default.RowErrorHeight;
                btnDevelop.IsChecked = true;
                gsLeftImage.Visibility = Visibility.Visible;
                btnFileOpen.IsEnabled = false;
                bDevelop = true;
            }
            else
            {
                gsLeftImage.Visibility = Visibility.Collapsed;
                btnFileOpen.IsEnabled = true;
                colText.MinWidth = 0;
                colText.Width = new GridLength(0);
                bDevelop = false;
            }
            _message = "";
        }


        /// <summary>
        /// Saves the size of the Develop panel for persistence
        /// </summary>
        private void SaveDevelopPanel()
        {

            if (bDevelop)
            {
                Settings.Default.ColTextWidth = colText.Width;
                Settings.Default.DevelopPanel = true;
            }
            else
            {
                Settings.Default.DevelopPanel = false;
            }
            Settings.Default.RowInputHeight = rowInput.Height;
            Settings.Default.RowErrorHeight = rowErrOut.Height;
        }


        #endregion


        #region OnStdIoCallback and Display Event Handler functions


        /// <summary>
        /// Input handler. Callback function for Ghostscript dll. Not used.
        /// </summary>
        /// <param name="handle">Dll Instance handle</param>
        /// <param name="pointer">Pointer to input string read from standard Input</param>
        /// <param name="count">Total characters read</param>
        /// <returns>zero if successful</returns>
        private int OnStdIoInput(IntPtr handle, IntPtr pointer, int count)
        {
            return count;
        }


        /// <summary>
        /// Output handler. Callback function for Ghostscript dll.
        /// Output messages are Stored in _message variable and then
        /// parsed for total pages 
        /// </summary>
        /// <param name="handle">Dll Instance handle</param>
        /// <param name="pointer">Pointer to output string from DLL</param>
        /// <param name="count">Total characters output</param>
        /// <returns>zero if successful</returns>
        private int OnStdIoOutput(IntPtr handle, IntPtr pointer, int count)
        {
            string output = Marshal.PtrToStringAnsi(pointer, count);
           _message += output;
            if (btnDevelop.IsChecked == true)
                tbOutput.Text += _message;

            return count;
        }


        /// <summary>
        /// This method is provided as an example on how to parse text received from stdio Output.
        /// The method parses the string for TOTALPAGES / PROCESSPERCENTAGE  and then looks for the value
        /// then converts that to into int / double and stores in global variable. The messages
        /// are stored in output window: tbOutput
        /// </summary>
        /// <param name="str">Input string to parse</param>
        private int ParseOutputMessage(string str)
        {
            int val = 0;
            try
            {
                string[] t = str.Split(CONSTANTS.NEWLINE);
                if (t.Length > 0)
                    val = Int32.Parse(t[0].Trim());
            }
            catch (Exception)
            {
                val = 0;
            }
                
          
            return val;
        }

        /// <summary>
        /// Error message handler. Callback function for Ghostscript dll.
        /// Error messages are output to Error window: tbError 
        /// </summary>
        /// <param name="handle">Dll Instance handle</param>
        /// <param name="pointer">Pointer to Error string from DLL</param>
        /// <param name="count">Total characters output</param>
        /// <returns>zero if successful</returns>
        private int OnStdIoError(IntPtr handle, IntPtr pointer, int count)
        {
            string message = Marshal.PtrToStringAnsi(pointer);
            if (bDevelop)
                tbError.Text += message;
            return count;
        }


        /// <summary>
        /// Handler for display event. This is raised from GSDisplay.cs whenever a bitmap 
        /// is ready to be presented. The event handler has two variables. Bitmap - which
        /// is used for presentation and no. of copies of the image -(this is not used) 
        /// </summary>
        /// <param name="obj">Object of the sender</param>
        /// <param name="e">Contains two variables, bitmap and no. of copies</param>
        private void GsInteractive_displayEvent(object obj, DisplayEvent e)
        {

            if (e.bmp != null)
            {
                if (bDevelop)
                {
                    _pageCount++;
                    _pageBitmapList[_pageCount] = e.bmp;
                    _totalPages = _pageCount;
                    slPage.Maximum = _pageCount;
                    slPage.Minimum = 1;
                    slPage.Value = _pageCount;
                    SetImage(GetBitmapImage(_pageCount));

                }
                else
                {
                    _pageBitmapList[_pageBitmapList.Count+1] = e.bmp;

                }
            }
        }

        #endregion


        #region File and Bitmap operations


        /// <summary>
        /// retrieves the image from the Dictionary list. Each page is stored as bitmap
        /// in a Dictionary list with page no and bitmap
        /// </summary>
        /// <param name="page">Page no. of the bitmap to be retrieved</param>
        /// <returns>Bitmapsource</returns>
        private BitmapSource GetBitmapImage(int page)
        {
            if (_pageBitmapList.Count == 0 || _pageBitmapList.Count < page || page <= 0)
                return null;
            return GetImageStream(_pageBitmapList[page]);
        }


        /// <summary>
        /// Converts Bitmap to Bitmap Source which is used as Image for canvas
        /// </summary>
        /// <param name="bitmap">Bitmap which is output from DLL</param>
        /// <returns>Bitmap source that can be used to set as Image</returns>
        private BitmapSource GetImageStream(System.Drawing.Bitmap bitmap)
        {

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
                
        }

        /// <summary>
        /// Called by OpenPDF document. Passes the input file for processing to DLL.
        /// Gets the total pages and sets the first image of the page. Other pages are 
        /// processed in the background
        /// </summary>
        /// <param name="filename">Filename of the PDF file</param>
        /// <returns>Zero if unable to open or total pages if successful</returns>
        private int OpenDocument(string filename)
        {
            _totalPages = 0;
            _message = "";
            _pageCount = 0;
            _pageBitmapList.Clear();
            slPage.Minimum = 1;
            slPage.Maximum = 1;
            slPage.Value = 1;
            bCancel = false;
            filename = filename.Replace("\\", "/"); //filename are used Linux / Unix complaint. Check Ghostscript document

            if (_gsInteractive.RunString(string.Format("({0}) (r) file runpdfbegin pdfpagecount = quit\n ", filename))!=CONSTANTS.NOERROR)
                return _gsInteractive.GetLastError();  //need to report error message

            //Get PageNos - No error check required since this gives 'type check error'
            _gsInteractive.RunString(string.Format(" ({0} ) pdfpagecount show \n ", CONSTANTS.TOTALPAGES));
            _totalPages = ParseOutputMessage(_message);
            
            if (_totalPages == 0)
                return 0;

            //Get the first page for display 
            _pageCount = 1;
            _gsInteractive.RunString(" 1 1 dopdfpages ");
            pbrProgressStatus.Value = 1;
            SetImage(GetBitmapImage(1));

            return _totalPages;
            
        }

       
        /// <summary>
        /// Background processing of rest of the pages of PDF document.
        /// Two options available. 
        /// First option is 12% faster but visual feedback & Cancel process to be worked out.
        /// Second option slower but provides visual feedback
        /// </summary>
        public void CallBackgroundProcessing()
        {
            /* - 12% Faster but need to work out visual feedback & Cancel process */
            /*
            CancellationTokenSource source = new CancellationTokenSource();
            source.Token.Register(CancelNotification);
            Task test = new Task(async () => await Backgroundprocessing(), source.Token);
            test.Start();
            */
            DoEvents();
            while (_pageCount < _totalPages)
            {
                if (bCancel)
                {
                    UpdateProgress(CONSTANTS.CANCELLEDPROCESS, _pageCount);
                    break;
                }
                _pageCount++;

                _gsInteractive.RunString(string.Format(" {0} {0} dopdfpages", _pageCount));
                
                UpdateProgress(CONSTANTS.PROGRESSPROCESS, _pageCount);
                DoEvents();
            }
            //*/

        }

        #region Alternate method of processing 12% faster. Not implemented

        /*
        /// <summary>
        /// This process runs all the pages at one stretch. Cancel process
        /// and visual feedback need to be worked out
        /// </summary>
        /// <returns></returns>
        public async Task<int>  Backgroundprocessing()
        {

            await Task.FromResult(_gsInteractive.RunString(string.Format(" 2 {1} dopdfpages", _pageCount, _totalPages)));
            return 0;
        }


        /// <summary>
        /// Use this notification to cancel the async process. bCancel can
        /// be used to raise a CancelNotification.
        /// </summary>
        private static void CancelNotification()
        {
            //Handle cancel notification
        }
        */

        #endregion

        #endregion


        #region Interactive Operations - Developer Panel

        /// <summary>
        /// Opens the file dialog box to load the postscript file.
        /// The script is loaded on to Input window and can be edited
        /// Use send button to run the script
        /// </summary>
        /// <param name="sender">Object of the sender - Not used</param>
        /// <param name="e">Routed event handler - Not used</param>
        private void BtnPSOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Post Script files (PS Files|*.ps",

                InitialDirectory = Settings.Default.PSWorkingFolder,
                RestoreDirectory = true,
                Title = "Open Postscript File"
            };

            if (ofd.ShowDialog() == true)
            {
                Settings.Default.PSWorkingFolder = ofd.FileName.Substring(0, ofd.FileName.LastIndexOf('\\'));
                Settings.Default.Save();
                StreamReader sr = new StreamReader(ofd.FileName);
                tbInput.Text = sr.ReadToEnd();
                sr.Close();
                bpsFileOpen = true;
                if (_gsInteractive != null)
                    _gsInteractive.RunStringContinue(CONSTANTS.CLEARSTACK);
                return;
            }
        }


        /// <summary>
        /// Single line command input. Pressing Enter will send the command direct.
        /// Can be used for testing purpose. Comment out if not required.
        /// </summary>
        /// <param name="sender">Object of the sender raising the event</param>
        /// <param name="e">Event argument passed. In this case KeyEventArgs</param>
        private void TbInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !bpsFileOpen)
            {
                ActionCommand(tbInput.Text);
                tbInput.Text = "";
                e.Handled = true;
            }
        }

        /// <summary>
        /// Clears the text and also clears the Dict buffer of Dll
        /// </summary>
        /// <param name="sender">Object of the sender raising the event - Not used</param>
        /// <param name="e">Event argument passed. Not used</param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            tbInput.Text = "";
            if (_gsInteractive != null)
                _gsInteractive.RunStringContinue(CONSTANTS.CLEARSTACK);

            bpsFileOpen = false;
        }


        /// <summary>
        /// Since <Enter> could be use to edit the script. To avoid conflict this button
        /// is used to send the PS file as text for running
        /// </summary>
        /// <param name="sender">Object of the sender - Not used</param>
        /// <param name="e">Routed event handler - Not used</param>
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            ActionCommand(tbInput.Text);
        }


        /// <summary>
        /// Runs postscript command. from debug window. Passes the text to the DLL.
        /// Since this is in interactive mode, can be used to test the code. Output
        /// of text is sent to Output window while any bitmap output will be  displayed 
        /// </summary>
        /// <param name="text">Postscript command text</param>
        private void ActionCommand(string text)
        {
            _message = "";
            bDevelop = true;
            if (_gsInteractive != null)
                _gsInteractive.RunStringContinue(text + CONSTANTS.NEWLINE);

        }


        #endregion


    }
}
