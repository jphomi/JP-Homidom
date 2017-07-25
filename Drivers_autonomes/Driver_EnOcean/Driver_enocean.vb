Imports HoMIDom
Imports HoMIDom.HoMIDom
Imports HoMIDom.HoMIDom.Server
Imports HoMIDom.HoMIDom.Device

Imports System.Text.RegularExpressions
Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.IO
Imports System.IO.Ports
Imports System.Net
Imports System.Net.Sockets
Imports System.Collections.Concurrent
Imports System.Threading
Imports System.Threading.Tasks
Imports EnOcean

' Auteur : JPHomi
' Date : 01/08/2017

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
    Private WithEvents port As New SerialPort
    Private port_name As String = ""
    Dim _baudspeed As Integer = 57600
    Dim _nbrebit As Integer = 8
    Dim _parity As IO.Ports.Parity = IO.Ports.Parity.None
    Dim _nbrebitstop As IO.Ports.StopBits = IO.Ports.StopBits.One


    ' BASE_ID of the USB gateway
    ' ==========================
    '
    '   Private gwBaseId As Byte() = New Byte(3) {}
    Dim _app_description As String = ""
    Dim _app_version As String = ""
    Dim _api_version As String = ""
    Dim _chip_id As String = ""
    Dim _base_id As String = ""

    ' Serial port buffer and pointer
    ' ==============================
    '
    Private recBuffer As Byte() = New Byte(4095) {}
    Private ptrBuffer As Integer = 0

    ' CRC8 calculation
    ' ================
    '
    ' The polynomial G(x) = x^8 + x^2 + x^1 + x^0 is used to generate the CRC8 table, needed for the CRC8 calculation
    '
    Private Shared CRC8Table As Byte() = {&H0, &H7, &HE, &H9, &H1C, &H1B, _
        &H12, &H15, &H38, &H3F, &H36, &H31, _
        &H24, &H23, &H2A, &H2D, &H70, &H77, _
        &H7E, &H79, &H6C, &H6B, &H62, &H65, _
        &H48, &H4F, &H46, &H41, &H54, &H53, _
        &H5A, &H5D, &HE0, &HE7, &HEE, &HE9, _
        &HFC, &HFB, &HF2, &HF5, &HD8, &HDF, _
        &HD6, &HD1, &HC4, &HC3, &HCA, &HCD, _
        &H90, &H97, &H9E, &H99, &H8C, &H8B, _
        &H82, &H85, &HA8, &HAF, &HA6, &HA1, _
        &HB4, &HB3, &HBA, &HBD, &HC7, &HC0, _
        &HC9, &HCE, &HDB, &HDC, &HD5, &HD2, _
        &HFF, &HF8, &HF1, &HF6, &HE3, &HE4, _
        &HED, &HEA, &HB7, &HB0, &HB9, &HBE, _
        &HAB, &HAC, &HA5, &HA2, &H8F, &H88, _
        &H81, &H86, &H93, &H94, &H9D, &H9A, _
        &H27, &H20, &H29, &H2E, &H3B, &H3C, _
        &H35, &H32, &H1F, &H18, &H11, &H16, _
        &H3, &H4, &HD, &HA, &H57, &H50, _
        &H59, &H5E, &H4B, &H4C, &H45, &H42, _
        &H6F, &H68, &H61, &H66, &H73, &H74, _
        &H7D, &H7A, &H89, &H8E, &H87, &H80, _
        &H95, &H92, &H9B, &H9C, &HB1, &HB6, _
        &HBF, &HB8, &HAD, &HAA, &HA3, &HA4, _
        &HF9, &HFE, &HF7, &HF0, &HE5, &HE2, _
        &HEB, &HEC, &HC1, &HC6, &HCF, &HC8, _
        &HDD, &HDA, &HD3, &HD4, &H69, &H6E, _
        &H67, &H60, &H75, &H72, &H7B, &H7C, _
        &H51, &H56, &H5F, &H58, &H4D, &H4A, _
        &H43, &H44, &H19, &H1E, &H17, &H10, _
        &H5, &H2, &HB, &HC, &H21, &H26, _
        &H2F, &H28, &H3D, &H3A, &H33, &H34, _
        &H4E, &H49, &H40, &H47, &H52, &H55, _
        &H5C, &H5B, &H76, &H71, &H78, &H7F, _
        &H6A, &H6D, &H64, &H63, &H3E, &H39, _
        &H30, &H37, &H22, &H25, &H2C, &H2B, _
        &H6, &H1, &H8, &HF, &H1A, &H1D, _
        &H14, &H13, &HAE, &HA9, &HA0, &HA7, _
        &HB2, &HB5, &HBC, &HBB, &H96, &H91, _
        &H98, &H9F, &H8A, &H8D, &H84, &H83, _
        &HDE, &HD9, &HD0, &HD7, &HC2, &HC5, _
        &HCC, &HCB, &HE6, &HE1, &HE8, &HEF, _
        &HFA, &HFD, &HF4, &HF3}

    ' Return the CRC8 using the CRC8 table


    ' Note that some constants are stil not used

    Public Shared ReadOnly PACKET_TYPE As New Dictionary(Of String, String)() From { _
        {"00", "Reserved"}, _
        {"01", "Radio Telegram"}, _
        {"02", "Response to any packet"}, _
        {"03", "Radio Subtelegram"}, _
        {"04", "Event Message"}, _
        {"05", "Common Command"}, _
        {"06", "Smart Ack Command"}, _
        {"07", "Remote Management Command"}, _
        {"08", "Reserved for EnOcean"}, _
        {"09", "Radio Message"}, _
        {"0A", "ERP2 Protocol Radio Message"}, _
        {"10", "RADIO_802_15_4"}, _
        {"11", "COMMANF_2_4"} _
    }

    Public Shared ReadOnly RORG As New Dictionary(Of String, String)() From { _
        {"F6", "Repeated switch (RPS)"}, _
        {"D5", "Digital Data (1BS)"}, _
        {"A5", "Analog Data (4BS)"}, _
        {"D2", "Variable Length Data (VLD"} _
    }

    Public Shared ReadOnly RPS As New Dictionary(Of String, String)() From { _
        {"00", "Switch(es) Released"}, _
        {"10", "Switch A Pressed Down"}, _
        {"30", "Switch A Pressed Up"}, _
        {"50", "Switch B Pressed Down"}, _
        {"70", "Switch B Pressed Up"}, _
        {"15", "Switches A & B Pressed Down"}, _
        {"37", "Switches A & B Pressed Up"} _
    }

    Public Shared ReadOnly RET As New Dictionary(Of String, String)() From { _
        {"00", "RET_OK"}, _
        {"01", "RET_ERROR"}, _
        {"02", "RET_NOT_SUPPORTED"}, _
        {"03", "RET_WRONG_PARAM"}, _
        {"04", "RET_OPERATION_DENIED"}, _
        {"05", "RET_LOCK_SET"}, _
        {"06", "RET_BUFFER_TO_SMALL"}, _
        {"07", "RET_NO_FREE_BUFFER"} _
    }

    Public Shared ReadOnly CO As New Dictionary(Of String, String)() From { _
        {"03", "CO_RD_VERSION"}, _
        {"08", "CO_RD_IDBASE"} _
    }

    Public Shared ReadOnly MANUFACTURER As New Dictionary(Of String, String)() From { _
        {"000", "MANUFACTURER_RESERVED"}, _
        {"001", "PEHA"}, _
        {"002", "THERMOKON"}, _
        {"003", "SERVODAN"}, _
        {"004", "ECHOFLEX_SOLUTIONS"}, _
        {"005", "OMNIO_AG"}, _
        {"006", "HARDMEIER_ELECTRONICS"}, _
        {"007", "REGULVAR_INC"}, _
        {"008", "AD_HOC_ELECTRONICS"}, _
        {"009", "DISTECH_CONTROLS"}, _
        {"00A", "KIEBACK_AND_PETER"}, _
        {"00B", "ENOCEAN_GMBH"}, _
        {"00C", "PROBARE"}, _
        {"00D", "ELTAKO"}, _
        {"00E", "LEVITON"}, _
        {"00F", "HONEYWELL"}, _
        {"010", "SPARTAN_PERIPHERAL_DEVICES"}, _
        {"011", "SIEMENS"}, _
        {"012", "T_MAC"}, _
        {"013", "RELIABLE_CONTROLS_CORPORATION"}, _
        {"014", "ELSNER_ELEKTRONIK_GMBH"}, _
        {"015", "DIEHL_CONTROLS"}, _
        {"016", "BSC_COMPUTER"}, _
        {"017", "S_AND_S_REGELTECHNIK_GMBH"}, _
        {"018", "MASCO_CORPORATION"}, _
        {"019", "INTESIS_SOFTWARE_SL"}, _
        {"01A", "VIESSMANN"}, _
        {"01B", "LUTUO_TECHNOLOGY"}, _
        {"01C", "SCHNEIDER_ELECTRIC"}, _
        {"01D", "SAUTER"}, _
        {"01E", "BOOT_UP"}, _
        {"01F", "OSRAM_SYLVANIA"}, _
        {"020", "UNOTECH"}, _
        {"021", "DELTA_CONTROLS_INC"}, _
        {"022", "UNITRONIC_AG"}, _
        {"023", "NANOSENSE"}, _
        {"024", "THE_S4_GROUP"}, _
        {"025", "MSR_SOLUTIONS"}, _
        {"026", "GE"}, _
        {"027", "MAICO"}, _
        {"028", "RUSKIN_COMPANY"}, _
        {"029", "MAGNUM_ENERGY_SOLUTIONS"}, _
        {"02A", "KMC_CONTROLS"}, _
        {"02B", "ECOLOGIX_CONTROLS"}, _
        {"02C", "TRIO_2_SYS"}, _
        {"02D", "AFRISO_EURO_INDEX"}, _
        {"030", "NEC_ACCESSTECHNICA_LTD"}, _
        {"031", "ITEC_CORPORATION"}, _
        {"032", "SIMICX_CO_LTD"}, _
        {"034", "EUROTRONIC_TECHNOLOGY_GMBH"}, _
        {"035", "ART_JAPAN_CO_LTD"}, _
        {"036", "TIANSU_AUTOMATION_CONTROL_SYSTE_CO_LTD"}, _
        {"038", "GRUPPO_GIORDANO_IDEA_SPA"}, _
        {"039", "ALPHAEOS_AG"}, _
        {"03A", "TAG_TECHNOLOGIES"}, _
        {"03C", "CLOUD_BUILDINGS_LTD"}, _
        {"03E", "GIGA_CONCEPT"}, _
        {"03F", "SENSORTEC"}, _
        {"040", "JAEGER_DIREKT"}, _
        {"041", "AIR_SYSTEM_COMPONENTS_INC"}, _
        {"7FF", "MULTI_USER_MANUFACTURER"} _
    }

    'Def de la liste des Noeuds
    <NonSerialized()> Private Shared eo_nodeList As New List(Of EONode)


    ' Definition d'un noeud EnOcean 
    <Serializable()> Public Class EONode

        Dim eo_id As String = ""
        Dim eo_name As String = ""
        Dim eo_location As String = ""
        Dim eo_label As String = ""
        Dim eo_manufacturer As String = ""
        Dim eo_product As String = ""
        Dim eo_rorg As String = ""
        '        Dim m_commandClass As New List(Of CommandClass)
        '        Dim m_groups As New List(Of Byte)

        Public Property ID() As Byte
            Get
                Return eo_id
            End Get
            Set(ByVal value As Byte)
                eo_id = value
            End Set
        End Property


        Public Property Name() As String
            Get
                Return eo_name
            End Get
            Set(ByVal value As String)
                eo_name = value
            End Set
        End Property

        Public Property Location() As String
            Get
                Return eo_location
            End Get
            Set(ByVal value As String)
                eo_location = value
            End Set
        End Property

        Public Property Label() As String
            Get
                Return eo_label
            End Get
            Set(ByVal value As String)
                eo_label = value
            End Set
        End Property

        Public Property Manufacturer() As String
            Get
                Return eo_manufacturer
            End Get
            Set(ByVal value As String)
                eo_manufacturer = value
            End Set
        End Property

        Public Property Product() As String
            Get
                Return eo_product
            End Get
            Set(ByVal value As String)
                eo_product = value
            End Set
        End Property

        'Public Property Values() As List(Of ZWValueID)
        '    Get
        '        Return m_values
        '    End Get
        '    Set(value As List(Of ZWValueID))
        '        m_values = value
        '    End Set
        'End Property

        'Public Property CommandClass() As List(Of CommandClass)
        '    Get
        '        Return m_commandClass
        '    End Get
        '    Set(ByVal value As List(Of CommandClass))
        '        m_CommandClass = value
        '    End Set
        'End Property
        'Public Property Groups() As List(Of Byte)
        '    Get
        '        Return m_groups
        '    End Get
        '    Set(ByVal value As List(Of Byte))
        '        m_groups = value
        '    End Set
        'End Property

        'Shared Sub New()

        'End Sub

        'Sub AddValue(ByVal valueID As ZWValueID)
        '    m_values.Add(valueID)
        'End Sub

        'Sub RemoveValue(ByVal valueID As ZWValueID)
        '    m_values.Remove(valueID)
        'End Sub

        'Sub SetValue(ByVal valueID As ZWValueID)
        '    Dim valueIndex As Integer = 0
        '    Dim index As Integer = 0

        '    While index < m_values.Count
        '        If m_values(index).GetId() = valueID.GetId() Then
        '            valueIndex = index
        '            Exit While
        '        End If
        '        System.Math.Max(System.Threading.Interlocked.Increment(index), index - 1)
        '    End While

        '    If valueIndex >= 0 Then
        '        m_values(valueIndex) = valueID
        '    Else
        '        AddValue(valueID)
        '    End If
        'End Sub
    End Class

    Private Shared Function GetCRC8FromTable(CRC8 As Byte, data As Byte) As Byte
        Return CRC8Table((CRC8 Xor data) And &HFF)
    End Function
    ''' <summary>
    ''' Return the CRC8 for the header of the telegram (frame[1]...frame[4])
    ''' </summary>
    ''' <param name="frame"></param>
    ''' <returns></returns>
    Public Shared Function GetCRC8Header(frame As Byte()) As Byte
        Dim CRC8 As Byte = 0
        For i As Integer = 1 To 4
            CRC8 = GetCRC8FromTable(CRC8, frame(i))
        Next
        Return CRC8
    End Function
    ''' <summary>
    ''' Return the CRC8 for the data of the telegram (frame[6]...frame[frameLength-1])
    ''' </summary>
    ''' <param name="frame"></param>
    ''' <returns></returns>
    Public Shared Function GetCRC8Data(frame As Byte()) As Byte
        Dim CRC8 As Byte = 0
        For i As Integer = 6 To (frame.Length - 1) - 1
            CRC8 = GetCRC8FromTable(CRC8, frame(i))
        Next
        Return CRC8
    End Function
    Public Shared Function GetCRC8Data(frame As Byte(), length As Integer) As Byte
        Dim CRC8 As Byte = 0
        For i As Integer = 6 To length - 1
            CRC8 = GetCRC8FromTable(CRC8, frame(i))
        Next
        Return CRC8
    End Function
    ''' <summary>
    ''' Recalculate the C8Header and the CRC8Data in the frame (the frame must be passed by reference)
    ''' </summary>
    ''' <param name="frame"></param>
    Public Shared Sub SetAllCRC8(ByRef frame As Byte())
        frame(5) = GetCRC8Header(frame)
        frame(frame.Length - 1) = GetCRC8Data(frame)
    End Sub


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
                        Select UCase(Command)
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

                _IsConnect = True
                WriteLog("Driver " & Me.Nom & " démarré ")

                AddHandler port.DataReceived, AddressOf ReceiveFromSerial
                ' Ask the USB Gateway to learn Version and BASE_ID
                ' CO_RD_VERSION
                SendToSerial("55-00-01-00-05-70-03-00")
                ' CO_RD_IDBASE
                SendToSerial("55-00-01-00-05-70-08-00")


                ' QueryID
                '   SendToSerial("55-00-04-00-07-BE-00-26-7E-64-00")

                'HARDCODED_UTE_APPAIR
                SendToSerial("55-00-0D-07-01-FD-D4-D2-01-08-00-3E-FF-91-00-00-00-00-00-03-01-85-64-14-FF-00-E9")

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
                WriteLog("Driver " & Me.Nom & ", port fermé")

                _IsConnect = False
                WriteLog("Driver " & Me.Nom & " arrêté")
            Else
                _IsConnect = False
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
            Dim buf As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
            Select Case True
                Case (Objet.Type = "LAMPE" Or Objet.Type = "APPAREIL" Or Objet.Type = "SWITCH")
                    Select Case True
                        Case UCase(Commande) = "ON"
                            buf = {&H55, &H0, &H1, &H0, &H5, &H70, &H3, &H9}
                        Case UCase(Commande) = "OFF"
                            buf = {&H1, &H94, &HB9, &H46}
                    End Select
                    port.Write(buf, 0, 8)
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

    '-----------------------------------------------------------------------------
    ''' <summary>Ouvrir le port EnOcean</summary>
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
                        If port.IsOpen() Then
                            'port.Close()
                            Return ("Port " & port_name & " ouvert")
                        Else
                            ' Le port n'existe pas ==> le controleur n'est pas present
                            Return ("Port " & port_name & " fermé")
                        End If
                    Catch ex As Exception
                        Return ("Port " & port_name & " n'existe pas")
                        Exit Function
                    End Try
                Else
                    Return ("Port " & port_name & " dejà ouvert")
                End If
            Catch ex As Exception
                Return ("ERR: " & ex.Message)
            End Try

        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    '' This code apply to EnOcean Serial Protocol 3.0 (ESP3)                       ESP3 Packet:    ----Sync Byte -------------------------
    ''                                                                                             ----Header-----------------------------
    '' As soon as a Sync-Byte (value 0x55) is identified,  the subsequent                          -------Data Length --------------------
    '' 4 byte-Header  is  compared with  the  corresponding  CRC8H value.                          ------ Optional Data Length -----------
    '' If the result is a match  the Sync-Byte is correct.  Consequently,                          -------Packet Type --------------------
    '' the ESP3 packet is detected properly  and the subsequent data will                          -------CRC8 Header --------------------
    '' be passed.  If the header does not match the CRC8H, the value 0x55                          ----Data ------------------------------
    '' does not correspond to a Sync-Byte. The next 0x55 withing the data                          ----Optional Data ---------------------
    '' stream is picked and the verification is repeated.                                          ----CRC8 Data -------------------------

    Private Sub ReceiveFromSerial(sender As Object, e As SerialDataReceivedEventArgs)

        Try
            Dim sp As SerialPort = CType(sender, SerialPort)
            Dim FrameStr As String = ""


            While sp.BytesToRead > 0
                recBuffer(ptrBuffer) = CByte(sp.ReadByte())
                ' Read one byte
                If ptrBuffer = 0 AndAlso recBuffer(ptrBuffer) <> &H55 Then
                    Return
                End If
                ' Wait for the Sync Byte
                If ptrBuffer = 5 Then
                    ' Wait for the Header of the frame (6 bytes received)
                    If GetCRC8Header(recBuffer) <> recBuffer(5) Then
                        ' Bad CRC8H
                        ShiftRecBuffer()
                        ' Shift left the header trying to find a new Sync Byte
                        Return
                    End If
                End If
                If ptrBuffer >= 5 Then
                    ' CRC8H is Ok. Analyse the frame length
                    Dim dataLength As UInteger = recBuffer(1) * CUInt(256) + recBuffer(2)
                    Dim optionalDataLength As UInteger = recBuffer(3)
                    If ptrBuffer = (dataLength + optionalDataLength + 6) Then
                        ' Wait for receiving the full frame
                        If GetCRC8Data(recBuffer, ptrBuffer) <> recBuffer(ptrBuffer) Then
                            ' Bad CRC8D
                            ShiftRecBuffer()
                            ' Shift left the frame trying to find a new Sync Byte
                            Return
                        End If
                        Dim frame As Byte() = New Byte(ptrBuffer) {}
                        For i As Integer = 0 To (ptrBuffer + 1) - 1
                            frame(i) = recBuffer(i)
                            recBuffer(i) = 0
                        Next
                        FrameStr = BitConverter.ToString(frame)
                        ptrBuffer = 0
                    Else
                        ptrBuffer += 1
                    End If
                Else
                    ptrBuffer += 1
                End If

                Select Case True
                    Case FrameStr.StartsWith("55-00-01-00-02-65-00-00")
                        ' RET_OK (Command is understood and triggered)
                        WriteLog("DBG: ReceiveFromSerial, Command is understood and triggered, " & FrameStr)
                    Case FrameStr.StartsWith("55-00-21-00-02-26-00")
                        ' // Response to CO_RD_VERSION
                        If _app_description <> FrameStr.Substring(69, 47) Then
                            WriteLog("DBG: ReceiveFromSerial, Response to CO_RD_VERSION : " & FrameStr)
                            _app_description = FrameStr.Substring(69, 47)
                            _app_version = FrameStr.Substring(21, 11).Replace("-", ".")
                            _api_version = FrameStr.Substring(33, 11).Replace("-", ".")
                            WriteLog("EnOcean Gateway (USB300/USB310) :")
                            Dim str As String = FrameStr.Substring(69, 47)
                            Dim str2 As String = ""
                            For i As Integer = 0 To str.Length Step 3
                                If str.Substring(i, 2) <> "00" Then str2 = str2 + ChrW(Convert.ToInt32(str.Substring(i, 2), 16))
                            Next
                            WriteLog("Description " & str2)
                            WriteLog("Version " & _app_version)
                            WriteLog("Api version " & _api_version)
                            WriteLog("Chip_Id : " + FrameStr.Substring(45, 11).Replace("-", "."))
                        End If
                    Case FrameStr.StartsWith("55-00-05-01-02-DB-00")
                        ' Response to CO_RD_IDBASE (return the BASE ID of the USB Gateway)
                        '  WriteLog("DBG: ReceiveFromSerial, Response to CO_RD_IDBASE : " & FrameStr)
                        _base_id = FrameStr.Substring(21, 11)
                        WriteLog("Base_Id : " & _base_id)
                        'Dim len As Integer = str.Length / 3
                        'Dim buffer As Byte() = New Byte(len - 1) {}
                        'For i As Integer = 0 To len - 1
                        '    buffer(i) = Convert.ToByte(str(i))
                        'Next
                        'gwBaseId = buffer
                        'WriteLog("Base_Id : " + BitConverter.ToString(gwBaseId))
                    Case FrameStr.StartsWith("55-00-07-07-01-7A-F6")
                        ' Response to action ex bouton poussoir
                        WriteLog("DBG: ReceiveFromSerial, frame reponse : " & FrameStr)
                        TraiteFrame(FrameStr)
                    Case Else
                        If FrameStr <> "" Then
                            WriteLog("DBG: ReceiveFromSerial, frame recue : " & FrameStr)
                            Dim len As Integer = FrameStr.Length / 3
                            Dim telegram As Byte() = New Byte(len - 1) {}
                            For i As Integer = 0 To len - 1
                                telegram(i) = Convert.ToByte(FrameStr(i))
                            Next
                            If BitConverter.ToString(telegram) <> "" Then
                                WriteLog("DBG: Telegram : " + BitConverter.ToString(telegram))
                            End If
                        End If
                End Select
            End While




        Catch ex As Exception
            WriteLog("ERR: ReceiveFromSerial Exception: " + ex.Message)
        End Try
        System.Threading.Thread.Sleep(100)
        '       End While
        '        WriteLog("ERR: Bye from task ReceiveFromSerial")
    End Sub

    ''' <summary>
    ''' Shift left the frame trying to find a new Sync Byte
    ''' </summary>
    Private Sub TraiteFrame(frame As String)

        Try
            Dim strdic As String = ""
            Dim identdevice As String = ""
            identdevice = frame.Substring(24, 11)
            Dim pck_type As String = ""
            If PACKET_TYPE.TryGetValue(frame.Substring(12, 2), strdic) Then
                pck_type = strdic
                '                        WriteLog("DBG: ReceiveFromSerial, Packet type : " + pck_type)
                Select Case pck_type
                    Case Is = "Radio Telegram"
                        Dim rps_value As String = ""
                        If RPS.TryGetValue(frame.Substring(36, 2), strdic) Then
                            rps_value = strdic
                            '   WriteLog("DBG: ReceiveFromSerial, Packet type : " & rps_value)
                            Select Case rps_value
                                Case Is = "Switch(es) Released"
                                Case Is = "Switch A Pressed Down"
                                Case Is = "Switch A Pressed Up"
                                Case Is = "Switch B Pressed Down"
                                Case Is = "Switch B Pressed Up"
                                Case Is = "Switches A & B Pressed Down"
                                Case Is = "Switches A & B Pressed Up"
                            End Select
                            WriteLog("DBG: ReceiveFromSerial, device " & identdevice & " , " & rps_value)
                        End If
                End Select
            End If

        Catch ex As Exception
            WriteLog("ERR: TraiteFrame; Exception " + ex.Message)
        End Try
        System.Threading.Thread.Sleep(100)
        '       End While
        '        WriteLog("ERR: Bye from task ReceiveFromSerial")
    End Sub

    '' utilisé dans lecture de la trame
    Private Sub ShiftRecBuffer()
        While ptrBuffer <> 0
            For i As Integer = 0 To ptrBuffer - 1
                recBuffer(i) = recBuffer(i + 1)
            Next
            If recBuffer(0) = &H55 Then
                ptrBuffer = 0
            Else
                ptrBuffer -= 1
            End If
        End While
    End Sub
#End Region

#Region "Send to serial"
    ''' <summary>
    ''' Send ESP3 frame to serial port associated to the EnOcean USB gateway
    ''' 
    ''' The sender address must be in the range 00.00.00.00 and 00.00.00.7F because the EnOcean gateway can emulate 128 sender addresses from the Base_Id
    ''' For example, if the Base_Id is FF.56.2A.80 and the sender address is 00.0.00.46, the resultant address is FF.56.2A.C6
    ''' </summary>
    Private Sub SendToSerial(frame As String)
        Try
            Dim lenbuf As Integer = Len(frame) / 3
            If (lenbuf * 3) < frame.Length Then
                lenbuf += 1
            End If
            Dim buffer As Byte() = New Byte(lenbuf - 1) {}
            For i As Integer = 0 To lenbuf - 1
                buffer(i) = Convert.ToByte(frame.Substring(i * 3, 2), 16)
            Next
            SetAllCRC8(buffer)
            port.Write(buffer, 0, buffer.Length)
            WriteLog("DBG: SendToSerial, " & BitConverter.ToString(buffer) & " / buffer.Length = " & buffer.Length)
        Catch ex As Exception
            WriteLog("ERR: SendToSerial Exception: " + ex.Message)
        End Try
        System.Threading.Thread.Sleep(100)
    End Sub

    Function Get_Config(nomfileconfig As String) As Boolean
        ' recupere les configurations des equipements 

        Try
            'recherche des equipements
            If eo_nodeList.Count Then
                If Not My.Computer.FileSystem.FileExists(nomfileconfig) Then
                    Return False
                    WriteLog("ERR: " & "GET_Config, fichier" & nomfileconfig & " inexistant")
                    Exit Function
                End If

                '               _Adr1Txt.Clear()
                Dim _libelleadr1 As String = ""
                Dim _libelleadr2 As String = ""
                Dim response As String = ""

                For Each NodeTemp As EONode In eo_nodeList
                    _libelleadr1 += NodeTemp.ID & " # " & NodeTemp.Product & "|"
                    '               _Adr1Txt.Add(NodeTemp.ID & " # " & NodeTemp.Product)
                    WriteLog("DBG: _libelleadr1, " & _libelleadr1)

                    'recherche des parametres de l'equipement
                    response = ""
                    '                   response = LectureNoeudConfigXml(nomfileconfig, NodeTemp.ID)
                    If response <> "" Then
                        _libelleadr2 += response
                        WriteLog("DBG: _libelleadr2, " & response)
                    End If
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


            End If
            Return True
        Catch ex As Exception
            WriteLog("ERR: " & "GET_Config, " & ex.Message)
            Return False
        End Try
    End Function

    Function GetNode(ByVal id As String) As EONode
        Try
            Dim nodetmp As EONode

        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " WriteLog", ex.Message)
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

