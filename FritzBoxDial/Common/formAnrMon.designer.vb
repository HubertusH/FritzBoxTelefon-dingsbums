<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class formAnrMon
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItemKontakt�ffnen = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemR�ckruf = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItemKopieren = New System.Windows.Forms.ToolStripMenuItem()
        Me.PopUpAnrMon = New FritzBoxDial.PopUpAnrMon()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItemKontakt�ffnen, Me.ToolStripMenuItemR�ckruf, Me.ToolStripMenuItemKopieren})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(222, 70)
        '
        'ToolStripMenuItemKontakt�ffnen
        '
        Me.ToolStripMenuItemKontakt�ffnen.Image = Global.FritzBoxDial.My.Resources.Resources.Bild4
        Me.ToolStripMenuItemKontakt�ffnen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None
        Me.ToolStripMenuItemKontakt�ffnen.Name = "ToolStripMenuItemKontakt�ffnen"
        Me.ToolStripMenuItemKontakt�ffnen.Size = New System.Drawing.Size(221, 22)
        Me.ToolStripMenuItemKontakt�ffnen.Text = "Kontakt �ffnen"
        '
        'ToolStripMenuItemR�ckruf
        '
        Me.ToolStripMenuItemR�ckruf.Image = Global.FritzBoxDial.My.Resources.Resources.Bild2
        Me.ToolStripMenuItemR�ckruf.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None
        Me.ToolStripMenuItemR�ckruf.Name = "ToolStripMenuItemR�ckruf"
        Me.ToolStripMenuItemR�ckruf.Size = New System.Drawing.Size(221, 22)
        Me.ToolStripMenuItemR�ckruf.Text = "R�ckruf"
        '
        'ToolStripMenuItemKopieren
        '
        Me.ToolStripMenuItemKopieren.Image = Global.FritzBoxDial.My.Resources.Resources.Bild5
        Me.ToolStripMenuItemKopieren.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None
        Me.ToolStripMenuItemKopieren.Name = "ToolStripMenuItemKopieren"
        Me.ToolStripMenuItemKopieren.Size = New System.Drawing.Size(221, 22)
        Me.ToolStripMenuItemKopieren.Text = "In Zwischenablage kopieren"
        '
        'PopUpAnrMon
        '
        Me.PopUpAnrMon.AnrName = Nothing
        Me.PopUpAnrMon.ContentFont = New System.Drawing.Font("Tahoma", 8.0!)
        Me.PopUpAnrMon.Firma = Nothing
        Me.PopUpAnrMon.Image = Nothing
        Me.PopUpAnrMon.ImagePosition = New System.Drawing.Point(12, 21)
        Me.PopUpAnrMon.ImageSize = New System.Drawing.Size(32, 32)
        Me.PopUpAnrMon.MoveDirecktion = FritzBoxDial.PopUpAnrMon.eMoveDirection.Y
        Me.PopUpAnrMon.OptionsMenu = Nothing
        Me.PopUpAnrMon.PositionsKorrektur = New System.Drawing.Size(0, 0)
        Me.PopUpAnrMon.Size = New System.Drawing.Size(400, 100)
        Me.PopUpAnrMon.Startpunkt = FritzBoxDial.PopUpAnrMon.eStartPosition.BottomRight
        Me.PopUpAnrMon.TelName = Nothing
        Me.PopUpAnrMon.TelNr = Nothing
        Me.PopUpAnrMon.TelNrFont = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.PopUpAnrMon.TextPadding = New System.Windows.Forms.Padding(0)
        Me.PopUpAnrMon.TitleFont = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.PopUpAnrMon.Uhrzeit = Nothing
        '
        'formAnrMon
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(155, 56)
        Me.Name = "formAnrMon"
        Me.Text = "Form1"
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents PopupNotifier As PopUpAnrMon
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ToolStripMenuItemR�ckruf As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItemKopieren As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItemKontakt�ffnen As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PopUpAnrMon As FritzBoxDial.PopUpAnrMon

End Class
