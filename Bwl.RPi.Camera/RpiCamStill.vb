Imports System.Diagnostics

Public Class RpiCamStill
    Implements IDisposable
    Implements IRpiCam

    Private _prc As Process

    Public Event FrameReady(source As IRpiCam) Implements IRpiCam.FrameReady

    Public ReadOnly Property FrameBytesSynclock As New Object Implements IRpiCam.FrameBytesSynclock
    Public ReadOnly Property FrameBytesBuffer As Byte() = {} Implements IRpiCam.FrameBytesBuffer
    Public ReadOnly Property FrameBytesFormat As RpiCamFrameType = RpiCamFrameType.bmp Implements IRpiCam.FrameBytesFormat
    Public ReadOnly Property FrameCounter As Integer Implements IRpiCam.FrameCounter

    Public ReadOnly Property FrameBytesLength As Integer Implements IRpiCam.FrameBytesLength
        Get
            Return FrameBytesBuffer.Length
        End Get
    End Property

    Public Sub New()

    End Sub

    Public Sub Open(width As Integer, height As Integer, fps As Integer, options As String) Implements IRpiCam.Open
        Close()

        _FrameBytesBuffer = New Byte((width * height * 3) + 53) {}
        If System.Environment.OSVersion.Platform = PlatformID.Unix Then
            _prc = New Process
            _prc.StartInfo.FileName = "raspistill"
            _prc.StartInfo.Arguments = "-e bmp -h " + height.ToString + " -w " + width.ToString + " -n -t 999999999 -k " + options + "-o -"
            _prc.StartInfo.RedirectStandardError = False
            _prc.StartInfo.RedirectStandardInput = True
            _prc.StartInfo.RedirectStandardOutput = True
            _prc.StartInfo.UseShellExecute = False
            _prc.Start()
        Else
            Throw New Exception("Not implemented on Windows")
        End If
    End Sub

    Public Sub CaptureFrame() Implements IRpiCam.CaptureOrWaitFrame
        SyncLock FrameBytesSynclock
            If System.Environment.OSVersion.Platform = PlatformID.Unix Then
                _prc.StandardOutput.DiscardBufferedData()
                _prc.StandardInput.Write(vbLf)

                Dim start = Now
                Dim totalBytes = 0
                Do
                    totalBytes += _prc.StandardOutput.BaseStream.Read(FrameBytesBuffer, totalBytes, FrameBytesBuffer.Length - totalBytes)
                Loop While totalBytes < FrameBytesBuffer.Length And (Now - start).TotalSeconds < 3
                If totalBytes < FrameBytesBuffer.Length Then
                    Throw New Exception("Buffer not full, capture failed")
                Else
                    _FrameCounter += 1
                End If
            Else
                Throw New Exception("Not implemented on Windows")
            End If
        End SyncLock
        RaiseEvent FrameReady(Me)
    End Sub

    ''' <summary>
    ''' Finishes raspistill
    ''' </summary>
    Public Sub Close() Implements IRpiCam.Close
        Try
            _prc.Kill()
        Catch
        End Try

        Try
            Dim prc As New Process
            prc.StartInfo.FileName = "pkill"
            prc.StartInfo.Arguments = "raspistill"
            prc.Start()
            prc.WaitForExit()
        Catch ex As Exception
        End Try
        Threading.Thread.Sleep(500)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Close()
    End Sub

    Public Function CreateBytesCopy() As Byte() Implements IRpiCam.CreateBytesCopy
        SyncLock FrameBytesSynclock
            Dim bytes(FrameBytesLength - 1) As Byte
            Array.Copy(FrameBytesBuffer, bytes, bytes.Length)
            Return bytes
        End SyncLock
    End Function
End Class
