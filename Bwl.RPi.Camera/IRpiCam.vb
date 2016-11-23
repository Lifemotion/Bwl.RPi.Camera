Public Enum RpiCamFrameType
    bmp
    jpg
End Enum

Public Interface IRpiCam
    Event FrameReady(source As IRpiCam)

    Sub Open(width As Integer, height As Integer, fps As Integer, options As String)
    Sub Close()

    ReadOnly Property FrameCounter As Integer
    ReadOnly Property FrameBytesBuffer As Byte()
    ReadOnly Property FrameBytesLength As Integer
    ReadOnly Property FrameBytesFormat As RpiCamFrameType
    ReadOnly Property FrameBytesSynclock As Object

    Function CreateBytesCopy() As Byte()

    Sub CaptureOrWaitFrame()
End Interface

