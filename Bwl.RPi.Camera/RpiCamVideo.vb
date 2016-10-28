Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports Bwl.RPi.Camera

Public Class RpiCamVideo
    Implements IDisposable
    Implements IRpiCam

    Private ReadOnly _prc As Process
    Private ReadOnly _buffer As Byte() = New Byte(1024 * 8) {}
    Private _width As Integer
    Private _height As Integer
    Private _counter As Integer
    Private _readThread As New Threading.Thread(AddressOf ReadingThread)

    Private _frameBuffer As Byte() = New Byte(1024 * 512) {}
    Private _frameBufferPosition As Integer = 0
    Private _frameStarted As Boolean = False
    Private _frameBytes As Byte() = New Byte(1024 * 512) {}
    Private _frameBytesLength As Integer
    '   Private _frameBytesCopy As Byte() = New Byte(1024 * 512) {}
    '  Private _frameBytesCopyLength As Integer

    Private Sub ReadingThread()
        Do
            Try
                Dim read = _prc.StandardOutput.BaseStream.Read(_buffer, 0, _buffer.Length - 16)
                For i = 0 To read - 1
                    If _buffer(i + 0) = &HFF AndAlso
                       _buffer(i + 1) = &HD8 AndAlso
                       _buffer(i + 3) = &HE0 AndAlso
                       _buffer(i + 6) = &H4A AndAlso
                       _buffer(i + 7) = &H46 AndAlso
                       _buffer(i + 8) = &H49 Then
                        If _frameStarted Then
                            SyncLock _frameBytes
                                _frameBytesLength = _frameBufferPosition
                                Array.Copy(_frameBuffer, _frameBytes, _frameBytesLength)
                                _counter += 1
                            End SyncLock
                        End If
                        _frameStarted = True
                        _frameBufferPosition = 0
                    End If
                    _frameBuffer(_frameBufferPosition) = _buffer(i)
                    _frameBufferPosition += 1
                Next
            Catch ex As Exception
                Console.WriteLine("Read Thread error: " + ex.Message)
            End Try
        Loop
    End Sub

    Public ReadOnly Property FrameCounter As Integer Implements IRpiCam.FrameCounter
        Get
            Return _counter
        End Get
    End Property

    Public ReadOnly Property LastFrameBytesLength As Integer Implements IRpiCam.LastFrameBytesLength

    Public Sub New()
        Me.New(640, 480, 30, "")
    End Sub

    Public Sub New(width As Integer, height As Integer, maxFrameRate As Integer, options As String)
        _width = width
        _height = height
        If System.Environment.OSVersion.Platform = PlatformID.Unix Then
            Try
                Dim prc As New Process
                prc.StartInfo.FileName = "pkill"
                prc.StartInfo.Arguments = "raspivid"
                prc.Start()
                prc.WaitForExit()
            Catch ex As Exception
            End Try
            Threading.Thread.Sleep(500)
            _prc = New Process
            _prc.StartInfo.FileName = "raspivid"
            _prc.StartInfo.Arguments = "-cd MJPEG -h " + height.ToString + " -w " + width.ToString + " -fps " + maxFrameRate.ToString + " -n -t 999999999 " + options + "-o -"
            _prc.StartInfo.RedirectStandardError = False
            _prc.StartInfo.RedirectStandardInput = True
            _prc.StartInfo.RedirectStandardOutput = True
            _prc.StartInfo.UseShellExecute = False
            _prc.Start()
            _readThread.Start()
        Else

        End If
    End Sub

    Public Function GetFrameAsBitmap() As Bitmap Implements IRpiCam.GetFrameAsBitmap
        Dim buff = GetFrameAsBytes()
        Try
            SyncLock buff
                Using m = New MemoryStream(buff)
                    Return Bitmap.FromStream(m)
                End Using
            End SyncLock
        Catch
            Return Nothing
        End Try
    End Function

    Public Function GetFrameAsBytes() As Byte() Implements IRpiCam.GetFrameAsBytes
        Static lastCounterValue As Integer
        If System.Environment.OSVersion.Platform = PlatformID.Unix Then
            Dim start = Now
            Do
                Threading.Thread.Sleep(1)
            Loop While lastCounterValue = _counter And (Now - start).TotalSeconds < 3
            If lastCounterValue <> _counter Then
                lastCounterValue = _counter
                _LastFrameBytesLength = _frameBytesLength
                Return _frameBytes
            Else
                Throw New Exception("Frame not captured in 3 seconds")
            End If
        Else
            Dim testJpg = My.Resources.cat
            _LastFrameBytesLength = testJpg.Length
            Return testJpg
        End If
    End Function

    ''' <summary>
    ''' Finishes raspivid
    ''' </summary>
    Public Sub Close() Implements IRpiCam.Close
        Try
            _prc.Kill()
        Catch
        End Try

        Try
            _readThread.Abort()
        Catch
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose, IRpiCam.Dispose
        Close()
    End Sub

End Class
