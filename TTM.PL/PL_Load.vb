Imports TTM.BLL
Public Class PL_Load
    Dim _t As Threading.Thread
    Private Sub PL_Load_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim usc(0) As usc
        Dim BLL_Load As BLL_Load = New BLL_Load
        Dim DtTable As DataTable = BLL_Load.GetTable()
        ReDim usc(DtTable.Rows.Count - 1)
        Dim interval As Integer = 5
        Dim _rowUsc As Integer = 7
        Dim x As Integer = interval
        Dim y As Integer = interval
        For i As Integer = 0 To usc.Length - 1
            usc(i) = New usc
            usc(i).Location = New Point(x, y)
            usc(i).ProcessName = DtTable.Rows(i)("display")
            x += usc(i).Size.Width + interval
            If i = _rowUsc Then
                _rowUsc = (_rowUsc * 2) + 1
                x = interval
                y += usc(i).Size.Height + interval
            End If
            Me.Controls.Add(usc(i))
        Next
        _t = New Threading.Thread(AddressOf Run)
        _t.IsBackground = True
        _t.Start()
    End Sub


    Private Sub Run()
        While _t.IsAlive
            Dim Key As String = Command() '"snNseQbdEIMtzUFx0uN3JQt2YhCtjZFbW0EyGFxQoWt"
            Dim BLL_Load As BLL_Load = New BLL_Load
            Dim DtLine As DataTable = BLL_Load.GetMsgLine
            For Each _Item In DtLine.Rows
                BLL_Load.SendLine(Key, _Item("txt_msg"))
                BLL_Load.UpdateLogMonitor(_Item("log_id"))
            Next
            Threading.Thread.Sleep(1000)
        End While
    End Sub
End Class
