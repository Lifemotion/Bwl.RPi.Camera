Imports System.Drawing

Module App
    <STAThread>
    Sub Main()
        Console.WriteLine("Width, Height, Options:")
        Dim line = Console.ReadLine
        Dim parts = line.Split(",")
        Dim cam As RpiCamStill

        If parts.Length = 3 Then
            cam = New RpiCamStill(Val(parts(0).Trim), Val(parts(1).Trim), parts(2).Trim)
        Else
            cam = New RpiCamStill(1280, 720, "")
        End If

        Dim b1 As Bitmap

        Dim time = Now
        For i = 1 To 10
            b1 = cam.GetFrameAsBitmap
            Console.WriteLine(cam.FrameCounter.ToString)
        Next
        Dim ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS without save: " + (1000 / ms).ToString("0.0"))

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
