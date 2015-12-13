Imports HoMIDom
Imports HoMIDom.HoMIDom
Imports HoMIDom.HoMIDom.Server
Imports HoMIDom.HoMIDom.Device

Imports System.Text.RegularExpressions
Imports STRGS = Microsoft.VisualBasic.Strings

Imports EnOcean

Public Class Driver_ZWave

End Class
' Auteur : JPHomi
' Date : 28/12/2015

''' <summary>Class Driver_EnOcan, permet de communiquer avec le controleur EnOcean</summary>
''' <remarks></remarks>
''' 
<Serializable()> Public Class Driver_EnOcean
    Implements HoMIDom.HoMIDom.IDriver

#Region "Variables génériques"
    '!!!Attention les variables ci-dessous doivent avoir une valeur par défaut obligatoirement
    'aller sur l'adresse http://www.somacon.com/p113.php pour avoir un ID
    Dim _ID As String = "0541271C-6F2F-11E5-853F-74E11D5D46B0"
    Dim _Nom As String = "EnOcean"
    Dim _Enable As Boolean = False
    Dim _Description As String = "Controleur EnOcean"
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
    Private WithEvents port As New System.IO.Ports.SerialPort
    Private port_name As String = ""
    Dim MyRep As String = System.IO.Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

    ' Denition d'un noeud EnOcean 
    <Serializable()> Public Class Node

        Dim m_id As Byte = 0
        Dim m_homeId As UInt32 = 0
        Dim m_name As String = ""
        Dim m_location As String = ""
        Dim m_label As String = ""
        Dim m_manufacturer As String = ""
        Dim m_product As String = ""
        '        Dim m_values As New List(Of ZWValueID)
    End Class
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
        ' Permet de construire le message à afficher dans la console
    Dim texteCommande As String

    Try
        If MyDevice IsNot Nothing Then
            'Pas de commande demandée donc erreur
            If Command = "" Then
                Return False
            Else
                texteCommande = UCase(Command)

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
    Dim retour As String = ""


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
                retour = ouvrir(_Com)
            Else
                retour = "ERR: Port Com non défini. Impossible d'ouvrir le port !"
                _IsConnect = False
            End If
            WriteLog(retour)
        Catch ex As Exception
            WriteLog("ERR: Driver " & Me.Nom & " Erreur démarrage " & ex.Message)
            _IsConnect = False
    End Try
End Sub

''' <summary>Arrêter le driver</summary>
''' <remarks></remarks>
Public Sub [Stop]() Implements HoMIDom.HoMIDom.IDriver.Stop
    Dim retour As String
    Try
        If _IsConnect Then
            retour = fermer()
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

           If _Enable = False Then Exit Sub

        If _IsConnect = False Then
            WriteLog("ERR: READ, Le driver n'est pas démarré, impossible d'écrire sur le port")
            Exit Sub
        End If
        Try

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

        'Paramétres avancés
        Add_ParamAvance("Debug", "Activer le Debug complet (True/False)", False)
            '        Add_ParamAvance("AfficheLog", "Afficher Log OpenZwave à l'écran (True/False)", True)
            '        Add_ParamAvance("StartIdleTime", "Durée durant laquelle le driver ne traite aucun message lors de son démarrage (en secondes).", 10)

        'ajout des commandes avancées pour les devices
        Add_DeviceCommande("ALL_LIGHT_ON", "", 0)
        Add_DeviceCommande("ALL_LIGHT_OFF", "", 0)
        Add_DeviceCommande("SetName", "Nom du composant", 0)
        Add_DeviceCommande("GetName", "Nom du composant", 0)
        Add_DeviceCommande("SetConfigParam", "paramètre de configuration - Par1 : Index - Par2 : Valeur", 2)
        Add_DeviceCommande("GetConfigParam", "paramètre de configuration - Par1 : Index", 1)
        Add_DeviceCommande("RequestNodeState", "Nom du composant", 0)
        Add_DeviceCommande("TestNetworkNode", "Nom du composant", 0)
        Add_DeviceCommande("RequestNetworkUpdate", "Nom du composant", 0)
        Add_DeviceCommande("GetNumGroups", "Nom du composant", 0)

        'Libellé Driver
        Add_LibelleDriver("HELP", "Aide...", "Ce module permet de recuperer les informations delivrées par un contrôleur Z-Wave ")

        'Libellé Device
        Add_LibelleDevice("ADRESSE1", "Adresse", "Adresse du composant de Z-Wave")
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

'-----------------------------------------------------------------------------
    ''' <summary>Ouvrir le port EnOcean</summary>
''' <param name="numero">Nom/Numero du port COM: COM2</param>
''' <remarks></remarks>
Private Function ouvrir(ByVal numero As String) As String
        Try
            Try
                'ouverture du port
                If Not _IsConnect Then
                    ' Test d'ouveture du port Com du controleur 
                    port.PortName = numero
                    port.Open()
                    WriteLog("DBG: Message.MessageType.InformSoftwareVersion =>" & EnOcean.Message.MessageType.InformSoftwareVersion)
                    WriteLog("DBG: Message.MessageType.InformRadioSensitivity =>" & EnOcean.Message.MessageType.InformRadioSensitivity)
                    WriteLog("DBG: Message.MessageType.InformModemStatus =>" & EnOcean.Message.MessageType.InformModemStatus)
                    WriteLog("DBG: Message.MessageType.InformInit =>" & EnOcean.Message.MessageType.InformInit)
                    WriteLog("DBG: Message.MessageType.InformIdBase =>" & EnOcean.Message.MessageType.InformIdBase)
                    WriteLog("DBG: Org.Modem.ToString =>" & EnOcean.Org.ModemAcknowledge.ToString)
                    WriteLog(EnOcean.TelegramType.ReceiveMessageTelegram.ToString)
                    ' WriteLog("DBG: Message.MessageType.Ok =>" & EnOcean.Message.Ok.Parse.tostring)
                    '     WriteLog("DBG: Radio.Sensor.Manufacturer.EnOcean =>" & EnOcean.Radio.Sensor.FourByteSensorTeachTelegram)
                    '   WriteLog("DBG: Radio.Sensor.Manufacturer.AdHocElectronics =>" & EnOcean.Message.MessageTelegram)
                    Return ("Port " & port_name & " ouvert")
                Else
                    ' Le port n'existe pas ==> le controleur n'est pas present
                    Return ("Port " & port_name & " fermé")
                End If

            Catch ex As Exception
                _IsConnect = False
                Return ("Port " & port_name & " n'existe pas")
                Exit Function
            End Try

        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
End Function

    ''' <summary>Fermer le port EnOcean</summary>
''' <remarks></remarks>
Private Function fermer() As String
    Try
        If _IsConnect Then
            If (Not (port Is Nothing)) Then ' The COM port exists.
                    port.Close()
                Else
                    Return ("Port " & _Com & " n'existe pas")
            End If
        Else
            Return ("Port " & _Com & "  est déjà fermé (port_ouvert=false)")
        End If
        
    Catch ex As Exception
        Return ("ERR: Port " & _Com & " IGNORE: " & ex.Message)
    End Try
End Function


#End Region

End Class


