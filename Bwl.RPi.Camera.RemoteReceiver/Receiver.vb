Imports Bwl.Network.Transport

Public Class Receiver
    Private WithEvents _listener As New TCPServer(8042)

    Private Sub _listener_NewConnection(server As IPortListener, transport As IPacketTransport) Handles _listener.NewConnection
        AddHandler transport.ReceivedPacket, AddressOf ReceivedPacketHandler
    End Sub

    Private Sub ReceivedPacketHandler(packet As BytePacket)
        Try
            IO.File.WriteAllBytes(Now.Ticks.ToString + ".jpg", packet.Bytes)
            Dim bmp As New Bitmap(New IO.MemoryStream(packet.Bytes))
            Me.Invoke(Sub()
                          PictureBox1.Image = bmp
                          PictureBox1.Refresh()
                      End Sub)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Receiver_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
