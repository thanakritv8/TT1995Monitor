Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Windows.Forms
Imports TTM.DAL

Public Class BLL_Load

#Region "Utility"

    Private keyWho As String = "snNseQbdEIMtzUFx0uN3JQt2YhCtjZFbW0EyGFxQoWt"
    Public Property _keyWho As String
        Get
            Return keyWho
        End Get
        Set(value As String)
            keyWho = value
        End Set
    End Property

    Public Function GetTable() As DataTable
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select DISTINCT(ct.name_table) as name_table, ct.display from config_table as ct join config_column as cc on ct.table_id = cc.table_id join lookup as lu on cc.column_id = lu.column_id join config_monitor as cm on lu.lookup_id = cm.lookup_id"
        Dim DT As DataTable = objDB.SelectSQL(_SQL, Con)
        objDB.DisconnectDB(Con)
        Return DT
    End Function

    Public Function GetMsgLine() As DataTable
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select * from log_monitor where send_status is null"
        Dim DT As DataTable = objDB.SelectSQL(_SQL, Con)
        objDB.DisconnectDB(Con)
        Return DT
    End Function

    Public Function GetStatus(ByVal TableId As Integer) As DataTable
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select lu.data_list from config_column as cc join lookup as lu on cc.column_id = lu.column_id join config_monitor as cm on cm.lookup_id = lu.lookup_id where cc.table_id = " & TableId
        Dim DT As DataTable = objDB.SelectSQL(_SQL, Con)
        objDB.DisconnectDB(Con)
        Return DT
    End Function

    Public Sub UpdateLogMonitor(ByVal log_id As Integer)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "UPDATE log_monitor set send_status = 1 where log_id = " & log_id
        objDB.ExecuteSQL(_SQL, Con)
        objDB.DisconnectDB(Con)
    End Sub

    Public Function GetDataLC(ByVal fk_id As Integer) As DataTable
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select lc.lc_number as number, (select N'เบอร์รถหัว' + number_car + '(' + license_car + ')' from license where license_id = lcp.license_id_head) as number_head, (select N'เบอร์รถท้าย' + number_car + '(' + license_car + ')' from license where license_id = lcp.license_id_tail) as number_tail from license_cambodia_permission as lcp join license_cambodia as lc on lcp.lc_id = lc.lc_id where lcp.lc_id =" & fk_id
        Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
        objDB.DisconnectDB(Con)
        Return DtLicense
    End Function
    Public Function GetDataLMR(ByVal fk_id As Integer) As DataTable
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select lmr.lmr_number as number, (select N'เบอร์รถหัว' + number_car + '(' + license_car + ')' from license where license_id = lmrp.license_id_head) as number_head, (select N'เบอร์รถท้าย' + number_car + '(' + license_car + ')' from license where license_id = lmrp.license_id_tail) as number_tail from license_mekong_river_permission as lmrp join license_mekong_river as lmr on lmrp.lmr_id = lmr.lmr_id where lmrp.lmr_id =" & fk_id
        Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
        objDB.DisconnectDB(Con)
        Return DtLicense
    End Function

    Public Sub SendLine(ByVal _Who As String, ByVal _Msg As String)
        Try
            Cursor.Current = Cursors.WaitCursor
            System.Net.ServicePointManager.Expect100Continue = False
            Dim request = DirectCast(WebRequest.Create("https://notify-api.line.me/api/notify”), HttpWebRequest)
            Dim postData = String.Format("message={0}", _Msg.Replace("%", ""))
            Dim data = Encoding.UTF8.GetBytes(postData)
            request.Method = "POST"
            request.ContentType = "application/x-www-form-urlencoded"
            request.ContentLength = data.Length
            request.Headers.Add("Authorization", "Bearer " & _Who)
            request.AllowWriteStreamBuffering = True
            request.KeepAlive = False
            request.Credentials = CredentialCache.DefaultCredentials
            Using stream = request.GetRequestStream()
                stream.Write(data, 0, data.Length)
            End Using
            Dim response = DirectCast(request.GetResponse(), HttpWebResponse)
            Dim responseString = New StreamReader(response.GetResponseStream()).ReadToEnd()
        Catch ex As Exception
            'MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
        Finally
            Cursor.Current = Cursors.Default
        End Try
    End Sub

#End Region

#Region "Business IN"
    Private Sub UpdateBusinessIn(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal txt_msg As String, ByVal send_who As String, ByVal log_status As String, ByVal flag_status As Integer)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, txt_msg, send_who, log_status) VALUES (" & table_id & ", " & fk_id & ", N'" & txt_msg & "', '" & send_who & "', N'" & log_status & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE business_in SET business_status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE business_id = " & fk_id
            Else
                _SQL = "UPDATE business_in SET flag_status = " & flag_status & " WHERE business_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub

    Public Sub CheckStatusBusinessInNotify()
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select bi.business_id, bi.business_number, bi.business_status, bi.business_expire, isnull(bi.update_status, bi.create_date) as update_last, isnull(bi.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day"
        _SQL &= "from business_in As bi left join lookup As lu On bi.business_status = lu.data_list join config_monitor As cm On lu.lookup_id = cm.lookup_id where lu.column_id = 272 And bi.business_status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            If _Item("business_status") = "เสร็จสมบูรณ์" Then
                Dim dateCondition As DateTime = DateTime.Parse(_Item("business_expire")).AddDays(-90)
                If dateCondition <= Now Then
                    Dim _Msg As String = "ใบประกอบการเลขที่ " & _Item("business_number") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                    UpdateBusinessIn(3, _Item("business_id"), _Msg, keyWho, _Item("business_status"), 0)
                    'SendLine(keyWho, _Msg)
                    'Insert log, send line and update flag_status = 0 update_status = now
                End If
            Else
                If _Item("flag_status") = 0 Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                    If dateCondition <= Now Then
                        Dim _Msg As String = "ใบประกอบการเลขที่ " & _Item("business_number") & " มีสถานะ " & _Item("business_status") & " เกิน " & _Item("notify_day") & " วัน"
                        UpdateBusinessIn(3, _Item("business_id"), _Msg, keyWho, _Item("business_status"), 1)
                        'SendLine(keyWho, _Msg)
                        'Insert log, send line and update flag_status = 1 update_status = now
                    End If
                Else
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                    If dateCondition <= Now Then
                        Dim _Msg As String = "ใบประกอบการเลขที่ " & _Item("business_number") & " มีสถานะ " & _Item("business_status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                        UpdateBusinessIn(3, _Item("business_id"), _Msg, keyWho, _Item("business_status"), _Item("flag_status") + 1)
                        'SendLine(keyWho, _Msg)
                        'Insert log, send line and update flag_status + 1
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub

#End Region

#Region "Tax"
    Private Sub UpdateTax(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer, ByVal number_car As String, ByVal license_car As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days, number_car, license_car) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ", '" & number_car & "', '" & license_car & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE tax SET tax_status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE tax_id = " & fk_id
            Else
                _SQL = "UPDATE tax SET flag_status = " & flag_status & " WHERE tax_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub

    Public Function CheckStatusTaxNotify(ByVal _ProcessName As String) As String
        Dim _MsgReturn As String = String.Empty
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select t.tax_id, t.license_id, t.tax_status, t.tax_expire, isnull(t.update_status, t.create_date) as update_last, isnull(t.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day"
        _SQL &= " from tax as t left join lookup as lu on t.tax_status = lu.data_list join config_monitor as cm on lu.lookup_id = cm.lookup_id where lu.column_id = 46 and t.tax_status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            _SQL = "SELECT number_car, license_car FROM license WHERE license_id = " & _Item("license_id")
            Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
            If DtLicense.Rows.Count > 0 Then
                If _Item("tax_status") = "เสร็จสมบูรณ์" Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("tax_expire")).AddDays(-90)
                    If dateCondition <= Now Then
                        Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateTax(3, _Item("tax_id"), _Item("tax_status"), 0, 0, DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        'Insert log, send line and update flag_status = 0 update_status = now
                        _MsgReturn = _Msg
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition <= Now Then
                            Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("tax_status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateTax(3, _Item("tax_id"), _Item("tax_status"), 1, _Item("notify_day"), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))                            'SendLine(keyWho, _Msg)
                            'Insert log, send line and update flag_status = 1 update_status = now
                            _MsgReturn = _Msg
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition <= Now Then
                            Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("tax_status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateTax(3, _Item("tax_id"), _Item("tax_status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                            'Insert log, send line and update flag_status + 1
                            _MsgReturn = _Msg
                        End If
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
        Return _MsgReturn
    End Function

#End Region

#Region "license_mekong_river ใบอนุญาตลุ่มน้ำโขง"
    Private Sub UpdateLMR(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ")"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE license_mekong_river SET lmr_status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE lmr_id = " & fk_id
            Else
                _SQL = "UPDATE license_mekong_river SET flag_status = " & flag_status & " WHERE lmr_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub

    Public Sub CheckStatusLMRNotify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select lmr.lmr_id, lmr.lmr_number, lmr.lmr_status, lmr.lmr_expire, isnull(lmr.update_status, lmr.create_date) as update_last, isnull(lmr.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day"
        _SQL &= " from license_mekong_river As lmr left join lookup As lu On lmr.lmr_status = lu.data_list join config_monitor As cm On lu.lookup_id = cm.lookup_id where lu.column_id = 328 And lmr.lmr_status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            If _Item("lmr_status") = "เสร็จสมบูรณ์" Then
                Dim dateCondition As DateTime = DateTime.Parse(_Item("lc_expire")).AddDays(-90)
                If dateCondition <= Now Then
                    UpdateLC(23, _Item("lmr_id"), _Item("lmr_status"), 0, 0)
                End If
            Else
                If _Item("flag_status") = 0 Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                    If dateCondition <= Now Then
                        UpdateLC(23, _Item("lmr_id"), _Item("lmr_status"), 1, _Item("notify_day"))
                    End If
                Else
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                    If dateCondition <= Now Then
                        UpdateLC(23, _Item("lmr_id"), _Item("lmr_status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub
#End Region

#Region "license_cambodia ใบอนุญาตกัมพูชา"
    Private Sub UpdateLC(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ")"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE license_cambodia SET lc_status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE lc_id = " & fk_id
            Else
                _SQL = "UPDATE license_cambodia SET flag_status = " & flag_status & " WHERE lc_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub

    Public Sub CheckStatusLCNotify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select lc.lc_id, lc.lc_number, lc.lc_status, lc.lc_expire, isnull(lc.update_status, lc.create_date) as update_last, isnull(lc.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day"
        _SQL &= " from license_cambodia As lc left join lookup As lu On lc.lc_status = lu.data_list join config_monitor As cm On lu.lookup_id = cm.lookup_id where lu.column_id = 318 And lc.lc_status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            If _Item("lc_status") = "เสร็จสมบูรณ์" Then
                Dim dateCondition As DateTime = DateTime.Parse(_Item("lc_expire")).AddDays(-90)
                If dateCondition <= Now Then
                    UpdateLC(22, _Item("lc_id"), _Item("lc_status"), 0, 0)
                End If
            Else
                If _Item("flag_status") = 0 Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                    If dateCondition <= Now Then
                        UpdateLC(22, _Item("lc_id"), _Item("lc_status"), 1, _Item("notify_day"))
                    End If
                Else
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                    If dateCondition <= Now Then
                        UpdateLC(22, _Item("lc_id"), _Item("lc_status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub
#End Region

#Region "license_v8 ใบอนุญาต วอ.8"
    Private Sub UpdateLV8(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer, ByVal number_car As String, ByVal license_car As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days, number_car, license_car) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ", '" & number_car & "', '" & license_car & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE license_v8 SET status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE lv8_id = " & fk_id
            Else
                _SQL = "UPDATE license_v8 SET flag_status = " & flag_status & " WHERE lv8_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub
    Public Sub CheckStatusLV8Notify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select lv8.lv8_id, lv8.license_id, lv8.lv8_status, lv8.lv8_expire, isnull(lv8.update_status, lv8.create_date) as update_last, isnull(lv8.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day"
        _SQL &= " from license_v8 as lv8 left join lookup as lu on lv8.lv8_status = lu.data_list join config_monitor as cm on lu.lookup_id = cm.lookup_id where lu.column_id = 46 and lv8.lv8_status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            _SQL = "SELECT number_car, license_car FROM license WHERE license_id = " & _Item("license_id")
            Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
            If DtLicense.Rows.Count > 0 Then
                If _Item("lv8_status") = "เสร็จสมบูรณ์" Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("end_date")).AddDays(-90)
                    If dateCondition <= Now Then
                        'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateLV8(28, _Item("lv8_id"), _Item("lv8_status"), 0, 0, DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("lv8_status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateLV8(28, _Item("lv8_id"), _Item("lv8_status"), 1, _Item("notify_day"), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("lv8_status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateLV8(28, _Item("lv8_id"), _Item("lv8_status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub
#End Region

#Region "license_factory ใบอนุญาตโรงงาน"
    Private Sub UpdateLF(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer, ByVal driver_name As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days, driver_name) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ", '" & driver_name & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE license_factory SET status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE license_factory_id = " & fk_id
            Else
                _SQL = "UPDATE license_factory SET flag_status = " & flag_status & " WHERE license_factory_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub

    Public Sub CheckStatusLFNotify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select lf.license_factory_id, lf.driver_id, lf.license_factory_status, lf.expire_date, isnull(lf.update_status, lf.create_date) as update_last, isnull(lf.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day "
        _SQL &= " from license_factory as lf left join lookup as lu on lf.license_factory_status = lu.data_list join config_monitor as cm on lu.lookup_id = cm.lookup_id where lu.column_id = 46 and lf.license_factory_status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            _SQL = "SELECT driver_name FROM driver WHERE driver_id = " & _Item("driver_id")
            Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
            If DtLicense.Rows.Count > 0 Then
                If _Item("license_factory_status") = "เสร็จสมบูรณ์" Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("expire_date")).AddDays(-90)
                    If dateCondition <= Now Then
                        'Dim _Msg As String = "(" & _ProcessName & ") พนักงานขับรถ " & DtLicense.Rows(0)("driver_name") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateLF(30, _Item("license_factory_id"), _Item("license_factory_status"), 0, 0, DtLicense.Rows(0)("driver_name"))
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") พนักงานขับรถ " & DtLicense.Rows(0)("driver_name") & " มีสถานะ " & _Item("license_factory_status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateLF(30, _Item("license_factory_id"), _Item("license_factory_status"), 1, _Item("notify_day"), DtLicense.Rows(0)("driver_name"))
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") พนักงานขับรถ " & DtLicense.Rows(0)("driver_name") & " มีสถานะ " & _Item("license_factory_status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateLF(30, _Item("license_factory_id"), _Item("license_factory_status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")), DtLicense.Rows(0)("driver_name"))
                        End If
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub
#End Region

#Region "act_insurance ประกันพรบ."
    Private Sub UpdateActInsurance(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer, ByVal number_car As String, ByVal license_car As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days, number_car, license_car) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ", '" & number_car & "', '" & license_car & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE act_insurance SET status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE ai_id = " & fk_id
            Else
                _SQL = "UPDATE act_insurance SET flag_status = " & flag_status & " WHERE ai_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub
    Public Sub CheckStatusActInsuranceNotify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select ai.ai_id, ai.license_id, ai.status, ai.end_date, isnull(ai.update_status, ai.create_date) as update_last, isnull(ai.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day"
        _SQL &= " from act_insurance as ai left join lookup as lu on ai.status = lu.data_list join config_monitor as cm on lu.lookup_id = cm.lookup_id where lu.column_id = 46 and ai.status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            _SQL = "SELECT number_car, license_car FROM license WHERE license_id = " & _Item("license_id")
            Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
            If DtLicense.Rows.Count > 0 Then
                If _Item("status") = "เสร็จสมบูรณ์" Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("end_date")).AddDays(-90)
                    If dateCondition <= Now Then
                        'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateActInsurance(17, _Item("ai_id"), _Item("status"), 0, 0, DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateActInsurance(17, _Item("ai_id"), _Item("status"), 1, _Item("notify_day"), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateActInsurance(17, _Item("ai_id"), _Item("status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub

#End Region

#Region "main_insurance ประกันภัยรถยนต์"
    Private Sub UpdateMainInsurance(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer, ByVal number_car As String, ByVal license_car As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days, number_car, license_car) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ", '" & number_car & "', '" & license_car & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE main_insurance SET status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE mi_id = " & fk_id
            Else
                _SQL = "UPDATE main_insurance SET flag_status = " & flag_status & " WHERE mi_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub
    Public Sub CheckStatusMainInsuranceNotify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select mi.mi_id, mi.license_id, mi.status, mi.end_date, isnull(mi.update_status, mi.create_date) as update_last, isnull(mi.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day"
        _SQL &= " from main_insurance as mi left join lookup as lu on mi.status = lu.data_list join config_monitor as cm on lu.lookup_id = cm.lookup_id where lu.column_id = 46 and mi.status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            _SQL = "SELECT number_car, license_car FROM license WHERE license_id = " & _Item("license_id")
            Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
            If DtLicense.Rows.Count > 0 Then
                If _Item("status") = "เสร็จสมบูรณ์" Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("end_date")).AddDays(-90)
                    If dateCondition <= Now Then
                        'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateMainInsurance(15, _Item("mi_id"), _Item("status"), 0, 0, DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateMainInsurance(15, _Item("mi_id"), _Item("status"), 1, _Item("notify_day"), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateMainInsurance(15, _Item("mi_id"), _Item("status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub

#End Region

#Region "environment_insurance ประกันภัยสิ่งแวดล้อม"
    Private Sub UpdateEnvInsurance(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer, ByVal number_car As String, ByVal license_car As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days, number_car, license_car) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ", '" & number_car & "', '" & license_car & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE environment_insurance SET status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE ei_id = " & fk_id
            Else
                _SQL = "UPDATE environment_insurance SET flag_status = " & flag_status & " WHERE ei_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub
    Public Sub CheckStatusEnvInsuranceNotify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select ei.ei_id, ei.license_id, ei.status, ei.end_date, isnull(ei.update_status, ei.create_date) as update_last, isnull(ei.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day "
        _SQL &= " from environment_insurance as ei left join lookup as lu on ei.status = lu.data_list join config_monitor as cm on lu.lookup_id = cm.lookup_id where lu.column_id = 46 and ei.status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            _SQL = "SELECT number_car, license_car FROM license WHERE license_id = " & _Item("license_id")
            Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
            If DtLicense.Rows.Count > 0 Then
                If _Item("status") = "เสร็จสมบูรณ์" Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("end_date")).AddDays(-90)
                    If dateCondition <= Now Then
                        Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateEnvInsurance(18, _Item("ei_id"), _Item("status"), 0, 0, DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition <= Now Then
                            Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateEnvInsurance(18, _Item("ei_id"), _Item("status"), 1, _Item("notify_day"), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition <= Now Then
                            Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateEnvInsurance(18, _Item("ei_id"), _Item("status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub
#End Region

#Region "domestic_product_insurance ประกันภัยสินค้าภายในประเทศ"
    Private Sub UpdateDPI(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal log_status As String, ByVal flag_status As Integer, ByVal days As Integer, ByVal number_car As String, ByVal license_car As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, log_status, log_days, number_car, license_car) VALUES (" & table_id & ", " & fk_id & ", N'" & log_status & "', " & days & ", '" & number_car & "', '" & license_car & "')"
        If objDB.ExecuteSQL(_SQL, Con) Then
            If flag_status = 0 Then
                _SQL = "UPDATE domestic_product_insurance SET status = N'ยังไม่ได้ดำเนินการ', flag_status = " & flag_status & ", update_status = GETDATE() WHERE dpi_id = " & fk_id
            Else
                _SQL = "UPDATE domestic_product_insurance SET flag_status = " & flag_status & " WHERE dpi_id = " & fk_id
            End If
            If objDB.ExecuteSQL(_SQL, Con) Then
                'Success
            Else
                'Error
            End If
        Else
            'Error
        End If
        objDB.DisconnectDB(Con)
    End Sub
    Public Sub CheckStatusDPINotify(ByVal _ProcessName As String)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "select dpi.dpi_id, dpi.license_id, dpi.status, dpi.end_date, isnull(dpi.update_status, dpi.create_date) as update_last, isnull(dpi.flag_status,0) as flag_status, cm.notify_day, cm.frequency_day "
        _SQL &= " from domestic_product_insurance as dpi left join lookup as lu on dpi.status = lu.data_list join config_monitor as cm on lu.lookup_id = cm.lookup_id where lu.column_id = 46 and dpi.status <> ''"
        Dim _Dt As DataTable = objDB.SelectSQL(_SQL, Con)
        For Each _Item In _Dt.Rows
            _SQL = "SELECT number_car, license_car FROM license WHERE license_id = " & _Item("license_id")
            Dim DtLicense As DataTable = objDB.SelectSQL(_SQL, Con)
            If DtLicense.Rows.Count > 0 Then
                If _Item("status") = "เสร็จสมบูรณ์" Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("end_date")).AddDays(-90)
                    If dateCondition <= Now Then
                        'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateDPI(16, _Item("dpi_id"), _Item("status"), 0, 0, DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateDPI(16, _Item("dpi_id"), _Item("status"), 1, _Item("notify_day"), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition <= Now Then
                            'Dim _Msg As String = "(" & _ProcessName & ") เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateDPI(16, _Item("dpi_id"), _Item("status"), _Item("flag_status") + 1, _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")), DtLicense.Rows(0)("number_car"), DtLicense.Rows(0)("license_car"))
                        End If
                    End If
                End If
            End If
        Next
        objDB.DisconnectDB(Con)
    End Sub
#End Region

End Class
