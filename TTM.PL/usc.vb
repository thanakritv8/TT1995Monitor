Imports TTM.BLL
Public Class usc
    Private _ProcessName As String
    Public Property ProcessName
        Get
            Return _ProcessName
        End Get
        Set(value)
            _ProcessName = value
        End Set
    End Property

    Dim _t As Threading.Thread
    Private Sub usc_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lbProcessName.Text = _ProcessName
        _t = New Threading.Thread(AddressOf Run)
        _t.IsBackground = True
        _t.Start()
    End Sub

    Private Sub Run()
        While _t.IsAlive
            Dim BLL_Load As BLL_Load = New BLL_Load
            'If _ProcessName = "ภาษี" Then
            'Dim _Msg As String = BLL_Load.CheckStatusTaxNotify()
            'If Not String.IsNullOrEmpty(_Msg) Then
            '    UpdateList("Clear")
            '    UpdateList(_Msg)
            'End If

            If _ProcessName = "ภาษี" Then
                BLL_Load.CheckStatusTaxNotify(_ProcessName)
            ElseIf _ProcessName = "ประกันพรบ." Then
                BLL_Load.CheckStatusActInsuranceNotify(_ProcessName)
            ElseIf _ProcessName = "ประกันภัยรถยนต์" Then
                BLL_Load.CheckStatusMainInsuranceNotify(_ProcessName)
            ElseIf _ProcessName = "ประกันภัยสิ่งแวดล้อม" Then
                BLL_Load.CheckStatusEnvInsuranceNotify(_ProcessName)
            ElseIf _ProcessName = "ประกันภัยสินค้าภายในประเทศ" Then
                BLL_Load.CheckStatusDPINotify(_ProcessName)
            ElseIf _ProcessName = "ใบอนุญาต วอ.8" Then
                BLL_Load.CheckStatusLV8Notify(_ProcessName)
            ElseIf _ProcessName = "ใบอนุญาตโรงงาน" Then
                BLL_Load.CheckStatusLFNotify(_ProcessName)
            ElseIf _ProcessName = "ใบอนุญาตกัมพูชา" Then
                BLL_Load.CheckStatusLCNotify(_ProcessName)
            ElseIf _ProcessName = "ใบอนุญาตลุ่มน้ำโขง" Then
                BLL_Load.CheckStatusLMRNotify(_ProcessName)
            Else
                End If

            'End If
            Threading.Thread.Sleep(1000)
        End While
    End Sub

    Delegate Sub DelUpdateList(ByVal _Msg As String)
    Private Sub UpdateList(ByVal _Msg As String)
        If InvokeRequired Then
            Invoke(New DelUpdateList(AddressOf UpdateList), _Msg)
        Else
            If _Msg = "Clear" Then
                lbControl.Items.Clear()
            Else
                lbControl.Items.Add(_Msg)
            End If
        End If
    End Sub
End Class
