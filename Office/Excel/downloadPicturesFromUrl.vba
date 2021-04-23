Private Declare PtrSafe Function URLDownloadToFile Lib "urlmon" Alias _
   "URLDownloadToFileA" (ByVal pCaller As Long, _
   ByVal szURL As String, _
   ByVal szFileName As String, _
   ByVal dwReserved As Long, _
   ByVal lpfnCB As Long) As Long

Public Sub GURoL(url As String, FileName As String)
    Dim lngRetVal As Long
    lngRetVal = URLDownloadToFile(0, url, FileName, 0, 0)
    If lngRetVal <> 0 Then
    MsgBox "GURol godo: Can't download from " & url & " to " & FileName
    End If
End Sub

Function FileExists(FileName As String) As Boolean
     FileExists = (Dir(FileName) > "")
End Function

Sub DownloadFromURL()
    Dim localImagePath As String
    
    On Error Resume Next
    Application.ScreenUpdating = False
    Set ws = Sheets("topContributors")
    
    fromRow = 2
    toRow = 190
    ImageWidth = 100
    ImageHeight = 100
    
    Dim urlColumnName As String: urlColumnName = "E"
    Dim imageColumnName As String: imageColumnName = "F"
    
    Set url_column = ws.Range(urlColumnName & fromRow & ":" & urlColumnName & toRow)
    Set image_column = ws.Range(imageColumnName & fromRow & ":" & imageColumnName & toRow)
    
    'image_column.ColumnWidth = ImageWidth
    For i = 1 To url_column.Cells.Count
        localImagePath = "c:\Temp\xls-" & (fromRow + i - 1) & ".png"
        If (FileExists(localImagePath)) Then GoTo AddImage
        
        'Downloading the image
        Call GURoL(url_column.Cells(i).Value, localImagePath)

AddImage:
        'Adding image to excel
        Set Shape = ws.Shapes.AddPicture(localImagePath, msoFalse, msoTrue, 0, 0, 100, 100)
        With Shape
            .LockAspectRatio = msoTrue
            .Left = image_column.Cells(i).Left
            .Top = image_column.Cells(i).Top
            .Height = ImageHeight
            .Width = ImageWidth
            image_column.Cells(i).EntireRow.RowHeight = ImageHeight
            .Placement = xlMoveAndSize
        End With
NextRow:
    Next
    Application.ScreenUpdating = True
End Sub
