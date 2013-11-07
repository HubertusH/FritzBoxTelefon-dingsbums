Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports System.Threading
Imports System.Windows.Forms

Friend Class formCfg
    Private C_XML As MyXML
    Private C_Crypt As Rijndael
    Private C_Helfer As Helfer
    Private C_Kontakte As Contacts
    Private C_Phoner As PhonerInterface
    Private GUI As GraphicalUserInterface
    Private OlI As OutlookInterface
    Private AnrMon As AnrufMonitor
    Private C_FBox As FritzBox

    Private WithEvents BWTelefone As BackgroundWorker
    Private WithEvents BWIndexer As BackgroundWorker
    Private WithEvents emc As New EventMulticaster

    Private tmpCheckString As String
    Private StatusWert As String
    Private KontaktName As String
    Private Anzahl As Integer = 0
    Private Dauer As TimeSpan
    Private Startzeit As Date

    Private Delegate Sub DelgButtonTelEinl()
    Private Delegate Sub DelgSetLine()
    Private Delegate Sub DelgStatistik()
    Private Delegate Sub DelgSetProgressbar()

    Public Sub New(ByVal InterfacesKlasse As GraphicalUserInterface, _
                   ByVal XMLKlasse As MyXML, _
                   ByVal HelferKlasse As Helfer, _
                   ByVal CryptKlasse As Rijndael, _
                   ByVal AnrufMon As AnrufMonitor, _
                   ByVal fritzboxKlasse As FritzBox, _
                   ByVal OutlInter As OutlookInterface, _
                   ByVal kontaktklasse As Contacts, _
                   ByVal Phonerklasse As PhonerInterface)

        ' Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent()
        ' F�gen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        C_Helfer = HelferKlasse
        C_XML = XMLKlasse
        C_Crypt = CryptKlasse
        GUI = InterfacesKlasse
        OlI = OutlInter
        AnrMon = AnrufMon
        C_FBox = fritzboxKlasse
        C_Kontakte = kontaktklasse
        C_Phoner = Phonerklasse
    End Sub

    Private Sub UserForm_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Me.TBAnrMonMoveGeschwindigkeit.BackColor = CType(IIf(iTa.IsThemeActive, SystemColors.ControlLightLight, SystemColors.ControlLight), Color)
        Me.BAnrMonTest.Enabled = Not AnrMon Is Nothing
        Me.BTelefonliste.Enabled = Not C_FBox Is Nothing
        Ausf�llen()
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

#Region "Ausf�llen"

    Private Sub Ausf�llen()
        Me.ToolTipFBDBConfig.SetToolTip(Me.ButtonXML, "�ffnet die Datei " & vbCrLf & C_XML.GetXMLDateiPfad)
        Dim Passwort As String
#If OVer >= 14 Then
        If Not Me.FBDB_MP.TabPages.Item("PSymbolleiste") Is Nothing Then
            Me.FBDB_MP.TabPages.Remove(Me.FBDB_MP.TabPages.Item("PSymbolleiste"))
        End If
#End If
        ' Beim Einblenden die Werte aus der Registry einlesen
        Me.Label7.Text += ThisAddIn.Version
        ' Einstellungen f�r das W�hlmakro laden
        Me.TBLandesVW.Text = C_XML.Read("Optionen", "TBLandesVW", "0049")
        Me.TBAmt.Text = C_XML.Read("Optionen", "TBAmt", "")
        Me.TBAmt.Text = CStr(IIf(Me.TBAmt.Text = "-1", "", Me.TBAmt.Text))
        Me.TBFBAdr.Text = C_XML.Read("Optionen", "TBFBAdr", ThisAddIn.P_FritzBox.P_DefaultFBAddr)
        Me.CBForceFBAddr.Checked = CBool(IIf(C_XML.Read("Optionen", "CBForceFBAddr", "False") = "True", True, False))
        Me.TBBenutzer.Text = C_XML.Read("Optionen", "TBBenutzer", vbNullString)
        If C_XML.Read("Optionen", Me.TBBenutzer.Text, "2") = "0" Then
            Me.TBBenutzer.BackColor = Color.Red
            Me.ToolTipFBDBConfig.SetToolTip(Me.TBBenutzer, "Der Benutzer " & Me.TBBenutzer.Text & " hat keine ausreichenden Berechtigungen auf der Fritz!Box.")
        End If
        Passwort = C_XML.Read("Optionen", "TBPasswort", "")
        If Not Len(Passwort) = 0 Then
            Me.TBPasswort.Text = "1234"
        End If
        Me.TBVorwahl.Text = C_XML.Read("Optionen", "TBVorwahl", "")
        Me.TBEnblDauer.Text = CStr(CInt(C_XML.Read("Optionen", "TBEnblDauer", "10")))
        Me.CBAnrMonAuto.Checked = CBool(C_XML.Read("Optionen", "CBAnrMonAuto", "False"))
        Me.TBAnrMonX.Text = C_XML.Read("Optionen", "TBAnrMonX", "0")
        Me.TBAnrMonY.Text = C_XML.Read("Optionen", "TBAnrMonY", "0")
        Me.CBAnrMonMove.Checked = CBool(IIf(C_XML.Read("Optionen", "CBAnrMonMove", "True") = "True", True, False))
        Me.CBAnrMonTransp.Checked = CBool(IIf(C_XML.Read("Optionen", "CBAnrMonTransp", "True") = "True", True, False))
        Me.TBAnrMonMoveGeschwindigkeit.Value = CInt((100 - CDbl(C_XML.Read("Optionen", "TBAnrMonMoveGeschwindigkeit", "50"))) / 10)
        Me.CBAnrMonContactImage.Checked = CBool(IIf(C_XML.Read("Optionen", "CBAnrMonContactImage", "True") = "True", True, False))
        Me.CBIndexAus.Checked = CBool(C_XML.Read("Optionen", "CBIndexAus", "False"))
        Me.CBShowMSN.Checked = CBool(C_XML.Read("Optionen", "CBShowMSN", "False"))
        ' optionale allgemeine Einstellungen laden
        Me.CBAutoClose.Checked = CBool(IIf(C_XML.Read("Optionen", "CBAutoClose", "True") = "True", True, False))
        Me.CBVoIPBuster.Checked = CBool(IIf(C_XML.Read("Optionen", "CBVoIPBuster", "False") = "True", True, False))
        Me.ToolTipFBDBConfig.SetToolTip(Me.CBVoIPBuster, "Mit dieser Einstellung wird die Landesvorwahl " & Me.TBLandesVW.Text & " immer mitgew�hlt.")
        Me.CBCbCunterbinden.Checked = CBool(IIf(C_XML.Read("Optionen", "CBCbCunterbinden", "False") = "True", True, False))
        Me.CBCallByCall.Checked = CBool(IIf(C_XML.Read("Optionen", "CBCallByCall", "False") = "True", True, False))
        Me.CBDialPort.Checked = CBool(IIf(C_XML.Read("Optionen", "CBDialPort", "False") = "True", True, False))
        Me.CBRueckwaertssuche.Checked = CBool(IIf(C_XML.Read("Optionen", "CBRueckwaertssuche", "False") = "True", True, False))
        Me.CBKErstellen.Checked = CBool(IIf(C_XML.Read("Optionen", "CBKErstellen", "False") = "True", True, False))
        Me.CBLogFile.Checked = CBool(IIf(C_XML.Read("Optionen", "CBLogFile", "False") = "True", True, False))
#If OVer < 14 Then
        ' Einstellungen f�r die Symbolleiste laden
        Me.CBSymbWwdh.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbWwdh", "True") = "True", True, False))
        Me.CBSymbAnrMon.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbAnrMon", "True") = "True", True, False))
        Me.CBSymbAnrMonNeuStart.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbAnrMonNeuStart", "False") = "True", True, False))
        Me.CBSymbAnrListe.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbAnrListe", "True") = "True", True, False))
        Me.CBSymbDirekt.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbDirekt", "True") = "True", True, False))
        Me.CBSymbRWSuche.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbRWSuche", "True") = "True", True, False))
        Me.CBSymbVIP.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbVIP", "False") = "True", True, False))
        Me.CBSymbJournalimport.Checked = CBool(IIf(C_XML.Read("Optionen", "CBSymbJournalimport", "False") = "True", True, False))
#End If
        Me.CBJImport.Checked = CBool(IIf(C_XML.Read("Optionen", "CBJImport", "False") = "True", True, False))
        ' Einstellungen f�er die R�ckw�rtssuche laden
        Me.CBKHO.Checked = CBool(IIf(C_XML.Read("Optionen", "CBKHO", "True") = "True", True, False))
        Me.CBRWSIndex.Checked = CBool(IIf(C_XML.Read("Optionen", "CBRWSIndex", "True") = "True", True, False))
        With Me.ComboBoxRWS.Items
            .Add("11880.com")
            .Add("DasTelefonbuch.de")
            .Add("tel.search.ch")
            .Add("Alle")
        End With

        Me.ComboBoxRWS.SelectedItem = Me.ComboBoxRWS.Items.Item(CInt(C_XML.Read("Optionen", "CBoxRWSuche", "0")))
        If Not Me.CBRueckwaertssuche.Checked Then Me.ComboBoxRWS.Enabled = False
        ' Einstellungen f�r das Journal laden
        Me.CBJournal.Checked = CBool(IIf(C_XML.Read("Optionen", "CBJournal", "False") = "True", True, False))

        Me.CBUseAnrMon.Checked = CBool(C_XML.Read("Optionen", "CBUseAnrMon", "True"))
        Me.CBIndexAus.Enabled = Not Me.CBUseAnrMon.Checked
        Me.PanelAnrMon.Enabled = Me.CBUseAnrMon.Checked
        Me.CBCheckMobil.Checked = CBool(C_XML.Read("Optionen", "CBCheckMobil", "True"))

        'StoppUhr
        Me.CBStoppUhrEinblenden.Checked = CBool(C_XML.Read("Optionen", "CBStoppUhrEinblenden", "False"))
        Me.CBStoppUhrAusblenden.Checked = CBool(C_XML.Read("Optionen", "CBStoppUhrAusblenden", "False"))
        Me.TBStoppUhr.Text = C_XML.Read("Optionen", "TBStoppUhr", "10")

        Me.CBStoppUhrAusblenden.Enabled = Me.CBStoppUhrEinblenden.Checked
        If Not Me.CBStoppUhrEinblenden.Checked Then Me.CBStoppUhrAusblenden.Checked = False
        Me.TBStoppUhr.Enabled = Me.CBStoppUhrAusblenden.Checked And Me.CBStoppUhrEinblenden.Checked

        'Telefonnummernformat
        Me.TBTelNrMaske.Text = C_XML.Read("Optionen", "TBTelNrMaske", "%L (%O) %N - %D")
        Me.CBTelNrGruppieren.Checked = CBool(C_XML.Read("Optionen", "CBTelNrGruppieren", "True"))
        Me.CBintl.Checked = CBool(C_XML.Read("Optionen", "CBintl", "False"))
        Me.CBIgnoTelNrFormat.Checked = CBool(C_XML.Read("Optionen", "CBIgnoTelNrFormat", "False"))
#If OVer < 14 Then
        If Not Me.CBJournal.Checked Then Me.CBSymbJournalimport.Checked = False
        Me.CBSymbJournalimport.Enabled = Me.CBJournal.Checked
#End If

        'Phoner
        Dim PhonerVerfuegbar As Boolean = CBool(C_XML.Read("Phoner", "PhonerVerf�gbar", "False"))
        Dim TelName() As String
        Dim PhonerPasswort As String
        Me.PanelPhoner.Enabled = PhonerVerfuegbar
        If PhonerVerfuegbar Then
            Me.CBPhoner.Checked = CBool(IIf(C_XML.Read("Phoner", "CBPhoner", "False") = "True", True, False))
        Else
            Me.CBPhoner.Checked = False
        End If
        Me.LabelPhoner.Text = Replace(Me.LabelPhoner.Text, " [nicht]", CStr(IIf(PhonerVerfuegbar, "", " nicht")), , , CompareMethod.Text)
        'Me.CBPhonerKeineFB.Checked = CBool(IIf(C_XML.Read("Phoner", "CBPhonerKeineFB", "False") = "True", True, False))
        'If Not Me.CBPhonerKeineFB.Checked Then
        For i = 20 To 29
            TelName = Split(C_XML.Read("Telefone", CStr(i), "-1;"), ";", , CompareMethod.Text)
            If Not TelName(0) = "-1" And Not TelName.Length = 2 Then
                Me.ComboBoxPhonerSIP.Items.Add(TelName(2))
            End If
        Next
        If Not Me.ComboBoxPhonerSIP.Items.Count = 0 Then
            Me.ComboBoxPhonerSIP.SelectedIndex = CInt(C_XML.Read("Phoner", "ComboBoxPhonerSIP", "0"))
        End If
        'Else
        'Me.ComboBoxPhonerSIP.SelectedIndex = 0
        'Me.ComboBoxPhonerSIP.Enabled = False
        'End If
        Me.CBPhonerAnrMon.Checked = CBool(IIf(C_XML.Read("Phoner", "CBPhonerAnrMon", "False") = "True", True, False))
        PhonerPasswort = C_XML.Read("Phoner", "PhonerPasswort", "")
        If Not Len(PhonerPasswort) = 0 Then
            Me.PhonerPasswort.Text = "1234"
        End If

        Dim PhonerInstalliert As Boolean = C_Phoner.PhonerReady()
        Me.PanelPhonerAktiv.BackColor = CType(IIf(PhonerInstalliert, Color.LightGreen, Color.Red), Color)
        Me.LabelPhoner.Text = "Phoner ist " & CStr(IIf(PhonerInstalliert, "", "nicht ")) & "aktiv."
        Me.PanelPhoner.Enabled = PhonerInstalliert
        C_XML.Write("Phoner", "PhonerVerf�gbar", CStr(PhonerInstalliert), True)

        FillLogTB()
        FillTelListe()
        CLBTelNrAusf�llen()
    End Sub

    Private Sub FillTelListe()
        Dim Zeile As New ArrayList
        Dim Nebenstellen() As String
        Dim j As Integer
        Dim tmpein(3) As Double
        Dim xPathTeile As New ArrayList

        With xPathTeile
            .Add("Telefone")
            .Add("Telefone")
            .Add("*")
            .Add("Telefon")
            .Add("TelName")
        End With
        Nebenstellen = Split(C_XML.Read(xPathTeile, "-1;"), ";", , CompareMethod.Text)

        If Not Nebenstellen(0) = "-1" Then
            With Me.TelList
                .Rows.Clear()
                j = 0
                For Each Nebenstelle As String In Nebenstellen
                    j += 1
                    xPathTeile.Clear()

                    With xPathTeile
                        .Add("Telefone")
                        .Add("Telefone")
                        .Add("*")
                        .Add("Telefon")
                        .Add("[TelName = """ & Nebenstelle & """]")
                        .Add("@Standard")
                        Zeile.Add(CBool(C_XML.Read(xPathTeile, "False")))
                        Zeile.Add(CStr(j))
                        .Item(.Count - 1) = "@Dialport"
                        Zeile.Add(C_XML.Read(xPathTeile, "-1;")) 'Nebenstelle
                        .RemoveAt(.Count - 1)
                        Zeile.Add(C_XML.ReadElementName(xPathTeile, "-1;")) 'Telefontyp
                        Zeile.Add(Nebenstelle) ' TelName
                        .Add("TelNr")
                        Zeile.Add(Replace(C_XML.Read(xPathTeile, "-"), ";", ", ", , , CompareMethod.Text)) 'TelNr
                        .Item(.Count - 1) = "Eingehend"
                        Zeile.Add(C_XML.Read(xPathTeile, "0")) 'Eingehnd
                        tmpein(0) += CDbl(Zeile.Item(Zeile.Count - 1))
                        .Item(.Count - 1) = "Ausgehend"
                        Zeile.Add(C_XML.Read(xPathTeile, "0")) 'Ausgehnd
                        tmpein(1) += CDbl(Zeile.Item(Zeile.Count - 1))
                        Zeile.Add(CStr(CDbl(Zeile.Item(Zeile.Count - 2)) + CDbl(Zeile.Item(Zeile.Count - 1)))) 'Gesamt
                        tmpein(2) += CDbl(Zeile.Item(Zeile.Count - 1))
                        For i = Zeile.Count - 3 To Zeile.Count - 1
                            Zeile.Item(i) = C_Helfer.GetTimeInterval(CInt(Zeile.Item(i)))
                        Next
                    End With
                    .Rows.Add(Zeile.ToArray)
                    Zeile.Clear()
                Next
                Zeile.Add(False)
                Zeile.Add(vbNullString)
                Zeile.Add(vbNullString)
                Zeile.Add(vbNullString)
                Zeile.Add(vbNullString)
                Zeile.Add("Gesamt:")
                For i = 0 To 2
                    Zeile.Add(C_Helfer.GetTimeInterval(tmpein(i)))
                Next

                .Rows.Add(Zeile.ToArray)
            End With
        End If

        If C_XML.Read("Statistik", "ResetZeit", "-1") = "-1" Then C_XML.Write("Statistik", "ResetZeit", CStr(System.DateTime.Now), True)
        Me.TBAnderes.Text = C_XML.Read("Statistik", "Verpasst", "0") & " verpasste Telefonate" & vbCrLf
        Me.TBAnderes.Text = Me.TBAnderes.Text & C_XML.Read("Statistik", "Nichterfolgreich", "0") & " nicht erfolgreiche Telefonate" & vbCrLf
        Me.TBAnderes.Text = Me.TBAnderes.Text & C_XML.Read("Statistik", "Kontakt", "0") & " erstellte Kontakte" & vbCrLf
        Me.TBAnderes.Text = Me.TBAnderes.Text & C_XML.Read("Statistik", "Journal", "0") & " erstellte Journaleintr�ge" & vbCrLf
        Me.TBReset.Text = "Letzter Reset: " & C_XML.Read("Statistik", "ResetZeit", "Noch nicht festgelegt")
        Me.TBSchlie�Zeit.Text = "Letzter Journaleintrag: " & C_XML.Read("Journal", "Schlie�Zeit", "Noch nicht festgelegt")
        xPathTeile = Nothing
        Zeile = Nothing
    End Sub

    Sub CLBTelNrAusf�llen()
        Dim xPathTeile As New ArrayList
        Dim TelNrString() As String
        With xPathTeile
            .Add("Telefone")
            .Add("Nummern")
            .Add("*[starts-with(name(.), ""POTS"") or starts-with(name(.), ""MSN"") or starts-with(name(.), ""SIP"")]")

            TelNrString = Split("Alle Telefonnummern;" & C_XML.Read(xPathTeile, ""), ";", , CompareMethod.Text)

            TelNrString = (From x In TelNrString Select x Distinct).ToArray 'Doppelte entfernen
            TelNrString = (From x In TelNrString Where Not x Like "" Select x).ToArray ' Leere entfernen
            Me.CLBTelNr.Items.Clear()

            For Each TelNr In TelNrString
                Me.CLBTelNr.Items.Add(TelNr)
            Next
            'etwas unsch�n
            .Add("")
            For i = 1 To Me.CLBTelNr.Items.Count - 1
                .Item(.Count - 2) = "*[. = """ & Me.CLBTelNr.Items(i).ToString & """]"
                .Item(.Count - 1) = "@Checked"
                Me.CLBTelNr.SetItemChecked(i, C_Helfer.IsOneOf("1", Split(C_XML.Read(xPathTeile, "0;") & ";", ";", , CompareMethod.Text)))
            Next
        End With
        Me.CLBTelNr.SetItemChecked(0, Me.CLBTelNr.CheckedItems.Count = Me.CLBTelNr.Items.Count - 1)
    End Sub

#End Region

    Private Function Speichern() As Boolean
        Speichern = True
        Dim xPathTeile As New ArrayList
        Dim tmpTeile As String = vbNullString
        Dim CheckTelNr As CheckedListBox.CheckedItemCollection = Me.CLBTelNr.CheckedItems
        If CheckTelNr.Count = 0 Then
            For i = 0 To Me.CLBTelNr.Items.Count - 1
                Me.CLBTelNr.SetItemChecked(i, True)
            Next
            CheckTelNr = Me.CLBTelNr.CheckedItems
        End If
        If Me.CLBTelNr.Items.Count > 1 Then
            With xPathTeile
                .Add("Telefone")
                .Add("Nummern")
                .Add("*")
                For i = 1 To Me.CLBTelNr.Items.Count - 1
                    tmpTeile += ". = " & """" & Me.CLBTelNr.Items(i).ToString & """" & " or "
                Next
                tmpTeile = Strings.Left(tmpTeile, Len(tmpTeile) - Len(" or "))
                .Add("[" & tmpTeile & "]")
                C_XML.WriteAttribute(xPathTeile, "Checked", "0")
                tmpTeile = vbNullString
                For i = 0 To CheckTelNr.Count - 1
                    tmpTeile += ". = " & """" & CheckTelNr.Item(i).ToString & """" & " or "
                Next
                tmpTeile = Strings.Left(tmpTeile, Len(tmpTeile) - Len(" or "))
                .Item(.Count - 1) = "[" & tmpTeile & "]"
                C_XML.WriteAttribute(xPathTeile, "Checked", "1")
            End With
        End If

        ' Sichert die Einstellungen und schlie�t das Fenster
        If (CInt(Me.TBEnblDauer.Text) < 4) Then Me.TBEnblDauer.Text = "4"
        C_XML.Write("Optionen", "TBLandesVW", Me.TBLandesVW.Text, False)
        C_XML.Write("Optionen", "TBAmt", CStr(IIf(Me.TBAmt.Text = "", "-1", Me.TBAmt.Text)), False)
        C_XML.Write("Optionen", "TBFBAdr", Me.TBFBAdr.Text, False)
        ' So ist es sch�n:
        C_FBox.P_FBAddr = Me.TBFBAdr.Text
        ' So nicht:
        ThisAddIn.P_AnrMon.P_FBAddr = Me.TBFBAdr.Text
        C_XML.Write("Optionen", "CBForceFBAddr", CStr(Me.CBForceFBAddr.Checked), False)
        C_XML.Write("Optionen", "TBAnrMonX", Me.TBAnrMonX.Text, False)
        C_XML.Write("Optionen", "TBAnrMonY", Me.TBAnrMonY.Text, False)
        C_XML.Write("Optionen", "TBBenutzer", Me.TBBenutzer.Text, False)
        If Not Me.TBPasswort.Text = "1234" Then
            C_XML.Write("Optionen", "TBPasswort", C_Crypt.EncryptString128Bit(Me.TBPasswort.Text, "Fritz!Box Script"), False)
            SaveSetting("FritzBox", "Optionen", "Zugang", "Fritz!Box Script")
            C_Helfer.KeyChange()
        End If
        C_XML.Write("Optionen", "TBVorwahl", Me.TBVorwahl.Text, False)
        C_XML.Write("Optionen", "CBLogFile", CStr(Me.CBLogFile.Checked), False)
        ' Einstellungen f�r den Anrufmonitor speichern
        C_XML.Write("Optionen", "TBEnblDauer", CStr(Int(CDbl(Me.TBEnblDauer.Text))), False)
        C_XML.Write("Optionen", "CBAnrMonAuto", CStr(Me.CBAnrMonAuto.Checked), False)
        C_XML.Write("Optionen", "CBAutoClose", CStr(Me.CBAutoClose.Checked), False)
        C_XML.Write("Optionen", "CBAnrMonMove", CStr(Me.CBAnrMonMove.Checked), False)
        C_XML.Write("Optionen", "CBAnrMonTransp", CStr(Me.CBAnrMonTransp.Checked), False)
        C_XML.Write("Optionen", "CBAnrMonContactImage", CStr(Me.CBAnrMonContactImage.Checked), False)
        C_XML.Write("Optionen", "TBAnrMonMoveGeschwindigkeit", CStr((10 - Me.TBAnrMonMoveGeschwindigkeit.Value) * 10), False)
        C_XML.Write("Optionen", "CBIndexAus", CStr(Me.CBIndexAus.Checked), False)
        C_XML.Write("Optionen", "CBShowMSN", CStr(Me.CBShowMSN.Checked), False)
        ' optionale allgemeine Einstellungen speichern
        C_XML.Write("Optionen", "CBVoIPBuster", CStr(Me.CBVoIPBuster.Checked), False)
        C_XML.Write("Optionen", "CBDialPort", CStr(Me.CBDialPort.Checked), False)
        C_XML.Write("Optionen", "CBCbCunterbinden", CStr(Me.CBCbCunterbinden.Checked), False)
        C_XML.Write("Optionen", "CBCallByCall", CStr(Me.CBCallByCall.Checked), False)
        C_XML.Write("Optionen", "CBRueckwaertssuche", CStr(Me.CBRueckwaertssuche.Checked), False)
        C_XML.Write("Optionen", "CBKErstellen", CStr(Me.CBKErstellen.Checked), False)
        ' Einstellungen f�r die R�ckw�rtssuche speichern
        C_XML.Write("Optionen", "CBoxRWSuche", CStr(Me.ComboBoxRWS.SelectedIndex), False)
        C_XML.Write("Optionen", "CBKHO", CStr(Me.CBKHO.Checked), False)
        C_XML.Write("Optionen", "CBRWSIndex", CStr(Me.CBRWSIndex.Checked), False)
        ' Einstellungen f�r das Journal speichern
        C_XML.Write("Optionen", "CBJournal", CStr(Me.CBJournal.Checked), False)
        C_XML.Write("Optionen", "CBJImport", CStr(Me.CBJImport.Checked), False)
        ' NEU
        C_XML.Write("Optionen", "CBUseAnrMon", CStr(Me.CBUseAnrMon.Checked), False)
        C_XML.Write("Optionen", "CBCheckMobil", CStr(Me.CBCheckMobil.Checked), False)
        ' StoppUhr
        C_XML.Write("Optionen", "CBStoppUhrEinblenden", CStr(Me.CBStoppUhrEinblenden.Checked), False)
        C_XML.Write("Optionen", "CBStoppUhrAusblenden", CStr(Me.CBStoppUhrAusblenden.Checked), False)
        If Not Me.TBStoppUhr.Text = vbNullString Then
            If CInt(Me.TBStoppUhr.Text) < 0 Then
                Me.TBStoppUhr.Text = "10"
            End If
        Else
            Me.TBStoppUhr.Text = "10"
        End If
#If OVer < 14 Then
        C_XML.Write("Optionen", "CBSymbWwdh", CStr(Me.CBSymbWwdh.Checked), False)
        C_XML.Write("Optionen", "CBSymbAnrMonNeuStart", CStr(Me.CBSymbAnrMonNeuStart.Checked), False)
        C_XML.Write("Optionen", "CBSymbAnrMon", CStr(Me.CBSymbAnrMon.Checked), False)
        C_XML.Write("Optionen", "CBSymbAnrListe", CStr(Me.CBSymbAnrListe.Checked), False)
        C_XML.Write("Optionen", "CBSymbDirekt", CStr(Me.CBSymbDirekt.Checked), False)
        C_XML.Write("Optionen", "CBSymbRWSuche", CStr(Me.CBSymbRWSuche.Checked), False)
        C_XML.Write("Optionen", "CBSymbJournalimport", CStr(Me.CBSymbJournalimport.Checked), False)
        C_XML.Write("Optionen", "CBSymbVIP", CStr(Me.CBSymbVIP.Checked), False)
#End If

        C_XML.Write("Optionen", "TBStoppUhr", Me.TBStoppUhr.Text, False)
        'Telefonnummernformat
        If Pr�feMaske() Then
            C_XML.Write("Optionen", "TBTelNrMaske", Me.TBTelNrMaske.Text, False)
        End If
        C_XML.Write("Optionen", "CBTelNrGruppieren", CStr(Me.CBTelNrGruppieren.Checked), False)
        C_XML.Write("Optionen", "CBintl", CStr(Me.CBintl.Checked), False)
        C_XML.Write("Optionen", "CBIgnoTelNrFormat", CStr(Me.CBIgnoTelNrFormat.Checked), False)
        ' Telefone
#If OVer < 14 Then
        GUI.SetVisibleButtons()
#End If
        With xPathTeile
            .Clear()
            .Add("Telefone")
            .Add("Telefone")
            .Add("*")
            .Add("Telefon")
            .Add(vbNullString)
            For i = 0 To TelList.Rows.Count - 2
                .Item(.Count - 1) = "[@Dialport = """ & TelList.Rows(i).Cells(2).Value.ToString & """]"
                C_XML.WriteAttribute(xPathTeile, "Standard", CStr(CBool(TelList.Rows(i).Cells(0).Value)))
            Next
        End With
        ' Phoner
        Dim TelName() As String
        Dim PhonerTelNameIndex As String = "0"

        C_XML.Write("Phoner", "CBPhoner", CStr(Me.CBPhoner.Checked), False)

        For i = 20 To 29
            TelName = Split(C_XML.Read("Telefone", CStr(i), "-1;;"), ";", , CompareMethod.Text)
            If Not TelName(0) = "-1" And Not ComboBoxPhonerSIP.SelectedItem Is Nothing And Not TelName.Length = 2 Then
                If TelName(2) = ComboBoxPhonerSIP.SelectedItem.ToString Then
                    PhonerTelNameIndex = CStr(i)
                    Exit For
                End If
            End If
        Next
        C_XML.Write("Phoner", "PhonerTelNameIndex", PhonerTelNameIndex, False)
        C_XML.Write("Phoner", "ComboBoxPhonerSIP", CStr(Me.ComboBoxPhonerSIP.SelectedIndex), False)
        C_XML.Write("Phoner", "CBPhonerAnrMon", CStr(Me.CBPhonerAnrMon.Checked), False)
        'C_XML.Write("Phoner", "CBPhonerKeineFB", CStr(Me.CBPhonerKeineFB.Checked), True)
        'ThisAddIn.NutzePhonerOhneFritzBox = Me.CBPhonerKeineFB.Checked
        If Me.PhonerPasswort.Text = "" And Me.CBPhoner.Checked Then
            If C_Helfer.FBDB_MsgBox("Es wurde kein Passwort f�r Phoner eingegeben! Da W�hlen �ber Phoner wird nicht funktionieren!", MsgBoxStyle.OkCancel, "Speichern") = MsgBoxResult.Cancel Then
                Speichern = False
            End If
        End If
        If Me.CBPhoner.Checked Then
            If Not Me.PhonerPasswort.Text = "" Then
                If Not Me.PhonerPasswort.Text = "1234" Then
                    C_XML.Write("Phoner", "PhonerPasswort", C_Crypt.EncryptString128Bit(Me.PhonerPasswort.Text, "Fritz!Box Script"), False)
                    SaveSetting("FritzBox", "Optionen", "ZugangPasswortPhoner", "Fritz!Box Script")
                    C_Helfer.KeyChange()
                End If
            End If
        End If
        C_XML.SpeichereXMLDatei()
    End Function

#Region "Button Link"
    Private Sub Button_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ButtonZuruecksetzen.Click, _
                                                                                   ButtonOK.Click, _
                                                                                   ButtonAbbruch.Click, _
                                                                                   ButtonUebernehmen.Click, _
                                                                                   ButtonXML.Click, _
                                                                                   BAnrMonTest.Click, _
                                                                                   BIndizierungStart.Click, _
                                                                                   BIndizierungAbbrechen.Click, _
                                                                                   ButtonIndexDatei�ffnen.Click, _
                                                                                   BZwischenablage.Click, _
                                                                                   BTelefonliste.Click, _
                                                                                   BTelefonDatei.Click, _
                                                                                   BStartDebug.Click, _
                                                                                   BResetStat.Click, _
                                                                                   BProbleme.Click
        Select Case CType(sender, Windows.Forms.Button).Name
            Case "ButtonZuruecksetzen"
                ' Startwerte zur�cksetzen
                ' Einstellungen f�r das W�hlmakro zur�cksetzen
                Me.TBLandesVW.Text = "0049"
                Me.TBAmt.Text = ""
                Me.CBCheckMobil.Checked = True

                ' Einstellungen f�r den Anrufmonitor zur�cksetzen
                Me.TBEnblDauer.Text = "10"
                Me.TBAnrMonX.Text = "0"
                Me.TBAnrMonY.Text = "0"
                Me.CBAnrMonAuto.Checked = False
                Me.CBAutoClose.Checked = True
                Me.CBAnrMonMove.Checked = True
                Me.CBAnrMonTransp.Checked = True
                Me.CBAnrMonContactImage.Checked = True
                Me.CBShowMSN.Checked = False
                Me.TBAnrMonMoveGeschwindigkeit.Value = 5
                Me.CBIndexAus.Checked = False
                Me.CBIndexAus.Enabled = False
                ' optionale allgemeine Einstellungen zuruecksetzen
                Me.CBVoIPBuster.Checked = False
                Me.CBDialPort.Checked = False
                Me.CBCallByCall.Checked = False
                Me.CBCbCunterbinden.Checked = False
                Me.CBRueckwaertssuche.Checked = False
                Me.CBKErstellen.Checked = False
                Me.CBLogFile.Checked = False
                Me.CBForceFBAddr.Checked = False
#If OVer < 14 Then
                ' Einstellungen f�r die Symbolleiste zur�cksetzen
                Me.CBSymbAnrMonNeuStart.Checked = False
                Me.CBSymbWwdh.Checked = True
                Me.CBSymbAnrMon.Checked = True
                Me.CBSymbAnrListe.Checked = True
                Me.CBSymbDirekt.Checked = True
                Me.CBSymbRWSuche.Checked = False
                Me.CBSymbJournalimport.Checked = False
#End If
                ' Einstellungen f�r die R�ckw�rtssuche zur�cksetzen
                Me.ComboBoxRWS.Enabled = False
                Me.ComboBoxRWS.SelectedIndex = 0
                Me.CBRWSIndex.Checked = True
                ' Einstellungen f�r das Journal zur�cksetzen
                Me.CBKHO.Checked = True
                Me.CBJournal.Checked = False
                Me.CBJImport.Checked = False
                Me.CBLogFile.Checked = True

                'StoppUhr
                Me.CBStoppUhrEinblenden.Checked = False
                Me.CBStoppUhrAusblenden.Checked = False
                Me.TBStoppUhr.Text = "10"

                'Telefonnummernformat
                Me.TBTelNrMaske.Text = "%L (%O) %N - %D"
                Me.CBTelNrGruppieren.Checked = True
                Me.CBintl.Checked = False
                Me.CBIgnoTelNrFormat.Checked = False
            Case "BTelefonliste"
                Dim xPathTeile As New ArrayList
                C_FBox.SetEventProvider(emc)
                Me.BTelefonliste.Enabled = False
                Me.BTelefonliste.Text = "Bitte warten..."
                Windows.Forms.Application.DoEvents()
                Speichern()

                BWTelefone = New BackgroundWorker
                With BWTelefone
                    .WorkerReportsProgress = False
                    .RunWorkerAsync(True)
                End With
            Case "ButtonOK"
                Dim formschlie�en As Boolean = Speichern()
                ThisAddIn.P_UseAnrMon = Me.CBUseAnrMon.Checked
#If OVer >= 14 Then
                GUI.RefreshRibbon()
#End If
                If formschlie�en Then
                    Dispose(True)
                End If
            Case "ButtonAbbruch"
                ' Schlie�t das Fenster
                Dispose(True)
            Case "ButtonUebernehmen"
                Speichern()
            Case "ButtonXML"
                System.Diagnostics.Process.Start(C_XML.GetXMLDateiPfad)
            Case "BAnrMonTest"
                Speichern()
                Dim ID As Integer = CInt(C_XML.Read("letzterAnrufer", "Letzter", CStr(0)))
                Dim forman As New formAnrMon(ID, False, C_XML, C_Helfer, AnrMon, OlI)
            Case "BZwischenablage"
                My.Computer.Clipboard.SetText(Me.TBDiagnose.Text)
            Case "BProbleme"
                Dim T As New Thread(AddressOf NewMail)
                T.Start()
                If C_Helfer.FBDB_MsgBox("Der Einstellungsdialog wird jetzt geschlossen. Danach werden alle erforderlichen Informationen gesammelt, was ein paar Sekunden dauern kann." & vbNewLine & _
                                                "Danach wird eine neue E-Mail ge�ffnet, die Sie bitte vervollst�ndigen und absenden.", MsgBoxStyle.Information, "") = MsgBoxResult.Ok Then
                    Me.Close()
                End If
            Case "BStartDebug"
                Me.TBDiagnose.Text = vbNullString
                AddLine("Start")
                If Me.CBTelefonDatei.Checked Then
                    If System.IO.File.Exists(Me.TBTelefonDatei.Text) Then
                        If C_Helfer.FBDB_MsgBox("Sind Sie sicher was sie da tun? Das Testen einer fehlerhaften oder falschen Datei kann sehr unerfreulich enden.", _
                                                        MsgBoxStyle.YesNo, "Telefondatei testen") = vbYes Then
                            Me.TBTelefonDatei.Enabled = False
                        End If
                    Else
                        Me.CBTelefonDatei.Checked = False
                    End If
                End If
                C_FBox.SetEventProvider(emc)
                AddLine("Fritz!Box Klasse mit Verweis auf dieses Formular erstellt.")

                BWTelefone = New BackgroundWorker
                AddLine("BackgroundWorker erstellt.")
                With BWTelefone
                    .WorkerReportsProgress = True
                    .RunWorkerAsync(True)
                    AddLine("BackgroundWorker gestartet.")
                End With
                Me.TBTelefonDatei.Enabled = True
            Case "BTelefonDatei"
                Dim fDialg As New System.Windows.Forms.OpenFileDialog
                With fDialg
                    .Filter = "htm-Dateien (*.htm)| *.htm"
                    .Multiselect = False
                    .Title = "Fritz!Box Telefon-Datei ausw�hlen"
                    .FilterIndex = 1
                    .RestoreDirectory = True
                    If .ShowDialog = Windows.Forms.DialogResult.OK Then
                        If System.IO.File.Exists(fDialg.FileName) Then
                            Me.TBTelefonDatei.Text = fDialg.FileName
                        Else
                            Me.TBTelefonDatei.Text = "Fehler!"
                        End If
                    End If
                End With
                fDialg = Nothing
            Case "BResetStat"
                Dim xPathTeile As New ArrayList
                C_XML.Delete("Statistik")
                With xPathTeile
                    .Add("Statistik")
                    .Add("ResetZeit")
                    C_XML.Write(xPathTeile, CStr(System.DateTime.Now), False)
                    .Clear()
                    .Add("Telefone")
                    .Add("Telefone")
                    .Add("*")
                    .Add("Telefon")
                    .Add("Eingehend")
                    C_XML.Write(xPathTeile, "0", False)
                    .Item(.Count - 1) = "Ausgehend"
                    C_XML.Write(xPathTeile, "0", False)
                End With
                FillTelListe()
                xPathTeile = Nothing
            Case "BIndizierungStart"
                StarteIndizierung()
            Case "BIndizierungAbbrechen"
                BWIndexer.CancelAsync()
                Me.BIndizierungAbbrechen.Enabled = False
                Me.BIndizierungStart.Enabled = True
        End Select
    End Sub

    Private Sub Link_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkHomepage.LinkClicked, _
                                                                                                                                LinkForum.LinkClicked, _
                                                                                                                                LinkEmail.LinkClicked, _
                                                                                                                                LinkLogFile.LinkClicked
        Select Case CType(sender, Windows.Forms.LinkLabel).Name
            Case "LinkEmail"
                Me.Close()
                System.Diagnostics.Process.Start("mailto:kruemelino@gert-michael.de")
            Case "LinkForum"
                System.Diagnostics.Process.Start("http://www.ip-phone-forum.de/showthread.php?t=237086")
            Case "LinkHomepage"
                System.Diagnostics.Process.Start("http://github.com/Kruemelino/FritzBoxTelefon-dingsbums")
            Case "LinkLogFile"
                System.Diagnostics.Process.Start(C_Helfer.Dateipfade("LogDatei"))
        End Select
    End Sub

#End Region

#Region "�nderungen"
    Private Sub ValueChanged(sender As Object, e As EventArgs) Handles _
                                                                        CBRueckwaertssuche.CheckedChanged, _
                                                                        CBCbCunterbinden.CheckedChanged, _
                                                                        CBAutoClose.CheckedChanged, _
 _
                                                                        CBJournal.CheckedChanged, _
                                                                        CBIndexAus.CheckedChanged, _
                                                                        CBUseAnrMon.CheckedChanged, _
                                                                        CBStoppUhrEinblenden.CheckedChanged, _
                                                                        CBStoppUhrAusblenden.CheckedChanged, _
                                                                        CBLogFile.CheckedChanged, _
                                                                        TBLandesVW.Leave, _
                                                                        TBVorwahl.TextChanged, _
                                                                        TBEnblDauer.TextChanged, _
                                                                        TBAnrMonX.TextChanged, _
                                                                        TBAnrMonY.TextChanged, _
                                                                        TBLandesVW.TextChanged, _
                                                                        TBTelNrMaske.Leave, _
                                                                        CLBTelNr.SelectedIndexChanged
        Select Case sender.GetType().Name
            Case "CheckBox"
                Select Case CType(sender, CheckBox).Name
                    Case "CBTelefonDatei"
                        Me.PTelefonDatei.Enabled = Me.CBTelefonDatei.Checked
                        If Not Me.CBTelefonDatei.Checked Then
                            Me.TBTelefonDatei.Text = vbNullString
                        End If
                    Case "CBRueckwaertssuche"
                        ' Combobox f�r R�ckw�rtssuchmaschinen je nach CheckBox f�r R�ckw�rtssuche ein- bzw. ausblenden
                        Me.ComboBoxRWS.Enabled = Me.CBRueckwaertssuche.Checked
                        Me.CBKErstellen.Checked = Me.CBRueckwaertssuche.Checked
                        Me.CBKErstellen.Enabled = Me.CBRueckwaertssuche.Checked
                        Me.CBRWSIndex.Enabled = Me.CBRueckwaertssuche.Checked
                        Me.CBRWSIndex.Checked = Me.CBRueckwaertssuche.Checked
                    Case "CBCbCunterbinden"
                        Me.CBCallByCall.Enabled = Not Me.CBCbCunterbinden.Checked
                        If Me.CBCbCunterbinden.Checked Then Me.CBCallByCall.Checked = False
                    Case "CBAutoClose"
                        Me.TBEnblDauer.Enabled = Me.CBAutoClose.Checked
                        Me.LEnblDauer.Enabled = Me.CBAutoClose.Checked
                    Case "CBJournal"
                        If Not Me.CBJournal.Checked Then Me.CBJImport.Checked = False
                        Me.CBJImport.Enabled = Me.CBJournal.Checked
#If OVer < 14 Then
                If Not Me.CBJournal.Checked Then Me.CBSymbJournalimport.Checked = False
                Me.CBSymbJournalimport.Enabled = Me.CBJournal.Checked
#End If
                    Case "CBIndexAus"
                        Me.BIndizierungStart.Enabled = Not Me.CBIndexAus.Checked
                    Case "CBUseAnrMon"
                        Me.PanelAnrMon.Enabled = Me.CBUseAnrMon.Checked
                        Me.CBIndexAus.Enabled = Not Me.CBUseAnrMon.Checked
                        Me.GroupBoxStoppUhr.Enabled = Me.CBUseAnrMon.Checked

                        If Not Me.CBUseAnrMon.Checked Then
                            Me.CBStoppUhrEinblenden.Checked = False
                            Me.CBStoppUhrAusblenden.Checked = False
                        End If
                    Case "CBStoppUhrEinblenden"
                        Me.CBStoppUhrAusblenden.Enabled = Me.CBStoppUhrEinblenden.Checked
                        If Not Me.CBStoppUhrEinblenden.Checked Then Me.CBStoppUhrAusblenden.Checked = False
                        Me.TBStoppUhr.Enabled = Me.CBStoppUhrAusblenden.Checked And Me.CBStoppUhrEinblenden.Checked
                        Me.LabelStoppUhr.Enabled = Me.CBStoppUhrEinblenden.Checked
                    Case "CBStoppUhrAusblenden"
                        Me.TBStoppUhr.Enabled = Me.CBStoppUhrAusblenden.Checked And Me.CBStoppUhrEinblenden.Checked
                    Case "CBLogFile"
                        Me.GBLogging.Enabled = Me.CBLogFile.Checked
                End Select
            Case "TextBox"
                Select Case CType(sender, TextBox).Name
                    Case "TBLandesVW"
                        If Me.TBLandesVW.Text = "0049" Then
                            Me.CBRueckwaertssuche.Enabled = True

                            Me.CBKErstellen.Enabled = True
                            Me.ComboBoxRWS.Enabled = Me.CBRueckwaertssuche.Checked
                        Else
                            Me.CBRueckwaertssuche.Checked = False
                            Me.CBRueckwaertssuche.Enabled = False

                            Me.CBKErstellen.Enabled = False
                            Me.CBKErstellen.Checked = False
                            Me.ComboBoxRWS.Enabled = False
                        End If
                    Case "TBVorwahl"
                        C_Helfer.AcceptOnlyNumeric(Me.TBVorwahl.Text)
                    Case "TBEnblDauer"
                        C_Helfer.AcceptOnlyNumeric(Me.TBEnblDauer.Text)
                    Case "TBAnrMonX"
                        C_Helfer.AcceptOnlyNumeric(Me.TBAnrMonX.Text)
                    Case "TBAnrMonY"
                        C_Helfer.AcceptOnlyNumeric(Me.TBAnrMonY.Text)
                    Case "TBLandesVW"
                        Me.ToolTipFBDBConfig.SetToolTip(Me.CBVoIPBuster, "Mit dieser Einstellung wird die Landesvorwahl " & Me.TBLandesVW.Text & " immer mitgew�hlt.")
                    Case "TBTelNrMaske"
                        Pr�feMaske()
                End Select
            Case "CheckedListBox"
                Select Case CType(sender, CheckedListBox).Name
                    Case "CLBTelNr"
                        Dim alle As Boolean = True
                        With Me.CLBTelNr
                            Select Case .SelectedIndex
                                Case 0
                                    For i = 1 To .Items.Count - 1
                                        .SetItemChecked(i, .GetItemChecked(0))
                                    Next
                                Case 1 To .Items.Count - 1
                                    For i = 1 To .Items.Count - 1
                                        If .GetItemChecked(i) = False Then
                                            alle = False
                                            Exit For
                                        End If
                                    Next
                                    .SetItemChecked(0, alle)
                            End Select
                        End With
                End Select
        End Select
    End Sub

    Private Sub TelList_CellMouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs)
        ' Sichersellen, dass nur ein Haken gesetzt ist.
        If TypeOf Me.TelList.CurrentCell Is Windows.Forms.DataGridViewCheckBoxCell Then
            Me.TelList.EndEdit()
            If Not Me.TelList.CurrentCell.Value Is Nothing Then
                Dim cellVal As Boolean = DirectCast(Me.TelList.CurrentCell.Value, Boolean)
                If cellVal Then
                    If Not Me.TelList.CurrentCell Is Me.TelList.Rows(Me.TelList.Rows.Count - 1).Cells(0) Then
                        For i = 0 To TelList.Rows.Count - 1
                            Me.TelList.Rows(i).Cells(0).Value = False
                        Next
                        If Not (Me.TelList.Rows(Me.TelList.CurrentCell.RowIndex).Cells(3).Value.ToString = "TAM" Or _
                             Me.TelList.Rows(Me.TelList.CurrentCell.RowIndex).Cells(3).Value.ToString = "FAX") Then Me.TelList.CurrentCell.Value = cellVal
                    Else
                        Me.TelList.CurrentCell.Value = False
                    End If
                End If
            End If
        End If
    End Sub

#End Region

#Region "Helfer"
    Function Pr�feMaske() As Boolean
        ' "%L (%O) %N - %D"
        Dim pos(2) As String
        pos(0) = CStr(InStr(Me.TBTelNrMaske.Text, "%L", CompareMethod.Text))
        pos(1) = CStr(InStr(Me.TBTelNrMaske.Text, "%O", CompareMethod.Text))
        pos(2) = CStr(InStr(Me.TBTelNrMaske.Text, "%N", CompareMethod.Text))
        If C_Helfer.IsOneOf("0", pos) Then
            C_Helfer.FBDB_MsgBox("Achtung: Die Maske f�r die Telefonnummernformatierung ist nicht korrekt." & vbNewLine & _
                        "Pr�fen Sie, ob folgende Zeichen in der Maske Enthalten sind: ""%L"", ""%V"" und ""%N"" (""%D"" kann wegelassen werden)!" & vbNewLine & _
                        "Beispiel: ""%L (%O) %N - %D""", MsgBoxStyle.Information, "Einstellungen")
            Return False
        End If
        Return True
    End Function

    Private Sub NewMail()
        Dim NeueFW As Boolean
        Dim SID As String = C_FBox.P_DefaultSID
        Dim URL As String
        Dim FBOX_ADR As String = C_XML.Read("Optionen", "TBFBAdr", ThisAddIn.P_FritzBox.P_DefaultFBAddr)

        Dim FBEncoding As System.Text.Encoding = System.Text.Encoding.UTF8
        Dim MailText As String
        Dim PfadTMPfile As String
        Dim tmpFileName As String
        Dim tmpFilePath As String
        Dim FBBenutzer As String
        Dim FBPasswort As String

        'C_FBox = Nothing
        'C_FBox = New FritzBox(C_XML, C_Helfer, C_Crypt)
        C_FBox.SetEventProvider(emc)
        Do While SID = C_FBox.P_DefaultSID
            FBBenutzer = InputBox("Geben Sie den Benutzernamen der Fritz!Box ein (Lassen Sie das Feld leer, falls Sie kein Benutzername ben�tigen.):")
            FBPasswort = InputBox("Geben Sie das Passwort der Fritz!Box ein:")
            If Len(FBPasswort) = 0 Then
                If C_Helfer.FBDB_MsgBox("Haben Sie das Passwort vergessen?", MsgBoxStyle.YesNo, "NewMail") = vbYes Then
                    Exit Sub
                End If
            End If
            SID = C_FBox.FBLogIn(NeueFW, FBBenutzer, FBPasswort)
        Loop

        If NeueFW Then
            URL = "http://" & FBOX_ADR & "/fon_num/fon_num_list.lua?sid=" & SID
        Else
            URL = "http://" & FBOX_ADR & "/cgi-bin/webcm?sid=" & SID & "&getpage=&var:lang=de&var:menu=fon&var:pagename=fondevices"
        End If
        MailText = C_Helfer.httpRead(URL, FBEncoding, Nothing)

        With My.Computer.FileSystem
            PfadTMPfile = .GetTempFileName()
            tmpFilePath = .GetFileInfo(PfadTMPfile).DirectoryName
            tmpFileName = Split(.GetFileInfo(PfadTMPfile).Name, ".", , CompareMethod.Text)(0) & "_Telefonieger�te.htm"
            .RenameFile(PfadTMPfile, tmpFileName)
            PfadTMPfile = .GetFiles(tmpFilePath, FileIO.SearchOption.SearchTopLevelOnly, "*_Telefonieger�te.htm")(0).ToString
            .WriteAllText(PfadTMPfile, MailText, False)
        End With
        OlI.NeuEmail(PfadTMPfile, C_XML.GetXMLDateiPfad, C_Helfer.GetInformationSystemFritzBox(FBOX_ADR))
    End Sub

    Public Function SetTelNrListe() As Boolean
        SetTelNrListe = False
        If Me.InvokeRequired Then
            Dim D As New DelgSetLine(AddressOf CLBTelNrAusf�llen)
            Invoke(D)
        Else
            CLBTelNrAusf�llen()
        End If
    End Function

    Private Sub TextChangedHandler(ByVal sender As Object, ByVal e As EventArgs) Handles emc.GenericEvent
        StatusWert = DirectCast(sender, Control).Text
        AddLine(StatusWert)
    End Sub

    Public Function AddLine(ByVal Zeile As String) As Boolean
        AddLine = False
        StatusWert = Zeile
        If Me.InvokeRequired Then
            Dim D As New DelgSetLine(AddressOf setline)
            Invoke(D)
        Else
            setline()
        End If
    End Function

    Private Sub setline()
        Me.LTelStatus.Text = "Status: " & StatusWert
        With Me.TBDiagnose
            .Text += StatusWert & vbCrLf
            .SelectionStart = .Text.Length
            .ScrollToCaret()
        End With
    End Sub

#End Region

#Region "Kontaktindizierung"

    Sub StarteIndizierung()
        Startzeit = Date.Now
        BWIndexer = New BackgroundWorker
        Me.ProgressBarIndex.Value = 0
        Me.LabelAnzahl.Text = "Status: 0/" & CStr(Me.ProgressBarIndex.Maximum)
        Me.BIndizierungAbbrechen.Enabled = True
        Me.BIndizierungStart.Enabled = False
        Me.LabelAnzahl.Text = "Status: Bitte Warten!"
        With BWIndexer
            .WorkerSupportsCancellation = True
            .WorkerReportsProgress = True
            .RunWorkerAsync()
        End With

    End Sub

#Region "Vorbereitung"
    Private Function ErmittleKontaktanzahl() As Boolean
        ErmittleKontaktanzahl = True
        Dim olNamespace As Outlook.NameSpace ' MAPI-Namespace
        Dim olfolder As Outlook.MAPIFolder
        Dim LandesVW As String = Me.TBLandesVW.Text
        Anzahl = 0
        olNamespace = OlI.GetOutlook.GetNamespace("MAPI")

        If Me.CBKHO.Checked Then
            olfolder = olNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderContacts)
            Z�hleKontakte(olfolder, Nothing)
        Else
            Z�hleKontakte(Nothing, olNamespace)
        End If
        If Me.InvokeRequired Then
            Dim D As New DelgSetProgressbar(AddressOf SetProgressbarMax)
            Invoke(D)
        Else
            SetProgressbarMax()
        End If
    End Function

    Private Function Z�hleKontakte(ByVal Ordner As Outlook.MAPIFolder, ByVal NamensRaum As Outlook.NameSpace) As Integer

        Z�hleKontakte = 0
        Dim iOrdner As Long    ' Z�hlvariable f�r den aktuellen Ordner

        Dim aktKontakt As Outlook.ContactItem  ' aktueller Kontakt
        Dim alleTE(13) As String  ' alle TelNr/Email eines Kontakts
        ' Wenn statt einem Ordner der NameSpace �bergeben wurde braucht man zuerst mal die oberste Ordnerliste.
        If Not NamensRaum Is Nothing Then
            Dim j As Integer = 1
            Do While (j <= NamensRaum.Folders.Count)
                Z�hleKontakte(NamensRaum.Folders.Item(j), Nothing)
                j = j + 1
            Loop
            aktKontakt = Nothing
            Return 0
        End If

        If Ordner.DefaultItemType = Outlook.OlItemType.olContactItem Then
            'Debug.Print(Ordner.Name, Ordner.Items.Count)
            Anzahl += Ordner.Items.Count
        End If

        ' Unterordner werden rekursiv durchsucht
        iOrdner = 1
        Do While (iOrdner <= Ordner.Folders.Count)
            Z�hleKontakte(Ordner.Folders.Item(iOrdner), Nothing)
            iOrdner = iOrdner + 1
        Loop

        aktKontakt = Nothing
    End Function
#End Region

    Private Sub KontaktIndexer(ByVal LandesVW As String, Optional ByVal Ordner As Outlook.MAPIFolder = Nothing, Optional ByVal NamensRaum As Outlook.NameSpace = Nothing) 'as Boolean
        'KontaktIndexer = False
        Dim iOrdner As Long    ' Z�hlvariable f�r den aktuellen Ordner

        'Dim item As Object      ' aktuelles Element
        Dim aktKontakt As Outlook.ContactItem  ' aktueller Kontakt
        ' Wenn statt einem Ordner der NameSpace �bergeben wurde braucht man zuerst mal die oberste Ordnerliste.
        If Not NamensRaum Is Nothing Then
            Dim j As Integer = 1
            Do While (j <= NamensRaum.Folders.Count)
                KontaktIndexer(LandesVW, NamensRaum.Folders.Item(j))
                j = j + 1
            Loop
            aktKontakt = Nothing
            'Return True
        Else
            If Ordner.DefaultItemType = Outlook.OlItemType.olContactItem And Not BWIndexer.CancellationPending Then
                'C_Kontakte.IndiziereOrdner(Ordner)
                For Each item In Ordner.Items
                    ' nur Kontakte werden durchsucht
                    If TypeOf item Is Outlook.ContactItem Then
                        aktKontakt = CType(item, Outlook.ContactItem)

                        'With aktKontakt
                        'KontaktName = " (" & aktKontakt.FullNameAndCompany & ")"
                        KontaktName = " (" & aktKontakt.FullName & ")"
                        C_Kontakte.IndiziereKontakt(aktKontakt, False)
                        BWIndexer.ReportProgress(1)
                        If BWIndexer.CancellationPending Then Exit For
                    Else
                        BWIndexer.ReportProgress(1)
                    End If
                    C_Helfer.NAR(item)
                    Windows.Forms.Application.DoEvents()
                Next 'Item
                'Elemente = Nothing
            End If

            ' Unterordner werden rekursiv durchsucht
            iOrdner = 1
            Do While (iOrdner <= Ordner.Folders.Count) And Not BWIndexer.CancellationPending
                KontaktIndexer(LandesVW, Ordner.Folders.Item(iOrdner))
                iOrdner = iOrdner + 1
            Loop
            aktKontakt = Nothing
        End If
    End Sub

    Private Sub KontaktDeIndexer(ByVal Ordner As Outlook.MAPIFolder, ByVal NamensRaum As Outlook.NameSpace) 'As Boolean

        'KontaktDeIndexer = False
        Dim iOrdner As Long    ' Z�hlvariable f�r den aktuellen Ordner

        'Dim item As Object      ' aktuelles Element
        Dim aktKontakt As Outlook.ContactItem  ' aktueller Kontakt
        ' Wenn statt einem Ordner der NameSpace �bergeben wurde braucht man zuerst mal die oberste Ordnerliste.
        If Not NamensRaum Is Nothing Then
            Dim j As Integer = 1
            Do While (j <= NamensRaum.Folders.Count)
                KontaktDeIndexer(NamensRaum.Folders.Item(j), Nothing)
                j = j + 1
            Loop
            aktKontakt = Nothing
            'Return True
        Else

            'If BWIndexer.CancellationPending Then Exit Function

            If Ordner.DefaultItemType = Outlook.OlItemType.olContactItem And Not BWIndexer.CancellationPending Then
                For Each item In Ordner.Items
                    ' nur Kontakte werden durchsucht
                    If TypeOf item Is Outlook.ContactItem Then
                        aktKontakt = CType(item, Outlook.ContactItem)

                        'With aktKontakt
                        'KontaktName = " (" & aktKontakt.FullNameAndCompany & ")"
                        KontaktName = " (" & aktKontakt.FullName & ")"
                        C_Kontakte.DeIndizierungKontakt(aktKontakt, False)
                        BWIndexer.ReportProgress(-1)
                        If BWIndexer.CancellationPending Then Exit For
                    Else
                        BWIndexer.ReportProgress(-1)
                    End If
                    C_Helfer.NAR(item)
                    Windows.Forms.Application.DoEvents()
                Next 'Item
                C_Kontakte.DeIndizierungOrdner(Ordner)
            End If
            ' Unterordner werden rekursiv durchsucht
            iOrdner = 1
            Do While (iOrdner <= Ordner.Folders.Count) And Not BWIndexer.CancellationPending
                KontaktDeIndexer(Ordner.Folders.Item(iOrdner), Nothing)
                iOrdner = iOrdner + 1
            Loop
            aktKontakt = Nothing
        End If
    End Sub
#End Region

#Region "Logging"
    Sub FillLogTB()
        Dim LogDatei As String = C_Helfer.Dateipfade("LogDatei")

        If C_XML.Read("Optionen", "CBLogFile", "False") = "True" Then
            If My.Computer.FileSystem.FileExists(LogDatei) Then
                Me.TBLogging.Text = My.Computer.FileSystem.OpenTextFileReader(LogDatei).ReadToEnd
            End If
        End If
        Me.LinkLogFile.Text = LogDatei
    End Sub

    Private Sub FBDB_MP_TabIndexChanged(sender As Object, e As EventArgs) Handles FBDB_MP.SelectedIndexChanged
        Me.Update()
        If Me.FBDB_MP.SelectedTab.Name = "PLogging" Then
            With Me.TBLogging
                .Focus()
                .SelectionStart = .TextLength
                .ScrollToCaret()
            End With
        End If
    End Sub

    Private Sub BLogging_Click(sender As Object, e As EventArgs) Handles BLogging.Click
        With Me.TBLogging
            If .SelectedText = vbNullString Then
                My.Computer.Clipboard.SetText(.Text)
            Else
                My.Computer.Clipboard.SetText(.SelectedText)
            End If
        End With
    End Sub

#End Region

#Region "Delegate"
    Private Sub SetProgressbar()
        With Me.ProgressBarIndex
            .Value += CInt(StatusWert)
            Me.LabelAnzahl.Text = "Status: " & .Value & "/" & CStr(.Maximum) & KontaktName
        End With
    End Sub

    Private Sub SetProgressbarToMax()
        With Me.ProgressBarIndex
            If Me.RadioButtonErstelle.Checked And Not Me.RadioButtonEntfernen.Checked Then
                .Value = .Maximum
            ElseIf Me.RadioButtonEntfernen.Checked And Not Me.RadioButtonErstelle.Checked Then
                .Value = 0
            End If
        End With
        Me.BIndizierungStart.Enabled = True
        Me.BIndizierungAbbrechen.Enabled = False
    End Sub

    Private Sub SetProgressbarMax()
        Me.ProgressBarIndex.Maximum = Anzahl
    End Sub

    Private Sub DelBTelefonliste()
        If Me.InvokeRequired Then
            Dim D As New DelgButtonTelEinl(AddressOf DelBTelefonliste)
            Me.Invoke(D)
        Else
            Me.BTelefonliste.Text = "Telefone erneut einlesen"
            Me.BTelefonliste.Enabled = True
        End If
    End Sub

#End Region

#Region "Backroundworker"
    Private Sub BWIndexer_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BWIndexer.DoWork

        ErmittleKontaktanzahl()
        If Me.RadioButtonEntfernen.Checked And Not Me.RadioButtonErstelle.Checked Then
            StatusWert = Me.ProgressBarIndex.Maximum.ToString
            BWIndexer.ReportProgress(Me.ProgressBarIndex.Maximum)
        End If

        Dim olNamespace As Outlook.NameSpace ' MAPI-Namespace
        Dim olfolder As Outlook.MAPIFolder
        Dim LandesVW As String = Me.TBLandesVW.Text

        olNamespace = OlI.GetOutlook.GetNamespace("MAPI")

        If Me.CBKHO.Checked Then
            olfolder = olNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderContacts)
            If Me.RadioButtonErstelle.Checked Then
                KontaktIndexer(LandesVW, Ordner:=olfolder)
            ElseIf Me.RadioButtonEntfernen.Checked Then
                KontaktDeIndexer(olfolder, Nothing)
            End If
        Else
            If Me.RadioButtonErstelle.Checked Then
                KontaktIndexer(LandesVW, NamensRaum:=olNamespace)
            ElseIf Me.RadioButtonEntfernen.Checked Then
                KontaktDeIndexer(Nothing, olNamespace)
            End If
        End If
    End Sub

    Private Sub BWIndexer_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BWIndexer.ProgressChanged
        StatusWert = CStr(e.ProgressPercentage)
        If Me.InvokeRequired Then
            Dim D As New DelgSetProgressbar(AddressOf SetProgressbar)
            Invoke(D)
        Else
            SetProgressbar()
        End If
    End Sub

    Private Sub BWIndexer_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BWIndexer.RunWorkerCompleted

        If Me.InvokeRequired Then
            Dim D As New DelgSetProgressbar(AddressOf SetProgressbarToMax)
            Invoke(D)
        Else
            SetProgressbarToMax()
        End If
        BWIndexer.Dispose()
        Dauer = Date.Now - Startzeit
        If Me.RadioButtonErstelle.Checked And Not Me.RadioButtonEntfernen.Checked Then
            C_XML.Write("Optionen", "LLetzteIndizierung", CStr(Date.Now), True)
            C_Helfer.LogFile("Indizierung abgeschlossen: " & Anzahl & " Kontakte in " & Dauer.TotalMilliseconds & " ms")
        ElseIf Me.RadioButtonEntfernen.Checked And Not Me.RadioButtonErstelle.Checked Then
            C_Helfer.LogFile("Deindizierung abgeschlossen: " & Anzahl & " Kontakte in " & Dauer.TotalMilliseconds & " ms")
        End If
    End Sub

    Private Sub BWTelefone_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BWTelefone.DoWork
        AddLine("Einlesen der Telefone gestartet.")
        C_FBox.P_SpeichereDaten = CBool(e.Argument)
        e.Result = CBool(e.Argument)
        If Me.TBTelefonDatei.Text = vbNullString Then
            C_FBox.FritzBoxDaten()
        Else
            C_FBox.FritzBoxDatenDebug(Me.TBTelefonDatei.Text)
        End If
    End Sub

    Private Sub BWTelefone_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BWTelefone.RunWorkerCompleted
        AddLine("BackgroundWorker ist fertig.")
        Dim xPathTeile As New ArrayList
        Dim tmpTelefon As String

        'Statistik zur�ckschreiben
        With xPathTeile
            .Add("Telefone")
            .Add("Telefone")
            .Add("*")
            .Add("Telefon")
            .Add("[@Dialport = """ & """]")
            .Add("TelName")

            For Row = 0 To TelList.Rows.Count - 2
                .Item(.Count - 2) = "[@Dialport = """ & TelList.Rows(Row).Cells(2).Value.ToString & """]"
                .Item(.Count - 1) = "TelName"
                ' Pr�fe ob Telefonname und Telefonnummer �bereinstimmt
                tmpTelefon = C_XML.Read(xPathTeile, "-1")
                If Not tmpTelefon = "-1" Then
                    .Item(.Count - 1) = "TelNr"
                    If tmpTelefon = TelList.Rows(Row).Cells(4).Value.ToString And _
                        C_XML.Read(xPathTeile, "-1") = Replace(TelList.Rows(Row).Cells(5).Value.ToString, ", ", ";", , , CompareMethod.Text) Then
                        Dim Dauer As Date
                        .Item(.Count - 1) = "Eingehend"
                        Dauer = CDate(TelList.Rows(Row).Cells(6).Value.ToString())
                        C_XML.Write(xPathTeile, CStr((Dauer.Hour * 60 + Dauer.Minute) * 60 + Dauer.Second), False)
                        .Item(.Count - 1) = "Ausgehend"
                        Dauer = CDate(TelList.Rows(Row).Cells(7).Value.ToString())
                        C_XML.Write(xPathTeile, CStr((Dauer.Hour * 60 + Dauer.Minute) * 60 + Dauer.Second), True)
                    End If
                End If
            Next

            'CLBTelNrAusf�llen setzen
            .Clear()
            Dim CheckTelNr As CheckedListBox.CheckedItemCollection = Me.CLBTelNr.CheckedItems
            Dim tmpTeile As String = vbNullString
            .Add("Telefone")
            .Add("Nummern")
            .Add("*")
            For i = 0 To CheckTelNr.Count - 1
                tmpTeile += ". = " & """" & CheckTelNr.Item(i).ToString & """" & " or "
            Next
            tmpTeile = Strings.Left(tmpTeile, Len(tmpTeile) - Len(" or "))
            .Add("[" & tmpTeile & "]")
            C_XML.WriteAttribute(xPathTeile, "Checked", "1")
        End With

        CLBTelNrAusf�llen()
        SetTelNrListe()
        FillTelListe()
        DelBTelefonliste()
        BWTelefone = Nothing
        AddLine("BackgroundWorker wurde eliminiert.")
        If CBool(e.Result) Then AddLine("Das Einlesen der Telefone ist abgeschlossen.")
    End Sub
#End Region

#Region "Phoner"
    'Phoner
    'Private Sub CBKeineFB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
    '    If Me.CBPhonerKeineFB.Checked Then Me.CBJImport.Checked = False
    '    Me.CBJImport.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.ButtonTelefonliste.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.TBFBAdr.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.CBForceFBAddr.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.TBPasswort.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.lblTBPasswort.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.CBPhonerAnrMon.Checked = Me.CBPhonerKeineFB.Checked
    '    Me.CBPhonerAnrMon.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.ComboBoxPhonerSIP.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    Me.CBPhoner.Enabled = Not Me.CBPhonerKeineFB.Checked
    '    If Me.CBPhonerKeineFB.Checked Then
    '        Me.CBPhoner.Checked = True
    '        Me.ComboBoxPhonerSIP.SelectedIndex = 0
    '        Me.CLBTelNr.SetItemChecked(0, True)
    '        For i = 0 To TelList.Rows.Count - 1
    '            TelList.Rows(i).Cells(0).Value = False
    '        Next
    '    End If
    '    Me.CLBTelNr.Enabled = Not Me.CBPhonerKeineFB.Checked
    'End Sub

    Private Sub LinkPhoner_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkPhoner.LinkClicked
        System.Diagnostics.Process.Start("http://www.phoner.de/")
    End Sub

    Private Sub ButtonPhoner_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPhoner.Click
        Dim PhonerInstalliert As Boolean = C_Phoner.PhonerReady()
        Me.PanelPhonerAktiv.BackColor = CType(IIf(PhonerInstalliert, Color.LightGreen, Color.Red), Color)
        Me.LabelPhoner.Text = "Phoner ist " & CStr(IIf(PhonerInstalliert, "", "nicht ")) & "aktiv."
        Me.PanelPhoner.Enabled = PhonerInstalliert
        C_XML.Write("Phoner", "PhonerVerf�gbar", CStr(PhonerInstalliert), True)
    End Sub

    Private Sub CBPhoner_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CBPhoner.CheckedChanged
        Me.PhonerPasswort.Enabled = Me.CBPhoner.Checked
        Me.LPassworPhoner.Enabled = Me.CBPhoner.Checked
    End Sub
#End Region

End Class

Public NotInheritable Class iTa
    ' Callers do not require Unmanaged permission       
    Public Shared ReadOnly Property IsThemeActive() As Boolean
        Get
            ' No need to demand a permission in place of               
            ' UnmanagedCode as GetTickCount is considered               
            ' a safe method               
            Return SafeNativeMethods.IsThemeActive()
        End Get
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class


