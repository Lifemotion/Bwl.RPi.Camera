Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports Bwl.RPi.Camera

Public Class RpiCamStill
    Implements IDisposable
    Implements IRpiCam

    Private ReadOnly _prc As Process
    Private ReadOnly _buffer As Byte()
    Private _width As Integer
    Private _height As Integer
    Private _counter As Integer

    Public ReadOnly Property FrameCounter As Integer Implements IRpiCam.FrameCounter
        Get
            Return _counter
        End Get
    End Property

    Public ReadOnly Property LastFrameBytesLength As Integer Implements IRpiCam.LastFrameBytesLength

    Public Sub New()
        Me.New(1920, 1080, "")
    End Sub

    Public Sub New(width As Integer, height As Integer, options As String)
        _width = width
        _height = height
        _buffer = New Byte((width * height * 3) + 53) {}
        If System.Environment.OSVersion.Platform = PlatformID.Unix Then
            Try
                Dim prc As New Process
                prc.StartInfo.FileName = "pkill"
                prc.StartInfo.Arguments = "raspistill"
                prc.Start()
                prc.WaitForExit()
            Catch ex As Exception
            End Try
            Threading.Thread.Sleep(500)
            _prc = New Process
            _prc.StartInfo.FileName = "raspistill"
            _prc.StartInfo.Arguments = "-e bmp -h " + height.ToString + " -w " + width.ToString + " -n -t 999999999 -k " + options + "-o -"
            _prc.StartInfo.RedirectStandardError = False
            _prc.StartInfo.RedirectStandardInput = True
            _prc.StartInfo.RedirectStandardOutput = True
            _prc.StartInfo.UseShellExecute = False
            _prc.Start()
        Else

        End If
    End Sub

    Public Function GetFrameAsBitmap() As Bitmap Implements IRpiCam.GetFrameAsBitmap
        Dim buff = GetFrameAsBytes()
        Try
            Using m = New MemoryStream(buff)
                Return Bitmap.FromStream(m)
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    Public Function GetFrameAsBytes() As Byte() Implements IRpiCam.GetFrameAsBytes
        If System.Environment.OSVersion.Platform = PlatformID.Unix Then
            _prc.StandardOutput.DiscardBufferedData()
            _prc.StandardInput.Write(vbLf)

            Dim start = Now
            Dim totalBytes = 0
            Do
                totalBytes += _prc.StandardOutput.BaseStream.Read(_buffer, totalBytes, _buffer.Length - totalBytes)
            Loop While totalBytes < _buffer.Length And (Now - start).TotalSeconds < 3
            If totalBytes < _buffer.Length Then
                Throw New Exception("Buffer not full, capture failed")
            Else
                _counter += 1
                _LastFrameBytesLength = _buffer.Length
                Return _buffer
            End If
        Else
            Dim testJpg = My.Resources.cat
            _LastFrameBytesLength = testJpg.Length
            Return testJpg
        End If
    End Function

    ''' <summary>
    ''' Finishes raspistill
    ''' </summary>
    Public Sub Close() Implements IRpiCam.Close
        Try
            _prc.Kill()
        Catch
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose, IRpiCam.Dispose
        Close()
    End Sub

End Class
