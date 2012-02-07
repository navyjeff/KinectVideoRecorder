using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;


namespace KinectVideoRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
        }
    }
}


/// here's where it starts breaking
/// 
private void ConnectButton_Click(object sender, RoutedEventArgs e)
{
    InitializeKinect();
    UpdateButtons();
}

void InitializeKinect()
{
    try
    {
        m_nui = new Runtime();
        m_nui.Initialize(RuntimeOptions.UseColor);
    }
    catch (InvalidOperationException)
    {
        MessageBox.Show("No Kinects detected!");
        m_nui = null;
        return;
    }


    try
    {
        var resolution = ImageResolution.Resolution1280x1024;
        if (ResolutionComboBox.Text == "640x480")
        {
            resolution = ImageResolution.Resolution640x480;
        }
        m_nui.VideoStream.Open(ImageStreamType.Video, 2, resolution, ImageType.Color);


        FPS.Text = "0";

        if (UsageModelComboBox.Text == "Event")
        {
            m_nui.VideoFrameReady += 
                (sender, e) =>
                    {
                        ShowImage(e.ImageFrame);
                    };

            m_eventDriven = true;
            FPS.IsEnabled = true;
            m_samples = 0;
            m_sw.Reset();
            m_sw.Start();
        }
        else
        {
            FPS.IsEnabled = false;
            m_eventDriven = false;
        }

        CameraName.Text = m_nui.NuiCamera.UniqueDeviceName;
        CameraElevationAngle.Text = m_nui.NuiCamera.ElevationAngle.ToString();

        CameraElevationAngle.Text = m_nui.NuiCamera.ElevationAngle.ToString();
    }
    catch (InvalidOperationException e)
    {
        MessageBox.Show( e.Message, "Error opening video stream!");
        UninitializeKinect();
    }
}

private void CameraElevationAngleButton_Click(object sender, RoutedEventArgs e)
{
    int iValue;
    if (!int.TryParse(CameraElevationAngle.Text, out iValue))
    {
        MessageBox.Show("The camera angle is in degrees!");
        return;
    }
    if (iValue > Camera.ElevationMaximum)
    {
        MessageBox.Show(string.Format("The camera angle cannot be higher than {0}!", 
           Camera.ElevationMaximum));
        return;
    }
    if (iValue < Camera.ElevationMinimum)
    {
        MessageBox.Show(string.Format("The camera angle cannot be lower than {0}!", 
           Camera.ElevationMinimum));
        return;
    }
    if (m_nui.NuiCamera.ElevationAngle == iValue)
    {
        return;
    }
    if (m_warnUser)
    {
        var result = MessageBox.Show("You should tilt the Kinect sensor as few times as possible, to minimize wear on the camera and to minimize tilting time. The camera motor is not designed for constant or repetitive movement, and attempts to use it that way may cause degradation of motor function. This beta SDK limits the rate at which applications can tilt the sensor, to protect the Kinect hardware. If the application tries to tilt the sensor too frequently, the runtime imposes a short lockout period during which any further calls return an error code.\nDo you wish to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.No)
        {
            return;
        }

        m_warnUser = false;
    }

    try
    {
        m_nui.NuiCamera.ElevationAngle = iValue;
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message);
    }
}