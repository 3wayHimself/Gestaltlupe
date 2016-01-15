﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Fractrace.Basic;
using Fractrace.PictureArt;

namespace Fractrace
{

    /// <summary>
    /// Control which displays the rendered image.
    /// </summary>
    public partial class ResultImageView : Form, IAsyncComputationStarter
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ResultImageView"/> class.
        /// </summary>
        public ResultImageView()
        {
            InitializeComponent();
            //            object o = FileSystem.Exemplar;
            GlobalParameters.SetGlobalParameters();
            _paras = new ParameterInput();
            _paras.Show();
            PublicForm = this;
        }


        /// <summary>
        /// The input control.
        /// </summary>
        private ParameterInput _paras = null;

        /// <summary>
        /// Unique Instance of this Window.
        /// </summary>
        public static ResultImageView PublicForm = null;

        /// <summary>
        /// This graphic is used to generate the bitmap. 
        /// </summary>
        Graphics _graphics = null;

        /// <summary>
        /// Return Graphics of the computed gestalt.
        /// </summary>
        public Graphics GestaltPicture { get { return _graphics; } }

        /// <summary>
        /// With of the bitmap.
        /// </summary>
        int _width = 0;

        /// <summary>
        /// Heigth of the Bitmap.
        /// </summary>
        int _height = 0;

        /// <summary>
        ///  Get current computing algorithm of the surface data of the "Gestalt". 
        /// </summary>
        public Iterate Iterate { get { return _iterate; } }
        /// <summary>Current computing algorithm of the surface data of the "Gestalt". </summary>
        Iterate _iterate = null;

        /// <summary>
        /// Copy of iter for using in picture art. 
        /// </summary>
        public Iterate IterateForPictureArt { get { return _iterateForPictureArt; } }
        Iterate _iterateForPictureArt = null;

        /// <summary>
        /// Zoom Area
        /// </summary>
        int _zoomX1 = 0, _zoomX2 = 0, _zoomY1 = 0, _zoomY2 = 0;

        /// <summary>
        /// True, while left mouse button is pressed.
        /// </summary>
        private bool _inMouseDown = false;

        /// <summary>
        /// The Hash of the Parameters of the last rendering (but without picture art settings).
        /// </summary>
        protected string _oldParameterHashWithoutPictureArt = "";

        /// <summary>
        /// The Hash of the Parameters of the last rendering (but without picture art settings and navigation).
        /// </summary>
        protected string _oldParameterHashWithoutPictureArtAndNavigation = "";

        /// <summary>
        /// Delegate for the OneStepEnds event.
        /// </summary>
        delegate void OneStepEndsDelegate();

        /// <summary>
        /// If false, a warning message is shown before closing the application.
        /// </summary>
        protected bool _forceClosing = false;

        /// <summary>
        /// Progress of surface computation in percent.
        /// </summary>
        protected double _progress = 0;

        /// <summary>
        /// If true, mProgress has changed but progress is not updated.
        /// </summary>
        protected bool _progressChanged = false;

        /// <summary>
        /// Used to save inPaint thread while updating.
        /// </summary>
        object _paintMutex = new object();

        /// <summary>
        /// Used to save inPaint thread while updating first preview.
        /// </summary>
        object _firstPaintMutex = new object();

        /// <summary>
        /// True while method Paint() runs.
        /// </summary>
        bool _inPaint = false;

        /// <summary>
        /// True, if after end of DrawPicture() a new paint request should be startet. 
        /// </summary>
        bool _repaintRequested = false;

        /// <summary>
        /// Count the number of update steps
        /// </summary>
        int _updateCount = 0;

        /// <summary>
        /// Get or set the number of pictures, which was updated on the same dataset.
        /// </summary>
        public int CurrentUpdateStep { set { _currentUpdateStep = value; } get { return _currentUpdateStep; } }
        /// <summary> Count the number of pictures, wich was updated on the same dataset. </summary>
        int _currentUpdateStep = 0;

        /// <summary>
        /// Different handling of Progress Bar while in preview.
        /// </summary>
        public bool _inPreview = false;

        /// <summary>
        /// Currently running picture art.
        /// </summary>
        public Renderer LastPicturArt { get { return _lastPicturArt; } }
        Renderer _lastPicturArt = null;

        Renderer _currentPicturArt = null;

        /// <summary>
        /// Get true, if the surface data generation is running. 
        /// </summary>
        public bool InComputation { get { return Scheduler.GrandScheduler.Exemplar.inComputeOneStep; } }

        /// <summary>
        /// Delegate for the drawEnds event.
        /// </summary>
        delegate void DrawImageDelegate();

        /// <summary>
        /// To determine, if parameter has changed since last saved animation step.
        /// </summary>
        string _lastAnimationParameterHash = "";

        /// <summary>
        /// Sets the size of the picture box and the corresponding image from settings.
        /// </summary>
        public void SetPictureBoxSize()
        {
            double widthInPixel = ParameterDict.Current.GetDouble("View.Width");
            double heightInPixel = ParameterDict.Current.GetDouble("View.Height");
            int sizeX = (int)(widthInPixel * _paras.ScreenSize);
            int sizeY = (int)(heightInPixel * _paras.ScreenSize);
            if (_width != sizeX || _height != sizeY)
            {
                _width = sizeX;
                _height = sizeY;
                pictureBox1.Width = _width;
                pictureBox1.Height = _height;
                Image labelImage = new Bitmap((int)(_width), (int)(_height));
                pictureBox1.Image = labelImage;
                _graphics = Graphics.FromImage(labelImage);
            }
        }


        /// <summary>
        /// Load bitmap from the given file and displays it.
        /// </summary>
        /// <param name="fileName"></param>
        public void ShowPictureFromFile(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                SetPictureBoxSize();
                pictureBox1.Image = Image.FromFile(fileName);
                _graphics = Graphics.FromImage(pictureBox1.Image);
                this.Refresh();
                this.WindowState = FormWindowState.Normal;
            }
        }


        /// <summary>
        /// Computes the hash of all Parameter entries which are used in 
        /// rendering, but not in picture art.
        /// </summary>
        /// <returns></returns>
        protected string GetParameterHashWithoutPictureArt()
        {
            StringBuilder tempHash = new StringBuilder();
            tempHash.Append(ParameterDict.Current.GetHash("View.Size"));
            tempHash.Append(ParameterDict.Current.GetHash("View.Perspective"));
            tempHash.Append(ParameterDict.Current.GetHash("View.Width"));
            tempHash.Append(ParameterDict.Current.GetHash("View.Height"));
            tempHash.Append(ParameterDict.Current.GetHash("View.Deph"));
            tempHash.Append(ParameterDict.Current.GetHash("View.DephAdd"));
            tempHash.Append(ParameterDict.Current.GetHash("View.PosterX"));
            tempHash.Append(ParameterDict.Current.GetHash("View.PosterZ"));
            tempHash.Append(ParameterDict.Current.GetHash("Border"));
            tempHash.Append(ParameterDict.Current.GetHash("Transformation"));
            tempHash.Append(ParameterDict.Current.GetHash("Formula"));
            tempHash.Append(ParameterDict.Current.GetHash("Intern.Formula"));
            // The following categories are not used: 
            // Composite
            // Computation.NoOfThreads
            return tempHash.ToString();
        }


        /// <summary>
        /// Create surface model.
        /// </summary>
        public void ComputeOneStep()
        {
            if (_paras != null)
                _paras.InComputing = true;
            this.WindowState = FormWindowState.Normal;
            if (Scheduler.GrandScheduler.Exemplar.inComputeOneStep)
                return;
            try
            {
                Scheduler.GrandScheduler.Exemplar.inComputeOneStep = true;
                SetPictureBoxSize();
                string tempParameterHash = GetParameterHashWithoutPictureArt();
                _paras.Assign();
                if (_oldParameterHashWithoutPictureArt == tempParameterHash)
                {
                    // Update last render for better quality
                    _currentUpdateStep++;
                    DataTypes.GraphicData oldData = null;
                    DataTypes.PictureData oldPictureData = null;
                    if (_iterate != null && !_iterate.InAbort)
                    {
                        oldData = _iterate.GraphicInfo;
                        oldPictureData = _iterate.PictureData;
                    }
                    _iterate = new Iterate(_width, _height, this, false);
                    _updateCount++;
                    _iterate.SetOldData(oldData, oldPictureData, _updateCount);
                    if (!ParameterDict.Current.GetBool("View.Pipeline.UpdatePreview"))
                        _iterate._oneStepProgress = _inPreview;
                    else
                        _iterate._oneStepProgress = false;
                    if (_updateCount > ParameterDict.Current.GetDouble("View.UpdateSteps") + 1)
                        _iterate._oneStepProgress = true;
                    _iterate.StartAsync(_paras.Parameter, _paras.Cycles, _paras.ScreenSize, _paras.Formula, ParameterDict.Current.GetBool("View.Perspective"));
                }
                else
                {
                    // Initiate new rendering
                    {
                        // Stop subrendering, if some formula parameters changed
                        if (_currentUpdateStep > 0)
                        {
                            _currentUpdateStep = 0;
                            if (_paras != null)
                                _paras.InComputing = false;
                            Scheduler.GrandScheduler.Exemplar.inComputeOneStep = false;
                            _oldParameterHashWithoutPictureArt = "";
                            _updateCount = 1;
                        }
                        _oldParameterHashWithoutPictureArt = tempParameterHash;
                        _paras.Assign();
                        _updateCount = 1;
                        _iterate = new Iterate(_width, _height, this, false);
                        _iterate._oneStepProgress = false;
                        _iterate.StartAsync(_paras.Parameter, _paras.Cycles, _paras.ScreenSize, _paras.Formula, ParameterDict.Current.GetBool("View.Perspective"));
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                if (_paras != null)
                    _paras.InComputing = false;
                Scheduler.GrandScheduler.Exemplar.inComputeOneStep = false;
            }
        }


        /// <summary>
        /// Raise the event "the asynchrone computation has ended".
        /// </summary>
        public void ComputationEnds()
        {
            _iterateForPictureArt = _iterate;
            Scheduler.GrandScheduler.Exemplar.ComputeOneStepEnds();
            try
            {
                this.Invoke(new OneStepEndsDelegate(OneStepEnds));
            }
            catch { }
        }


        /// <summary>
        /// Asyncrone computation is ready (but not the generation of the bitmap).
        /// </summary>
        protected void OneStepEnds()
        {
            Scheduler.GrandScheduler.Exemplar.inComputeOneStep = false;
            if (_paras != null)
                _paras.InComputing = false;
        }


        /// <summary>
        /// Get internal image data.
        /// </summary>
        /// <returns></returns>
        public Image GetImage()
        {
            return pictureBox1.Image;
        }


        /// <summary>
        /// Show parameter window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _paras.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Always zoom
            // if (_inZoom)
            {
                _zoomX1 = e.X;
                _zoomY1 = e.Y;
                _inMouseDown = true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_inMouseDown)
            {
                _zoomX2 = e.X;
                _zoomY2 = e.Y;
                Pen pen = new Pen(Color.Black);
                // Sorry, this Rectangle is not shown
                _graphics.DrawRectangle(pen, _zoomX1, _zoomY1, _zoomX2 - _zoomX1, _zoomY2 - _zoomY1);
            }
        }


        /// <summary>
        /// Transfer of zooming parameters ends.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_inMouseDown)
            {
                _zoomX2 = e.X;
                _zoomY2 = e.Y;
                _inMouseDown = false;
                SetZoom();
            }
        }


        /// <summary>
        /// Activate zooming.
        /// </summary>
        private void SetZoom()
        {
            if (_iterate == null)
            {
                return;
            }

            try
            {
                // compute Min and Max
                double minX = double.MaxValue;
                double minY = double.MaxValue;
                double minZ = double.MaxValue;

                double maxX = double.MinValue;
                double maxY = double.MinValue;
                double maxZ = double.MinValue;

                if (_zoomX2 - _zoomX1 < 4)
                {
                    _zoomX1 -= _width / 10;
                    _zoomX2 += _width / 10;
                    if (_zoomX1 < 0)
                        _zoomX1 = 0;
                    if (_zoomX2 >= _width)
                        _zoomX2 = _width - 1;
                    _zoomY1 -= _height / 10;
                    _zoomY2 += _height / 10;
                    if (_zoomY1 < 0)
                        _zoomY1 = 0;
                    if (_zoomY2 >= _height)
                        _zoomY2 = _height - 1;
                }

                //  iter.PictureData.Points
                double x, y, z;
                int noOfPixelsFound = 0;
                for (int i = _zoomX1; i <= _zoomX2; i++)
                {
                    for (int j = _zoomY1; j <= _zoomY2; j++)
                    {
                        if (_iterate.GraphicInfo.PointInfo[i, j] != null)
                        {
                            x = _iterate.GraphicInfo.PointInfo[i, j].i;
                            y = _iterate.GraphicInfo.PointInfo[i, j].j;
                            z = _iterate.GraphicInfo.PointInfo[i, j].k;

                            Geometry.Vec3 trans2 = _iterate.LastUsedFormulas.GetTransform(x, y, z);
                            x = trans2.X;
                            y = trans2.Y;
                            z = trans2.Z;

                            if (minX > x)
                                minX = x;
                            if (maxX < x)
                                maxX = x;
                            if (minY > y)
                                minY = y;
                            if (maxY < y)
                                maxY = y;
                            if (minZ > z)
                                minZ = z;
                            if (maxZ < z)
                                maxZ = z;

                            noOfPixelsFound++;
                        }
                    }
                }

                if (noOfPixelsFound < 5)
                {
                    System.Diagnostics.Debug.WriteLine("No Pixel found in zoom.");
                    return;
                }
                // Set parameters:
                _paras.Parameter.start_tupel.x = minX;
                _paras.Parameter.start_tupel.y = minY;
                _paras.Parameter.start_tupel.z = minZ;

                _paras.Parameter.end_tupel.x = maxX;
                _paras.Parameter.end_tupel.y = maxY;
                _paras.Parameter.end_tupel.z = maxZ;

                _paras.Parameter.TransferToParameterDict();

                _paras.UpdateFromData();
                ParameterInput.MainParameterInput.DrawSmallPreview();
            }
            catch (System.Exception ex)
            {
                // Array out of bounds
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// Stops the surface generation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Stop()
        {
            _currentUpdateStep = ParameterDict.Current.GetInt("View.UpdateSteps") + 1;
            if (_iterate != null)
            {
                _iterate.Abort();
            }
            Progress(0);
            // Warning: Compution in stereo window is not stopped.
        }


        /// <summary>
        /// Generates the bitmap and save it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRepaint_Click(object sender, EventArgs e)
        {
            ActivatePictureArt();
        }


        /// <summary>
        /// The surface data is analysed. The generation of the corresponding bitmap starts here .
        /// </summary>
        public void ActivatePictureArt()
        {
            try
            {
                if (_iterate != null && !_iterate.InAbort)
                {
                    System.Threading.ThreadStart tStart = new System.Threading.ThreadStart(DrawPicture);
                    System.Threading.Thread thread = new System.Threading.Thread(tStart);
                    Scheduler.GrandScheduler.Exemplar.AddThread(thread);
                    thread.Start();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// Create PictureArt and use it to paint the current picture according to iter.
        /// </summary>
        void DrawPicture()
        {
            if (_inPaint)
            {
                lock (_firstPaintMutex)
                {
                    if (_currentPicturArt != null)
                    {
                        if (!IsSubStepRendering())
                        {
                            _currentPicturArt.StopAndWait();
                            _lastPicturArt = _currentPicturArt;
                            _currentPicturArt = null;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            lock (_paintMutex)
            {
                _inPaint = true;
                try
                {
                    if (_currentPicturArt != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Error in DrawPicture() currentPicturArt != null");
                    }
                    FastRenderingFilter fastRenderingFilter = null;
                    if (IsSubStepRendering() && !Fractrace.Animation.AnimationControl.InAnimation)
                    {
                        fastRenderingFilter = new FastRenderingFilter();
                        fastRenderingFilter.Apply();
                    }
                    _currentPicturArt = PictureArtFactory.Create(_iterateForPictureArt.PictureData, _iterateForPictureArt.LastUsedFormulas);
                    _currentPicturArt.Paint(_graphics);
                    while (_repaintRequested)
                    {
                        _lastPicturArt = _currentPicturArt;
                        _currentPicturArt = PictureArtFactory.Create(_iterateForPictureArt.PictureData, _iterateForPictureArt.LastUsedFormulas);
                        _currentPicturArt.Paint(_graphics);
                    }
                    if (fastRenderingFilter != null)
                        fastRenderingFilter.Restore();

                    _lastPicturArt = _currentPicturArt;
                    _currentPicturArt = null;
                    _repaintRequested = false;
                    CallDrawImage();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
                _inPaint = false;
            }
        }


        /// <summary>
        /// Call drawImage in Windows.Forms thread.
        /// </summary>
        public void CallDrawImage()
        {
            this.Invoke(new DrawImageDelegate(drawImage));
        }


        /// <summary>
        /// Refresh view. This is necessary to display pictureBox1.Image.
        /// </summary>
        private void drawImage()
        {
            this.Refresh();
            string fileName = FileSystem.Exemplar.GetFileName("pic.png");
            ParameterInput.MainParameterInput.SaveHistory(fileName);
            this.Text = fileName;
            pictureBox1.Image.Save(fileName);
            if (Fractrace.ParameterInput.MainParameterInput.AutomaticSaveInAnimation)
            {
                if (ParameterDict.Current["Intern.Filter"] == "" && _lastAnimationParameterHash != ParameterDict.Current.GetHash(""))
                {
                    Animation.AnimationControl.MainAnimationControl.AddCurrentHistoryEntry();
                    _lastAnimationParameterHash = ParameterDict.Current.GetHash("");
                }
            }

            Fractrace.ParameterInput.MainParameterInput.EnableRepaint(true);
            /*
            // On Mono: the Image ist not shown. Quickfix is to reload the saved image.
            if (!Environment.OSVersion.ToString().ToLower().Contains("microsoft"))
            {
                ShowPictureFromFile(fileName);
            }
            */
            if (ResultImageView.PublicForm.LastPicturArt != null)
            {
                if (_progress == 100) // TODO: Better test of last rendering process
                {
                    if (Fractrace.Scheduler.GrandScheduler.Exemplar.PictureIsCreated(ResultImageView.PublicForm.IterateForPictureArt, ResultImageView.PublicForm.LastPicturArt.PictureData))
                    {
                        ParameterInput.MainParameterInput.StartRendering();
                    }
                }
            }
        }


        /// <summary>
        /// Set computation progress in percent.
        /// </summary>
        /// <param name="progressInPercent"></param>
        public void Progress(double progressInPercent)
        {
            if (progressInPercent >= 0 && progressInPercent <= 100)
            {
                _progress = progressInPercent;
                _progressChanged = true;
            }
        }


        /// <summary>
        /// The value of the progressbar is updated.
        /// </summary>
        protected void OnProgress()
        {
            progressBar1.Value = (int)_progress;
        }


        /// <summary>
        /// Ends this application.
        /// </summary>
        public void ForceClosing()
        {
            _forceClosing = true;
            this.Close();
        }


        /// <summary>
        /// Dialogabfrage vor Beendigung der Anwendung.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_forceClosing)
            {
                if (MessageBox.Show("Close Application?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (_iterate != null)
                        _iterate.Abort();
                    base.OnClosing(e);
                }
                else e.Cancel = true;
            }
        }


        /// <summary>
        /// Try to update in background (still with some sync problems).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_inPaint)
                Fractrace.ParameterInput.MainParameterInput.EnableRepaint(false);
            //  btnRepaint.Enabled = false;

            if (_progressChanged)
            {
                _progressChanged = false;
                progressBar1.Value = (int)_progress;
            }
        }



        /// <summary>
        /// Return true, if bitmap creation is for substeps only.
        /// </summary>
        /// <returns></returns>
        private bool IsSubStepRendering()
        {
            int currentStep = _updateCount;
            int lastStep = ParameterDict.Current.GetInt("View.UpdateSteps") + 1;
            return currentStep < lastStep;
        }


    }
}
