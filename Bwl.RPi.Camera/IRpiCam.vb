Imports System.Drawing

Public Interface IRpiCam
    Sub Close()
    Sub Dispose()
    Function GetFrameAsBitmap() As Bitmap
    Function GetFrameAsBytes() As Byte()
    ReadOnly Property FrameCounter As Integer
    ReadOnly Property LastFrameBytesLength As Integer
End Interface
