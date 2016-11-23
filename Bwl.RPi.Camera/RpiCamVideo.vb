Imports System.Diagnostics
Imports System.IO

Public Class RpiCamVideo
    Implements IDisposable
    Implements IRpiCam

    Public Event FrameReady(source As IRpiCam) Implements IRpiCam.FrameReady
    Public ReadOnly Property FrameCounter As Integer Implements IRpiCam.FrameCounter

    Public ReadOnly Property FrameBytesSynclock As New Object Implements IRpiCam.FrameBytesSynclock
    Public ReadOnly Property FrameBytesBuffer As Byte() = New Byte(1024 * 512) {} Implements IRpiCam.FrameBytesBuffer
    Public ReadOnly Property FrameBytesFormat As RpiCamFrameType = RpiCamFrameType.jpg Implements IRpiCam.FrameBytesFormat
    Public ReadOnly Property FrameBytesLength As Integer Implements IRpiCam.FrameBytesLength

    Private _prc As Process
    Private _readBuffer As Byte() = New Byte(1024 * 8) {}
    Private _readThread As Threading.Thread
    Private _currentFrameBuffer As Byte() = New Byte(1024 * 512) {}

    Public Sub New()

    End Sub

    Public Sub Open(width As Integer, height As Integer, fps As Integer, options As String) Implements IRpiCam.Open
        Close()

        If System.Environment.OSVersion.Platform = PlatformID.Unix Then
            _prc = New Process
            _prc.StartInfo.FileName = "raspivid"
            Dim args = "-cd MJPEG -h " + height.ToString + " -w " + width.ToString
            If fps > 0 Then
                args += " -fps " + fps.ToString
            End If
            args += " -n -t 999999999"
            If options IsNot Nothing AndAlso options > "" Then
                args += " " + options
            End If
            args += " -o -"
            _prc.StartInfo.Arguments = args
            _prc.StartInfo.RedirectStandardError = False
            _prc.StartInfo.RedirectStandardInput = True
            _prc.StartInfo.RedirectStandardOutput = True
            _prc.StartInfo.UseShellExecute = False
            _prc.Start()

            _readThread = New Threading.Thread(AddressOf ReadingThread)
            _readThread.Start()
        Else
            Throw New Exception("Not implemented on Windows")
        End If

    End Sub

    Private Sub ReadingThread()
        Dim _currentFramePosition As Integer
        Dim _frameStarted As Boolean
        Do
            Try
                Dim read = _prc.StandardOutput.BaseStream.Read(_readBuffer, 0, _readBuffer.Length - 16)
                'Console.WriteLine("readed " + read.ToString)
                For i = 0 To read - 1
                    If _readBuffer(i + 0) = &HFF AndAlso
                       _readBuffer(i + 1) = &HD8 AndAlso
                       _readBuffer(i + 3) = &HE0 AndAlso
                       _readBuffer(i + 6) = &H4A AndAlso
                       _readBuffer(i + 7) = &H46 AndAlso
                       _readBuffer(i + 8) = &H49 Then
                        If _frameStarted Then
                            SyncLock FrameBytesSynclock
                                _FrameBytesLength = _currentFramePosition
                                Array.Copy(_currentFrameBuffer, FrameBytesBuffer, FrameBytesLength)
                                _FrameCounter += 1
                            End SyncLock
                            RaiseEvent FrameReady(Me)
                        End If

                        _frameStarted = True
                        _currentFramePosition = 0
                    End If
                    _currentFrameBuffer(_currentFramePosition) = _readBuffer(i)
                    _currentFramePosition += 1
                Next
            Catch ex As Exception
                Console.WriteLine("Read Thread error: " + ex.Message)
            End Try
        Loop
    End Sub

    Public Sub WaitNewFrame() Implements IRpiCam.CaptureOrWaitFrame
        Static lastCounterValue As Integer
        Dim start = Now
        Do
            Threading.Thread.Sleep(1)
        Loop While lastCounterValue = _FrameCounter And (Now - start).TotalSeconds < 15
        If lastCounterValue <> _FrameCounter Then
            lastCounterValue = _FrameCounter
            Return
        Else
            Throw New Exception("Frame not captured in 5 seconds")
        End If
    End Sub

    ''' <summary>
    ''' Finishes raspivid
    ''' </summary>
    Public Sub Close() Implements IRpiCam.Close
        Try
            _readThread.Abort()
        Catch
        End Try
        _readThread = Nothing

        Try
            _prc.Kill()
        Catch
        End Try
        _prc = Nothing

        Try
            Dim prc As New Process
            prc.StartInfo.FileName = "pkill"
            prc.StartInfo.Arguments = "raspivid"
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
