Imports HoMIDom
Imports HoMIDom.HoMIDom
Imports HoMIDom.HoMIDom.Server
Imports HoMIDom.HoMIDom.Device

Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports STRGS = Microsoft.VisualBasic.Strings


' Auteur : jphomi 
' Date : 01/11/2015

''' <summary>Driver DLink_DSG1100x pour piloter switch poe</summary>
''' <remarks></remarks>
<Serializable()> Public Class Driver_DLink_DSG1100x
    Implements HoMIDom.HoMIDom.IDriver

#Region "Variables génériques"
    '!!!Attention les variables ci-dessous doivent avoir une valeur par défaut obligatoirement
    'aller sur l'adresse http://www.somacon.com/p113.php pour avoir un ID
    Dim _ID As String = "BEE0C28A-7683-11E5-ADA6-D3A51D5D46B0"
    Dim _Nom As String = "DLink_DSG1100x"
    Dim _Enable As Boolean = False
    Dim _Description As String = "Switch DLink_DSG1100x"
    Dim _StartAuto As Boolean = False
    Dim _Protocol As String = "WEB"
    Dim _IsConnect As Boolean = False
    Dim _IP_TCP As String = "@"
    Dim _Port_TCP As String = "@"
    Dim _IP_UDP As String = "@"
    Dim _Port_UDP As String = "@"
    Dim _Com As String = "@"
    Dim _Refresh As Integer = 120
    Dim _Modele As String = "DLink_DSG1100x"
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

    'A ajouter dans les ppt du driver
    Dim _tempsentrereponse As Integer = 1500
    Dim _ignoreadresse As Boolean = False
    Dim _lastetat As Boolean = True

#End Region

#Region "Variables internes"

    'param avancé
    Dim _DEBUG As Boolean = False
    Dim _IPAdress As String = ""
    Dim _Password As String = ""
    Dim moncook As String = ""
    Dim sessid As String = ""
    Dim vlogiciel As String = ""

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
            _Refresh = value
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
                    'Write(deviceobject, Command, Param(0), Param(1))
                    Select Case UCase(Command)
                        Case ""
                        Case Else
                    End Select
                    Return True
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
                Case "ADRESSE1"
                    If Value IsNot Nothing Then
                        If String.IsNullOrEmpty(Value) Or IsNumeric(Value) Then
                            retour = "Veuillez saisir le numero de(s) port(s) ( 2-3 )"
                        End If
                    End If
                Case "ADRESSE2"
                    If Value IsNot Nothing Then
                        If String.IsNullOrEmpty(Value) Or IsNumeric(Value) Then
                            retour = "Veuillez saisir le nom de la commande en respectant la casse ( state=1, Priority=1 )"
                        End If
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
        Try
            'récupération des paramétres avancés
            Try
                _DEBUG = _Parametres.Item(0).Valeur
                _IPAdress = _Parametres.Item(1).Valeur
                _Password = _Parametres.Item(2).Valeur

            Catch ex As Exception
                _DEBUG = False
                _Parametres.Item(0).Valeur = False
                WriteLog("ERR: Erreur dans les paramétres avancés. utilisation des valeur par défaut : " & ex.Message)
            End Try

            'lance le time du driver, mini toutes les 2 minutes
            If _Refresh = 0 Then _Refresh = 120
            MyTimer.Interval = _Refresh * 1000
            MyTimer.Enabled = True
            AddHandler MyTimer.Elapsed, AddressOf TimerTick

            _IsConnect = Connect(_IPAdress, _Password)
            If _IsConnect Then
                WriteLog("Matériel " & Me.Nom & " trouvé")
            Else
                WriteLog("Matériel " & Me.Nom & " non trouvé")
            End If

        Catch ex As Exception
            _IsConnect = False
            WriteLog("ERR: START Driver " & Me.Nom & " Erreur démarrage " & ex.Message)
        End Try
    End Sub

    ''' <summary>Arrêter le du driver</summary>
    ''' <remarks></remarks>
    Public Sub [Stop]() Implements HoMIDom.HoMIDom.IDriver.Stop
        Try
            _IsConnect = False
            MyTimer.Enabled = False
            WriteLog("Driver " & Me.Nom & " arrêté")
        Catch ex As Exception
            WriteLog("ERR: STOP Driver " & Me.Nom & " Erreur arrêt " & ex.Message)
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

            If _Enable = False Then Exit Sub

            If _IsConnect = False Then
                WriteLog("ERR: READ, Le driver n'est pas démarré, impossible d'écrire sur le port")
                Exit Sub
            End If
            Try
                Select Case Objet.Type
                    Case "GENERIQUESTRING"
                        Dim paramreq As String
                        paramreq = "port_f=" & Mid(Objet.Adresse1, 1, 1) & "&port_t=" & Mid(Objet.Adresse1, Len(Objet.Adresse1), 1) & "&" & Objet.Adresse2
                        ' Datas("http://" & _IPAdress & "/cgi/port.cgi", moncook, sessid, paramreq)
                        Objet.Value = Datas("http://" & _IPAdress & "/cgi/poe_port.cgi", moncook, sessid, paramreq)
                    Case Else
                        WriteLog("ERR: WRITE Erreur Write Type de composant non géré ")
                End Select
            Catch ex As Exception
                WriteLog("ERR: Write, Exception : " & ex.Message)
            End Try

        Catch ex As Exception
            WriteLog("ERR: READ, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Commander un device</summary>
    ''' <param name="Objet">Objet représentant le device à interroger</param>
    ''' <param name="Command">La commande à passer</param>
    ''' <param name="Parametre1"></param>
    ''' <param name="Parametre2"></param>
    ''' <remarks></remarks>
    Public Sub Write(ByVal Objet As Object, ByVal Command As String, Optional ByVal Parametre1 As Object = Nothing, Optional ByVal Parametre2 As Object = Nothing) Implements HoMIDom.HoMIDom.IDriver.Write
        Try
            If _Enable = False Then Exit Sub

            If _IsConnect = False Then
                WriteLog("ERR: READ, Le driver n'est pas démarré, impossible d'écrire sur le port")
                Exit Sub
            End If

            WriteLog("DBG: WRITE Device " & Objet.Name & " --> " & Command)
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
            WriteLog("ERR: add_devicecommande, Exception :" & ex.Message)
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
            WriteLog("ERR: add_devicecommande, Exception : " & ex.Message)
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
            WriteLog("ERR: add_devicecommande, Exception : " & ex.Message)
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
            WriteLog("ERR: add_devicecommande, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Creation d'un objet de type</summary>
    ''' <remarks></remarks>
    Public Sub New()
        Try
            _Version = Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString

            'liste des devices compatibles
            _DeviceSupport.Add(ListeDevices.ENERGIEINSTANTANEE)
            _DeviceSupport.Add(ListeDevices.GENERIQUEVALUE)
            _DeviceSupport.Add(ListeDevices.GENERIQUESTRING)

            'Parametres avancés
            Add_ParamAvance("Debug", "Activer le Debug complet (True/False)", False)
            Add_ParamAvance("IPPort", "Adresse IP par défaut", "192.168.0.0")
            Add_ParamAvance("Mot de passe", "Mot de passe par défaut", "Homidom")

            'Libellé Driver
            Add_LibelleDriver("HELP", "Aide...", "Pas d'aide actuellement...")

            'Libellé Device
            Add_LibelleDevice("ADRESSE1", "Num port switch", "ex : 2-2, 2-3", "")
            Add_LibelleDevice("ADRESSE2", "Nom de la commande", "ex : state=1, Powerlimit=10", "")
            ' Libellés Device inutiles
            Add_LibelleDevice("REFRESH", "@", "")
            Add_LibelleDevice("SOLO", "@", "")
            Add_LibelleDevice("MODELE", "@", "")
            Add_LibelleDevice("LASTCHANGEDUREE", "@", "")
        Catch ex As Exception
            WriteLog("ERR: New, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Si refresh >0 gestion du timer</summary>
    ''' <remarks>PAS UTILISE CAR IL FAUT LANCER UN TIMER QUI LANCE/ARRETE CETTE FONCTION dans Start/Stop</remarks>
    Private Sub TimerTick(ByVal source As Object, ByVal e As System.Timers.ElapsedEventArgs)
        ' Attente de 3s pour eviter le relancement de la procedure dans le laps de temps
        'System.Threading.Thread.Sleep(3000)
        'ScanData()
        _IsConnect = Connect(_IPAdress, _Password)
        If _IsConnect Then
            WriteLog("Matériel " & Me.Nom & " trouvé")
        Else
            WriteLog("Matériel " & Me.Nom & " non trouvé")
        End If
    End Sub

#End Region

#Region "Fonctions internes"

    Private Function Connect(ipadres As String, password As String)

        Dim reqparam As String = ""
        Dim responsebodystr As String = ""

        Try
            ' connection au site 
            Dim url As String = "http://" & _IPAdress & "/login.htm"
            Dim postBytes As Byte() = Encoding.ASCII.GetBytes(reqparam)
            Dim Request As HttpWebRequest = HttpWebRequest.Create(url)
            Request.ContentType = "text/html; charset=utf-8"
            Request.Method = "POST"
            Request.AllowAutoRedirect = False
            Request.ProtocolVersion = HttpVersion.Version10
            Request.KeepAlive = True
            Request.ContentLength = reqparam.Length
            Request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0"
            Request.CookieContainer = New CookieContainer
            Request.Referer = "http://" & _IPAdress
            Request.Host = _IPAdress
            Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            Request.Timeout = 5000

            Dim requestStream As Stream = Request.GetRequestStream()
            requestStream.Write(postBytes, 0, postBytes.Length)
            requestStream.Close()
            WriteLog("DBG: Connect, Url " & url)

            ' on passe le password par la fenetre login
            reqparam = "pass=" & _Password
            url = "http://" & _IPAdress & "/cgi/login.cgi"
            postBytes = Encoding.ASCII.GetBytes(reqparam)
            Request = HttpWebRequest.Create(url)
            Request.ContentType = "text/html; charset=utf-8"
            Request.Method = "POST"
            Request.AllowAutoRedirect = False
            Request.ProtocolVersion = HttpVersion.Version10
            Request.KeepAlive = True
            Request.ContentLength = reqparam.Length
            Request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0"
            Request.CookieContainer = New CookieContainer
            Request.Referer = "http://" & _IPAdress & "/login.htm"
            Request.Host = _IPAdress
            Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            Request.Timeout = 5000

            requestStream = Request.GetRequestStream()
            requestStream.Write(postBytes, 0, postBytes.Length)
            requestStream.Close()

            Dim Response As HttpWebResponse = Request.GetResponse()
            WriteLog("DBG: Connect, response headers.StatusDescription : " & Response.StatusDescription.ToString)

            If Response.StatusDescription.ToString <> "OK" Then
                WriteLog("DBG: Connect, site inaccesible " & url)
                Return False
            End If

            Dim responsereader = New StreamReader(Response.GetResponseStream())
            responsebodystr = responsereader.ReadToEnd()
            responsereader.Close()

            '            on recupère le cookie et sessid
            Dim tmpstr As String = responsebodystr
            tmpstr = Mid(tmpstr, InStr(tmpstr, "SessID="), Len(tmpstr))
            sessid = Mid(tmpstr, 8, InStr(tmpstr, ";") - 8)
            tmpstr = responsebodystr
            tmpstr = Mid(tmpstr, InStr(tmpstr, "Gambit="), Len(tmpstr))
            moncook = Mid(tmpstr, 8, InStr(tmpstr, ";") - 8)

            WriteLog("DBG: Connect, sessID : " & sessid & ", cookie : " & moncook)
            Return True

        Catch ex As Exception
            WriteLog("ERR: Connect, Exception : " & ex.Message)
            Return False
        End Try

    End Function

    Private Function Datas(url As String, moncookie As String, monphpid As String, paramreq As String)

        Try
            WriteLog("DBG: Datas, url " & url)
            Dim postBytes As Byte() = Encoding.ASCII.GetBytes(paramreq)
            Dim Request As HttpWebRequest = HttpWebRequest.Create(url)
            Request.ContentType = "application/x-www-form-urlencoded"
            Request.Method = "POST"
            Request.AllowAutoRedirect = False
            Request.ProtocolVersion = HttpVersion.Version10
            Request.KeepAlive = True
            Request.ContentLength = paramreq.Length
            Request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0"
            Request.CookieContainer = New CookieContainer
            Request.CookieContainer.Add(New Uri(url), New Cookie("Gambit", moncookie))
            Request.CookieContainer.Add(New Uri(url), New Cookie("SessID", monphpid))
            Request.Referer = "http://" & _IPAdress & "/login.htm"
            Request.Host = _IPAdress
            Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            Request.Timeout = 5000

            Dim requestStream As Stream = Request.GetRequestStream()
            requestStream.Write(postBytes, 0, postBytes.Length)
            requestStream.Close()
            Dim Response As HttpWebResponse = Request.GetResponse()

            If Response.StatusDescription.ToString <> "OK" Then
                WriteLog("DBG: Connect, site inaccesible " & url)
                Datas = ""
            End If

            Dim responsereader = New StreamReader(Response.GetResponseStream())
            WriteLog("DBG: Datas, reponsereader " & Replace(responsereader.ReadToEnd(), vbCrLf, ""))
            responsereader.Close()
            Datas = Response.StatusDescription.ToString

        Catch ex As Exception
            WriteLog("ERR: Datas, Exception : " & ex.Message)
            Datas = ""
        End Try

    End Function
    Private Sub WriteLog(ByVal message As String)
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
