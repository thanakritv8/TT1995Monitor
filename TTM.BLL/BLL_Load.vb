Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Windows.Forms
Imports TTM.DAL

Public Class BLL_Load
#Region "Develop By Thung"

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

    Public Sub UpdateLogMonitor(ByVal log_id As Integer)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "UPDATE log_monitor set send_status = 1 where log_id = " & log_id
        objDB.ExecuteSQL(_SQL, Con)
        objDB.DisconnectDB(Con)
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
                Dim dateCondition As DateTime = DateTime.Parse(_Item("tax_expire")).AddDays(-90)
                If dateCondition <= Now Then
                    Dim _Msg As String = "ใบประกอบการเลขที่ " & _Item("business_number") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                    UpdateBusinessIn(3, _Item("business_id"), _Msg, keyWho, _Item("business_status"), 0)
                    'SendLine(keyWho, _Msg)
                    'Insert log, send line and update flag_status = 0 update_status = now
                End If
            Else
                If _Item("flag_status") = 0 Then
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                    If dateCondition >= Now Then
                        Dim _Msg As String = "ใบประกอบการเลขที่ " & _Item("business_number") & " มีสถานะ " & _Item("business_status") & " เกิน " & _Item("notify_day") & " วัน"
                        UpdateBusinessIn(3, _Item("business_id"), _Msg, keyWho, _Item("business_status"), 1)
                        'SendLine(keyWho, _Msg)
                        'Insert log, send line and update flag_status = 1 update_status = now
                    End If
                Else
                    Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                    If dateCondition >= Now Then
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
    Private Sub UpdateTax(ByVal table_id As Integer, ByVal fk_id As Integer, ByVal txt_msg As String, ByVal send_who As String, ByVal log_status As String, ByVal flag_status As Integer)
        Dim Con As SqlConnection = objDB.ConnectDB(My.Settings.NameServer, My.Settings.Username, My.Settings.Password, My.Settings.DataBase)
        Dim _SQL As String = "INSERT INTO log_monitor (table_id, fk_id, txt_msg, send_who, log_status) VALUES (" & table_id & ", " & fk_id & ", N'" & txt_msg & "', '" & send_who & "', N'" & log_status & "')"
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

    Public Function CheckStatusTaxNotify() As String
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
                        Dim _Msg As String = "เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " ถูกเปลี่ยนสถานะจาก เสร็จสมบูรณ์ เป็น ยังไม่ได้ดำเนินการ"
                        UpdateTax(3, _Item("tax_id"), _Msg, keyWho, _Item("tax_status"), 0)
                        'Insert log, send line and update flag_status = 0 update_status = now
                        _MsgReturn = _Msg
                    End If
                Else
                    If _Item("flag_status") = 0 Then
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day"))
                        If dateCondition >= Now Then
                            Dim _Msg As String = "เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("tax_status") & " เกิน " & _Item("notify_day") & " วัน"
                            UpdateTax(3, _Item("tax_id"), _Msg, keyWho, _Item("tax_status"), 1)                            'SendLine(keyWho, _Msg)
                            'Insert log, send line and update flag_status = 1 update_status = now
                            _MsgReturn = _Msg
                        End If
                    Else
                        Dim dateCondition As DateTime = DateTime.Parse(_Item("update_last")).AddDays(_Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")))
                        If dateCondition >= Now Then
                            Dim _Msg As String = "เบอร์รถ " & DtLicense.Rows(0)("number_car") & " ทะเบียน " & DtLicense.Rows(0)("license_car") & " มีสถานะ " & _Item("tax_status") & " เกิน " & _Item("notify_day") + (_Item("frequency_day") * _Item("flag_status")) & " วัน"
                            UpdateTax(3, _Item("tax_id"), _Msg, keyWho, _Item("tax_status"), _Item("flag_status") + 1)
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

#Region "Line"

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

#End Region

#Region "Develop By Tew"

#End Region

#Region "Develop By Poom"

#End Region
End Class
