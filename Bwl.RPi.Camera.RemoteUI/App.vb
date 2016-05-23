Imports Bwl.Framework
Imports Bwl.Network.ClientServer

Module App
    Dim _cam As RpiCam
    Dim _ui As New AutoUI
    Dim _logger As New Logger
    Dim _appFormDescriptor As New AutoFormDescriptor(_ui, "form") With {.ShowLogger = True, .LoggerExtended = False, .FormWidth = 950, .FormHeight = 920}
    Dim WithEvents _frame As New AutoBitmap(_ui, "Frame")
    Dim WithEvents _initParams As New AutoTextbox(_ui, "Parameters")
    Dim WithEvents _initButton As New AutoButton(_ui, "Init")
    Dim WithEvents _captureButton As New AutoButton(_ui, "Capture")
    Dim WithEvents _testSpeedButton As New AutoButton(_ui, "TestSpeed")
    Dim WithEvents _exitButton As New AutoButton(_ui, "CloseApp")

    Dim _appServer As New RemoteAppServer(3194, Nothing, _logger, _ui, "BwlRpiCam", RemoteAppBeaconMode.broadcast)
    Dim _running As Boolean = True

    Sub Main()
        _frame.Info.Width = 640
        _frame.Info.Height = 480
        _initParams.Text = "640,480"
        Do While _running
                Threading.Thread.Sleep(500)
                If Console.KeyAvailable Then
                    Dim key = Console.ReadKey
                    If key.Key = ConsoleKey.Escape Then _running = False
                End If
            Loop
            If _cam IsNot Nothing Then _cam.Close()
    End Sub

    Private Sub _exitButton_Click(source As AutoButton) Handles _exitButton.Click
        _running = False
    End Sub

    Private Sub _captureButton_Click(source As AutoButton) Handles _captureButton.Click
        If _cam Is Nothing Then Return
        Dim img = _cam.GetFrameAsBitmap
        _frame.Image = img
    End Sub

    Private Sub _testSpeedButton_Click(source As AutoButton) Handles _testSpeedButton.Click
        If _cam Is Nothing Then Return
        Dim time = Now
        For i = 1 To 5
            Dim b1 = _cam.GetFrameAsBitmap
            b1.Save(i.ToString + ".jpg", Drawing.Imaging.ImageFormat.Jpeg)
        Next
        Dim ms = (Now - time).TotalMilliseconds / 5
        _logger.AddMessage("FPS with save: " + (1000 / ms).ToString("0.0"))


        time = Now
        For i = 1 To 5
            Dim b1 = _cam.GetFrameAsBitmap
        Next
        ms = (Now - time).TotalMilliseconds / 5
        _logger.AddMessage("FPS without save: " + (1000 / ms).ToString("0.0"))
    End Sub

    Private Sub _initButton_Click(source As AutoButton) Handles _initButton.Click
        If _cam IsNot Nothing Then _cam.Close()
        Dim parts = _initParams.Text.Split(",")
        _cam = New RpiCam(CInt(parts(0)), CInt(parts(1)))
    End Sub
End Module
