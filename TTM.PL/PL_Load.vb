Imports TTM.BLL
Public Class PL_Load
    Dim _t As Threading.Thread
    Dim StatusRun As Boolean
    Private Sub PL_Load_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        StatusRun = True
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
            If Now.Hour = 15 And StatusRun Then
                StatusRun = False

                Dim Key As String = Command() '"snNseQbdEIMtzUFx0uN3JQt2YhCtjZFbW0EyGFxQoWt"
                Dim BLL_Load As BLL_Load = New BLL_Load
                Dim DtLine As DataTable = BLL_Load.GetMsgLine
                Dim DTDay As DataTable = DtLine.DefaultView.ToTable(True, "log_days")
                Dim ArrTableId() As Integer = {3, 15, 18, 16, 17, 30, 28, 22, 23}
                Dim ArrTableName() As String = {"ภาษี", "ประกันภัยรถยนต์", "ประกันภัยสิ่งแวดล้อม", "ประกันภัยสินค้าภายในประเทศ", "ประกันพรบ.", "ใบอนุญาตโรงงาน", "ใบอนุญาต วอ.8", "ใบอนุญาตกัมพูชา", "ใบอนุญาตลุ่มน้ำโขง"}
                For Each _ItemDay In DTDay.Rows

                    For i As Integer = 0 To ArrTableId.Length - 1
                        Dim _Title As String = ArrTableName(i)
                        Dim DtStatus As DataTable = BLL_Load.GetStatus(ArrTableId(i))
                        For Each _ItemStatus In DtStatus.Rows
                            Dim _ContentNumber As String = String.Empty
                            Dim _ContentLicense As String = String.Empty
                            Dim _ContentDriver As String = String.Empty
                            Dim _Content As String = String.Empty

                            Dim _ContentDataLicense As String = String.Empty
                            Dim _ContentNumberLicense As String = String.Empty

                            Dim DrRow() As DataRow = DtLine.Select("log_status = '" & _ItemStatus("data_list") & "' and table_id = " & ArrTableId(i) & " and log_days = " & _ItemDay("log_days"))
                            For r As Integer = 0 To DrRow.Length - 1
                                If ArrTableId(i) = 22 Or ArrTableId(i) = 23 Then
                                    Dim DtDataLicense As DataTable = New DataTable
                                    If ArrTableId(i) = 22 Then
                                        DtDataLicense = BLL_Load.GetDataLC(DrRow(r)("fk_id"))
                                    ElseIf ArrTableId(i) = 23 Then
                                        DtDataLicense = BLL_Load.GetDataLMR(DrRow(r)("fk_id"))
                                    End If
                                    For Each _ItemDataLicense In DtDataLicense.Rows
                                        _ContentNumberLicense = _ItemDataLicense("number")
                                        _ContentDataLicense &= _ItemDataLicense("number_head") & "=>" & _ItemDataLicense("number_tail") & ","
                                    Next
                                    If (_ContentDataLicense.Length > 0) Then
                                        _ContentDataLicense = Mid(_ContentDataLicense, 1, _ContentDataLicense.Length - 1)
                                    End If
                                Else
                                    If r = DrRow.Length - 1 Then
                                        _ContentNumber &= IIf(IsDBNull(DrRow(r)("number_car")), String.Empty, DrRow(r)("number_car"))
                                        _ContentLicense &= IIf(IsDBNull(DrRow(r)("license_car")), String.Empty, DrRow(r)("license_car"))
                                        _ContentDriver &= IIf(IsDBNull(DrRow(r)("driver_name")), String.Empty, DrRow(r)("driver_name"))
                                    Else
                                        _ContentNumber &= IIf(IsDBNull(DrRow(r)("number_car")), String.Empty, DrRow(r)("number_car")) & ", "
                                        _ContentLicense &= IIf(IsDBNull(DrRow(r)("license_car")), String.Empty, DrRow(r)("license_car")) & ", "
                                        _ContentDriver &= IIf(IsDBNull(DrRow(r)("driver_name")), String.Empty, DrRow(r)("driver_name")) & ", "
                                    End If
                                End If

                                BLL_Load.UpdateLogMonitor(DrRow(r)("log_id"))
                            Next
                            If DrRow.Length > 0 Then
                                _Content = "(" & ArrTableName(i) & ")"
                                If ArrTableId(i) = 22 Or ArrTableId(i) = 23 Then
                                    _Content &= " ใบอนุญาตเลขที่ " & _ContentNumberLicense
                                    _Content &= " " & _ContentDataLicense
                                Else
                                    If _ContentLicense = String.Empty Then
                                        _Content &= " พนักงานขับรถ " & _ContentDriver
                                    Else
                                        _Content &= " เบอร์รถ " & _ContentNumber
                                        _Content &= " ทะเบียนรถ " & _ContentLicense
                                    End If

                                End If
                                If _ItemDay("log_days") = 0 Then
                                    _Content &= " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                                Else
                                    _Content &= " มีสถานะ " & _ItemStatus("data_list")
                                    _Content &= " เกิน " & _ItemDay("log_days") & " วัน"
                                End If
                                BLL_Load.SendLine(Key, _Content)
                            End If
                        Next
                    Next
                Next
            ElseIf Now.Hour <> 15 Then
                StatusRun = True
            End If
            Threading.Thread.Sleep(1000)
        End While
    End Sub
End Class
