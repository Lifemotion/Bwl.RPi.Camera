Imports System.Diagnostics
Imports System.Drawing
Imports System.IO

Public Class RpiCam
    Implements IDisposable
    Implements IRpiCam

    Private ReadOnly _prc As Process
    Private ReadOnly _buffer As Byte()

    Public Sub New()
        Me.New(1920, 1080)
    End Sub

    Public Sub New(width As Integer, height As Integer)
        _buffer = New Byte((width * height * 3) + 53) {}
        _prc = New Process
        _prc.StartInfo.FileName = "raspistill"
        _prc.StartInfo.Arguments = "-e bmp -h " + height.ToString + " -w " + width.ToString + " -n -t 999999999 -k -o -"
        _prc.StartInfo.RedirectStandardError = False
        _prc.StartInfo.RedirectStandardInput = True
        _prc.StartInfo.RedirectStandardOutput = True
        _prc.StartInfo.UseShellExecute = False
        _prc.Start()
    End Sub

    Public Function GetFrameAsBitmap() As Bitmap Implements IRpiCam.GetFrameAsBitmap
        GetFrameAsBytes()
        Try
            Using m = New MemoryStream(_buffer)
                Return Image.FromStream(m)
            End Using
        Catch
            Return Nothing
        End Try
    End Function

    Public Function GetFrameAsBytes() As Byte() Implements IRpiCam.GetFrameAsBytes
        _prc.StandardOutput.DiscardBufferedData()
        _prc.StandardInput.WriteLine(vbLf)

        Dim totalBytes = 0
        Do
            totalBytes += _prc.StandardOutput.BaseStream.Read(_buffer, totalBytes, _buffer.Length - totalBytes)
        Loop While totalBytes < _buffer.Length

        Return _buffer
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
