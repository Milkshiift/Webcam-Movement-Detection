using System;
using MovementDetectionGUI.Views;
using OpenCvSharp;
using ReactiveUI;

namespace MovementDetectionGUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _debugMode = true;
    public bool DebugMode
    {
        get => _debugMode;
        set => _debugMode = value;
    }

    
    
    private static int _roix;
    public int Roix
    {
        get => _roix;
        set
        {
            _roix = this.RaiseAndSetIfChanged(ref _roix, value);
            SetRoi();
        }
    }


    private static int _roiy;
    public int Roiy
    {
        get => _roiy;
        set
        {
            _roiy = this.RaiseAndSetIfChanged(ref _roiy, value);
            SetRoi();
        }
    }


    private static int _roiWidth = 50;
    public int RoiWidth
    {
        get => _roiWidth;
        set
        {
            _roiWidth = this.RaiseAndSetIfChanged(ref _roiWidth, value);
            SetRoi();
        }
    }


    private static int _roiHeight = 25;
    public int RoiHeight
    {
        get => _roiHeight;
        set
        {
            _roiHeight = this.RaiseAndSetIfChanged(ref _roiHeight, value);
            SetRoi();
        }
    }


    private int _kernelSize = 5;
    public int KernelSize
    {
        get => _kernelSize;
        set => this.RaiseAndSetIfChanged(ref _kernelSize, value);
    }
    
    private int _blurStrength = 21;
    public int BlurStrength
    {
        get => _blurStrength;
        set => this.RaiseAndSetIfChanged(ref _blurStrength, value);
    }
    
    
    private int _captureWidth = 100;
    public int CaptureWidth
    {
        get => _captureWidth;
        set => this.RaiseAndSetIfChanged(ref _captureWidth, value);
    }

    
    private int _captureHeight = 100;
    public int CaptureHeight
    {
        get => _captureHeight;
        set => this.RaiseAndSetIfChanged(ref _captureHeight, value);
    }
    
    
    private string _status = "Waiting for input";
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }
    
    public static void SetRoi()
    {
        if (!MainWindow._running) return;
        var fw = (int) MainWindow._cap.Get(VideoCaptureProperties.FrameWidth);
        var fh = (int) MainWindow._cap.Get(VideoCaptureProperties.FrameHeight);
        MainWindow._roi = new Rect(Math.Clamp(_roix, 0, fw-_roiWidth), Math.Clamp(_roiy, 0, fh-_roiHeight),
            Math.Clamp(_roiWidth, 10, fw), Math.Clamp(_roiHeight, 10, fh));
        if (!MainWindow._runningReal) return;
        
    }
}