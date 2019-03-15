<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class usc
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lbControl = New System.Windows.Forms.ListBox()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.lbProcessName = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lbControl
        '
        Me.lbControl.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lbControl.FormattingEnabled = True
        Me.lbControl.Location = New System.Drawing.Point(3, 28)
        Me.lbControl.Name = "lbControl"
        Me.lbControl.Size = New System.Drawing.Size(137, 191)
        Me.lbControl.TabIndex = 0
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.lbControl, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.lbProcessName, 0, 0)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.46245!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88.53755!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(143, 222)
        Me.TableLayoutPanel1.TabIndex = 1
        '
        'lbProcessName
        '
        Me.lbProcessName.AutoSize = True
        Me.lbProcessName.BackColor = System.Drawing.Color.DodgerBlue
        Me.lbProcessName.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lbProcessName.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbProcessName.ForeColor = System.Drawing.SystemColors.Window
        Me.lbProcessName.Location = New System.Drawing.Point(3, 0)
        Me.lbProcessName.Name = "lbProcessName"
        Me.lbProcessName.Size = New System.Drawing.Size(137, 25)
        Me.lbProcessName.TabIndex = 1
        Me.lbProcessName.Text = "Label1"
        Me.lbProcessName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'usc
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "usc"
        Me.Size = New System.Drawing.Size(143, 222)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lbControl As ListBox
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents lbProcessName As Label
End Class
