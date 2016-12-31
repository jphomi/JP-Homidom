Imports System.IO
Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Collections.ObjectModel
Imports System.Text

Public Class WLog
    Dim _IsClient As Boolean = False
    Dim _NbView As Integer = 16
    ' Used when manually scrolling.
    Private scrollTarget As Point
    Private scrollStartPoint As Point
    Private scrollStartOffset As Point

    Private Sub BtnRefresh_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles BtnRefresh.Click
        RefreshLog()
    End Sub

    Private Sub RefreshLog()
        Try
            'Variables
            Dim _LigneIgnorees As Integer = 0
            Me.Cursor = Cursors.Wait

            Dim ligneLog As New ObservableCollection(Of Dictionary(Of String, Object))
            ligneLog.Clear()
            Dim keys As New List(Of String)
            keys.Add("DateTime")
            keys.Add("TypeSource")
            keys.Add("Source")
            keys.Add("Fonction")
            keys.Add("Message")

            If Stk Is Nothing Then
                Exit Sub
            Else
                Stk.Children.Clear()
            End If

            If IsConnect = True Then
                Dim TargetFile As StreamWriter
                TargetFile = New StreamWriter("log.txt", False)
                If _IsClient Then
                    Label1.Content = "Log du Client"
                    If CbView.Text <> "Tous" Then
                        TargetFile.Write(ReturnLog(CbView.Text))
                    Else
                        TargetFile.Write(ReturnLog)
                    End If
                Else
                    Label1.Content = "Log du Serveur"
                    If CbView.Text <> "Tous" Then
                        TargetFile.Write(myService.ReturnLog(CbView.Text))
                    Else
                        TargetFile.Write(myService.ReturnLog)
                    End If
                End If
                TargetFile.Close()

                Dim tr As TextReader = New StreamReader("log.txt")

                'lecture de la premiere ligne souvent incomplete ou avec un message du serveur
                Dim line As String = tr.ReadLine()
                If line <> "" Then
                    Dim tmp As String() = line.Trim.Split(vbTab)
                    If tmp.Length > 3 Then
                        If tmp(3) = "ReturnLog" Then
                            MessageBox.Show(tmp(4), "Message du serveur", MessageBoxButton.OK, MessageBoxImage.Information)
                            line = tr.ReadLine()
                        End If
                    End If
                End If

                While tr.Peek() >= 0
                    Try
                        line = tr.ReadLine()

                        If line <> "" Then
                            Dim tmp As String() = line.Trim.Split(vbTab)

                            If tmp.Length < 6 And tmp.Length > 3 Then
                                If tmp(4).Length > 255 Then tmp(4) = Mid(tmp(4), 1, 255)
                                Dim sensorData As New Dictionary(Of String, Object) ' creates a dictionary where column name is the key and data is the value
                                For i As Integer = 0 To tmp.Length - 1
                                    sensorData.Add(keys(i), tmp(i))
                                Next
                                ligneLog.Insert(0, sensorData)
                                sensorData = Nothing
                            Else
                                'ligne au format incorrect 
                                Dim sensorData As New Dictionary(Of String, Object) ' creates a dictionary where column name is the key and data is the value
                                sensorData(keys(0)) = ""
                                sensorData(keys(1)) = ""
                                sensorData(keys(2)) = ""
                                sensorData(keys(3)) = ""
                                sensorData(keys(4)) = line.Trim.ToString
                                ligneLog.Insert(0, sensorData)
                                sensorData = Nothing
                            End If
                        End If
                        If ligneLog IsNot Nothing Then
                            Dim widLog As New UWidgetLog(ligneLog.First.Values(1), ligneLog.First.Values(2), ligneLog.First.Values(3), ligneLog.First.Values(4), ligneLog.First.Values(0))
                            Stk.Children.Add(widLog)
                        End If
                    Catch ex As Exception
                        MessageBox.Show("Erreur lors de la récupération du fichier log: " & ex.ToString, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error)
                    End Try
                End While
                tr.Close()
            End If

            ' If File.Exists("log.txt") Then File.Delete("log.txt")
            Me.Cursor = Nothing
        Catch ex As Exception
            MessageBox.Show("Erreur lors de la récupération du fichier log: " & ex.ToString, "ERREUR", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Public Sub New(Optional ByVal IsClient As Boolean = False)

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        _IsClient = IsClient
        RefreshLog()
    End Sub

    ''' <summary>renvoi le fichier log suivant une requête xml si besoin</summary>
    ''' <param name="Requete"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function ReturnLog(Optional ByVal Requete As String = "") As String
        Try
            Dim retour As String = ""
            If String.IsNullOrEmpty(Requete) = True Then
                If System.IO.File.Exists(_MonRepertoire & "\logs\log_" & DateAndTime.Now.ToString("yyyyMMdd") & ".txt") Then
                    Dim SR As New StreamReader(_MonRepertoire & "\logs\log_" & DateAndTime.Now.ToString("yyyyMMdd") & ".txt", Encoding.GetEncoding("ISO-8859-1"))
                    Do
                        If SR.EndOfStream Then Exit Do
                        Dim line As String = Trim(SR.ReadLine())
                        If line <> "" Then
                            If retour = "" Then
                                retour = retour + line
                            Else
                                retour = retour + vbCrLf + line
                            End If
                        End If
                    Loop
                    retour = HtmlDecode(retour)
                    SR.Close()
                    SR.Dispose()
                    SR = Nothing
                Else
                    retour = ""
                End If
            Else
                If IsNumeric(Requete) Then
                    Dim SR As New StreamReader(_MonRepertoire & "\logs\log_" & DateAndTime.Now.ToString("yyyyMMdd") & ".txt", Encoding.GetEncoding("ISO-8859-1"))
                    Dim i As Integer = 0
                    Dim nbtotal As Integer = 0
                    'cpte nbre total ligne dans le fichier
                    Do
                        If SR.EndOfStream Then Exit Do
                        Dim line As String = SR.ReadLine()
                        nbtotal += 1
                    Loop
                    SR.Close()
                    SR.Dispose()
                    SR = Nothing
                    SR = New StreamReader(_MonRepertoire & "\logs\log_" & DateAndTime.Now.ToString("yyyyMMdd") & ".txt", Encoding.GetEncoding("ISO-8859-1"))
                    Do
                        If SR.EndOfStream Then Exit Do
                        Dim line As String = Trim(SR.ReadLine())
                        If line <> "" Then
                            If i > nbtotal - Requete Then ' limite nbre de ligne demandé
                                If retour = "" Then
                                    retour = retour + line
                                Else
                                    retour = retour + vbCrLf + line
                                End If
                            End If
                        End If
                        i += 1
                    Loop
                    SR.Close()
                    SR.Dispose()
                    SR = Nothing
                    retour = HtmlDecode(retour)
                Else
                    'creation d'une nouvelle instance du membre xmldocument
                    Dim XmlDoc As XmlDocument = New XmlDocument()
                    XmlDoc.Load(_MonRepertoire & "\logs\log.xml")
                End If

            End If
            If retour.Length > 1000000 Then
                Dim retour2 As String = Mid(retour, retour.Length - 1000001, 1000000)
                retour = Now & vbTab & "ERREUR" & vbTab & "CLIENT" & vbTab & "ReturnLog" & vbTab & "Trop de lignes à traiter dans le log du jour, seules les dernières lignes seront affichées, merci de consulter le fichier sur le serveur par en avoir la totalité." & vbCrLf & vbCrLf & retour2
                Return retour
            End If
            Return retour
        Catch ex As Exception
            ReturnLog = "Erreur lors de la récupération du log: " & ex.ToString
        End Try
    End Function

    Private Sub BtnClose_Click(sender As Object, e As System.Windows.RoutedEventArgs) Handles BtnClose.Click
        DialogResult = True
    End Sub

#Region "Barre du bas"

    Private Sub ScrollViewer1_PreviewMouseDown(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs) Handles ScrollViewer1.PreviewMouseDown
        scrollStartPoint = e.GetPosition(Me)
        scrollStartOffset.Y = ScrollViewer1.VerticalOffset
    End Sub

    Private Sub ScrollViewer1_PreviewMouseMove(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs) Handles ScrollViewer1.PreviewMouseMove
        If e.LeftButton = MouseButtonState.Pressed Then
            Dim currentPoint As Point = e.GetPosition(Me)
            Dim delta As New Point(scrollStartPoint.X - currentPoint.X, scrollStartPoint.Y - currentPoint.Y)
            scrollTarget.Y = scrollStartOffset.Y + delta.Y
            ScrollToPosition(ScrollViewer1, currentPoint.X, scrollTarget.Y, m_SpeedTouch)
        End If
    End Sub
#End Region

    Private Sub CbView_SelectionChanged(sender As System.Object, e As System.Windows.Controls.SelectionChangedEventArgs) Handles CbView.SelectionChanged
        Select Case CbView.SelectedIndex
            Case 0
                _NbView = 0
            Case 1
                _NbView = 16
            Case 2
                _NbView = 32
            Case 3
                _NbView = 64
        End Select

        RefreshLog()
    End Sub
End Class
