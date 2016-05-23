Imports System.Drawing

Public Interface IRpiCam
    Sub Close()
    Sub Dispose()
    Function GetFrameAsBitmap() As Bitmap
    Function GetFrameAsBytes() As Byte()
End Interface
