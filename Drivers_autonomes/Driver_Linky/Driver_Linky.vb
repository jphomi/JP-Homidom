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


<Serializable()> Public Class Driver_Linky
    Implements HoMIDom.HoMIDom.IDriver

#Region "Variables génériques"
    '!!!Attention les variables ci-dessous doivent avoir une valeur par défaut obligatoirement
    'aller sur l'adresse http://www.somacon.com/p113.php pour avoir un ID
    Dim _ID As String = "C68E7576-4724-11E8-A0D2-B54EF4E0CD91" 'ne pas modifier car utilisé dans le code du serveur
    Dim _Nom As String = "Linky" 'Nom du driver à afficher
    Dim _Enable As Boolean = False 'Activer/Désactiver le driver
    Dim _Description As String = "Driver Linky" 'Description du driver
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


    Dim _LoginBaseUri As String = "espace-client-connexion.enedis.fr"
    Dim _ApiBaseUri As String = "espace-client-particuliers.enedis.fr"
    Dim _ApiEndPointLogin As String = "/auth/UI/Login"
    Dim _ApiEndPointData As String = "/group/espace-particuliers/suivi-de-consommation"
    Dim _ApiAcceptTerm As String = "/c/portal/update_terms_of_use"
    Dim _ApiRealm As String = "realm=particuliers"
    Dim _AdrsAcceuil As String = "https://" & _LoginBaseUri & _ApiEndPointLogin & "?" & _ApiRealm & "&goto=https://espace-client-particuliers.enedis.fr:/group/espace-particuliers/accueil"
    Dim _UserName As String = ""
    Dim _Password As String = ""
    Dim _typeProfil As String = "particulier"
    Dim _Obj As Object = Nothing
    Dim CookieLogin As New CookieContainer()

    Dim authenconnec As New reponseauthen

    Public Class reponseauthen
        Public login As Boolean
        Public JSESSIONID As String
        Public AMAuthCookie As String
        Public iPlanetDirectoryPro As Boolean = False
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
                _typeProfil = _Parametres.Item(3).Valeur
            Catch ex As Exception
                _DEBUG = False
                _Parametres.Item(0).Valeur = False
                WriteLog("ERR: Start, Erreur dans les paramétres avancés. utilisation des valeur par défaut : " & ex.Message)
            End Try

            'pour etre sur que l'écriture est bonne
            If InStr("entrepri", _typeProfil) > 0 Then
                _typeProfil = "entreprises"
            Else
                _typeProfil = "particuliers"
            End If
            _ApiBaseUri = _ApiBaseUri.Replace("particuliers", _typeProfil)
            _ApiEndPointData = _ApiEndPointData.Replace("particuliers", _typeProfil)
            _ApiRealm = _ApiRealm.Replace("particuliers", _typeProfil)
            _AdrsAcceuil = _AdrsAcceuil.Replace("particuliers", _typeProfil)

            WriteLog("Start, connection au serveur " & _LoginBaseUri)

            If Get_Token("https://" & _LoginBaseUri & _ApiEndPointLogin, _UserName, _Password) Then
                _IsConnect = True
                WriteLog("Driver démarré avec succés à l'adresse " & _LoginBaseUri)

                Get_Data("https://" & _ApiBaseUri & _ApiEndPointData, "urlCdcHeure")

                'lance le time du driver, mini toutes les 30 minutes
                If _Refresh = 0 Then _Refresh = 5400
                MyTimer.Interval = _Refresh * 1000
                MyTimer.Enabled = True
                AddHandler MyTimer.Elapsed, AddressOf TimerTick

            Else
                _IsConnect = False
                WriteLog("ERR: Driver non démarré à l'adresse " & _LoginBaseUri)
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


                Case "GENERIQUESTRING"
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
            _DeviceSupport.Add(ListeDevices.ENERGIEINSTANTANEE)


            'Parametres avancés
            Add_ParamAvance("Debug", "Activer le Debug complet (True/False)", False)
            Add_ParamAvance("Username", "Nom utilisateur", "admin")
            Add_ParamAvance("Password", "Mot de passe", "homi123456")
            Add_ParamAvance("Profil", "Type profil (particulier/entreprise)", "particulier")

            'ajout des commandes avancées pour les devices

            'ajout des commandes avancées pour les devices
            Add_DeviceCommande("ALL_LIGHT_ON", "", 0)
            Add_DeviceCommande("ALL_LIGHT_OFF", "", 0)

            'Libellé Driver
            Add_LibelleDriver("HELP", "Aide...", "Pas d'aide actuellement...")

            'Libellé Device
            Add_LibelleDevice("ADRESSE1", "Pas de temps", "Heure|Mois|Mois")
            Add_LibelleDevice("ADRESSE2", "@", "")
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

        '    Get_Device(_UrlLinkyDevices)

    End Sub


#End Region

#Region "Fonctions internes"
    'Insérer ci-dessous les fonctions propres au driver

    Function Get_Token(adrs As String, user As String, password As String) As Boolean
        ' recupere les configuration de Linky

        'chaine de parametre exemple
        'IDToken1=enedis%40ljpr.eu&IDToken2=2482WI141777.&SunQueryParamsString=cmVhbG09cGFydGljdWxpZXJz&encoded=true&gx_charset=UTF-8&goto=aHR0cHM6Ly9lc3BhY2UtY2xpZW50LXBhcnRpY3VsaWVycy5lbmVkaXMuZnI6L2dyb3VwL2VzcGFjZS1wYXJ0aWN1bGllcnMvYWNjdWVpbA%3D%3D&gotoOnFail=
        Dim reqparam As String = "IDToken1=" & user.Replace("@", "%40") & "&IDToken2=" & password & "&SunQueryParamsString=" & Convert.ToBase64String(Encoding.UTF8.GetBytes(_ApiRealm)) & "&encoded=true&gx_charset=UTF-8&goto=aHR0cHM6Ly9lc3BhY2UtY2xpZW50LXBhcnRpY3VsaWVycy5lbmVkaXMuZnI6L2dyb3VwL2VzcGFjZS1wYXJ0aWN1bGllcnMvYWNjdWVpbA%3D%3D&gotoOnFail="

        Dim responsebodystr As String = ""
        Dim CookieJar As New CookieContainer()
        authenconnec.AMAuthCookie = False

        Try
            Try
                'connection au site enedis pour recupérer cookies
                WriteLog("Get_Token, Tentative de connexion au site " & _AdrsAcceuil)

                Dim RequestBase As HttpWebRequest = HttpWebRequest.Create(_AdrsAcceuil)
                RequestBase.ContentType = "application/x-www-form-urlencoded"
                RequestBase.Method = "GET"
                RequestBase.UserAgent = "Mozilla/5.0 (Windows NT 10.0; …) Gecko/20100101 Firefox/64.0"
                RequestBase.Accept = "application/json, text/javascript, */*; q=0.01"
                RequestBase.AllowAutoRedirect = True

                Dim ResponseBase As HttpWebResponse = RequestBase.GetResponse()
                If ResponseBase.Headers("Set-Cookie") <> Nothing Then
                    Dim ckProp() As String = Split(ResponseBase.Headers("Set-Cookie"), ";")
                    For Each ck As String In ckProp
                        CookieJar.SetCookies(New Uri(_AdrsAcceuil), ck)
                    Next
                    WriteLog("DBG: Get_Token, responsebasecookies.count -> " & CookieJar.Count)
                    For i = 0 To CookieJar.Count - 1
                        ' WriteLog("DBG: Get_Token, responsebasecookies " & CookieJar.GetCookies(New Uri(_AdrsAcceuil)).Item(i).Name & " : " & CookieJar.GetCookies(New Uri(_AdrsAcceuil)).Item(i).Value)
                        Select Case CookieJar.GetCookies(New Uri(_AdrsAcceuil)).Item(i).Name
                            Case "JSESSIONID" : authenconnec.JSESSIONID = CookieJar.GetCookies(New Uri(_AdrsAcceuil)).Item(i).Value
                            Case "AMAuthCookie" : authenconnec.AMAuthCookie = CookieJar.GetCookies(New Uri(_AdrsAcceuil)).Item(i).Value
                        End Select
                    Next
                    WriteLog("Get_Token, connexion au site enedis.fr en cours")
                Else
                    WriteLog("ERR: Get_Token, connexion au site " & _AdrsAcceuil & " non effectuée")
                    Return False
                End If
            Catch ex As Exception
                WriteLog("ERR: " & "GET_Token, connexion enedis.fr, " & ex.Message)
                Return False
            End Try

            Try
                CookieLogin = CookieJar
                WriteLog("DBG: Get_Token, tentative de connexion au site -> " & adrs)
                WriteLog("DBG: Get_Token, parametre de connexion -> " & reqparam)

                Dim postReq As HttpWebRequest = DirectCast(WebRequest.Create(adrs), HttpWebRequest)
                postReq.Method = "POST"
                postReq.KeepAlive = True
                postReq.CookieContainer = CookieJar
                postReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0"
                postReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
                postReq.AllowAutoRedirect = False
                postReq.Host = _LoginBaseUri
                postReq.Referer = _AdrsAcceuil
                postReq.ContentType = "application/x-www-form-urlencoded"
                '    WriteLog("DBG: Get_Token, postReq.Headers.ToString -> " & postReq.Headers.ToString)

                Dim writer As New StreamWriter(postReq.GetRequestStream)
                writer.Write(reqparam)
                writer.Close()

                Dim postresponse As HttpWebResponse = DirectCast(postReq.GetResponse(), HttpWebResponse)
                If postresponse.Headers("Set-Cookie") <> Nothing Then
                    Dim header As String = postresponse.Headers("Set-Cookie").ToString
                    WriteLog("DBG: Get_Token, responseHeaders -> " & postresponse.Headers.ToString)
                    header = header.Replace("/,", "/")
                    header = header.Replace(", ", " ")
                    '                    WriteLog("DBG: Get_Token, responseHeaders -> " & header)
                    Dim ckPropLogin() As String = Split(header, ";")
                    For Each ckLogin As String In ckPropLogin
                        CookieLogin.SetCookies(New Uri(adrs), ckLogin)
                    Next
                    WriteLog("DBG: Get_Token, LoginCookies.count -> " & CookieLogin.Count)
                    For i = 0 To CookieLogin.Count - 1
                        WriteLog("DBG: Get_Token, responsecookies " & CookieLogin.GetCookies(New Uri(adrs)).Item(i).Name & " : " & CookieLogin.GetCookies(New Uri(adrs)).Item(i).Value)
                        Select Case CookieLogin.GetCookies(New Uri(adrs)).Item(i).Name
                            Case "JSESSIONID" : authenconnec.JSESSIONID = CookieLogin.GetCookies(New Uri(adrs)).Item(i).Value
                            Case "AMAuthCookie" : authenconnec.AMAuthCookie = CookieLogin.GetCookies(New Uri(adrs)).Item(i).Value
                            Case "iPlanetDirectoryPro" : authenconnec.iPlanetDirectoryPro = True
                        End Select
                    Next
                End If

                If authenconnec.iPlanetDirectoryPro Then
                    WriteLog("Get_Token, login au site " & _ApiBaseUri & " effectuée")
                    Return True
                Else
                    WriteLog("ERR: Get_Token, login au site " & adrs & " non effectuée")
                    Return False
                End If
            Catch ex As Exception
                WriteLog("ERR: " & "GET_Token, connexion, " & ex.Message)
                Return False
            End Try

        Catch ex As Exception
            WriteLog("ERR: " & "GET_Token, " & ex.Message)
            WriteLog("ERR: " & "GET_Token, Url: " & adrs)
            Return False
        End Try
    End Function

    Function Get_Data(adrs As String, pasdetemps As String) As Boolean
        ' recupere les datas de Linky

        ' exemple parametre requete
        ' https://espace-client-particuliers.enedis.fr/group/espace-particuliers/suivi-de-consommation?p_p_id=lincspartdisplaycdc_WAR_lincspartcdcportlet&p_p_lifecycle=2&p_p_state=normal&p_p_mode=view&p_p_resource_id=urlCdcJour&p_p_cacheability=cacheLevelPage&p_p_col_id=column-1&p_p_col_count=2
        '  reqparam = "_lincspartdisplaycdc_WAR_lincspartcdcportlet_dateDebut=29%2F11%2F2018&_lincspartdisplaycdc_WAR_lincspartcdcportlet_dateFin=29%2F12%2F2018"

        Dim datedebut As String = Date.Now.AddDays(-1).ToShortDateString
        Dim datefin As String = Date.Now.ToShortDateString
        Dim reqpart As String = "lincspartdisplaycdc_WAR_lincspartcdcportlet"
        Dim reqparam As String = "_" & reqpart & "_dateDebut=" & datedebut.Replace("/", "%2F") & "&_" & reqpart & "_dateFin=" & datefin.Replace("/", "%2F")
        Dim reqadrs As String = "?p_p_id=" & reqpart & "&p_p_lifecycle=2&p_p_state=normal&p_p_mode=view&p_p_resource_id=" & pasdetemps & "&p_p_cacheability=cacheLevelPage&p_p_col_id=column-1&p_p_col_count=2"
        Dim responsebodystr As String = ""
        Dim urldata As String = adrs & reqadrs

        Dim AdrsConso As String = "https://" & _ApiBaseUri & _ApiEndPointData
        Dim CookieJar As New CookieContainer()

        Try

            Try
                WriteLog("DBG: Get_Data, url -> " & urldata)
                WriteLog("DBG: Get_Data, reqparam -> " & reqparam)
                Dim postReq As HttpWebRequest = DirectCast(WebRequest.Create(urldata), HttpWebRequest)
                postReq.Method = "POST"
                postReq.KeepAlive = True
                postReq.CookieContainer = CookieJar
                postReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0"
                postReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
                postReq.AllowAutoRedirect = False
                postReq.Host = _ApiBaseUri
                postReq.Referer = AdrsConso
                postReq.ContentType = "application/x-www-form-urlencoded;charset=UTF-8"
                '    WriteLog("DBG: Get_Token, postReq.Headers.ToString -> " & postReq.Headers.ToString)

                Dim writer As New StreamWriter(postReq.GetRequestStream)
                writer.Write(reqparam)
                writer.Close()

                Dim postresponse As HttpWebResponse = DirectCast(postReq.GetResponse(), HttpWebResponse)

                Dim responsereader = New StreamReader(postresponse.GetResponseStream())
                responsebodystr = responsereader.ReadToEnd()
                responsereader.Close()

                WriteLog("DBG: Get_Data, reponsebodystr -> " & responsebodystr)


                '           Dim jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of Object))(responsebodystr)


                'Dim _libelleadr1 As String = "0 # All |"
                'Dim _libelleadr2 As String = ""

                '' evite les doublons 
                'Dim ld0 As New HoMIDom.HoMIDom.Driver.cLabels
                'For i As Integer = 0 To _LabelsDevice.Count - 1
                '    ld0 = _LabelsDevice(i)
                '    Select Case ld0.NomChamp
                '        Case "ADRESSE1"
                '            _libelleadr1 = Mid(_libelleadr1, 1, Len(_libelleadr1) - 1) 'enleve le dernier | pour eviter davoir une ligne vide a la fin
                '            ld0.Parametre = _libelleadr1
                '            _LabelsDevice(i) = ld0
                '        Case "ADRESSE2"
                '            _libelleadr2 = Mid(_libelleadr2, 1, Len(_libelleadr2) - 1) 'enleve le dernier | pour eviter davoir une ligne vide a la fin
                '            ld0.Parametre = _libelleadr2
                '            _LabelsDevice(i) = ld0
                '    End Select
                'Next

                Return True
            Catch ex As Exception
                WriteLog("ERR: " & "GET_Data, connexion, " & ex.Message)
                Return False
            End Try

        Catch ex As Exception
            WriteLog("ERR: " & "GET_Data, " & ex.Message)
            WriteLog("ERR: " & "GET_Data, Url: " & urldata)
            Return False
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
