// Vars
var
  DataDirPage: TInputDirWizardPage;

function InitializeSetup(): Boolean;
begin
	// Check dotnet
	 if not IsDotNetDetected('v4.5', 0) then begin
        MsgBox(ExpandConstant('{#AppName}') + ' requires Microsoft .NET Framework 4.5.'#13#13
            'Please use update to install this version,'#13
            'and then re-run the ' + ExpandConstant('{#AppName}') +' setup program.', mbInformation, MB_OK);
        Result := false;
    end else
        Result := true;
end;

procedure InitializeWizard();
begin

	

    // Create the page
    DataDirPage := CreateInputDirPage(wpSelectTasks, ExpandConstant('{cm:SelectFolder}'), ExpandConstant('{cm:AskFolder}'), 
        ExpandConstant('{cm:DescFolder1}') + #13#10#13#10 +  ExpandConstant('{cm:DescFolder2}'), False, ExpandConstant('{cm:NewFolder}'));

    // Add item (with an empty caption)
    DataDirPage.Add('');

    // Set initial value (optional)
    DataDirPage.Values[0] :=  GetPreviousData('DataDir', '');
    if DataDirPage.Values[0] = '' then
      DataDirPage.Values[0] := ExpandConstant('{sd}\{#AppName} {#VersionInfoTextVersion}');


end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
    if (PageID = DataDirPage.ID) and not IsTaskSelected('createfolders') then
        Result := True
    else
        Result := False
end;

procedure RegisterPreviousData(PreviousDataKey: Integer);
begin
    SetPreviousData(PreviousDataKey, 'DataDir', DataDirPage.Values[0]);
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  S: String;
begin
  S :=  ''
  if  IsTaskSelected('createfolders') then 
    S := S + NewLine + Space + DataDirPage.Values[0] + ' (data folders)' + NewLine;
  Result := MemoDirInfo + S;
end;

function GetDataDir(Param: String): String;
begin
  Result := DataDirPage.Values[0];
end;

