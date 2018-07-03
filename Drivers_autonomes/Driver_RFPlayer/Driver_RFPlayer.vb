Imports HoMIDom
Imports HoMIDom.HoMIDom
Imports HoMIDom.HoMIDom.Server
Imports HoMIDom.HoMIDom.Device

Imports System.Text.RegularExpressions
Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.IO
Imports System.IO.Ports
'Imports System.Net
'Imports System.Net.Sockets
'Imports System.Collections.Concurrent
'Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq

' Auteur : JPHomi
' Date : 01/01/2018

''' <summary>Class Driver_RFPlayer, permet de communiquer avec le controleur RFPlayer</summary>
''' <remarks></remarks>
''' 
<Serializable()> Public Class Driver_RFPlayer
    Implements HoMIDom.HoMIDom.IDriver

#Region "Variables génériques"
    '!!!Attention les variables ci-dessous doivent avoir une valeur par défaut obligatoirement
    'aller sur l'adresse http://www.somacon.com/p113.php pour avoir un ID
    Dim _ID As String = "C931722E-D3BB-11E7-A673-09F918A05F0F"
    Dim _Nom As String = "RFPlayer"
    Dim _Enable As Boolean = False
    Dim _Description As String = "Controleur RFPlayer"
    Dim _StartAuto As Boolean = False
    Dim _Protocol As String = "COM"
    Dim _IsConnect As Boolean = False
    Dim _IP_TCP As String = "@"
    Dim _Port_TCP As String = "@"
    Dim _IP_UDP As String = "@"
    Dim _Port_UDP As String = "@"
    Dim _Com As String = "COM1"
    Dim _Refresh As Integer = 0
    Dim _Modele As String = "@"
    Dim _Version As String = My.Application.Info.Version.ToString
    Dim _OsPlatform As String = "3264"
    Dim _Picture As String = ""
    Dim _Server As HoMIDom.HoMIDom.Server
    Dim _Device As HoMIDom.HoMIDom.Device
    Dim _DeviceSupport As New ArrayList
    Dim _Parametres As New ArrayList
    Dim _LabelsDriver As New ArrayList
    Dim _LabelsDevice As New ArrayList
    Dim MyTimer As New Timers.Timer
    Dim _IdSrv As String
    Dim _DeviceCommandPlus As New List(Of HoMIDom.HoMIDom.Device.DeviceCommande)
    Dim _AutoDiscover As Boolean = False

    'parametres avancés
    Dim _DEBUG As Boolean = False

    'Ajoutés dans les ppt avancés dans New()
    Dim _tempsentrereponse As Integer = 1500
    Dim _ignoreadresse As Boolean = False
    Dim _lastetat As Boolean = True


#End Region

#Region "Variables Internes"
    ' Variables de gestion du port COM
    Private WithEvents port As New SerialPort
    Private port_name As String = ""
    Dim _baudspeed As Integer = 115200
    Dim _nbrebit As Integer = 8
    Dim _parity As IO.Ports.Parity = IO.Ports.Parity.None
    Dim _nbrebitstop As IO.Ports.StopBits = IO.Ports.StopBits.One
    Dim FrameStr As String = ""
    Dim FrameCompleted As Boolean = False
    '    Dim TransmitterActif As New List(Of String)
    Dim TransmitterAvailable As New List(Of String)
    Dim ReceiverAvailable As New List(Of String)
    Dim ReceiverEnable As New List(Of String)
    Dim RepeaterAvailable As New List(Of String)
    Dim RepeaterEnable As New List(Of String)
    Dim jsonObjConf As Object
    Dim jsonObjDatas As Object

    Public ListInfoSystemStatus As List(Of infosystem) = New List(Of infosystem)
    Public ListOfDevices As List(Of device) = New List(Of device)


    Public Class infosystem
        Public n As String
        Public v As String
        Public unit As String
        Public c As String
    End Class

    Public Class device
        Public id As String
        Public name As String
        Public protocol As String
        Public subTypeMeaning As String
        Public qualifier As String
    End Class

    Enum ListProtocol As Integer
        X10 = 1
        VISONIC433 = 2
        VISONIC868 = 2
        BLYSS = 3
        CHACON = 4
        DOMIA = 6
        X2D433 = 8
        X2D868 = 8
        XDSHUTTER = 8
        RTS = 9
        KD101 = 10
        PARROT = 11
    End Enum
#End Region

#Region "Propriétés génériques"
    Public WriteOnly Property IdSrv As String Implements HoMIDom.HoMIDom.IDriver.IdSrv
        Set(ByVal value As String)
            _IdSrv = value
        End Set
    End Property

    Public Property COM() As String Implements HoMIDom.HoMIDom.IDriver.COM
        Get
            Return _Com
        End Get
        Set(ByVal value As String)
            _Com = value
        End Set
    End Property
    Public ReadOnly Property Description() As String Implements HoMIDom.HoMIDom.IDriver.Description
        Get
            Return _Description
        End Get
    End Property
    Public ReadOnly Property DeviceSupport() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.DeviceSupport
        Get
            Return _DeviceSupport
        End Get
    End Property

    Public Property Parametres() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.Parametres
        Get
            Return _Parametres
        End Get
        Set(ByVal value As System.Collections.ArrayList)
            _Parametres = value
        End Set
    End Property

    Public Property LabelsDriver() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.LabelsDriver
        Get
            Return _LabelsDriver
        End Get
        Set(ByVal value As System.Collections.ArrayList)
            _LabelsDriver = value
        End Set
    End Property
    Public Property LabelsDevice() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.LabelsDevice
        Get
            Return _LabelsDevice
        End Get
        Set(ByVal value As System.Collections.ArrayList)
            _LabelsDevice = value
        End Set
    End Property

    Public Event DriverEvent(ByVal DriveName As String, ByVal TypeEvent As String, ByVal Parametre As Object) Implements HoMIDom.HoMIDom.IDriver.DriverEvent
    Public Property Enable() As Boolean Implements HoMIDom.HoMIDom.IDriver.Enable
        Get
            Return _Enable
        End Get
        Set(ByVal value As Boolean)
            _Enable = value
        End Set
    End Property
    Public ReadOnly Property ID() As String Implements HoMIDom.HoMIDom.IDriver.ID
        Get
            Return _ID
        End Get
    End Property
    Public Property IP_TCP() As String Implements HoMIDom.HoMIDom.IDriver.IP_TCP
        Get
            Return _IP_TCP
        End Get
        Set(ByVal value As String)
            _IP_TCP = value
        End Set
    End Property
    Public Property IP_UDP() As String Implements HoMIDom.HoMIDom.IDriver.IP_UDP
        Get
            Return _IP_UDP
        End Get
        Set(ByVal value As String)
            _IP_UDP = value
        End Set
    End Property
    Public ReadOnly Property IsConnect() As Boolean Implements HoMIDom.HoMIDom.IDriver.IsConnect
        Get
            Return _IsConnect
        End Get
    End Property
    Public Property Modele() As String Implements HoMIDom.HoMIDom.IDriver.Modele
        Get
            Return _Modele
        End Get
        Set(ByVal value As String)
            _Modele = value
        End Set
    End Property
    Public ReadOnly Property Nom() As String Implements HoMIDom.HoMIDom.IDriver.Nom
        Get
            Return _Nom
        End Get
    End Property
    Public Property Picture() As String Implements HoMIDom.HoMIDom.IDriver.Picture
        Get
            Return _Picture
        End Get
        Set(ByVal value As String)
            _Picture = value
        End Set
    End Property
    Public Property Port_TCP() As String Implements HoMIDom.HoMIDom.IDriver.Port_TCP
        Get
            Return _Port_TCP
        End Get
        Set(ByVal value As String)
            _Port_TCP = value
        End Set
    End Property
    Public Property Port_UDP() As String Implements HoMIDom.HoMIDom.IDriver.Port_UDP
        Get
            Return _Port_UDP
        End Get
        Set(ByVal value As String)
            _Port_UDP = value
        End Set
    End Property
    Public ReadOnly Property Protocol() As String Implements HoMIDom.HoMIDom.IDriver.Protocol
        Get
            Return _Protocol
        End Get
    End Property
    Public Property Refresh() As Integer Implements HoMIDom.HoMIDom.IDriver.Refresh
        Get
            Return _Refresh
        End Get
        Set(ByVal value As Integer)
            If value >= 1 Then
                _Refresh = value

            End If
        End Set
    End Property
    Public Property Server() As HoMIDom.HoMIDom.Server Implements HoMIDom.HoMIDom.IDriver.Server
        Get
            Return _Server
        End Get
        Set(ByVal value As HoMIDom.HoMIDom.Server)
            _Server = value
        End Set
    End Property
    Public ReadOnly Property Version() As String Implements HoMIDom.HoMIDom.IDriver.Version
        Get
            Return _Version
        End Get
    End Property
    Public ReadOnly Property OsPlatform() As String Implements HoMIDom.HoMIDom.IDriver.OsPlatform
        Get
            Return _OsPlatform
        End Get
    End Property
    Public Property StartAuto() As Boolean Implements HoMIDom.HoMIDom.IDriver.StartAuto
        Get
            Return _StartAuto
        End Get
        Set(ByVal value As Boolean)
            _StartAuto = value
        End Set
    End Property
    Public Property AutoDiscover() As Boolean Implements HoMIDom.HoMIDom.IDriver.AutoDiscover
        Get
            Return _AutoDiscover
        End Get
        Set(ByVal value As Boolean)
            _AutoDiscover = value
        End Set
    End Property

#End Region

#Region "Fonctions génériques"
    ''' <summary>
    ''' Retourne la liste des Commandes avancées
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetCommandPlus() As List(Of DeviceCommande)
        Return _DeviceCommandPlus
    End Function

    ''' <summary>Execute une commande avancée</summary>
    ''' <param name="MyDevice">Objet représentant le Device </param>
    ''' <param name="Command">Nom de la commande avancée à éxécuter</param>
    ''' <param name="Param">tableau de paramétres</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExecuteCommand(ByVal MyDevice As Object, ByVal Command As String, Optional ByVal Param() As Object = Nothing) As Boolean
        Dim retour As Boolean = False

        Try
            If MyDevice IsNot Nothing Then
                'Pas de commande demandée donc erreur
                If Command = "" Then
                    Return False
                Else
                    ' Traitement de la commande 
                    Select Case UCase(Command)
                        Case "ADD_ASSOCIATION"
                            ' CO_CR_LEARNMODE
                            Dim buf1 As Byte() = {&H55, &H0, &H1, &H0, &H5, &H23, &H1, &H0}
                            port.Write(buf1, 0, 8)
                            WriteLog("ExecuteCommand, Passage par la commande ADD_ASSOCIATION ")
                        Case "STOP_ASSOCIATION"
                            ' CO_CR_LEARNMODE
                            Dim buf1 As Byte() = {&H55, &H0, &H1, &H0, &H5, &H23, &H0, &H0}
                            port.Write(buf1, 0, 8)
                            WriteLog("ExecuteCommand, Passage par la commande STOP_ASSOCIATION ")

                            Return True
                    End Select
                End If
            Else
                Return False
            End If
        Catch ex As Exception
            WriteLog("ERR: ExecuteCommand exception : " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Permet de vérifier si un champ est valide
    ''' </summary>
    ''' <param name="Champ"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Function VerifChamp(ByVal Champ As String, ByVal Value As Object) As String Implements HoMIDom.HoMIDom.IDriver.VerifChamp
        Try
            Dim retour As String = "0"

            Select Case UCase(Champ)
                Case "ADRESSE2"
                    ' Suppression des espaces inutiles
                    If InStr(Value, ":") Then
                        Dim ParaAdr2 = Split(Value, ":")
                        Value = Trim(ParaAdr2(0)) & ":" & Trim(ParaAdr2(1))
                    End If

            End Select
            Return retour
        Catch ex As Exception
            Return "Une erreur est apparue lors de la vérification du champ " & Champ & ": " & ex.ToString
        End Try
    End Function

    ''' <summary>Démarrer le driver</summary>
    ''' <remarks></remarks>
    Public Sub Start() Implements HoMIDom.HoMIDom.IDriver.Start

        'récupération des paramétres avancés
        Try
            _DEBUG = _Parametres.Item(0).Valeur

        Catch ex As Exception
            _DEBUG = False
            _Parametres.Item(0).Valeur = False
            WriteLog("ERR: Start, Erreur dans les paramétres avancés. utilisation des valeur par défaut : " & ex.Message)
        End Try

        'ouverture du port suivant le Port Com
        Try
            If _Com <> "" Then
                WriteLog("Demarrage du pilote, ceci peut prendre plusieurs secondes")
                WriteLog(ouvrir(_Com))

                If Not _IsConnect Then Exit Sub
                WriteLog("Driver " & Me.Nom & " démarré ")


                AddHandler port.DataReceived, AddressOf ReceiveFromSerial
                'passe les échanges en json
                SendToSerial("ZIA++FORMAT JSON", 0)

                SendToSerial("ZIA++STATUS SYSTEM JSON", 3)
                WriteLog("Version " & ListInfoSystemStatus.Item(0).v & " / mac adress " & ListInfoSystemStatus.Item(1).v)

                '    SendToSerial("ZIA++STATUS RADIO JSON", 3)

                ' SendToSerial("ZIA++FRAME JSON", 3)



            Else
                WriteLog("ERR: Port Com non défini. Impossible d'ouvrir le port !")
                _IsConnect = False
            End If
        Catch ex As Exception
            WriteLog("ERR: Driver " & Me.Nom & " Erreur démarrage " & ex.Message)
            _IsConnect = False
        End Try
    End Sub

    ''' <summary>Arrêter le driver</summary>
    ''' <remarks></remarks>
    Public Sub [Stop]() Implements HoMIDom.HoMIDom.IDriver.Stop

        Try
            If _IsConnect Then
                port.Dispose()
                port.Close()
                _IsConnect = False
                WriteLog("Driver " & Me.Nom & ", port fermé")
                WriteLog("Driver " & Me.Nom & " arrêté")
            Else
                WriteLog("Driver " & Me.Nom & "Port " & _Com & " est déjà fermé")
            End If

        Catch ex As Exception
            WriteLog("ERR: Driver " & Me.Nom & " Erreur arrêt " & ex.Message)
        End Try
    End Sub

    ''' <summary>Re-Démarrer le du driver</summary>
    ''' <remarks></remarks>
    Public Sub Restart() Implements HoMIDom.HoMIDom.IDriver.Restart
        [Stop]()
        Start()
    End Sub

    ''' <summary>Intérroger un device</summary>
    ''' <param name="Objet">Objet représetant le device à interroger</param>
    ''' <remarks>pas utilisé</remarks>
    Public Sub Read(ByVal Objet As Object) Implements HoMIDom.HoMIDom.IDriver.Read
        Try
            Try ' lecture de la variable debug, permet de rafraichir la variable debug sans redemarrer le service
                _DEBUG = _Parametres.Item(0).Valeur
            Catch ex As Exception
                _DEBUG = False
                _Parametres.Item(0).Valeur = False
                WriteLog("ERR: Erreur de lecture de debug : " & ex.Message)
            End Try

            If _Enable = False Then Exit Sub

            If _IsConnect = False Then
                WriteLog("ERR: READ, Le driver n'est pas démarré, impossible d'écrire sur le port")
                Exit Sub
            End If
        Catch ex As Exception
            WriteLog("ERR: Read, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Commander un device</summary>
    ''' <param name="Objet">Objet représetant le device à interroger</param>
    ''' <param name="Commande">La commande à passer</param>
    ''' <param name="Parametre1"></param>
    ''' <param name="Parametre2"></param>
    ''' <remarks></remarks>
    Public Sub Write(ByVal Objet As Object, ByVal Commande As String, Optional ByVal Parametre1 As Object = Nothing, Optional ByVal Parametre2 As Object = Nothing) Implements HoMIDom.HoMIDom.IDriver.Write


        Dim texteCommande As String

        Dim TempVersion As Byte = 0
        _DEBUG = _Parametres.Item(0).Valeur


        If _Enable = False Then
            WriteLog("ERR: " & "Write, Erreur: Impossible de traiter la commande car le driver n'est pas activé (Enable)")
            Exit Sub
        End If

        If _IsConnect = False Then
            WriteLog("ERR: " & "Write, Erreur: Impossible de traiter la commande car le driver n'est pas connecté")
            Exit Sub
        End If
        Try

            Dim ParaAdr1 = Split(Objet.Adresse1, "#")
            ParaAdr1(0) = Trim(ParaAdr1(0))
            ParaAdr1(1) = Trim(ParaAdr1(1))
            Dim qlif As String = "0"
            For i As Integer = 0 To ListOfDevices.Count - 1
                If (ListOfDevices.Item(i).id = Objet.Adresse2) And (ListOfDevices.Item(i).protocol = ParaAdr1(0)) Then
                    qlif = ListOfDevices.Item(i).qualifier
                End If
            Next


            Select Case True
                Case (Objet.Type = "LAMPE" Or Objet.Type = "APPAREIL" Or Objet.Type = "SWITCH" Or Objet.Type = "VOLET")
                    texteCommande = UCase(Commande)
                    Select Case True
                        Case UCase(Commande) = "ON"
                            Select Case ParaAdr1(0)
                                Case "1" 'Frame Protocol 1		X10, infotype 0, 1
                                    WriteInfoType0("ZIA++ON " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "2" 'Frame Protocol 2		VISONIC, infotype 2
                                    WriteInfoType2("ZIA++ON " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "3" 'Frame Protocol 3		BLYSS, infotype 1
                                Case "4" 'Frame Protocol 4		CHACON, infotype 1
                                Case "5" 'Frame Protocol 5		Oregon, infotype 4, 5, 6, 7, 9
                                Case "6" 'Frame Protocol 6		DOMIA, infotype 0
                                    WriteInfoType0("ZIA++ON " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "7" 'Frame Protocol 7		OWL, infotype 8
                                Case "8" 'Frame Protocol 8		X2D, infotype 10, 11
                                Case "9" 'Frame Protocol 9		RTS, infotype 3
                                    If qlif = "0" Then
                                        WriteInfoType3("ZIA++ON " & ParaAdr1(1) & " " & Objet.Adresse2)
                                    Else
                                        WriteInfoType3("ZIA++ON " & ParaAdr1(1) & " " & Objet.Adresse2 & " QUALIFIER " & qlif)
                                    End If
                                Case "10" 'Frame Protocol 10	KD101, infotype 1
                                Case "11" 'Frame Protocol 11   PARROT, infotype 0
                                    WriteInfoType0("ZIA++ON " & ParaAdr1(1) & " " & Objet.Adresse2)
                            End Select

                        Case UCase(Commande) = "OFF"
                            Select Case ParaAdr1(0)
                                Case "1" 'Frame Protocol 1		X10, infotype 0, 1
                                    WriteInfoType0("ZIA++OFF " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "2" 'Frame Protocol 2		VISONIC, infotype 2
                                    WriteInfoType2("ZIA++OFF " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "3" 'Frame Protocol 3		BLYSS, infotype 1
                                Case "4" 'Frame Protocol 4		CHACON, infotype 1
                                Case "5" 'Frame Protocol 5		Oregon, infotype 4, 5, 6, 7, 9
                                Case "6" 'Frame Protocol 6		DOMIA, infotype 0
                                    WriteInfoType0("ZIA++OFF " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "7" 'Frame Protocol 7		OWL, infotype 8
                                Case "8" 'Frame Protocol 8		X2D, infotype 10, 11
                                Case "9" 'Frame Protocol 9		RTS, infotype 3
                                    If qlif = "0" Then
                                        WriteInfoType3("ZIA++OFF " & ParaAdr1(1) & " " & Objet.Adresse2)
                                    Else
                                        WriteInfoType3("ZIA++OFF " & ParaAdr1(1) & " " & Objet.Adresse2 & " QUALIFIER " & qlif)
                                    End If
                                Case "10" 'Frame Protocol 10	KD101, infotype 1
                                Case "11" 'Frame Protocol 11   PARROT, infotype 0
                                    WriteInfoType0("ZIA++OFF " & ParaAdr1(1) & " " & Objet.Adresse2)
                            End Select

                        Case UCase(Commande) = "DIM" Or UCase(Commande) = "OUVERTURE"
                            Select Case ParaAdr1(0)
                                Case "1" 'Frame Protocol 1		X10, infotype 0, 1
                                Case "2" 'Frame Protocol 2		VISONIC, infotype 2
                                Case "3" 'Frame Protocol 3		BLYSS, infotype 1
                                Case "4" 'Frame Protocol 4		CHACON, infotype 1
                                Case "5" 'Frame Protocol 5		Oregon, infotype 4, 5, 6, 7, 9
                                Case "6" 'Frame Protocol 6		DOMIA, infotype 0
                                Case "7" 'Frame Protocol 7		OWL, infotype 8
                                Case "8" 'Frame Protocol 8		X2D, infotype 10, 11
                                Case "9" 'Frame Protocol 9		RTS, infotype 3
                                    ' If qlif = "0" Then
                                    'End If
                                    WriteInfoType3("ZIA++DIM %" & Parametre1 & " " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "10" 'Frame Protocol 10	KD101, infotype 1
                                Case "11" 'Frame Protocol 11   PARROT, infotype 0
                            End Select
                        Case UCase(Commande) = "SETPOINT"   'Ecrire une valeur vers le device physique
                            If Not IsNothing(Parametre1) Then
                                Dim ValDimmer As Single
                                If IsNumeric(Parametre1) Then ValDimmer = Parametre1
                                Select Case True  ' gestion des modes de chauffage
                                    Case (UCase(Parametre1) = "CONFORT" Or UCase(Parametre1) = "CONF")
                                        ValDimmer = 95
                                    Case (UCase(Parametre1) = "CONFORT-1" Or UCase(Parametre1) = "CONF-1")
                                        ValDimmer = 45
                                    Case (UCase(Parametre1) = "CONFORT-2" Or UCase(Parametre1) = "CONF-2")
                                        ValDimmer = 35
                                    Case (UCase(Parametre1) = "ECO" Or UCase(Parametre1) = "EC")
                                        ValDimmer = 25
                                    Case (UCase(Parametre1) = "HORSGEL" Or UCase(Parametre1) = "HG")
                                        ValDimmer = 15
                                    Case UCase(Parametre1) = "ARRET"
                                        ValDimmer = 5
                                End Select
                            End If
                        Case "ALL_LIGHT_ON"
                            Select Case ParaAdr1(0)
                                Case "1" 'Frame Protocol 1		X10, infotype 0, 1
                                Case "2" 'Frame Protocol 2		VISONIC, infotype 2
                                Case "3" 'Frame Protocol 3		BLYSS, infotype 1
                                Case "4" 'Frame Protocol 4		CHACON, infotype 1
                                Case "5" 'Frame Protocol 5		Oregon, infotype 4, 5, 6, 7, 9
                                Case "6" 'Frame Protocol 6		DOMIA, infotype 0
                                Case "7" 'Frame Protocol 7		OWL, infotype 8
                                Case "8" 'Frame Protocol 8		X2D, infotype 10, 11
                                Case "9" 'Frame Protocol 9		RTS, infotype 3
                                    If qlif = "0" Then WriteInfoType3("ZIA++DIM %" & Parametre1 & " " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "10" 'Frame Protocol 10	KD101, infotype 1
                                Case "11" 'Frame Protocol 11   PARROT, infotype 0
                            End Select

                        Case "ALL_LIGHT_OFF"
                            Select Case ParaAdr1(0)
                                Case "1" 'Frame Protocol 1		X10, infotype 0, 1
                                Case "2" 'Frame Protocol 2		VISONIC, infotype 2
                                Case "3" 'Frame Protocol 3		BLYSS, infotype 1
                                Case "4" 'Frame Protocol 4		CHACON, infotype 1
                                Case "5" 'Frame Protocol 5		Oregon, infotype 4, 5, 6, 7, 9
                                Case "6" 'Frame Protocol 6		DOMIA, infotype 0
                                Case "7" 'Frame Protocol 7		OWL, infotype 8
                                Case "8" 'Frame Protocol 8		X2D, infotype 10, 11
                                Case "9" 'Frame Protocol 9		RTS, infotype 3
                                    If qlif = "0" Then WriteInfoType3("ZIA++DIM %" & Parametre1 & " " & ParaAdr1(1) & " " & Objet.Adresse2)
                                Case "10" 'Frame Protocol 10	KD101, infotype 1
                                Case "11" 'Frame Protocol 11   PARROT, infotype 0
                            End Select
                    End Select
                    WriteLog("DBG: " & "ExecuteCommand, Passage par la commande " & UCase(Commande))
            End Select
        Catch ex As Exception
            WriteLog("ERR: Write, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Fonction lancée lors de la suppression d'un device</summary>
    ''' <param name="DeviceId">Objet représetant le device à interroger</param>
    ''' <remarks></remarks>
    Public Sub DeleteDevice(ByVal DeviceId As String) Implements HoMIDom.HoMIDom.IDriver.DeleteDevice
        Try

        Catch ex As Exception
            WriteLog("ERR: DeleteDevice, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Fonction lancée lors de l'ajout d'un device</summary>
    ''' <param name="DeviceId">Objet représetant le device à interroger</param>
    ''' <remarks></remarks>
    Public Sub NewDevice(ByVal DeviceId As String) Implements HoMIDom.HoMIDom.IDriver.NewDevice
        Try

        Catch ex As Exception
            WriteLog("ERR: NewDevice, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>ajout des commandes avancées pour les devices</summary>
    ''' <param name="nom">Nom de la commande avancée</param>
    ''' <param name="description">Description qui sera affichée dans l'admin</param>
    ''' <param name="nbparam">Nombre de parametres attendus</param>
    ''' <remarks></remarks>
    Private Sub Add_DeviceCommande(ByVal Nom As String, ByVal Description As String, ByVal NbParam As Integer)
        Try
            Dim x As New DeviceCommande
            x.NameCommand = Nom
            x.DescriptionCommand = Description
            x.CountParam = NbParam
            _DeviceCommandPlus.Add(x)
        Catch ex As Exception
            WriteLog("ERR: Add_DeviceCommande, Exception :" & ex.Message)
        End Try
    End Sub

    ''' <summary>ajout Libellé pour le Driver</summary>
    ''' <param name="nom">Nom du champ : HELP</param>
    ''' <param name="labelchamp">Nom à afficher : Aide</param>
    ''' <param name="tooltip">Tooltip à afficher au dessus du champs dans l'admin</param>
    ''' <remarks></remarks>
    Private Sub Add_LibelleDriver(ByVal Nom As String, ByVal Labelchamp As String, ByVal Tooltip As String, Optional ByVal Parametre As String = "")
        Try
            Dim y0 As New HoMIDom.HoMIDom.Driver.cLabels
            y0.LabelChamp = Labelchamp
            y0.NomChamp = UCase(Nom)
            y0.Tooltip = Tooltip
            y0.Parametre = Parametre
            _LabelsDriver.Add(y0)
        Catch ex As Exception
            WriteLog("ERR: Add_DeviceCommande, Exception :" & ex.Message)
        End Try
    End Sub

    ''' <summary>Ajout Libellé pour les Devices</summary>
    ''' <param name="nom">Nom du champ : HELP</param>
    ''' <param name="labelchamp">Nom à afficher : Aide, si = "@" alors le champ ne sera pas affiché</param>
    ''' <param name="tooltip">Tooltip à afficher au dessus du champs dans l'admin</param>
    ''' <remarks></remarks>
    Private Sub Add_LibelleDevice(ByVal Nom As String, ByVal Labelchamp As String, ByVal Tooltip As String, Optional ByVal Parametre As String = "")
        Try
            Dim ld0 As New HoMIDom.HoMIDom.Driver.cLabels
            ld0.LabelChamp = Labelchamp
            ld0.NomChamp = UCase(Nom)
            ld0.Tooltip = Tooltip
            ld0.Parametre = Parametre
            _LabelsDevice.Add(ld0)
        Catch ex As Exception
            WriteLog("ERR: Add_LibelleDevice, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>ajout de parametre avancés</summary>
    ''' <param name="nom">Nom du parametre (sans espace)</param>
    ''' <param name="description">Description du parametre</param>
    ''' <param name="valeur">Sa valeur</param>
    ''' <remarks></remarks>
    Private Sub Add_ParamAvance(ByVal nom As String, ByVal description As String, ByVal valeur As Object)
        Try
            Dim x As New HoMIDom.HoMIDom.Driver.Parametre
            x.Nom = nom
            x.Description = description
            x.Valeur = valeur
            _Parametres.Add(x)
        Catch ex As Exception
            WriteLog("ERR: Add_ParamAvance, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Creation d'un objet de type</summary>
    ''' <remarks></remarks>
    Public Sub New()
        Try

            'liste des devices compatibles
            _DeviceSupport.Add(ListeDevices.APPAREIL.ToString)
            _DeviceSupport.Add(ListeDevices.BATTERIE.ToString)
            _DeviceSupport.Add(ListeDevices.CONTACT.ToString)
            _DeviceSupport.Add(ListeDevices.DETECTEUR.ToString)
            _DeviceSupport.Add(ListeDevices.DIRECTIONVENT.ToString)
            _DeviceSupport.Add(ListeDevices.ENERGIEINSTANTANEE.ToString)
            _DeviceSupport.Add(ListeDevices.ENERGIETOTALE.ToString)
            _DeviceSupport.Add(ListeDevices.GENERIQUEBOOLEEN.ToString)
            _DeviceSupport.Add(ListeDevices.GENERIQUESTRING.ToString)
            _DeviceSupport.Add(ListeDevices.GENERIQUEVALUE.ToString)
            _DeviceSupport.Add(ListeDevices.HUMIDITE.ToString)
            _DeviceSupport.Add(ListeDevices.LAMPE.ToString)
            _DeviceSupport.Add(ListeDevices.LAMPERGBW.ToString)
            _DeviceSupport.Add(ListeDevices.SWITCH.ToString)
            _DeviceSupport.Add(ListeDevices.TELECOMMANDE.ToString)
            _DeviceSupport.Add(ListeDevices.TEMPERATURE.ToString)
            _DeviceSupport.Add(ListeDevices.TEMPERATURECONSIGNE.ToString)
            _DeviceSupport.Add(ListeDevices.VITESSEVENT.ToString)
            _DeviceSupport.Add(ListeDevices.VOLET.ToString)

            'Paramétres avancés
            Add_ParamAvance("Debug", "Activer le Debug complet (True/False)", False)
            '        Add_ParamAvance("AfficheLog", "Afficher Log OpenZwave à l'écran (True/False)", True)
            '        Add_ParamAvance("StartIdleTime", "Durée durant laquelle le driver ne traite aucun message lors de son démarrage (en secondes).", 10)

            'ajout des commandes avancées pour les devices
            Add_DeviceCommande("ALL_LIGHT_ON", "", 0)
            Add_DeviceCommande("ALL_LIGHT_OFF", "", 0)

            'Libellé Driver
            Add_LibelleDriver("HELP", "Aide...", "Ce module permet de recuperer les informations delivrées par un contrôleur Z-Wave ")

            'Libellé Device
            Add_LibelleDevice("ADRESSE1", "Protocole", "Protocole utilisé")
            Add_LibelleDevice("ADRESSE2", "Label de la donnée:Index", "'Temperature', 'Relative Humidity', 'Battery Level' suivi de l'index (si necessaire)")
            Add_LibelleDevice("SOLO", "@", "")
            Add_LibelleDevice("MODELE", "@", "")

        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " New", ex.Message)
        End Try
    End Sub

    ''' <summary>Si refresh >0 gestion du timer</summary>
    ''' <remarks>PAS UTILISE CAR IL FAUT LANCER UN TIMER QUI LANCE/ARRETE CETTE FONCTION dans Start/Stop</remarks>
    Private Sub TimerTick(ByVal source As Object, ByVal e As System.Timers.ElapsedEventArgs)

    End Sub

#End Region

#Region "Fonctions internes"

    '-----------------------------------------------------------------------------
    ''' <summary>Ouvrir le port RFPlayer</summary>
    ''' <param name="numero">Nom/Numero du port COM: COM2</param>
    ''' <remarks></remarks>
    Private Function ouvrir(ByVal numero As String) As String
        Try
            Try
                'ouverture du port
                If Not _IsConnect Then
                    Try
                        ' Test d'ouveture du port Com du controleur 
                        port.PortName = numero
                        port.BaudRate = _baudspeed
                        port.DataBits = _nbrebit
                        port.Parity = _parity
                        port.StopBits = _nbrebitstop
                        WriteLog("Ouvrir - Ouverture du port " & port.PortName & " à la vitesse " & port.BaudRate & port.DataBits & port.Parity.ToString & port.StopBits.ToString)
                        port.Open()
                        ' Le port existe ==> le controleur est present
                        If port IsNot Nothing AndAlso port.IsOpen() Then
                            _IsConnect = True
                            Return ("Le port " & port_name & " ouvert")
                        Else
                            ' Le port n'existe pas ==> le controleur n'est pas present
                            _IsConnect = False
                            Return ("Le port " & port_name & " fermé")
                        End If
                    Catch ex As Exception
                        _IsConnect = False
                        Return ("Le port " & port_name & " n'existe pas")
                        Exit Function
                    End Try
                Else
                    Return ("Le port " & port_name & " dejà ouvert")
                End If
            Catch ex As Exception
                _IsConnect = False
                Return ("ERR: " & ex.Message)
            End Try
        Catch ex As Exception
            _IsConnect = False
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Place le controller en mode "inclusion"
    ''' </summary>
    ''' <remarks></remarks>
    Sub StartInclusionMode(Optional ByVal NumProtocole As String = Nothing, Optional ByVal iddevice As String = Nothing)
        Select Case NumProtocole
            Case "1" 'Frame Protocol 1		X10, infotype 0, 1
            Case "2" 'Frame Protocol 2		VISONIC, infotype 2
            Case "3" 'Frame Protocol 3		BLYSS, infotype 1
            Case "4" 'Frame Protocol 4		CHACON, infotype 1
            Case "5" 'Frame Protocol 5		Oregon, infotype 4, 5, 6, 7, 9
            Case "6" 'Frame Protocol 6		DOMIA, infotype 0
            Case "7" 'Frame Protocol 7		OWL, infotype 8
            Case "8" 'Frame Protocol 8		X2D, infotype 10, 11
            Case "9" 'Frame Protocol 9		RTS, infotype 3
                WriteInfoType3("ZIA++ASSOC " & NumProtocole & " " & iddevice)
            Case "10" 'Frame Protocol 10	KD101, infotype 1
            Case "11" 'Frame Protocol 11   PARROT, infotype 0
        End Select

    End Sub

    Private Sub SendToSerial(frame As String, timeout As Integer)
        Try
            Dim utf8Encoding As New System.Text.UTF8Encoding
            Dim encodedString() As Byte
            encodedString = utf8Encoding.GetBytes(frame & vbCrLf)
            FrameCompleted = False
            port.Write(utf8Encoding.GetString(encodedString))
            WriteLog("DBG: SendToSerial, " & frame)
            Dim i As Integer = 0
            While (timeout >= i)
                System.Threading.Thread.Sleep(1000 * i)
                i = i + 1
                If FrameCompleted Then Exit While
            End While

        Catch ex As Exception
            WriteLog("ERR: SendToSerial Exception: " + ex.Message)
        End Try
        System.Threading.Thread.Sleep(100)
    End Sub

    Private Sub ReceiveFromSerial(sender As Object, e As SerialDataReceivedEventArgs)
        Try
            Dim sp As SerialPort = CType(sender, SerialPort)
            Dim _libelleadr2 As String = ""

            While (sp.BytesToRead > 0)
                FrameStr = FrameStr & port.ReadExisting
            End While

            ' si trame entiere on traite
            If InStr(FrameStr, "}]}}") > 0 Then
                Select Case True
                    Case InStr(FrameStr, "ZIA--{") > 0
                        ReadConf(FrameStr.Replace("ZIA--", ""))
                        '   Case InStr(FrameStr, "ZIA33{") > 0
                        FrameStr = "ZIA33{ ""frame"" :{""header"": {""frameType"": ""0"", ""cluster"": ""0"", ""dataFlag"": ""0"", ""rfLevel"": ""-64"", ""floorNoise"": ""-103"", ""rfQuality"": ""9"", ""protocol"": ""9"", ""protocolMeaning"": ""RTS"", ""infoType"": ""3"", ""frequency"": ""433920""},""infos"": {""subType"": ""0"", ""subTypeMeaning"": ""Shutter"", ""id"": ""A1"", ""qualifier"": ""4"", ""qualifierMeaning"": { ""flags"": [""My""]}}}}"
                        ReadDatas(FrameStr.Replace("ZIA33", ""))
                        '                FrameStr = "ZIA33{ ""frame"" :{""header"": {""frameType"": ""0"", ""cluster"": ""0"", ""dataFlag"": ""0"", ""rfLevel"": ""-64"", ""floorNoise"": ""-103"", ""rfQuality"": ""9"", ""protocol"": ""9"", ""protocolMeaning"": ""RTS"", ""infoType"": ""3"", ""frequency"": ""433920""},""infos"": {""subType"": ""1"", ""subTypeMeaning"": ""Portal"", ""id"": ""14813215"", ""qualifier"": ""4"", ""qualifierMeaning"": { ""flags"": [""Portail""]}}}}"
                        '               ReadDatas(FrameStr.Replace("ZIA33", ""))
                        For i As Integer = 0 To ListOfDevices.Count - 1
                            _libelleadr2 += ListOfDevices.Item(i).protocol & " #; " & ListOfDevices.Item(i).id & "|"
                        Next
                        ' evite les doublons 
                        Dim ld0 As New HoMIDom.HoMIDom.Driver.cLabels
                        For i As Integer = 0 To _LabelsDevice.Count - 1
                            ld0 = _LabelsDevice(i)
                            Select Case ld0.NomChamp
                                Case "ADRESSE2"
                                    _libelleadr2 = Mid(_libelleadr2, 1, Len(_libelleadr2) - 1) 'enleve le dernier | pour eviter davoir une ligne vide a la fin
                                    ld0.Parametre = _libelleadr2
                                    _LabelsDevice(i) = ld0
                            End Select
                        Next
                    Case Else
                        WriteLog("DBG: ReceiveFromSerial, framestr: " + FrameStr)
                End Select
                'efface variables reponse une fois traitée
                FrameStr = ""
                FrameCompleted = True
            End If
        Catch ex As Exception
            WriteLog("ERR: ReceiveFromSerial Exception: " + ex.Message)
        End Try
    End Sub
    Private Sub ReadConf(str As String)
        Try
            WriteLog("DBG: ReadConf à traiter : " + str)
            jsonObjConf = Newtonsoft.Json.JsonConvert.DeserializeObject(str)
            Dim st As JProperty
            Dim _libelleadr1 As String = ""

            Select Case True
                Case InStr(str, "systemStatus") > 0
                    For i As Integer = 0 To 9
                        Dim info As infosystem = New infosystem
                        For Each st In jsonObjConf("systemStatus")("info")(i)
                            Select Case True
                                Case (st.Name.ToString = "n")
                                    info.n = st.Value.ToString
                                Case (st.Name.ToString = "v")
                                    info.v = st.Value.ToString
                                Case (st.Name.ToString = "unit")
                                    info.unit = st.Value.ToString
                                Case (st.Name.ToString = "c")
                                    info.c = st.Value.ToString
                            End Select
                        Next
                        If (info.n = "Version") Or (info.n = "Mac") Then
                            ListInfoSystemStatus.Add(info)
                            WriteLog("DBG: ReadConf Enregistrement: " & ListInfoSystemStatus.Item(ListInfoSystemStatus.Count - 1).n & " -> " & ListInfoSystemStatus.Item(ListInfoSystemStatus.Count - 1).v)
                        End If
                    Next
                    For Each st In jsonObjConf("systemStatus")("info")(10)("transmitter")("available")
                        TransmitterAvailable.Clear()
                        For Each t As String In st.Value
                            TransmitterAvailable.Add(t)
                            Dim ptc As String = ""
                            Select Case True
                                Case InStr(t, "X10") 'Frame Protocol 1		X10, infotype 0, 1
                                    ptc = "1 # X10|"
                                Case InStr(t, "VISONIC") 'Frame Protocol 2		VISONIC, infotype 2
                                    ptc = "2 # VISONIC|"
                                Case InStr(t, "BLYSS") 'Frame Protocol 3		BLYSS, infotype 1
                                    ptc = "3 # BLYSS|"
                                Case InStr(t, "CHACON")  'Frame Protocol 4		CHACON, infotype 1
                                    ptc = "4 # CHACON|"
                                Case InStr(t, "OREGON")  'Frame Protocol 5		Oregon, infotype 4, 5, 6, 7, 9
                                    ptc = "5 # OREGON|"
                                Case InStr(t, "DOMIA")  'Frame Protocol 6		DOMIA, infotype 0
                                    ptc = "6 # DOMIA|"
                                Case InStr(t, "OWL")  'Frame Protocol 7		OWL, infotype 8
                                    ptc = "7 # OWL|"
                                Case InStr(t, "X2D")  'Frame Protocol 8		X2D, infotype 10, 11
                                    ptc = "8 # X2D|"
                                Case InStr(t, "RTS")  'Frame Protocol 9		RTS, infotype 3
                                    ptc = "9 # RTS|"
                                Case InStr(t, "KD101")  'Frame Protocol 10	KD101, infotype 1
                                    ptc = "10 # KD101|"
                                Case InStr(t, "PARROT")  'Frame Protocol 11   PARROT, infotype 0
                                    ptc = "11 # PARROT|"
                            End Select
                            If (ptc <> "") And (InStr(_libelleadr1, ptc) = 0) Then _libelleadr1 += ptc
                        Next
                        WriteLog("ReadConf Transmetteur Available " & TransmitterAvailable.Count.ToString())
                    Next
                    For Each st In jsonObjConf("systemStatus")("info")(11)("receiver")("available")
                        ReceiverAvailable.Clear()
                        For Each t As String In st.Value
                            ReceiverAvailable.Add(t)
                        Next
                        WriteLog("DBG: ReadConf Recepteur Available " & ReceiverAvailable.Count.ToString())
                    Next
                    For Each st In jsonObjConf("systemStatus")("info")(12)("receiver")("enabled")
                        ReceiverEnable.Clear()
                        For Each t As String In st.Value
                            ReceiverEnable.Add(t)
                        Next
                        WriteLog("DBG: ReadConf Recepteur Enable " & ReceiverEnable.Count.ToString())
                    Next
                    For Each st In jsonObjConf("systemStatus")("info")(13)("repeater")("available")
                        RepeaterAvailable.Clear()
                        For Each t As String In st.Value
                            RepeaterAvailable.Add(t)
                        Next
                        WriteLog("DBG: ReadConf Repeteur Available " & RepeaterAvailable.Count.ToString())
                    Next
                    For Each st In jsonObjConf("systemStatus")("info")(14)("repeater")("enabled")
                        RepeaterEnable.Clear()
                        For Each t As String In st.Value
                            RepeaterEnable.Add(t)
                        Next
                        WriteLog("DBG: ReadConf Repeteur Enable " & RepeaterEnable.Count.ToString())
                    Next
                    ' evite les doublons 
                    Dim ld0 As New HoMIDom.HoMIDom.Driver.cLabels
                    For i As Integer = 0 To _LabelsDevice.Count - 1
                        ld0 = _LabelsDevice(i)
                        Select Case ld0.NomChamp
                            Case "ADRESSE1"
                                _libelleadr1 = Mid(_libelleadr1, 1, Len(_libelleadr1) - 1) 'enleve le dernier | pour eviter davoir une ligne vide a la fin
                                ld0.Parametre = _libelleadr1
                                _LabelsDevice(i) = ld0
                                'Case "ADRESSE2"
                                '    _libelleadr2 = Mid(_libelleadr2, 1, Len(_libelleadr2) - 1) 'enleve le dernier | pour eviter davoir une ligne vide a la fin
                                '    ld0.Parametre = _libelleadr2
                                '    _LabelsDevice(i) = ld0
                        End Select
                    Next
                Case InStr(str, "radioStatus") > 0
            End Select



        Catch ex As Exception
            WriteLog("ERR: ReadConf Exception: " + ex.Message)
        End Try
    End Sub

    Private Sub ReadDatas(str As String)
        Try
            WriteLog("DBG: ReadDatas à traiter : " + str)
            jsonObjConf = Newtonsoft.Json.JsonConvert.DeserializeObject(str)

            Dim st As String = jsonObjConf("frame")("header")("infoType")
            WriteLog("DBG: infoType: " & st)

            Select Case st
                Case "0" 'Frame infoType 0		ON/OFF
                Case "1" 'Frame infoType 1		ON/OFF   error in API receive id instead of id_lsb and id_msb
                Case "2" 'Frame infoType 2		Visonic
                Case "3" 'Frame infoType 3		RTS
                    DecodeInfoType3(jsonObjConf, st)
                Case "4" 'Frame infoType 4		Oregon thermo/hygro sensors
                Case "5" 'Frame infoType 5		Oregon thermo/hygro/pressure sensors
                Case "6" 'Frame infoType 6		Oregon Wind sensors
                Case "7" 'Frame infoType 7		Oregon UV sensors
                Case "8" 'Frame infoType 8		OWL Energy/power sensors
                Case "9" 'Frame infoType 9		Oregon Rain sensors
                Case "10" 'Frame infoType 10	Thermostats  X2D protocol
                Case "11" 'Frame infoType 11   Alarm X2D protocol / Shutter
            End Select



        Catch ex As Exception
            WriteLog("ERR: ReadDatas Exception: " + ex.Message)
        End Try
    End Sub

    Private Sub DecodeInfoType0(DecData, infoType)
        Try
            Dim protocol As String = DecData("frame")("header")("protocol")
            Dim subType As String = DecData("frame")("infos")("subType")
            Dim subTypeMeaning As String = DecData("frame")("infos")("subTypeMeaning")
            Dim id As String = DecData("frame")("infos")("id")
            Dim qualifier As String = DecData("frame")("infos")("qualifier")
            Dim qualifierMeaning As String = DecData("frame")("infos")("qualifierMeaning")("flags")(0)

            Dim dev As device = New device
            dev.id = id
            dev.name = qualifierMeaning
            dev.protocol = Trim(protocol)
            dev.subTypeMeaning = subTypeMeaning
            dev.qualifier = qualifier
            If (ListOfDevices.IndexOf(dev) = -1) Then
                ListOfDevices.Add(dev)
                WriteLog("DBG: " & Trim(dev.protocol) & " #; " & subTypeMeaning & " " & dev.id & "|")
            End If
        Catch ex As Exception
            WriteLog("ERR: DecodeInfoType0 Exception: " + ex.Message)
        End Try
    End Sub
    Private Sub WriteInfoType0(command)
        Try
            SendToSerial(command, 3)
            WriteLog("DBG: WriteInfoType0, commande " & command & " exécutée")
        Catch ex As Exception
            WriteLog("ERR: WriteInfoType0 Exception: " + ex.Message)
            '   Return ""
        End Try
    End Sub
    Private Sub DecodeInfoType2(DecData, infoType)
        Try
            Dim protocol As String = DecData("frame")("header")("protocol")
            Dim subType As String = DecData("frame")("infos")("subType")
            Dim subTypeMeaning As String = DecData("frame")("infos")("subTypeMeaning")
            Dim id As String = DecData("frame")("infos")("id")
            Dim qualifier As String = DecData("frame")("infos")("qualifier")
            Dim qualifierMeaning As String = DecData("frame")("infos")("qualifierMeaning")("flags")(0)

            Dim dev As device = New device
            dev.id = id
            dev.name = qualifierMeaning
            dev.protocol = Trim(protocol)
            dev.subTypeMeaning = subTypeMeaning
            dev.qualifier = qualifier
            If (ListOfDevices.IndexOf(dev) = -1) Then
                ListOfDevices.Add(dev)
                WriteLog("DBG: " & Trim(dev.protocol) & " #; " & subTypeMeaning & " " & dev.id & "|")
            End If
        Catch ex As Exception
            WriteLog("ERR: DecodeInfoType2 Exception: " + ex.Message)
        End Try
    End Sub
    Private Sub WriteInfoType2(command)
        Try
            SendToSerial(command, 3)
            WriteLog("DBG: WriteInfoType2, commande " & command & " exécutée")
        Catch ex As Exception
            WriteLog("ERR: WriteInfoType2 Exception: " + ex.Message)
            '   Return ""
        End Try
    End Sub
    Private Sub DecodeInfoType3(DecData, infoType)
        Try
            Dim protocol As String = DecData("frame")("header")("protocol")
            Dim subType As String = DecData("frame")("infos")("subType")
            Dim subTypeMeaning As String = DecData("frame")("infos")("subTypeMeaning")
            Dim id As String = DecData("frame")("infos")("id")
            Dim qualifier As String = DecData("frame")("infos")("qualifier")
            Dim qualifierMeaning As String = DecData("frame")("infos")("qualifierMeaning")("flags")(0)

            Dim dev As device = New device
            dev.id = id
            dev.name = qualifierMeaning
            dev.protocol = Trim(protocol)
            dev.subTypeMeaning = subTypeMeaning
            dev.qualifier = qualifier
            If (ListOfDevices.IndexOf(dev) = -1) Then
                ListOfDevices.Add(dev)
                WriteLog("DBG: " & Trim(dev.protocol) & " #; " & subTypeMeaning & " " & dev.id & "|")
            End If
        Catch ex As Exception
            WriteLog("ERR: DecodeInfoType3 Exception: " + ex.Message)
        End Try
    End Sub
    Private Sub WriteInfoType3(command)
        Try
            SendToSerial(command, 3)
            WriteLog("DBG: WriteInfoType3, commande " & command & " exécutée")
        Catch ex As Exception
            WriteLog("ERR: WriteInfoType3 Exception: " + ex.Message)
            '   Return ""
        End Try
    End Sub
    Private Sub WriteLog(message)
        Try
            'utilise la fonction de base pour loguer un event
            If STRGS.InStr(message, "DBG:") > 0 Then
                If _DEBUG Then
                    _Server.Log(TypeLog.DEBUG, TypeSource.DRIVER, Me.Nom, STRGS.Right(message, message.Length - 5))
                End If
            ElseIf STRGS.InStr(message, "ERR:") > 0 Then
                _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom, STRGS.Right(message, message.Length - 5))
            Else
                _Server.Log(TypeLog.INFO, TypeSource.DRIVER, Me.Nom, message)
            End If
        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " WriteLog", ex.Message)
        End Try
    End Sub


#End Region

End Class

'###########
'	#infotype0  ==> ok
'	#ReqRcv = 'ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-44", "floorNoise": "-99", "rfQuality": "10", "protocol": "6", "protocolMeaning": "DOMIA", "infoType": "0", "frequency": "433920"},"infos": {"subType": "0", "id": "235", "subTypeMeaning": "OFF", "idMeaning": "O12"}}}'
'###########
'	#infotype1 ==> ok
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-72", "floorNoise": "-106", "rfQuality": "8", "protocol": "4", "protocolMeaning": "CHACON", "infoType": "1", "frequency": "433920"},"infos": {"subType": "1", "id": "424539265", "subTypeMeaning": "ON"}}}'
'###########
'	#infotype2
'	#==> ok
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-51", "floorNoise": "-103", "rfQuality": "10", "protocol": "2", "protocolMeaning": "VISONIC", "infoType": "2", "frequency": "433920"},"infos": {"subType": "0", "subTypeMeaning": "Detector/Sensor", "id": "335547184", "qualifier": "3", "qualifierMeaning": { "flags": ["Tamper","Alarm"]}}}}'
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-55", "floorNoise": "-102", "rfQuality": "10", "protocol": "2", "protocolMeaning": "VISONIC", "infoType": "2", "frequency": "433920"},"infos": {"subType": "0", "subTypeMeaning": "Detector/Sensor", "id": "2034024048", "qualifier": "1", "qualifierMeaning": { "flags": ["Tamper"]}}}}'
'	#OK ==>  protocol = 3
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-66", "floorNoise": "-106", "rfQuality": "10", "protocol": "3", "protocolMeaning": "BLYSS", "infoType": "2", "frequency": "433920"},"infos": {"subType": "0", "subTypeMeaning": "Detector/Sensor", "id": "256292321", "qualifier": "0"}}}'
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "1", "rfLevel": "-84", "floorNoise": "-106", "rfQuality": "5", "protocol": "2", "protocolMeaning": "VISONIC", "infoType": "2", "frequency": "868950"},"infos": {"subType": "0", "subTypeMeaning": "Detector/Sensor", "id": "2039708784", "qualifier": "0", "qualifierMeaning": { "flags": []}}}}'
'	###########
'	#infotype3 RTS Subtype0 ==> ok  // 
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-64", "floorNoise": "-103", "rfQuality": "9", "protocol": "9", "protocolMeaning": "RTS", "infoType": "3", "frequency": "433920"},"infos": {"subType": "0", "subTypeMeaning": "Shutter", "id": "14813191", "qualifier": "4", "qualifierMeaning": { "flags": ["My"]}}}}'
'###########
'	#infotype4
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-86", "floorNoise": "-100", "rfQuality": "3", "protocol": "5", "protocolMeaning": "OREGON", "infoType": "4", "frequency": "433920"},"infos": {"subType": "0", "id_PHY": "0xEA4C", "id_PHYMeaning": "THC238/268,THWR288,THRN122,THN122/132,AW129/131", "adr_channel": "21762", "adr": "85", "channel": "2", "qualifier": "33", "lowBatt": "1", "measures" : [{"type" : "temperature", "value" : "-17.8", "unit" : "Celsius"}, {"type" : "hygrometry", "value" : "0", "unit" : "%"}]}}}'
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-46", "floorNoise": "-105", "rfQuality": "10", "protocol": "5", "protocolMeaning": "OREGON", "infoType": "4", "frequency": "433920"},"infos": {"subType": "0", "id_PHY": "0x1A2D", "id_PHYMeaning": "THGR122/228/238/268,THGN122/123/132", "adr_channel": "63492", "adr": "248", "channel": "4", "qualifier": "32", "lowBatt": "0", "measures" : [{"type" : "temperature", "value" : "+20.3", "unit" : "Celsius"}, {"type" : "hygrometry", "value" : "41", "unit" : "%"}]}}}'
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-77", "floorNoise": "-100", "rfQuality": "5", "protocol": "5", "protocolMeaning": "OREGON", "infoType": "4", "frequency": "433920"},"infos": {"subType": "0", "id_PHY": "0xFA28", "id_PHYMeaning": "THGR810", "adr_channel": "64513", "adr": "252", "channel": "1", "qualifier": "48", "lowBatt": "0", "measures" : [{"type" : "temperature", "value" : "+21.0", "unit" : "Celsius"}, {"type" : "hygrometry", "value" : "35", "unit" : "%"}]}}}'
'###########
'	#infotype5
'###########
'	#infotype6
'###########
'	#infotype7
'###########
'	#infotype8 OWL ==> ok
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "0", "rfLevel": "-85", "floorNoise": "-97", "rfQuality": "3", "protocol": "7", "protocolMeaning": "OWL", "infoType": "8", "frequency": "433920"},"infos": {"subType": "0", "id_PHY": "0x0002", "id_PHYMeaning": "CM180", "adr_channel": "35216",  "adr": "2201",  "channel": "0",  "qualifier": "1",  "lowBatt": "1", "measures" : [{"type" : "energy", "value" : "871295", "unit" : "Wh"}, {"type" : "power", "value" : "499", "unit" : "W"}]}}}'
'###########
'	#infotype9
'###########
'	#infotype10
'###########
'	#infotype11 ==> ok
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "1", "rfLevel": "-75", "floorNoise": "-99", "rfQuality": "6", "protocol": "8", "protocolMeaning": "X2D", "infoType": "11", "frequency": "868350"},"infos": {"subType": "0", "subTypeMeaning": "Detector/Sensor", "id": "2888689920", "qualifier": "10", "qualifierMeaning": { "flags": ["Alarm","Supervisor/Alive"]}}}}'
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "1", "rfLevel": "-57", "floorNoise": "-106", "rfQuality": "10", "protocol": "8", "protocolMeaning": "X2D", "infoType": "11", "frequency": "868350"},"infos": {"subType": "0", "subTypeMeaning": "Detector/Sensor", "id": "1112729857", "qualifier": "2", "qualifierMeaning": { "flags": ["Alarm"]}}}}'
'	#ReqRcv='ZIA33{ "frame" :{"header": {"frameType": "0", "cluster": "0", "dataFlag": "1", "rfLevel": "-57", "floorNoise": "-106", "rfQuality": "10", "protocol": "8", "protocolMeaning": "X2D", "infoType": "11", "frequency": "868350"},"infos": {"subType": "0", "subTypeMeaning": "Detector/Sensor", "id": "1112729857", "qualifier": "0", "qualifierMeaning": { "flags": []}}}}'
'	###########


