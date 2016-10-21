Module App
    Dim cam As New RpiCam(1024, 768, "")
    <STAThread>
    Sub Main()
        Dim b1 = cam.GetFrameAsBitmap
        b1.Save("file.jpg", Drawing.Imaging.ImageFormat.Jpeg)
        cam.Close()
        Return

        Dim time = Now
        For i = 1 To 10
            b1 = cam.GetFrameAsBitmap
        Next
        Dim ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS without save: " + (1000 / ms).ToString("0.0"))

        time = Now
        For i = 1 To 10
            b1 = cam.GetFrameAsBitmap
            b1.Save("file" + i.ToString + ".jpg", Drawing.Imaging.ImageFormat.Jpeg)
        Next
        ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS with save: " + (1000 / ms).ToString("0.0"))




        cam.Close()
    End Sub

End Module
