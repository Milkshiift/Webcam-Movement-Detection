using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MovementDetectionGUI.ViewModels;
using OpenCvSharp;
using WindowsInput;
using WindowsInput.Native;
using Window = Avalonia.Controls.Window;

namespace MovementDetectionGUI.Views;

public partial class MainWindow : Window
{
    public static VideoCapture _cap = null!;
    public static Rect _roi;
    public static bool _running;
    public static bool _runningReal;
    private bool _adapting = true;
    private bool _canceled;
    private BackgroundSubtractorMOG2 _fgbg = null!;
    private bool _isMovementDetected;
    private Timer _timer = null!;

    private static void DoAfterDetection()
    {
        // Simulate Ctrl+Win+RightArrow key combination
        var simulator = new InputSimulator();
        simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
        simulator.Keyboard.KeyDown(VirtualKeyCode.LWIN);
        simulator.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
        simulator.Keyboard.KeyUp(VirtualKeyCode.RIGHT);
        simulator.Keyboard.KeyUp(VirtualKeyCode.LWIN);
        simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
        // Simulate Fn+F8 to stop playing media
        simulator.Keyboard.KeyDown(VirtualKeyCode.MEDIA_STOP);
        simulator.Keyboard.KeyUp(VirtualKeyCode.MEDIA_STOP);
    }

    public MainWindow()
    {
        MinWidth = 700; // Set the minimum width of the window
        MinHeight = 600;
        Width = 700;
        Height = 600;
        InitializeComponent();
        ViewModel = new MainWindowViewModel();

        DataContext = ViewModel;
    }

    // ViewModel to hold the settings
    private static MainWindowViewModel? ViewModel { get; set; }

    private async Task WorkerAsync()
    {
        _runningReal = true;
        using var frame = new Mat();
        while (!_cap.IsDisposed && _cap.Read(frame))
        {
            if (_canceled) break;

            var isMovementDetected = DetectMovement(frame);

            if (isMovementDetected && !_isMovementDetected && !_adapting)
            {
                Trace.WriteLine("--- Movement detected ---");
                if (ChkDebugMode.IsChecked ?? false)
                {
                    Console.Beep();
                }
                else
                {
                    DoAfterDetection();
                }
            }

            _isMovementDetected = isMovementDetected; // To prevent continuous detection

            Cv2.Rectangle(frame, _roi, new Scalar(0, 255, 0), 2);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                processedFrameImage.Source = new Bitmap(frame.ToMemoryStream());
            });
        }
    }

    private bool DetectMovement(Mat frame)
    {
        using var fgmask = new Mat();
        frame.SubMat(_roi).CopyTo(fgmask);
        Cv2.GaussianBlur(fgmask, fgmask, new Size(ViewModel.BlurStrength, ViewModel.BlurStrength), 0);
        _fgbg.Apply(fgmask, fgmask);

        var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(ViewModel.KernelSize, ViewModel.KernelSize));
        Cv2.MorphologyEx(fgmask, fgmask, MorphTypes.Open, kernel);
        Cv2.MorphologyEx(fgmask, fgmask, MorphTypes.Close, kernel);
        
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(fgmask, out contours, out hierarchy, RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        return contours.Length > 0;
    }

    private async void InitializeDetection()
    {
        // Initialize video capture
        using (_cap = new VideoCapture(0))
        {
            _cap.Set(VideoCaptureProperties.FrameWidth, ViewModel.CaptureWidth);
            _cap.Set(VideoCaptureProperties.FrameHeight, ViewModel.CaptureHeight);

            MainWindowViewModel.SetRoi();

            _fgbg = BackgroundSubtractorMOG2.Create();

            _timer = new Timer(5000); // 5000 milliseconds = 5 seconds
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
            Trace.WriteLine("Started adapting, detections will not work");
            await Dispatcher.UIThread.InvokeAsync(() => { TxtStatus.Text = "Adapting..."; });
            await Task.Run(WorkerAsync);
        }
    }

    private void btnStartDetection_Click(object sender, RoutedEventArgs e)
    {
        if (_running) return;
        _running = true;
        TxtStatus.Text = "Initializing. You may need to wait a bit";
        _canceled = false;
        Task.Run(InitializeDetection);
    }

    private void btnStopDetection_Click(object sender, RoutedEventArgs e)
    {
        if (!_running) return;
        _running = false;
        _runningReal = false;
        _canceled = true;
        _timer.Stop();
        TxtStatus.Text = "Waiting for input";
        processedFrameImage.Source = null;
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _adapting = false;
        Trace.WriteLine("Finished adapting");
        Dispatcher.UIThread.InvokeAsync(() => { TxtStatus.Text = "Detecting movement"; });
        _timer.Stop();
    }
}