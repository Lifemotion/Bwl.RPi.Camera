﻿Module App
    Dim cam As New RpiCam(1024, 768)

    Sub Main()
        Dim time = Now
        For i = 1 To 10
            Dim b1 = cam.GetFrameAsBitmap
            b1.Save(i.ToString + ".jpg", Drawing.Imaging.ImageFormat.Jpeg)
        Next
        Dim ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS with save: " + (1000 / ms).ToString("0.0"))


        time = Now
        For i = 1 To 10
            Dim b1 = cam.GetFrameAsBitmap
        Next
        ms = (Now - time).TotalMilliseconds / 10
        Console.WriteLine("FPS without save: " + (1000 / ms).ToString("0.0"))

        cam.Close()
    End Sub

End Module
