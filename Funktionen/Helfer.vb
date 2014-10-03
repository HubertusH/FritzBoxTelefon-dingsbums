Imports System.Net
Imports System.Text
Imports System.Threading
Imports System.IO

Public Class Helfer
    Public Enum Vorwahllisten
        Liste_Landesvorwahlen = 0
        Liste_Ortsvorwahlen_Ausland = 1
        Liste_Ortsvorwahlen_Deutschland = 2
    End Enum

    Private C_DP As DataProvider
    Private C_Crypt As MyRijndael

    Public Sub New(ByVal DataProviderKlasse As DataProvider, ByVal CryptKlasse As MyRijndael)
        C_DP = DataProviderKlasse
        C_Crypt = CryptKlasse
    End Sub

#Region " String Behandlung"
    ''' <summary>
    ''' Entnimmt aus dem String <c>Text</c> einen enthaltenen Sub-String ausgehend von einer Zeichenfolge davor <c>StringDavor</c> 
    ''' und deiner Zeichenfolge danach <c>StringDanach</c>.
    ''' </summary>
    ''' <param name="Text">String aus dem der Sub-String entnommen werden soll.</param>
    ''' <param name="StringDavor">Zeichenfolge vor dem zu entnehmenden Sub-String.</param>
    ''' <param name="StringDanach">Zeichenfolge nach dem zu entnehmenden Sub-String.</param>
    ''' <param name="Reverse">Flag, Ob die Suche nach den Zeichenfolgen vor und nach dem Sub-String vom Ende des <c>Textes</c> aus begonnen werden soll.</param>
    ''' <returns>Wenn <c>StringDavor</c> und <c>StringDanach</c> enthalten sind, dann wird der Teilstring zur�ckgegeben. Ansonsten "-1".</returns>
    ''' <remarks></remarks>
    Public Overloads Function StringEntnehmen(ByVal Text As String, ByVal StringDavor As String, ByVal StringDanach As String, Optional ByVal Reverse As Boolean = False) As String
        Dim pos(1) As Integer

        If Not Reverse Then
            pos(0) = InStr(1, Text, StringDavor, CompareMethod.Text) + Len(StringDavor)
            pos(1) = InStr(pos(0), Text, StringDanach, CompareMethod.Text)
        Else
            pos(1) = InStrRev(Text, StringDanach, , CompareMethod.Text)
            pos(0) = InStrRev(Text, StringDavor, pos(1), CompareMethod.Text) + Len(StringDavor)
        End If

        If Not pos(0) = Len(StringDavor) Then
            StringEntnehmen = Mid(Text, pos(0), pos(1) - pos(0))
        Else
            StringEntnehmen = C_DP.P_Def_ErrorMinusOne_String
        End If
    End Function

    ''' <summary>
    ''' Entnimmt aus dem String <c>Text</c> einen enthaltenen Sub-String ausgehend von einer Zeichenfolge davor <c>StringDavor</c> 
    ''' und deiner Zeichenfolge danach <c>StringDanach</c>.
    ''' Beginnt Suche nach TeilString an einem Startpunkt <c>StartPosition</c>.
    ''' </summary>
    ''' <param name="Text">String aus dem der Sub-String entnommen werden soll.</param>
    ''' <param name="StringDavor">Zeichenfolge vor dem zu entnehmenden Sub-String.</param>
    ''' <param name="StringDanach">Zeichenfolge nach dem zu entnehmenden Sub-String.</param>
    ''' <param name="StartPosition">Startposition, bei der mit der Suche nach den Zeichenfolgen vor und nach dem Sub-String begonnen werden soll.</param>
    ''' <returns>Wenn <c>StringDavor</c> und <c>StringDanach</c> enthalten sind, dann wird der Teilstring zur�ckgegeben. Ansonsten "-1".</returns>
    ''' <remarks></remarks>
    Public Overloads Function StringEntnehmen(ByVal Text As String, ByVal StringDavor As String, ByVal StringDanach As String, ByRef StartPosition As Integer) As String
        Dim pos(1) As Integer

        pos(0) = InStr(StartPosition, Text, StringDavor, CompareMethod.Text) + Len(StringDavor)
        pos(1) = InStr(pos(0), Text, StringDanach, CompareMethod.Text)

        If Not pos(0) = Len(StringDavor) Then
            StringEntnehmen = Mid(Text, pos(0), pos(1) - pos(0))
            StartPosition = pos(1)
        Else
            StringEntnehmen = C_DP.P_Def_ErrorMinusOne_String
        End If

    End Function

    ''' <summary>
    ''' Pr�ft ob, ein String <c>A</c> in einem Sting-Array <c>B</c> enthalten ist. 
    ''' </summary>
    ''' <param name="A">Zu pr�fender String.</param>
    ''' <param name="B">Array in dem zu pr�fen ist.</param>
    ''' <returns><c>True</c>, wenn enthalten, <c>False</c>, wenn nicht.</returns>
    ''' <remarks></remarks>
    Public Function IsOneOf(ByVal A As String, ByVal B() As String) As Boolean
        Return CBool(IIf((From Strng In B Where Strng = A).ToArray.Count = 0, False, True))
    End Function
#End Region

    Public Sub NAR(ByVal o As Object)
        If o IsNot Nothing Then
            Try
                System.Runtime.InteropServices.Marshal.ReleaseComObject(o)
            Catch ex As Exception
                FBDB_MsgBox(ex.Message, MsgBoxStyle.Critical, "NAR")
            Finally
                o = Nothing
            End Try
        End If
    End Sub

    ''' <summary>
    ''' F�hrt einen Ping zur Gegenstelle aus.
    ''' </summary>
    ''' <param name="IPAdresse">IP-Adresse Netzwerkname der Gegenstelle. R�ckgabe der IP-Adresse</param>
    ''' <returns>Boolean</returns>
    ''' <remarks></remarks>
    Public Function Ping(ByRef IPAdresse As String) As Boolean
        Ping = False

        Dim IPHostInfo As IPHostEntry
        Dim PingSender As New NetworkInformation.Ping()
        Dim Options As New NetworkInformation.PingOptions()
        Dim PingReply As NetworkInformation.PingReply = Nothing
        Dim data As String = C_DP.P_Def_StringEmpty

        Dim buffer As Byte() = Encoding.ASCII.GetBytes(data)
        Dim timeout As Integer = 120

        Options.DontFragment = True

        Try
            PingReply = PingSender.Send(IPAdresse, timeout, buffer, Options)
        Catch ex As Exception
            LogFile("Ping zu """ & IPAdresse & """ nicht erfolgreich: " & ex.InnerException.Message)
            Ping = False
        End Try

        If PingReply IsNot Nothing Then
            With PingReply
                If .Status = NetworkInformation.IPStatus.Success Then
                    If .Address.AddressFamily = Sockets.AddressFamily.InterNetworkV6 Then
                        'Zugeh�rige IPv4 ermitteln
                        IPHostInfo = Dns.GetHostEntry(.Address)
                        For Each _IPAddress As IPAddress In IPHostInfo.AddressList
                            If _IPAddress.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                                IPAdresse = _IPAddress.ToString
                                ' Pr�fen ob es eine generel g�ltige lokale IPv6 Adresse gibt: fd00::2665:11ff:fed8:6086
                                ' und wie die zu ermitteln ist
                                LogFile("IPv6: " & .Address.ToString & ", IPv4: " & IPAdresse)
                                Exit For
                            End If
                        Next
                    Else
                        IPAdresse = .Address.ToString
                    End If
                    Ping = True
                Else
                    LogFile("Ping zu """ & IPAdresse & """ nicht erfolgreich: " & .Status)
                    Ping = False
                End If
            End With
        End If
        PingSender.Dispose()
        Options = Nothing
        PingSender = Nothing
    End Function

    Public Function FastPing(ByVal URL As String) As Boolean
        Return OutlookSecurity.InternetCheckConnection(URL, 0)
    End Function

    ''' <summary>
    ''' Wandelt die eingegebene IP-Adresse in eine f�r dieses Addin g�ltige IPAdresse.
    ''' IPv4 und IPv6 m�ssen differenziert behandelt werden.
    ''' F�r Anrufmonitor ist es egal ob IPv4 oder IPv6 da der RemoteEndPoint ein IPAddress-Objekt verwendet.
    ''' Die HTML/URL m�ssen gesondert beachtet werden. Daf�n muss die IPv6 in eckige Klammern gesetzt werden.
    ''' 
    ''' M�glicher Input:
    ''' IPv4: Nichts unternehmen
    ''' IPv6: 
    ''' String, der aufgel�st werden kann z.B. "fritz.box"
    ''' String, der nicht aufgel�st werden kann
    ''' </summary>
    ''' <param name="InputIP">IP-Adresse</param>
    ''' <returns>Korrekte IP-Adresse</returns>
    Public Function ValidIP(ByVal InputIP As String) As String
        Dim IPAddresse As IPAddress = Nothing
        Dim IPHostInfo As IPHostEntry

        ValidIP = C_DP.P_Def_FritzBoxAdress

        If IPAddress.TryParse(InputIP, IPAddresse) Then
            Select Case IPAddresse.AddressFamily
                Case Sockets.AddressFamily.InterNetworkV6
                    ValidIP = "[" & IPAddresse.ToString & "]"
                Case Sockets.AddressFamily.InterNetwork
                    ValidIP = IPAddresse.ToString
                Case Else
                    LogFile("Die IP """ & InputIP & """ kann nicht zugeordnet werden.")
                    ValidIP = InputIP
            End Select
        Else
            Try
                IPHostInfo = Dns.GetHostEntry(C_DP.P_TBFBAdr)
                For Each IPAddresse In IPHostInfo.AddressList
                    If IPAddresse.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                        ValidIP = IPAddresse.ToString
                    End If
                Next
            Catch ' ex As Exception
                LogFile("Die Adresse """ & C_DP.P_TBFBAdr & """ kann nicht zugeordnet werden.")
                ValidIP = C_DP.P_TBFBAdr
            End Try
        End If

    End Function

    Public Sub LogFile(ByVal Meldung As String)
        Dim LogDatei As String = C_DP.P_Arbeitsverzeichnis & C_DP.P_Def_Log_FileName
        If C_DP.P_CBLogFile Then
            With My.Computer.FileSystem
                If .FileExists(LogDatei) Then
                    If .GetFileInfo(LogDatei).Length > 1048576 Then .DeleteFile(LogDatei)
                End If
                Try
                    .WriteAllText(LogDatei, Date.Now & " - " & Meldung & vbNewLine, True)
                Catch : End Try
            End With
        End If
    End Sub

    Public Function GetEncoding(ByVal Encoding As String) As System.Text.Encoding
        Select Case LCase(Encoding)
            Case "utf-8"
                Return System.Text.Encoding.UTF8
            Case Else
                Return System.Text.Encoding.Default
        End Select
    End Function

    Public Function FBDB_MsgBox(ByVal Meldung As String, ByVal Style As MsgBoxStyle, ByVal Aufruf As String) As MsgBoxResult
        If Style = MsgBoxStyle.Critical Or Style = MsgBoxStyle.Exclamation Then
            Meldung = "Die Funktion " & Aufruf & " meldet folgenden Fehler:" & vbCrLf & vbCrLf & Meldung
            LogFile(Meldung)
        End If
        Return MsgBox(Meldung, Style, C_DP.P_Def_Addin_LangName) '"Fritz!Box Telefon-Dingsbums"
    End Function

    ''' <summary>
    ''' Diese Routine �ndert den Zugang zu den verschl�sselten Passwort.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub KeyChange()
        Dim AlterZugang As String
        Dim NeuerZugang As String
        If Not C_DP.P_TBPasswort = C_DP.P_Def_StringEmpty Then
            With C_DP
                AlterZugang = .GetSettingsVBA("Zugang", .P_Def_ErrorMinusOne_String)
                If Not AlterZugang = .P_Def_ErrorMinusOne_String Then
                    NeuerZugang = C_Crypt.GetSalt
                    .P_TBPasswort = C_Crypt.EncryptString128Bit(C_Crypt.DecryptString128Bit(.P_TBPasswort, AlterZugang), NeuerZugang)
                    .SaveSettingsVBA("Zugang", NeuerZugang)
                Else
                    LogFile(.P_Lit_KeyChange("die Fritz!Box"))
                    .P_TBPasswort = .P_Def_StringEmpty
                End If
            End With
        End If

        If Not C_DP.P_TBPhonerPasswort = C_DP.P_Def_StringEmpty Then
            With C_DP
                AlterZugang = .GetSettingsVBA("ZugangPasswortPhoner", .P_Def_ErrorMinusOne_String)
                If Not AlterZugang = .P_Def_ErrorMinusOne_String Then
                    NeuerZugang = C_Crypt.GetSalt
                    .P_TBPhonerPasswort = C_Crypt.EncryptString128Bit(C_Crypt.DecryptString128Bit(.P_TBPhonerPasswort, AlterZugang), NeuerZugang)
                    .SaveSettingsVBA("ZugangPasswortPhoner", NeuerZugang)
                Else
                    LogFile(.P_Lit_KeyChange("Phoner"))
                    .P_TBPhonerPasswort = .P_Def_StringEmpty
                End If
            End With
        End If

        C_DP.SpeichereXMLDatei()

    End Sub ' (Key�nderung) 

#Region " Telefonnummern formatieren"
    ''' <summary>
    ''' Formatiert die Telefonnummern nach g�ngigen Regeln
    ''' </summary>
    ''' <param name="TelNr">Die zu formatierende Telefonnummer</param>
    ''' <returns>Die formatierte Telefonnummer</returns>
    ''' <remarks></remarks>
    Function formatTelNr(ByVal TelNr As String) As String
        Dim RufNr As String ' Telefonnummer ohne Vorwahl

        Dim LandesVW As String
        Dim OrtsVW As String
        Dim Durchwahl As String
        Dim posOrtsVW As Integer   ' Position der Vorwahl in TelNr
        Dim posDurchwahl As Integer   ' Position der Durchwahl in TelNr
        Dim tempOrtsVW As String = C_DP.P_Def_StringEmpty ' Hilfsstring f�r OrtsVW
        Dim tempRufNr As String = C_DP.P_Def_StringEmpty ' Hilfsstring f�r RufNr
        Dim tempDurchwahl As String = C_DP.P_Def_StringEmpty ' Hilfsstring f�r LandesVW
        Dim TelTeile() As String = TelNrTeile(TelNr)
        Dim Maske As String = C_DP.P_TBTelNrMaske

        LandesVW = TelTeile(0)
        OrtsVW = TelTeile(1)
        Durchwahl = TelTeile(2)

        TelNr = nurZiffern(TelNr) ' Nur ziffern erntfernt Landesvorwahl, wenn diese mir der in den Einstellungen �bereinstimmt.

        ' 1. Landesvorwahl abtrennen
        ' Landesvorwahl ist immer an erster Stelle (wenn vorhanden)
        ' [0043]123456789
        ' Italien ist eine Ausnahme:
        ' Die f�hrende Null der Ortskennung ist fester, unver�nderlicher und unverzichtbarer Bestandteil und muss bestehen bleiben. 
        ' Handynummern in Italien haben dagegen keine f�hrende Null.
        If Not LandesVW = C_DP.P_Def_StringEmpty And Not LandesVW = C_DP.P_TBLandesVW Then
            RufNr = Mid(TelNr, Len(LandesVW) + 1)
            'If LandesVW = "0039" AndAlso Left(RufNr, 1) = "0" Then ' Italien
            ''Mach irgendwas. Oder auch nicht.
            'End If
        Else
            RufNr = TelNr
        End If

        ' 2. Ortsvorwahl entfernen
        ' [0123]456789
        If Not OrtsVW = C_DP.P_Def_StringEmpty Then
            posOrtsVW = InStr(RufNr, OrtsVW, CompareMethod.Text)
            RufNr = Mid(RufNr, posOrtsVW + Len(OrtsVW))
        Else
            If RufNr = C_DP.P_Def_StringEmpty Then RufNr = TelNr
        End If

        ' nur ausf�hren, wenn die Ortsvorwahl in der Telefonnummer enthalten ist
        ' LandesVW und RufNr aus TelNr separieren

        posDurchwahl = InStr(1, RufNr, Durchwahl, CompareMethod.Text)
        If posDurchwahl = 1 And Not Durchwahl = C_DP.P_Def_StringEmpty Then
            tempDurchwahl = Mid(RufNr, Len(Durchwahl) + 1)
            RufNr = Durchwahl
        Else
            Durchwahl = C_DP.P_Def_StringEmpty
        End If

        If LandesVW = "0" Then
            OrtsVW = "0" & OrtsVW
            LandesVW = C_DP.P_Def_StringEmpty
        End If

        ' Maske Pr�fen
        If InStr(Maske, "%D", CompareMethod.Text) = 0 Then Maske = Replace(Maske, "%N", "%N%D")
        If Not InStr(Maske, "%N%D", CompareMethod.Text) = 0 Then
            RufNr = RufNr & tempDurchwahl
            tempDurchwahl = C_DP.P_Def_StringEmpty
        End If

        If OrtsVW = C_DP.P_Def_StringEmpty And Not C_DP.P_CBintl Then
            ' Keine Ortsvorwahl: Alles zwischen %L und %N entfernen
            Dim pos1 As Integer
            Dim pos2 As Integer
            Dim CutOut As String
            pos1 = InStr(Maske, "%L", CompareMethod.Text) + 2
            pos2 = InStr(Maske, "%N", CompareMethod.Text)
            CutOut = Mid(Maske, pos1, pos2 - pos1)
            Maske = Replace(Maske, CutOut, CStr(IIf(Left(CutOut, 1) = " ", " ", C_DP.P_Def_StringEmpty)), , 1, CompareMethod.Text)
        End If
        If LandesVW = C_DP.P_Def_StringEmpty Then LandesVW = C_DP.P_TBLandesVW
        If C_DP.P_CBintl Or Not LandesVW = C_DP.P_TBLandesVW Then

            If OrtsVW = C_DP.P_Def_StringEmpty Then
                OrtsVW = C_DP.P_TBVorwahl
                If Not LandesVW = "0039" Then
                    'Else
                    If Left(OrtsVW, 1) = "0" Then
                        OrtsVW = Mid(OrtsVW, 2)
                    End If
                End If
            End If

            If Left(LandesVW, 2) = "00" Then LandesVW = Replace(LandesVW, "00", "+", 1, 1, CompareMethod.Text)
        Else
            OrtsVW = CStr(IIf(Left(OrtsVW, 1) = "0", OrtsVW, "0" & OrtsVW))
            LandesVW = C_DP.P_Def_StringEmpty
        End If

        ' NANP
        If LandesVW = "+1" Then
            Maske = "%L (%O) %N-%D"
            C_DP.P_CBTelNrGruppieren = False
            If tempDurchwahl = C_DP.P_Def_StringEmpty Then
                tempDurchwahl = Mid(RufNr, 4)
                RufNr = Left(RufNr, 3)
            End If
        End If

        If C_DP.P_CBTelNrGruppieren Then
            tempOrtsVW = GruppiereNummer(OrtsVW)
            tempRufNr = GruppiereNummer(RufNr)
            tempDurchwahl = GruppiereNummer(tempDurchwahl)
        Else
            tempOrtsVW = OrtsVW
            tempRufNr = RufNr
        End If
        ' formatTelNr zusammenstellen
        tempRufNr = Trim(Replace(tempRufNr, "  ", " ", , , CompareMethod.Text))
        ' Maske %L (%O) % - %D
        Maske = Replace(Maske, "%L", Trim(LandesVW))
        Maske = Replace(Maske, "%O", Trim(tempOrtsVW))
        Maske = Replace(Maske, "%N", tempRufNr)
        If Not Trim(tempDurchwahl) = C_DP.P_Def_StringEmpty Then
            Maske = Replace(Maske, "%D", Trim(tempDurchwahl))
        Else
            posDurchwahl = InStr(Maske, tempRufNr, CompareMethod.Text) + Len(tempRufNr) - 1
            Maske = Left(Maske, posDurchwahl)
        End If
        Maske = Trim(Replace(Maske, "  ", " ", , , CompareMethod.Text))

        Return Maske
    End Function

    Function GruppiereNummer(ByVal Nr As String) As String
        Dim imax As Integer
        imax = CInt(Math.Round(Len(Nr) / 2 + 0.1))
        GruppiereNummer = C_DP.P_Def_StringEmpty
        For i = 1 To imax
            GruppiereNummer = Right(Nr, 2) & " " & GruppiereNummer
            If Not Len(Nr) = 1 Then Nr = Left(Nr, Len(Nr) - 2)
        Next
    End Function

    Function EigeneVorwahlenEntfernen(ByVal TelNr As String) As String

        Dim tmpLandesVorwahl As String
        Dim tmpOrtsVorwahl As String

        If Not TelNr = C_DP.P_Def_StringEmpty Then

            ' TelNr bereinigen. vielleicht unn�tig
            TelNr = Replace(TelNr, "(0)", " ", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "++", "00", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "+ ", "+", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "+", "00", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "[", "(", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "]", ")", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "{", "(", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "[", ")", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "#", "", , , CompareMethod.Text)
            TelNr = Replace(TelNr, " ", "", , , CompareMethod.Text)
            With C_DP

                tmpLandesVorwahl = .P_Def_TBLandesVW
                tmpOrtsVorwahl = .P_TBVorwahl

                ' F�hrende Null der Ortsvorwahl wegschneiden
                If Left(tmpOrtsVorwahl, 1) = "0" Then tmpOrtsVorwahl = Mid(.P_TBVorwahl, 2)
                ' F�hrende 00 der Landesvorwahl entfernen
                If Left(tmpLandesVorwahl, 2) = "00" Then tmpLandesVorwahl = Mid(tmpLandesVorwahl, 3)

                ' Landesvorwahl vorhanden
                If Left(TelNr, 2) = "00" OrElse Left(TelNr, Len(tmpLandesVorwahl & tmpOrtsVorwahl)) = tmpLandesVorwahl & tmpOrtsVorwahl Then
                    ' 00 davorh�ngen falls n�tig
                    If Left(TelNr, 2) = "00" Then tmpLandesVorwahl = "00" & tmpLandesVorwahl

                    'Landesvorwahl entfernen
                    If Left(TelNr, Len(tmpLandesVorwahl)) = tmpLandesVorwahl Then TelNr = Mid(TelNr, Len(tmpLandesVorwahl) + 1)
                End If


                ' F�hrende Null der Telefonnummer wegschneide
                If Left(TelNr, 1) = "0" Then TelNr = Mid(TelNr, 2)
                ' Vorwahl wegschneiden
                If Strings.Left(TelNr, Len(tmpOrtsVorwahl)) = tmpOrtsVorwahl Then TelNr = Mid(TelNr, Len(tmpOrtsVorwahl) + 1)

            End With
        End If
        Return TelNr
    End Function

    Function TelNrTeile(ByVal TelNr As String) As String()
        ' Findet die Ortsvorwahl in einem formatierten Telefonstring
        ' Kriterien: die Ortsvorwahl befindet sich in Klammern
        '            die OrtsVorwahl wird duch ein "-", "/" oder " " von der Rufnummer separiert
        ' Eine eventuell vorhandene Landesvorwahl wird ber�cksichtigt (vorher entfernt)
        ' Parameter:  TelNr (String):  Telefonnummer, die die Ortsvorwahl enth�lt
        ' R�ckgabewert (String):       Ortsvorwahl

        Dim pos1 As Integer   ' Positionen innerhalb der TelNr
        Dim pos2 As Integer   ' Positionen innerhalb der TelNr
        Dim c As String ' einzelnes Zeichen des TelNr-Strings
        Dim OrtsVW As String = C_DP.P_Def_StringEmpty
        Dim LandesVW As String
        Dim Durchwahl As String
        Dim ErsteZiffer As String

        If Not TelNr = C_DP.P_Def_StringEmpty Then
            TelNr = Replace(TelNr, "(0)", " ", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "++", "00", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "+ ", "+", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "+", "00", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "[", "(", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "]", ")", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "{", "(", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "[", ")", , , CompareMethod.Text)
            TelNr = Replace(TelNr, "#", "", , , CompareMethod.Text)
            TelNr = Replace(TelNr, " ", "", , , CompareMethod.Text)
            If Left(TelNr, 2) = "00" Then
                'Landesvorwahl vorhanden
                LandesVW = VorwahlausDatei(TelNr, My.Resources.Liste_Landesvorwahlen)
                If Not LandesVW = C_DP.P_Def_StringEmpty Then
                    LandesVW = "00" & LandesVW
                    TelNr = Mid(TelNr, Len(LandesVW) + 1)
                End If
            Else
                LandesVW = C_DP.P_Def_StringEmpty
            End If
            LandesVW = Replace(LandesVW, " ", "", , , CompareMethod.Text) 'Leerzeichen entfernen'

            pos1 = InStr(1, TelNr, "(", CompareMethod.Text) + 1
            pos2 = InStr(1, TelNr, ")", CompareMethod.Text)
            If pos1 = 1 Or pos2 = 0 Then
                If LandesVW = C_DP.P_Def_TBLandesVW Or LandesVW = C_DP.P_Def_StringEmpty Then 'nur Deutschland
                    ' Ortsvorwahl nicht in Klammern
                    If Left(TelNr, 1) = "0" Then TelNr = Mid(TelNr, 2)
                    OrtsVW = VorwahlausDatei(TelNr, My.Resources.Liste_Ortsvorwahlen_Deutschland)

                    ' Vierstellige Mobilfunkvorwahlen ermitteln
                    'ErsteZiffer = Mid(TelNr, Len(OrtsVW) + 1, 1)
                    'Select Case OrtsVW
                    '    Case "150" ' Group3G UMTS Holding GmbH
                    '        If ErsteZiffer = "5" Then OrtsVW += ErsteZiffer
                    '    Case "151" ' Telekom Deutschland GmbH
                    '        If IsOneOf(ErsteZiffer, New String() {"1", "2", "4", "5", "6", "7"}) Then OrtsVW += ErsteZiffer
                    '    Case "152" ' Vodafone D2 GmbH
                    '        If IsOneOf(ErsteZiffer, New String() {"0", "1", "2", "3", "5", "6"}) Then OrtsVW += ErsteZiffer
                    '    Case "157" ' E-Plus Mobilfunk GmbH & Co. KG 
                    '        If IsOneOf(ErsteZiffer, New String() {"0", "3", "5", "7", "8", "9"}) Then OrtsVW += ErsteZiffer
                    '    Case "159" ' Telef�nica Germany GmbH & Co. OHG (O2) 
                    '        If IsOneOf(ErsteZiffer, New String() {"0"}) Then OrtsVW += ErsteZiffer
                    'End Select

                    ''Die Vorwahlen sind von der Bundesnetzagentur wie folgt vergeben, Wikipedia abgerufen 24.05.2014

                    ''Telekom: 01511, 01512, 01514, 01515, 01516, 01517, 0160, 0170, 0171, 0175
                    ''Vodafone: 01520, 01522, 01523, 01525, 01526 (ab M�rz 2014), 0162, 0172, 0173, 0174, 01529 (Tru)
                    ''Virtuelle Netzbetreiber (nutzt Netz von Vodafone, im Hintergrund eigene Infrastruktur): 01521 Lycamobile
                    ''E-Plus: 01573, 01575, 01577, 01578, 0163, 0177, 0178
                    ''Virtuelle Netzbetreiber (nutzen Netz von E-Plus, im Hintergrund eigene Infrastruktur): 01570 Telogic (Betrieb eingestellt), 01579 Sipgate Wireless
                    ''O2: 01590, 0176, 0179
                Else
                    OrtsVW = AuslandsVorwahlausDatei(TelNr, LandesVW)
                    Select Case LandesVW
                        Case "007" ' Kasachstan
                            ErsteZiffer = Mid(TelNr, Len(OrtsVW) + 1, 1)
                            If IsOneOf(OrtsVW, New String() {"3292", "3152", "3252", "3232", "3262"}) And ErsteZiffer = "2" Then OrtsVW += ErsteZiffer
                        Case "0039" ' Italien
                            If Left(TelNr, 1) = "0" Then OrtsVW = "0" & OrtsVW
                    End Select
                End If
                TelNr = Mid(TelNr, Len(OrtsVW) + 1) 'CInt(IIf(Left(TelNr, 1) = "0", 2, 1))
            Else
                ' Ortsvorwahl in Klammern
                OrtsVW = nurZiffern(Mid(TelNr, pos1, pos2 - pos1))
                TelNr = Trim(Mid(TelNr, pos2 + 1))
            End If
            'Durchwahl ermitteln
            pos1 = 0
            Do
                pos1 = pos1 + 1
                c = Mid(TelNr, pos1, 1)
                Windows.Forms.Application.DoEvents()
            Loop While (c >= "0" And c <= "9") And pos1 <= Len(TelNr)
            If Not pos1 = 0 And Not pos1 = Len(TelNr) + 1 Then
                Durchwahl = Left(TelNr, pos1 - 1)
            Else
                Durchwahl = C_DP.P_Def_StringEmpty
            End If
            Durchwahl = Replace(Durchwahl, " ", "", , , CompareMethod.Text) 'Leerzeichen entfernen'
        Else
            LandesVW = C_DP.P_Def_StringEmpty
            OrtsVW = C_DP.P_Def_StringEmpty
            Durchwahl = C_DP.P_Def_StringEmpty
        End If
        TelNrTeile = New String() {LandesVW, OrtsVW, Durchwahl}

    End Function

    Function VorwahlausDatei(ByVal TelNr As String, ByVal Liste As String) As String
        VorwahlausDatei = C_DP.P_Def_StringEmpty
        Dim Suchmuster As String
        Dim Vorwahlen() As String = Split(Liste, vbNewLine, , CompareMethod.Text)
        Dim i As Integer = 1
        Dim tmpErgebnis As String
        Dim Treffer As String = C_DP.P_Def_StringEmpty

        If Left(TelNr, 2) = "00" Then TelNr = Mid(TelNr, 3)
        If Left(TelNr, 1) = "0" Then TelNr = Mid(TelNr, 2)
        Do
            i += 1
            Suchmuster = Strings.Left(TelNr, i) & ";*"
            Dim Trefferliste = From s In Vorwahlen Where s.ToLower Like Suchmuster.ToLower Select s
            tmpErgebnis = Split(Trefferliste(0), ";", , CompareMethod.Text)(0)
            If Not tmpErgebnis = C_DP.P_Def_StringEmpty Then
                Treffer = tmpErgebnis
            End If
            Windows.Forms.Application.DoEvents()
        Loop Until i = 5 'Loop Until Not VorwahlausDatei = C_DP.P_Def_StringEmpty Or i = 5
        Return Treffer
    End Function

    Function AuslandsVorwahlausDatei(ByVal TelNr As String, ByVal LandesVW As String) As String
        TelNr = Replace(TelNr, "*", "", , , CompareMethod.Text)
        AuslandsVorwahlausDatei = C_DP.P_Def_StringEmpty
        Dim Suchmuster As String
        Dim Vorwahlen() As String = Split(My.Resources.Liste_Ortsvorwahlen_Ausland, vbNewLine, , CompareMethod.Text)
        Dim i As Integer = 0
        Dim tmpvorwahl() As String

        If Left(LandesVW, 2) = "00" Then LandesVW = Mid(LandesVW, 3)
        If Left(LandesVW, 1) = "0" Then LandesVW = Mid(LandesVW, 2)
        If Left(TelNr, 2) = "00" Then TelNr = Mid(TelNr, 3)
        If Left(TelNr, 1) = "0" Then TelNr = Mid(TelNr, 2)
        Do
            i += 1
            Suchmuster = LandesVW & ";" & Strings.Left(TelNr, i) & ";*"
            Dim Trefferliste = From s In Vorwahlen Where s.ToLower Like Suchmuster.ToLower Select s
            Windows.Forms.Application.DoEvents()
            tmpvorwahl = Split(Trefferliste(0), ";", , CompareMethod.Text)
            If Not tmpvorwahl.Length = 1 Then AuslandsVorwahlausDatei = tmpvorwahl(1)
        Loop Until i = 5
        'Loop Until Not AuslandsVorwahlausDatei = C_DP.P_Def_StringEmpty Or i = 5
    End Function

    Public Function nurZiffern(ByVal TelNr As String) As String
        ' aus FritzBoxDial �bernommen
        ' ist jetzt eine eigenst�ndige Funktion, da sie h�ufig gebraucht wird
        ' bereinigt die Telefunnummer von Sonderzeichen wie Klammern und Striche
        ' Buchstaben werden wie auf der Telefontastatur in Zahlen �bertragen
        ' Parameter:  TelNr (String):     Telefonnummer mit Sonderzeichen
        '             LandesVW (String):  eigene Landesvorwahl (wird entfernt)
        ' R�ckgabewert (String):       saubere Telefonnummer (nur aus Ziffern bestehend)

        Dim i As Integer   ' Z�hlvariable
        Dim c As String ' einzelnes Zeichen
        ' Dim Vorwahl As String
        'Dim pos As Integer

        nurZiffern = C_DP.P_Def_StringEmpty
        TelNr = UCase(TelNr)
        'Vorwahl = TelNrTeile(TelNr)(1)
        'pos = InStr(1, Vorwahl, ";", vbTextCompare) + 1
        'Vorwahl = Mid(Vorwahl, pos, InStr(pos, Vorwahl, ";", vbTextCompare) - pos)
        ' Nur g�ltige Zeichen in der Nummer erlauben!
        For i = 1 To Len(TelNr)
            c = Mid(TelNr, i, 1)
            Select Case c                ' Einzelnes Char auswerten
                ' Zahlen und Steuerzeichen direkt �bertragen.
                Case "0" To "9", "*", "#"
                    nurZiffern = nurZiffern + c
                    ' Restliche Buchstaben umwandeln.
                Case "A" To "C"
                    nurZiffern = nurZiffern + "2"
                Case "D" To "F"
                    nurZiffern = nurZiffern + "3"
                Case "G" To "I"
                    nurZiffern = nurZiffern + "4"
                Case "J" To "C"
                    nurZiffern = nurZiffern + "5"
                Case "M" To "O"
                    nurZiffern = nurZiffern + "6"
                Case "P" To "S"
                    nurZiffern = nurZiffern + "7"
                Case "T" To "V"
                    nurZiffern = nurZiffern + "8"
                Case "W" To "Z"
                    nurZiffern = nurZiffern + "9"
                Case "+"
                    nurZiffern = nurZiffern + "00"
            End Select
        Next
        ' Landesvorwahl entfernen bei Inlandsgespr�chen (einschlie�lich nachfolgender 0)
        If Left(nurZiffern, Len(C_DP.P_TBLandesVW)) = C_DP.P_TBLandesVW Then
            nurZiffern = Replace(nurZiffern, C_DP.P_TBLandesVW & "0", "0", , 1)
            nurZiffern = Replace(nurZiffern, C_DP.P_TBLandesVW, "0", , 1)
        End If

        ' Bei diversen VoIP-Anbietern werden 2 f�hrende Nullen zus�tzlich gew�hlt: Entfernen "000" -> "0"
        If Left(nurZiffern, 3) = "000" Then nurZiffern = Right(nurZiffern, Len(nurZiffern) - 2)
    End Function '(nurZiffern)

    Public Function Mobilnummer(ByVal TelNr As String) As Boolean
        Dim TempTelNr As String() = TelNrTeile(TelNr)
        Dim Vorwahl As String = Left(TempTelNr(1), 2)
        If TempTelNr(0) = C_DP.P_TBLandesVW Or TempTelNr(0) = C_DP.P_Def_StringEmpty Then
            If Vorwahl = "15" Or Vorwahl = "16" Or Vorwahl = "17" Then Return True
        End If
        Return False
    End Function

    Public Function TelNrVergleich(ByVal TelNr1 As String, ByVal TelNr2 As String) As Boolean
        Return nurZiffern(TelNr1) = nurZiffern(TelNr2)
    End Function

    Public Function VorwahlListe(ByVal VList As Vorwahllisten) As String
        Select Case VList
            Case Vorwahllisten.Liste_Landesvorwahlen
                VorwahlListe = My.Resources.Liste_Landesvorwahlen
            Case Vorwahllisten.Liste_Ortsvorwahlen_Ausland
                VorwahlListe = My.Resources.Liste_Ortsvorwahlen_Ausland
            Case Vorwahllisten.Liste_Ortsvorwahlen_Deutschland
                VorwahlListe = My.Resources.Liste_Ortsvorwahlen_Deutschland
            Case Else
                VorwahlListe = C_DP.P_Def_ErrorMinusOne_String
        End Select
    End Function


#End Region

#Region " HTTPTransfer"
    Public Function httpGET(ByVal Link As String, ByVal Encoding As System.Text.Encoding, ByRef FBError As Boolean) As String
        Dim UniformResourceIdentifier As New Uri(Link)

        httpGET = C_DP.P_Def_StringEmpty

        Select Case UniformResourceIdentifier.Scheme
            Case Uri.UriSchemeHttp
                If C_DP.P_Debug_Use_WebClient Then
                    Dim webClient As New WebClient
                    With webClient
                        .Encoding = Encoding
                        .Proxy = Nothing
                        .CachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.BypassCache)
                        .Headers.Add(HttpRequestHeader.KeepAlive, "False")
                        Try
                            httpGET = .DownloadString(UniformResourceIdentifier)
                        Catch exANE As ArgumentNullException
                            FBError = True
                            LogFile("httpGET_WebClient: " & exANE.Message)
                        Catch exWE As WebException
                            FBError = True
                            LogFile("httpGET_WebClient: " & exWE.Message & " - Link: " & Link)
                        End Try
                    End With
                Else
                    With CType(HttpWebRequest.Create(UniformResourceIdentifier), HttpWebRequest)
                        .Method = WebRequestMethods.Http.Get
                        .Proxy = Nothing
                        .KeepAlive = False
                        .CachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.BypassCache)
                        Try
                            With New IO.StreamReader(.GetResponse().GetResponseStream(), Encoding)
                                httpGET = .ReadToEnd()
                                .Close()
                            End With
                        Catch exANE As ArgumentNullException
                            FBError = True
                            LogFile("httpGET_Stream (ArgumentNullException): " & exANE.Message)
                        Catch exWE As WebException
                            FBError = True
                            LogFile("httpGET_Stream (WebException): " & exWE.Message & " - Link: " & Link)
                        End Try
                    End With
                End If
            Case Uri.UriSchemeFile
                With My.Computer.FileSystem
                    If .FileExists(Link) Then
                        httpGET = .ReadAllText(Link, Encoding)
                    Else
                        LogFile("Datei kann nicht gefunden werden: " & Link)
                        FBError = True
                    End If
                End With
            Case Else
                LogFile("Uri.Scheme: " & UniformResourceIdentifier.Scheme)
                FBError = True
        End Select

    End Function

    Public Function httpPOST(ByVal Link As String, ByVal Daten As String, ByVal ZeichenCodierung As System.Text.Encoding) As String
        httpPOST = C_DP.P_Def_StringEmpty
        Dim UniformResourceIdentifier As New Uri(Link)
        If UniformResourceIdentifier.Scheme = Uri.UriSchemeHttp Then
            If C_DP.P_Debug_Use_WebClient Then
                Dim webClient As New WebClient
                With webClient
                    .Encoding = ZeichenCodierung
                    .Proxy = Nothing
                    .CachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.BypassCache)

                    With .Headers
                        .Add(HttpRequestHeader.ContentLength, Daten.Length.ToString)
                        .Add(HttpRequestHeader.UserAgent, C_DP.P_Def_Header_UserAgent)
                        .Add(HttpRequestHeader.KeepAlive, "True")
                        .Add(HttpRequestHeader.Accept, C_DP.P_Def_Header_Accept)
                    End With

                    Try
                        httpPOST = .UploadString(UniformResourceIdentifier, Daten)
                    Catch exANE As ArgumentNullException
                        LogFile("httpPOST_WebClient: " & exANE.Message)
                    Catch exWE As WebException
                        LogFile("httpPOST_WebClient: " & exWE.Message & " - Link: " & Link)
                    End Try
                End With
            Else
                With CType(HttpWebRequest.Create(UniformResourceIdentifier), HttpWebRequest)
                    .Method = WebRequestMethods.Http.Post
                    .Proxy = Nothing
                    .KeepAlive = True
                    .ContentLength = Daten.Length
                    .ContentType = C_DP.P_Def_Header_ContentType
                    .Accept = C_DP.P_Def_Header_Accept
                    .UserAgent = C_DP.P_Def_Header_UserAgent
                    .CachePolicy = New Cache.HttpRequestCachePolicy(Cache.HttpRequestCacheLevel.BypassCache)
                    Try
                        With New IO.StreamWriter(.GetRequestStream)
                            .Write(Daten)
                            ThreadSleep(100)
                            .Close()
                        End With

                        With New IO.StreamReader(CType(.GetResponse, HttpWebResponse).GetResponseStream(), ZeichenCodierung)
                            httpPOST = .ReadToEnd()
                            'ThreadSleep(1000)
                            .Close()
                        End With
                    Catch exANE As ArgumentNullException
                        LogFile("httpPOST_Stream: " & exANE.Message)
                    Catch exWE As WebException
                        LogFile("httpPOST_Stream: " & exWE.Message & " - Link: " & Link)
                    End Try
                End With
            End If
        End If
    End Function
#End Region

#Region " Timer"
    Public Function SetTimer(ByRef Interval As Double) As System.Timers.Timer
        Dim aTimer As New System.Timers.Timer

        With aTimer
            .Interval = Interval
            .AutoReset = True
            .Enabled = True
        End With
        Return aTimer

    End Function

    Public Function KillTimer(ByVal Timer As System.Timers.Timer) As System.Timers.Timer
        If Timer IsNot Nothing Then
            With Timer
                .AutoReset = False
                .Enabled = False
                .Dispose()
            End With
        End If
        Return Nothing
    End Function
#End Region

#Region "Threads"
    Sub ThreadSleep(ByRef Dauer As Integer)
        Thread.Sleep(Dauer)
    End Sub
#End Region

    Public Function GetTimeInterval(ByVal nSeks As Double) As String
        'http://www.vbarchiv.net/faq/date_sectotime.php
        Dim h As Double, m As Double
        h = nSeks / 3600
        nSeks = nSeks Mod 3600
        m = nSeks / 60
        nSeks = nSeks Mod 60
        Return Format(h, "00") & ":" & Format(m, "00") & ":" & Format(nSeks, "00")
    End Function

    Public Function AcceptOnlyNumeric(ByVal sTxt As String) As String
        If sTxt = C_DP.P_Def_StringEmpty Then Return C_DP.P_Def_StringEmpty
        If Mid(sTxt, Len(sTxt), 1) Like "[0-9]" = False Then
            Return Mid(sTxt, 1, Len(sTxt) - 1)
        End If
        Return sTxt
    End Function

    Public Function TelefonName(ByVal MSN As String) As String
        TelefonName = C_DP.P_Def_StringEmpty
        If Not MSN = C_DP.P_Def_StringEmpty Then
            Dim xPathTeile As New ArrayList
            With xPathTeile
                .Add("Telefone")
                .Add("Telefone")
                .Add("*")
                .Add("Telefon")
                .Add("[contains(TelNr, """ & MSN & """) and not(@Dialport > 599)]") ' Keine Anrufbeantworter
                .Add("TelName")
            End With
            TelefonName = Replace(C_DP.Read(xPathTeile, ""), ";", ", ")
            xPathTeile = Nothing
        End If
    End Function
End Class
