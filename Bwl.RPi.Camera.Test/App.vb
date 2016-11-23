Imports System.Drawing
Imports Bwl.Network.Transport

Module App
    Dim cam As IRpiCam = New RpiCamVideo
    <STAThread>
    Sub Main()
        'CreateUserDefinedCamera()
        cam.Open(640, 480, 20, "")
        TestTransmission()
        cam.Close()
    End Sub

    Public Sub TestTransmission()
        Dim client As New TCPTransport
        client.Open("20.20.25.80:8042", "")
        Do
            Dim time = Now
            Dim size = 0.0
            For i = 1 To 10
                cam.CaptureOrWaitFrame()
                size += cam.FrameBytesLength
                Dim bytes = cam.CreateBytesCopy
                Dim pkt As New BytePacket(bytes)
                client.SendPacket(pkt)
            Next
            Dim MS = (Now - time).TotalMilliseconds / 10
            Dim fps = (1000 / MS)
            Console.WriteLine("FPS without save, bitmaps: " + fps.ToString("0.0") + ", " + (size / 1024.0 / 1024.0 / 10.0 * fps).ToString("0.0") + " Mbytes\sec")
        Loop
    End Sub

    Public Sub CreateUserDefinedCamera()
        Console.WriteLine("Width, Height, FPS, Options:")
        Dim line = Console.ReadLine
        Dim parts = line.Split(",")

        If parts.Length = 4 Then
            cam.Open(Val(parts(0).Trim), Val(parts(1).Trim), Val(parts(2).Trim), parts(3).Trim)
        Else
            cam.Open(640, 480, 20, "")
        End If
    End Sub

    Public Sub TestFpsBitmaps(repeats As Integer)
        Throw New NotImplementedException

        For k = 1 To repeats
            Dim time = Now
            Dim Size = 0.0
            For i = 1 To 10
                cam.CaptureOrWaitFrame()
                Size += cam.FrameBytesLength
            Next
            Dim MS = (Now - time).TotalMilliseconds / 10
            Dim fps = (1000 / MS)
            Console.WriteLine("FPS without save, bitmaps: " + fps.ToString("0.0") + ", " + (Size / 1024.0 / 1024.0 / 10.0 * fps).ToString("0.0") + " Mbytes\sec")
        Next
    End Sub

    Public Sub TestFpsBytes(repeats As Integer)
        For k = 1 To repeats
            Dim time = Now
            Dim Size = 0.0
            For i = 1 To 10
                cam.CaptureOrWaitFrame()
                Size += cam.FrameBytesLength
            Next
            Dim MS = (Now - time).TotalMilliseconds / 10
            Dim fps = (1000 / MS)
            Console.WriteLine("FPS without save, bytes: " + fps.ToString("0.0") + ", " + (Size / 1024.0 / 1024.0 / 10.0 * fps).ToString("0.0") + " Mbytes\sec")
        Next
    End Sub

    Public Function BytesToBitmap(bytes As Byte()) As Bitmap
        Dim bmp As New Bitmap(New IO.MemoryStream(bytes))
        Return bmp
    End Function
End Module
