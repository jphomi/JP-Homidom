Imports HoMIDom
Imports HoMIDom.HoMIDom.Server
Imports HoMIDom.HoMIDom.Device
Imports System.Net
Imports System.Reflection
Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Text
Imports System.Xml
Imports System.Xml.XPath
Imports Newtonsoft.Json.Linq
Imports MySql.Data


<Serializable()> Public Class Driver_SenCrop
    Implements HoMIDom.HoMIDom.IDriver

#Region "Variables génériques"
    '!!!Attention les variables ci-dessous doivent avoir une valeur par défaut obligatoirement
    'aller sur l'adresse http://www.somacon.com/p113.php pour avoir un ID
    Dim _ID As String = "39A8DAA6-7881-11E7-B68F-4F379AC7F8C8" 'ne pas modifier car utilisé dans le code du serveur
    Dim _Nom As String = "SenCrop" 'Nom du driver à afficher
    Dim _Enable As Boolean = False 'Activer/Désactiver le driver
    Dim _Description As String = "Driver SenCrop" 'Description du driver
    Dim _StartAuto As Boolean = False 'True si le driver doit démarrer automatiquement
    Dim _Protocol As String = "Http" 'Protocole utilisé par le driver, exemple: RS232
    Dim _IsConnect As Boolean = False 'True si le driver est connecté et sans erreur
    Dim _IP_TCP As String = "@" 'Adresse IP TCP à utiliser, "@" si non applicable pour le cacher côté client
    Dim _Port_TCP As String = "@" 'Port TCP à utiliser, "@" si non applicable pour le cacher côté client
    Dim _IP_UDP As String = "@" 'Adresse IP UDP à utiliser, , "@" si non applicable pour le cacher côté client
    Dim _Port_UDP As String = "@" 'Port UDP à utiliser, , "@" si non applicable pour le cacher côté client
    Dim _Com As String = "@" 'Port COM à utiliser, , "@" si non applicable pour le cacher côté client
    Dim _Refresh As Integer = 180 'Valeur à laquelle le driver doit rafraichir les valeurs des devices (ex: toutes les 200ms aller lire les devices)
    Dim _Modele As String = "@" 'Modèle du driver/interface
    Dim _Version As String = My.Application.Info.Version.ToString 'Version du driver
    Dim _OsPlatform As String = "3264" 'plateforme compatible 32 64 ou 3264 bits
    Dim _Picture As String = "" 'Image du driver (non utilisé actuellement)
    Dim _Server As HoMIDom.HoMIDom.Server 'Objet Reflètant le serveur
    Dim _DeviceSupport As New ArrayList 'Type de Device supporté par le driver
    Dim _Device As HoMIDom.HoMIDom.Device 'Image reflétant un device
    Dim _Parametres As New ArrayList 'Paramètres supplémentaires associés au driver
    Dim _LabelsDriver As New ArrayList 'Libellés, tooltip associés au driver
    Dim _LabelsDevice As New ArrayList 'Libellés, tooltip des devices associés au driver
    Dim MyTimer As New Timers.Timer 'Timer du driver
    Dim _IdSrv As String 'Id du Serveur (pour autoriser à utiliser des commandes)
    Dim _DeviceCommandPlus As New List(Of HoMIDom.HoMIDom.Device.DeviceCommande) 'Liste des commandes avancées du driver
    Dim _AutoDiscover As Boolean = False

    'A ajouter dans les ppt du driver

    'param avancé
    Dim _DEBUG As Boolean = False

#End Region

#Region "Variables Internes"
    'Insérer ici les variables internes propres au driver et non communes


    Dim _UrlSencrop As String = "https://tt1ph811kc.execute-api.eu-central-1.amazonaws.com/prod/"
    Dim _UrlSencropDevices As String = "https://tt1ph811kc.execute-api.eu-central-1.amazonaws.com/prod/devices"
    Dim _UrlSencropPrevisions As String = "https://tt1ph811kc.execute-api.eu-central-1.amazonaws.com/prod/weather01_trans/"
    Dim _UserName As String = ""
    Dim _Password As String = ""
    Dim _Obj As Object = Nothing

    Dim authenconnec As reponseauthen
    Dim listestat As New List(Of Stations)
    '    Dim meteo As List(Of String) = New List(Of String)
    Dim listprevisions As Previsions

    Public Class reponseauthen
        Public login As Boolean
        Public authentication_token As String
    End Class

    Public Class Stations
        Public serial As String
        Public role As String
        Public status As Status
        Public fonctionlive As List(Of FonctionLive)
    End Class

    Public Class Status
        Public model_name As String
        Public calibration As String
        Public user_id As Integer
        Public name As String
        Public identification As String
        Public serial As String
        Public firmware As String
        Public gps_fix As Integer
        Public active As Integer
        Public turn_off As Object
        Public signalnr As Object
        Public batt As Object
        Public latitude As Double
        Public altitude As Integer
        Public longitude As Double

    End Class

    Public Class FonctionLive
        Public fonction As String
        Public params As List(Of String)
        Public result As List(Of Double)
        Public sensor_fonction As String
    End Class

    Public Class Allvaluedetail
        Public serial As String
        Public value As Object
    End Class

    Public Class Previsions
        Public latitude As Double
        Public longitude As Double
        Public timezone As String
        Public currently As PrevisionDetail
        Public hourly As Previsions48h
        Public daily As Previsions8j
    End Class

    Public Class Previsions48h
        Public summary As String
        Public data As List(Of PrevisionDetail)
    End Class

    Public Class Previsions8j
        Public summary As String
        Public data As List(Of PrevisionDetail)
    End Class

    Public Class PrevisionDetail
        Public time As Integer
        Public summary As String
        Public icon As String
        Public precipIntensity As Double
        Public precipProbability As Double
        Public precipType As String
        Public temperature As Double
        Public temperatureHigh As Double
        Public temperatureLow As Double
        Public dewPoint As Double
        Public humidity As Double
        Public pressure As Double
        Public windSpeed As Double
        Public windGust As Double
        Public windBearing As Double
        Public cloudCover As Double
        Public uvIndex As Double
        Public temperatureMin As Double
        Public temperatureMax As Double
    End Class

#End Region

#Region "Propriétés génériques"
    ''' <summary>
    ''' Evènement déclenché par le driver au serveur
    ''' </summary>
    ''' <param name="DriveName"></param>
    ''' <param name="TypeEvent"></param>
    ''' <param name="Parametre"></param>
    ''' <remarks></remarks>
    Public Event DriverEvent(ByVal DriveName As String, ByVal TypeEvent As String, ByVal Parametre As Object) Implements HoMIDom.HoMIDom.IDriver.DriverEvent

    ''' <summary>
    ''' ID du serveur
    ''' </summary>
    ''' <value>ID du serveur</value>
    ''' <remarks>Permet d'accéder aux commandes du serveur pour lesquels il faut passer l'ID du serveur</remarks>
    Public WriteOnly Property IdSrv As String Implements HoMIDom.HoMIDom.IDriver.IdSrv
        Set(ByVal value As String)
            _IdSrv = value
        End Set
    End Property

    ''' <summary>
    ''' Port COM du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' Retourne la liste des devices supportés par le driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Voir Sub New</remarks>
    Public ReadOnly Property DeviceSupport() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.DeviceSupport
        Get
            Return _DeviceSupport
        End Get
    End Property

    ''' <summary>
    ''' Liste des paramètres avancés du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Voir Sub New</remarks>
    Public Property Parametres() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.Parametres
        Get
            Return _Parametres
        End Get
        Set(ByVal value As System.Collections.ArrayList)
            _Parametres = value
        End Set
    End Property

    ''' <summary>
    ''' Liste les libellés et tooltip des champs associés au driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LabelsDriver() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.LabelsDriver
        Get
            Return _LabelsDriver
        End Get
        Set(ByVal value As System.Collections.ArrayList)
            _LabelsDriver = value
        End Set
    End Property

    ''' <summary>
    ''' Liste les libellés et tooltip des champs associés au device associé au driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LabelsDevice() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.LabelsDevice
        Get
            Return _LabelsDevice
        End Get
        Set(ByVal value As System.Collections.ArrayList)
            _LabelsDevice = value
        End Set
    End Property

    ''' <summary>
    ''' Active/Désactive le driver
    ''' </summary>
    ''' <value>True si actif</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Enable() As Boolean Implements HoMIDom.HoMIDom.IDriver.Enable
        Get
            Return _Enable
        End Get
        Set(ByVal value As Boolean)
            _Enable = value
        End Set
    End Property

    ''' <summary>
    ''' ID du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ID() As String Implements HoMIDom.HoMIDom.IDriver.ID
        Get
            Return _ID
        End Get
    End Property

    ''' <summary>
    ''' Adresse IP TCP du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IP_TCP() As String Implements HoMIDom.HoMIDom.IDriver.IP_TCP
        Get
            Return _IP_TCP
        End Get
        Set(ByVal value As String)
            _IP_TCP = value
        End Set
    End Property

    ''' <summary>
    ''' Adresse IP UDP du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IP_UDP() As String Implements HoMIDom.HoMIDom.IDriver.IP_UDP
        Get
            Return _IP_UDP
        End Get
        Set(ByVal value As String)
            _IP_UDP = value
        End Set
    End Property

    ''' <summary>
    ''' Permet de savoir si le driver est actif
    ''' </summary>
    ''' <value>Retourne True si le driver est démarré</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsConnect() As Boolean Implements HoMIDom.HoMIDom.IDriver.IsConnect
        Get
            Return _IsConnect
        End Get
    End Property

    ''' <summary>
    ''' Modèle du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Modele() As String Implements HoMIDom.HoMIDom.IDriver.Modele
        Get
            Return _Modele
        End Get
        Set(ByVal value As String)
            _Modele = value
        End Set
    End Property

    ''' <summary>
    ''' Nom du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Nom() As String Implements HoMIDom.HoMIDom.IDriver.Nom
        Get
            Return _Nom
        End Get
    End Property

    ''' <summary>
    ''' Image du driver (non utilisé)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Picture() As String Implements HoMIDom.HoMIDom.IDriver.Picture
        Get
            Return _Picture
        End Get
        Set(ByVal value As String)
            _Picture = value
        End Set
    End Property

    ''' <summary>
    ''' Port TCP du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Port_TCP() As String Implements HoMIDom.HoMIDom.IDriver.Port_TCP
        Get
            Return _Port_TCP
        End Get
        Set(ByVal value As String)
            _Port_TCP = value
        End Set
    End Property

    ''' <summary>
    ''' Port UDP du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Port_UDP() As String Implements HoMIDom.HoMIDom.IDriver.Port_UDP
        Get
            Return _Port_UDP
        End Get
        Set(ByVal value As String)
            _Port_UDP = value
        End Set
    End Property

    ''' <summary>
    ''' Type de protocole utilisé par le driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Protocol() As String Implements HoMIDom.HoMIDom.IDriver.Protocol
        Get
            Return _Protocol
        End Get
    End Property

    ''' <summary>
    ''' Valeur de rafraichissement des devices
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Refresh() As Integer Implements HoMIDom.HoMIDom.IDriver.Refresh
        Get
            Return _Refresh
        End Get
        Set(ByVal value As Integer)
            _Refresh = value
        End Set
    End Property

    ''' <summary>
    ''' Objet représentant le serveur
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Server() As HoMIDom.HoMIDom.Server Implements HoMIDom.HoMIDom.IDriver.Server
        Get
            Return _Server
        End Get
        Set(ByVal value As HoMIDom.HoMIDom.Server)
            _Server = value
        End Set
    End Property

    ''' <summary>
    ''' Version du driver
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
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

    ''' <summary>
    ''' True si le driver doit démarrer automatiquement
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
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

    ''' <summary>Retourne la liste des Commandes avancées de type DeviceCommande</summary>
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

    ''' <summary>Permet de vérifier si un champ est valide</summary>
    ''' <param name="Champ">Nom du champ à vérifier, ex ADRESSE1</param>
    ''' <param name="Value">Valeur à vérifier</param>
    ''' <returns>Retourne 0 si OK, sinon un message d'erreur</returns>
    ''' <remarks></remarks>
    Public Function VerifChamp(ByVal Champ As String, ByVal Value As Object) As String Implements HoMIDom.HoMIDom.IDriver.VerifChamp
        Try
            Dim retour As String = "0"
            Select Case UCase(Champ)
                Case "ADRESSE1"
                    If Value IsNot Nothing Then
                        If String.IsNullOrEmpty(Value) Or IsNumeric(Value) Then
                            retour = "Veuillez choisir l'équipement"
                        End If
                    End If
                Case "ADRESSE2"

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
            If My.Computer.Network.IsAvailable = False Then
                _IsConnect = False
                WriteLog("ERR: Pas d'accés réseau! Vérifiez votre connection")
                WriteLog("Driver non démarré")
                Exit Sub
            End If

            Try
                _DEBUG = _Parametres.Item(0).Valeur
                _UserName = _Parametres.Item(1).Valeur
                _Password = _Parametres.Item(2).Valeur
            Catch ex As Exception
                _DEBUG = False
                _Parametres.Item(0).Valeur = False
                WriteLog("ERR: Start, Erreur dans les paramétres avancés. utilisation des valeur par défaut : " & ex.Message)
            End Try

            WriteLog("Start, connection au serveur " & _UrlSencrop)

            If Get_Token(_UrlSencrop & "users/sign_in", _UserName, _Password) Then
                _IsConnect = True
                WriteLog("Driver démarré avec succés à l'adresse " & _UrlSencrop)

                Get_Device(_UrlSencropDevices)
 
                'lance le time du driver, mini toutes les minutes
                If _Refresh = 0 Then _Refresh = 180
                MyTimer.Interval = _Refresh * 1000
                MyTimer.Enabled = True
                AddHandler MyTimer.Elapsed, AddressOf TimerTick

            Else
                _IsConnect = False
                WriteLog("ERR: Driver non démarré à l'adresse " & _UrlSencrop)
            End If
        Catch ex As Exception
            _IsConnect = False
            WriteLog("ERR: Driver Erreur démarrage " & ex.Message)
            WriteLog("Driver non démarré")
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
    ''' <remarks>Le device demande au driver d'aller le lire suivant son adresse</remarks>
    Public Sub Read(ByVal Objet As Object) Implements HoMIDom.HoMIDom.IDriver.Read
        Try
            If _Enable = False Then
                WriteLog("ERR: Read, Erreur: Impossible de traiter la commande car le driver n'est pas activé (Enable)")
                Exit Sub
            End If
            Try ' lecture de la variable debug, permet de rafraichir la variable debug sans redemarrer le service
                _DEBUG = _Parametres.Item(0).Valeur
            Catch ex As Exception
                _DEBUG = False
                _Parametres.Item(0).Valeur = False
                WriteLog("ERR: Erreur de lecture de debug : " & ex.Message)
            End Try

            'Si internet n'est pas disponible on ne mets pas à jour les informations
            If My.Computer.Network.IsAvailable = False Then
                WriteLog("ERR: READ, Pas de réseau! Lecture du périphérique impossible")
                Exit Sub
            End If

            Dim workTable As New DataTable
            workTable.Columns.Add("origine", GetType(String))
            workTable.Columns.Add("ident", GetType(String))
            workTable.Columns.Add("lat_long", GetType(String))
            workTable.Columns.Add("altitude", GetType(String))
            workTable.Columns.Add("parametre", GetType(String))
            workTable.Columns.Add("values", GetType(String))

            Dim id_stat As Integer = CInt(Mid(Objet.adresse1, 1, InStr(Objet.adresse1, "#") - 1)) - 1

            Select Case Objet.Type
                Case "METEO"
                    Get_Previsions(_UrlSencropPrevisions & listestat.Item(id_stat).status.latitude & "," & listestat.Item(id_stat).status.longitude)

                    For i = 0 To listestat.Item(id_stat).fonctionlive.Count - 1
                        If listestat.Item(id_stat).fonctionlive.Item(i).sensor_fonction = "TEMP_AIR_H1-FL0000_lastrecord" Then
                            Objet.TemperatureActuel = Regex.Replace(CStr(listestat.Item(id_stat).fonctionlive.Item(i).result(1)), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                        End If
                    Next
                    For i = 0 To listestat.Item(id_stat).fonctionlive.Count - 1
                        If listestat.Item(id_stat).fonctionlive.Item(i).sensor_fonction = "RH_AIR_H1-FL0000_lastrecord" Then
                            Objet.HumiditeActuel = Regex.Replace(CStr(listestat.Item(id_stat).fonctionlive.Item(i).result(1)), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                        End If
                    Next

                    ' conversion den degre celsius
                    Dim tmp_f As Double
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(0).temperatureHigh), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MaxToday = (tmp_f - 32) / 1.8
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(1).temperatureHigh), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MaxJ1 = (tmp_f - 32) / 1.8
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(2).temperatureHigh), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MaxJ2 = (tmp_f - 32) / 1.8
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(3).temperatureHigh), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MaxJ3 = (tmp_f - 32) / 1.8
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(0).temperatureLow), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MinToday = (tmp_f - 32) / 1.8
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(1).temperatureLow), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MinJ1 = (tmp_f - 32) / 1.8
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(2).temperatureLow), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MinJ2 = (tmp_f - 32) / 1.8
                    tmp_f = Regex.Replace(CStr(listprevisions.daily.data.Item(3).temperatureLow), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                    Objet.MinJ3 = (tmp_f - 32) / 1.8

                    Dim st_icon As String
                    Objet.ConditionToday = listprevisions.currently.summary
                    st_icon = listprevisions.currently.icon.Replace("-day", "")
                    st_icon = st_icon.Replace("-", "_")
                    Objet.IconToday = st_icon
                    Objet.ConditionJ1 = listprevisions.daily.data.Item(0).summary
                    st_icon = listprevisions.daily.data.Item(0).icon.Replace("-day", "")
                    st_icon = st_icon.Replace("-", "_")
                    Objet.IconJ1 = st_icon
                    Objet.ConditionJ2 = listprevisions.daily.data.Item(1).summary
                    st_icon = listprevisions.daily.data.Item(1).icon.Replace("-day", "")
                    st_icon = st_icon.Replace("-", "_")
                    Objet.IconJ2 = st_icon
                    Objet.ConditionJ3 = listprevisions.daily.data.Item(2).summary
                    st_icon = listprevisions.daily.data.Item(2).icon.Replace("-day", "")
                    st_icon = st_icon.Replace("-", "_")
                    Objet.IconJ3 = st_icon

                    Objet.JourToday = TraduireJour(Mid(Now.DayOfWeek.ToString, 1, 3))
                    Objet.JourJ1 = TraduireJour(Mid(Now.AddDays(1).DayOfWeek.ToString, 1, 3))
                    Objet.JourJ2 = TraduireJour(Mid(Now.AddDays(2).DayOfWeek.ToString, 1, 3))
                    Objet.JourJ3 = TraduireJour(Mid(Now.AddDays(3).DayOfWeek.ToString, 1, 3))

                    ' objet.VentActuel = Regex.Replace(CStr(moduleIDalire.dashboard_data.WindStrength), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)

                Case "BATTERIE"
                    Objet.Value = listestat.Item(id_stat).status.batt

                Case "TEMPERATURE"
                    For i = 0 To listestat.Item(id_stat).fonctionlive.Count - 1
                        If listestat.Item(id_stat).fonctionlive.Item(i).sensor_fonction = "TEMP_AIR_H1-FL0000_lastrecord" Then
                            Objet.value = Regex.Replace(CStr(listestat.Item(id_stat).fonctionlive.Item(i).result(1)), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                        End If
                    Next

                Case "HUMIDITE"
                    For i = 0 To listestat.Item(id_stat).fonctionlive.Count - 1
                        If listestat.Item(id_stat).fonctionlive.Item(i).sensor_fonction = "RH_AIR_H1-FL0000_lastrecord" Then
                            Objet.Value = Regex.Replace(CStr(listestat.Item(id_stat).fonctionlive.Item(i).result(1)), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                        End If
                    Next
                Case "PLUIETOTAL"
                    For i = 0 To listestat.Item(id_stat).fonctionlive.Count - 1
                        If listestat.Item(id_stat).fonctionlive.Item(i).sensor_fonction = "RAIN_TIC-FL0001_sumlast" Then
                            Objet.Value = Regex.Replace(CStr(listestat.Item(id_stat).fonctionlive.Item(i).result(2)), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                        End If
                    Next
                Case "PLUIECOURANT"
                    For i = 0 To listestat.Item(id_stat).fonctionlive.Count - 1
                        If listestat.Item(id_stat).fonctionlive.Item(i).sensor_fonction = "RAIN_TIC-FL0001_sumlast" Then
                            Objet.Value = Regex.Replace(CStr(listestat.Item(id_stat).fonctionlive.Item(i).result(0)), "[.,]", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                        End If
                    Next

                Case "GENERIQUESTRING"
                    If InStr(Objet.adresse1, "All") > 0 Then
                        workTable.Clear()
                        If (InStr(Objet.adresse2, "lastrecord")) Or (InStr(Objet.adresse2, "sumlast")) > 0 Then
                            For i = 0 To listestat.Count - 1
                                For j = 0 To listestat.Item(i).fonctionlive.Count - 1
                                    If listestat.Item(i).fonctionlive.Item(j).sensor_fonction = Objet.adresse2 Then
                                        If InStr(Objet.adresse2, "RAIN") = 0 Then
                                            workTable.Rows.Add("Sencrop", listestat.Item(i).status.serial, listestat.Item(i).status.latitude & " , " & listestat.Item(i).status.longitude, listestat.Item(i).status.altitude.ToString, Objet.adresse2, listestat.Item(i).fonctionlive.Item(j).result(1))
                                        Else
                                            workTable.Rows.Add("Sencrop", listestat.Item(i).status.serial, listestat.Item(i).status.latitude & " , " & listestat.Item(i).status.longitude, listestat.Item(i).status.altitude.ToString, Objet.adresse2, listestat.Item(i).fonctionlive.Item(j).result(2))
                                        End If
                                    End If
                                Next
                            Next
                        Else
                            workTable.Clear()
                            For Each station In listestat
                                Dim statu As Status
                                statu = station.status
                                Dim strjson As String = Newtonsoft.Json.JsonConvert.SerializeObject(statu, Newtonsoft.Json.Formatting.None)

                                Dim ArrayStatus() As String = Split(strjson, ",")
                                Dim stid As String
                                Dim stvalue As Object
                                For Each st In ArrayStatus
                                    st = st.Replace("""", "")
                                    st = st.Replace("{", "")
                                    st = st.Replace("}", "")
                                    stid = Mid(st, 1, (InStr(st, ":") - 1))
                                    stvalue = Mid(st, InStr(st, ":") + 1, Len(st))
                                    If InStr(stid, Objet.adresse2) > 0 Then
                                        workTable.Rows.Add("Sencrop", station.serial, station.status.latitude & " / " & station.status.longitude, station.status.altitude.ToString, Objet.adresse2, stvalue)
                                    End If
                                Next
                            Next
                        End If
                        Objet.value = Newtonsoft.Json.JsonConvert.SerializeObject(workTable)
                    End If
                Case Else
                    WriteLog("ERR: GetData=> Pas de valeur enregistrée")
                    Exit Sub
            End Select
            '           WriteLog("DBG: Valeur enregistrée : " & Objet.Type & " -> " & Objet.value)

        Catch ex As Exception
            WriteLog("ERR: Read, adresse1 : " & Objet.adresse1 & " - adresse2 : " & Objet.adresse2)
            WriteLog("ERR: Read, Exception : " & ex.Message)
        End Try

    End Sub

    ''' <summary>Commander un device</summary>
    ''' <param name="Objet">Objet représetant le device à commander</param>
    ''' <param name="Command">La commande à passer</param>
    ''' <param name="Parametre1">parametre 1 de la commande, optionnel</param>
    ''' <param name="Parametre2">parametre 2 de la commande, optionnel</param>
    ''' <remarks></remarks>
    Public Sub Write(ByVal Objet As Object, ByVal Command As String, Optional ByVal Parametre1 As Object = Nothing, Optional ByVal Parametre2 As Object = Nothing) Implements HoMIDom.HoMIDom.IDriver.Write
        Try
            If _Enable = False Then
                WriteLog("ERR: Read, Erreur: Impossible de traiter la commande car le driver n'est pas activé (Enable)")
                Exit Sub
            End If
            Try ' lecture de la variable debug, permet de rafraichir la variable debug sans redemarrer le service
                _DEBUG = _Parametres.Item(0).Valeur
                _UserName = _Parametres.Item(1).Valeur
                _Password = _Parametres.Item(2).Valeur
            Catch ex As Exception
                _DEBUG = False
                _Parametres.Item(0).Valeur = False
                WriteLog("ERR: Erreur de lecture de debug : " & ex.Message)
            End Try

            'Si internet n'est pas disponible on ne mets pas à jour les informations
            If My.Computer.Network.IsAvailable = False Then
                WriteLog("ERR: READ, Pas de réseau! Lecture du périphérique impossible")
                Exit Sub
            End If

        Catch ex As Exception
            WriteLog("ERR: WRITE, Exception : " & ex.Message)
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
            WriteLog("ERR: add_DeviceCommande, Exception :" & ex.Message)
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
            WriteLog("ERR: add_LibelleDriver, Exception : " & ex.Message)
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
            WriteLog("ERR: add_LibelleDevice, Exception : " & ex.Message)
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
            WriteLog("ERR: add_ParamAvance, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Creation d'un objet de type</summary>
    ''' <remarks></remarks>
    Public Sub New()
        Try
            _Version = Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString

            'liste des devices compatibles
            _DeviceSupport.Add(ListeDevices.METEO)
            _DeviceSupport.Add(ListeDevices.BAROMETRE)
            _DeviceSupport.Add(ListeDevices.DIRECTIONVENT)
            _DeviceSupport.Add(ListeDevices.HUMIDITE)
            _DeviceSupport.Add(ListeDevices.UV)
            _DeviceSupport.Add(ListeDevices.VITESSEVENT)
            _DeviceSupport.Add(ListeDevices.TEMPERATURE)
            _DeviceSupport.Add(ListeDevices.BATTERIE)
            '  _DeviceSupport.Add(ListeDevices.PLUIECOURANT)
            _DeviceSupport.Add(ListeDevices.HUMIDITE)
            _DeviceSupport.Add(ListeDevices.PLUIETOTAL)
            _DeviceSupport.Add(ListeDevices.GENERIQUESTRING)
            _DeviceSupport.Add(ListeDevices.GENERIQUEVALUE)


            'Parametres avancés
            Add_ParamAvance("Debug", "Activer le Debug complet (True/False)", False)
            Add_ParamAvance("Username", "Nom utilisateur", "admin")
            Add_ParamAvance("Password", "Mot de passe", "homi123456")

            'ajout des commandes avancées pour les devices

            'ajout des commandes avancées pour les devices
            Add_DeviceCommande("ALL_LIGHT_ON", "", 0)
            Add_DeviceCommande("ALL_LIGHT_OFF", "", 0)

            'Libellé Driver
            Add_LibelleDriver("HELP", "Aide...", "Pas d'aide actuellement...")

            'Libellé Device
            Add_LibelleDevice("ADRESSE1", "Numéro de la station", "Numéro de la station")
            Add_LibelleDevice("ADRESSE2", "Choix de la valeur", "Choix de la valeur")
            Add_LibelleDevice("REFRESH", "Refresh (sec)", "Valeur de rafraîchissement de la mesure en secondes")
            'Add_LibelleDevice("LASTCHANGEDUREE", "LastChange Durée", "")

        Catch ex As Exception
            WriteLog("ERR: New, Exception : " & ex.Message)
        End Try
    End Sub

    ''' <summary>Si refresh >0 gestion du timer</summary>
    ''' <remarks>PAS UTILISE CAR IL FAUT LANCER UN TIMER QUI LANCE/ARRETE CETTE FONCTION dans Start/Stop</remarks>
    Private Sub TimerTick(ByVal source As Object, ByVal e As System.Timers.ElapsedEventArgs)
        If (_Enable = False) Or (_IsConnect = False) Then
            'arrete le timer du driver
            MyTimer.Enabled = False
            Exit Sub
        End If

        Get_Device(_UrlSencropDevices)

    End Sub


#End Region

#Region "Fonctions internes"
    'Insérer ci-dessous les fonctions propres au driver

    Function Get_Token(adrs As String, user As String, password As String) As Boolean
        ' recupere les configuration de sencrop

        Dim reqparam As String = ""
        Dim responsebodystr As String = ""

        Try
            reqparam = "{""email"":""" & user & """,""password"":""" & password & """}"
            reqparam = reqparam.PadRight(46)
            WriteLog("DBG: Get_Token, reqparam -> '" & reqparam & "' / " & reqparam.Length)
            Dim postBytes As Byte() = Encoding.ASCII.GetBytes(reqparam)
            Dim Request As HttpWebRequest = HttpWebRequest.Create(adrs)
            Request.ContentType = "application/json"
            Request.Method = "POST"
            Request.KeepAlive = True
            Request.ContentLength = reqparam.Length
            Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0"
            Request.Accept = "*/*"
            Request.Timeout = 5000
            Request.Host = "tt1ph811kc.execute-api.eu-central-1.amazonaws.com"
            Request.Referer = "http://app.sencrop.com/login"

            Dim writer As New StreamWriter(Request.GetRequestStream)
            writer.Write(reqparam)
            writer.Close()

            Dim Response As HttpWebResponse = Request.GetResponse()

            Dim responsereader = New StreamReader(Response.GetResponseStream())
            responsebodystr = responsereader.ReadToEnd()
            responsereader.Close()
            authenconnec = Newtonsoft.Json.JsonConvert.DeserializeObject(responsebodystr, GetType(reponseauthen))

            WriteLog("DBG: Get_Token, responsebodystr -> " & responsebodystr)
            WriteLog("DBG: Get_Token, authenconnec.login -> " & authenconnec.login)
            WriteLog("DBG: Get_Token, authenconnec.token -> " & authenconnec.authentication_token)

            Return authenconnec.login

        Catch ex As Exception
            WriteLog("ERR: " & "GET_Token, " & ex.Message)
            WriteLog("ERR: " & "GET_Token, Url: " & adrs)
            Return False
        End Try
    End Function

    Function Get_Device(adrs As String) As Boolean
        ' recupere les device de sencrop

        Dim reqparam As String = ""
        Dim responsebodystr As String = ""

        Try
            WriteLog("DBG: Get_Device, url -> " & adrs)
            Dim httpDate As String = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss ") + "GMT"
            Dim Request As HttpWebRequest = HttpWebRequest.Create(adrs)
            Request.ContentType = "application/x-amz-json-1.1"
            Request.Method = "GET"
            Request.KeepAlive = True
            Request.ContentLength = reqparam.Length
            Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0"
            Request.Accept = "*/*"
            Request.Host = "tt1ph811kc.execute-api.eu-central-1.amazonaws.com"
            Request.Referer = "http://app.sencrop.com/dashboard/measures"
            Request.Headers.Clear()
            Request.Headers.Add("authentication_token", authenconnec.authentication_token)
            Request.Headers.Add("origin", "http://app.sencrop.com")

            Dim Response As HttpWebResponse = Request.GetResponse()

            Dim responsereader = New StreamReader(Response.GetResponseStream())
            responsebodystr = responsereader.ReadToEnd()
            responsereader.Close()

            '            responsebodystr = "[{""serial"":""203737"",""role"":""owner""},{""serial"":""203F66"",""role"":""owner""},{""serial"":""203D21"",""role"":""owner""},{""serial"":""201C54"",""role"":""owner""},{""serial"":""201A97"",""role"":""owner""},{""role"":""collaborator"",""serial"":""74415""},{""serial"":""2035EF"",""role"":""owner""},{""serial"":""20327A"",""role"":""owner""},{""serial"":""203279"",""role"":""owner""},{""role"":""collaborator"",""serial"":""1B28E4""}]"

            'nettoie la chaine car mot clé VB
            responsebodystr = responsebodystr.Replace("function", "fonction") ' function mot reservé VB
            responsebodystr = responsebodystr.Replace("sensor-fonction", "sensor_fonction") ' signe - reservé VB

            Dim jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of Object))(responsebodystr)

            listestat.Clear()
            Dim s As String = ""
            For Each _stat In jsonObj
                s = _stat.ToString
                'nettoie la chaine
                s = s.Replace(" ", "")
                s = s.Replace(vbCr, "")
                s = s.Replace(vbLf, "")
                '     s = "{""serial"":""203737"",""role"":""owner""}"
                Dim st As Stations
                st = Newtonsoft.Json.JsonConvert.DeserializeObject(s, GetType(Stations))
                If st.serial.Length > 0 Then
                    listestat.Insert(listestat.Count, st)
                    '                    WriteLog("DBG: Get_Device, " & listestat.Count & " " & "station -> " & listestat.Item(listestat.Count - 1).serial)
                End If
            Next
            WriteLog("DBG: Get_Device, " & listestat.Count & " " & "stations trouvées")

            'recherche des champs de statuts. Permet d'avoir une liste variable en fonction de la class status
            Dim workTable As New DataTable
            workTable.Columns.Add("ident", GetType(String))
            workTable.Clear()
            For Each station In listestat
                Dim statu As Status
                statu = station.status
                Dim strjson As String = Newtonsoft.Json.JsonConvert.SerializeObject(statu, Newtonsoft.Json.Formatting.None)
                Dim ArrayStatus() As String = Split(strjson, ",")
                For Each st In ArrayStatus
                    st = st.Replace("""", "")
                    st = st.Replace("{", "")
                    st = st.Replace("}", "")
                    workTable.Rows.Add(Mid(st, 1, (InStr(st, ":") - 1)))
                Next
                Exit For
            Next

            Dim _libelleadr1 As String = "0 # All |"
            Dim _libelleadr2 As String = ""

            'cas d'un choix de valeur pour toutes les stations
            For Each st In workTable.Rows
                _libelleadr2 += "0 #;  " & st(0) & " |"
            Next
            'ajoute les paramètre spécifique des stations sans generer de doublons
            For i = 0 To listestat.Count - 1
                For j = 0 To listestat.Item(i).fonctionlive.Count - 1
                    If InStr(_libelleadr2, listestat.Item(i).fonctionlive.Item(j).sensor_fonction) = 0 Then
                        _libelleadr2 += "0 #; " & listestat.Item(i).fonctionlive.Item(j).sensor_fonction & "|"
                    End If
                Next
            Next

            'cas d'un choix de valeur par station
            For i = 0 To listestat.Count - 1
                _libelleadr1 += i + 1 & " # " & listestat.Item(i).serial & " / " & listestat.Item(i).status.name & "|"
                For Each st In workTable.Rows
                    _libelleadr2 += i + 1 & " #;  " & st(0) & " |"
                Next
                'complement avec les champ de fonction variables suivant stations
                For j = 0 To listestat.Item(i).fonctionlive.Count - 1
                    _libelleadr2 += i + 1 & " #; " & listestat.Item(i).fonctionlive.Item(j).sensor_fonction & "|"
                Next
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
                    Case "ADRESSE2"
                        _libelleadr2 = Mid(_libelleadr2, 1, Len(_libelleadr2) - 1) 'enleve le dernier | pour eviter davoir une ligne vide a la fin
                        ld0.Parametre = _libelleadr2
                        _LabelsDevice(i) = ld0
                End Select
            Next

            Return True

        Catch ex As Exception
            WriteLog("ERR: " & "GET_Device, " & ex.Message)
            WriteLog("ERR: " & "GET_Device, Url: " & adrs)
            Return False
        End Try
    End Function

    Function Get_Previsions(adrs As String) As Boolean
        ' recupere les device de sencrop

        Dim reqparam As String = ""
        Dim responsebodystr As String = ""

        Try
            WriteLog("DBG: Get_Device, url -> " & adrs)
            Dim httpDate As String = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss ") + "GMT"
            Dim Request As HttpWebRequest = HttpWebRequest.Create(adrs)
            Request.ContentType = "application/x-amz-json-1.1"
            Request.Method = "GET"
            Request.KeepAlive = True
            Request.ContentLength = reqparam.Length
            Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0"
            Request.Accept = "*/*"
            Request.Host = "tt1ph811kc.execute-api.eu-central-1.amazonaws.com"
            Request.Referer = "http://app.sencrop.com/dashboard/measures"
            Request.Headers.Clear()
            Request.Headers.Add("authentication_token", authenconnec.authentication_token)
            Request.Headers.Add("origin", "http://app.sencrop.com")

            Dim Response As HttpWebResponse = Request.GetResponse()

            Dim responsereader = New StreamReader(Response.GetResponseStream())
            responsebodystr = responsereader.ReadToEnd()
            responsereader.Close()

             listprevisions = Newtonsoft.Json.JsonConvert.DeserializeObject(responsebodystr, GetType(Previsions))

            Return True
        Catch ex As Exception
            WriteLog("ERR: " & "GET_Device, " & ex.Message)
            WriteLog("ERR: " & "GET_Device, Url: " & adrs)
            Return False
        End Try
    End Function

    Private Function TraduireJour(ByVal Jour As String) As String
        Try
            TraduireJour = "?"
            Select Case Jour
                Case "Thu"
                    TraduireJour = "Jeu"
                Case "Fri"
                    TraduireJour = "Ven"
                Case "Sat"
                    TraduireJour = "Sam"
                Case "Sun"
                    TraduireJour = "Dim"
                Case "Mon"
                    TraduireJour = "Lun"
                Case "Tue"
                    TraduireJour = "Mar"
                Case "Wed"
                    TraduireJour = "Mer"
            End Select
        Catch ex As Exception
            WriteLog("ERR: TraduireJour, " & ex.ToString)
            Return "?"
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
