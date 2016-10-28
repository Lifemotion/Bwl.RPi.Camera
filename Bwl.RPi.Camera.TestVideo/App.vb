Imports System.Drawing

Module App
    <STAThread>
    Sub Main()
        Console.WriteLine("Width, Height, FPS, Options:")
        Dim line = ""
        line = Console.ReadLine
        Dim parts = line.Split(",")
        Dim cam As RpiCamVideo

        If parts.Length = 4 Then
            cam = New RpiCamVideo(Val(parts(0).Trim), Val(parts(1).Trim), Val(parts(2).Trim), parts(3).Trim)
        Else
            cam = New RpiCamVideo()
        End If

        Dim b1 As Bitmap
        Console.WriteLine("Working")
        Dim time = Now
        For i = 1 To 10
            b1 = cam.GetFrameAsBitmap
            Console.WriteLine(cam.FrameCounter.ToString)
        Next
        Dim ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS without save, bitmap: " + (1000 / ms).ToString("0.0"))

        time = Now
        For i = 1 To 10
            Dim bytes = cam.GetFrameAsBytes
            Console.WriteLine(cam.LastFrameBytesLength.ToString)
            Console.WriteLine(cam.FrameCounter.ToString)
        Next
        ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS without save, bytes: " + (1000 / ms).ToString("0.0"))

        time = Now
        For i = 1 To 10
            b1 = cam.GetFrameAsBitmap
            b1.Save("file" + i.ToString + ".jpg", Imaging.ImageFormat.Jpeg)
            Console.WriteLine(cam.FrameCounter.ToString)
        Next
        ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS with save: " + (1000 / ms).ToString("0.0"))

        cam.Close()
    End Sub

End Module
