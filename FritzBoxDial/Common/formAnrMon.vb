Imports System.Timers
Imports System.IO.Path

Friend Class formAnrMon
    Private TelefonName As String
    Private aID As Integer
    Private C_DP As DataProvider
    Private HelferFunktionen As Helfer
    Private TelNr As String              ' TelNr des Anrufers
    Private KontaktID As String              ' KontaktID des Anrufers
    Private StoreID As String
    Private MSN As String
    Private AnrMon As AnrufMonitor
    Public AnrmonClosed As Boolean
    Private OlI As OutlookInterface
    Private WithEvents TimerAktualisieren As Timer


    Public Sub New(ByVal iAnrufID As Integer, _
                   ByVal Aktualisieren As Boolean, _
                   ByVal DataProviderKlasse As DataProvider, _
                   ByVal HelferKlasse As Helfer, _
                   ByVal AnrufMon As AnrufMonitor, _
                   ByVal OutlInter As OutlookInterface)

        ' Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent()
        HelferFunktionen = HelferKlasse
        ' F�gen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        'If ThisAddIn.Debug Then ThisAddIn.Diagnose.AddLine("formAnrMon aufgerufen")
        aID = iAnrufID
        C_DP = DataProviderKlasse
        OlI = OutlInter
        AnrMon = AnrufMon
        AnrMonausf�llen()
        AnrmonClosed = False


        Dim OInsp As Outlook.Inspector = Nothing
        If Aktualisieren Then
            TimerAktualisieren = HelferFunktionen.SetTimer(100)
            If TimerAktualisieren Is Nothing Then
                HelferFunktionen.LogFile("formAnrMon.New: TimerNeuStart nicht gestartet")
            End If
        End If
        OlI.InspectorVerschieben(True)

        With PopupNotifier
            .ShowDelay = C_DP.P_TBEnblDauer * 1000
            .AutoAusblenden = C_DP.P_CBAutoClose
            Dim FormVerschiebung As New Drawing.Size(C_DP.P_TBAnrMonX, C_DP.P_TBAnrMonY)
            .PositionsKorrektur = FormVerschiebung
            .EffektMove = C_DP.P_CBAnrMonMove
            .EffektTransparenz = C_DP.P_CBAnrMonTransp
            .EffektMoveGeschwindigkeit = 10 * (10 - C_DP.P_TBAnrMonMoveGeschwindigkeit)
            .Popup()
        End With
        OlI.InspectorVerschieben(False)
    End Sub

    Sub AnrMonausf�llen()
        ' Diese Funktion nimmt Daten aus der Registry und �ffnet 'formAnMon'.
        Dim AnrName As String              ' Name des Anrufers
        Dim Uhrzeit As String
        'LA(0) = Zeit
        'LA(1) = Anrufer
        'LA(2) = TelNr
        'LA(3) = MSN
        'LA(4) = StoreID
        'LA(5) = KontaktID

        Dim xPathTeile As New ArrayList
        With xPathTeile
            .Add("LetzterAnrufer")
            .Add("Eintrag[@ID = """ & aID & """]")
            .Add("Zeit")
            Uhrzeit = C_DP.Read(xPathTeile, CStr(DateTime.Now))

            .Item(.Count - 1) = "Anrufer"
            AnrName = C_DP.Read(xPathTeile, "")

            .Item(.Count - 1) = "TelNr"
            TelNr = C_DP.Read(xPathTeile, C_DP.P_Def_StringUnknown)

            .Item(.Count - 1) = "MSN"
            MSN = C_DP.Read(xPathTeile, "")

            .Item(.Count - 1) = "StoreID"
            StoreID = C_DP.Read(xPathTeile, "-1")

            .Item(.Count - 1) = "KontaktID"
            KontaktID = C_DP.Read(xPathTeile, "-1")
        End With

        TelefonName = AnrMon.TelefonName(MSN)
        With PopupNotifier
            If TelNr = C_DP.P_Def_StringUnknown Then
                With .OptionsMenu
                    .Items("ToolStripMenuItemR�ckruf").Enabled = False ' kein R�ckruf im Fall 1
                    .Items("ToolStripMenuItemKopieren").Enabled = False ' in dem Fall sinnlos
                    .Items("ToolStripMenuItemKontakt�ffnen").Text = "Einen neuen Kontakt erstellen"
                End With
            End If
            ' Uhrzeit des Telefonates eintragen
            .Uhrzeit = Uhrzeit
            ' Telefonnamen eintragen
            .TelName = TelefonName & CStr(IIf(C_DP.P_CBShowMSN, " (" & MSN & ")", C_DP.P_Def_StringEmpty))

            If Not Strings.Left(KontaktID, 2) = C_DP.P_Def_ErrorMinusOne Then
                If Not TimerAktualisieren Is Nothing Then HelferFunktionen.KillTimer(TimerAktualisieren)
                ' Kontakt einblenden wenn in Outlook gefunden
                Try
                    OlI.KontaktInformation(KontaktID, StoreID, PopupNotifier.AnrName, PopupNotifier.Firma)
                    If C_DP.P_CBAnrMonContactImage Then
                        Dim BildPfad = OlI.KontaktBild(KontaktID, StoreID)
                        If Not BildPfad Is C_DP.P_Def_StringEmpty Then
                            PopupNotifier.Image = Drawing.Image.FromFile(BildPfad)
                            ' Seitenverh�ltnisse anpassen
                            Dim Bildgr��e As New Drawing.Size(PopupNotifier.ImageSize.Width, CInt((PopupNotifier.ImageSize.Width * PopupNotifier.Image.Size.Height) / PopupNotifier.Image.Size.Width))
                            PopupNotifier.ImageSize = Bildgr��e
                        End If
                    End If
                Catch ex As Exception
                    HelferFunktionen.LogFile("formAnrMon: Fehler beim �ffnen des Kontaktes " & AnrName & " (" & ex.Message & ")")
                    .Firma = C_DP.P_Def_StringEmpty
                    If AnrName = C_DP.P_Def_StringEmpty Then
                        .TelNr = C_DP.P_Def_StringEmpty
                        .AnrName = TelNr
                    Else
                        .TelNr = TelNr
                        .AnrName = AnrName
                    End If
                End Try

                .TelNr = TelNr
            Else
                .Firma = C_DP.P_Def_StringEmpty
                If AnrName = C_DP.P_Def_StringEmpty Then
                    .TelNr = C_DP.P_Def_StringEmpty
                    .AnrName = TelNr
                Else
                    .TelNr = TelNr
                    .AnrName = AnrName
                End If
            End If
        End With
    End Sub

    Private Sub PopupNotifier_Close() Handles PopupNotifier.Close
        PopupNotifier.Hide()
    End Sub

    Private Sub ToolStripMenuItemR�ckruf_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ToolStripMenuItemR�ckruf.Click
        ThisAddIn.P_WClient.Rueckruf(aID)
    End Sub

    Private Sub ToolStripMenuItemKopieren_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ToolStripMenuItemKopieren.Click
        With PopupNotifier
            My.Computer.Clipboard.SetText(.AnrName & CStr(IIf(Len(.TelNr) = 0, "", " (" & .TelNr & ")")))
        End With
    End Sub

    Private Sub PopupNotifier_Closed() Handles PopupNotifier.Closed
        AnrmonClosed = True
        If Not TimerAktualisieren Is Nothing Then HelferFunktionen.KillTimer(TimerAktualisieren)
    End Sub

    Private Sub ToolStripMenuItemKontakt�ffnen_Click() Handles ToolStripMenuItemKontakt�ffnen.Click, PopupNotifier.LinkClick
        ' blendet den Kontakteintrag des Anrufers ein
        ' ist kein Kontakt vorhanden, dann wird einer angelegt und mit den vCard-Daten ausgef�llt
        Dim Kontaktdaten(2) As String
        Kontaktdaten(0) = KontaktID
        Kontaktdaten(1) = StoreID
        Kontaktdaten(2) = TelNr
        ThisAddIn.P_WClient.ZeigeKontakt(Kontaktdaten)
    End Sub

    Private Sub TimerAktualisieren_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles TimerAktualisieren.Elapsed
        Dim VergleichString As String = PopupNotifier.AnrName
        AnrMonausf�llen()
        If Not VergleichString = PopupNotifier.AnrName Then HelferFunktionen.KillTimer(TimerAktualisieren)
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
